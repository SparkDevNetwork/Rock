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
    public partial class Components : Rock.Web.UI.Block
    {
        #region Private Variables

        private bool _isAuthorizedToConfigure = false;
        private IContainer _container;

        #endregion

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _isAuthorizedToConfigure = PageInstance.IsAuthorized( "Configure", CurrentUser );

            Type containerType = Type.GetType( AttributeValue( "ComponentContainer" ) );
            if ( containerType != null )
            {
                PropertyInfo instanceProperty = containerType.GetProperty( "Instance" );
                if ( instanceProperty != null )
                {
                    _container = instanceProperty.GetValue( null, null ) as IContainer;
                    if ( _container != null )
                    {
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

        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack && _container != null)
                BindGrid();

            if ( Page.IsPostBack && hfComponentId.Value != string.Empty )
                ShowEdit( Int32.Parse( hfComponentId.Value ), false );

            base.OnLoad( e );
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
            ShowEdit( ( int )rGrid.DataKeys[e.RowIndex]["id"], true );
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
            hfComponentId.Value = string.Empty;
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
            Rock.Attribute.IHasAttributes component = _container.Dictionary[Int32.Parse( hfComponentId.Value )].Value;

            Rock.Attribute.Helper.GetEditValues( dlProperties, component );
            Rock.Attribute.Helper.SaveAttributeValues( component, CurrentPersonId );

            hfComponentId.Value = string.Empty;

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
                Type type = component.Value.GetType();
                if ( Rock.Attribute.Helper.UpdateAttributes( type, type.FullName, string.Empty, string.Empty, null ) )
                    Rock.Attribute.Helper.LoadAttributes( component.Value.Value );
                dataSource.Add( new ComponentDescription( component.Key, component.Value.Value ) );
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
        protected void ShowEdit( int serviceId, bool setValues )
        {
            Rock.Attribute.IHasAttributes component = _container.Dictionary[serviceId].Value;
            hfComponentId.Value = serviceId.ToString();

            dlProperties.Controls.Clear();
            foreach ( HtmlGenericControl li in Rock.Attribute.Helper.GetEditControls( component, setValues ) )
                if (li.Attributes["attribute-key"] != "Order")
                    dlProperties.Controls.Add( li );

            lProperties.Text = _container.Dictionary[serviceId].Key + " Properties";

            pnlList.Visible = false;
            pnlDetails.Visible = true;
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