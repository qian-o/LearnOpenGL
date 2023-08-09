using Silk.NET.Maths;
using Silk.NET.OpenGLES;

namespace Core.Helpers;

public unsafe class ShaderProgram : IDisposable
{
    private readonly GL _gl;
    private readonly Dictionary<string, int> _attribLocations;
    private readonly Dictionary<string, int> _uniformLocations;

    public uint Id { get; }

    public Shader Vs { get; private set; } = null!;

    public Shader Fs { get; private set; } = null!;

    public ShaderProgram(GL gl)
    {
        _gl = gl;
        _attribLocations = new Dictionary<string, int>();
        _uniformLocations = new Dictionary<string, int>();

        Id = _gl.CreateProgram();
    }

    public void Attach(Shader vs, Shader fs)
    {
        if (vs != null && Vs != vs)
        {
            if (Vs != null)
            {
                _gl.DetachShader(Id, Vs.Id);
            }

            _gl.AttachShader(Id, vs.Id);

            Vs = vs;
        }

        if (fs != null && Fs != vs)
        {
            if (Fs != null)
            {
                _gl.DetachShader(Id, Fs.Id);
            }

            _gl.AttachShader(Id, fs.Id);

            Fs = fs;
        }

        _gl.LinkProgram(Id);

        string error = _gl.GetProgramInfoLog(Id);

        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception($"Program:{Id}, Error:{error}");
        }
    }

    public void Enable()
    {
        _gl.UseProgram(Id);
    }

    public void Disable()
    {
        _gl.UseProgram(0);
    }

    public int GetAttrib(string name)
    {
        if (!_attribLocations.TryGetValue(name, out int value))
        {
            value = _gl.GetAttribLocation(Id, name);

            _attribLocations[name] = value;
        }

        return value;
    }

    public int GetUniform(string name)
    {
        if (!_uniformLocations.TryGetValue(name, out int value))
        {
            value = _gl.GetUniformLocation(Id, name);

            _uniformLocations[name] = value;
        }

        return value;
    }

    public void SetUniform(string name, int data)
    {
        Enable();

        _gl.Uniform1(GetUniform(name), data);
    }

    public void SetUniform(string name, float data)
    {
        Enable();

        _gl.Uniform1(GetUniform(name), data);
    }

    public void SetUniform(string name, Vector2D<float> data)
    {
        Enable();

        _gl.Uniform2(GetUniform(name), 1, (float*)&data);
    }

    public void SetUniform(string name, Vector3D<float> data)
    {
        Enable();

        _gl.Uniform3(GetUniform(name), 1, (float*)&data);
    }

    public void SetUniform(string name, Vector4D<float> data)
    {
        Enable();

        _gl.Uniform4(GetUniform(name), 1, (float*)&data);
    }

    public void SetUniform(string name, Matrix2X2<float> data)
    {
        Enable();

        _gl.UniformMatrix2(GetUniform(name), 1, false, (float*)&data);
    }

    public void SetUniform(string name, Matrix3X3<float> data)
    {
        Enable();

        _gl.UniformMatrix3(GetUniform(name), 1, false, (float*)&data);
    }

    public void SetUniform(string name, Matrix4X4<float> data)
    {
        Enable();

        _gl.UniformMatrix4(GetUniform(name), 1, false, (float*)&data);
    }

    public void Dispose()
    {
        _gl.DeleteProgram(Id);
        _attribLocations.Clear();
        _uniformLocations.Clear();

        GC.SuppressFinalize(this);
    }
}
