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
                            <p><strong>ConnectionRequests, ConnectionOpportunity, DetailPage, CurrentPerson, Context, PageParameter, Campuses</strong>)</p>
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
    #endregion Block Attributes

    public partial class WebConnectionRequestListLava : RockBlock
    {
        #region Default Lava
        private static class Lava
        {
            public const string ConnectionRequests = @"
{% comment %}
   This is the default lava template for the ConnectionOpportunitySelect block

   Available Lava Fields:
       ConnectionRequests
       ConnectionOpportunity
       DetailPage (Detail Page GUID)
       CurrentPerson
       Context
       PageParameter
       Campuses
{% endcomment %}
<style>
    .card:hover {
      transform: scale(1.01);
      box-shadow: 0 10px 20px rgba(0,0,0,.12), 0 4px 8px rgba(0,0,0,.06);
    }

    .person-image-small {
        position: relative;
        box-sizing: border-box;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: 40px;
        height: 40px;
        vertical-align: top;
        background: center/cover #cbd4db;
        border-radius: 50%;
        box-shadow: inset 0 0 0 1px rgba(0,0,0,0.07)
    }
</style>
{% for connectionRequest in ConnectionRequests %}
    <a href='{{ DetailPage | Default:'0' | PageRoute }}?ConnectionRequestId={{ connectionRequest.Id }}&ConnectionOpportunityId={{connectionRequest.ConnectionOpportunityId }}' stretched-link>
        <div class='card mb-2'>
            <div class='card-body'>
                <div class='row pt-2' style='height:60px;'>
                    <div class='col-xs-2 col-md-1 mx-auto'>
                        <img class='person-image-small' src='{{ connectionRequest.ConnectorPersonAlias.Person.PhotoUrl | Default: '/Assets/Images/person-no-photo-unknown.svg'  }}' alt=''>
                    </div>
                    <div class='col-xs-6 col-md-9 pl-md-0 mx-auto'>
                       <strong class='text-color'>{{ connectionRequest.ConnectorPersonAlias.Person.FullName | Default: 'Unassigned' }}</strong>
                       <small class='pl-1 text-muted'>{{ connectionRequest.Campus.Name | Default: 'Main Campus' }}</small>
                       </br>
                       {% assign lastActivity = connectionRequest.ConnectionRequestActivities | Last %}
                       <small class='text-muted'>Last Activity: {{ lastActivity.Note | Default: 'No Activity' | Capitalize  }}
                           {% if lastActivity.CreatedDateTime %}
                               ({{ lastActivity.CreatedDateTime | DaysFromNow }})
                           {% endif %}
                       </small>
                    </div>
                    <div class='col-xs-4 col-md-2 mx-auto text-right'>
                        <small class='text-muted'>{{ connectionRequest.CreatedDateTime | Date:'M/d/yyyy' }}</small>
                    </div>
                </div>
            </div>
        </div>
       </a>
{% endfor %}
";

        }
        #endregion Default Lava

        #region Attribute Keys
        private static class AttributeKey
        {
            public const string RequestTemplate = "RequestTemplate";
            public const string DetailPage = "DetailPage";
            public const string MaxRequestsToShow = "MaxRequestsToShow";
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
            mdOptions.Show();
        }

        protected void mdOptions_SaveClick( object sender, EventArgs e )
        {
            SetConnectionStatesPreference();
            SetBlockUserPreference( UserPreferenceKey.OnlyShowMyConnections, swOnlyShowMyConnections.Checked.ToString(), true );
            _onlyShowMyConnections = swOnlyShowMyConnections.Checked;

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
                SetBlockUserPreference( UserPreferenceKey.ConnectionStates, selectedItemsString );
                LoadConnectionStates();
            }
            else
            {
                DeleteBlockUserPreference( UserPreferenceKey.ConnectionStates );
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

            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionOpportunity = new ConnectionOpportunityService( rockContext ).GetNoTracking( _connectionOpportunityGuid );
                bool hasMore;
                List<ConnectionRequest> requests;

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

                    var qry = connectionRequestService.GetConnectionRequestsQuery( filterOptions );

                    // We currently don't support showing connected connection requests
                    // since that could end up being a massive list for mobile.
                    qry = qry.Where( r => r.ConnectionState != ConnectionState.Connected );

                    // Put all the requests in memory so we can check security and
                    // then get the current set of requests, plus one. The extra is
                    // so that we can tell if there are more to load.

                    requests = qry
                        .ToList()
                        .Where( r => r.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                        .Skip( ( pageNumber * MaxRequestsToShow ) )
                        .Take( MaxRequestsToShow + 1 )
                        .ToList();

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
                    HasMore = hasMore
                };

                divLoadPrevious.Visible = pageNumber != 0;
                divLoadMore.Visible = _currentRequestsViewModel.HasMore;


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
                if(state== ConnectionState.Connected )
                {
                    continue;
                }

                cblStates.Items.Add( new ListItem { Text = state.ToString().SplitCase(), Value = ( ( int ) state ).ToString() } );
            }

            // Get the OnlyShowMyConnections user preference on load
            bool onlyShowMyConnections;
            bool.TryParse( GetBlockUserPreference( UserPreferenceKey.OnlyShowMyConnections ), out onlyShowMyConnections );
            swOnlyShowMyConnections.Checked = onlyShowMyConnections;
            _onlyShowMyConnections = onlyShowMyConnections;

            // Get the ConnectionStates user preference on load
            var connectionStateString = GetBlockUserPreference( UserPreferenceKey.ConnectionStates );
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