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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    /// User control for managing the system emails
    /// </summary>
    [DisplayName( "System Email List" )]
    [Category( "Communication" )]
    [Description( "Lists the system emails that can be configured." )]

    [LinkedPage( "Detail Page" )]
    public partial class SystemEmailList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            BindFilter();

            if ( RockPage.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
            {
                gEmailTemplates.DataKeyNames = new string[] { "id" };
                gEmailTemplates.Actions.ShowAdd = true;
                gEmailTemplates.Actions.AddClick += gEmailTemplates_AddClick;
                gEmailTemplates.GridRebind += gEmailTemplates_GridRebind;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbMessage.Visible = false;

            if ( RockPage.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
            {
                if ( !Page.IsPostBack )
                {
                    BindGrid();
                }
            }
            else
            {
                gEmailTemplates.Visible = false;
                nbMessage.Text = WarningMessage.NotAuthorizedToEdit( SystemEmail.FriendlyTypeName );
                nbMessage.Visible = true;
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Category", ddlCategoryFilter.SelectedValue );
            BindGrid();
        }


        /// <summary>
        /// Handles the AddClick event of the gEmailTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gEmailTemplates_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "emailId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gEmailTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gEmailTemplates_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "emailId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gEmailTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gEmailTemplates_Delete( object sender, RowEventArgs e )
        {
            SystemEmailService emailTemplateService = new SystemEmailService();
            SystemEmail emailTemplate = emailTemplateService.Get( (int)gEmailTemplates.DataKeys[e.RowIndex]["id"] );
            if ( emailTemplate != null )
            {
                emailTemplateService.Delete( emailTemplate, CurrentPersonAlias );
                emailTemplateService.Save( emailTemplate, CurrentPersonAlias );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gEmailTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gEmailTemplates_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ddlCategoryFilter.Items.Clear();
            ddlCategoryFilter.Items.Add( new ListItem(All.Text, All.Id.ToString()) );

            SystemEmailService emailTemplateService = new SystemEmailService();
            var items = emailTemplateService.Queryable().
                Where( a => a.Category.Trim() != "" && a.Category != null ).
                OrderBy( a => a.Category ).
                Select( a => a.Category.Trim() ).
                Distinct().ToList();

            foreach ( var item in items )
            {
                ListItem li = new ListItem( item );
                li.Selected = ( !Page.IsPostBack && rFilter.GetUserPreference( "Category" ) == item );
                ddlCategoryFilter.Items.Add( li );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            SystemEmailService emailTemplateService = new SystemEmailService();
            SortProperty sortProperty = gEmailTemplates.SortProperty;

            var emailTemplates = emailTemplateService.Queryable();

            if ( ddlCategoryFilter.SelectedValue != All.Id.ToString() )
            {
                emailTemplates = emailTemplates.Where( a => a.Category.Trim() == ddlCategoryFilter.SelectedValue );
            }

            if ( sortProperty != null )
            {
                gEmailTemplates.DataSource = emailTemplates.Sort( sortProperty ).ToList();
            }
            else
            {
                gEmailTemplates.DataSource = emailTemplates.OrderBy( a => a.Category ).ThenBy( a => a.Title ).ToList();
            }

            gEmailTemplates.DataBind();
        }

        #endregion
    }
}