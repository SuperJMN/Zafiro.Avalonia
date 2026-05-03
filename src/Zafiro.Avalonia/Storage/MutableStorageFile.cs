using System.Reactive.Concurrency;
using Avalonia.Platform.Storage;
using CSharpFunctionalExtensions;
using Zafiro.DivineBytes;
using Zafiro.FileSystem.Mutable;
using Path = Zafiro.DivineBytes.Path;

namespace Zafiro.Avalonia.Storage;

public class MutableStorageFile : IMutableFile
{
    public MutableStorageFile(IStorageFile storageFile)
    {
        StorageFile = storageFile;
    }

    public IStorageFile StorageFile { get; }

    public Path Path => StorageFile.Path.ToString();

    public string Name => StorageFile.Name;

    public Task<Result<IByteSource>> GetContents() => GetData();

    public bool IsHidden { get; }

    public async Task<Result> SetContents(IByteSource data, IScheduler? scheduler = null, CancellationToken cancellationToken = default)
    {
        var stream = await StorageFile.OpenWriteAsync().ConfigureAwait(false);
        await using var stream1 = stream.ConfigureAwait(false);
        return await data.WriteTo(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private Task<Result<IByteSource>> GetData()
    {
        return Result.Try(async () =>
        {
            var length = await GetLength().ConfigureAwait(false);
            return ByteSource.FromAsyncStreamFactory(() => StorageFile.OpenReadAsync(), length);
        });
    }

    private async Task<Maybe<long>> GetLength()
    {
        var properties = await Result.Try(() => StorageFile.GetBasicPropertiesAsync()).ConfigureAwait(false);
        return properties
            .Map(x => ToLength(x.Size))
            .GetValueOrDefault(Maybe<long>.None);
    }

    private static Maybe<long> ToLength(ulong? size)
    {
        if (size is null || size.Value > long.MaxValue)
        {
            return Maybe<long>.None;
        }

        return Maybe.From((long)size.Value);
    }

    public Task<Result> Delete()
    {
        throw new NotImplementedException();
    }

    public Task<Result> Create()
    {
        throw new NotImplementedException();
    }
}
