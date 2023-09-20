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

using System;
using System.ComponentModel;
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

    [EnumsField(
        "Hide Connection Requests With These States",
        Key = AttributeKey.HideRequestStates,
        Description = "Any of the states you select here will be excluded from the list.",
        EnumSourceType = typeof( ConnectionState ),
        IsRequired = false,
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
    [Rock.SystemGuid.BlockTypeGuid( "39C53B93-C75A-45DE-B9E7-DFA4EE6B7027" )]
    public partial class ConnectionRequests : Rock.Web.UI.PersonBlock
    {
        #region Attribute Keys
        private static class AttributeKey
        {
            public const string HideRequestStates = "HideRequestStates";
            public const string ConnectionRequestDetail = "ConnectionRequestDetail";
            public const string UseConnectionRequestDetailPageFromConnectionType = "UseConnectionRequestDetailPageFromConnectionType";
        }
        #endregion Attribute Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // This event gets fired after block settings are updated. It's nice to repaint the screen if these settings would alter it.
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upPanel );
        }

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

        #endregion

        #region Events

        // Handlers called by the controls on your block.

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindData();
        }

        #endregion

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

                        // LPC MODIFIED CODE
                        if (connectRequest.AssignedGroup != null)
                        {
                            connectionNameHtml = string.Format( "<a href='{0}'>{1}</a><br>[{2}]", connectionRequestDetailPage.BuildUrl(), connectionName, connectRequest.AssignedGroup.Name );
                        }
                        else
                        {
                            connectionNameHtml = string.Format( "<a href='{0}'>{1}</a>", connectionRequestDetailPage.BuildUrl(), connectionName );
                        }
                        // END LPC MODIFIED CODE
                    }
                    else
                    {
                        connectionNameHtml = connectionName;
                    }

                    if ( ShowNewConnectionType( connectRequest.ConnectionType.Id ) )
                    {
                        listHtml += string.Format( "<div class='card-subheader'>{0}</div>", connectRequest.ConnectionType.Name );
                    }

                    // LPC MODIFIED CODE
                    const string evenTemplate = @"
<dl class='m-0 p-0 d-flex text-sm font-medium'>
    <div class='d-flex py-3 pl-3 pr-2 justify-content-between' style='width: 97%;'>
        <dt class='text-gray-900'>{0}</dt>
        <dd class='text-gray-500' style='text-align: right;'>{1}</dd>
    </div>
    <div style='background-color: {2}; width: 3%;' data-toggle='tooltip' data-placement='left' title='{3}'></div>
 </dl>";
                    const string oddTemplate = @"
<dl class='bg-gray-100 m-0 p-0 d-flex text-sm font-medium'>
    <div class='d-flex py-3 pl-3 pr-2 justify-content-between' style='width: 97%;'>
        <dt class='text-gray-900'>{0}</dt>
        <dd class='text-gray-500' style='text-align: right;'>{1}</dd>
    </div>
    <div style='background-color: {2}; width: 3%;' data-toggle='tooltip' data-placement='left' title='{3}'></div>
</dl>
";
                    // END LPC MODIFIED CODE

                    string connectionStatus = connectRequest.ConnectionRequestConnectionState == ConnectionState.Connected ? "Connected" : connectRequest.ConnectionStatus.ToString();

                    // LPC CODE
                    string color = "#d9d9d9";
                    string state = "Inactive";
                    switch ( connectRequest.ConnectionRequestConnectionState )
                    {
                        case ConnectionState.Active:
                            color = "#16c98d";
                            state = "Active";
                            break;
                        case ConnectionState.FutureFollowUp:
                            color = "#ffc870";
                            state = "Future Follow Up";
                            break;
                        case ConnectionState.Connected:
                            color = "#009ce3";
                            state = "Connected";
                            break;
                    }
                    // END LPC CODE

                    if ( e.Item.ItemIndex % 2 == 0 )
                    {
                        // LPC MODIFIED CODE
                        listHtml += string.Format( evenTemplate, connectionNameHtml, connectionStatus, color, state );
                        // END LPC MODIFIED CODE
                    }
                    else
                    {
                        // LPC MODIFIED CODE
                        listHtml += string.Format( oddTemplate, connectionNameHtml, connectionStatus, color, state );
                        // END LPC MODIFIED CODE
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
                var hideRequestStates = GetAttributeValue( AttributeKey.HideRequestStates ).SplitDelimitedValues().ToList().Select( x => Enum.Parse( typeof( ConnectionState ), x ) );
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
                            AssignedGroup = cr.AssignedGroup,  // LPC CODE
                        } )
                        .Where( crvm => crvm.PersonId == Person.Id );

                    // If hiding inactive ConnectionRequest.ConnectionStatus also filter out inactive ConnectionOpportunity instances (ConnectionOpportunity.IsActive).
                    if ( hideRequestStates.Contains( ConnectionState.Inactive ) )
                    {
                        connectionTypesList = connectionTypesList.Where( t => t.IsActive );
                    }

                    rConnectionTypes.DataSource = connectionTypesList
                        .ToList()
                        .Where( crm => !hideRequestStates.Contains( crm.ConnectionState ) )
                        .Where( crm => crm.ConnectionType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                        .Where( crm => crm.ConnectionOpportunity.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                        .OrderBy( a => a.ConnectionType.Order )
                        .ThenBy( a => a.ConnectionType.Name );
                    rConnectionTypes.DataBind();
                }
            }
        }

        private int _currentConnectionTypeId = 0;

        protected bool ShowNewConnectionType(int connectionTypeId )
        {
            if (_currentConnectionTypeId == connectionTypeId )
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

            public Group AssignedGroup { get; set; }   // LPC CODE
        }
    }
}