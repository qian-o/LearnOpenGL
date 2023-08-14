using Core.Helpers;
using Core.Models;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.Windowing;
using System.Drawing;
using Shader = Core.Helpers.Shader;
using Texture = Core.Helpers.Texture;

namespace Examples;

internal class Program
{
    private static IWindow window = null!;
    private static GL gl = null!;
    private static IInputContext inputContext = null!;
    private static Camera camera = null!;

    #region Input
    private static IMouse mouse = null!;
    private static IKeyboard keyboard = null!;
    private static bool firstMove = true;
    private static Vector2D<float> lastPos;
    #endregion

    #region Models
    private static Cube[] cubes = null!;
    #endregion

    #region Positions
    private static Vector3D<float>[] cubePositions = null!;
    #endregion

    #region Speeds
    private static readonly float cameraSpeed = 4.0f;
    private static readonly float cameraSensitivity = 0.2f;
    #endregion

    #region Programs
    // 纯色
    private static ShaderProgram solidColorProgram = null!;
    // 纹理
    private static ShaderProgram textureProgram = null!;
    #endregion

    #region Textures
    private static Texture cubeDiffuseMap = null!;
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
        inputContext = window.CreateInput();
        camera = new Camera
        {
            Position = new Vector3D<float>(0.0f, 2.0f, 3.0f),
            Fov = 45.0f
        };

        mouse = inputContext.Mice[0];
        keyboard = inputContext.Keyboards[0];

        cubes = new Cube[10];

        for (int i = 0; i < cubes.Length; i++)
        {
            cubes[i] = new Cube(gl);
        }

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
        using Shader solidColor = new(gl, GLEnum.FragmentShader, File.ReadAllText("Shaders/solid_color.frag"));
        using Shader texture = new(gl, GLEnum.FragmentShader, File.ReadAllText("Shaders/texture.frag"));

        solidColorProgram = new ShaderProgram(gl);
        solidColorProgram.Attach(mvp, solidColor);

        textureProgram = new ShaderProgram(gl);
        textureProgram.Attach(mvp, texture);

        cubeDiffuseMap = new Texture(gl, GLEnum.Rgba, GLEnum.UnsignedByte);
        cubeDiffuseMap.WriteImage("container2.png");

        gl.Enable(GLEnum.DepthTest);

        gl.Enable(GLEnum.StencilTest);
        gl.StencilOp(GLEnum.Keep, GLEnum.Keep, GLEnum.Replace);

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

        for (int i = 0; i < cubes.Length; i++)
        {
            cubes[i].Transform = Matrix4X4.CreateTranslation(cubePositions[i]);
        }
    }

    private static void Window_Render(double obj)
    {
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        // 物体
        {
            gl.StencilFunc(GLEnum.Always, 1, 0xFF);
            gl.StencilMask(0xFF);

            textureProgram.Enable();

            textureProgram.SetUniform("view", camera.View);
            textureProgram.SetUniform("projection", camera.Projection);

            uint positionAttrib = (uint)textureProgram.GetAttrib("position");
            uint texCoordsAttrib = (uint)textureProgram.GetAttrib("texCoords");

            gl.EnableVertexAttribArray(positionAttrib);
            gl.EnableVertexAttribArray(texCoordsAttrib);

            gl.ActiveTexture(GLEnum.Texture0);
            cubeDiffuseMap.Enable();

            for (int i = 0; i < cubes.Length; i++)
            {
                textureProgram.SetUniform("model", cubes[i].Transform);

                textureProgram.SetUniform("diffuse", 0);

                cubes[i].Draw(positionAttrib, null, texCoordsAttrib);
            }

            gl.DisableVertexAttribArray(positionAttrib);
            gl.DisableVertexAttribArray(texCoordsAttrib);

            cubeDiffuseMap.Disable();

            textureProgram.Disable();
        }

        // 边框
        {
            gl.StencilFunc(GLEnum.Notequal, 1, 0xFF);
            gl.StencilMask(0x00);

            solidColorProgram.Enable();

            solidColorProgram.SetUniform("view", camera.View);
            solidColorProgram.SetUniform("projection", camera.Projection);

            solidColorProgram.SetUniform("color", new Vector3D<float>(0.04f, 0.28f, 0.26f));

            uint positionAttrib = (uint)solidColorProgram.GetAttrib("position");

            gl.EnableVertexAttribArray(positionAttrib);

            for (int i = 0; i < cubes.Length; i++)
            {
                solidColorProgram.SetUniform("model", Matrix4X4.CreateScale(1.05f) * cubes[i].Transform);

                cubes[i].Draw(positionAttrib);
            }

            gl.DisableVertexAttribArray(positionAttrib);

            solidColorProgram.Disable();

            gl.StencilMask(0xFF);
            gl.StencilFunc(GLEnum.Always, 0, 0xFF);
        }
    }

    private static void Window_FramebufferResize(Vector2D<int> obj)
    {
        gl.Viewport(obj);
    }
}
