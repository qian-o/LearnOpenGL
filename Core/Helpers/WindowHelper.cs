using Silk.NET.Windowing;

namespace Core.Helpers;

public static class WindowHelper
{
    public static IWindow GetWindowByOpenGLES(APIVersion version, WindowOptions? options = null)
    {
        WindowOptions windowOptions = options ?? WindowOptions.Default;
        windowOptions.API = new GraphicsAPI(ContextAPI.OpenGLES, version);
        windowOptions.Samples = 8;

        return Window.Create(windowOptions);
    }
}
