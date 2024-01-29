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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.PhotoOptOut;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Allows a person to opt-out of future photo requests.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Photo Opt-Out" )]
    [Category( "CRM" )]
    [Description( "Allows a person to opt-out of future photo requests." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [TextField(
        "Success Message",
        Key = AttributeKey.SuccessMessage,
        Description = "Message to show after a successful opt out.",
        DefaultValue = "You've been opted out and should no longer receive photo requests from us.",
        IsRequired = false,
        Order = 0 )]

    [TextField(
        "Not Found Message",
        Key = AttributeKey.NotFoundMessage,
        Description = "Message to show if the account is not found.",
        DefaultValue = "We could not find your record in our system. Please contact our office at the number below.",
        IsRequired = false,
        Order = 1 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "63f1d46a-eb78-4b0f-b398-099a83e058e8" )]
    [Rock.SystemGuid.BlockTypeGuid( "7e2dfb55-f1ab-4452-a5df-6ce65fbfddad" )]
    public class PhotoOptOutDetail : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string SuccessMessage = "SuccessMessage";
            public const string NotFoundMessage = "NotFoundMessage";
        }


        private static class PageParameterKey
        {
            public const string Person = "Person";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                return GetPhotoOptOutBag( rockContext );
            }
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="entityBag">The entityBag to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private PhotoOptOutBag GetPhotoOptOutBag( RockContext rockContext )
        {
            Person entity = null;
            var entityBag = new PhotoOptOutBag();
            string personKey = PageParameter( PageParameterKey.Person );

            try
            {
                entity = new PersonService( rockContext ).GetByPersonActionIdentifier( personKey, "OptOut" );
                if ( entity == null )
                {
                    entity = new PersonService( rockContext ).GetByUrlEncodedKey( personKey );
                }
            }
            catch
            {
                entityBag.ErrorMessage = "No, that's not right. Are you sure you copied that web address correctly?";
                entityBag.AlertType = "warning";
                return entityBag;
            }

            if ( entity == null )
            {
                entityBag.ErrorMessage = GetAttributeValue( AttributeKey.NotFoundMessage );
                entityBag.AlertType = "info";
                return entityBag;
            }
            else
            {
                try
                {
                    GroupService service = new GroupService( rockContext );
                    Rock.Model.Group photoRequestGroup = service.GetByGuid( Rock.SystemGuid.Group.GROUP_PHOTO_REQUEST.AsGuid() );
                    var groupMember = photoRequestGroup.Members.Where( m => m.PersonId == entity.Id ).FirstOrDefault();
                    if ( groupMember == null )
                    {
                        groupMember = new GroupMember();
                        groupMember.GroupId = photoRequestGroup.Id;
                        groupMember.PersonId = entity.Id;
                        groupMember.GroupRoleId = photoRequestGroup.GroupType.DefaultGroupRoleId ?? 0;
                        photoRequestGroup.Members.Add( groupMember );
                    }

                    groupMember.GroupMemberStatus = GroupMemberStatus.Inactive;

                    rockContext.SaveChanges();
                    entityBag.IsOptOutSuccessful = true;
                    entityBag.SuccessMessage = GetAttributeValue( AttributeKey.SuccessMessage );
                }
                catch
                {
                    entityBag.ErrorMessage = "Something went wrong and we could not save your request. If it happens again please contact our office at the number below.";
                    entityBag.AlertType = "danger";
                }

                return entityBag;
            }
        }

        #endregion
    }
}
