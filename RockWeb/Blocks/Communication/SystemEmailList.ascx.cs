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
using Rock.Data;
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
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;
            if ( IsUserAuthorized( Authorization.ADMINISTRATE ) )
            {
                gEmailTemplates.DataKeyNames = new string[] { "Id" };
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

            if ( IsUserAuthorized( Authorization.ADMINISTRATE ) )
            {
                if ( !Page.IsPostBack )
                {
                    BindFilter();
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
            int? categoryId = cpCategory.SelectedValueAsInt();
            rFilter.SaveUserPreference( "Category", categoryId.HasValue ? categoryId.Value.ToString() : "");

            BindGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( e.Key == "Category" )
            {
                int? categoryId = e.Value.AsIntegerOrNull();
                if ( categoryId.HasValue )
                {
                    var category = Rock.Web.Cache.CategoryCache.Read( categoryId.Value );
                    if ( category != null )
                    {
                        e.Value = category.Name;
                    }
                }
                else
                {
                    e.Value = string.Empty;
                }
            }
            else
            {
                e.Value = string.Empty;
            }
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
            NavigateToLinkedPage( "DetailPage", "emailId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gEmailTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gEmailTemplates_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            SystemEmailService emailTemplateService = new SystemEmailService( rockContext );
            SystemEmail emailTemplate = emailTemplateService.Get( e.RowKeyId );
            if ( emailTemplate != null )
            {
                emailTemplateService.Delete( emailTemplate );
                rockContext.SaveChanges();
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
            int? categoryId = rFilter.GetUserPreference( "Category" ).AsIntegerOrNull();
            cpCategory.SetValue( categoryId );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var systemEmailService = new SystemEmailService( new RockContext() );
            SortProperty sortProperty = gEmailTemplates.SortProperty;

            var systemEmails = systemEmailService.Queryable( "Category" );

            int? categoryId = rFilter.GetUserPreference( "Category" ).AsIntegerOrNull();
            if ( categoryId.HasValue )
            {
                systemEmails = systemEmails.Where( a => a.CategoryId.HasValue && a.CategoryId.Value == categoryId.Value  );
            }

            if ( sortProperty != null )
            {
                gEmailTemplates.DataSource = systemEmails.Sort( sortProperty ).ToList();
            }
            else
            {
                gEmailTemplates.DataSource = systemEmails.OrderBy( a => a.Category.Name ).ThenBy( a => a.Title ).ToList();
            }

            gEmailTemplates.EntityTypeId = EntityTypeCache.Read<Rock.Model.SystemEmail>().Id;
            gEmailTemplates.DataBind();
        }

        #endregion
    }
}