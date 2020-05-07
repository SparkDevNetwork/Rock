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
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace RockWeb.Blocks.Connection
{
    /// <summary>
    /// Block to display the connection types.
    /// </summary>
    [DisplayName( "Connection Type List" )]
    [Category( "Connection" )]
    [Description( "Block to display the connection types." )]

    #region Block Attributes
    [LinkedPage(
        "Detail Page",
        Description = "Page used to view details of a connection type.",
        Key = AttributeKey.DetailPage )]
    #endregion Block Attributes

    public partial class ConnectionTypeList : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string ConnectionTypeId = "ConnectionTypeId";
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
            lbAddConnectionType.Visible = UserCanAdministrate;
            rptConnectionTypes.ItemCommand += rptConnectionTypes_ItemCommand;
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
                GetData();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            GetData();
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptConnectionTypes control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptConnectionTypes_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int? connectionTypeId = e.CommandArgument.ToString().AsIntegerOrNull();
            if ( connectionTypeId.HasValue )
            {
                NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.ConnectionTypeId, connectionTypeId.Value );
            }

            GetData();
        }

        /// <summary>
        /// Handles the Click event of the lbAddConnectionType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddConnectionType_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.ConnectionTypeId, 0 );
        }
        #endregion

        #region Methods

        /// <summary>
        /// Gets the data.
        /// </summary>
        private void GetData()
        {
            using ( var rockContext = new RockContext() )
            {
                // Get all of the event calendars
                var allConnectionTypes = new ConnectionTypeService( rockContext ).Queryable()
                    .OrderBy( w => w.Name )
                    .ToList();

                var authorizedConnectionTypes = new List<ConnectionType>();
                foreach ( var connectionType in allConnectionTypes )
                {
                    if ( UserCanEdit || connectionType.IsAuthorized( Authorization.VIEW, CurrentPerson ))
                    {
                        authorizedConnectionTypes.Add( connectionType );
                    }
                }

                rptConnectionTypes.DataSource = authorizedConnectionTypes.ToList();
                rptConnectionTypes.DataBind();
            }
        }

        #endregion
    }
}