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
    /// List Prayer Requests using a Lava template.
    /// </summary>
    [DisplayName( "Prayer Request List Lava" )]
    [Category( "Prayer" )]
    [Description( "List Prayer Requests using a Lava template." )]

    [CategoryField( "Category",
        Description = "The category (or parent category) to limit the listed prayer requests to.",
        AllowMultiple = true,
        EntityTypeName = "Rock.Model.PrayerRequest",
        Order = 0,
        Key = AttributeKey.Category )]

    [LinkedPage( "Prayer Request Detail Page",
        Description = "The Page Request Detail Page to use for the LinkUrl merge field.  The LinkUrl field will include a [Id] which can be replaced by the prayerrequestitem.Id.",
        Order = 1,
        Key = AttributeKey.PrayerRequestDetailPage )]

    [CodeEditorField( "Lava Template",
        Description = "Lava template to use to display content",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = @"
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
        Category = "",
        Order = 2,
        Key = AttributeKey.LavaTemplate )]

    [IntegerField( "Max Results",
        Description = "The maximum number of results to display.",
        IsRequired = false,
        DefaultIntegerValue = 100,
        Order = 3,
        Key = AttributeKey.MaxResults )]

    [CustomDropdownListField( "Sort by",
        Description = "",
        ListSource = "0^Entered Date Descending,1^Entered Date Ascending,2^Text",
        IsRequired = false,
        DefaultValue = "0",
        Order = 4,
        Key = AttributeKey.Sortby )]

    [CustomDropdownListField("Approval Status",
        Description = "Which statuses to display.",
        ListSource = "1^Approved,2^Unapproved,3^All",
        IsRequired = true,
        DefaultValue = "1",
        Order = 5,
        Key = AttributeKey.ApprovalStatus )]

    [BooleanField( "Show Expired",
        Description = "Includes expired prayer requests.",
        DefaultBooleanValue = false,
        Order = 6,
        Key = AttributeKey.ShowExpired )]

    [SlidingDateRangeField( "Date Range",
        Description = "Date range to limit by.",
        IsRequired = false,
        DefaultValue = "",
        EnabledSlidingDateRangeTypes = "Last,Previous,Current",
        Order = 7,
        Key = AttributeKey.DateRange )]

    [Rock.SystemGuid.BlockTypeGuid( "AF0B20C3-B969-4246-81CD-76CC443CFDEB" )]
    public partial class PrayerRequestListLava : Rock.Web.UI.RockBlock
    {
        #region Keys

        private static class AttributeKey
        {
            public const string Category = "Category";
            public const string PrayerRequestDetailPage = "PrayerRequestDetailPage";
            public const string LavaTemplate = "LavaTemplate";
            public const string MaxResults = "MaxResults";
            public const string Sortby = "Sortby";
            public const string ApprovalStatus = "ApprovalStatus";
            public const string ShowExpired = "ShowExpired";
            public const string DateRange = "DateRange";
        }

        #endregion

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
            if ( !Page.IsPostBack )
            {
                LoadContent();
            }

            base.OnLoad( e );
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
            if ( !GetAttributeValue( AttributeKey.ShowExpired ).AsBoolean() )
            {
                qryPrayerRequests = qryPrayerRequests.Where( r => r.ExpirationDate >= RockDateTime.Now );
            }

            // filter by date range
            var requestDateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( GetAttributeValue( AttributeKey.DateRange ) ?? "-1||" );

            if ( requestDateRange.Start != null && requestDateRange.End != null )
            {
                qryPrayerRequests = qryPrayerRequests.Where( r => r.EnteredDateTime >= requestDateRange.Start && r.EnteredDateTime <= requestDateRange.End );
            }

            var categoryGuids = ( GetAttributeValue( AttributeKey.Category ) ?? string.Empty ).SplitDelimitedValues().AsGuidList();
            if ( categoryGuids.Any() )
            {
                qryPrayerRequests = qryPrayerRequests.Where( a => a.CategoryId.HasValue && ( categoryGuids.Contains( a.Category.Guid ) || ( a.Category.ParentCategoryId.HasValue && categoryGuids.Contains( a.Category.ParentCategory.Guid ) ) ) );
            }

            // filter by status
            int? statusFilterType = GetAttributeValue( AttributeKey.ApprovalStatus ).AsIntegerOrNull();

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

            int sortBy = GetAttributeValue( AttributeKey.Sortby ).AsInteger();
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

            int? maxResults = GetAttributeValue( AttributeKey.MaxResults ).AsIntegerOrNull();
            if ( maxResults.HasValue && maxResults > 0 )
            {
                qryPrayerRequests = qryPrayerRequests.Take( maxResults.Value );
            }

            mergeFields.Add( "PrayerRequestItems", qryPrayerRequests );

            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "PrayerRequestId", "_PrayerRequestIdParam_" );
            string url = LinkedPageUrl( AttributeKey.PrayerRequestDetailPage, queryParams );
            if ( !string.IsNullOrWhiteSpace( url ) )
            {
                url = url.Replace( "_PrayerRequestIdParam_", "[Id]" );
            }

            mergeFields.Add( "LinkUrl", url );

            string template = GetAttributeValue( AttributeKey.LavaTemplate );
            lContent.Text = template.ResolveMergeFields( mergeFields );
        }

        #endregion
    }
}