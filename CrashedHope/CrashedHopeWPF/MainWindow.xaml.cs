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
        private static int modelsCount = 5;

        private Model towerModel;
        private Model boomModel;
        private Model hookModel;
        private Model platformModel;
        private List<float[]> vertices = new List<float[]>(modelsCount);
        private List<uint[]> indices = new List<uint[]>(modelsCount);
        private List<float[]> normals = new List<float[]>(modelsCount);

        private Model groundModel;

        private DateTime startTime;
        private DateTime phaseOne;
        private DateTime phaseTwo;
        private double duration1;        private const float G = (float)9.8;
        private float horisontalSpeed = (float)0.0;

        // used for storing the id of the vbo
        uint[] vertexBufferObjectIds = new uint[modelsCount];
        uint[] normalBufferObjectIds = new uint[modelsCount];

        float[] light0Diffuse = { 1.0f, 1.0f, 1.0f };
        float[] light0Direction = { 0.0f, 0.0f, 1.0f, 0.0f };

        private float color = 0.71484375f;
        public MainWindow()
        {
            InitializeComponent();
            InitializeTimeMarkers(DateTime.Now.AddSeconds(5));
        }

        private void InitializeTimeMarkers(DateTime startDate)
        {
            startTime = startDate;
            phaseOne = startTime.AddSeconds(10);
            phaseTwo = phaseOne.AddSeconds(10);
            duration1 = phaseOne.Ticks - startTime.Ticks;
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

            gl.LookAt(-40, 40, -40, 0, 10, 0, 0, 1, 0);

            // Set the color to
            gl.Color(0.7890625f, 0.71484375f, 0.046875f);

            for (int i = 0; i < modelsCount; i++)
            {
                DrawVertexBuffer(i, args);
            }

            gl.Disable(OpenGL.GL_LIGHT0);
        }

        private void DrawVertexBuffer(int bufferNum, OpenGLEventArgs args)
        {
            OpenGL gl = args.OpenGL;

            gl.Enable(OpenGL.GL_LIGHT0);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0Diffuse);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0Direction);

            light0Diffuse = new float[]{ 1.0f, 1.0f, 1.0f };
            //color -= 0.1f;
            
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

            if (bufferNum == 1)
            {
                AnimationOfObject1(args);
            }
            if (bufferNum == 2)
            {
                AnimationOfObject2(args);
            }

            if (bufferNum == 0)
            {
                AnimationOfObject0(args);
            }
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
            groundModel = modelLoader.LoadModel(@"..\..\Resources\ground.obj");

            OpenGL gl = args.OpenGL;

            gl.ShadeModel(OpenGL.GL_SMOOTH);

            gl.GenBuffers(modelsCount, vertexBufferObjectIds);
            gl.GenBuffers(modelsCount, normalBufferObjectIds);

            AddBuffer(towerModel, args, 0);
            AddBuffer(boomModel, args, 1);
            AddBuffer(hookModel, args, 2);
            AddBuffer(platformModel, args, 3);
            AddBuffer(groundModel, args, 4);

            gl.Enable(OpenGL.GL_LIGHTING);
            gl.LightModel(OpenGL.GL_LIGHT_MODEL_TWO_SIDE, OpenGL.GL_TRUE);
            gl.Enable(OpenGL.GL_NORMALIZE);
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

        private void AnimationOfObject1(OpenGLEventArgs args)
        {
            OpenGL gl = args.OpenGL;
            if (DateTime.Now < phaseOne)
            {
                var a = 90 -
                        (float)
                        ((phaseOne.Ticks - DateTime.Now.Ticks) / duration1 * 90);
                gl.Rotate(0, 0, a);
            }
            else
            {
                gl.Rotate(0, 0, 90);
            }
        }

        private void AnimationOfObject2(OpenGLEventArgs args)
        {
            OpenGL gl = args.OpenGL;
            if (DateTime.Now < phaseTwo && DateTime.Now > phaseOne)
            {
                var a =
                        (float)
                        ((phaseOne.Ticks - DateTime.Now.Ticks) / duration1 * 20);
                gl.Translate(0, a, 0);
            }
            else
            {
                if (DateTime.Now > phaseTwo)
                {
                    gl.Translate(0, 20, 0);
                }
            }
        }

        private void AnimationOfObject0(OpenGLEventArgs args)
        {
            OpenGL gl = args.OpenGL;
            if (DateTime.Now < phaseOne)
            {
                var a = 90 -
                        (float)
                        ((phaseOne.Ticks - DateTime.Now.Ticks) / duration1 * 90);
                gl.Rotate(a, 0, 0);
            }
            else
            {
                gl.Rotate(90, 0, 0);
            }
        }
    }
}
