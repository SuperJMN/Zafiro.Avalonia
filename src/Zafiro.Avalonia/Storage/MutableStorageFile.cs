using System.Reactive.Concurrency;
using System.Runtime.InteropServices;
using Avalonia.Platform.Storage;
using CSharpFunctionalExtensions;
using Zafiro.DivineBytes;
using Zafiro.FileSystem.Mutable;
using Zafiro.Reactive;
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


    public Task<Result<IByteSource>> GetContents()
    {
        if (RuntimeInformation.ProcessArchitecture == Architecture.Wasm)
        {
            return GetDataWasm();
        }

        return GetDataNoWasm();
    }

    public bool IsHidden { get; }

    public async Task<Result> SetContents(IByteSource data, IScheduler? scheduler = null, CancellationToken cancellationToken = new CancellationToken())
    {
        var stream = await StorageFile.OpenWriteAsync().ConfigureAwait(false);
        await using var stream1 = stream.ConfigureAwait(false);
        return await data.WriteTo(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private Task<Result<IByteSource>> GetDataWasm()
    {
        return Result.Try(async () =>
        {
            var readAsync = await StorageFile.OpenReadAsync().ConfigureAwait(false);
            var bytes = await readAsync.ReadBytesToEnd().ConfigureAwait(false);
            return ByteSource.FromBytes(bytes);
        });
    }

    private async Task<Result<IByteSource>> GetDataNoWasm()
    {
        return Result.Success(ByteSource.FromAsyncStreamFactory(() => StorageFile.OpenReadAsync()));
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