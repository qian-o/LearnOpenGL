using Silk.NET.Maths;
using Silk.NET.OpenGLES;

namespace Core.Models;

public unsafe abstract class BaseModel
{
    protected readonly GL _gl;

    private Vector2D<float> textureScale;

    public Vector3D<float>[] VertexData { get; protected set; } = Array.Empty<Vector3D<float>>();

    public uint VertexBuffer { get; protected set; }

    public Vector3D<float>[] NormalData { get; protected set; } = Array.Empty<Vector3D<float>>();

    public uint NormalBuffer { get; protected set; }

    public Vector2D<float>[] TextureData { get; protected set; } = Array.Empty<Vector2D<float>>();

    public uint TextureBuffer { get; protected set; }

    public Vector2D<float> TextureScale
    {
        get => textureScale;
        set
        {
            if (textureScale != value)
            {
                textureScale = value;

                ScaleTextureData();
            }
        }
    }

    public GLEnum Mode { get; protected set; } = GLEnum.Triangles;

    public Matrix4X4<float> Transform { get; set; } = Matrix4X4<float>.Identity;

    public float* VertexDataPointer
    {
        get
        {
            fixed (Vector3D<float>* vertexDataPointer = VertexData)
            {
                return (float*)vertexDataPointer;
            }
        }
    }

    public float* NormalDataPointer
    {
        get
        {
            fixed (Vector3D<float>* normalDataPointer = NormalData)
            {
                return (float*)normalDataPointer;
            }
        }
    }

    public float* TextureDataPointer
    {
        get
        {
            fixed (Vector2D<float>* textureDataPointer = TextureData)
            {
                return (float*)textureDataPointer;
            }
        }
    }

    protected BaseModel(GL gl)
    {
        _gl = gl;
    }

    public void Draw(uint positionAttrib, uint? normalAttrib = null, uint? texCoordsAttrib = null)
    {
        _gl.BindBuffer(GLEnum.ArrayBuffer, VertexBuffer);
        _gl.VertexAttribPointer(positionAttrib, 3, GLEnum.Float, false, 0, null);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        if (normalAttrib != null)
        {
            _gl.BindBuffer(GLEnum.ArrayBuffer, NormalBuffer);
            _gl.VertexAttribPointer(normalAttrib.Value, 3, GLEnum.Float, false, 0, null);
            _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        }

        if (texCoordsAttrib != null)
        {
            _gl.BindBuffer(GLEnum.ArrayBuffer, TextureBuffer);
            _gl.VertexAttribPointer(texCoordsAttrib.Value, 2, GLEnum.Float, false, 0, null);
            _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        }

        _gl.DrawArrays(Mode, 0, (uint)VertexData.Length);
    }

    private void ScaleTextureData()
    {
        for (int i = 0; i < TextureData.Length; i++)
        {
            TextureData[i].X *= TextureScale.X;
            TextureData[i].Y *= TextureScale.Y;
        }

        _gl.BindBuffer(GLEnum.ArrayBuffer, TextureBuffer);
        _gl.BufferData(GLEnum.ArrayBuffer, (uint)(TextureData.Length * 2 * sizeof(float)), TextureDataPointer, GLEnum.StaticDraw);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
    }
}
