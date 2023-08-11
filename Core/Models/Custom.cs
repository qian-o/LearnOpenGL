using Core.Helpers;
using Silk.NET.Assimp;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using AssimpMesh = Silk.NET.Assimp.Mesh;
using CoreMesh = Core.Helpers.Mesh;

namespace Core.Models;

public unsafe class Custom : BaseModel
{
    private readonly Assimp _assimp;
    private readonly string _directory;
    private readonly List<CoreMesh> _meshes;

    public Custom(GL gl, string path) : base(gl)
    {
        _assimp = Assimp.GetApi();
        _directory = Path.GetDirectoryName(path)!;
        _meshes = new List<CoreMesh>();

        Scene* scene = _assimp.ImportFile(path, (uint)(PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs));

        ProcessNode(scene->MRootNode, scene);
    }

    private void ProcessNode(Node* node, Scene* scene)
    {
        for (uint i = 0; i < node->MNumMeshes; i++)
        {
            AssimpMesh* mesh = scene->MMeshes[node->MMeshes[i]];

            _meshes.Add(ProcessMesh(mesh, scene));
        }

        for (uint i = 0; i < node->MNumChildren; i++)
        {
            ProcessNode(node->MChildren[i], scene);
        }
    }

    private CoreMesh ProcessMesh(AssimpMesh* mesh, Scene* scene)
    {
        List<Vertex> vertices = new();
        List<uint> indices = new();
        List<TextureInfo> textures = new();

        for (uint i = 0; i < mesh->MNumVertices; i++)
        {
            Vertex vertex = new()
            {
                Position = new Vector3D<float>(mesh->MVertices[i].X, mesh->MVertices[i].Y, mesh->MVertices[i].Z),
                Normal = new Vector3D<float>(mesh->MNormals[i].X, mesh->MNormals[i].Y, mesh->MNormals[i].Z),
                TexCoords = new Vector2D<float>(mesh->MTextureCoords[0]->X, mesh->MTextureCoords[0]->Y)
            };

            vertices.Add(vertex);
        }

        for (uint i = 0; i < mesh->MNumFaces; i++)
        {
            Face face = mesh->MFaces[i];

            for (uint j = 0; j < face.MNumIndices; j++)
            {
                indices.Add(face.MIndices[j]);
            }
        }

        if (mesh->MMaterialIndex >= 0)
        {
            Material* material = scene->MMaterials[mesh->MMaterialIndex];

            LoadMaterialTextures(material, TextureType.Diffuse);
        }

        return new CoreMesh(_gl, vertices.ToArray(), indices.ToArray(), textures.ToArray());
    }

    private void LoadMaterialTextures(Material* mat, TextureType type)
    {
        uint textureCount = _assimp.GetMaterialTextureCount(mat, type);
        for (uint i = 0; i < textureCount; i++)
        {
            AssimpString path;
            _assimp.GetMaterialTexture(mat, type, i, &path, null, null, null, null, null, null);
        }
    }
}
