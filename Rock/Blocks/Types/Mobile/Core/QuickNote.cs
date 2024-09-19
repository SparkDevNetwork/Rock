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
using System.ComponentModel;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Core
{
    /// <summary>
    /// A block that allows an individual to quickly add a note that is not tied
    /// to any particular entity.
    /// </summary>

    [DisplayName( "Quick Note" )]
    [Category( "Mobile > Core" )]
    [Description( "Allows an individual to quickly add a note that is not tied to any specific entity." )]
    [IconCssClass( "fa fa-sticky-note" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [NoteTypeField( "Note Type",
        Description = "The note type associated with the Quick Note.",
        AllowMultiple = false,
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.NoteType.QUICK_NOTE,
        Order = 0,
        Key = AttributeKey.NoteType )]

    [TextField( "Placeholder Text",
        Description = "The text to display in the note text area when it is empty.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.PlaceholderText,
        DefaultValue = "Add a quick note..." )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_CORE_QUICK_NOTE_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_CORE_QUICK_NOTE )]
    public class QuickNote : RockBlockType
    {
        #region Keys

        /// <summary>
        /// A list of attribute keys used by the block.
        /// </summary>
        private static class AttributeKey
        {
            public const string NoteType = "NoteType";
            public const string PlaceholderText = "PlaceholderText";
        }

        #endregion

        #region Properties

        /// <summary>
        /// The note type to associated with the quick note.
        /// </summary>
        protected Guid? NoteType => GetAttributeValue( AttributeKey.NoteType ).AsGuidOrNull();

        #endregion

        #region Methods

        /// <summary>
        /// Adds a note to the system.
        /// </summary>
        /// <returns>The ID of the note that was added.</returns>
        private static int AddQuickNote( Guid noteTypeGuid, string noteText, RockContext rockContext )
        {
            var noteService = new NoteService( rockContext );
            var note = new Note();

            var noteTypeId = NoteTypeCache.Get( noteTypeGuid ).Id;
            note.NoteTypeId = noteTypeId;
            note.Text = noteText;
            note.IsSystem = false;
            note.IsAlert = false;

            noteService.Add( note );
            rockContext.SaveChanges();

            return note.Id;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Saves a new or updates an existing note.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult SaveNote( AddQuickNoteRequestBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( NoteType == null )
                {
                    return ActionBadRequest( "A note type is required." );
                }

                var noteType = NoteTypeCache.Get( NoteType.Value );
                if ( !noteType.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionForbidden( "Not authorized to add note." );
                }

                AddQuickNote( NoteType.Value, options.NoteText, rockContext );

                return new BlockActionResult( System.Net.HttpStatusCode.OK );
            }
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// The request bag for the AddQuickNote action.
        /// </summary>
        public class AddQuickNoteRequestBag
        {
            /// <summary>
            /// Gets or sets the text of the note.
            /// </summary>
            public string NoteText { get; set; }
        }

        #endregion 
    }
}
