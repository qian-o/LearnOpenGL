using Silk.NET.OpenGLES;

namespace Core.Helpers;

public unsafe class Mesh
{
    public uint VAO { get; }

    public uint VBO { get; }

    public uint EBO { get; }

    public uint VertexCount { get; }

    public uint IndexCount { get; }

    public TextureInfo[] Textures { get; set; }

    public Mesh(GL gl, Vertex[] vertices, uint[] indices, TextureInfo[] textures)
    {
        VertexCount = (uint)vertices.Length * 8;
        IndexCount = (uint)indices.Length;
        Textures = textures;

        VAO = gl.GenVertexArray();
        VBO = gl.GenBuffer();
        EBO = gl.GenBuffer();

        gl.BindVertexArray(VAO);

        gl.BindBuffer(GLEnum.ArrayBuffer, VBO);
        gl.BufferData(GLEnum.ArrayBuffer, VertexCount * sizeof(float), GetVertices(vertices), GLEnum.StaticDraw);

        gl.BindBuffer(GLEnum.ElementArrayBuffer, EBO);
        gl.BufferData<uint>(GLEnum.ElementArrayBuffer, IndexCount * sizeof(uint), indices, GLEnum.StaticDraw);

        gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 8 * sizeof(float), (void*)0);
        gl.EnableVertexAttribArray(0);

        gl.VertexAttribPointer(1, 3, GLEnum.Float, false, 8 * sizeof(float), (void*)(3 * sizeof(float)));
        gl.EnableVertexAttribArray(1);

        gl.VertexAttribPointer(2, 2, GLEnum.Float, false, 8 * sizeof(float), (void*)(6 * sizeof(float)));
        gl.EnableVertexAttribArray(2);

        gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        gl.BindVertexArray(0);
    }

    public void Draw(GL gl)
    {
        gl.BindVertexArray(VAO);
        gl.DrawElements(GLEnum.Triangles, IndexCount, GLEnum.UnsignedInt, (void*)0);
        gl.BindVertexArray(0);
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
