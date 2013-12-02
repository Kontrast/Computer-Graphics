using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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
        private static int modelsCount = 6;

        private Model towerModel;
        private Model boomModel;
        private Model hookModel;
        private Model platformModel;
        private Model skySphereModel;
        private List<float[]> vertices = new List<float[]>(modelsCount);
        private List<uint[]> indices = new List<uint[]>(modelsCount);
        private List<float[]> normals = new List<float[]>(modelsCount);
        private List<byte[]> colors = new List<byte[]>(modelsCount);
        private List<float[]> texCoords = new List<float[]>(modelsCount);

        private Model groundModel;

        private DateTime startTime;
        private DateTime phaseOne;
        private DateTime phaseTwo;
        private double duration1;
        private double duration2;
        private float space = 20;
        private double acceleration;

        // used for storing the id of the vbo
        uint[] vertexBufferObjectIds = new uint[modelsCount];
        uint[] normalBufferObjectIds = new uint[modelsCount];
        uint[] colorBufferObjectIds = new uint[modelsCount];
        uint[] texCoordsBufferObjectIds = new uint[modelsCount];

        float[] light0Diffuse = { 1.0f, 1.0f, 1.0f, 0.0f };
        float[] light0Direction = { 1.0f, 0.0f, 1.0f, 0.0f };

        float[] light1Diffuse = {1.0f, 1.0f, 1.0f};
        float[] light1Position = {0.0f, 10.0f, 0.0f, 100000.0f};

        Texture groundTexture = new Texture();
        Texture skyTexture = new Texture();
        uint[] shadowTex = new uint[1];

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimeMarkers(DateTime.Now.AddSeconds(5));
        }

        /// <summary>
        /// Handles the OpenGLDraw event of the OpenGLControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void OpenGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            OpenGL gl = args.OpenGL;
            
            gl.ClearColor(0.39f, 0.53f, 0.92f, 1);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.LoadIdentity();
            
            gl.Ortho(400, -400, 400, -400, 0, -400);
            gl.LookAt(-1, 1, -1, 0, 0, 0, 0, -1, 0);
           
            // Set the color to
            gl.Color(0.7890625f, 0.71484375f, 0.046875f);

            for (int i = 0; i < modelsCount; i++)
            {
                DrawVertexBuffer(i, args);
            }

            gl.Disable(OpenGL.GL_LIGHT0);

            if (DateTime.Now > phaseTwo.AddSeconds(2))
            {
                InitializeTimeMarkers(DateTime.Now);
            }
        }

        private uint[] Shadow(OpenGLEventArgs args, int width, int height)
        {
            OpenGL gl = args.OpenGL;

            gl.MatrixMode(OpenGL.GL_MODELVIEW_MATRIX);
            gl.LoadIdentity();
            gl.Translate(light1Position.ElementAt(0), light1Position.ElementAt(1), light1Position.ElementAt(2));
            
            uint[] shadowTexture = new uint[1];

            // запросим у OpenGL свободный индекс текстуры
            gl.GenTextures(1, shadowTexture);

            // сделаем текстуру активной
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, shadowTexture[0]);

            // установим параметры фильтрации текстуры - линейная фильтрация
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);

            // установим параметры "оборачиваниея" текстуры - отсутствие оборачивания
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_CLAMP_TO_EDGE);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_CLAMP_TO_EDGE);

            // необходимо для использования depth-текстуры как shadow map
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_COMPARE_MODE, OpenGL.GL_COMPARE_REF_TO_TEXTURE);

            // соаздем "пустую" текстуру под depth-данные
            gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, OpenGL.GL_DEPTH_COMPONENT, width, height, 0, OpenGL.GL_DEPTH_COMPONENT, OpenGL.GL_FLOAT, null);

            return shadowTexture;
        }

        private void DrawVertexBuffer(int bufferNum, OpenGLEventArgs args)
        {
            OpenGL gl = args.OpenGL;

            gl.PushMatrix();
            Animate(bufferNum, args);

            gl.Disable(OpenGL.GL_CLIP_PLANE0);
            gl.Disable(OpenGL.GL_CLIP_PLANE1);
            gl.Disable(OpenGL.GL_CLIP_PLANE2);
            gl.Disable(OpenGL.GL_CLIP_PLANE3);
            gl.Disable(OpenGL.GL_CLIP_PLANE4);
            gl.Disable(OpenGL.GL_CLIP_PLANE5);

            gl.EnableClientState(OpenGL.GL_VERTEX_ARRAY);
            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, vertexBufferObjectIds[bufferNum]);
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, OpenGL.GL_FLOAT, false, 0, new IntPtr(0));

            gl.EnableClientState(OpenGL.GL_NORMAL_ARRAY);
            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, normalBufferObjectIds[bufferNum]);
            gl.NormalPointer(OpenGL.GL_FLOAT, 0, new IntPtr(0));

            gl.EnableClientState(OpenGL.GL_COLOR_ARRAY);
            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, colorBufferObjectIds[bufferNum]);
            gl.ColorPointer(3, OpenGL.GL_UNSIGNED_BYTE, 0, new IntPtr(0));

            gl.EnableClientState(OpenGL.GL_TEXTURE_COORD_ARRAY);
            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, texCoordsBufferObjectIds[bufferNum]);
            gl.TexCoordPointer(2, OpenGL.GL_FLOAT, 0, new IntPtr(0));

            gl.DrawElements(OpenGL.GL_TRIANGLES, indices.ElementAt(bufferNum).Length, indices.ElementAt(bufferNum));

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, shadowTex[0]);
            gl.CopyTexSubImage2D(OpenGL.GL_TEXTURE_2D, 0, 0, 0, 0, 0, 512, 512);
            gl.PopMatrix();
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
            skySphereModel = modelLoader.LoadModel(@"..\..\Resources\sphere.obj");

            OpenGL gl = args.OpenGL;

            groundTexture.Create(gl, @"..\..\Resources\ground-texture04.jpg");

            skyTexture.Create(gl, @"..\..\Resources\sky-tex3.jpg");

            //Create the shadow map texture

            gl.GenTextures(1, shadowTex);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, shadowTex[0]);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER,OpenGL. GL_NEAREST);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_CLAMP);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_CLAMP);
            gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, OpenGL.GL_DEPTH_COMPONENT, 1366, 768, 0, OpenGL.GL_DEPTH_COMPONENT, OpenGL.GL_UNSIGNED_BYTE, null);

            //shadowTex = Shadow(args, (int)Application.Current.MainWindow.Height, (int)Application.Current.MainWindow.Width);

            //// Framebuffer Object (FBO) для рендера в него буфера глубины
            //uint[] depthFBO = new uint[1];
            //// переменная для получения состояния FBO
            //uint fboStatus;

            //// создаем FBO для рендера глубины в текстуру
            //gl.GenFramebuffersEXT(1, depthFBO);

            //// делаем созданный FBO текущим
            //gl.BindFramebufferEXT(OpenGL.GL_FRAMEBUFFER_EXT, depthFBO[0]);

            //// отключаем вывод цвета в текущий FBO
            //gl.DrawBuffer(OpenGL.GL_NONE);
            //gl.ReadBuffer(OpenGL.GL_NONE);

            //// указываем для текущего FBO текстуру, куда следует производить рендер глубины
            //gl.FramebufferTexture(OpenGL.GL_FRAMEBUFFER_EXT, OpenGL.GL_DEPTH_ATTACHMENT_EXT, shadowTex[0], 0);

            //// проверим текущий FBO на корректность
            //if ((fboStatus = gl.CheckFramebufferStatusEXT(OpenGL.GL_FRAMEBUFFER_EXT)) != OpenGL.GL_FRAMEBUFFER_COMPLETE_EXT)
            //{
            //    return;
            //}

            //// возвращаем FBO по-умолчанию
            //gl.BindFramebufferEXT(OpenGL.GL_FRAMEBUFFER_EXT, 0);

            //// установим активный FBO
            //gl.BindFramebufferEXT(OpenGL.GL_FRAMEBUFFER_EXT, depthFBO[0]);

            //// размер вьюпорта должен совпадать с размером текстуры для хранения буфера глубины
            //gl.Viewport(0, 0, (int)Application.Current.MainWindow.Height, (int)Application.Current.MainWindow.Width);

            //// отключаем вывод цвета
            ////gl.ColorMask((byte)OpenGL.GL_FALSE, (byte)OpenGL.GL_FALSE, (byte)OpenGL.GL_FALSE, (byte)OpenGL.GL_FALSE);

            //// включаем вывод буфера глубины
            //gl.DepthMask((byte)OpenGL.GL_TRUE);

            //// очищаем буфер глубины перед его заполнением
            //gl.Clear(OpenGL.GL_DEPTH_BUFFER_BIT);

            //// отключаем отображение внешних граней объекта, оставляя внутренние
            //gl.CullFace(OpenGL.GL_FRONT);

            gl.Enable(OpenGL.GL_COLOR_MATERIAL);

            gl.Enable(OpenGL.GL_LIGHTING);
            gl.LightModel(OpenGL.GL_LIGHT_MODEL_TWO_SIDE, OpenGL.GL_TRUE);
            gl.Enable(OpenGL.GL_NORMALIZE);

            gl.Enable(OpenGL.GL_LIGHT1);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, light1Diffuse);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, light1Position);

            gl.Enable(OpenGL.GL_TEXTURE_2D);

            gl.GenBuffers(modelsCount, vertexBufferObjectIds);
            gl.GenBuffers(modelsCount, normalBufferObjectIds);
            gl.GenBuffers(modelsCount, colorBufferObjectIds);
            gl.GenBuffers(modelsCount, texCoordsBufferObjectIds);

            AddBuffer(towerModel, args, 0);
            AddBuffer(boomModel, args, 1);
            AddBuffer(hookModel, args, 2);
            AddBuffer(platformModel, args, 3);
            AddBuffer(groundModel, args, 4);
            AddBuffer(skySphereModel, args, 5);
        }

        private void AddBuffer(Model model, OpenGLEventArgs args, int bufferNum)
        {
            OpenGL gl = args.OpenGL;

            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.ShadeModel(OpenGL.GL_SMOOTH);

            gl.Hint(OpenGL.GL_PERSPECTIVE_CORRECTION_HINT, OpenGL.GL_NICEST);

            vertices.Add(new float[model.Data.Vertices.Count() * 3]);
            colors.Add(new byte[model.Data.Vertices.Count() * 3]);
            indices.Add(new uint[model.Data.Tris.Count() * 3]);
            normals.Add(new float[model.Data.Tris.Count() * 9]);
            texCoords.Add(new float[model.Data.Tris.Count() * 6]);

            int i = 0;
            foreach (var vertice in model.Data.Vertices)
            {
                vertices.ElementAt(bufferNum)[i] = (float)vertice.X;
                vertices.ElementAt(bufferNum)[i + 1] = (float)vertice.Y;
                vertices.ElementAt(bufferNum)[i + 2] = (float)vertice.Z;
                if (bufferNum < 4)
                {
                    colors.ElementAt(bufferNum)[i] = 255;
                    colors.ElementAt(bufferNum)[i + 1] = 204;
                    colors.ElementAt(bufferNum)[i + 2] = 51;
                }
                else
                {
                    colors.ElementAt(bufferNum)[i] = 177;
                    colors.ElementAt(bufferNum)[i + 1] = 177;
                    colors.ElementAt(bufferNum)[i + 2] = 139;
                }
                i += 3;
            }

            i = 0;
            int j = 0;
            foreach (var tri in model.Data.Tris)
            {
                //normals
                normals.ElementAt(bufferNum)[i] = (float)model.Data.Normals.ElementAt(tri.P1.Normal).X;
                normals.ElementAt(bufferNum)[i + 1] = (float)model.Data.Normals.ElementAt(tri.P1.Normal).Y;
                normals.ElementAt(bufferNum)[i + 2] = (float)model.Data.Normals.ElementAt(tri.P1.Normal).Z;
                //textures
                texCoords.ElementAt(bufferNum)[j] = (float)model.Data.TexCoords.ElementAt(tri.P1.TexCoord).X;
                texCoords.ElementAt(bufferNum)[j + 1] = (float)model.Data.TexCoords.ElementAt(tri.P1.TexCoord).Y;

                i += 3;
                j += 2;

                //normals
                normals.ElementAt(bufferNum)[i] = (float)model.Data.Normals.ElementAt(tri.P2.Normal).X;
                normals.ElementAt(bufferNum)[i + 1] = (float)model.Data.Normals.ElementAt(tri.P2.Normal).Y;
                normals.ElementAt(bufferNum)[i + 2] = (float)model.Data.Normals.ElementAt(tri.P2.Normal).Z;
                //textures
                texCoords.ElementAt(bufferNum)[j] = (float)model.Data.TexCoords.ElementAt(tri.P2.TexCoord).X;
                texCoords.ElementAt(bufferNum)[j + 1] = (float)model.Data.TexCoords.ElementAt(tri.P2.TexCoord).Y;

                i += 3;
                j += 2;

                //normals
                normals.ElementAt(bufferNum)[i] = (float)model.Data.Normals.ElementAt(tri.P3.Normal).X;
                normals.ElementAt(bufferNum)[i + 1] = (float)model.Data.Normals.ElementAt(tri.P3.Normal).Y;
                normals.ElementAt(bufferNum)[i + 2] = (float)model.Data.Normals.ElementAt(tri.P3.Normal).Z;
                //textures
                texCoords.ElementAt(bufferNum)[j] = (float)model.Data.TexCoords.ElementAt(tri.P3.TexCoord).X;
                texCoords.ElementAt(bufferNum)[j + 1] = (float)model.Data.TexCoords.ElementAt(tri.P3.TexCoord).Y;

                i += 3;
                j += 2;
            }

            i = 0;
            foreach (var ind in model.Data.Tris)
            {
                indices.ElementAt(bufferNum)[i] = (uint)ind.P1.Vertex;
                indices.ElementAt(bufferNum)[i + 1] = (uint)ind.P2.Vertex;
                indices.ElementAt(bufferNum)[i + 2] = (uint)ind.P3.Vertex;
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

            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, colorBufferObjectIds[bufferNum]);

            unsafe
            {
                fixed (byte* color = colors.ElementAt(bufferNum))
                {
                    var ptr = new IntPtr(color);
                    int size = colors.ElementAt(bufferNum).Length * sizeof(byte);
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
                    IntPtr nullPointer = new IntPtr();
                    gl.BufferData(OpenGL.GL_ARRAY_BUFFER, size, nullPointer, OpenGL.GL_STATIC_DRAW);
                    gl.BufferSubData(OpenGL.GL_ARRAY_BUFFER, 0, size, ptr);
                }
            }

            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, texCoordsBufferObjectIds[bufferNum]);

            unsafe
            {
                fixed (float* texCoord = texCoords.ElementAt(bufferNum))
                {
                    var ptr = new IntPtr(texCoord);
                    int size = texCoords.ElementAt(bufferNum).Length * sizeof(float);
                    IntPtr nullPointer = new IntPtr();
                    gl.BufferData(OpenGL.GL_ARRAY_BUFFER, size, nullPointer, OpenGL.GL_STATIC_DRAW);
                    gl.BufferSubData(OpenGL.GL_ARRAY_BUFFER, 0, size, ptr);
                }
            }
        }

        #region Animation

        private void InitializeTimeMarkers(DateTime startDate)
        {
            startTime = startDate;
            phaseOne = startTime.AddSeconds(5);
            phaseTwo = phaseOne.AddSeconds(10);
            duration1 = phaseOne.Ticks - startTime.Ticks;
            duration2 = phaseTwo.Ticks - phaseOne.Ticks;
            space = 20;
            acceleration = 2 * space / (duration2 * duration2);
        }

        private void Animate(int bufferNum, OpenGLEventArgs args)
        {
            args.OpenGL.Translate(-100, 90, -100);

            switch (bufferNum)
            {
                case 0: AnimationOfObject0(args);
                    break;
                case 1: AnimationOfObject1(args);
                    break;
                case 2: AnimationOfObject2(args);
                    break;
                case 3: AnimationOfObject3(args);
                    break;
                case 4:
                    groundTexture.Bind(args.OpenGL);
                    args.OpenGL.BindTexture(OpenGL.GL_TEXTURE_2D, shadowTex[0]);
                    args.OpenGL.CopyTexSubImage2D(OpenGL.GL_TEXTURE_2D, 0, 0, 0, 0, 0, 1366, 768);
                    //args.OpenGL.BindTexture(OpenGL.GL_TEXTURE_2D, shadowTex[0]);
                    break;
                case 5:
                    args.OpenGL.Scale(200, 200, 200);
                    skyTexture.Bind(args.OpenGL);
                    break;

                default: return;
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
                gl.Rotate(a, a, 0);
            }
            else
            {
                gl.Rotate(90, 90, 0);
            }
        }

        private void AnimationOfObject2(OpenGLEventArgs args)
        {
            OpenGL gl = args.OpenGL;

            if (DateTime.Now < phaseTwo && DateTime.Now > phaseOne)
            {
                var a = (float)
                        ((phaseOne.Ticks - DateTime.Now.Ticks) * (phaseOne.Ticks - DateTime.Now.Ticks) * acceleration / 2);
                gl.Translate(0, -a, 0);
            }
            else
            {
                if (DateTime.Now > phaseTwo)
                {
                    gl.Translate(0, -space, 0);
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

        private void AnimationOfObject3(OpenGLEventArgs args)
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

        #endregion
    }
}
