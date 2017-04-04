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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.PersonProfile.Badge
{
    /// <summary>
    /// Giving Badge
    /// </summary>
    [Description( "This badge highlights if the person has made a contribution within the provided date range." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Giving" )]

    [AccountsField( "Accounts", "The accounts to limit this to, or leave blank to include all accounts", false, order: 1 )]
    [DecimalField( "Minimum Amount", "The minimum contribution amount", required: false, order: 2 )]
    [SlidingDateRangeField( "Date Range", "The date range in which the contributions were made.", defaultValue: "Last|6|Month||", required: false, order: 3 )]
    [CodeEditorField( "Lava Template", "The lava template to use for the badge display", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, order: 4, defaultValue:
        @"
{% if DateRange and DateRange.Summary != '' %}
  {% capture dateRangeText %} in the {{ DateRange.Summary | Downcase  }}{% endcapture %}
{% else %}
  {% assign dateRangeText = '' %}
{% endif %}

{% if Contributions and Contributions.Count > 0 %}
  {% assign iconColor = 'green' %}
  {% capture tooltipText %}{{ Person.NickName }} has contributed {{ Contributions.Count }} times{{ dateRangeText }}.{% endcapture %}
{% else %}
  {% assign iconColor = '#c4c4c4' %}
  {% capture tooltipText %}{{ Person.NickName }} has not contributed{{ dateRangeText }}.{% endcapture %}
{% endif %}

<div class='badge badge-giving badge-id-{{Badge.Id}}' data-toggle='tooltip' data-original-title='{{ tooltipText }}'>
  <i class='badge-icon fa fa-heartbeat' style='color: {{ iconColor }}'></i>
</div>
" )]
    public class Giving : BadgeComponent
    {
        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            var accountGuids = this.GetAttributeValue( badge, "Accounts" )?.SplitDelimitedValues().AsGuidList();
            var minimumAmount = this.GetAttributeValue( badge, "MinimumAmount" )?.AsDecimalOrNull();
            var slidingDateRangeDelimitedValues = this.GetAttributeValue( badge, "DateRange" );
            var lavaTemplate = this.GetAttributeValue( badge, "LavaTemplate" );

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( slidingDateRangeDelimitedValues );
            var dateRangeSummary = SlidingDateRangePicker.FormatDelimitedValues( slidingDateRangeDelimitedValues );

            var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null, null, new Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            mergeFields.Add( "Person", this.Person );
            using ( var rockContext = new RockContext() )
            {
                mergeFields.Add( "Badge", badge );
                mergeFields.Add( "DateRange", new { Dates = dateRange, Summary = dateRangeSummary } );

                // fetch all the possible PersonAliasIds that have this GivingID to help optimize the SQL
                var personAliasIds = new PersonAliasService( rockContext ).Queryable().Where( a => a.Person.GivingId == this.Person.GivingId ).Select( a => a.Id ).ToList();

                var transactionTypeContributionValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() ).Id;
                var qry = new FinancialTransactionService( rockContext ).Queryable().Where( a => a.TransactionTypeValueId == transactionTypeContributionValueId );

                // get the transactions for the person or all the members in the person's giving group (Family)
                qry = qry.Where( t => t.AuthorizedPersonAliasId.HasValue && personAliasIds.Contains( t.AuthorizedPersonAliasId.Value ) );

                if ( dateRange.Start.HasValue )
                {
                    qry = qry.Where( t => t.TransactionDateTime >= dateRange.Start.Value );
                }

                if ( dateRange.End.HasValue )
                {
                    qry = qry.Where( t => t.TransactionDateTime < dateRange.End.Value );
                }

                if ( minimumAmount.HasValue )
                {
                    qry = qry.Where( a => a.TransactionDetails.Sum( d => (decimal?)d.Amount ) > minimumAmount );
                }

                var contributionList = qry.Select( a => new
                {
                    a.TransactionDateTime,
                    ContributionAmount = a.TransactionDetails.Sum( d => (decimal?)d.Amount )
                } ).ToList();

                if ( contributionList.Any() )
                {
                    var contributionResult = new
                    {
                        Count = contributionList.Count(),
                        LastDateTime = contributionList.Max( a => a.TransactionDateTime ),
                        TotalAmount = contributionList.Sum( a => a.ContributionAmount ),
                    };

                    mergeFields.Add( "Contributions", contributionResult );
                }

                string output = lavaTemplate.ResolveMergeFields( mergeFields );

                writer.Write( output );
            }
        }
    }
}
