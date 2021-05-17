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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Connection Requests" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Allows you to view connection requests of a particular person." )]

    [BooleanField(
        "Hide Inactive Connection Requests",
        Key = AttributeKey.HideInactiveConnectionRequests,
        Description = "Show only connection requests that are active?",
        DefaultBooleanValue = false,
        Order = 0 )]

    [LinkedPage(
        "Connection Request Detail",
        Key = AttributeKey.ConnectionRequestDetail,
        Description = "The Connection Request Detail page.",
        Order = 1 )]
    [BooleanField(
        "Use Connection Request Detail Page From Connection Type",
        Key = AttributeKey.UseConnectionRequestDetailPageFromConnectionType,
        Description = "If enabled, the Connection Request Detail page defined by the Connection Type will be used to view the request(if it's not empty/unset). Otherwise the Connection Request Detail page configured on this block will be used.",
        DefaultBooleanValue = true,
        Order = 2 )]
    public partial class ConnectionRequests : Rock.Web.UI.PersonBlock
    {
        #region Attribute Keys
        private static class AttributeKey
        {
            public const string HideInactiveConnectionRequests = "HideInactive";
            public const string ConnectionRequestDetail = "ConnectionRequestDetail";
            public const string UseConnectionRequestDetailPageFromConnectionType = "UseConnectionRequestDetailPageFromConnectionType";
        }
        #endregion Attribute Keys

        DateTime _midnightTomorrow = RockDateTime.Today.AddDays( 1 );

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( Person != null && Person.Id != 0 )
            {
                upPanel.Visible = true;
                if ( !Page.IsPostBack )
                {
                    BindData();
                }
            }
            else
            {
                upPanel.Visible = false;
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rConnectionTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rConnectionTypes_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var lConnectionOpportunityList = e.Item.FindControl( "lConnectionOpportunityList" ) as Literal;
            if ( lConnectionOpportunityList != null )
            {
                var connectRequest = e.Item.DataItem as ConnectionRequestViewModel;

                if ( connectRequest != null && this.Person != null )
                {
                    var pageGuid = this.GetAttributeValue( AttributeKey.ConnectionRequestDetail ).AsGuidOrNull();
                    PageReference connectionRequestDetailPage = null;

                    if ( GetAttributeValue( AttributeKey.UseConnectionRequestDetailPageFromConnectionType ).AsBoolean() &&
                         ( connectRequest.ConnectionRequestDetailPageId.HasValue || connectRequest.ConnectionRequestDetailPageRouteId.HasValue ) )
                    {
                        connectionRequestDetailPage = new PageReference( connectRequest.ConnectionRequestDetailPageId ?? 0, connectRequest.ConnectionRequestDetailPageRouteId ?? 0 );
                    }
                    else if ( pageGuid.HasValue )
                    {
                        connectionRequestDetailPage = new PageReference( pageGuid.Value.ToString() );
                    }

                    string listHtml = string.Empty;
                    string connectionNameHtml;
                    string connectionName;

                    if ( connectRequest.CampusId.HasValue )
                    {

                        connectionName = string.Format( "{0} ({1})", connectRequest.ConnectionOpportunity, CampusCache.Get( connectRequest.CampusId.Value ) );
                    }
                    else
                    {
                        connectionName = string.Format( "{0}", connectRequest.ConnectionOpportunity );
                    }

                    if ( connectionRequestDetailPage != null && connectionRequestDetailPage.PageId > 0 )
                    {
                        connectionRequestDetailPage.Parameters = new System.Collections.Generic.Dictionary<string, string>();
                        connectionRequestDetailPage.Parameters.Add( "ConnectionRequestId", connectRequest.ConnectionRequestId.ToString() );
                        connectionRequestDetailPage.Parameters.Add( "ConnectionOpportunityId", connectRequest.ConnectionOpportunity.Id.ToString() );

                        connectionNameHtml = string.Format( "<a href='{0}'>{1}</a>", connectionRequestDetailPage.BuildUrl(), connectionName );
                    }
                    else
                    {
                        connectionNameHtml = connectionName;
                    }

                    listHtml += string.Format(
                        "<li {0}>{1} - <small>{2}</small></li>",
                        ( connectRequest.ConnectionRequestConnectionState == ConnectionState.Connected || connectRequest.ConnectionRequestConnectionState == ConnectionState.Inactive ) ? "class='is-inactive'" : string.Empty,
                        connectionNameHtml,
                        connectRequest.ConnectionRequestConnectionState == ConnectionState.Connected ? "Connected" : connectRequest.ConnectionStatus.ToString() );

                    var isFirstRow = _currentConnectionTypeId == 0;
                    if ( ShowNewConnectionType( connectRequest.ConnectionType.Id ) )
                    {
                        var headerHtml = @"<li>
                                <span class=""control-label"">" + connectRequest.ConnectionType.Name + @"</span>
                                  <ul class=""connectionopportunity-list list-unstyled margin-l-md"">";

                        if ( !isFirstRow )
                        {
                            // Close previous connection type.
                            headerHtml = "</ul></li>" + headerHtml;
                        }
                        listHtml = headerHtml + listHtml;
                    }
                    lConnectionOpportunityList.Text = listHtml;
                }
            }
        }

        #region Internal Methods

        /// <summary>
        /// Binds the data.
        /// </summary>
        private void BindData()
        {
            if ( Person != null && Person.Id > 0 )
            {
                var hideInactive = GetAttributeValue( AttributeKey.HideInactiveConnectionRequests ).AsBoolean();
                using ( var rockContext = new RockContext() )
                {
                    var connectionTypeService = new ConnectionTypeService( rockContext );
                    var connectionTypesQry = connectionTypeService.Queryable();

                    var connectionTypesList = connectionTypesQry
                        .SelectMany( ct => ct.ConnectionOpportunities )
                        .SelectMany( co => co.ConnectionRequests )
                        .Select( cr => new ConnectionRequestViewModel
                        {
                            ConnectionType = cr.ConnectionOpportunity.ConnectionType,
                            CampusId = cr.CampusId,
                            ConnectionRequestId = cr.Id,
                            ConnectionRequestConnectionState = cr.ConnectionState,
                            ConnectionOpportunity = cr.ConnectionOpportunity,
                            ConnectionRequestDetailPageId = cr.ConnectionOpportunity.ConnectionType.ConnectionRequestDetailPageId,
                            ConnectionRequestDetailPageRouteId = cr.ConnectionOpportunity.ConnectionType.ConnectionRequestDetailPageRouteId,
                            ConnectionStatus = cr.ConnectionStatus,
                            ConnectionState = cr.ConnectionState,
                            PersonId = cr.PersonAlias.PersonId,
                            IsActive = cr.ConnectionOpportunity.IsActive,
                            FollowupDate = cr.FollowupDate,
                        } )
                        .Where( crvm => crvm.PersonId == Person.Id );

                    if ( hideInactive )
                    {
                        connectionTypesList = connectionTypesList
                            .Where( t => t.IsActive )
                            .Where( t => t.ConnectionState == ConnectionState.Active ||
                                            ( t.ConnectionState == ConnectionState.FutureFollowUp && t.FollowupDate.HasValue && t.FollowupDate.Value <= _midnightTomorrow ) );
                    }

                    rConnectionTypes.DataSource = connectionTypesList
                        .ToList()
                        .Where( crm => crm.ConnectionType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                        .Where( crm => crm.ConnectionOpportunity.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                        .OrderBy( a => a.ConnectionType.Name );
                    rConnectionTypes.DataBind();
                }
            }
        }

        private int _currentConnectionTypeId = 0;
        protected bool ShowNewConnectionType(int connectionTypeId )
        {
            if(_currentConnectionTypeId == connectionTypeId )
            {
                return false;
            }
            _currentConnectionTypeId = connectionTypeId;
            return true;
        }
        #endregion Methods

        protected class ConnectionRequestViewModel
        {
            public int? CampusId { get; set; }
            public int ConnectionRequestId { get; set; }
            public ConnectionState ConnectionRequestConnectionState { get; set; }
            public ConnectionOpportunity ConnectionOpportunity { get; set; }
            public int? ConnectionRequestDetailPageId { get; set; }
            public int? ConnectionRequestDetailPageRouteId { get; set; }
            public ConnectionStatus ConnectionStatus { get; set; }
            public int PersonId { get; set; }
            public bool IsActive { get; set; }
            public ConnectionState ConnectionState { get; set; }
            public DateTime? FollowupDate { get; set; }
            public ConnectionType ConnectionType { get; set; }
        }
    }
}