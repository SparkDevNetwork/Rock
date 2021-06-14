// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using DotLiquid;
using DotLiquid.Exceptions;
using DotLiquid.FileSystems;
using Rock.Lava.DotLiquid;

namespace Rock.Lava
{
    /// <summary>
    /// A proxy for a LavaFileSystem component that is compatible with the DotLiquid framework.
    /// </summary>
    internal class DotLiquidFileSystem : ILavaFileSystem, IFileSystem
    {
        private ILavaFileSystem _lavaFileSystem;

        #region Constructors

        /// <summary>
        /// Create a new proxy for a LavaFileSystem instance.
        /// </summary>
        /// <param name="lavaFileSystem"></param>
        public DotLiquidFileSystem( ILavaFileSystem lavaFileSystem )
        {
            _lavaFileSystem = lavaFileSystem;
        }

        #endregion

        #region IFileSystem implementation

        private static LavaToLiquidTemplateConverter _lavaConverter = new LavaToLiquidTemplateConverter();

        string IFileSystem.ReadTemplateFile( Context context, string templateName )
        {
            // Trim delimiters from the template name.
            templateName = templateName ?? string.Empty;
            templateName = templateName.Trim( @"'""".ToCharArray() );

            try
            {
                var lavaContext = new DotLiquidRenderContext( context );

                // This method is called directly by the DotLiquid framework.
                // Therefore, we need to load the Lava template from the file and convert it to Liquid-compatible syntax before returning it to the DotLiquid engine.
                var lavaText = _lavaFileSystem.ReadTemplateFile( lavaContext, templateName );

                var liquidText = _lavaConverter.ConvertToLiquid( lavaText );

                return liquidText;
            }
            catch
            {
                throw new LavaException( "File Load Failed. The template \"{0}\" could not loaded.", templateName );
            }
        }

        #endregion

        #region ILavaFileSystem implementation

        /// <summary>
        /// Called by Liquid to retrieve a template file
        /// </summary>
        /// <param name="context"></param>
        /// <param name="templateName"></param>
        /// <returns></returns>
        /// <exception cref="FileSystemException">LavaFileSystem Template Not Found</exception>
        public string ReadTemplateFile( ILavaRenderContext context, string templateName )
        {
            return _lavaFileSystem.ReadTemplateFile( context, templateName );
        }

        /// <summary>
        /// Returns a flag indicating if the specified file exists.
        /// </summary>
        /// <param name="filePath">A relative file path.</param>
        /// <returns></returns>
        public bool FileExists( string filePath )
        {
            return _lavaFileSystem.FileExists( filePath );
        }

        #endregion
    }
}