using Silk.NET.OpenGLES;

namespace Core.Helpers;

public unsafe class Mesh
{
    private readonly GL _gl;

    public uint VBO { get; }

    public uint EBO { get; }

    public uint VertexCount { get; }

    public uint IndexCount { get; }

    public Texture Diffuse { get; }

    public Texture Specular { get; }

    public Mesh(GL gl, Vertex[] vertices, uint[] indices, Texture diffuse, Texture specular)
    {
        _gl = gl;

        VertexCount = (uint)vertices.Length;
        IndexCount = (uint)indices.Length;
        Diffuse = diffuse;
        Specular = specular;

        VBO = gl.GenBuffer();
        EBO = gl.GenBuffer();

        gl.BindBuffer(GLEnum.ArrayBuffer, VBO);
        gl.BufferData<Vertex>(GLEnum.ArrayBuffer, VertexCount * (uint)sizeof(Vertex), vertices, GLEnum.StaticDraw);
        gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        gl.BindBuffer(GLEnum.ElementArrayBuffer, EBO);
        gl.BufferData<uint>(GLEnum.ElementArrayBuffer, IndexCount * sizeof(uint), indices, GLEnum.StaticDraw);
        gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
    }

    public void Draw(uint positionAttrib, uint normalAttrib, uint texCoordsAttrib)
    {
        _gl.BindBuffer(GLEnum.ArrayBuffer, VBO);
        _gl.VertexAttribPointer(positionAttrib, 3, GLEnum.Float, false, 8 * sizeof(float), (void*)0);
        _gl.VertexAttribPointer(normalAttrib, 3, GLEnum.Float, false, 8 * sizeof(float), (void*)(3 * sizeof(float)));
        _gl.VertexAttribPointer(texCoordsAttrib, 2, GLEnum.Float, false, 8 * sizeof(float), (void*)(6 * sizeof(float)));
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        _gl.BindBuffer(GLEnum.ElementArrayBuffer, EBO);
        _gl.DrawElements(GLEnum.Triangles, VertexCount, GLEnum.UnsignedInt, (void*)0);
        _gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
    }

    private float* GetVertices(Vertex[] vertices)
    {
        float[] data = new float[vertices.Length * 8];

        for (int i = 0; i < vertices.Length; i++)
        {
            data[i * 8] = vertices[i].Position.X;
            data[i * 8 + 1] = vertices[i].Position.Y;
            data[i * 8 + 2] = vertices[i].Position.Z;

            data[i * 8 + 3] = vertices[i].Normal.X;
            data[i * 8 + 4] = vertices[i].Normal.Y;
            data[i * 8 + 5] = vertices[i].Normal.Z;

            data[i * 8 + 6] = vertices[i].TexCoords.X;
            data[i * 8 + 7] = vertices[i].TexCoords.Y;
        }

        fixed (float* ptr = data)
        {
            return ptr;
        }
    }
}
