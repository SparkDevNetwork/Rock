using System.Collections.Generic;

using Rock.Lava;
using Rock.Lava.Fluid;

namespace Rock.CodeGeneration.Lava
{
    /// <summary>
    /// Provides methods to work with Lava in the code generator.
    /// </summary>
    public static class LavaHelper
    {
        #region Fields

        /// <summary>
        /// The engine that is being used to render lava templates.
        /// </summary>
        private static readonly ILavaEngine _engine;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes static members of the <see cref="LavaHelper"/> class.
        /// </summary>
        static LavaHelper()
        {
            var engineOptions = new LavaEngineConfigurationOptions
            {
                DefaultEnabledCommands = new List<string>()
            };

            var fluidEngine = new FluidEngine();

            // Initialize the engine and register all the filters.
            fluidEngine.Initialize( engineOptions );
            fluidEngine.RegisterFilters( typeof( Rock.Lava.Filters.TemplateFilters ) );
            fluidEngine.RegisterFilters( typeof( Rock.Lava.LavaFilters ) );
            fluidEngine.RegisterFilters( typeof( CustomLavaFilters ) );

            _engine = fluidEngine;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Renders the specified template and merge fields.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <returns>A string that contains the rendered output.</returns>
        public static string Render( string template, IDictionary<string, object> mergeFields )
        {
            var result = _engine.RenderTemplate( template, LavaRenderParameters.WithContext( _engine.NewRenderContext( mergeFields ) ) );

            return result.Error != null ? result.Error.Message : result.Text;
        }

        #endregion
    }
}
