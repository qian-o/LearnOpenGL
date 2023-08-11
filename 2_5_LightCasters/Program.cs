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
    private static IWindow window = null!;
    private static GL gl = null!;
    private static IInputContext inputContext = null!;
    private static ImGuiController controller = null!;
    private static Camera camera = null!;

    #region Input
    private static IMouse mouse = null!;
    private static IKeyboard keyboard = null!;
    private static bool firstMove = true;
    private static Vector2D<float> lastPos;
    #endregion

    #region Models
    private static Cube light = null!;
    private static Plane plane = null!;
    private static Cube[] cubes = null!;
    #endregion

    #region Colors
    private static Vector3D<float> lightColor = new(1.0f);
    #endregion

    #region Positions
    private static Vector3D<float> lightPos = new(1.2f, 4.0f, 2.0f);
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

        light = new Cube(gl);
        plane = new Plane(gl);
        cubes = new Cube[10];
        for (int i = 0; i < cubes.Length; i++)
        {
            cubes[i] = new Cube(gl);
        }

        cubePositions = new Vector3D<float>[]
        {
            new Vector3D<float>(0.0f, 0.0f, 0.0f),
            new Vector3D<float>(2.0f, 5.0f, -15.0f),
            new Vector3D<float>(-1.5f, -2.2f, -2.5f),
            new Vector3D<float>(-3.8f, -2.0f, -12.3f),
            new Vector3D<float>(2.4f, -0.4f, -3.5f),
            new Vector3D<float>(-1.7f, 3.0f, -7.5f),
            new Vector3D<float>(1.3f, -2.0f, -2.5f),
            new Vector3D<float>(1.5f, 2.0f, -2.5f),
            new Vector3D<float>(1.5f, 0.2f, -1.5f),
            new Vector3D<float>(-1.3f, 1.0f, -1.5f)
        };

        using Shader mvp = new(gl, GLEnum.VertexShader, File.ReadAllText("Shaders/mvp.vert"));
        using Shader solidColor = new(gl, GLEnum.FragmentShader, File.ReadAllText("Shaders/solidColor.frag"));
        using Shader lighting = new(gl, GLEnum.FragmentShader, File.ReadAllText("Shaders/lighting.frag"));

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

        light.Transform = Matrix4X4.CreateScale(0.5f) * Matrix4X4.CreateTranslation(lightPos);
        plane.Transform = Matrix4X4.CreateScale(new Vector3D<float>(1.0f, 1.0f, 2.3f)) * Matrix4X4.CreateScale(100.0f);
        plane.TextureScale = new Vector2D<float>(100.0f);
        for (int i = 0; i < 10; i++)
        {
            Matrix4X4<float> matrix = Matrix4X4<float>.Identity;
            matrix *= Matrix4X4.CreateTranslation(cubePositions[i]);
            matrix *= Matrix4X4.CreateTranslation(new Vector3D<float>(0.0f, 1.0f, 0.0f));
            matrix *= Matrix4X4.CreateRotationX(MathHelper.DegreesToRadians(20.0f * i), new Vector3D<float>(1.0f, 1.3f, 0.5f));

            cubes[i].Transform = matrix;
        }
    }

    private static void Window_Render(double obj)
    {
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        // 点光源
        {
            uint positionAttrib = (uint)solidColorProgram.GetAttrib("position");

            gl.EnableVertexAttribArray(positionAttrib);

            solidColorProgram.Enable();

            solidColorProgram.SetUniform("model", light.Transform);
            solidColorProgram.SetUniform("view", camera.View);
            solidColorProgram.SetUniform("projection", camera.Projection);

            solidColorProgram.SetUniform("color", lightColor);

            light.Draw(positionAttrib);

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

            Vector3D<float> diffuseColor = lightColor * new Vector3D<float>(0.5f, 0.5f, 0.5f);
            Vector3D<float> ambientColor = diffuseColor * new Vector3D<float>(0.2f, 0.2f, 0.2f);

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
            lightingProgram.SetUniform("light.position", camera.Position);
            lightingProgram.SetUniform("light.direction", camera.Front);
            lightingProgram.SetUniform("light.cutOff", MathHelper.Cos(MathHelper.DegreesToRadians(12.5f)));
            lightingProgram.SetUniform("light.outerCutOff", MathHelper.Cos(MathHelper.DegreesToRadians(17.5f)));
            lightingProgram.SetUniform("light.ambient", ambientColor);
            lightingProgram.SetUniform("light.diffuse", diffuseColor);
            lightingProgram.SetUniform("light.specular", new Vector3D<float>(1.0f));
            lightingProgram.SetUniform("light.constant", 1.0f);
            lightingProgram.SetUniform("light.linear", 0.007f);
            lightingProgram.SetUniform("light.quadratic", 0.0002f);

            // plane
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

            ImGui.Begin("Light Settings");

            Vector3 color = (Vector3)lightColor;
            ImGui.ColorEdit3("Light Color", ref color);
            lightColor.X = color.X;
            lightColor.Y = color.Y;
            lightColor.Z = color.Z;

            Vector3 position = (Vector3)lightPos;
            ImGui.DragFloat3("Light Position", ref position, 0.1f);
            lightPos.X = position.X;
            lightPos.Y = position.Y;
            lightPos.Z = position.Z;

            ImGui.Begin("Camera Settings");

            ImGui.DragFloat("Camera Speed", ref cameraSpeed, 0.5f, 0.5f, 20.0f);
            ImGui.DragFloat("Camera Sensitivity", ref cameraSensitivity, 0.2f, 0.2f, 10.0f);

            controller.Render();
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
