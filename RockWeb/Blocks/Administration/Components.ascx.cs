//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using Rock;
using Rock.Attribute;
using Rock.Extension;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// Used to manage the <see cref="Rock.Extension.Component"/> classes found through MEF.  Provides a way to edit the value
    /// of the attributes specified in each class
    /// </summary>
    [TextField( "Component Container", "The Rock Extension Managed Component Container to manage")]
    public partial class Components : RockBlock, IDimmableBlock
    {
        #region Private Variables

        private bool _isAuthorizedToConfigure = false;
        private IContainer _container;

        #endregion

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _isAuthorizedToConfigure = CurrentPage.IsAuthorized( "Administrate", CurrentPerson );

            // wire up page naviagte
            RockPage page = Page as RockPage;

            if (page != null)
            {
                page.PageNavigate += page_PageNavigate;
            }

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
                            _container.Refresh();

                        rGrid.DataKeyNames = new string[] { "id" };
                        if ( _isAuthorizedToConfigure )
                            rGrid.GridReorder += rGrid_GridReorder;
                        rGrid.Columns[0].Visible = _isAuthorizedToConfigure;    // Reorder
                        rGrid.GridRebind += rGrid_GridRebind;
                        rGrid.RowDataBound += rGrid_RowDataBound;

                        if ( containerType.BaseType.GenericTypeArguments.Length > 0 )
                        {
                            rGrid.RowItemText = containerType.BaseType.GenericTypeArguments[0].Name.SplitCase();
                        }
                    }
                    else
                        DisplayError( "Could not get ContainerManaged instance from Instance property" );
                }
                else
                    DisplayError( "ContainerManaged class does not have an 'Instance' property" );
            }
            else
                DisplayError( "Could not get the type of the specified Manged Component Container" );
        }

        /// <summary>
        /// Handles the history state to allow the back button to work in the update panel.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.HistoryEventArgs"/> instance containing the history data.</param>
        void page_PageNavigate(object sender, HistoryEventArgs e)
        {
            SetEditMode(false);
        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            LoadEditControls();
        }
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack && _container != null)
                BindGrid();
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
        void rGrid_GridReorder( object sender, GridReorderEventArgs e )
        {
            var components = _container.Dictionary.ToList();
            var movedItem = components[e.OldIndex];
            components.RemoveAt( e.OldIndex );
            if ( e.NewIndex >= components.Count )
                components.Add( movedItem );
            else
                components.Insert( e.NewIndex, movedItem );

            using ( new Rock.Data.UnitOfWorkScope() )
            {
                int order = 0;
                foreach ( var item in components )
                {
                    Component component = item.Value.Value;
                    if (  component.Attributes.ContainsKey("Order") )
                    {
                        Rock.Attribute.Helper.SaveAttributeValue( component, component.Attributes["Order"], order.ToString(), CurrentPersonId );
                    }
                    order++;
                }
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
            ShowEdit( ( int )rGrid.DataKeys[e.RowIndex]["id"] );
        }

        /// <summary>
        /// Handles the GridRebind event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        void rGrid_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
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
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            SetEditMode( false );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            int serviceId = ( int )ViewState["serviceId"];
            Rock.Attribute.IHasAttributes component = _container.Dictionary[serviceId].Value;

            Rock.Attribute.Helper.GetEditValues( phProperties, component );
            Rock.Attribute.Helper.SaveAttributeValues( component, CurrentPersonId );

            BindGrid();

            SetEditMode( false );
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
            foreach ( var component in _container.Dictionary )
            {
                Type type = component.Value.Value.GetType();
                using ( new Rock.Data.UnitOfWorkScope() )
                {
                    if ( Rock.Attribute.Helper.UpdateAttributes( type, Rock.Web.Cache.EntityTypeCache.GetId( type.FullName ), string.Empty, string.Empty, null ) )
                    {
                        component.Value.Value.LoadAttributes();
                    }
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
            // set edit history marker
            this.AddHistory("edit", "", "Edit " + _container.Dictionary[serviceId].Key);
            
            ViewState["serviceId"] = serviceId;
            phProperties.Controls.Clear();
            LoadEditControls();

            lProperties.Text = _container.Dictionary[serviceId].Key + " Properties";

            SetEditMode( true );
        }


        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode (bool editable)
        {
            pnlDetails.Visible = editable;
            pnlList.Visible = !editable;

            this.DimOtherBlocks( editable );
        }

        private void LoadEditControls()
        {
            int serviceId = ( int )ViewState["serviceId"];
            Rock.Attribute.IHasAttributes component = _container.Dictionary[serviceId].Value;

            Rock.Attribute.Helper.AddEditControls( component, phProperties, true, new List<string>() { "Order" }  );
        }

        private void DisplayError( string message )
        {
            mdAlert.Show( message, ModalAlertType.Alert );
        }

        #region IDimmableBlock

        /// <summary>
        /// Sets the dimmed.
        /// </summary>
        /// <param name="dimmed">if set to <c>true</c> [dimmed].</param>
        public void SetDimmed( bool dimmed )
        {
            pnlList.Enabled = !dimmed;
            pnlDetails.Enabled = !dimmed;
            rGrid.Enabled = !dimmed;
        }

        #endregion
        
        #endregion
    }
}