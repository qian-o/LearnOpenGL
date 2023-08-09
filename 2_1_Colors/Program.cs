using Core.Helpers;
using Core.Models;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.Windowing;
using System.Drawing;
using Shader = Core.Helpers.Shader;

namespace Examples;

internal class Program
{
    private static IWindow window = null!;
    private static GL gl = null!;
    private static Camera camera = null!;

    #region Input
    private static IMouse mouse = null!;
    private static IKeyboard keyboard = null!;
    private static bool firstMove = true;
    private static Vector2D<float> lastPos;
    #endregion

    #region Models
    private static Cube lightCube = null!;
    private static Cube lightingCube = null!;
    #endregion

    #region Positions
    private static Vector3D<float> lightPos = new(1.2f, -1.2f, -2.0f);
    private static Vector3D<float> lightingPos = new(0.0f, 0.0f, -1.0f);
    #endregion

    #region Programs
    // 光源
    private static ShaderProgram lightProgram = null!;

    // 进行光照
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

        window.Run();
    }

    private static void Window_Load()
    {
        gl = window.CreateOpenGLES();
        camera = new Camera();

        IInputContext inputContext = window.CreateInput();

        mouse = inputContext.Mice[0];
        keyboard = inputContext.Keyboards[0];

        lightCube = new Cube(gl);
        lightingCube = new Cube(gl);

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
        if (mouse.IsButtonPressed(MouseButton.Left))
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

        lightCube.Transform = Matrix4X4.CreateScale(new Vector3D<float>(0.5f, 0.5f, 0.5f)) * Matrix4X4.CreateTranslation(lightPos);
        lightingCube.Transform = Matrix4X4.CreateTranslation(lightingPos);
    }

    private static void Window_Render(double obj)
    {
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        // 光源
        {
            uint positionAttrib = (uint)lightProgram.GetAttrib("position");

            gl.EnableVertexAttribArray(positionAttrib);

            lightProgram.Enable();

            lightProgram.SetUniform("model", lightCube.Transform);
            lightProgram.SetUniform("view", camera.View);
            lightProgram.SetUniform("projection", camera.Projection);

            lightCube.Draw(positionAttrib);

            lightProgram.Disable();

            gl.DisableVertexAttribArray(positionAttrib);
        }

        // 进行光照
        {
            uint positionAttrib = (uint)lightingProgram.GetAttrib("position");

            gl.EnableVertexAttribArray(positionAttrib);

            lightingProgram.Enable();

            lightingProgram.SetUniform("model", lightingCube.Transform);
            lightingProgram.SetUniform("view", camera.View);
            lightingProgram.SetUniform("projection", camera.Projection);

            lightingProgram.SetUniform("objectColor", new Vector3D<float>(1.0f, 0.5f, 0.31f));
            lightingProgram.SetUniform("lightColor", new Vector3D<float>(1.0f, 1.0f, 1.0f));

            lightingCube.Draw(positionAttrib);

            lightingProgram.Disable();

            gl.DisableVertexAttribArray(positionAttrib);
        }
    }

    private static void Window_FramebufferResize(Vector2D<int> obj)
    {
        gl.Viewport(obj);
    }
}
