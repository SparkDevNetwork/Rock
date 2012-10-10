//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// <see cref="Grid"/> Column for deleting a row in a grid
    /// </summary>
    [ToolboxData( "<{0}:DeleteField runat=server></{0}:DeleteField>" )]
    public class DeleteField : TemplateField
    {
        /// <summary>
        /// Performs basic instance initialization for a data control field.
        /// </summary>
        /// <param name="sortingEnabled">A value that indicates whether the control supports the sorting of columns of data.</param>
        /// <param name="control">The data control that owns the <see cref="T:System.Web.UI.WebControls.DataControlField"/>.</param>
        /// <returns>
        /// Always returns false.
        /// </returns>
        public override bool Initialize( bool sortingEnabled, Control control )
        {
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            this.ItemStyle.CssClass = "grid-icon-cell delete";

            DeleteFieldTemplate deleteFieldTemplate = new DeleteFieldTemplate();
            deleteFieldTemplate.LinkButtonClick += deleteFieldTemplate_LinkButtonClick;
            this.ItemTemplate = deleteFieldTemplate;
            string rowItemText = ( control as Grid ).RowItemText;

            // Add Javascript Confirm to grids that use the DeleteField
            string script = string.Format( @"
        Sys.Application.add_load(function () {{
            $('#{0} td.grid-icon-cell.delete a').click(function(){{
                return confirm('Are you sure you want to delete this {1}?');
                }});
        }});
            ", control.ClientID, rowItemText );

            string scriptKey = string.Format( "grid-confirm-delete-{0}", control.ClientID );

            if ( !control.Page.ClientScript.IsClientScriptBlockRegistered( scriptKey ) )
            {
                control.Page.ClientScript.RegisterStartupScript( control.Page.GetType(), scriptKey, script, true );
            }

            return base.Initialize( sortingEnabled, control );
        }

        /// <summary>
        /// Handles the LinkButtonClick event of the deleteFieldTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        void deleteFieldTemplate_LinkButtonClick( object sender, RowEventArgs e )
        {
            OnClick( e );
        }

        /// <summary>
        /// Occurs when [click].
        /// </summary>
        public event EventHandler<RowEventArgs> Click;

        /// <summary>
        /// Raises the <see cref="E:Click"/> event.
        /// </summary>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        public virtual void OnClick( RowEventArgs e )
        {
            if ( Click != null )
            {
                Click( this, e );
            }
        }
    }

    /// <summary>
    /// Template used by the <see cref="DeleteField"/> control
    /// </summary>
    public class DeleteFieldTemplate : ITemplate
    {
        /// <summary>
        /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control"/> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
        /// </summary>
        /// <param name="container">The <see cref="T:System.Web.UI.Control"/> object to contain the instances of controls from the inline template.</param>
        public void InstantiateIn( Control container )
        {
            DataControlFieldCell cell = container as DataControlFieldCell;
            if ( cell != null )
            {
                LinkButton lbDelete = new LinkButton();
                lbDelete.ToolTip = "Delete";
                lbDelete.Click += lbDelete_Click;

                cell.Controls.Add( lbDelete );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void lbDelete_Click( object sender, EventArgs e )
        {
            if ( LinkButtonClick != null )
            {
                GridViewRow row = (GridViewRow)( (LinkButton)sender ).Parent.Parent;
                RowEventArgs args = new RowEventArgs( row.RowIndex );
                LinkButtonClick( sender, args );
            }
        }

        /// <summary>
        /// Occurs when [link button click].
        /// </summary>
        internal event EventHandler<RowEventArgs> LinkButtonClick;
    }
}