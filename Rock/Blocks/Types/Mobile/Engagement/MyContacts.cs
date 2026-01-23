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

using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Engagement.MyContact;
using Rock.Mobile;
using Rock.Model;
using Rock.Utility;

namespace Rock.Blocks.Types.Mobile.Engagement
{
    /// <summary>
    /// Allows you to view and edit an existing contact.
    /// </summary>
    [DisplayName( "My Contact" )]
    [Category( "Engagement" )]
    [IconCssClass( "ti ti-user-circle" )]
    [Description( "Allows you to view and edit an existing contact." )]
    [SupportedSiteTypes( SiteType.Mobile )]

    #region Block Attributes

    [LinkedPage(
        "Add Contact Page",
        Description = "Page to link to when user taps on the plus button.",
        IsRequired = true,
        Key = AttributeKey.AddContact,
        Order = 0 )]

    [LinkedPage(
        "Contact Profile",
        Description = "Page to link to when the user taps on the contact.",
        IsRequired = true,
        Key = AttributeKey.ContactProfil,
        Order = 1 )]

    #endregion

    [SystemGuid.EntityTypeGuid( SystemGuid.EntityType.MOBILE_OUTREACH_MY_CONTACTS_BLOCK_TYPE )]
    [SystemGuid.BlockTypeGuid( SystemGuid.BlockType.MOBILE_OUTREACH_MY_CONTACTS )]
    public class MyContacts : RockBlockType
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string AddContact = "AddContact";
            public const string ContactProfil = "ContactProfile";
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Searches the contacts with options.
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult Search( ContactSearchOptions option )
        {
            var currentPerson = GetCurrentPerson();
            if ( currentPerson == null )
            {
                return ActionBadRequest( "You are not logged in" );
            }

            var personAliasId = currentPerson.PrimaryAliasId;
            if ( personAliasId == null )
            {
                return ActionBadRequest( "The current person doesn't have a primary alias Id" );
            }

            ContactService contactService = new ContactService( RockContext );

            var qry = contactService
                .Queryable()
                .AsNoTracking()
                .Where( c => c.OwnerPersonAliasId == personAliasId );

            if ( option.SearchTerm.IsNotNullOrWhiteSpace() )
            {
                var searchTerm = option.SearchTerm.ToLower().Trim();
                qry = qry.Where( c =>
                    ( c.FirstName ?? "" ).ToLower().Contains( searchTerm ) ||
                    ( c.LastName ?? "" ).ToLower().Contains( searchTerm ) ||
                    ( ( ( c.FirstName ?? "" ) + " " + ( c.LastName ?? "" ) ).ToLower().Contains( searchTerm ) )
                );
            }

            var contacts = qry.OrderByDescending( c => c.LastName )
                .Skip( option.Offset )
                .Take( option.Limit )
                .ToList();

            var result = contacts.Select( c => new ContactItem
            {
                ContactIdKey = c.IdKey,
                Name = c.FirstName + " " + c.LastName,
                Gender = c.Gender.ToMobile(),
                ProfilePhotoUrl = c.PhotoId != null ? MobileHelper.BuildPublicApplicationRootUrl( FileUrlHelper.GetImageUrl( c.PhotoId.Value, new GetImageUrlOptions { Width = 256, Height = 256 } ) ) : string.Empty,
            } );

            return ActionOk( result );
        }

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc />
        public override object GetMobileConfigurationValues()
        {
            return new Rock.Common.Mobile.Blocks.Engagement.MyContact.Configuration
            {
                AddContactPageGuid = GetAttributeValue( AttributeKey.AddContact ).AsGuidOrNull(),
                ContactProfileGuid = GetAttributeValue( AttributeKey.ContactProfil ).AsGuidOrNull()
            };
        }

        #endregion
    }
}