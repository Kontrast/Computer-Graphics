using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using SharpGL;

namespace CrashedHopeWPF
{
    [TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
    [Serializable()]
    public class Texture
    {
        public Texture()
        {
        }

        public Texture(OpenGL gl, string path)
        {
            //  Load the image data for the texture.
            Create(gl, path);
        }


        /// <summary>
        /// This wraps the OpenGL function that makes the texture 'current'.
        /// </summary>
        public virtual void Bind(OpenGL gl)
        {
            //	Bind our texture object (make it the current texture).
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, TextureName);
        }

        /// <summary>
        /// This function creates the underlying OpenGL object.
        /// </summary>
        /// <param name="gl"></param>
        /// <returns></returns>
        public virtual bool Create(OpenGL gl)
        {
            //  If the texture currently exists in OpenGL, destroy it.
            Destroy(gl);

            //  Generate and store a texture identifier.
            gl.GenTextures(1, glTextureArray);

            return true;
        }

        /// <summary>
        /// This function creates the texture from an image file.
        /// </summary>
        /// <param name="gl">The OpenGL object.</param>
        /// <param name="path">The path to the image file.</param>
        /// <returns>True if the texture was successfully loaded.</returns>
        public virtual bool Create(OpenGL gl, string path)
        {
            //  Create the underlying OpenGL object.
            Create(gl);

            //  Try and load the bitmap. Return false on failure.
            Bitmap image = new Bitmap(path);
            if (image == null)
                return false;

            //	Get the maximum texture size supported by OpenGL.
            int[] textureMaxSize = { 0 };
            gl.GetInteger(OpenGL.GL_MAX_TEXTURE_SIZE, textureMaxSize);

            //	Find the target width and height sizes, which is just the highest
            //	posible power of two that'll fit into the image.
            int targetWidth = textureMaxSize[0];
            int targetHeight = textureMaxSize[0];

            for (int size = 1; size <= textureMaxSize[0]; size *= 2)
            {
                if (image.Width < size)
                {
                    targetWidth = size / 2;
                    break;
                }
                if (image.Width == size)
                    targetWidth = size;

            }

            for (int size = 1; size <= textureMaxSize[0]; size *= 2)
            {
                if (image.Height < size)
                {
                    targetHeight = size / 2;
                    break;
                }
                if (image.Height == size)
                    targetHeight = size;
            }

            //  If need to scale, do so now.
            if (image.Width != targetWidth || image.Height != targetHeight)
            {
                //  Resize the image.
                Image newImage = image.GetThumbnailImage(targetWidth, targetHeight, null, IntPtr.Zero);

                //  Destory the old image, and reset.
                image.Dispose();
                image = (Bitmap)newImage;
            }

            //  Create an array of pixels the correct size.
            pixelData = new byte[image.Width * image.Height * 4];
            int index = 0;

            //  TODO: The loop below is staggeringly slow and needs speeding up.
            //  Go through the pixels backwards, this seems to be how OpenGL wants the data.
            for (int y = image.Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    pixelData[index++] = pixel.R;
                    pixelData[index++] = pixel.G;
                    pixelData[index++] = pixel.B;
                    pixelData[index++] = pixel.A;
                }
            }

            //	Set the width and height.
            width = image.Width;
            height = image.Height;

            //  Dispose of the image file.
            image.Dispose();

            //	Bind our texture object (make it the current texture).
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, TextureName);

            //  Set the image data.
            gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, (int)OpenGL.GL_RGBA,
                width, height, 0, OpenGL.GL_RGBA, OpenGL.GL_UNSIGNED_BYTE,
                pixelData);

            //  Set linear filtering mode.
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);

            //  We're done!
            return true;
        }


        /// <summary>
        /// This function destroys the OpenGL object that is a representation of this texture.
        /// </summary>
        public virtual void Destroy(OpenGL gl)
        {
            //  Only destroy if we have a valid id.
            if (glTextureArray[0] != 0)
            {
                //	Delete the texture object.
                gl.DeleteTextures(1, glTextureArray);
                glTextureArray[0] = 0;

                //  Destroy the pixel data.
                pixelData = null;

                //  Reset width and height.
                width = height = 0;
            }
        }

        /// <summary>
        /// This function (attempts) to make a bitmap from the raw data. The fact that
        /// the byte array is a managed type makes it slightly more complicated.
        /// </summary>
        /// <returns>The texture object as a Bitmap.</returns>
        public virtual unsafe Bitmap ToBitmap()
        {
            //  Check for the trivial case.
            if (pixelData == null)
                return null;
            else
            {
                //	Fix the address of the pixel data.	
                fixed (byte* p = &pixelData[0])
                {
                    return new Bitmap(width, height, width * 4,
                        PixelFormat.Format32bppRgb, new IntPtr(p));
                }
            }
        }

        #region Member Data

        /// <summary>
        /// This is an array of bytes (r, g, b, a) that represent the pixels in this
        /// texture object.
        /// </summary>
        protected byte[] pixelData = null;

        /// <summary>
        /// The width of the texture image.
        /// </summary>
        protected int width = 0;

        /// <summary>
        /// The height of the texture image.
        /// </summary>
        protected int height = 0;

        /// <summary>
        /// This is for OpenGL textures, it is the unique ID for the OpenGL texture.
        /// </summary>
        protected uint[] glTextureArray = new uint[1] { 0 };

        #endregion

        #region Properties

        [Description("The internal texture code.."), Category("Texture")]
        public uint TextureName
        {
            get { return glTextureArray[0]; }
        }

        #endregion
    }
}
