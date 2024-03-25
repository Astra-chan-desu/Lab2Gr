using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
namespace strakhov2_tomogram_viewer
{


    class Bin
    {
        public static int X, Y, Z;
        public static short[] array;
        public Bin() { }
        public void readBin(string path)
        {
            if (File.Exists(path))
            {
                BinaryReader reader = new BinaryReader(File.OpenRead(path));
                X = reader.ReadInt32();
                Y = reader.ReadInt32();
                Z = reader.ReadInt32();

                int arraySize = X * Y * Z;
                array = new short[arraySize];
                for (int i = 0; i < arraySize; ++i)
                {
                    array[i] = reader.ReadInt16();
                }
            }
        }
    }
    class View
    {
        Bitmap textureImage;
        int VBOtexture;
        public void Load2DTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, VBOtexture);
            BitmapData data = textureImage.LockBits(
                new System.Drawing.Rectangle(0, 0, textureImage.Width, textureImage.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,data.Width,
                data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,PixelType.UnsignedByte,
                data.Scan0);
            
            textureImage.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);

            ErrorCode Er = GL.GetError();
            string str = Er.ToString();
        }
        public void generateTextureImage(int layerNumber)
        {
            textureImage = new Bitmap(Bin.X, Bin.Y);
            for (int i = 0; i< Bin.X; i++)
            {
                for(int j = 0; j< Bin.Y; j++)
                {
                    int pixelNumber = i + j * Bin.X + layerNumber * Bin.X * Bin.Y;
                    textureImage.SetPixel(i, j, TransferFunction(Bin.array[pixelNumber]));
                }
            }
        }
        public void DrawTexture()
        {

            GL.Clear(ClearBufferMask.ColorBufferBit|ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, VBOtexture);

            GL.Begin(BeginMode.Quads);

            GL.Color3(Color.White);

            GL.TexCoord2(0f, 0f);
            GL.Vertex2(0, 0);

            GL.TexCoord2(0f, 1f);
            GL.Vertex2(0, Bin.Y);

            GL.TexCoord2(1f,1f);
            GL.Vertex2(Bin.X, Bin.Y);

            GL.TexCoord2(1f, 0f);
            GL.Vertex2(Bin.X, 0);

            GL.End();

            GL.Disable(EnableCap.Texture2D);   
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        public View() { }
        public void setupView(int width, int height)
        {
            GL.ShadeModel(ShadingModel.Smooth);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Bin.X, Bin.Y, 0, -1, 1);
            GL.Viewport(0, 0, width, height);
        }
        public int min = 0;
        public int max = 2000;
        Color TransferFunction(short value)
        {
            
            int newVal = Clamp((value - min) * 255 / (max - min), 0, 255);
            return Color.FromArgb(255, newVal, newVal, newVal);
        }
        public void DrawQuadstrip(int layerNumber)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            int i = 0;
            int j = 0;
            short value;
            for (int y_coord = 0; y_coord < Bin.Y - 1; y_coord++)
            {
                GL.Begin(BeginMode.QuadStrip);
                for (int x_coord = 0; x_coord < Bin.X - 1; x_coord += 1)

                {

                    // for (int i = 0; i <= 1; i++)
                    //  for (int j = 0; j <= 1; j++)
                    //  {


                    j = 0;
                    value = Bin.array[x_coord + i + (y_coord + j) * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x_coord + i, y_coord + j);

                    j = 1;
                    value = Bin.array[x_coord + i + (y_coord + j) * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x_coord + i, y_coord + j);


                }
                GL.End();
            }
        }
        public void DrawQuads(int layerNumber)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Begin(BeginMode.Quads);
            for (int x_coord = 0; x_coord < Bin.X - 1; x_coord++)
                for (int y_coord = 0; y_coord < Bin.Y - 1; y_coord++)
                {

                    // for (int i = 0; i <= 1; i++)
                    //  for (int j = 0; j <= 1; j++)
                    //  {
                    int i, j;
                    short value;
                    i = 0; j = 0;
                    value = Bin.array[x_coord + i + (y_coord + j) * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value)); 
                    GL.Vertex2(x_coord + i, y_coord + j);

                    i = 0; j = 1;
                    value = Bin.array[x_coord + i + (y_coord + j) * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x_coord + i, y_coord + j);

                    i = 1; j = 1;
                    value = Bin.array[x_coord + i + (y_coord + j) * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x_coord + i, y_coord + j);

                    i = 1; j = 0;
                    value = Bin.array[x_coord + i + (y_coord + j) * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x_coord + i, y_coord + j);
                    //    }
                }
            GL.End();
        }

    }
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
