// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

    [LinkedPage( "Connection Request Detail" )]
    public partial class ConnectionRequests : Rock.Web.UI.PersonBlock
    {
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
        /// Binds the data.
        /// </summary>
        private void BindData()
        {
            if ( Person != null && Person.Id > 0 )
            {
                var rockContext = new RockContext();
                var connectionTypeService = new ConnectionTypeService( rockContext );
                var connectionTypesQry = connectionTypeService.Queryable().Where( a => a.ConnectionOpportunities.Any( b => b.ConnectionRequests.Any( r => r.PersonAlias.PersonId == this.Person.Id) ) ).OrderBy( a => a.Name );

                var connectionTypesList = connectionTypesQry.AsNoTracking().ToList();

                rConnectionTypes.DataSource = connectionTypesList.Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, this.CurrentPerson ) ).ToList();
                rConnectionTypes.DataBind();
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
                var pageGuid = this.GetAttributeValue( "ConnectionRequestDetail" ).AsGuidOrNull();
                PageReference connectionRequestDetailPage = null;
                if ( pageGuid.HasValue )
                {
                    connectionRequestDetailPage = new PageReference( pageGuid.Value.ToString() );
                }

                if ( connectionType != null && this.Person != null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        int personId = this.Person.Id;
                        var connectionRequestService = new ConnectionRequestService( rockContext );
                        var connectionRequestList = connectionRequestService.Queryable().Where( a => a.PersonAlias.PersonId == personId && a.ConnectionOpportunity.ConnectionTypeId == connectionType.Id ).OrderBy( a => a.ConnectionOpportunity.Name ).AsNoTracking().ToList();
                        string listHtml = string.Empty;
                        foreach ( var connectionRequest in connectionRequestList )
                        {
                            string connectionNameHtml;
                            string connectionName;
                            if ( connectionRequest.CampusId.HasValue )
                            {
                                connectionName = string.Format( "{0} ({1})", connectionRequest.ConnectionOpportunity, CampusCache.Read( connectionRequest.CampusId.Value ) );
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
                                connectionRequest.ConnectionState == ConnectionState.Connected ? "class='is-inactive'" : string.Empty,
                                connectionNameHtml,
                                connectionRequest.ConnectionState == ConnectionState.Connected ? "Connected" : connectionRequest.ConnectionStatus.ToString() );
                        }

                        lConnectionOpportunityList.Text = listHtml;
                    }
                }
            }
        }
    }
}