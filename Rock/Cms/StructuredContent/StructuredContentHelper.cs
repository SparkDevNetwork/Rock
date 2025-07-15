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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rock.Cms.StructuredContent
{
    /// <summary>
    /// Various helper methods when working with structured content. Automates
    /// many of the tasks you might otherwise have to code by hand.
    /// </summary>
    public class StructuredContentHelper
    {
        #region Fields

        /// <summary>
        /// The custom block renderers that have been found.
        /// </summary>
        private static Dictionary<string, IStructuredContentBlockRenderer> _customRenderers;

        /// <summary>
        /// The standard block renderers that have been found.
        /// </summary>
        private static Dictionary<string, IStructuredContentBlockRenderer> _standardRenderers;

        /// <summary>
        /// The block renderers that will be used during standard render operations.
        /// </summary>
        private static Dictionary<string, IStructuredContentBlockRenderer> _blockRenderers;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the structured content JSON string.
        /// </summary>
        /// <value>
        /// The structured content JSON string.
        /// </value>
        public string Content { get; private set; }

        /// <summary>
        /// Gets the user values JSON string.
        /// </summary>
        public Dictionary<string, string> UserValues { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StructuredContentHelper"/> class.
        /// </summary>
        /// <param name="content">The structured content JSON string.</param>
        public StructuredContentHelper( string content )
        {
            Content = content;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StructuredContentHelper"/> class.
        /// </summary>
        /// <param name="content">The structured content JSON string.</param>
        /// <param name="userValues">The user values associated with this structured content.</param>
        public StructuredContentHelper( string content, Dictionary<string, string> userValues )
            : this( content )
        {
            UserValues = userValues;
        }

        #endregion

        #region Method

        /// <summary>
        /// Gets the custom block renderers registered in the system.
        /// </summary>
        /// <returns>A dictionary of <see cref="IStructuredContentBlockRenderer"/> instances.</returns>
        public virtual IReadOnlyDictionary<string, IStructuredContentBlockRenderer> GetCustomRenderers()
        {
            if ( _customRenderers == null )
            {
                var blockRendererTypes = Reflection.FindTypes( typeof( IStructuredContentBlockRenderer ) )
                    .Where( a => a.Value.Assembly != typeof( BlockTypes.ParagraphRenderer ).Assembly )
                    .Where( t => t.Value.GetCustomAttribute<StructuredContentBlockAttribute>() != null )
                    .Select( a => a.Value );

                var blockRenderers = new Dictionary<string, IStructuredContentBlockRenderer>();

                foreach ( var type in blockRendererTypes )
                {
                    try
                    {
                        var renderer = ( IStructuredContentBlockRenderer ) Activator.CreateInstance( type );

                        blockRenderers.TryAdd( type.GetCustomAttribute<StructuredContentBlockAttribute>().BlockType, renderer );
                    }
                    catch
                    {
                        /* Exception intentionally ignored. */
                    }
                }

                _customRenderers = blockRenderers;
            }

            return _customRenderers;
        }

        /// <summary>
        /// Gets the standard block renderers registered in the system.
        /// </summary>
        /// <returns>A dictionary of <see cref="IStructuredContentBlockRenderer"/> instances.</returns>
        public virtual IReadOnlyDictionary<string, IStructuredContentBlockRenderer> GetStandardRenderers()
        {
            if ( _standardRenderers == null )
            {
                var blockRendererTypes = Reflection.FindTypes( typeof( IStructuredContentBlockRenderer ) )
                    .Where( a => a.Value.Assembly == typeof( BlockTypes.ParagraphRenderer ).Assembly )
                    .Where( t => t.Value.GetCustomAttribute<StructuredContentBlockAttribute>() != null )
                    .Select( a => a.Value );

                var blockRenderers = new Dictionary<string, IStructuredContentBlockRenderer>();

                foreach ( var type in blockRendererTypes )
                {
                    try
                    {
                        var renderer = ( IStructuredContentBlockRenderer ) Activator.CreateInstance( type );

                        blockRenderers.TryAdd( type.GetCustomAttribute<StructuredContentBlockAttribute>().BlockType, renderer );
                    }
                    catch
                    {
                        /* Exception intentionally ignored. */
                    }
                }

                _standardRenderers = blockRenderers;
            }

            return _standardRenderers;
        }

        /// <summary>
        /// Gets the block types used for standard rendering operations.
        /// </summary>
        /// <returns>A dictionary of <see cref="IStructuredContentBlockRenderer"/> instances.</returns>
        public virtual IReadOnlyDictionary<string, IStructuredContentBlockRenderer> GetBlockTypes()
        {
            if ( _blockRenderers == null )
            {
                var blockTypes = GetStandardRenderers().ToDictionary( a => a.Key, a => a.Value );

                foreach ( var renderer in GetCustomRenderers() )
                {
                    blockTypes.TryAdd( renderer.Key, renderer.Value );
                }

                _blockRenderers = blockTypes;
            }

            return _blockRenderers;
        }

        /// <summary>
        /// Renders the content and returns the string representation.
        /// </summary>
        /// <returns>A string that contains the rendered contents.</returns>
        public string Render()
        {
            var sb = new StringBuilder();

            using ( var writer = new StringWriter( sb ) )
            {
                Render( writer );
            }

            return sb.ToString();
        }

        /// <summary>
        /// Renders the block contents.
        /// </summary>
        /// <param name="writer">The writer to use when rendering blocks.</param>
        public void Render( TextWriter writer )
        {
            Render( writer, GetBlockTypes() );
        }

        /// <summary>
        /// Renders the block contents using the specified block types.
        /// </summary>
        /// <param name="writer">The writer to use when rendering blocks.</param>
        /// <param name="blockRenderers">The block types used to render.</param>
        public void Render( TextWriter writer, IReadOnlyDictionary<string, IStructuredContentBlockRenderer> blockRenderers )
        {
            var contentData = Content?.FromJsonOrNull<StructuredContentData>() ?? new StructuredContentData();
            var hasUserValues = UserValues != null && UserValues.Any();

            foreach ( var block in contentData.Blocks )
            {
                if ( blockRenderers.TryGetValue( block.Type, out var blockType ) )
                {
                    // Special handling for NoteRenderer with dynamic block data
                    if ( blockType is Rock.Cms.StructuredContent.BlockTypes.NoteRenderer noteRenderer && hasUserValues )
                    {
                        try
                        {
                            var id = ( string ) block.Data.id;

                            // If we have a user value for this block, use it
                            // and render the note in a special way.
                            if ( UserValues.TryGetValue( id, out var realNoteValue ) )
                            {
                                writer.WriteLine( $"<textarea>{realNoteValue.EncodeXml( true )}</textarea>" );
                                continue;
                            }
                        }
                        catch
                        {
                            // Ignore exceptions
                        }
                    }

                    blockType.Render( writer, block.Data );
                }
            }
        }

        /// <summary>
        /// Processes any Lava in blocks that support Lava merge fields. This will
        /// update the <see cref="Content"/> property with the new values after
        /// the merge fields have been resolved.
        /// </summary>
        /// <param name="mergeFields">The merge fields to use when processing Lava.</param>
        internal void ResolveMergeFields( Dictionary<string, object> mergeFields )
        {
            ResolveMergeFields( mergeFields, GetBlockTypes() );
        }

        /// <summary>
        /// Processes any Lava in blocks that support Lava merge fields. This will
        /// update the <see cref="Content"/> property with the new values after
        /// the merge fields have been resolved.
        /// </summary>
        /// <param name="mergeFields">The merge fields to use when processing Lava.</param>
        /// <param name="blockRenderers">The block types used to process the blocks.</param>
        private void ResolveMergeFields( Dictionary<string, object> mergeFields, IReadOnlyDictionary<string, IStructuredContentBlockRenderer> blockRenderers )
        {
            var contentData = Content?.FromJsonOrNull<StructuredContentData>();

            if ( contentData == null )
            {
                return;
            }

            foreach ( var block in contentData.Blocks )
            {
                if ( !blockRenderers.TryGetValue( block.Type, out var blockType ) )
                {
                    continue;
                }

                if ( blockType is IStructuredContentBlockLavaRenderer lavaRenderer )
                {
                    var newData = lavaRenderer.ResolveMergeFields( mergeFields, block.Data );

                    if ( !ReferenceEquals( newData, block.Data ) )
                    {
                        block.Data = newData;
                    }
                }
            }

            Content = contentData.ToJson( false );
        }

        #endregion
    }
}
