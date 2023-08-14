using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Core.Helpers;

public static class WindowHelper
{
    public static IWindow GetWindowByOpenGLES(APIVersion version, WindowOptions? options = null)
    {
        WindowOptions windowOptions = options ?? WindowOptions.Default;
        windowOptions.API = new GraphicsAPI(ContextAPI.OpenGLES, version);
        windowOptions.Samples = 8;
        windowOptions.PreferredDepthBufferBits = 32;
        windowOptions.PreferredStencilBufferBits = 32;
        windowOptions.PreferredBitDepth = new Vector4D<int>(8);

        return Window.Create(windowOptions);
    }
}
