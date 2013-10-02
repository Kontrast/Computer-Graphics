using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelParser
{
    /// <summary>
    /// Load 3d model from file
    /// </summary>
    public class ModelLoader : IModelLoader
    {
        #region Private Istance Fields

        private readonly ModelType type;

        #endregion

        #region Public Instance Properties

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The model file extension.
        /// </value>
        ModelType Type {get { return type; } }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelLoader"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public ModelLoader(ModelType type)
        {
            this.type = type;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads the model from file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public virtual Model LoadModel(string filePath)
        {
            //This method must be overridden in child classes
            return null;
        }

        #endregion
    }
}
