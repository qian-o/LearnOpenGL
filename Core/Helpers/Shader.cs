using Silk.NET.OpenGLES;

namespace Core.Helpers;

public class Shader : IDisposable
{
    private readonly GL _gl;

    public uint Id { get; }

    public Shader(GL gl, GLEnum type, string shaderSource, (string, string)[]? replaceDefines = null)
    {
        if (replaceDefines != null)
        {
            foreach ((string, string) define in replaceDefines)
            {
                shaderSource = shaderSource.Replace(define.Item1, define.Item2);
            }
        }

        _gl = gl;

        Id = _gl.CreateShader(type);
        _gl.ShaderSource(Id, shaderSource);
        _gl.CompileShader(Id);

        string error = _gl.GetShaderInfoLog(Id);

        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception($"{type}: {error}");
        }
    }

    public void Dispose()
    {
        _gl.DeleteShader(Id);

        GC.SuppressFinalize(this);
    }
}
