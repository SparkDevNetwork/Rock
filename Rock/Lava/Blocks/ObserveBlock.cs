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
using System.Text.RegularExpressions;
using Rock.Observability;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Allows you to wrap the provide Lava in an observability activity.
    ///
    /// {% observe name:'My custom transactions' %}
    ///     __ Lava to run __
    /// {% endobserve %}
    /// </summary>
    public class ObserveBlock : LavaBlockBase, ILavaSecured
    {
        string _markup = string.Empty;

        string _blockMarkup = null;

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        /// <exception cref="System.Exception">Could not find the variable to place results in.</exception>
        public override void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            _markup = markup;

            // Get the internal content of the block. The list of tokens passed in to custom blocks includes the block closing tag,
            // We need to remove the unmatched closing tag to get the valid internal markup for the block.
            if ( tokens.Any() )
            {
                var lastToken = tokens.Last().Replace( " ", "" );

                if ( lastToken.StartsWith( "{%end" ) || lastToken.StartsWith( "{%-end" ) )
                {
                    _blockMarkup = tokens.Take( tokens.Count - 1 ).JoinStrings( string.Empty );
                }
                else
                {
                    // If the final tag is not an (unmatched) closing tag, include it.
                    _blockMarkup = tokens.JoinStrings( string.Empty );
                }
            }
            else
            {
                _blockMarkup = string.Empty;
            }

            base.OnInitialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            // First ensure that cached commands are allowed in the context
            if ( !this.IsAuthorized( context ) )
            {
                result.Write( string.Format( LavaBlockBase.NotAuthorizedMessage, this.SourceElementName ) );
                return;
            }

            var settings = LavaElementAttributes.NewFromMarkup( _markup, context );
            var activityName = settings.GetString( "name", "Lava Observe" );

            using ( var activity = ObservabilityHelper.StartActivity( activityName ) )
            {
                // Add an custom tags to the activity. These are all attributes on the Lava command except the name
                foreach( var setting in settings.Attributes )
                {
                    if ( setting.Key == "name" )
                    {
                        continue;
                    }

                    activity?.AddTag( setting.Key, setting.Value );
                }

                // Run and return the Lava
                var lavaResults = MergeLava( _blockMarkup.ToString(), context );
                result.Write( lavaResults );
            }
        }

        /// <summary>
        /// Calculates the content hash.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        private int CalculateContentHash( string content )
        {
            return ( content + _markup ).GetHashCode();
        }

        /// <summary>
        /// Merges the lava.
        /// </summary>
        /// <param name="lavaTemplate">The lava template.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private string MergeLava( string lavaTemplate, ILavaRenderContext context )
        {
            // Resolve the Lava template contained in this block in a new context.
            var engine = context.GetService<ILavaEngine>();

            var newContext = engine.NewRenderContext();

            newContext.SetMergeFields( context.GetMergeFields() );
            newContext.SetInternalFields( context.GetInternalFields() );

            // Resolve the inner template.
            var result = engine.RenderTemplate( lavaTemplate, LavaRenderParameters.WithContext( newContext ) );

            return result.Text;
        }

        #region ILavaSecured

        /// <inheritdoc/>
        public string RequiredPermissionKey
        {
            get
            {
                return "Observe";
            }
        }

        #endregion
    }
}