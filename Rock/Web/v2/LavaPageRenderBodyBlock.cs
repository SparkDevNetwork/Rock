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

using Rock.Lava;

namespace Rock.Web.v2
{
    /// <summary>
    /// Special use Lava block for rendering the body content from a template
    /// that has referenced a layout.
    /// </summary>
    internal class LavaPageRenderBodyBlock : LavaBlockBase
    {
        #region Fields

        /// <summary>
        /// The markup contained within the block.
        /// </summary>
        private string _blockMarkup = string.Empty;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LavaPageSectionBlock"/> class.
        /// </summary>
        public LavaPageRenderBodyBlock()
        {
            IncludeClosingTokenInParseResult = false;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            if ( tokens.Any() )
            {
                _blockMarkup = tokens.JoinStrings( string.Empty );
            }
        }

        /// <inheritdoc/>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            var engine = context.GetService<ILavaEngine>();
            var bodyContent = context.GetInternalField( "LavaPageBody" ) as string ?? _blockMarkup;

            if ( bodyContent.IsNotNullOrWhiteSpace() )
            {
                var renderResult = engine.RenderTemplate( bodyContent, LavaRenderParameters.WithContext( context ) );

                result.Write( renderResult.Text );
            }
        }

        #endregion
    }
}
