using System.Reactive.Subjects;
using Avalonia.Platform.Storage;
using CSharpFunctionalExtensions;
using Zafiro.CSharpFunctionalExtensions;
using Zafiro.FileSystem.Core;
using Zafiro.FileSystem.Mutable;
using Path = Zafiro.DivineBytes.Path;

namespace Zafiro.Avalonia.Storage;

public class StorageDirectory : IMutableDirectory
{
    private readonly IStorageFolder folder;

    public StorageDirectory(IStorageFolder folder)
    {
        this.folder = folder;
    }

    public Path Path => folder.Path.ToString();

    public Task<Result<IEnumerable<IMutableNode>>> GetChildren(CancellationToken cancellationToken = default)
    {
        return Result.Try(() => folder.GetItemsAsync())
            .Map(async a =>
            {
                var storageItems = await a.ToListAsync(cancellationToken).ConfigureAwait(false);
                return storageItems.AsEnumerable();
            })
            .MapEach(ToMutableNode)
            .Map(Task.WhenAll)
            .Map(nodes => nodes.AsEnumerable());
    }

    public IObservable<IMutableFile> FileCreated { get; } = new Subject<IMutableFile>();
    public IObservable<IMutableDirectory> DirectoryCreated { get; } = new Subject<IMutableDirectory>();
    public IObservable<string> FileDeleted { get; } = new Subject<string>();
    public IObservable<string> DirectoryDeleted { get; } = new Subject<string>();

    public async Task<Result> DeleteFile(string name)
    {
        return await Result.Try(async () =>
        {
            var file = await folder.GetItemsAsync().OfType<IStorageFile>().FirstOrDefaultAsync(f => f.Name == name).ConfigureAwait(false);
            if (file is not null)
            {
                await file.DeleteAsync().ConfigureAwait(false);
            }
        });
    }

    public async Task<Result> DeleteSubdirectory(string name)
    {
        return await Result.Try(async () =>
        {
            var dir = await folder.GetItemsAsync().OfType<IStorageFolder>().FirstOrDefaultAsync(f => f.Name == name).ConfigureAwait(false);
            if (dir is not null)
            {
                await dir.DeleteAsync().ConfigureAwait(false);
            }
        });
    }

    public async Task<Result<IMutableFile>> CreateFile(string entryName)
    {
        return await Result.Try(async () =>
        {
            var file = await folder.CreateFileAsync(entryName).ConfigureAwait(false);
            return (IMutableFile)new MutableStorageFile(file!);
        });
    }

    public async Task<Result<IMutableDirectory>> CreateSubdirectory(string name)
    {
        return await Result.Try(async () =>
        {
            var subFolder = await folder.CreateFolderAsync(name).ConfigureAwait(false);
            return (IMutableDirectory)new StorageDirectory(subFolder!);
        });
    }

    public async Task<Result<bool>> HasFile(string name)
    {
        return await Result.Try(async () => await folder.GetItemsAsync().OfType<IStorageFile>().AnyAsync(f => f.Name == name).ConfigureAwait(false));
    }

    public async Task<Result<bool>> HasSubdirectory(string name)
    {
        return await Result.Try(async () => await folder.GetItemsAsync().OfType<IStorageFolder>().AnyAsync(f => f.Name == name).ConfigureAwait(false));
    }

    public string Name => folder.Name;
    public bool IsHidden => false;

    public Task<Result> Delete()
    {
        throw new NotImplementedException();
    }

    private async Task<IMutableNode> ToMutableNode(IStorageItem item)
    {
        return item switch
        {
            IStorageFile storageFile => new MutableStorageFile(storageFile),
            IStorageFolder storageFolder => new StorageDirectory(storageFolder),
            _ => throw new ArgumentOutOfRangeException(nameof(item))
        };
    }

    public Task<Result<IEnumerable<INode>>> Children() => GetChildren().Map(x => x.Cast<INode>());

    public Task<Result> Create()
    {
        throw new NotImplementedException();
    }
}