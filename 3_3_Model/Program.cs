using Core.Helpers;
using Core.Models;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.OpenGLES.Extensions.ImGui;
using Silk.NET.Windowing;
using System.Drawing;
using System.Numerics;
using Plane = Core.Models.Plane;
using Shader = Core.Helpers.Shader;
using Texture = Core.Helpers.Texture;

namespace Examples;

internal class Program
{
    private static readonly List<double> fpsSample = new();

    private static IWindow window = null!;
    private static GL gl = null!;
    private static IInputContext inputContext = null!;
    private static ImGuiController controller = null!;
    private static Camera camera = null!;
    private static int fps;

    #region Input
    private static IMouse mouse = null!;
    private static IKeyboard keyboard = null!;
    private static bool firstMove = true;
    private static Vector2D<float> lastPos;
    #endregion

    #region Models
    private static Cube dirLight = null!;
    private static Cube[] pointLights = null!;
    private static Plane plane = null!;
    private static Cube[] cubes = null!;
    #endregion

    #region Colors
    private static Vector3D<float> dirLightColor = new(1.0f);
    private static Vector3D<float>[] pointLightColors = null!;
    #endregion

    #region Positions
    private static Vector3D<float> dirLightPos = new(1.2f, 10.0f, 2.0f);
    private static Vector3D<float> dirLightDirection = new(-0.2f, -1.0f, -0.3f);
    private static Vector3D<float>[] pointLightPositions = null!;
    private static Vector3D<float>[] cubePositions = null!;
    #endregion

    #region Speeds
    private static float cameraSpeed = 4.0f;
    private static float cameraSensitivity = 0.2f;
    #endregion

    #region Programs
    // 纯色
    private static ShaderProgram solidColorProgram = null!;
    // 场景内光照
    private static ShaderProgram lightingProgram = null!;
    #endregion

    #region Textures
    private static Texture planeDiffuseMap = null!;
    private static Texture planeSpecularMap = null!;
    private static Texture cubeDiffuseMap = null!;
    private static Texture cubeSpecularMap = null!;
    #endregion

    static void Main(string[] args)
    {
        _ = args;

        window = WindowHelper.GetWindowByOpenGLES(new APIVersion(3, 2));

        window.Load += Window_Load;
        window.Update += Window_Update;
        window.Render += Window_Render;
        window.FramebufferResize += Window_FramebufferResize;
        window.Closing += Window_Closing;

        window.Run();
    }

    private static void Window_Load()
    {
        controller = new ImGuiController(gl = window.CreateOpenGLES(), window, inputContext = window.CreateInput());
        camera = new Camera
        {
            Position = new Vector3D<float>(0.0f, 2.0f, 3.0f),
            Fov = 45.0f
        };

        mouse = inputContext.Mice[0];
        keyboard = inputContext.Keyboards[0];

        dirLight = new Cube(gl);
        pointLights = new Cube[10];
        plane = new Plane(gl);
        cubes = new Cube[10];

        for (int i = 0; i < pointLights.Length; i++)
        {
            pointLights[i] = new Cube(gl);
        }

        for (int i = 0; i < cubes.Length; i++)
        {
            cubes[i] = new Cube(gl);
        }

        pointLightColors = new Vector3D<float>[]
        {
            new Vector3D<float>(1.0f),
            new Vector3D<float>(1.0f),
            new Vector3D<float>(1.0f),
            new Vector3D<float>(1.0f),
            new Vector3D<float>(1.0f),
            new Vector3D<float>(1.0f),
            new Vector3D<float>(1.0f),
            new Vector3D<float>(1.0f),
            new Vector3D<float>(1.0f),
            new Vector3D<float>(1.0f)
        };

        pointLightPositions = new Vector3D<float>[]
        {
            new Vector3D<float>(-1.0f, 4.0f, -1.0f),
            new Vector3D<float>(1.0f, 4.0f, 1.0f),
            new Vector3D<float>(3.0f, 4.0f, 3.0f),
            new Vector3D<float>(5.0f, 4.0f, 5.0f),
            new Vector3D<float>(7.0f, 4.0f, 7.0f),
            new Vector3D<float>(9.0f, 4.0f, 9.0f),
            new Vector3D<float>(11.0f, 4.0f, 11.0f),
            new Vector3D<float>(13.0f, 4.0f, 13.0f),
            new Vector3D<float>(15.0f, 4.0f, 15.0f),
            new Vector3D<float>(17.0f, 4.0f, 17.0f)
        };

        cubePositions = new Vector3D<float>[]
        {
            new Vector3D<float>(-1.0f, 0.5001f, -1.0f),
            new Vector3D<float>(1.0f, 0.5001f, 1.0f),
            new Vector3D<float>(3.0f, 0.5001f, 3.0f),
            new Vector3D<float>(5.0f, 0.5001f, 5.0f),
            new Vector3D<float>(7.0f, 0.5001f, 7.0f),
            new Vector3D<float>(9.0f, 0.5001f, 9.0f),
            new Vector3D<float>(11.0f, 0.5001f, 11.0f),
            new Vector3D<float>(13.0f, 0.5001f, 13.0f),
            new Vector3D<float>(15.0f, 0.5001f, 15.0f),
            new Vector3D<float>(17.0f, 0.5001f, 17.0f)
        };

        using Shader mvp = new(gl, GLEnum.VertexShader, File.ReadAllText("Shaders/mvp.vert"));
        using Shader solidColor = new(gl, GLEnum.FragmentShader, File.ReadAllText("Shaders/solidColor.frag"));
        using Shader lighting = new(gl, GLEnum.FragmentShader, File.ReadAllText("Shaders/lighting.frag"), new (string, string)[] { ("#define NR_POINT_LIGHTS 1", $"#define NR_POINT_LIGHTS {pointLights.Length}") });

        solidColorProgram = new ShaderProgram(gl);
        solidColorProgram.Attach(mvp, solidColor);

        lightingProgram = new ShaderProgram(gl);
        lightingProgram.Attach(mvp, lighting);

        planeDiffuseMap = new Texture(gl, GLEnum.Rgba, GLEnum.UnsignedByte);
        planeDiffuseMap.WriteImage("wood_floor.jpg");

        planeSpecularMap = new Texture(gl, GLEnum.Rgba, GLEnum.UnsignedByte);
        planeSpecularMap.WriteColor(new Vector3D<float>(0.7843137f));

        cubeDiffuseMap = new Texture(gl, GLEnum.Rgba, GLEnum.UnsignedByte);
        cubeDiffuseMap.WriteImage("container2.png");

        cubeSpecularMap = new Texture(gl, GLEnum.Rgba, GLEnum.UnsignedByte);
        cubeSpecularMap.WriteImage("container2_specular.png");

        gl.Enable(GLEnum.DepthTest);
        gl.ClearColor(Color.Black);

        Custom custom = new(gl, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nanosuit/nanosuit.obj"));
    }

    private static void Window_Update(double obj)
    {
        if (mouse.IsButtonPressed(MouseButton.Middle))
        {
            Vector2D<float> vector = new(mouse.Position.X, mouse.Position.Y);

            if (firstMove)
            {
                lastPos = vector;

                firstMove = false;
            }
            else
            {
                float deltaX = vector.X - lastPos.X;
                float deltaY = vector.Y - lastPos.Y;

                camera.Yaw += deltaX * cameraSensitivity;
                camera.Pitch += -deltaY * cameraSensitivity;

                lastPos = vector;
            }
        }
        else
        {
            firstMove = true;
        }

        if (keyboard.IsKeyPressed(Key.W))
        {
            camera.Position += camera.Front * cameraSpeed * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.A))
        {
            camera.Position -= camera.Right * cameraSpeed * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.S))
        {
            camera.Position -= camera.Front * cameraSpeed * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.D))
        {
            camera.Position += camera.Right * cameraSpeed * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.Q))
        {
            camera.Position -= camera.Up * cameraSpeed * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.E))
        {
            camera.Position += camera.Up * cameraSpeed * (float)obj;
        }

        camera.Width = window.Size.X;
        camera.Height = window.Size.Y;

        dirLight.Transform = Matrix4X4.CreateScale(0.5f) * Matrix4X4.CreateTranslation(dirLightPos);
        plane.Transform = Matrix4X4.CreateScale(new Vector3D<float>(1.0f, 1.0f, 2.3f)) * Matrix4X4.CreateScale(100.0f);
        plane.TextureScale = new Vector2D<float>(50.0f);

        for (int i = 0; i < pointLights.Length; i++)
        {
            pointLights[i].Transform = Matrix4X4.CreateScale(0.5f) * Matrix4X4.CreateTranslation(pointLightPositions[i]);
        }

        for (int i = 0; i < cubes.Length; i++)
        {
            cubes[i].Transform = Matrix4X4.CreateTranslation(cubePositions[i]);
        }
    }

    private static void Window_Render(double obj)
    {
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        // 定向光
        {
            uint positionAttrib = (uint)solidColorProgram.GetAttrib("position");

            gl.EnableVertexAttribArray(positionAttrib);

            solidColorProgram.Enable();

            solidColorProgram.SetUniform("model", dirLight.Transform);
            solidColorProgram.SetUniform("view", camera.View);
            solidColorProgram.SetUniform("projection", camera.Projection);

            solidColorProgram.SetUniform("color", dirLightColor);

            dirLight.Draw(positionAttrib);

            solidColorProgram.Disable();

            gl.DisableVertexAttribArray(positionAttrib);
        }

        // 点光源
        {
            uint positionAttrib = (uint)solidColorProgram.GetAttrib("position");

            gl.EnableVertexAttribArray(positionAttrib);

            solidColorProgram.Enable();

            for (int i = 0; i < pointLights.Length; i++)
            {
                solidColorProgram.SetUniform("model", pointLights[i].Transform);
                solidColorProgram.SetUniform("view", camera.View);
                solidColorProgram.SetUniform("projection", camera.Projection);

                solidColorProgram.SetUniform("color", pointLightColors[i]);

                pointLights[i].Draw(positionAttrib);
            }

            solidColorProgram.Disable();

            gl.DisableVertexAttribArray(positionAttrib);
        }

        // 场景内物体
        {
            gl.ActiveTexture(TextureUnit.Texture0);
            planeDiffuseMap.Enable();
            gl.ActiveTexture(TextureUnit.Texture1);
            planeSpecularMap.Enable();
            gl.ActiveTexture(TextureUnit.Texture2);
            cubeDiffuseMap.Enable();
            gl.ActiveTexture(TextureUnit.Texture3);
            cubeSpecularMap.Enable();

            uint positionAttrib = (uint)lightingProgram.GetAttrib("position");
            uint normalAttrib = (uint)lightingProgram.GetAttrib("normal");
            uint texCoordsAttrib = (uint)lightingProgram.GetAttrib("texCoords");

            gl.EnableVertexAttribArray(positionAttrib);
            gl.EnableVertexAttribArray(normalAttrib);
            gl.EnableVertexAttribArray(texCoordsAttrib);

            lightingProgram.Enable();

            lightingProgram.SetUniform("view", camera.View);
            lightingProgram.SetUniform("projection", camera.Projection);

            lightingProgram.SetUniform("viewPos", camera.Position);

            // 定向光 Uniform
            {
                Vector3D<float> diffuseColor = dirLightColor * new Vector3D<float>(0.5f, 0.5f, 0.5f);
                Vector3D<float> ambientColor = diffuseColor * new Vector3D<float>(0.2f, 0.2f, 0.2f);

                lightingProgram.SetUniform("dirLight.direction", dirLightDirection);

                lightingProgram.SetUniform("dirLight.ambient", ambientColor);
                lightingProgram.SetUniform("dirLight.diffuse", diffuseColor);
                lightingProgram.SetUniform("dirLight.specular", dirLightColor);
            }

            // 点光源 Uniform
            {
                for (int i = 0; i < pointLights.Length; i++)
                {
                    Vector3D<float> light = pointLightColors[i];

                    Vector3D<float> diffuseColor = light * new Vector3D<float>(0.5f, 0.5f, 0.5f);
                    Vector3D<float> ambientColor = diffuseColor * new Vector3D<float>(0.2f, 0.2f, 0.2f);

                    lightingProgram.SetUniform("pointLights[" + i + "].position", pointLightPositions[i]);

                    lightingProgram.SetUniform("pointLights[" + i + "].constant", 1.0f);
                    lightingProgram.SetUniform("pointLights[" + i + "].linear", 0.09f);
                    lightingProgram.SetUniform("pointLights[" + i + "].quadratic", 0.032f);

                    lightingProgram.SetUniform("pointLights[" + i + "].ambient", ambientColor);
                    lightingProgram.SetUniform("pointLights[" + i + "].diffuse", diffuseColor);
                    lightingProgram.SetUniform("pointLights[" + i + "].specular", light);
                }
            }

            // 聚光灯 Uniform
            {
                lightingProgram.SetUniform("spotLight.position", camera.Position);
                lightingProgram.SetUniform("spotLight.direction", camera.Front);
                lightingProgram.SetUniform("spotLight.cutOff", MathHelper.Cos(MathHelper.DegreesToRadians(12.5f)));
                lightingProgram.SetUniform("spotLight.outerCutOff", MathHelper.Cos(MathHelper.DegreesToRadians(17.5f)));

                lightingProgram.SetUniform("spotLight.constant", 1.0f);
                lightingProgram.SetUniform("spotLight.linear", 0.09f);
                lightingProgram.SetUniform("spotLight.quadratic", 0.032f);

                lightingProgram.SetUniform("spotLight.ambient", new Vector3D<float>(0.0f));
                lightingProgram.SetUniform("spotLight.diffuse", new Vector3D<float>(1.0f));
                lightingProgram.SetUniform("spotLight.specular", new Vector3D<float>(1.0f));
            }

            // 地板
            {
                lightingProgram.SetUniform("model", plane.Transform);
                lightingProgram.SetUniform("material.diffuse", 0);
                lightingProgram.SetUniform("material.specular", 1);
                lightingProgram.SetUniform("material.shininess", 64.0f);

                plane.Draw(positionAttrib, normalAttrib, texCoordsAttrib);
            }

            foreach (Cube cube in cubes)
            {
                lightingProgram.SetUniform("model", cube.Transform);
                lightingProgram.SetUniform("material.diffuse", 2);
                lightingProgram.SetUniform("material.specular", 3);
                lightingProgram.SetUniform("material.shininess", 64.0f);

                cube.Draw(positionAttrib, normalAttrib, texCoordsAttrib);
            }

            lightingProgram.Disable();

            gl.DisableVertexAttribArray(normalAttrib);
            gl.DisableVertexAttribArray(positionAttrib);
            gl.DisableVertexAttribArray(texCoordsAttrib);

            planeDiffuseMap.Disable();
            planeSpecularMap.Disable();
            cubeDiffuseMap.Disable();
            cubeSpecularMap.Disable();
        }

        // ImGui
        {
            controller!.Update((float)obj);

            ImGui.GetBackgroundDrawList().AddText(new Vector2(0, 0), ImGui.GetColorU32(new Vector4(0.0f, 1.0f, 0.0f, 1.0f)), fps.ToString());

            ImGui.Begin("Light Settings");

            Vector3 color = (Vector3)dirLightColor;
            ImGui.ColorEdit3("Dir Light Color", ref color);
            dirLightColor.X = color.X;
            dirLightColor.Y = color.Y;
            dirLightColor.Z = color.Z;

            Vector3 position = (Vector3)dirLightDirection;
            ImGui.DragFloat3("Dir Light Direction", ref position, 0.1f);
            dirLightDirection.X = position.X;
            dirLightDirection.Y = position.Y;
            dirLightDirection.Z = position.Z;

            for (int i = 0; i < pointLights.Length; i++)
            {
                ImGui.Text("Point Light " + i);

                Vector3 pointColor = (Vector3)pointLightColors[i];
                ImGui.ColorEdit3("Point Light Color " + i, ref pointColor);
                pointLightColors[i].X = pointColor.X;
                pointLightColors[i].Y = pointColor.Y;
                pointLightColors[i].Z = pointColor.Z;

                Vector3 pointPosition = (Vector3)pointLightPositions[i];
                ImGui.DragFloat3("Point Light Position " + i, ref pointPosition, 0.1f);
                pointLightPositions[i].X = pointPosition.X;
                pointLightPositions[i].Y = pointPosition.Y;
                pointLightPositions[i].Z = pointPosition.Z;
            }

            ImGui.Begin("Camera Settings");

            ImGui.DragFloat("Camera Speed", ref cameraSpeed, 0.5f, 0.5f, 20.0f);
            ImGui.DragFloat("Camera Sensitivity", ref cameraSensitivity, 0.2f, 0.2f, 10.0f);

            controller.Render();
        }

        // Fps
        {
            if (fpsSample.Count == 100)
            {
                fps = Convert.ToInt32(fpsSample.Average());

                fpsSample.Clear();
            }

            fpsSample.Add(1.0d / obj);
        }
    }

    private static void Window_FramebufferResize(Vector2D<int> obj)
    {
        gl.Viewport(obj);
    }

    private static void Window_Closing()
    {
        lightingProgram.Dispose();

        controller.Dispose();
        inputContext.Dispose();

        gl.Dispose();
    }
}
