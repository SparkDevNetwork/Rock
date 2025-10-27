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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Rock.Lava;

namespace Rock.Web.v2
{
    /// <summary>
    /// Special use Lava block for defining a page section that can be rendered
    /// later in the layout.
    /// </summary>
    internal class LavaPageSectionBlock : LavaBlockBase
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

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LavaPageSectionBlock"/> class.
        /// </summary>
        public LavaPageSectionBlock()
        {
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
            if ( !( context.GetInternalField( "LavaPageSections" ) is ConcurrentDictionary<string, string> sections ) )
            {
                sections = new ConcurrentDictionary<string, string>();
                context.SetInternalField( "LavaPageSections", sections );
            }

            var settings = LavaElementAttributes.NewFromMarkup( _markup, context );
            var sectionName = settings.GetStringOrNull( "id" );

            if ( sectionName.IsNotNullOrWhiteSpace() )
            {
                sections[sectionName] = _blockMarkup;
            }
        }

        #endregion
    }
}
