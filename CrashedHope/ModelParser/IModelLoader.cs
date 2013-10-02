using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelParser
{
    public interface  IModelLoader
    {
        Model LoadModel(string filePath);
    }
}
