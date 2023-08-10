using Silk.NET.Maths;
using SkiaSharp;

namespace Core.Helpers;

public unsafe static class TextureExtensions
{
    public static void WriteImage(this Texture texture, string file)
    {
        SKImage image = SKImage.FromEncodedData(file);

        texture.AllocationBuffer(new Vector2D<uint>((uint)image.Width, (uint)image.Height), out nint pboData);

        image.ReadPixels(new SKImageInfo((int)texture.Size.X, (int)texture.Size.Y, SKColorType.Rgba8888), pboData, (int)texture.Size.X * 4, 0, 0);

        texture.FlushTexture();
    }

    public static void WriteColor(this Texture texture, Vector3D<float> color)
    {
        texture.AllocationBuffer(new Vector2D<uint>(1, 1), out nint pboData);

        Span<Vector4D<byte>> span = new((void*)pboData, 1);

        span[0] = new Vector4D<byte>((byte)(color.X * 255), (byte)(color.Y * 255), (byte)(color.Z * 255), 255);

        texture.FlushTexture();
    }

    public static void WriteColor(this Texture texture, Vector4D<float> color)
    {
        texture.AllocationBuffer(new Vector2D<uint>(1, 1), out nint pboData);

        Span<Vector4D<byte>> span = new((void*)pboData, 1);

        span[0] = new Vector4D<byte>((byte)(color.X * 255), (byte)(color.Y * 255), (byte)(color.Z * 255), (byte)(color.W * 255));

        texture.FlushTexture();
    }
}
