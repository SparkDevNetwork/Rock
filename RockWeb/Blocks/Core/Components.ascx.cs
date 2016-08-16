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
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Extension;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Used to manage the <see cref="Rock.Extension.Component"/> classes found through MEF.  Provides a way to edit the value
    /// of the attributes specified in each class.
    /// </summary>
    [System.ComponentModel.DisplayName( "Components" )]
    [System.ComponentModel.Category( "Core" )]
    [System.ComponentModel.Description( "Block to administrate MEF plugins." )]

    [TextField( "Component Container", "The Rock Extension Managed Component Container to manage. For example: 'Rock.Search.SearchContainer, Rock'", true, "", "", 1 )]
    [BooleanField( "Support Ordering", "Should user be allowed to re-order list of components?", true, "", 2 )]
    public partial class Components : RockBlock
    {
        #region Private Variables

        private bool _supportOrdering = true;
        private bool _isAuthorizedToConfigure = false;
        private IContainer _container;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _isAuthorizedToConfigure = IsUserAuthorized( Authorization.ADMINISTRATE );

            _supportOrdering = GetAttributeValue( "SupportOrdering" ).AsBoolean( true );

            Type containerType = Type.GetType( GetAttributeValue( "ComponentContainer" ) );
            if ( containerType != null )
            {
                PropertyInfo instanceProperty = containerType.GetProperty( "Instance" );
                if ( instanceProperty != null )
                {
                    _container = instanceProperty.GetValue( null, null ) as IContainer;
                    if ( _container != null )
                    {
                        BindFilter();
                        rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

                        if ( !Page.IsPostBack )
                        {
                            _container.Refresh();
                        }

                        rGrid.DataKeyNames = new string[] { "Id" };
                        rGrid.Columns[0].Visible = _supportOrdering && _isAuthorizedToConfigure;
                        rGrid.GridReorder += rGrid_GridReorder;
                        rGrid.GridRebind += rGrid_GridRebind;
                        rGrid.RowDataBound += rGrid_RowDataBound;

                        if ( containerType.BaseType.GenericTypeArguments.Length > 0 )
                        {
                            rGrid.RowItemText = containerType.BaseType.GenericTypeArguments[0].Name.SplitCase();
                        }

                        mdEditComponent.SaveClick += mdEditComponent_SaveClick;
                    }
                    else
                    {
                        DisplayError( "Could not get ContainerManaged instance from Instance property" );
                    }
                }
                else
                {
                    DisplayError( "ContainerManaged class does not have an 'Instance' property" );
                }
            }
            else
            {
                DisplayError( "Could not get the type of the specified Managed Component Container" );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbValidationError.Visible = false;

            if ( !Page.IsPostBack )
            {
                if ( _container != null )
                {
                    BindGrid();
                }
            }
            else
            {
                if ( hfActiveDialog.Value.Trim().ToUpper() == "EDITCOMPONENT" )
                {
                    int? serviceId = ViewState["serviceId"] as int?;
                    if ( serviceId.HasValue )
                    {
                        LoadEditControls( serviceId.Value, false );
                    }
                }

                ShowDialog();
            }
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the fDevice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Name", tbName.Text );
            rFilter.SaveUserPreference( "Description", tbDescription.Text );
            rFilter.SaveUserPreference( "Active", rblActive.SelectedValue );

            BindGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Controls.GridReorderEventArgs"/> instance containing the event data.</param>
        protected void rGrid_GridReorder( object sender, GridReorderEventArgs e )
        {
            var components = _container.Dictionary.ToList();
            var movedItem = components[e.OldIndex];
            components.RemoveAt( e.OldIndex );
            if ( e.NewIndex >= components.Count )
            {
                components.Add( movedItem );
            }
            else
            {
                components.Insert( e.NewIndex, movedItem );
            }

            var rockContext = new RockContext();
            int order = 0;
            foreach ( var item in components )
            {
                Component component = item.Value.Value;
                if ( component.Attributes.ContainsKey( "Order" ) )
                {
                    Rock.Attribute.Helper.SaveAttributeValue( component, component.Attributes["Order"], order.ToString(), rockContext );
                }

                order++;
            }

            _container.Refresh();

            BindGrid();
        }

        /// <summary>
        /// Handles the Edit event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( e.RowKeyId );
        }

        /// <summary>
        /// Handles the GridRebind event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void rGrid_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            ComponentDescription componentDescription = e.Row.DataItem as ComponentDescription;
            if ( componentDescription != null )
            {
                HtmlAnchor aSecure = e.Row.FindControl( "aSecure" ) as HtmlAnchor;
                if ( aSecure != null )
                {
                    aSecure.Visible = true;

                    var entityType = EntityTypeCache.Read( componentDescription.Type );
                    string url = Page.ResolveUrl( string.Format( "~/Secure/{0}/{1}?t={2}&pb=&sb=Done", entityType.Id, 0, componentDescription.Name + " Security" ) );
                    aSecure.HRef = "javascript: Rock.controls.modal.show($(this), '" + url + "')";
                }
            }
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the SaveClick event of the mdEditComponent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdEditComponent_SaveClick( object sender, EventArgs e )
        {
            int serviceId = (int)ViewState["serviceId"];
            Component component = _container.Dictionary[serviceId].Value;

            Rock.Attribute.Helper.GetEditValues( phProperties, component );
            component.SaveAttributeValues();

            string errorMessage = string.Empty;
            if ( !component.ValidateAttributeValues( out errorMessage ) )
            {
                nbValidationError.Text = string.Format( "<ul><li>{0}</li></ul>", errorMessage );
                nbValidationError.Visible = true;
            }
            else
            {
                HideDialog();
                BindGrid();
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            if ( !Page.IsPostBack )
            {
                tbName.Text = rFilter.GetUserPreference( "Name" );
                tbDescription.Text = rFilter.GetUserPreference( "Description" );
                rblActive.SelectedValue = rFilter.GetUserPreference( "Active" );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var dataSource = new List<ComponentDescription>();

            // Get components ordered by 'Order' attribute (if ordering is supported) or by Name
            var components = new Dictionary<int, KeyValuePair<string, Component>>();
            if ( _supportOrdering )
            {
                components = _container.Dictionary;
            }
            else
            {
                components = new Dictionary<int, KeyValuePair<string, Component>>();
                _container.Dictionary.OrderBy( c => c.Value.Key ).ToList().ForEach( c =>
                {
                    components.Add( c.Key, c.Value );
                } );
            }

            var rockContext = new RockContext();
            foreach ( var component in components )
            {
                Type type = component.Value.Value.GetType();
                if ( Rock.Attribute.Helper.UpdateAttributes( type, Rock.Web.Cache.EntityTypeCache.GetId( type.FullName ), string.Empty, string.Empty, rockContext ) )
                {
                    component.Value.Value.LoadAttributes( rockContext );
                }

                dataSource.Add( new ComponentDescription( component.Key, component.Value ) );
            }

            var items = dataSource.AsQueryable();

            string name = rFilter.GetUserPreference( "Name" );
            if ( !string.IsNullOrWhiteSpace( name ) )
            {
                items = items.Where( c => c.Name.ToLower().Contains( name.ToLower() ) );
            }

            string description = rFilter.GetUserPreference( "Description" );
            if ( !string.IsNullOrWhiteSpace( description ) )
            {
                items = items.Where( c => c.Name.Contains( description ) );
            }

            string active = rFilter.GetUserPreference( "Active" );
            if ( !string.IsNullOrWhiteSpace( active ) )
            {
                if ( active == "Yes" )
                {
                    items = items.Where( c => c.IsActive );
                }
                else
                {
                    items = items.Where( c => !c.IsActive );
                }
            }

            rGrid.DataSource = items.ToList();
            rGrid.DataBind();
        }

        /// <summary>
        /// Shows the edit panel
        /// </summary>
        /// <param name="serviceId">The service id.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        protected void ShowEdit( int serviceId )
        {
            ViewState["serviceId"] = serviceId;
            phProperties.Controls.Clear();
            LoadEditControls( serviceId, true );

            mdEditComponent.Title = ( _container.Dictionary[serviceId].Key + " Properties" ).FormatAsHtmlTitle();

            ShowDialog( "EditComponent" );
        }

        /// <summary>
        /// Loads the edit controls.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void LoadEditControls( int serviceId, bool setValues )
        {
            Component component = _container.Dictionary[serviceId].Value;

            component.InitializeAttributeValues( Request, ResolveRockUrl( "~/" ) );

            phProperties.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( component, phProperties, setValues, BlockValidationGroup, new List<string>() { "Order" } );
        }

        /// <summary>
        /// Displays the error.
        /// </summary>
        /// <param name="message">The message.</param>
        private void DisplayError( string message )
        {
            mdAlert.Show( message, ModalAlertType.Alert );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "EDITCOMPONENT":
                    mdEditComponent.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "EDITCOMPONENT":
                    mdEditComponent.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion
    }
}