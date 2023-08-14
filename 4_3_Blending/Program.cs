using Core.Helpers;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.Windowing;
using System.Drawing;
using Plane = Core.Models.Plane;
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
    private static Plane plane = null!;
    private static Plane[] grasses = null!;
    private static Plane[] windows = null!;
    #endregion

    #region Positions
    private static Vector3D<float>[] grassesPositions = null!;
    private static Vector3D<float>[] windowsPositions = null!;
    #endregion

    #region Speeds
    private static readonly float cameraSpeed = 4.0f;
    private static readonly float cameraSensitivity = 0.2f;
    #endregion

    #region Programs
    private static ShaderProgram textureProgram = null!;
    #endregion

    #region Textures
    private static Texture planeMap = null!;
    private static Texture grassMap = null!;
    private static Texture windowMap = null!;
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

        plane = new Plane(gl);
        grasses = new Plane[10];
        windows = new Plane[10];

        for (int i = 0; i < grasses.Length; i++)
        {
            grasses[i] = new Plane(gl);
        }

        for (int i = 0; i < windows.Length; i++)
        {
            windows[i] = new Plane(gl);
        }

        grassesPositions = new Vector3D<float>[]
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

        windowsPositions = new Vector3D<float>[]
        {
            new Vector3D<float>(-1.0f, 0.5001f, 1.0f),
            new Vector3D<float>(1.0f, 0.5001f, -1.0f),
            new Vector3D<float>(3.0f, 0.5001f, 1.0f),
            new Vector3D<float>(5.0f, 0.5001f, -1.0f),
            new Vector3D<float>(7.0f, 0.5001f, 1.0f),
            new Vector3D<float>(9.0f, 0.5001f, -1.0f),
            new Vector3D<float>(11.0f, 0.5001f, 1.0f),
            new Vector3D<float>(13.0f, 0.5001f, -1.0f),
            new Vector3D<float>(15.0f, 0.5001f, 1.0f),
            new Vector3D<float>(17.0f, 0.5001f, -1.0f)
        };

        using Shader mvp = new(gl, GLEnum.VertexShader, File.ReadAllText("Shaders/mvp.vert"));
        using Shader texture = new(gl, GLEnum.FragmentShader, File.ReadAllText("Shaders/texture.frag"));

        textureProgram = new ShaderProgram(gl);
        textureProgram.Attach(mvp, texture);

        planeMap = new Texture(gl, GLEnum.Rgba, GLEnum.UnsignedByte);
        planeMap.WriteImage("wood_floor.jpg");

        grassMap = new Texture(gl, GLEnum.Rgba, GLEnum.UnsignedByte, GLEnum.ClampToEdge);
        grassMap.WriteImage("grass.png");

        windowMap = new Texture(gl, GLEnum.Rgba, GLEnum.UnsignedByte, GLEnum.ClampToEdge);
        windowMap.WriteImage("window.png");

        gl.Enable(GLEnum.DepthTest);

        gl.Enable(GLEnum.StencilTest);
        gl.StencilOp(GLEnum.Keep, GLEnum.Keep, GLEnum.Replace);

        gl.Enable(GLEnum.Blend);
        gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

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

        plane.Transform = Matrix4X4.CreateScale(new Vector3D<float>(1.0f, 1.0f, 2.3f)) * Matrix4X4.CreateScale(100.0f);
        plane.TextureScale = new Vector2D<float>(50.0f);

        for (int i = 0; i < grasses.Length; i++)
        {
            grasses[i].Transform = Matrix4X4.CreateRotationX(MathHelper.DegreesToRadians(-90.0f)) * Matrix4X4.CreateTranslation(grassesPositions[i]);
        }

        for (int i = 0; i < windows.Length; i++)
        {
            windows[i].Transform = Matrix4X4.CreateRotationX(MathHelper.DegreesToRadians(-90.0f)) * Matrix4X4.CreateTranslation(windowsPositions[i]);
        }
    }

    private static void Window_Render(double obj)
    {
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        textureProgram.Enable();

        uint positionAttrib = (uint)textureProgram.GetAttrib("position");
        uint texCoordsAttrib = (uint)textureProgram.GetAttrib("texCoords");

        gl.EnableVertexAttribArray(positionAttrib);
        gl.EnableVertexAttribArray(texCoordsAttrib);

        textureProgram.SetUniform("view", camera.View);
        textureProgram.SetUniform("projection", camera.Projection);

        // Floor
        {
            textureProgram.SetUniform("model", plane.Transform);

            gl.ActiveTexture(GLEnum.Texture0);
            planeMap.Enable();

            plane.Draw(positionAttrib, null, texCoordsAttrib);

            planeMap.Disable();
        }

        // Grass
        {
            for (int i = 0; i < grasses.Length; i++)
            {
                textureProgram.SetUniform("model", grasses[i].Transform);

                gl.ActiveTexture(GLEnum.Texture0);
                grassMap.Enable();

                grasses[i].Draw(positionAttrib, null, texCoordsAttrib);

                grassMap.Disable();
            }
        }

        // Windows
        {
            for (int i = 0; i < windows.Length; i++)
            {
                textureProgram.SetUniform("model", windows[i].Transform);

                gl.ActiveTexture(GLEnum.Texture0);
                windowMap.Enable();

                windows[i].Draw(positionAttrib, null, texCoordsAttrib);

                windowMap.Disable();
            }
        }

        gl.DisableVertexAttribArray(texCoordsAttrib);
        gl.DisableVertexAttribArray(positionAttrib);

        textureProgram.Disable();
    }

    private static void Window_FramebufferResize(Vector2D<int> obj)
    {
        gl.Viewport(obj);
    }
}
