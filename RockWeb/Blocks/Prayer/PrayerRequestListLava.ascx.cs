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
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Prayer
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Prayer Request List Lava" )]
    [Category( "Prayer" )]
    [Description( "List Prayer Requests using a Lava template." )]
    
    [CategoryField( "Category", "The category (or parent category) to limit the listed prayer requests to.", true, "Rock.Model.PrayerRequest", order: 0 )]
    [LinkedPage( "Prayer Request Detail Page", "The Page Request Detail Page to use for the LinkUrl merge field.  The LinkUrl field will include a [Id] which can be replaced by the prayerrequestitem.Id.", order: 1 )]
    [CodeEditorField( "Lava Template", "Lava template to use to display content", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"
<div class='panel panel-block'> 
    <div class='panel-heading'>
       <h4 class='panel-title'>Prayer Requests</h4>
    </div>
    <div class='panel-body'>

        <ul>
        {% for prayerrequestitem in PrayerRequestItems %}
            {% if LinkUrl != '' %}
                <li>{{ prayerrequestitem.EnteredDateTime | Date:'M/d/yyyy'}} - <a href='{{ LinkUrl | Replace:'[Id]',prayerrequestitem.Id }}'>{{ prayerrequestitem.Text }}</a></li>
            {% else %}
                <li>{{ prayerrequestitem.EnteredDateTime | Date:'M/d/yyyy'}} - {{ prayerrequestitem.Text }}</li>
            {% endif %}
        {% endfor %}
        </ul>
        
    </div>
</div>",
       "", 2, "LavaTemplate" )]
    
    [IntegerField( "Max Results", "The maximum number of results to display.", false, 100, order: 3 )]
    [CustomDropdownListField( "Sort by", "", "0^Entered Date Descending,1^Entered Date Ascending,2^Text", false, "0", order: 4 )]
    [CustomDropdownListField("Approval Status", "Which statuses to display.", "1^Approved,2^Unapproved,3^All", true, "1", order: 5)]
    [BooleanField( "Show Expired", "Includes expired prayer requests.", false, order: 6)]
    [SlidingDateRangeField( "Date Range", "Date range to limit by.", false, "", enabledSlidingDateRangeTypes: "Last,Previous,Current", order: 7 )]
    public partial class PrayerRequestListLava : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                LoadContent();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadContent();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the content.
        /// </summary>
        protected void LoadContent()
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            
            RockContext rockContext = new RockContext();

            var prayerRequestService = new PrayerRequestService( rockContext );
            var qryPrayerRequests = prayerRequestService.Queryable();

            // filter out expired
            if ( !GetAttributeValue( "Show Expired" ).AsBoolean() )
            {
                qryPrayerRequests = qryPrayerRequests.Where( r => r.ExpirationDate >= RockDateTime.Now );
            }

            // filter by date range
            var requestDateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( GetAttributeValue( "DateRange" ) ?? "-1||" );

            if ( requestDateRange.Start != null && requestDateRange.End != null )
            {
                qryPrayerRequests = qryPrayerRequests.Where( r => r.EnteredDateTime >= requestDateRange.Start && r.EnteredDateTime <= requestDateRange.End );
            }

            var categoryGuids = ( GetAttributeValue( "Category" ) ?? string.Empty ).SplitDelimitedValues().AsGuidList();
            if ( categoryGuids.Any() )
            {
                qryPrayerRequests = qryPrayerRequests.Where( a => a.CategoryId.HasValue && ( categoryGuids.Contains( a.Category.Guid ) || ( a.Category.ParentCategoryId.HasValue && categoryGuids.Contains( a.Category.ParentCategory.Guid ) ) ) );
            }

            // filter by status
            int? statusFilterType = GetAttributeValue( "ApprovalStatus" ).AsIntegerOrNull();

            if ( statusFilterType.HasValue )
            {
                switch ( statusFilterType.Value )
                {
                    case 1: 
                        {
                            qryPrayerRequests = qryPrayerRequests.Where( a => a.IsApproved == true );
                            break;
                        }
                    case 2:
                        {
                            qryPrayerRequests = qryPrayerRequests.Where( a => a.IsApproved == false );
                            break;
                        }
                }
            }


            int sortBy = GetAttributeValue( "Sortby" ).AsInteger();
            switch ( sortBy )
            {
                case 0: qryPrayerRequests = qryPrayerRequests.OrderBy( a => a.EnteredDateTime );
                    break;

                case 1: qryPrayerRequests = qryPrayerRequests.OrderByDescending( a => a.EnteredDateTime );
                    break;

                case 2: qryPrayerRequests = qryPrayerRequests.OrderBy( a => a.Text );
                    break;

                default: qryPrayerRequests = qryPrayerRequests.OrderBy( a => a.EnteredDateTime );
                    break;
            }

            int? maxResults = GetAttributeValue( "MaxResults" ).AsIntegerOrNull();
            if ( maxResults.HasValue && maxResults > 0 )
            {
                qryPrayerRequests = qryPrayerRequests.Take( maxResults.Value );
            }

            mergeFields.Add( "PrayerRequestItems", qryPrayerRequests );

            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "PrayerRequestId", "_PrayerRequestIdParam_" );
            string url = LinkedPageUrl( "PrayerRequestDetailPage", queryParams );
            if ( !string.IsNullOrWhiteSpace( url ) )
            {
                url = url.Replace( "_PrayerRequestIdParam_", "[Id]" );
            }

            mergeFields.Add( "LinkUrl", url );

            string template = GetAttributeValue( "LavaTemplate" );
            lContent.Text = template.ResolveMergeFields( mergeFields );

        }

        #endregion
    }
}