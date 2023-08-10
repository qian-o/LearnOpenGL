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
    private static Cube lightCube = null!;
    private static Plane plane = null!;
    private static Cube cube1 = null!;
    #endregion

    #region Colors
    private static Vector3D<float> lightColor = new(1.0f, 1.0f, 1.0f);
    #endregion

    #region Positions
    private static Vector3D<float> lightPos = new(1.2f, 6.0f, -6.0f);
    private static Vector3D<float> cube1Pos = new(0.0f, 0.5f, -1.5f);
    #endregion

    #region Programs
    // 光源
    private static ShaderProgram lightProgram = null!;

    // 场景内物体光照
    private static ShaderProgram lightingProgram = null!;
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
        camera = new Camera();
        camera.Position = new Vector3D<float>(0.0f, 2.0f, 3.0f);

        mouse = inputContext.Mice[0];
        keyboard = inputContext.Keyboards[0];

        lightCube = new Cube(gl);
        plane = new Plane(gl);
        cube1 = new Cube(gl);

        using Shader mvp = new(gl, GLEnum.VertexShader, File.ReadAllText("Shaders/mvp.vert"));
        using Shader light = new(gl, GLEnum.FragmentShader, File.ReadAllText("Shaders/light.frag"));
        using Shader lighting = new(gl, GLEnum.FragmentShader, File.ReadAllText("Shaders/lighting.frag"));

        lightProgram = new ShaderProgram(gl);
        lightProgram.Attach(mvp, light);

        lightingProgram = new ShaderProgram(gl);
        lightingProgram.Attach(mvp, lighting);

        gl.Enable(GLEnum.DepthTest);
        gl.ClearColor(Color.Black);
    }

    private static void Window_Update(double obj)
    {
        if (mouse.IsButtonPressed(MouseButton.Right))
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

                camera.Yaw += deltaX * 0.2f;
                camera.Pitch += -deltaY * 0.2f;

                lastPos = vector;
            }
        }
        else
        {
            firstMove = true;
        }

        if (keyboard.IsKeyPressed(Key.W))
        {
            camera.Position += camera.Front * 1.5f * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.A))
        {
            camera.Position -= camera.Right * 1.5f * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.S))
        {
            camera.Position -= camera.Front * 1.5f * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.D))
        {
            camera.Position += camera.Right * 1.5f * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.Q))
        {
            camera.Position -= camera.Up * 1.5f * (float)obj;
        }

        if (keyboard.IsKeyPressed(Key.E))
        {
            camera.Position += camera.Up * 1.5f * (float)obj;
        }

        camera.Width = window.Size.X;
        camera.Height = window.Size.Y;

        lightCube.Transform = Matrix4X4.CreateScale(0.2f) * Matrix4X4.CreateTranslation(lightPos);
        plane.Transform = Matrix4X4.CreateScale(10.0f);
        cube1.Transform = Matrix4X4.CreateTranslation(cube1Pos);
    }

    private static void Window_Render(double obj)
    {
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        // 光源
        {
            uint positionAttrib = (uint)lightProgram.GetAttrib("position");
            uint normalAttrib = (uint)lightProgram.GetAttrib("normal");

            gl.EnableVertexAttribArray(positionAttrib);
            gl.EnableVertexAttribArray(normalAttrib);

            lightProgram.Enable();

            lightProgram.SetUniform("model", lightCube.Transform);
            lightProgram.SetUniform("view", camera.View);
            lightProgram.SetUniform("projection", camera.Projection);

            lightProgram.SetUniform("color", lightColor);

            lightCube.Draw(positionAttrib, normalAttrib);

            lightProgram.Disable();

            gl.DisableVertexAttribArray(normalAttrib);
            gl.DisableVertexAttribArray(positionAttrib);
        }

        // 场景内物体
        {
            Vector3D<float> diffuseColor = lightColor * new Vector3D<float>(0.5f, 0.5f, 0.5f);
            Vector3D<float> ambientColor = diffuseColor * new Vector3D<float>(0.2f, 0.2f, 0.2f);

            uint positionAttrib = (uint)lightingProgram.GetAttrib("position");
            uint normalAttrib = (uint)lightingProgram.GetAttrib("normal");

            gl.EnableVertexAttribArray(positionAttrib);
            gl.EnableVertexAttribArray(normalAttrib);

            lightingProgram.Enable();

            lightingProgram.SetUniform("view", camera.View);
            lightingProgram.SetUniform("projection", camera.Projection);

            lightingProgram.SetUniform("lightPos", lightPos);
            lightingProgram.SetUniform("viewPos", camera.Position);
            lightingProgram.SetUniform("light.ambient", ambientColor);
            lightingProgram.SetUniform("light.diffuse", diffuseColor);
            lightingProgram.SetUniform("light.specular", new Vector3D<float>(1.0f, 1.0f, 1.0f));

            // plane
            {
                lightingProgram.SetUniform("model", plane.Transform);
                lightingProgram.SetUniform("material.ambient", MaterialsHelper.WhitePlastic.Ambient);
                lightingProgram.SetUniform("material.diffuse", MaterialsHelper.WhitePlastic.Diffuse);
                lightingProgram.SetUniform("material.specular", MaterialsHelper.WhitePlastic.Specular);
                lightingProgram.SetUniform("material.shininess", MaterialsHelper.WhitePlastic.Shininess);

                plane.Draw(positionAttrib, normalAttrib);
            }

            // cube1
            {
                lightingProgram.SetUniform("model", cube1.Transform);
                lightingProgram.SetUniform("material.ambient", MaterialsHelper.Emerald.Ambient);
                lightingProgram.SetUniform("material.diffuse", MaterialsHelper.Emerald.Diffuse);
                lightingProgram.SetUniform("material.specular", MaterialsHelper.Emerald.Specular);
                lightingProgram.SetUniform("material.shininess", MaterialsHelper.Emerald.Shininess);

                cube1.Draw(positionAttrib, normalAttrib);
            }

            lightingProgram.Disable();

            gl.DisableVertexAttribArray(normalAttrib);
            gl.DisableVertexAttribArray(positionAttrib);
        }

        // ImGui
        {
            controller!.Update((float)obj);

            ImGui.Begin("Color Settings");

            Vector3 vector = (Vector3)lightColor;
            ImGui.ColorEdit3("Light Color", ref vector);
            lightColor.X = vector.X;
            lightColor.Y = vector.Y;
            lightColor.Z = vector.Z;

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
        lightProgram.Dispose();

        controller.Dispose();
        inputContext.Dispose();

        gl.Dispose();
    }
}
