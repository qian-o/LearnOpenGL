using Silk.NET.Maths;
using Silk.NET.OpenGLES;

namespace Core.Models;

public unsafe class Plane : BaseModel
{
    public Plane(GL gl) : base(gl)
    {
        VertexData = new[]
        {
            new Vector3D<float>(-0.5f, 0.0f, -0.5f),
            new Vector3D<float>(0.5f,  0.0f, -0.5f),
            new Vector3D<float>(0.5f,  0.0f,  0.5f),
            new Vector3D<float>(0.5f,  0.0f,  0.5f),
            new Vector3D<float>(-0.5f, 0.0f,  0.5f),
            new Vector3D<float>(-0.5f, 0.0f, -0.5f)
        };

        VertexBuffer = _gl.GenBuffer();
        _gl.BindBuffer(GLEnum.ArrayBuffer, VertexBuffer);
        _gl.BufferData(GLEnum.ArrayBuffer, (uint)(VertexData.Length * 3 * sizeof(float)), VertexDataPointer, GLEnum.StaticDraw);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        NormalData = new[]
        {
            new Vector3D<float>(0.0f,  1.0f,  0.0f),
            new Vector3D<float>(0.0f,  1.0f,  0.0f),
            new Vector3D<float>(0.0f,  1.0f,  0.0f),
            new Vector3D<float>(0.0f,  1.0f,  0.0f),
            new Vector3D<float>(0.0f,  1.0f,  0.0f),
            new Vector3D<float>(0.0f,  1.0f,  0.0f)
        };

        NormalBuffer = _gl.GenBuffer();
        _gl.BindBuffer(GLEnum.ArrayBuffer, NormalBuffer);
        _gl.BufferData(GLEnum.ArrayBuffer, (uint)(NormalData.Length * 3 * sizeof(float)), NormalDataPointer, GLEnum.StaticDraw);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        TextureData = new[]
        {
            new Vector2D<float>(0.0f, 1.0f),
            new Vector2D<float>(1.0f, 1.0f),
            new Vector2D<float>(1.0f, 0.0f),
            new Vector2D<float>(1.0f, 0.0f),
            new Vector2D<float>(0.0f, 0.0f),
            new Vector2D<float>(0.0f, 1.0f)
        };

        TextureBuffer = _gl.GenBuffer();
        _gl.BindBuffer(GLEnum.ArrayBuffer, TextureBuffer);
        _gl.BufferData(GLEnum.ArrayBuffer, (uint)(TextureData.Length * 2 * sizeof(float)), TextureDataPointer, GLEnum.StaticDraw);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        Mode = GLEnum.Triangles;
    }
}
