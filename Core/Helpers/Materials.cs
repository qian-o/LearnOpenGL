using Silk.NET.Maths;

namespace Core.Helpers;

public struct Materials
{
    public Vector3D<float> Ambient { get; set; }

    public Vector3D<float> Diffuse { get; set; }

    public Vector3D<float> Specular { get; set; }

    public float Shininess { get; set; }
}
