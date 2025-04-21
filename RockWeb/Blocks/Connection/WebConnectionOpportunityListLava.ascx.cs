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
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.ClientService.Connection.ConnectionOpportunity;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Model.Connection.ConnectionOpportunity.Options;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Connection
{
    [DisplayName( "Connection Opportunity List" )]
    [Category( "Connection > WebView" )]
    [Description( "Displays the list of connection opportunities for a single connection type." )]
    [IconCssClass( "fa fa-list" )]

    #region Block Attributes
    [CodeEditorField( "Opportunity Template",
        Key = AttributeKey.OpportunityTemplate,
        Description = @"This Lava template will be used to display the Connection Types.
                         <i>(Note: The Lava will include the following merge fields:
                            <p><strong>ConnectionOpportunities, DetailPage, ConnectionRequestCounts, SumTotalConnectionRequests</strong>)</p>
                         </i>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = false,
        DefaultValue = Lava.ConnectionOpportunities,
        Order = 1 )]

    [LinkedPage( "Detail Page", Description = "Page to link to when user taps on a connection opportunity. ConnectionOpportunityGuid is passed in the query string.",
        Order = 2,
        Key = AttributeKey.DetailPage
         )]

    [BooleanField(
        "Update Page Title",
        Description = "Updates the page title with the connection type name.",
        IsRequired = false,
        DefaultBooleanValue = false,
        Key = AttributeKey.UpdatePageTitle,
        Order = 3 )]
    #endregion

    [Rock.SystemGuid.BlockTypeGuid( "B2E0E4E3-30B1-45BD-B808-C55BCD540894" )]
    public partial class WebConnectionOpportunityListLava : RockBlock
    {
        #region Default Lava
        private static class Lava
        {
            public const string ConnectionOpportunities = @"
/-
   This is the default lava template for the block

   Available Lava Fields:
       ConnectionOpportunities
       DetailPage (page GUID)
       ConnectionRequestCounts (a dictionary with key of Opportunity Id and value is the count; where count is either the total count or count of requests assigned to the individual)
       SumTotalConnectionRequests (a sum total of all the counts from that dictionary)
-/
<style>
    .card:hover {
      transform: scale(1.01);
      box-shadow: 0 10px 20px rgba(0,0,0,.12), 0 4px 8px rgba(0,0,0,.06);
    }
</style>

{% if SumTotalConnectionRequests > 0 %}
    {% for connectionOpportunity in ConnectionOpportunities %}
        {% assign opportunityId = connectionOpportunity.Id | ToString %}
        {% assign count = ConnectionRequestCounts[opportunityId] | AsInteger %}
        {% if count > 0 %}
            <div class=""card card-sm mb-2"">
                {% if DetailPage != '' and DetailPage != null %}
                {% capture pageRouteParams %}ConnectionOpportunityGuid={{ connectionOpportunity.Guid }}{% endcapture %}
                <a href=""{{ DetailPage | PageRoute:pageRouteParams }}"" class=""stretched-link""></a>
                {% endif %}
                <div class=""card-body d-flex flex-wrap align-items-center"" style=""min-height:60px;"">
                    <i class=""{{ connectionOpportunity.IconCssClass }} fa-2x text-muted""></i>
                    <div class=""px-3 flex-fill"">
                        <span class=""d-block text-color""><strong>{{ connectionOpportunity.Name }}</strong></span>
                        <span class=""text-muted text-sm"">{{ connectionOpportunity.Summary | Truncate:100,'...' }}</span>
                    </div>
                    <span class=""badge badge-pill badge-info small"">{{ count }}</span>
                </div>
            </div>
        {% endif %}
    {% endfor %}
{% else %}
    <div class=""alert alert-info"">No connection requests currently available or assigned to you.</div>
{% endif %}";
        }
        #endregion Lava

        #region Attribute Keys
        private static class AttributeKey
        {
            public const string OpportunityTemplate = "OpportunityTemplate";
            public const string DetailPage = "DetailPage";
            public const string UpdatePageTitle = "UpdatePageTitle";
        }
        #endregion

        #region Page PageParameterKeys
        private static class PageParameterKey
        {
            public const string ConnectionTypeGuid = "ConnectionTypeGuid";
        }

        #region User Preference Keys
        private static class UserPreferenceKey
        {
            public const string OnlyShowOpportunitiesWithRequestsForUser = "OnlyShowOpportunitiesWithRequestsForUser";
        }
        #endregion

        #endregion Page PageParameterKeys

        #region Properties
        /// <summary>
        /// Gets the opportunity template.
        /// </summary>
        /// <value>
        /// The opportunity template.
        /// </value>
        protected string OpportunityTemplate => GetAttributeValue( AttributeKey.OpportunityTemplate );

        /// <summary>
        /// Gets the detail page unique identifier.
        /// </summary>
        /// <value>
        /// The detail page unique identifier.
        /// </value>
        protected Guid? DetailPageGuid => GetAttributeValue( AttributeKey.DetailPage ).AsGuidOrNull();
        #endregion

        #region Private Fields
        private bool _onlyShowOpportunitiesWithRequestsForUser = false;
        private Guid _connectionTypeGuid = Guid.Empty;
        #endregion

        #region Base Control Events
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            _connectionTypeGuid = PageParameter( PageParameterKey.ConnectionTypeGuid ).AsGuid();

            if ( !Page.IsPostBack )
            {
                lTitle.Text = $"<h2>{GetConnectionTypeTitle()}</h2>";
                var preferences = GetBlockPersonPreferences();
                bool onlyShowOpportunitiesWithRequestsForUserPref;
                bool.TryParse( preferences.GetValue( UserPreferenceKey.OnlyShowOpportunitiesWithRequestsForUser ), out onlyShowOpportunitiesWithRequestsForUserPref );
                swOnlyShowOpportunitiesWithRequestsForUser.Checked = onlyShowOpportunitiesWithRequestsForUserPref;
                _onlyShowOpportunitiesWithRequestsForUser = onlyShowOpportunitiesWithRequestsForUserPref;

                GetConnectionOpportunities();
            }

            base.OnLoad( e );
        }
        #endregion Base Control Events

        #region Page Control Events
        protected void lbOptions_Click( object sender, EventArgs e )
        {
            if ( CurrentPerson == null )
            {
                nbWarning.Visible = true;
                swOnlyShowOpportunitiesWithRequestsForUser.Visible = false;
            }

            mdOptions.Show();
        }

        protected void mdOptions_SaveClick( object sender, EventArgs e )
        {
            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( UserPreferenceKey.OnlyShowOpportunitiesWithRequestsForUser, swOnlyShowOpportunitiesWithRequestsForUser.Checked.ToString() );
            preferences.Save();

            _onlyShowOpportunitiesWithRequestsForUser = swOnlyShowOpportunitiesWithRequestsForUser.Checked;

            GetConnectionOpportunities();

            mdOptions.Hide();
        }
        #endregion Page Control Events

        #region Methods
        private string GetConnectionTypeTitle()
        {
            var connectionType = new ConnectionTypeService( new RockContext() ).GetNoTracking( _connectionTypeGuid );
            return connectionType?.Name;
        }
        /// <summary>
        /// Gets the connection types view model that can be sent to the client.
        /// </summary>
        private void GetConnectionOpportunities()
        {
            using ( var rockContext = new RockContext() )
            {
                var opportunityService = new ConnectionOpportunityService( rockContext );

                var opportunityClientService = new ConnectionOpportunityClientService( rockContext, CurrentPerson );
                var connectionType = new ConnectionTypeService( rockContext ).GetNoTracking( _connectionTypeGuid );

                if ( connectionType == null )
                {
                    return;
                }

                // Determine if we should update the page title with the connection opportunity name
                var updatePageTitle = GetAttributeValue( AttributeKey.UpdatePageTitle ).AsBoolean();
                if ( updatePageTitle )
                {
                    RockPage.PageTitle = connectionType.Name;

                    var pageBreadCrumb = RockPage.PageReference.BreadCrumbs.FirstOrDefault();
                    if ( pageBreadCrumb != null )
                    {
                        pageBreadCrumb.Name = RockPage.PageTitle;
                    }
                }

                var filterOptions = new ConnectionOpportunityQueryOptions
                {
                    ConnectionTypeGuids = new List<Guid> { _connectionTypeGuid },
                    IncludeInactive = true
                };

                if ( _onlyShowOpportunitiesWithRequestsForUser && CurrentPerson != null )
                {
                    filterOptions.ConnectorPersonIds = new List<int> { CurrentPerson.Id };
                }
                else
                {
                    filterOptions.ConnectorPersonIds = null;
                }

                // Put all the opportunities in memory so we can check security.
                var connectionOpportunityQuery = opportunityService.GetConnectionOpportunitiesQuery( filterOptions );
                var opportunities = connectionOpportunityQuery.ToList()
                    .Where( o => o.IsAuthorized( Authorization.VIEW, CurrentPerson ) );

                // Get the various counts to make available to the Lava template.
                // The conversion of the value to a dictionary is a temporary work-around
                // until we have a way to mark external types as lava safe.
                var opportunityIds = opportunities.Select( o => o.Id ).ToList();
                var connectionRequestCounts = new Dictionary<string, string>();
                var sumTotalConnectionRequests = 0;

                // We only need to perform this query under this condition, otherwise we will use the
                // requestCountsPerOpportunity fetched earlier.
                if ( _onlyShowOpportunitiesWithRequestsForUser && CurrentPerson != null )
                {
                    var requestCounts = opportunityClientService.GetOpportunityRequestCounts( opportunityIds );
                    foreach ( var opportunityId in opportunityIds )
                    {
                        // Note use AssignedToYouCount here:
                        sumTotalConnectionRequests += requestCounts[opportunityId].AssignedToYouCount;
                        connectionRequestCounts.Add( opportunityId.ToString(), requestCounts[opportunityId].AssignedToYouCount.ToString() );
                    }
                }
                else
                {
                    var requestCounts = opportunityClientService.GetOpportunityRequestCounts( opportunityIds );
                    foreach ( var opportunityId in opportunityIds )
                    {
                        // Note use TotalCount here:
                        sumTotalConnectionRequests += requestCounts[opportunityId].TotalCount;
                        connectionRequestCounts.Add( opportunityId.ToString(), requestCounts[opportunityId].TotalCount.ToString() );
                    }
                }
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );

                mergeFields.AddOrReplace( "ConnectionOpportunities", opportunities );
                mergeFields.AddOrReplace( "DetailPage", DetailPageGuid );
                mergeFields.AddOrReplace( "ConnectionRequestCounts", connectionRequestCounts );
                mergeFields.AddOrReplace( "SumTotalConnectionRequests", sumTotalConnectionRequests );

                var content = OpportunityTemplate
                    .ResolveMergeFields( mergeFields )
                    .ResolveClientIds( upConnectionSelectLava.ClientID );

                lContent.Text = content;
            }
        }
        #endregion Methods
    }
}