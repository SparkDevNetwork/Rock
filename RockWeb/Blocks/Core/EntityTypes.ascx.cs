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
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Security;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Entity Types" )]
    [Category( "Core" )]
    [Description( "Administer the IEntity entity types." )]
    public partial class EntityTypes : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( IsUserAuthorized( Authorization.ADMINISTRATE ) )
            {
                gEntityTypes.DataKeyNames = new string[] { "Id" };
                gEntityTypes.Actions.ShowAdd = false;
                gEntityTypes.Actions.AddClick += Actions_AddClick;
                gEntityTypes.RowSelected += gEntityTypes_EditRow;
                gEntityTypes.GridRebind += gEntityTypes_GridRebind;
                gEntityTypes.RowDataBound += gEntityTypes_RowDataBound;

                mdEdit.SaveClick += mdEdit_SaveClick;
            }

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbWarning.Visible = false;

            if ( IsUserAuthorized( Authorization.ADMINISTRATE ) )
            {
                if ( !Page.IsPostBack )
                {
                    EntityTypeService.RegisterEntityTypes( Request.MapPath( "~" ) );
                    BindGrid();
                }
                else
                {
                    ShowDialog();
                }

            }
            else
            {
                gEntityTypes.Visible = false;
                nbWarning.Text = WarningMessage.NotAuthorizedToEdit( EntityType.FriendlyTypeName );
                nbWarning.Visible = true;
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events (main grid)

        /// <summary>
        /// Handles the EditRow event of the gEntityTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        void gEntityTypes_EditRow( object sender, RowEventArgs e )
        {
            ShowEdit( e.RowKeyId );
        }

        /// <summary>
        /// Handles the AddClick event of the Actions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void Actions_AddClick( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }


        /// <summary>
        /// Handles the GridRebind event of the gEntityTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gEntityTypes_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gEntityTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs" /> instance containing the event data.</param>
        void gEntityTypes_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            EntityType entityType = e.Row.DataItem as EntityType;
            if ( entityType != null )
            {
                HtmlAnchor aSecure = e.Row.FindControl( "aSecure" ) as HtmlAnchor;
                if ( aSecure != null )
                {
                    if ( entityType.IsSecured )
                    {
                        aSecure.Visible = true;
                        string url = Page.ResolveUrl( string.Format( "~/Secure/{0}/{1}?t={2}&pb=&sb=Done",
                            entityType.Id, 0, entityType.FriendlyName + " Security" ) );
                        aSecure.HRef = "javascript: Rock.controls.modal.show($(this), '" + url + "')";
                    }
                    else
                    {
                        aSecure.Visible = false;
                    }
                }
            }
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the SaveClick event of the mdEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdEdit_SaveClick( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            EntityTypeService entityTypeService = new EntityTypeService( rockContext );
            EntityType entityType = entityTypeService.Get( int.Parse( hfEntityTypeId.Value ) );

            if (entityType == null)
            {
                entityType = new EntityType();
                entityType.IsEntity = true;
                entityType.IsSecured = true;
                entityTypeService.Add( entityType );
            }

            entityType.Name = tbName.Text;
            entityType.FriendlyName = tbFriendlyName.Text;
            entityType.IsCommon = cbCommon.Checked;

            rockContext.SaveChanges();

            hfEntityTypeId.Value = string.Empty;

            HideDialog();

            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            EntityTypeService entityTypeService = new EntityTypeService( new RockContext() );
            SortProperty sortProperty = gEntityTypes.SortProperty;

            if ( sortProperty != null )
            {
                gEntityTypes.DataSource = entityTypeService
                    .Queryable()
                    .Where( e => e.IsSecured || e.IsEntity )
                    .Sort( sortProperty ).ToList();
            }
            else
            {
                gEntityTypes.DataSource = entityTypeService
                    .Queryable()
                    .Where( e => e.IsSecured || e.IsEntity )
                    .OrderBy( p => p.Name ).ToList();
            }

            gEntityTypes.DataBind();
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        protected void ShowEdit( int entityTypeId )
        {
            EntityTypeService entityTypeService = new EntityTypeService( new RockContext() );
            EntityType entityType = entityTypeService.Get( entityTypeId );

            if ( entityType != null )
            {
                mdEdit.Title = ActionTitle.Edit( EntityType.FriendlyTypeName );
                hfEntityTypeId.Value = entityType.Id.ToString();
                tbName.Text = entityType.Name;
                tbName.Enabled = false; // !entityType.IsEntity;
                tbFriendlyName.Text = entityType.FriendlyName;
                cbCommon.Checked = entityType.IsCommon;
            }
            else
            {
                mdEdit.Title = ActionTitle.Add( EntityType.FriendlyTypeName );
                hfEntityTypeId.Value = 0.ToString();
                tbName.Text = string.Empty;
                tbName.Enabled = true;
                tbFriendlyName.Text = string.Empty;
                cbCommon.Checked = false;
            }

            ShowDialog( "Edit" );
        }

        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "EDIT":
                    mdEdit.Show();
                    break;
            }
        }

        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {

                case "EDIT":
                    mdEdit.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion
    }
}