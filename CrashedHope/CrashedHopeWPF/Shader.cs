using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SharpGL;

namespace CrashedHopeWPF
{
    public class Shader
    {
        public uint CreateShader(OpenGL gl, string source, uint mode)
        {
            uint shaderId = gl.CreateShader(mode);
            gl.ShaderSource(shaderId, source);
            gl.CompileShader(shaderId);
            StringBuilder glError = new StringBuilder();
            gl.GetShaderInfoLog(shaderId, 1000, new IntPtr(), glError);
            return shaderId;
        }

        public string LoadFile(string path)
        {
            Stream fileStream = new FileStream(path, FileMode.Open);
            StringBuilder source = new StringBuilder();
            
            byte[] buffer = new byte[100];
            int index = 0;

            while (fileStream.Read(buffer, 100*index++, 100) != 0)
            {
                source.Append(buffer);
            }

            return source.ToString();
        }

        public void InitShader(string vName, string fName)
        {

        }
    }
}
