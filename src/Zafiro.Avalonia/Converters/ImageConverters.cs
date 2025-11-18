using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Zafiro.Avalonia.Converters;

public class ImageConverters
{
    public static FuncValueConverter<byte[], IImage> ByteArrayToBitmapImage = new FuncValueConverter<byte[], IImage>(bytes => LoadBitmapFromBytes(bytes));
    
    public static Bitmap LoadBitmapFromBytes(byte[] imageData)
    {
        if (imageData == null || imageData.Length == 0)
            throw new ArgumentException("The byte array cannot be empty.", nameof(imageData));

        using var stream = new MemoryStream(imageData); // The stream only lives inside this block
        return new Bitmap(stream); // Bitmap copies the data and no longer needs the stream
    }
    
    public static readonly FuncValueConverter<Uri, Bitmap?> UriToBitmap = new(uri =>
    {
        if (uri == null)
        {
            return null;
        }

        return ImageExtensions.ImageHelper.LoadFromResource(uri);
    });
}