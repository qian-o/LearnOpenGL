using Core.Helpers;
using Silk.NET.Assimp;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using AssimpMesh = Silk.NET.Assimp.Mesh;
using CoreMesh = Core.Helpers.Mesh;
using CoreTexture = Core.Helpers.Texture;

namespace Core.Models;

public unsafe class Custom : BaseModel
{
    private readonly Assimp _assimp;
    private readonly string _directory;
    private readonly Dictionary<string, CoreTexture> _cache;

    public List<CoreMesh> Meshes { get; }

    public Custom(GL gl, string path) : base(gl)
    {
        _assimp = Assimp.GetApi();
        _directory = Path.GetDirectoryName(path)!;
        _cache = new Dictionary<string, CoreTexture>();
        Meshes = new List<CoreMesh>();

        Scene* scene = _assimp.ImportFile(path, (uint)(PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs));

        ProcessNode(scene->MRootNode, scene);
    }

    private void ProcessNode(Node* node, Scene* scene)
    {
        for (uint i = 0; i < node->MNumMeshes; i++)
        {
            AssimpMesh* mesh = scene->MMeshes[node->MMeshes[i]];

            Meshes.Add(ProcessMesh(mesh, scene));
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
        CoreTexture? diffuse = null;
        CoreTexture? specular = null;

        for (uint i = 0; i < mesh->MNumVertices; i++)
        {
            Vertex vertex = new()
            {
                Position = new Vector3D<float>(mesh->MVertices[i].X, mesh->MVertices[i].Y, mesh->MVertices[i].Z),
                Normal = new Vector3D<float>(mesh->MNormals[i].X, mesh->MNormals[i].Y, mesh->MNormals[i].Z)
            };

            if (mesh->MTextureCoords[0] != null)
            {
                vertex.TexCoords = new Vector2D<float>(mesh->MTextureCoords[0][i].X, mesh->MTextureCoords[0][i].Y);
            }

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

            foreach (CoreTexture texture in LoadMaterialTextures(material, TextureType.Diffuse))
            {
                diffuse = texture;
            }

            foreach (CoreTexture texture in LoadMaterialTextures(material, TextureType.Specular))
            {
                specular = texture;
            }
        }

        if (diffuse == null)
        {
            diffuse = new CoreTexture(_gl, GLEnum.Rgba, GLEnum.UnsignedByte);
            diffuse.WriteColor(new Vector3D<float>(1.0f));
        }

        if (specular == null)
        {
            specular = new CoreTexture(_gl, GLEnum.Rgba, GLEnum.UnsignedByte);
            specular.WriteColor(new Vector3D<float>(1.0f));
        }

        return new CoreMesh(_gl, vertices.ToArray(), indices.ToArray(), diffuse, specular);
    }

    private List<CoreTexture> LoadMaterialTextures(Material* mat, TextureType type)
    {
        List<CoreTexture> materialTextures = new();

        uint textureCount = _assimp.GetMaterialTextureCount(mat, type);
        for (uint i = 0; i < textureCount; i++)
        {
            AssimpString path;
            _assimp.GetMaterialTexture(mat, type, i, &path, null, null, null, null, null, null);

            if (!_cache.TryGetValue(path.ToString(), out CoreTexture? texture))
            {
                texture = new(_gl, GLEnum.Rgba, GLEnum.UnsignedByte);
                texture.WriteImage(Path.Combine(_directory, path.ToString()));

                _cache.Add(path.ToString(), texture);
            }

            materialTextures.Add(texture);
        }

        return materialTextures;
    }
}
