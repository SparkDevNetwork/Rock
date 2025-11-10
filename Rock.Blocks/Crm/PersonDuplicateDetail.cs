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
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.PersonDuplicateDetail;
using Rock.ViewModels.Core.Grid;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Displays a list of person duplicates.
    /// </summary>

    [DisplayName( "Person Duplicate Detail" )]
    [Category( "CRM" )]
    [Description( "Shows records that are possible duplicates of the selected person." )]
    [IconCssClass( "ti ti-users" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [DecimalField(
        "Confidence Score High",
        Key = AttributeKey.ConfidenceScoreHigh,
        Description = "The minimum confidence score required to be considered a likely match.",
        IsRequired = true,
        DefaultDecimalValue = 80.00,
        Order = 0 )]

    [DecimalField(
        "Confidence Score Low",
        Key = AttributeKey.ConfidenceScoreLow,
        Description = "The maximum confidence score required to be considered an unlikely match. Values lower than this will not be shown in the grid.",
        IsRequired = true,
        DefaultDecimalValue = 60.00,
        Order = 1 )]

    [BooleanField(
        "Include Inactive",
        Key = AttributeKey.IncludeInactive,
        Description = "Set to true to also include potential matches when both records are inactive.",
        DefaultBooleanValue = false,
        Order = 2 )]

    [BooleanField(
        "Include Businesses",
        Key = AttributeKey.IncludeBusinesses,
        Description = "Set to true to also include potential matches when either record is a Business.",
        DefaultBooleanValue = false,
        Order = 3 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "B96C02DC-F624-4953-BED3-F7BA52CE854D" )]
    [Rock.SystemGuid.BlockTypeGuid( "AAA53F35-1891-4236-B9CB-37805B9134DF" )]
    [CustomizedGrid]
    public class PersonDuplicateDetail : RockEntityListBlockType<PersonDuplicate>
    {
        #region Fields

        // PersonIds, MatchCount
        Dictionary<int, int> _matchCounts = new Dictionary<int, int>();

        #endregion Fields

        #region Keys

        private static class AttributeKey
        {
            public const string ConfidenceScoreHigh = "ConfidenceScoreHigh";
            public const string ConfidenceScoreLow = "ConfidenceScoreLow";
            public const string IncludeInactive = "IncludeInactive";
            public const string IncludeBusinesses = "IncludeBusinesses";
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string PersonId = "PersonId";
        }

        #endregion Keys

        #region Methods

        #region Initialization Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<PersonDuplicateDetailOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private PersonDuplicateDetailOptionsBag GetBoxOptions()
        {
            var options = new PersonDuplicateDetailOptionsBag();

            options.ConfidenceScoreHigh = GetAttributeValue( AttributeKey.ConfidenceScoreHigh ).AsInteger();
            options.ConfidenceScoreLow = GetAttributeValue( AttributeKey.ConfidenceScoreLow ).AsInteger();
            options.IncludeInactive = GetAttributeValue( AttributeKey.IncludeInactive ).AsBoolean();
            options.IncludeBusinesses = GetAttributeValue( AttributeKey.IncludeBusinesses ).AsBoolean();
            options.HasMultipleCampuses = CampusCache.All().Count( c => c.IsActive ?? true ) > 1;

            return options;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "PersonId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<PersonDuplicate> GetListQueryable( RockContext rockContext )
        {
            var recordStatusInactiveId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() ).Id;
            var recordTypeBusinessId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;

            var personDuplicateService = new PersonDuplicateService( RockContext );
            var personService = new PersonService( RockContext );
            int personId = Rock.Utility.IdHasher.Instance.GetId( PageParameter( "PersonId" ).ToStringSafe() ).Value;

            //// Take duplicates that aren't confirmed as NotDuplicate and aren't IgnoreUntilScoreChanges.
            var query = personDuplicateService.Queryable()
                .Where( a => a.PersonAlias.PersonId == personId && !a.IsConfirmedAsNotDuplicate && !a.IgnoreUntilScoreChanges );

            // Don't include records where both the Person and Duplicate are inactive
            if ( this.GetAttributeValue( AttributeKey.IncludeInactive ).AsBoolean() == false )
            {
                query = query.Where( a => !(
                    a.PersonAlias.Person.RecordStatusValueId == recordStatusInactiveId
                    && a.DuplicatePersonAlias.Person.RecordStatusValueId == recordStatusInactiveId
                ) );
            }

            // Don't include records where either the Person or Duplicate is a Business
            if ( this.GetAttributeValue( AttributeKey.IncludeBusinesses ).AsBoolean() == false )
            {
                query = query.Where( a => !(
                    a.PersonAlias.Person.RecordTypeValueId == recordTypeBusinessId
                    || a.DuplicatePersonAlias.Person.RecordTypeValueId == recordTypeBusinessId
                ) );
            }

            // Don't include records that don't meet the minimum confidence score
            double? confidenceScoreLow = GetAttributeValue( AttributeKey.ConfidenceScoreLow ).AsDoubleOrNull();
            if ( confidenceScoreLow.HasValue )
            {
                query = query.Where( a => a.ConfidenceScore >= confidenceScoreLow );
            }

            return query;
        }

        /// <inheritdoc/>
        protected override IQueryable<PersonDuplicate> GetOrderedListQueryable( IQueryable<PersonDuplicate> query, RockContext rockContext )
        {
            if ( typeof( IOrdered ).IsAssignableFrom( typeof( PersonDuplicate ) ) )
            {
                return query.OrderBy( nameof( IOrdered.Order ) )
                    .ThenBy( personDuplicate => personDuplicate.Id );
            }
            else
            {
                return query = query.OrderByDescending( personDuplicate => personDuplicate.ConfidenceScore )
                    .ThenBy( personDuplicate => personDuplicate.PersonAlias.Person.LastName )
                    .ThenBy( personDuplicate => personDuplicate.PersonAlias.Person.FirstName );
            }
        }

        /// <inheritdoc/>
        protected override GridBuilder<PersonDuplicate> GetGridBuilder()
        {
            // The rows will consist of the Person and the Person's duplicates,
            // so we indicate which is which using the "isDuplicateRow" field
            // and pass ALL of the data and let the front ent sort it out.
            return new GridBuilder<PersonDuplicate>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddField( "personIdKey", a => a.PersonAlias.PersonId != a.DuplicatePersonAlias.PersonId ?
                                               a.DuplicatePersonAlias.Person.IdKey :
                                               a.PersonAlias.Person.IdKey )
                .AddField( "isDuplicateRow", a => a.PersonAlias.PersonId != a.DuplicatePersonAlias.PersonId )
                .AddField( "confidenceScore", a => a.ConfidenceScore.HasValue ? a.ConfidenceScore.Value : 0 )
                .AddTextField( "campus", a => a.PersonAlias.PersonId != a.DuplicatePersonAlias.PersonId ?
                                              a.DuplicatePersonAlias.Person.PrimaryCampus.ToStringSafe() :
                                              a.PersonAlias.Person.PrimaryCampus.ToString() )
                .AddTextField( "accountProtectionProfile", a => a.PersonAlias.PersonId != a.DuplicatePersonAlias.PersonId ?
                                                                a.DuplicatePersonAlias.Person.AccountProtectionProfile.ConvertToString() :
                                                                a.PersonAlias.Person.AccountProtectionProfile.ConvertToString() )
                .AddTextField( "firstName", a => a.PersonAlias.PersonId != a.DuplicatePersonAlias.PersonId ?
                                                 a.DuplicatePersonAlias.Person.FirstName :
                                                 a.PersonAlias.Person.FirstName )
                .AddTextField( "lastName", a => a.PersonAlias.PersonId != a.DuplicatePersonAlias.PersonId ?
                                                 a.DuplicatePersonAlias.Person.LastName :
                                                 a.PersonAlias.Person.LastName )
                .AddTextField( "email", a => a.PersonAlias.PersonId != a.DuplicatePersonAlias.PersonId ?
                                             ( string.IsNullOrEmpty( a.DuplicatePersonAlias.Person.Email ) ? "" : a.DuplicatePersonAlias.Person.Email ) :
                                             ( string.IsNullOrEmpty( a.PersonAlias.Person.Email ) ? "" : a.PersonAlias.Person.Email ) )
                .AddTextField( "gender", a => a.PersonAlias.PersonId != a.DuplicatePersonAlias.PersonId ?
                                             ( string.IsNullOrEmpty( a.DuplicatePersonAlias.Person.Gender.ConvertToStringSafe() ) ? "" : a.DuplicatePersonAlias.Person.Gender.ConvertToStringSafe() ) :
                                             ( string.IsNullOrEmpty( a.PersonAlias.Person.Gender.ConvertToStringSafe() ) ? "" : a.PersonAlias.Person.Gender.ConvertToStringSafe() ) )
                .AddTextField( "age", a => a.PersonAlias.PersonId != a.DuplicatePersonAlias.PersonId ?
                                           a.DuplicatePersonAlias.Person.Age.ToStringSafe() :
                                           a.PersonAlias.Person.Age.ToStringSafe() )
                .AddField( "addresses", a => a.PersonAlias.PersonId != a.DuplicatePersonAlias.PersonId ?
                                             GetAddressesWithType( a.DuplicatePersonAlias.Person ) :
                                             GetAddressesWithType( a.PersonAlias.Person ) )
                .AddField( "phoneNumbers", a => a.PersonAlias.PersonId != a.DuplicatePersonAlias.PersonId ?
                                             GetPhoneNumbersWithType( a.DuplicatePersonAlias.Person ) :
                                             GetPhoneNumbersWithType( a.PersonAlias.Person ) )
                .AddAttributeFields( GetGridAttributes() );
        }

        /// <summary>
        /// Gets a list of addresses for the given person, along with the type of each address.
        /// </summary>
        /// <param name="person">The person whose addresses will be retrieved.</param>
        /// <returns>
        /// A list of key-value pairs where the key is the <see cref="Location"/> and the value is the address type as a <see cref="string"/>.
        /// </returns>
        public List<KeyValuePair<Location, string>> GetAddressesWithType( Person person )
        {
            var addressList = person.GetFamilies()
                .SelectMany( family => family.GroupLocations )
                .OrderByDescending( gl => gl.IsMappedLocation )
                .ThenBy( gl => gl.Id )
                .Select( gl => new
                {
                    Location = gl.Location,
                    TypeValue = gl.GroupLocationTypeValue != null ? gl.GroupLocationTypeValue.Value : null
                } )
                .ToList()
                .Select( x => new KeyValuePair<Location, string>( x.Location, x.TypeValue ) ) // Selects from in-memory list
                .ToList();

            return addressList;
        }

        /// <summary>
        /// Gets a list of phone numbers for the given person, along with the type of each phone number.
        /// </summary>
        /// <param name="person">The person whose phone numbers will be retrieved.</param>
        /// <returns>
        /// A list of key-value pairs where the key is the <see cref="PhoneNumber"/> and the value is the phone number type as a <see cref="string"/>.
        /// </returns>
        public List<KeyValuePair<PhoneNumber, string>> GetPhoneNumbersWithType( Person person )
        {
            if ( person.PhoneNumbers == null )
            {
                return new List<KeyValuePair<PhoneNumber, string>>();
            }

            var phoneNumberList = person.PhoneNumbers
                .OrderBy( pn => pn.NumberTypeValue != null ? pn.NumberTypeValue.Order : int.MaxValue )
                .ThenBy( pn => pn.Id )
                .ToList()
                .Select( pn => new KeyValuePair<PhoneNumber, string>( pn, pn.NumberTypeValue != null ? pn.NumberTypeValue.Value : null ) )
                .ToList();

            return phoneNumberList;
        }

        #endregion Helper Methods

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the bag that describes the grid data to be displayed in the
        /// block.
        /// </summary>
        /// <returns>An action result that contains the grid data.</returns>
        [BlockAction]
        public virtual BlockActionResult GetPerson()
        {
            var gridDataBag = new GridDataBag();

            var personIdParam = PageParameter( PageParameterKey.PersonId ).ToStringSafe();
            var personId = Rock.Utility.IdHasher.Instance.GetId( personIdParam );
            if ( !personId.HasValue )
            {
                return ActionNotFound();
            }

            var personService = new PersonService( RockContext );
            var person = personService.Get( personId.Value );
            if ( person == null )
            {
                return ActionNotFound();
            }

            var row = new Dictionary<string, object>
            {
                ["idKey"] = person.IdKey,
                ["personIdKey"] = person.IdKey,
                ["isDuplicateRow"] = false,
                ["confidenceScore"] = 0,
                ["campus"] = person.PrimaryCampus != null ? person.PrimaryCampus.ToStringSafe() : string.Empty,
                ["accountProtectionProfile"] = person.AccountProtectionProfile.ConvertToString(),
                ["firstName"] = person.FirstName,
                ["lastName"] = person.LastName,
                ["email"] = string.IsNullOrEmpty( person.Email ) ? "" : person.Email,
                ["gender"] = string.IsNullOrEmpty( person.Gender.ConvertToStringSafe() ) ? "" : person.Gender.ConvertToStringSafe(),
                ["age"] = person.Age.ToStringSafe(),
                ["addresses"] = GetAddressesWithType( person ),
                ["phoneNumbers"] = GetPhoneNumbersWithType( person )
            };

            var gridAttributes = GetGridAttributes();
            if ( gridAttributes != null && gridAttributes.Any() )
            {
                person.LoadAttributes( RockContext );
                foreach ( var attr in gridAttributes )
                {
                    row[attr.Key] = person.GetAttributeValue( attr.Key );
                }
            }

            gridDataBag.Rows = new List<Dictionary<string, object>> { row };

            return ActionOk( gridDataBag );
        }

        [BlockAction]
        public virtual BlockActionResult MarkNotDuplicate( string personDuplicateIdKey )
        {
            var personDuplicateService = new PersonDuplicateService( RockContext );
            var personDuplicate = personDuplicateService.Get( personDuplicateIdKey, true );
            personDuplicate.IsConfirmedAsNotDuplicate = true;
            if ( RockContext.SaveChanges() > 0 )
            {
                return ActionOk();
            }
            else
            {
                return ActionBadRequest( $"Failed to mark IsConfirmedAsNotDuplicate for PersonDuplicate with idKey of '{personDuplicateIdKey}'" );
            }
        }

        [BlockAction]
        public virtual BlockActionResult MarkIgnoreDuplicate( string personDuplicateIdKey )
        {
            var personDuplicateService = new PersonDuplicateService( RockContext );
            var personDuplicate = personDuplicateService.Get( personDuplicateIdKey, true );
            personDuplicate.IgnoreUntilScoreChanges = true;
            if ( RockContext.SaveChanges() > 0 )
            {
                return ActionOk();
            }
            else
            {
                return ActionBadRequest( $"Failed to mark IgnoreUntilScoreChanges for PersonDuplicate with idKey of '{personDuplicateIdKey}'" );
            }
        }

        #endregion Block Actions
    }
}
