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
    /// Displays a list of person duplicates.
    /// </summary>

    [DisplayName( "Person Duplicate List" )]
    [Category( "CRM" )]
    [Description( "Displays a list of person duplicates." )]
    [IconCssClass( "fa fa-list" )]
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

    [LinkedPage( "Detail Page",
        Description = "The page that will show the person duplicate details.",
        Key = AttributeKey.DetailPage )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "38acc7c9-ef72-4b76-8c40-7ec35b2d6956" )]
    [Rock.SystemGuid.BlockTypeGuid( "15c94f35-03d4-4d00-9609-c91168e35110" )]
    [CustomizedGrid]
    public class PersonDuplicateList : RockListBlockType<PersonDuplicateResult>
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

            var options = new PersonDuplicateListOptionsBag()
            {
                ConfidenceScoreHigh = GetAttributeValue( AttributeKey.ConfidenceScoreHigh ).AsDoubleOrNull(),
                ConfidenceScoreLow = GetAttributeValue( AttributeKey.ConfidenceScoreLow ).AsDoubleOrNull(),
                HasMultipleCampuses = hasMultipleCampuses
            };

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
        protected override IQueryable<PersonDuplicateResult> GetListQueryable( RockContext rockContext )
        {
            int recordStatusInactiveId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() ).Id;
            int recordTypeBusinessId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;

            // list duplicates that:
            // - aren't confirmed as NotDuplicate and aren't IgnoreUntilScoreChanges,
            // - don't have the PersonAlias and DuplicatePersonAlias records pointing to the same person ( occurs after two people have been merged but before the Calculate Person Duplicates job runs).
            // - don't include records where both the Person and Duplicate are inactive (block option)
            var queryable = new PersonDuplicateService( rockContext )
                .Queryable()
                .Where( a => !a.IsConfirmedAsNotDuplicate )
                .Where( a => !a.IgnoreUntilScoreChanges )
                .Where( a => a.PersonAlias.PersonId != a.DuplicatePersonAlias.PersonId );

            if ( this.GetAttributeValue( AttributeKey.IncludeInactive ).AsBoolean() == false )
            {
                queryable = queryable.Where( a => !( a.PersonAlias.Person.RecordStatusValueId == recordStatusInactiveId && a.DuplicatePersonAlias.Person.RecordStatusValueId == recordStatusInactiveId ) );
            }

            if ( this.GetAttributeValue( AttributeKey.IncludeBusinesses ).AsBoolean() == false )
            {
                queryable = queryable.Where( a => !( a.PersonAlias.Person.RecordTypeValueId == recordTypeBusinessId || a.DuplicatePersonAlias.Person.RecordTypeValueId == recordTypeBusinessId ) );
            }

            double? confidenceScoreLow = GetAttributeValue( AttributeKey.ConfidenceScoreLow ).AsDoubleOrNull();
            if ( confidenceScoreLow.HasValue )
            {
                queryable = queryable.Where( a => a.ConfidenceScore > confidenceScoreLow );
            }
            var groupByQry = queryable.GroupBy( a => a.PersonAlias.PersonId );

            var qryPerson = new PersonService( rockContext ).Queryable();

            IQueryable<PersonDuplicateResult> qry = groupByQry.Select( a => new
            {
                PersonId = a.Key,
                MatchCount = a.Count(),
                MaxConfidenceScore = a.Max( s => s.ConfidenceScore ),
            } ).Join( qryPerson,
                k1 => k1.PersonId,
                k2 => k2.Id,
            ( personDuplicate, person ) =>
                new PersonDuplicateResult
                {
                    PersonId = person.Id,
                    SuffixValueId = person.SuffixValueId,
                    SuffixValue = person.SuffixValue,
                    LastName = person.LastName,
                    FirstName = person.FirstName,
                    PersonModifiedDateTime = person.ModifiedDateTime,
                    CreatedByPerson = person.CreatedByPersonAlias.Person.FirstName + " " + person.CreatedByPersonAlias.Person.LastName,
                    MatchCount = personDuplicate.MatchCount,
                    MaxConfidenceScore = Math.Round( ( double ) personDuplicate.MaxConfidenceScore, 2 ),
                    AccountProtectionProfile = person.AccountProtectionProfile,
                    Campus = person.PrimaryCampus.Name
                }
            ).OrderByDescending( a => a.MaxConfidenceScore ).ThenBy( a => a.LastName ).ThenBy( a => a.FirstName );

            return qry;
        }

        /// <inheritdoc/>
        protected override GridBuilder<PersonDuplicateResult> GetGridBuilder()
        {
            var q = new GridBuilder<PersonDuplicateResult>()
                .WithBlock( this )
                .AddField( "personId", a => a.PersonId )
                .AddField( "suffixValueId", a => a.SuffixValueId )
                .AddTextField( "suffixValue", a => a.SuffixValue != null ? a.SuffixValue.Value : "" )
                .AddTextField( "lastName", a => a.LastName )
                .AddTextField( "firstName", a => a.FirstName )
                .AddDateTimeField( "personModifiedDateTime", a => a.PersonModifiedDateTime )
                .AddTextField( "createdBy", a => a.CreatedByPerson )
                .AddField( "matchCount", a => a.MatchCount )
                .AddField( "maxConfidenceScore", a => a.MaxConfidenceScore )
                .AddField( "accountProtectionProfile", a => a.AccountProtectionProfile )
                .AddTextField( "campus", a => a.Campus )
                ;
            return q;
        }

        #endregion
    }
    public partial class PersonDuplicateResult
    {
        public int PersonId { get; set; }
        public int? SuffixValueId { get; set; }
        public DefinedValue SuffixValue { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public DateTime? PersonModifiedDateTime { get; set; }
        public string CreatedByPerson { get; set; }
        public int MatchCount { get; set; }
        public double? MaxConfidenceScore { get; set; }
        public AccountProtectionProfile AccountProtectionProfile { get; set; }
        public string Campus { get; set; }
    }
}
