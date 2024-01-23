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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Cms
{
    /// <summary>
    /// Displays a structured content channel item for the user to view and fill out.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Structured Content View" )]
    [Category( "Mobile > Cms" )]
    [Description( "Displays a structured content channel item for the user to view and fill out." )]
    [IconCssClass( "fa fa-list-alt" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [CodeEditorField( "Document Not Found Content",
        Description = "Template used to render when a document isn't found. Lava is not enabled. Leave blank to display nothing.",
        IsRequired = false,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Xml,
        Key = AttributeKeys.DocumentNotFoundContent,
        Order = 0 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_CMS_STRUCTUREDCONTENTVIEW_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( "A8BBE3F8-F3CC-4C0A-AB2F-5085F5BF59E7")]
    public class StructuredContentView : RockBlockType
    {
        /// <summary>
        /// The block setting attribute keys for the Structured Content View block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The document not found content attribute key.
            /// </summary>
            public const string DocumentNotFoundContent = "DocumentNotFoundContent";
        }

        #region Attribute Properties

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 1 );

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            var config = new Rock.Common.Mobile.Blocks.Cms.StructuredContentView.Configuration
            {
                DocumentNotFoundContent = GetAttributeValue( AttributeKeys.DocumentNotFoundContent ) ?? ""   
            };

            return config;
        }

        #endregion

        #region Methods

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the structured content for the given item.
        /// </summary>
        /// <param name="itemGuid">The content channel item unique identifier.</param>
        /// <returns>
        /// The structured content for the item.
        /// </returns>
        [BlockAction]
        public object GetStructuredContent( Guid itemGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var contentChannelItemService = new ContentChannelItemService( rockContext );
                var noteTypeId = NoteTypeCache.Get( SystemGuid.NoteType.CONTENT_CHANNEL_ITEM_STRUCTURED_CONTENT_USER_VALUE ).Id;

                var item = contentChannelItemService.Get( itemGuid );
                Dictionary<string, string> userValues = null;

                if ( item != null && RequestContext.CurrentPerson != null )
                {
                    var noteService = new NoteService( rockContext );
                    var note = noteService.Queryable()
                        .AsNoTracking()
                        .Where( a => a.NoteTypeId == noteTypeId && a.EntityId == item.Id )
                        .Where( a => a.CreatedByPersonAliasId.HasValue && a.CreatedByPersonAlias.PersonId == RequestContext.CurrentPerson.Id )
                        .FirstOrDefault();

                    if ( note != null )
                    {
                        userValues = note.Text.FromJsonOrNull<Dictionary<string, string>>();
                    }
                }

                return new
                {
                    DocumentJson = item?.StructuredContent,
                    UserValues = userValues
                };
            }
        }

        /// <summary>
        /// Saves the user values.
        /// </summary>
        /// <param name="itemGuid">The item unique identifier.</param>
        /// <param name="userValues">The user values.</param>
        /// <returns></returns>
        [BlockAction]
        public object SaveUserValues( Guid itemGuid, Dictionary<string, string> userValues )
        {
            if ( RequestContext.CurrentPerson == null )
            {
                return ActionStatusCode( System.Net.HttpStatusCode.Unauthorized );
            }

            using ( var rockContext = new RockContext() )
            {
                var contentChannelItemService = new ContentChannelItemService( rockContext );
                var noteService = new NoteService( rockContext );
                var noteTypeId = NoteTypeCache.Get( SystemGuid.NoteType.CONTENT_CHANNEL_ITEM_STRUCTURED_CONTENT_USER_VALUE ).Id;

                var item = contentChannelItemService.Get( itemGuid );

                if ( item == null )
                {
                    return ActionNotFound();
                }

                var note = noteService.Queryable()
                    .Where( a => a.NoteTypeId == noteTypeId && a.EntityId == item.Id )
                    .Where( a => a.CreatedByPersonAliasId.HasValue && a.CreatedByPersonAlias.PersonId == RequestContext.CurrentPerson.Id )
                    .FirstOrDefault();

                if ( note == null )
                {
                    note = new Note
                    {
                        NoteTypeId = noteTypeId,
                        EntityId = item.Id
                    };

                    noteService.Add( note );
                }

                note.Text = userValues.ToJson();

                rockContext.SaveChanges();
            }

            return ActionOk();
        }

        #endregion
    }
}
