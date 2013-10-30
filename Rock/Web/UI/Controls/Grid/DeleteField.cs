//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using Rock;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// <see cref="Grid"/> Column for deleting a row in a grid
    /// </summary>
    [ToolboxData( "<{0}:DeleteField runat=server></{0}:DeleteField>" )]
    public class DeleteField : TemplateField, INotRowSelectedField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteField" /> class.
        /// </summary>
        public DeleteField()
            : base()
        {
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            this.HeaderStyle.CssClass = "grid-columncommand";
            this.ItemStyle.CssClass = "grid-columncommand";
        }

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
            DeleteFieldTemplate deleteFieldTemplate = new DeleteFieldTemplate();
            deleteFieldTemplate.LinkButtonClick += deleteFieldTemplate_LinkButtonClick;
            this.ItemTemplate = deleteFieldTemplate;
            this.ParentGrid = control as Grid;
            return base.Initialize( sortingEnabled, control );
        }

        /// <summary>
        /// Handles the LinkButtonClick event of the deleteFieldTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void deleteFieldTemplate_LinkButtonClick( object sender, RowEventArgs e )
        {
            OnClick( e );
        }

        /// <summary>
        /// Gets the parent grid.
        /// </summary>
        /// <value>
        /// The parent grid.
        /// </value>
        public Grid ParentGrid { get; internal set; }

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
                DeleteField deleteField = cell.ContainingField as DeleteField;
                ParentGrid = deleteField.ParentGrid;
                LinkButton lbDelete = new LinkButton();
                lbDelete.CausesValidation = false;
                lbDelete.CssClass = "btn btn-danger btn-sm";
                lbDelete.ToolTip = "Delete";

                HtmlGenericControl buttonIcon = new HtmlGenericControl( "i" );
                buttonIcon.Attributes.Add( "class", "icon-remove" );
                lbDelete.Controls.Add( buttonIcon );

                lbDelete.Click += lbDelete_Click;
                lbDelete.DataBinding += lbDelete_DataBinding;
                lbDelete.PreRender += lbDelete_PreRender;

                // make sure delete button is registered for async postback (needed just in case the grid was created at runtime)
                var sm = ScriptManager.GetCurrent( this.ParentGrid.Page );
                sm.RegisterAsyncPostBackControl( lbDelete );

                cell.Controls.Add( lbDelete );
            }
        }

        /// <summary>
        /// Handles the PreRender event of the lbDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void lbDelete_PreRender( object sender, EventArgs e )
        {
            LinkButton lbDelete = sender as LinkButton;
            if ( lbDelete.Enabled && ( !ParentGrid.Enabled || !ParentGrid.IsDeleteEnabled ) )
            {
                lbDelete.AddCssClass( "disabled" );
                lbDelete.Enabled = false;
            }
            
            if ( lbDelete.Enabled &&
                ParentGrid.ShowConfirmDeleteDialog && 
                ParentGrid.Enabled && 
                ParentGrid.IsDeleteEnabled )
            {
                lbDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.controls.grid.confirmDelete(event, '{0}');", ParentGrid.RowItemText );
            }
            else
            {
                lbDelete.Attributes.Remove( "onclick" );
            }
        }

        /// <summary>
        /// Gets or sets the parent grid.
        /// </summary>
        /// <value>
        /// The parent grid.
        /// </value>
        private Grid ParentGrid { get; set; }

        /// <summary>
        /// Handles the DataBinding event of the lbDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbDelete_DataBinding( object sender, EventArgs e )
        {
            if ( ParentGrid.HideDeleteButtonForIsSystem )
            {
                LinkButton lbDelete = sender as LinkButton;
                GridViewRow dgi = lbDelete.NamingContainer as GridViewRow;
                if ( dgi.DataItem != null )
                {
                    PropertyInfo pi = dgi.DataItem.GetType().GetProperty( "IsSystem" );
                    if ( pi != null )
                    {
                        bool isSystem = (bool)pi.GetValue( dgi.DataItem );
                        if ( isSystem )
                        {
                            lbDelete.AddCssClass( "disabled" );
                            lbDelete.Enabled = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void lbDelete_Click( object sender, EventArgs e )
        {
            if ( LinkButtonClick != null )
            {
                GridViewRow row = (GridViewRow)( (LinkButton)sender ).Parent.Parent;
                RowEventArgs args = new RowEventArgs( row );
                LinkButtonClick( sender, args );
            }
        }

        /// <summary>
        /// Occurs when [link button click].
        /// </summary>
        internal event EventHandler<RowEventArgs> LinkButtonClick;
    }
}