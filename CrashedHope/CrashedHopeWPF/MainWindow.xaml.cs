using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using ModelParser;

namespace CrashedHopeWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Model towerModel;
        private Model boomModel;
        private Model hookModel;
        private Model platformModel;
        private List<float[]> vertices = new List<float[]>(4);
        private List<uint[]> indices = new List<uint[]>(4);
        private List<float[]> normals = new List<float[]>(4);

        private const float G = (float)9.8;
        private float horisontalSpeed = (float)0.0;

        // used for storing the id of the vbo
        uint[] vertexBufferObjectIds = new uint[4];
        uint[] normalBufferObjectIds = new uint[4];

        float boomRotation = 0;


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

            gl.LookAt(-20, 20, -20, 0, 10, 0, 0, 1, 0);
            // Set the color to
            gl.Color(0.7890625f, 0.71484375f, 0.046875f);


            DrawBuffers(0, args);
            DrawBuffers(3, args);
            DrawBuffers(1, args);
            DrawBuffers(2, args);
           
        }

        private void DrawBuffers(int bufferNum, OpenGLEventArgs args)
        {
            OpenGL gl = args.OpenGL;

            gl.PushMatrix();
            Animate(bufferNum, args);
            
            gl.EnableClientState(OpenGL.GL_NORMAL_ARRAY);
            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, normalBufferObjectIds[bufferNum]);
            gl.NormalPointer(OpenGL.GL_FLOAT, 0, new IntPtr(0));

            gl.EnableClientState(OpenGL.GL_VERTEX_ARRAY);
            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, vertexBufferObjectIds[bufferNum]);
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, OpenGL.GL_FLOAT, false, 0, new IntPtr(0));

            gl.DrawElements(OpenGL.GL_TRIANGLES, indices.ElementAt(bufferNum).Length, indices.ElementAt(bufferNum));
           
            gl.PopMatrix();
        }

        private void Animate(int bufferNum, OpenGLEventArgs args)
        {
            OpenGL gl = args.OpenGL;
            if (bufferNum == 1 || bufferNum == 2)
            {
                gl.Rotate(0, boomRotation / 2, boomRotation);
            }
            boomRotation++;
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the OpenGLControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void OpenGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            ObjModelLoader modelLoader = new ObjModelLoader();
            towerModel = modelLoader.LoadModel(@"..\..\Resources\Tower crane_tower.obj");
            boomModel = modelLoader.LoadModel(@"..\..\Resources\Tower crane_boomWithCabin.obj");
            hookModel = modelLoader.LoadModel(@"..\..\Resources\Tower crane_hook.obj");
            platformModel = modelLoader.LoadModel(@"..\..\Resources\Tower crane_platform.obj");

            OpenGL gl = args.OpenGL;

            gl.ShadeModel(OpenGL.GL_SMOOTH);

            gl.GenBuffers(4, vertexBufferObjectIds);
            gl.GenBuffers(4, normalBufferObjectIds);

            AddBuffer(towerModel, args, 0);
            AddBuffer(boomModel, args, 1);
            AddBuffer(hookModel, args, 2);
            AddBuffer(platformModel, args, 3);
        }

        private void AddBuffer(Model model, OpenGLEventArgs args, int bufferNum)
        {
            OpenGL gl = args.OpenGL;

            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.ShadeModel(OpenGL.GL_SMOOTH);

            gl.Hint(OpenGL.GL_PERSPECTIVE_CORRECTION_HINT, OpenGL.GL_NICEST);

            vertices.Add(new float[model.Data.Vertices.Count() * 3]);
            indices.Add(new uint[model.Data.Tris.Count() * 3]);
            normals.Add(new float[model.Data.Normals.Count() * 3]);

            int i = 0;
            foreach (var vertice in model.Data.Vertices)
            {
                vertices.ElementAt(bufferNum)[i] = (float)vertice.X;
                vertices.ElementAt(bufferNum)[i + 1] = (float)vertice.Y;
                vertices.ElementAt(bufferNum)[i + 2] = (float)vertice.Z;
                i += 3;
            }
            i = 0;
            foreach (var ind in model.Data.Tris)
            {
                indices.ElementAt(bufferNum)[i] = (uint)ind.P1.Vertex;
                indices.ElementAt(bufferNum)[i + 1] = (uint)ind.P2.Vertex;
                indices.ElementAt(bufferNum)[i + 2] = (uint)ind.P3.Vertex;
                i += 3;
            }
            i = 0;
            foreach (var normal in model.Data.Normals)
            {
                normals.ElementAt(bufferNum)[i] = (float)normal.X;
                normals.ElementAt(bufferNum)[i + 1] = (float)normal.X;
                normals.ElementAt(bufferNum)[i + 2] = (float)normal.X;
                i += 3;
            }

            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, vertexBufferObjectIds[bufferNum]);

            unsafe
            {
                fixed (float* verts = vertices.ElementAt(bufferNum))
                {
                    var ptr = new IntPtr(verts);
                    int size = vertices.ElementAt(bufferNum).Length * sizeof(float);
                    IntPtr nullPointer = new IntPtr();
                    gl.BufferData(OpenGL.GL_ARRAY_BUFFER, size, nullPointer, OpenGL.GL_STREAM_DRAW);
                    gl.BufferSubData(OpenGL.GL_ARRAY_BUFFER, 0, size, ptr);
                }
            }

            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, normalBufferObjectIds[bufferNum]);

            unsafe
            {
                fixed (float* normal = normals.ElementAt(bufferNum))
                {
                    var ptr = new IntPtr(normal);
                    int size = normals.ElementAt(bufferNum).Length * sizeof(float);
                    gl.BufferData(OpenGL.GL_ARRAY_BUFFER, size, ptr, OpenGL.GL_STATIC_DRAW);
                }
            }
        }
    }
}
