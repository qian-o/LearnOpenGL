using Silk.NET.Maths;

namespace Core.Helpers;

public struct Materials
{
    public Vector3D<float> Ambient;

    public Vector3D<float> Diffuse;

    public Vector3D<float> Specular;

    public float Shininess;

    public Materials(Vector3D<float> ambient, Vector3D<float> diffuse, Vector3D<float> specular, float shininess)
    {
        Ambient = ambient;
        Diffuse = diffuse;
        Specular = specular;
        Shininess = shininess;
    }
}
