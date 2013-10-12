using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SharpGL.SceneGraph;
using SharpGL;

namespace CrashedHopeWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        float[] vertices;
        uint[] indices;
        // used for storing the id of the vbo
        uint[] vertexBufferObjectIds = new uint[1];
        float rotation = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the OpenGLDraw event of the OpenGLControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void OpenGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            OpenGL gl = args.OpenGL;

            //CORNFLOWER BLUE
            gl.ClearColor(0.39f, 0.53f, 0.92f, 1);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();

            // point the camera at the center of the world 0,0,0 and move back 2, up 3 and over 2
            gl.LookAt(3, 0.5f, 3, 0, 0.5f, 0, 0, 1, 0);
            // Set the color to
            gl.Color(0.85f, 0.41f, 0, 1f);

            gl.Rotate(0, rotation++, rotation / 2);


            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, vertexBufferObjectIds[0]);
            gl.EnableClientState(OpenGL.GL_VERTEX_ARRAY);
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, OpenGL.GL_FLOAT, false, 0, new IntPtr(0));

            gl.DrawElements(OpenGL.GL_TRIANGLES, indices.Length, indices);
        }
        
        /// <summary>
        /// Handles the OpenGLInitialized event of the OpenGLControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void OpenGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            OpenGL gl = args.OpenGL;

            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.ShadeModel(OpenGL.GL_SMOOTH);

            gl.Hint(OpenGL.GL_PERSPECTIVE_CORRECTION_HINT, OpenGL.GL_NICEST);

            vertices = new float[]
            {                
                0, 0, 0, // 0 bottom back left
                1, 0, 0, // 1 bottom front left
                1, 1, 0, // 2 top front left
                0, 1, 0, // 3 top back left
                1, 0, 1, // 4 bottom front right
                1, 1, 1, // 5 top front right
            };

            indices = new uint[]
            {
                0, 1, 2, // left bottom triangle
                2, 3, 0,  // left top triangle
                1, 4, 5, // front bottom triangle
                5, 1, 2, // front top triangle
            };

            gl.GenBuffers(1, vertexBufferObjectIds);
            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, vertexBufferObjectIds[0]);

            unsafe
            {
                fixed (float* verts = vertices)
                {
                    var ptr = new IntPtr(verts);
                    gl.BufferData(OpenGL.GL_ARRAY_BUFFER, vertices.Length * sizeof(float), ptr, OpenGL.GL_STATIC_DRAW);
                }
            }
        }

    }
}
