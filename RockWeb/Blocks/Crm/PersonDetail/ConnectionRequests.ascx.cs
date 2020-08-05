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
                var connectionType = e.Item.DataItem as ConnectionType;

                if ( connectionType != null && this.Person != null )
                {
                    var pageGuid = this.GetAttributeValue( AttributeKey.ConnectionRequestDetail ).AsGuidOrNull();
                    PageReference connectionRequestDetailPage = null;

                    if ( GetAttributeValue( AttributeKey.UseConnectionRequestDetailPageFromConnectionType ).AsBoolean() &&
                         ( connectionType.ConnectionRequestDetailPageId.HasValue || connectionType.ConnectionRequestDetailPageRouteId.HasValue ) )
                    {
                        connectionRequestDetailPage = new PageReference( connectionType.ConnectionRequestDetailPageId ?? 0, connectionType.ConnectionRequestDetailPageRouteId ?? 0 );
                    }
                    else if( pageGuid.HasValue )
                    {
                        connectionRequestDetailPage = new PageReference( pageGuid.Value.ToString() );
                    }

                    using ( var rockContext = new RockContext() )
                    {
                        string listHtml = string.Empty;

                        int personId = this.Person.Id;
                        var connectionRequestService = new ConnectionRequestService( rockContext );
                        var connectionRequestQuery = connectionRequestService
                            .Queryable()
                            .Where( a => a.PersonAlias.PersonId == personId && a.ConnectionOpportunity.ConnectionTypeId == connectionType.Id );

                        var hideInactive = GetAttributeValue( AttributeKey.HideInactiveConnectionRequests ).AsBoolean();

                        if ( hideInactive )
                        {
                            connectionRequestQuery = connectionRequestQuery
                                .Where( r => r.ConnectionState == ConnectionState.Active ||
                                    ( r.ConnectionState == ConnectionState.FutureFollowUp && r.FollowupDate.HasValue && r.FollowupDate.Value <= _midnightTomorrow ) );
                        }

                        var connectionRequestList = connectionRequestQuery
                            .OrderBy( a => a.ConnectionOpportunity.Name )
                            .AsNoTracking()
                            .ToList();

                        foreach ( var connectionRequest in connectionRequestList )
                        {
                            string connectionNameHtml;
                            string connectionName;
                            if ( connectionRequest.CampusId.HasValue )
                            {

                                connectionName = string.Format( "{0} ({1})", connectionRequest.ConnectionOpportunity, CampusCache.Get( connectionRequest.CampusId.Value ) );
                            }
                            else
                            {
                                connectionName = string.Format( "{0}", connectionRequest.ConnectionOpportunity );
                            }

                            if ( connectionRequestDetailPage != null && connectionRequestDetailPage.PageId > 0 )
                            {
                                connectionRequestDetailPage.Parameters = new System.Collections.Generic.Dictionary<string, string>();
                                connectionRequestDetailPage.Parameters.Add( "ConnectionRequestId", connectionRequest.Id.ToString() );
                                connectionRequestDetailPage.Parameters.Add( "ConnectionOpportunityId", connectionRequest.ConnectionOpportunityId.ToString() );

                                connectionNameHtml = string.Format( "<a href='{0}'>{1}</a>", connectionRequestDetailPage.BuildUrl(), connectionName );
                            }
                            else
                            {
                                connectionNameHtml = connectionName;
                            }

                            listHtml += string.Format(
                                "<li {0}>{1} - <small>{2}</small></li>",
                                ( connectionRequest.ConnectionState == ConnectionState.Connected || connectionRequest.ConnectionState == ConnectionState.Inactive ) ? "class='is-inactive'" : string.Empty,
                                connectionNameHtml,
                                connectionRequest.ConnectionState == ConnectionState.Connected ? "Connected" : connectionRequest.ConnectionStatus.ToString() );
                        }

                        lConnectionOpportunityList.Text = listHtml;
                    }
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
                bool hideInactive = GetAttributeValue( AttributeKey.HideInactiveConnectionRequests ).AsBoolean();
                var rockContext = new RockContext();
                var connectionTypeService = new ConnectionTypeService( rockContext );
                var connectionTypesQry = connectionTypeService
                    .Queryable();

                if ( hideInactive )
                {
                    connectionTypesQry = connectionTypesQry
                        .Where( t => t.ConnectionOpportunities.Any( o => o.IsActive == true ) )
                        .Where( t => t.ConnectionOpportunities
                        .Any( o => o.ConnectionRequests
                            .Any( r => r.PersonAlias.PersonId == Person.Id && ( r.ConnectionState == ConnectionState.Active ||
                                ( r.ConnectionState == ConnectionState.FutureFollowUp && r.FollowupDate.HasValue && r.FollowupDate.Value <= _midnightTomorrow ) ) ) ) );
                }
                else
                {
                    connectionTypesQry = connectionTypesQry.Where( t => t.ConnectionOpportunities.Any( o => o.ConnectionRequests.Any( r => r.PersonAlias.PersonId == Person.Id ) ) );
                }

                var connectionTypesList = connectionTypesQry.OrderBy( a => a.Name ).AsNoTracking().ToList();

                rConnectionTypes.DataSource = connectionTypesList.Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, this.CurrentPerson ) ).ToList();
                rConnectionTypes.DataBind();
            }
        }

        #endregion Methods
    }
}