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
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// User controls for managing defined types and their values
    /// </summary>
    [DisplayName( "Defined Type List" )]
    [Category( "Core" )]
    [Description( "Lists all the defined types and allows for managing them and their values." )]

    [LinkedPage("Detail Page")]
    public partial class DefinedTypeList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            BindFilter();
            tFilter.ApplyFilterClick += tFilter_ApplyFilterClick;

            gDefinedType.DataKeyNames = new string[] { "id" };
            gDefinedType.Actions.ShowAdd = true;
            gDefinedType.Actions.AddClick += gDefinedType_Add;
            gDefinedType.GridRebind += gDefinedType_GridRebind;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gDefinedType.Actions.ShowAdd = canAddEditDelete;
            gDefinedType.IsDeleteEnabled = canAddEditDelete;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                gDefinedType_Bind();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Filter Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the tFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void tFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            tFilter.SaveUserPreference( "Category", ddlCategoryFilter.SelectedValue );

            gDefinedType_Bind();
        }

        #endregion

        #region Grid Events (main grid)

        /// <summary>
        /// Handles the Add event of the gDefinedType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gDefinedType_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "definedTypeId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gDefinedType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gDefinedType_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "definedTypeId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gDefinedType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gDefinedType_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var definedValueService = new DefinedValueService( rockContext );
            var definedTypeService = new DefinedTypeService( rockContext );

            DefinedType type = definedTypeService.Get( e.RowKeyId );

            if ( type != null )
            {
                string errorMessage;
                if ( !definedTypeService.CanDelete( type, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                // if this DefinedType has DefinedValues, see if they can be deleted
                var definedValues = definedValueService.GetByDefinedTypeId( type.Id ).ToList();

                foreach ( var value in definedValues )
                {
                    if ( !definedValueService.CanDelete( value, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }
                }

                foreach ( var value in definedValues )
                {
                    definedValueService.Delete( value );
                }

                definedTypeService.Delete( type );

                rockContext.SaveChanges();
            }

            gDefinedType_Bind();
        }

        /// <summary>
        /// Handles the GridRebind event of the gDefinedType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gDefinedType_GridRebind( object sender, EventArgs e )
        {
            gDefinedType_Bind();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ddlCategoryFilter.Items.Clear();
            ddlCategoryFilter.Items.Add( new ListItem( Rock.Constants.All.Text, string.Empty ) );

            var items = new DefinedTypeService( new RockContext() ).Queryable()
                .Where( a => a.Category != string.Empty)
                .OrderBy( a => a.Category )
                .Select( a => a.Category )
                .Distinct()
                .ToList();

            foreach ( var item in items )
            {
                ListItem li = new ListItem( item );
                li.Selected = ( !Page.IsPostBack && tFilter.GetUserPreference( "Category" ) == item );
                ddlCategoryFilter.Items.Add( li );
            }
        }

        /// <summary>
        /// Binds the grid for defined types.
        /// </summary>
        private void gDefinedType_Bind()
        {
            var queryable = new DefinedTypeService( new RockContext() ).Queryable().Select( a =>
                new
                {
                    a.Id,
                    a.Category,
                    a.Name,
                    a.Description,
                    a.IsSystem,
                    FieldTypeName = a.FieldType.Name
                } );

            string categoryFilter = tFilter.GetUserPreference( "Category" );
            if ( !string.IsNullOrWhiteSpace(categoryFilter) && categoryFilter != Rock.Constants.All.Text )
            {
                queryable = queryable.Where( a => a.Category == categoryFilter );
            }

            SortProperty sortProperty = gDefinedType.SortProperty;
            if ( sortProperty != null )
            {
                queryable = queryable.Sort( sortProperty );
            }
            else
            {
                queryable = queryable.OrderBy( a => a.Category ).ThenBy( a => a.Name );
            }

            gDefinedType.DataSource = queryable.ToList();
            gDefinedType.DataBind();
        }

        #endregion
    }
}