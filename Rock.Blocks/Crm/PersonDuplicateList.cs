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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Utility.Enums;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.PersonDuplicateList;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Displays a list of person records that have possible duplicates.
    /// </summary>
    [DisplayName( "Person Duplicate List" )]
    [Category( "CRM" )]
    [Description( "List of person records that have possible duplicates." )]
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

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 4 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "AD36B5DE-2781-459D-9B83-CB2838AED608" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "D050DA4E-94C4-48A4-9264-6AAA90B319B3" )]
    [Rock.SystemGuid.BlockTypeGuid( "12D89810-23EB-4818-99A2-E076097DD979" )]
    [CustomizedGrid]
    public class PersonDuplicateList : RockListBlockType<PersonDuplicateList.PersonDuplicateRow>
    {
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

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<PersonDuplicateListOptionsBag>();
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
        private PersonDuplicateListOptionsBag GetBoxOptions()
        {
            var hasMultipleCampuses = CampusCache.All().Count( c => c.IsActive ?? true ) > 1;

            return new PersonDuplicateListOptionsBag
            {
                HasMultipleCampuses = hasMultipleCampuses,
                ConfidenceScoreHigh = GetAttributeValue( AttributeKey.ConfidenceScoreHigh ).AsDoubleOrNull(),
                ConfidenceScoreLow = GetAttributeValue( AttributeKey.ConfidenceScoreLow ).AsDoubleOrNull()
            };
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, PageParameterKey.PersonId, "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<PersonDuplicateRow> GetListQueryable( RockContext rockContext )
        {
            var personDuplicateService = new PersonDuplicateService( rockContext );
            int recordStatusInactiveId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() ).Id;
            int recordTypeBusinessId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;

            // List duplicates that:
            // - aren't confirmed as NotDuplicate and aren't IgnoreUntilScoreChanges.
            // - don't have the PersonAlias and DuplicatePersonAlias records pointing to the same person
            //   (occurs after two people have been merged but before the Calculate Person Duplicates job runs).
            var personDuplicateQry = personDuplicateService.Queryable()
                .Where( a => !a.IsConfirmedAsNotDuplicate )
                .Where( a => !a.IgnoreUntilScoreChanges )
                .Where( a => a.PersonAlias.PersonId != a.DuplicatePersonAlias.PersonId );

            if ( !GetAttributeValue( AttributeKey.IncludeInactive ).AsBoolean() )
            {
                personDuplicateQry = personDuplicateQry.Where( a =>
                    !( a.PersonAlias.Person.RecordStatusValueId == recordStatusInactiveId
                    && a.DuplicatePersonAlias.Person.RecordStatusValueId == recordStatusInactiveId ) );
            }

            if ( !GetAttributeValue( AttributeKey.IncludeBusinesses ).AsBoolean() )
            {
                personDuplicateQry = personDuplicateQry.Where( a =>
                    !( a.PersonAlias.Person.RecordTypeValueId == recordTypeBusinessId
                    || a.DuplicatePersonAlias.Person.RecordTypeValueId == recordTypeBusinessId ) );
            }

            var confidenceScoreLow = GetAttributeValue( AttributeKey.ConfidenceScoreLow ).AsDoubleOrNull();
            if ( confidenceScoreLow.HasValue )
            {
                personDuplicateQry = personDuplicateQry.Where( a => a.ConfidenceScore > confidenceScoreLow );
            }

            // Group by PersonId and join with Person to produce the aggregate row data.
            var groupByQry = personDuplicateQry.GroupBy( a => a.PersonAlias.PersonId );

            var personQry = new PersonService( rockContext ).Queryable();

            var queryable = groupByQry
                .Select( a => new
                {
                    PersonId = a.Key,
                    MatchCount = a.Count(),
                    MaxConfidenceScore = a.Max( s => s.ConfidenceScore )
                } )
                .Join(
                    personQry,
                    dup => dup.PersonId,
                    person => person.Id,
                    ( dup, person ) => new PersonDuplicateRow
                    {
                        Person = person,
                        MatchCount = dup.MatchCount,
                        MaxConfidenceScore = dup.MaxConfidenceScore,
                        Campus = person.PrimaryCampus.Name,
                        AccountProtectionProfile = person.AccountProtectionProfile,
                        Suffix = person.SuffixValue.Value,
                        CreatedByPerson = person.CreatedByPersonAlias.Person,
                        PersonModifiedDateTime = person.ModifiedDateTime
                    } );

            return queryable;
        }

        /// <inheritdoc/>
        protected override IQueryable<PersonDuplicateRow> GetOrderedListQueryable( IQueryable<PersonDuplicateRow> queryable, RockContext rockContext )
        {
            return queryable
                .OrderByDescending( a => a.MaxConfidenceScore )
                .ThenBy( a => a.Person.LastName )
                .ThenBy( a => a.Person.FirstName );
        }

        /// <inheritdoc/>
        protected override GridBuilder<PersonDuplicateRow> GetGridBuilder()
        {
            return new GridBuilder<PersonDuplicateRow>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.Person.IdKey )
                .AddField( "maxConfidenceScore", a => a.MaxConfidenceScore )
                .AddTextField( "campus", a => a.Campus )
                .AddTextField( "accountProtectionProfile", a => a.AccountProtectionProfile.ConvertToString() )
                .AddField( "accountProtectionProfileValue", a => ( int ) a.AccountProtectionProfile )
                .AddTextField( "firstName", a => a.Person.FirstName )
                .AddTextField( "lastName", a => a.Person.LastName )
                .AddTextField( "suffix", a => a.Suffix )
                .AddField( "matchCount", a => a.MatchCount )
                .AddDateTimeField( "personModifiedDateTime", a => a.PersonModifiedDateTime )
                .AddPersonField( "createdByPerson", a => a.CreatedByPerson );
        }

        #endregion Methods

        #region Helper Classes

        /// <summary>
        /// A POCO that represents a row in the person duplicate list grid.
        /// Contains the person entity and aggregated duplicate match data.
        /// </summary>
        public class PersonDuplicateRow
        {
            /// <summary>
            /// Gets or sets the person entity.
            /// </summary>
            public Person Person { get; set; }

            /// <summary>
            /// Gets or sets the number of duplicate matches for this person.
            /// </summary>
            public int MatchCount { get; set; }

            /// <summary>
            /// Gets or sets the maximum confidence score across all duplicate matches.
            /// </summary>
            public double? MaxConfidenceScore { get; set; }

            /// <summary>
            /// Gets or sets the person's name suffix (e.g., Jr., Sr., III).
            /// Projected in the query to avoid lazy-loading the SuffixValue navigation property.
            /// </summary>
            public string Suffix { get; set; }

            /// <summary>
            /// Gets or sets the name of the person's primary campus.
            /// </summary>
            public string Campus { get; set; }

            /// <summary>
            /// Gets or sets the account protection profile of the person.
            /// </summary>
            public AccountProtectionProfile AccountProtectionProfile { get; set; }

            /// <summary>
            /// Gets or sets the person who created this person record.
            /// Projected in the query to avoid lazy-loading the CreatedByPersonAlias navigation property.
            /// </summary>
            public Person CreatedByPerson { get; set; }

            /// <summary>
            /// Gets or sets the date and time the person record was last modified.
            /// </summary>
            public DateTime? PersonModifiedDateTime { get; set; }
        }

        #endregion Helper Classes
    }
}
