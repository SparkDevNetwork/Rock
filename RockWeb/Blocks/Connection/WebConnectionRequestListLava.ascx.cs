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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Model.Connection.ConnectionRequest.Options;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Connection
{
    [DisplayName( "Connection Request List" )]
    [Category( "Connection > WebView" )]
    [Description( "Displays the list of connection requests for a single opportunity." )]
    [IconCssClass( "fa fa-list" )]

    #region Block Attributes
    [CodeEditorField( "Lava Template",
        Key = AttributeKey.RequestTemplate,
        Description = @"This Lava template will be used to display the Connection Types.
                         <i>(Note: The Lava will include the following merge fields:
                            <p><strong>ConnectionRequests, ConnectionOpportunity, DetailPage</strong>)</p>
                         </i>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = false,
        DefaultValue = Lava.ConnectionRequests,
        Order = 1 )]

    [LinkedPage(
        "Detail Page",
        Description = "Page to link to when user taps on a connection request. ConnectionRequestGuid is passed in the query string.",
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.Page.CONNECTION_REQUEST_DETAIL,
        Key = AttributeKey.DetailPage,
        Order = 2 )]

    [IntegerField(
        "Max Requests to Show",
        Description = "The maximum number of requests to show in a single load, a Load More button will be visible if there are more requests to show.",
        IsRequired = true,
        DefaultIntegerValue = 50,
        Key = AttributeKey.MaxRequestsToShow,
        Order = 3 )]

    [BooleanField(
        "Update Page Title",
        Description = "Updates the page title with the opportunity name.",
        IsRequired = false,
        DefaultBooleanValue = false,
        Key = AttributeKey.UpdatePageTitle,
        Order = 4)]
    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "E6BAA42C-D799-4189-ABC9-4A8CA1B91D5A" )]
    public partial class WebConnectionRequestListLava : RockBlock
    {
        #region Default Lava
        private static class Lava
        {
            public const string ConnectionRequests = @"
/-
   This is the default lava template for the block

   Available Lava Fields:
       ConnectionRequests
       ConnectionOpportunity
       DetailPage (page GUID)
-/
<style>
    .card:hover {
      transform: scale(1.01);
      box-shadow: 0 10px 20px rgba(0,0,0,.12), 0 4px 8px rgba(0,0,0,.06);
    }
</style>
{% assign count = ConnectionRequests | Size %}
{% if count > 0 %}
    {% for connectionRequest in ConnectionRequests %}
        <div class=""card card-sm mb-2"">
            {% if DetailPage != '' and DetailPage != null %}
            {% capture pageRouteParams %}ConnectionRequestId={{ connectionRequest.Id }}^ConnectionOpportunityId={{connectionRequest.ConnectionOpportunityId }}{% endcapture %}
            <a href=""{{ DetailPage | PageRoute:pageRouteParams }}"" class=""stretched-link""></a>
            {% endif %}
            <div class=""card-body d-flex flex-wrap align-items-center"" style=""min-height:60px;"">
                <img class=""avatar avatar-lg"" src=""{{ connectionRequest.PersonAlias.Person.PhotoUrl }}"" alt="""">
                <div class=""px-3 flex-fill"">
                    <span class=""d-block"">
                        <strong class=""text-color"">{{ connectionRequest.PersonAlias.Person.FullName }}</strong>
                        {% if connectionRequest.Campus != null %}
                        <span class=""pl-1 small text-muted"">{{ connectionRequest.Campus.Name }}</span>
                        {% endif %}
                    </span>
                    {% assign lastActivity = connectionRequest.ConnectionRequestActivities | Last %}
                    <span class=""text-muted small"">Last Activity: {{ lastActivity.ConnectionActivityType.Name | Default:'No Activity' }}
                        {% if lastActivity.CreatedDateTime %}
                            <span title=""{{ lastActivity.CreatedDateTime }}"">({{ lastActivity.CreatedDateTime | HumanizeDateTime }})</span>
                        {% endif %}
                    </span>
                </div>
                <span class=""small text-muted"" title=""Created {{ connectionRequest.CreatedDateTime }}"">{{ connectionRequest.CreatedDateTime | Date:'sd' }}</span>
            </div>
        </div>
    {% endfor %}
{% else %}
    <div class=""alert alert-info"">No connection requests currently available or assigned to you.</div>
{% endif %}";

        }
        #endregion Default Lava

        #region Attribute Keys
        private static class AttributeKey
        {
            public const string RequestTemplate = "RequestTemplate";
            public const string DetailPage = "DetailPage";
            public const string MaxRequestsToShow = "MaxRequestsToShow";
            public const string UpdatePageTitle = "UpdatePageTitle";
        }
        #endregion Attribute Keys

        #region Page PageParameterKeys
        private static class PageParameterKey
        {
            public const string ConnectionOpportunityGuid = "ConnectionOpportunityGuid";
            public const string CurrentPage = "CurrentPage";
        }
        #endregion Page PageParameterKeys

        #region User Preference Keys
        private static class UserPreferenceKey
        {
            public const string OnlyShowMyConnections = "OnlyShowMyConnections";
            public const string ConnectionStates = "ConnectionStates";
        }
        #endregion User Preference Keys

        private static class ViewStateKeys
        {
            public const string GetRequestsViewModel = "GetRequestsViewModel";
        }

        #region Properties
        /// <summary>
        /// Gets the opportunity template.
        /// </summary>
        /// <value>
        /// The opportunity template.
        /// </value>
        protected string RequestTemplate => GetAttributeValue( AttributeKey.RequestTemplate );

        /// <summary>
        /// Gets the detail page unique identifier.
        /// </summary>
        /// <value>
        /// The detail page unique identifier.
        /// </value>
        protected Guid? DetailPageGuid => GetAttributeValue( AttributeKey.DetailPage ).AsGuidOrNull();

        /// <summary>
        /// Gets the maximum number of requests to show per page load.
        /// </summary>
        /// <value>
        /// The maximum number of requests to show per page load.
        /// </value>
        protected int MaxRequestsToShow => GetAttributeValue( AttributeKey.MaxRequestsToShow ).AsIntegerOrNull() ?? 50;
        #endregion Properties

        #region Private Fields
        private bool _onlyShowMyConnections = false;
        private List<ConnectionState> _connectionStates = null;
        private Guid _connectionOpportunityGuid = Guid.Empty;
        private GetRequestsViewModel _currentRequestsViewModel = null;
        #endregion Private Fields

        #region Base Control Events
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            _connectionStates = new List<ConnectionState>();
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            _connectionOpportunityGuid = PageParameter( PageParameterKey.ConnectionOpportunityGuid ).AsGuid();

            if ( !Page.IsPostBack )
            {
                ConfigureSettings();

                LoadConnectionStates();

                // Get the GetConnectionRequests and use the set options
                GetConnectionRequests();
            }
        }
        #endregion Base Control Events

        #region Page Control Events
        protected void lbOptions_Click( object sender, EventArgs e )
        {
            if ( CurrentPerson == null )
            {
                nbWarning.Visible = true;
                swOnlyShowMyConnections.Visible = false;
            }
            mdOptions.Show();
        }

        protected void mdOptions_SaveClick( object sender, EventArgs e )
        {
            SetConnectionStatesPreference();

            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( UserPreferenceKey.OnlyShowMyConnections, swOnlyShowMyConnections.Checked.ToString() );
            preferences.Save();

            _onlyShowMyConnections = swOnlyShowMyConnections.Checked;
            // Assume changes happened, therefore reset the SetConnectionStates() and clear the GetRequestsViewModel so we start over.
            SetConnectionStates();
            ViewState[ViewStateKeys.GetRequestsViewModel] = null;

            GetConnectionRequests();

            mdOptions.Hide();
        }
        protected void lbLoadMode_Click( object sender, EventArgs e )
        {
            GetConnectionRequests();
        }
        protected void lbLoadPrevious_Click( object sender, EventArgs e )
        {
            GetConnectionRequests( true );
        }
        #endregion Page Control Events

        #region Methods
        private Tuple<string, string> GetConnectionOpportunityTitles()
        {
            var connectionOpportunity = new ConnectionOpportunityService( new RockContext() ).GetNoTracking( _connectionOpportunityGuid );
            return new Tuple<string, string>( connectionOpportunity?.Name, connectionOpportunity?.ConnectionType?.Name );
        }
        private IEnumerable<ConnectionState> GetConnectionStates()
        {
            return Enum.GetValues( typeof( ConnectionState ) ) as ConnectionState[];
        }
        private void LoadConnectionStates()
        {
            if ( _connectionStates?.Count() > 0 )
            {
                foreach ( var state in _connectionStates )
                {
                     cblStates.Items.FindByValue( ( ( int ) state ).ToString() ).Selected = true;
                }
            }
        }
        private void SetConnectionStatesPreference()
        {
            var preferences = GetBlockPersonPreferences();
            var selectedItems = new List<ListItem>();
            foreach ( ListItem item in cblStates.Items )
            {
                if ( item.Selected )
                {
                    selectedItems.Add( item );
                }
            }

            if ( selectedItems.Count > 0 )
            {
                var selectedItemsEnumerable = selectedItems?.Select( v => $"{v.Value}^" );
                var selectedItemsString = string.Join( "^", selectedItemsEnumerable.ToArray() );
                preferences.SetValue( UserPreferenceKey.ConnectionStates, selectedItemsString );
                LoadConnectionStates();
            }
            else
            {
                preferences.SetValue( UserPreferenceKey.ConnectionStates, string.Empty );
            }
        }

        /// <summary>
        /// Gets the connection requests view model that can be sent to the client.
        /// </summary>
        private void GetConnectionRequests( bool previous = false )
        {
            if ( ViewState[ViewStateKeys.GetRequestsViewModel] != null )
            {
                _currentRequestsViewModel = ViewState[ViewStateKeys.GetRequestsViewModel] as GetRequestsViewModel;
            }

            int pageNumber = 0;
            if ( !previous )
            {
                pageNumber = _currentRequestsViewModel != null ? _currentRequestsViewModel.PageNumber + 1 : 0;
            }
            else
            {
                pageNumber = _currentRequestsViewModel != null ? _currentRequestsViewModel.PageNumber - 1 : 0;
            }

            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );

                var connectionOpportunity = new ConnectionOpportunityService( rockContext ).GetNoTracking( _connectionOpportunityGuid );

                if ( connectionOpportunity == null )
                {
                    return;
                }

                bool hasMore;
                List<ConnectionRequest> requests;

                // Determine if we should update the page title with the connection opportunity name
                var updatePageTitle = GetAttributeValue( AttributeKey.UpdatePageTitle ).AsBoolean();
                if ( updatePageTitle )
                {
                    RockPage.PageTitle = connectionOpportunity.Name;

                    var pageBreadCrumb = RockPage.PageReference.BreadCrumbs.FirstOrDefault();
                    if ( pageBreadCrumb != null )
                    {
                        pageBreadCrumb.Name = RockPage.PageTitle;
                    }
                }

                if ( _onlyShowMyConnections && CurrentPerson == null )
                {
                    hasMore = false;
                    requests = new List<ConnectionRequest>();
                }
                else
                {
                    var filterOptions = new ConnectionRequestQueryOptions
                    {
                        ConnectionOpportunityGuids = new List<Guid> { _connectionOpportunityGuid },
                    };

                    if ( _connectionStates.Count > 0 )
                    {
                        filterOptions.ConnectionStates = _connectionStates;
                    }
                    else
                    {
                        filterOptions.ConnectionStates = null;
                    }

                    if ( _onlyShowMyConnections )
                    {
                        filterOptions.ConnectorPersonIds = new List<int> { CurrentPerson.Id };
                    }

                    var qry = connectionRequestService.GetConnectionRequestsQuery( filterOptions )
                        .Include( r => r.PersonAlias.Person )
                        .Include( r => r.ConnectionRequestActivities );

                    // We currently don't support showing connected connection requests
                    // since that could end up being a massive list for mobile.
                    qry = qry.Where( r => r.ConnectionState != ConnectionState.Connected );

                    if ( connectionOpportunity.ConnectionType.EnableRequestSecurity )
                    {
                        // Put all the requests in memory so we can check security and
                        // then get the current set of requests, plus one. The extra is
                        // so that we can tell if there are more to load.
                        requests = qry
                            .OrderByDescending( r => r.CreatedDateKey ).ThenByDescending( r => r.Id )
                            .ToList()
                            .Where( r => r.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                            .Skip( ( pageNumber * MaxRequestsToShow ) )
                            .Take( MaxRequestsToShow + 1 )
                            .ToList();
                    }
                    else
                    {
                        requests = qry
                            .OrderByDescending( r => r.CreatedDateKey ).ThenByDescending( r => r.Id )
                            .Skip( ( pageNumber * MaxRequestsToShow ) )
                            .Take( MaxRequestsToShow + 1 )
                            .ToList();
                    }

                    // Determine if we have more requests to show and then properly
                    // limit the requests to the correct amount.
                    hasMore = requests.Count > MaxRequestsToShow;
                    requests = requests.Take( MaxRequestsToShow ).ToList();
                }

                // Process the connection requests with the template.
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                mergeFields.AddOrReplace( "ConnectionRequests", requests );
                mergeFields.AddOrReplace( "DetailPage", DetailPageGuid );

                var content = RequestTemplate
                    .ResolveMergeFields( mergeFields )
                    .ResolveClientIds( upConnectionSelectLava.ClientID );

                if ( connectionOpportunity != null )
                {
                    mergeFields.Add( "ConnectionOpportunity", connectionOpportunity );
                }

                lContent.Text = content;

                _currentRequestsViewModel = new GetRequestsViewModel
                {
                    HasMore = hasMore,
                    PageNumber = pageNumber,
                };

                lbLoadPrevious.Visible = pageNumber != 0;
                lbLoadMore.Visible = _currentRequestsViewModel.HasMore;

                //Store current page information in view state so we can load next data pages
                ViewState[ViewStateKeys.GetRequestsViewModel] = _currentRequestsViewModel;
            }
        }

        private void ConfigureSettings()
        {
            var titles = GetConnectionOpportunityTitles();
            var connectionOpportunityTitle = titles.Item1;
            var connectionTypeTitle = titles.Item2;
            lTitle.Text = connectionOpportunityTitle;
            lSubTitle.Text = connectionTypeTitle;

            foreach ( var state in GetConnectionStates() )
            {
                if( state == ConnectionState.Connected )
                {
                    continue;
                }

                cblStates.Items.Add( new ListItem { Text = state.ToString().SplitCase(), Value = ( ( int ) state ).ToString() } );
            }

            // Get the OnlyShowMyConnections user preference on load
            var preferences = GetBlockPersonPreferences();
            bool onlyShowMyConnections;
            bool.TryParse( preferences.GetValue( UserPreferenceKey.OnlyShowMyConnections ), out onlyShowMyConnections );
            swOnlyShowMyConnections.Checked = onlyShowMyConnections;
            _onlyShowMyConnections = onlyShowMyConnections;

            // Get the ConnectionStates user preference on load
            SetConnectionStates();
        }

        private void SetConnectionStates()
        {
            var preferences = GetBlockPersonPreferences();
            var connectionStateString = preferences.GetValue( UserPreferenceKey.ConnectionStates );
            if ( !string.IsNullOrEmpty( connectionStateString ) )
            {
                _connectionStates = connectionStateString
                    .SplitDelimitedValues( "^", StringSplitOptions.RemoveEmptyEntries )?
                    .Select( v => ( ConnectionState ) Enum.Parse( typeof( ConnectionState ), v ) )?
                    .ToList();
            }
        }

        #endregion Methods

        #region Support Classes
        /// <summary>
        /// The view model returned by the GetRequests action.
        /// </summary>
        [Serializable]
        public class GetRequestsViewModel
        {
            /// <summary>
            /// Gets or sets a value indicating whether this instance has more connection requests.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance has more connection requests; otherwise, <c>false</c>.
            /// </value>
            public bool HasMore { get; set; }

            /// <summary>
            /// Gets or sets the page number for these results.
            /// </summary>
            /// <value>
            /// The page number for these results.
            /// </value>
            public int PageNumber { get; set; }
        }

        #endregion
    }
}