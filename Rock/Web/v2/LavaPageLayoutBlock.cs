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
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Extensions.FileProviders;

using Rock.Lava;

namespace Rock.Web.v2
{
    /// <summary>
    /// Special use Lava block for defining a page layout that will be used
    /// to render the primary content of the page, with optional sections that
    /// can be defined in the current template.
    /// </summary>
    internal class LavaPageLayoutBlock : LavaBlockBase
    {
        #region Fields

        /// <summary>
        /// The markup that makes up the parameters for the block.
        /// </summary>
        private string _markup = string.Empty;

        /// <summary>
        /// The markup contained within the block.
        /// </summary>
        private string _blockMarkup = string.Empty;

        /// <summary>
        /// The file provider to use when accessing files in the application.
        /// </summary>
        private readonly IFileProvider _fileProvider;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LavaPageSectionBlock"/> class.
        /// </summary>
        public LavaPageLayoutBlock( IFileProvider fileProvider )
        {
            _fileProvider = fileProvider;

            IncludeClosingTokenInParseResult = false;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            _markup = markup;

            if ( tokens.Any() )
            {
                _blockMarkup = tokens.JoinStrings( string.Empty );
            }
        }

        /// <inheritdoc/>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            var settings = LavaElementAttributes.NewFromMarkup( _markup, context );
            var src = settings.GetStringOrNull( "src" );
            var engine = context.GetService<ILavaEngine>();

            var renderResult = engine.RenderTemplate( _blockMarkup, LavaRenderParameters.WithContext( context ) );

            context.SetInternalField( "LavaPageBody", renderResult.Text );

            // TODO: Resolve src
            var fileInfo = _fileProvider.GetFileInfo( src );

            if ( !fileInfo.Exists )
            {
                throw new FileNotFoundException( "Lava layout file not found.", src );
            }

            using ( var stream = fileInfo.CreateReadStream() )
            {
                var bytes = stream.ReadBytesToEnd();
                var template = System.Text.Encoding.UTF8.GetString( bytes );

                renderResult = engine.RenderTemplate( template, LavaRenderParameters.WithContext( context ) );

                result.Write( renderResult.Text );
            }
        }

        #endregion
    }
}
