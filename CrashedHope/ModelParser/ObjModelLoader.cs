using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meshomatic;

namespace ModelParser
{
    /// <summary>
    /// Load model from file with .obj extension
    /// </summary>
    public class ObjModelLoader : ModelLoader
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjModelLoader"/> class.
        /// </summary>
        public ObjModelLoader() 
            : base (ModelType.ObjModel)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads the model from file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public override Model LoadModel(string filePath)
        {
	        Model result = null;

            ObjLoader loader = new ObjLoader();
	        MeshData modelData = loader.LoadFile(filePath);

	        if (modelData != null)
			{
		         result = new Model() {
			        Data = loader.LoadFile(filePath)
		        };
	        }

	        return result;
        }

        #endregion
    }
}
