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
using System.Linq;

using Rock.Attribute;
using Rock.Common.Mobile;
using Rock.Common.Mobile.Blocks.Engagement.AddContact;
using Rock.Common.Mobile.ViewModel;
using Rock.Communication;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;

namespace Rock.Blocks.Types.Mobile.Engagement
{

    /// <summary>
    /// Allows you to add contact.
    /// </summary>
    [DisplayName( "Add Contacts" )]
    [Category( "Engagement" )]
    [IconCssClass( "ti ti-users-plus" )]
    [Description( "Allows you to add contact." )]
    [SupportedSiteTypes( SiteType.Mobile )]

    #region Block Attributes

    [MobileNavigationActionField( "Post Save Action",
        Description = "The navigation action to perform when the delete button is pressed.",
        IsRequired = false,
        DefaultValue = MobileNavigationActionFieldAttribute.PopSinglePageValue,
        Key = AttributeKey.PostSave,
        Order = 0 )]

    #endregion

    [SystemGuid.EntityTypeGuid( SystemGuid.EntityType.MOBILE_OUTREACH_ADD_CONTACT_BLOCK_TYPE )]
    [SystemGuid.BlockTypeGuid( SystemGuid.BlockType.MOBILE_OUTREACH_ADD_CONTACT )]
    public class AddContact : RockBlockType
    {
        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The post save action
            /// </summary>
            public const string PostSave = "PostSave";
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Save contact
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult SaveContact( SaveContactBag saveContactBag )
        {
            var contactService = new ContactService( RockContext );

            int? photoId = null;
            if ( saveContactBag.PhotoGuid != null )
            {
                var binaryFileService = new BinaryFileService( RockContext );
                photoId = binaryFileService.GetId( saveContactBag.PhotoGuid.Value );
            }

            var currentPerson = GetCurrentPerson();

            var personAlias = currentPerson?.PrimaryAliasId;
            if ( personAlias == null )
            {
                return ActionBadRequest( "You must be logged in to add contact." );
            }

            if ( saveContactBag.Email.IsNotNullOrWhiteSpace() && !EmailAddressFieldValidator.IsValid( saveContactBag.Email ) )
            {
                return ActionBadRequest( "The email address is not valid." );
            }

            contactService.Add( new Contact
            {
                OwnerPersonAliasId = personAlias.Value,
                ForeignKey = saveContactBag.contactIdentifier,
                FirstName = saveContactBag.FirstName,
                LastName = saveContactBag.LastName,
                Gender = saveContactBag.Gender.ToNative(),
                PhotoId = photoId,
                Email = saveContactBag.Email,
                BirthDay = saveContactBag.Birthday,
                BirthMonth = saveContactBag.BirthMonth,
                BirthYear = saveContactBag.BirthYear,
                MobilePhone = saveContactBag.MobilePhone,
                RelationshipStrength = saveContactBag.RelationshipStrength.ToNative(),
                RelationshipFocus = saveContactBag.RelationshipFocus.ToNative(),
                PrayerCadence = saveContactBag.PrayerCadence.ToNative(),
                PrayerNote = saveContactBag.PrayerNote,
                ConnectionCadence = saveContactBag.ConnectionCadence.ToNative(),
                ConnectionNote = saveContactBag.ConnectionNote,
                HasAcceptedJesus = saveContactBag.HasAcceptedJesus,
                SalvationDay = saveContactBag.SalvationDay,
                SalvationMonth = saveContactBag.SalvationMonth,
                SalvationYear = saveContactBag.SalvationYear,
                HasBeenBaptized = saveContactBag.HasBeenBaptized,
                BaptismDay = saveContactBag.BaptismDay,
                BaptismMonth = saveContactBag.BaptismMonth,
                BaptismYear = saveContactBag.BaptismYear
            } );

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Checks if the contact has already been imported by looking for a matching foreign key.
        /// </summary>
        /// <param name="rockContact"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult CheckForContactMatch( RockContact rockContact )
        {
            var currentAliasId = GetCurrentPerson()?.PrimaryAliasId;
            if ( currentAliasId == null )
            {
                return ActionOk( ContactMatch.NoMatch );
            }

            var qry = new ContactService( RockContext ).Queryable()
                .Where( c => c.OwnerPersonAliasId == currentAliasId );

            // First look for a match on the foreign key, but if that doesn't exist then look for a match on first and last name.
            // If either of those are true then return no match.
            var match = qry
                .Where( c =>
                    c.ForeignKey == rockContact.Identifier ||
                    ( c.FirstName == rockContact.FirstName && c.LastName == rockContact.LastName ) )
                .Select( c => c.ForeignKey == rockContact.Identifier
                        ? ContactMatch.HardMatch
                        : ContactMatch.SoftMatch )
                .DefaultIfEmpty( ContactMatch.NoMatch )
                .Max();

            return ActionOk( match );
        }

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override object GetMobileConfigurationValues()
        {
            return new Rock.Common.Mobile.Blocks.Engagement.AddContact.Configuration
            {
                PostSave = GetAttributeValue( AttributeKey.PostSave ).FromJsonOrNull<MobileNavigationActionViewModel>() ?? new MobileNavigationActionViewModel(),
            };
        }

        #endregion
    }

    public enum ContactMatch
    {
        NoMatch = 0,
        SoftMatch = 1,
        HardMatch = 2
    }
}
