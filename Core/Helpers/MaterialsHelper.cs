using Silk.NET.Maths;

namespace Core.Helpers;

public class MaterialsHelper
{
    public static Materials Emerald { get; } = new(new Vector3D<float>(0.0215f, 0.1745f, 0.0215f),
                                                   new Vector3D<float>(0.07568f, 0.61424f, 0.07568f),
                                                   new Vector3D<float>(0.633f, 0.727811f, 0.633f),
                                                   0.6f);
    public static Materials Jade { get; } = new(new Vector3D<float>(0.135f, 0.2225f, 0.1575f),
                                                new Vector3D<float>(0.54f, 0.89f, 0.63f),
                                                new Vector3D<float>(0.316228f, 0.316228f, 0.316228f),
                                                0.1f);
    public static Materials Obsidian { get; } = new(new Vector3D<float>(0.05375f, 0.05f, 0.06625f),
                                                    new Vector3D<float>(0.18275f, 0.17f, 0.22525f),
                                                    new Vector3D<float>(0.332741f, 0.328634f, 0.346435f),
                                                    0.3f);
    public static Materials Pearl { get; } = new(new Vector3D<float>(0.25f, 0.20725f, 0.20725f),
                                                 new Vector3D<float>(1.0f, 0.829f, 0.829f),
                                                 new Vector3D<float>(0.296648f, 0.296648f, 0.296648f),
                                                 0.088f);
    public static Materials Ruby { get; } = new(new Vector3D<float>(0.1745f, 0.01175f, 0.01175f),
                                                new Vector3D<float>(0.61424f, 0.04136f, 0.04136f),
                                                new Vector3D<float>(0.727811f, 0.626959f, 0.626959f),
                                                0.6f);
    public static Materials Turquoise { get; } = new(new Vector3D<float>(0.1f, 0.18725f, 0.1745f),
                                                     new Vector3D<float>(0.396f, 0.74151f, 0.69102f),
                                                     new Vector3D<float>(0.297254f, 0.30829f, 0.306678f),
                                                     0.1f);
    public static Materials Brass { get; } = new(new Vector3D<float>(0.329412f, 0.223529f, 0.027451f),
                                                 new Vector3D<float>(0.780392f, 0.568627f, 0.113725f),
                                                 new Vector3D<float>(0.992157f, 0.941176f, 0.807843f),
                                                 0.21794872f);
    public static Materials WhitePlastic { get; } = new(new Vector3D<float>(0.0f, 0.0f, 0.0f),
                                                        new Vector3D<float>(0.55f, 0.55f, 0.55f),
                                                        new Vector3D<float>(0.70f, 0.70f, 0.70f),
                                                        0.25f);
}
