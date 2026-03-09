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
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.Finance.ConvertBusiness;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    [DisplayName( "Convert Business" )]
    [Category( "Finance" )]
    [Description( "Allows you to convert a Person record into a Business and vice-versa." )]
    [IconCssClass( "ti ti-building" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [DefinedValueField(
        "Default Connection Status",
        Description = "The default connection status to use when converting a business to a person.",
        IsRequired = false,
        Key = AttributeKey.DefaultConnectionStatus,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        Order = 0 )]

    [Rock.SystemGuid.EntityTypeGuid( "F2306C13-C7BC-4A89-B47D-A30DC3DFE2ED" )]
    [Rock.SystemGuid.BlockTypeGuid( "155BC217-1B29-4EFA-A7EA-29C075AE96B3" )]
    public class ConvertBusiness : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DefaultConnectionStatus = "DefaultConnectionStatus";
        }

        private static class Mode
        {
            public const string ToBusiness = "ToBusiness";
            public const string ToPerson = "ToPerson";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc />
        public override object GetObsidianBlockInitialization()
        {
            var box = new ConvertBusinessBag()
            {
                DefaultPersonConnectionStatus = GetDefaultConnectionStatus()?.ToListItemBag()
            };

            return box;
        }

        /// <summary>
        /// Gets the default connection status configured in block settings.
        /// </summary>
        /// <returns>The default connection status value or <c>null</c>.</returns>
        private DefinedValueCache GetDefaultConnectionStatus()
        {
            var defaultConnectionStatusGuid = GetAttributeValue( AttributeKey.DefaultConnectionStatus ).AsGuidOrNull();

            if ( !defaultConnectionStatusGuid.HasValue )
            {
                return null;
            }

            return DefinedValueCache.Get( defaultConnectionStatusGuid.Value );
        }

        /// <summary>
        /// Gets the person represented by the specified person alias GUID.
        /// </summary>
        /// <param name="personAliasGuid">The person alias guid.</param>
        /// <param name="rockContext">The data context.</param>
        /// <returns>The person instance or <c>null</c>.</returns>
        private Person GetPersonFromAliasGuid( Guid personAliasGuid, RockContext rockContext )
        {
            var person = new PersonAliasService( rockContext ).GetPerson( personAliasGuid );
            return person;
        }

        /// <summary>
        /// Gets validation details if the person cannot be converted to a business.
        /// </summary>
        /// <param name="person">The person to validate.</param>
        /// <returns>An error bag if conversion is not allowed, otherwise <c>null</c>.</returns>
        private static ConversionSelectionResultBag ValidateCanConvertToBusiness( Person person )
        {
            //
            // Ensure person record is a member of only one family.
            //
            var families = person.GetFamilies();
            if ( families.Count() != 1 )
            {
                return new ConversionSelectionResultBag
                {
                    ErrorHeading = "Cannot convert person record to a business.",
                    ErrorText = "To avoid data loss move this person to one family before proceeding."
                };
            }

            //
            // Ensure person record is the only record in the family.
            //
            var family = families.First();
            if ( family.Members.Count != 1 )
            {
                return new ConversionSelectionResultBag
                {
                    ErrorHeading = "Cannot convert person record to a business.",
                    ErrorText = "To avoid data loss move this person to their own family before proceeding."
                };
            }

            //
            // Ensure giving group is correct.
            //
            if ( person.GivingGroup == null || person.GivingGroup.Members.Count != 1 || person.GivingLeaderId != person.Id )
            {
                return new ConversionSelectionResultBag
                {
                    ErrorHeading = "Cannot convert person record to a business.",
                    ErrorText = "Please fix the giving group and then try again."
                };
            }

            return null;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Evaluates the selected person/business and prepares conversion data.
        /// </summary>
        /// <param name="personAliasGuid">The selected person alias GUID.</param>
        /// <returns>A bag indicating which conversion mode is available.</returns>
        [BlockAction]
        public BlockActionResult SelectPerson( Guid? personAliasGuid )
        {
            var response = new ConversionSelectionResultBag();

            if ( !personAliasGuid.HasValue )
            {
                return ActionOk( response );
            }

            var person = GetPersonFromAliasGuid( personAliasGuid.Value, RockContext );

            if ( person == null )
            {
                return ActionBadRequest( "Unable to find the selected person or business." );
            }

            // If this is a currently a business record then we'll allow conversion to a person.
            if ( person.RecordTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() )
            {
                response.Mode = Mode.ToPerson;
                response.PersonFirstName = string.Empty;
                response.PersonLastName = person.LastName;
                response.PersonGender = Gender.Unknown.ConvertToInt();
                response.PersonConnectionStatus = GetDefaultConnectionStatus()?.ToListItemBag();

                return ActionOk( response );
            }

            // If this is currently a person record then we'll allow conversion to a business
            // but first need to validate that the person is eligible for conversion.
            var validationResult = ValidateCanConvertToBusiness( person );
            if ( validationResult != null )
            {
                return ActionOk( validationResult );
            }

            response.Mode = Mode.ToBusiness;
            response.BusinessName = $"{person.FirstName} {person.LastName}".Trim();

            return ActionOk( response );

        }

        /// <summary>
        /// Converts a business record to a person record.
        /// </summary>
        /// <param name="bag">The conversion data.</param>
        /// <returns>Details about the successful conversion.</returns>
        [BlockAction]
        public BlockActionResult ConvertToPerson( ConvertToPersonRequestBag bag )
        {
            if ( bag == null || !bag.SourcePersonAliasGuid.HasValue )
            {
                return ActionBadRequest( "A person or business selection is required." );
            }

            if ( bag.PersonConnectionStatus?.Value.IsNullOrWhiteSpace() != false )
            {
                return ActionBadRequest( "Connection Status is required." );
            }

            var person = GetPersonFromAliasGuid( bag.SourcePersonAliasGuid.Value, RockContext );

            if ( person == null )
            {
                return ActionBadRequest( "Unable to find the selected business." );
            }

            var formerFullName = person.FullName;

            person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON ).Id;
            person.ConnectionStatusValueId = DefinedValueCache.Get( bag.PersonConnectionStatus.Value.AsGuid() )?.Id;
            person.FirstName = bag.PersonFirstName?.Trim();
            person.NickName = bag.PersonFirstName?.Trim();
            person.LastName = bag.PersonLastName?.Trim();
            person.Gender = bag.PersonGender;
            person.MaritalStatusValueId = bag.PersonMaritalStatus?.Value.AsGuidOrNull().HasValue == true
                ? DefinedValueCache.Get( bag.PersonMaritalStatus.Value.AsGuid() )?.Id
                : null;

            RockContext.SaveChanges();

            var parameters = new Dictionary<string, string>
            {
                ["PersonId"] = person.Id.ToString()
            };

            var pageRef = new Rock.Web.PageReference( Rock.SystemGuid.Page.PERSON_PROFILE_PERSON_PAGES, parameters );
            var message = $"The business formerly known as '{formerFullName.EncodeHtml()}' has been converted to a person <a href='{pageRef.BuildUrl()}'>{person.FullName.EncodeHtml()}</a>.";

            return ActionOk( new ConversionResultBag
            {
                SuccessMessage = message
            } );
        }

        /// <summary>
        /// Converts a person record to a business record.
        /// </summary>
        /// <param name="bag">The conversion data.</param>
        /// <returns>Details about the successful conversion.</returns>
        [BlockAction]
        public BlockActionResult ConvertToBusiness( ConvertToBusinessRequestBag bag )
        {
            if ( bag == null || !bag.SourcePersonAliasGuid.HasValue )
            {
                return ActionBadRequest( "A person or business selection is required." );
            }

            if ( bag.BusinessName.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Business Name is required." );
            }

            var person = GetPersonFromAliasGuid( bag.SourcePersonAliasGuid.Value, RockContext );

            if ( person == null )
            {
                return ActionBadRequest( "Unable to find the selected person." );
            }

            var validationResult = ValidateCanConvertToBusiness( person );
            if ( validationResult != null )
            {
                return ActionBadRequest( validationResult.ErrorText );
            }

            var formerFullName = person.FullName;

            person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS ).Id;
            person.ConnectionStatusValueId = null;
            person.TitleValueId = null;
            person.FirstName = null;
            person.NickName = null;
            person.MiddleName = null;
            person.LastName = bag.BusinessName.Trim();
            person.SuffixValueId = null;
            person.SetBirthDate( null );
            person.Gender = Gender.Unknown;
            person.MaritalStatusValueId = null;
            person.AnniversaryDate = null;
            person.GraduationYear = null;

            //
            // Check address(es) and make sure one is of type Work.
            //
            var family = person.GetFamily( RockContext );

            if ( family != null && family.GroupLocations.Count > 0 )
            {
                var workLocationTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK ).Id;
                var homeLocationTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME ).Id;

                var workLocation = family.GroupLocations.FirstOrDefault( gl => gl.GroupLocationTypeValueId == workLocationTypeId );
                if ( workLocation == null )
                {
                    var homeLocation = family.GroupLocations.FirstOrDefault( gl => gl.GroupLocationTypeValueId == homeLocationTypeId );
                    if ( homeLocation != null )
                    {
                        homeLocation.GroupLocationTypeValueId = workLocationTypeId;
                    }
                }
            }

            //
            // Check phone(es) and make sure one is of type Work.
            //
            if ( person.PhoneNumbers.Count > 0 )
            {
                var workPhoneTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK ).Id;
                var homePhoneTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ).Id;

                var workPhone = person.PhoneNumbers.FirstOrDefault( pn => pn.NumberTypeValueId == workPhoneTypeId );
                if ( workPhone == null )
                {
                    var homePhone = person.PhoneNumbers.FirstOrDefault( pn => pn.NumberTypeValueId == homePhoneTypeId );
                    if ( homePhone != null )
                    {
                        homePhone.NumberTypeValueId = workPhoneTypeId;
                    }
                }
            }

            if ( family != null )
            {
                //
                // Make sure member status in family is set to Adult.
                //
                var adultRoleId = GroupTypeCache.GetFamilyGroupType().Roles
                    .Where( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() )
                    .Select( a => a.Id )
                    .First();

                var familyMember = family.Members.FirstOrDefault( m => m.PersonId == person.Id );
                if ( familyMember != null )
                {
                    familyMember.GroupRoleId = adultRoleId;
                }
            }

            RockContext.SaveChanges();

            var parameters = new Dictionary<string, string>
            {
                ["BusinessId"] = person.Id.ToString()
            };

            var pageRef = new Rock.Web.PageReference( Rock.SystemGuid.Page.BUSINESS_DETAIL, parameters );
            var message = $"The person formerly known as '{formerFullName.EncodeHtml()}' has been converted to a business <a href='{pageRef.BuildUrl()}'>{person.FullName.EncodeHtml()}</a>.";

            return ActionOk( new ConversionResultBag
            {
                SuccessMessage = message
            } );

        }

        #endregion Block Actions
    }
}
