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
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    [DisplayName( "Structured Content View" )]
    [Category( "Mobile > Cms" )]
    [Description( "Displays a structured content channel item for the user to view and fill out." )]
    [IconCssClass( "fa fa-list-alt" )]

    #region Block Attributes

    #endregion

    public class StructuredContentView : RockMobileBlockType
    {
        /// <summary>
        /// The block setting attribute keys for the Structured Content View block.
        /// </summary>
        public static class AttributeKeys
        {
        }

        #region Attribute Properties

        #endregion

        #region IRockMobileBlockType Implementation

        /// <summary>
        /// Gets the required mobile application binary interface version required to render this block.
        /// </summary>
        /// <value>
        /// The required mobile application binary interface version required to render this block.
        /// </value>
        public override int RequiredMobileAbiVersion => 1;

        /// <summary>
        /// Gets the class name of the mobile block to use during rendering on the device.
        /// </summary>
        /// <value>
        /// The class name of the mobile block to use during rendering on the device
        /// </value>
        public override string MobileBlockType => "Rock.Mobile.Blocks.Cms.StructuredContentView";

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            var config = new
            {
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
