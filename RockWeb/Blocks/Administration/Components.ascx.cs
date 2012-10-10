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

using Rock.Extension;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// Used to manage the <see cref="Rock.MEF.StandardizeService"/> classes found through MEF.  Provides a way to edit the value
    /// of the attributes specified in each class
    /// </summary>
    [Rock.Attribute.Property( 0, "Component Container", "The Rock Extension Component Container to manage", true)]
    public partial class Components : Rock.Web.UI.RockBlock
    {
        #region Private Variables

        private bool _isAuthorizedToConfigure = false;
        private IContainer _container;

        #endregion

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _isAuthorizedToConfigure = CurrentPage.IsAuthorized( "Configure", CurrentPerson );

            Type containerType = Type.GetType( AttributeValue( "ComponentContainer" ) );
            if ( containerType != null )
            {
                PropertyInfo instanceProperty = containerType.GetProperty( "Instance" );
                if ( instanceProperty != null )
                {
                    _container = instanceProperty.GetValue( null, null ) as IContainer;
                    if ( _container != null )
                    {
                        if ( !Page.IsPostBack )
                            _container.Refresh();

                        rGrid.DataKeyNames = new string[] { "id" };
                        if ( _isAuthorizedToConfigure )
                            rGrid.GridReorder += rGrid_GridReorder;
                        rGrid.Columns[0].Visible = _isAuthorizedToConfigure;    // Reorder
                        rGrid.Columns[4].Visible = _isAuthorizedToConfigure;    // Edit Column
                        rGrid.GridRebind += rGrid_GridRebind;
                    }
                    else
                        DisplayError( "Could not get Container instance from Instance property" );
                }
                else
                    DisplayError( "Container class does not have an 'Instance' property" );
            }
            else
                DisplayError( "Could not get the type of the specified Component Container" );
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
                foreach ( var component in components )
                {
                    foreach ( var category in component.Value.Value.Attributes )
                        foreach ( var attribute in category.Value )
                            if ( attribute.Key == "Order" )
                                Rock.Attribute.Helper.SaveAttributeValue( component.Value.Value, attribute, order.ToString(), CurrentPersonId );
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

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlList.Visible = true;
            pnlDetails.Visible = false;
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

            pnlDetails.Visible = false;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var dataSource = new List<ComponentDescription>();
            foreach ( var component in _container.Dictionary )
            {
                Type type = component.Value.Value.GetType();
                if ( Rock.Attribute.Helper.UpdateAttributes( type, type.FullName, string.Empty, string.Empty, null ) )
                    Rock.Attribute.Helper.LoadAttributes( component.Value.Value );
                dataSource.Add( new ComponentDescription( component.Key, component.Value ) );
            }

            rGrid.DataSource = dataSource;
            rGrid.DataBind();

            pnlList.Visible = true;
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
            LoadEditControls();

            lProperties.Text = _container.Dictionary[serviceId].Key + " Properties";

            pnlList.Visible = false;
            pnlDetails.Visible = true;
        }

        private void LoadEditControls()
        {
            int serviceId = ( int )ViewState["serviceId"];
            Rock.Attribute.IHasAttributes component = _container.Dictionary[serviceId].Value;

            Rock.Attribute.Helper.AddEditControls( component, phProperties, true, new List<string>() { "Order" }  );
        }

        private void DisplayError( string message )
        {
            pnlMessage.Controls.Clear();
            pnlMessage.Controls.Add( new LiteralControl( message ) );
            pnlMessage.Visible = true;

            pnlList.Visible = false;
            pnlDetails.Visible = false;
        }

        #endregion
    }
}