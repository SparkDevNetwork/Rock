using System.Collections.Generic;

using Rock.Lava;
using Rock.Lava.Fluid;

namespace BlockGenerator.Lava
{
    /// <summary>
    /// Provides methods to work with Lava in the code generator.
    /// </summary>
    public static class LavaHelper
    {
        private static readonly ILavaEngine _engine;

        static LavaHelper()
        {
            var engineOptions = new LavaEngineConfigurationOptions
            {
                DefaultEnabledCommands = new List<string>()
            };

            var fluidEngine = new FluidEngine();

            fluidEngine.Initialize( engineOptions );
            fluidEngine.RegisterFilters( typeof( Rock.Lava.Filters.TemplateFilters ) );
            fluidEngine.RegisterFilters( typeof( CustomLavaFilters ) );

            _engine = fluidEngine;
        }

        /// <summary>
        /// Renders the specified template and merge fields.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <returns>A string that contains the rendered output.</returns>
        public static string Render( string template, IDictionary<string, object> mergeFields )
        {
            var result = _engine.RenderTemplate( template, LavaRenderParameters.WithContext( _engine.NewRenderContext( mergeFields ) ) );

            if ( result.Error != null )
            {
                return result.Error.Message;
            }

            return result.Text;
        }
    }
}
