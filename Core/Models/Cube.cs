using Silk.NET.Maths;
using Silk.NET.OpenGLES;

namespace Core.Models;

public unsafe class Cube : BaseModel
{
    public Cube(GL gl) : base(gl)
    {
        VertexData = new[]
        {
            // Front face
            new Vector3D<float>(-0.5f, -0.5f, -0.5f),
            new Vector3D<float>(0.5f, -0.5f, -0.5f),
            new Vector3D<float>(0.5f,  0.5f, -0.5f),
            new Vector3D<float>(0.5f,  0.5f, -0.5f),
            new Vector3D<float>(-0.5f,  0.5f, -0.5f),
            new Vector3D<float>(-0.5f, -0.5f, -0.5f),

            // Back face
            new Vector3D<float>(-0.5f, -0.5f, 0.5f),
            new Vector3D<float>(0.5f, -0.5f, 0.5f),
            new Vector3D<float>(0.5f,  0.5f, 0.5f),
            new Vector3D<float>(0.5f,  0.5f, 0.5f),
            new Vector3D<float>(-0.5f,  0.5f, 0.5f),
            new Vector3D<float>(-0.5f, -0.5f, 0.5f),

            // Left face
            new Vector3D<float>(-0.5f,  0.5f,  0.5f),
            new Vector3D<float>(-0.5f,  0.5f, -0.5f),
            new Vector3D<float>(-0.5f, -0.5f, -0.5f),
            new Vector3D<float>(-0.5f, -0.5f, -0.5f),
            new Vector3D<float>(-0.5f, -0.5f,  0.5f),
            new Vector3D<float>(-0.5f,  0.5f,  0.5f),

            // Right face
            new Vector3D<float>(0.5f,  0.5f,  0.5f),
            new Vector3D<float>(0.5f,  0.5f, -0.5f),
            new Vector3D<float>(0.5f, -0.5f, -0.5f),
            new Vector3D<float>(0.5f, -0.5f, -0.5f),
            new Vector3D<float>(0.5f, -0.5f,  0.5f),
            new Vector3D<float>(0.5f,  0.5f,  0.5f),

            // Bottom face
            new Vector3D<float>(-0.5f, -0.5f, -0.5f),
            new Vector3D<float>(0.5f, -0.5f, -0.5f),
            new Vector3D<float>(0.5f, -0.5f,  0.5f),
            new Vector3D<float>(0.5f, -0.5f,  0.5f),
            new Vector3D<float>(-0.5f, -0.5f,  0.5f),
            new Vector3D<float>(-0.5f, -0.5f, -0.5f),

            // Top face
            new Vector3D<float>(-0.5f,  0.5f, -0.5f),
            new Vector3D<float>(0.5f,  0.5f, -0.5f),
            new Vector3D<float>(0.5f,  0.5f,  0.5f),
            new Vector3D<float>(0.5f,  0.5f,  0.5f),
            new Vector3D<float>(-0.5f,  0.5f,  0.5f),
            new Vector3D<float>(-0.5f,  0.5f, -0.5f)
        };

        VertexBuffer = _gl.GenBuffer();
        _gl.BindBuffer(GLEnum.ArrayBuffer, VertexBuffer);
        _gl.BufferData(GLEnum.ArrayBuffer, (uint)(VertexData.Length * 3 * sizeof(float)), VertexDataPointer, GLEnum.StaticDraw);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        NormalData = new[]
        {
            // Front face
            new Vector3D<float>(0.0f,  0.0f, -1.0f),
            new Vector3D<float>(0.0f,  0.0f, -1.0f),
            new Vector3D<float>(0.0f,  0.0f, -1.0f),
            new Vector3D<float>(0.0f,  0.0f, -1.0f),
            new Vector3D<float>(0.0f,  0.0f, -1.0f),
            new Vector3D<float>(0.0f,  0.0f, -1.0f),

            // Back face
            new Vector3D<float>(0.0f,  0.0f, 1.0f),
            new Vector3D<float>(0.0f,  0.0f, 1.0f),
            new Vector3D<float>(0.0f,  0.0f, 1.0f),
            new Vector3D<float>(0.0f,  0.0f, 1.0f),
            new Vector3D<float>(0.0f,  0.0f, 1.0f),
            new Vector3D<float>(0.0f,  0.0f, 1.0f),

            // Left face
            new Vector3D<float>(-1.0f,  0.0f,  0.0f),
            new Vector3D<float>(-1.0f,  0.0f,  0.0f),
            new Vector3D<float>(-1.0f,  0.0f,  0.0f),
            new Vector3D<float>(-1.0f,  0.0f,  0.0f),
            new Vector3D<float>(-1.0f,  0.0f,  0.0f),
            new Vector3D<float>(-1.0f,  0.0f,  0.0f),

            // Right face
            new Vector3D<float>(1.0f,  0.0f,  0.0f),
            new Vector3D<float>(1.0f,  0.0f,  0.0f),
            new Vector3D<float>(1.0f,  0.0f,  0.0f),
            new Vector3D<float>(1.0f,  0.0f,  0.0f),
            new Vector3D<float>(1.0f,  0.0f,  0.0f),
            new Vector3D<float>(1.0f,  0.0f,  0.0f),

            // Bottom face
            new Vector3D<float>(0.0f, -1.0f,  0.0f),
            new Vector3D<float>(0.0f, -1.0f,  0.0f),
            new Vector3D<float>(0.0f, -1.0f,  0.0f),
            new Vector3D<float>(0.0f, -1.0f,  0.0f),
            new Vector3D<float>(0.0f, -1.0f,  0.0f),
            new Vector3D<float>(0.0f, -1.0f,  0.0f),

            // Top face
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
            // Front face
            new Vector2D<float>(0.0f, 0.0f),
            new Vector2D<float>(1.0f, 0.0f),
            new Vector2D<float>(1.0f, 1.0f),
            new Vector2D<float>(1.0f, 1.0f),
            new Vector2D<float>(0.0f, 1.0f),
            new Vector2D<float>(0.0f, 0.0f),

            // Back face
            new Vector2D<float>(0.0f, 0.0f),
            new Vector2D<float>(1.0f, 0.0f),
            new Vector2D<float>(1.0f, 1.0f),
            new Vector2D<float>(1.0f, 1.0f),
            new Vector2D<float>(0.0f, 1.0f),
            new Vector2D<float>(0.0f, 0.0f),

            // Left face
            new Vector2D<float>(1.0f, 0.0f),
            new Vector2D<float>(1.0f, 1.0f),
            new Vector2D<float>(0.0f, 1.0f),
            new Vector2D<float>(0.0f, 1.0f),
            new Vector2D<float>(0.0f, 0.0f),
            new Vector2D<float>(1.0f, 0.0f),

            // Right face
            new Vector2D<float>(1.0f, 0.0f),
            new Vector2D<float>(1.0f, 1.0f),
            new Vector2D<float>(0.0f, 1.0f),
            new Vector2D<float>(0.0f, 1.0f),
            new Vector2D<float>(0.0f, 0.0f),
            new Vector2D<float>(1.0f, 0.0f),

            // Bottom face
            new Vector2D<float>(0.0f, 1.0f),
            new Vector2D<float>(1.0f, 1.0f),
            new Vector2D<float>(1.0f, 0.0f),
            new Vector2D<float>(1.0f, 0.0f),
            new Vector2D<float>(0.0f, 0.0f),
            new Vector2D<float>(0.0f, 1.0f),

            // Top face
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
