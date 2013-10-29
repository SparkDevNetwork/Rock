//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// <see cref="Grid"/> Column for editing the value of a row in a grid
    /// </summary>
    [ToolboxData( "<{0}:EditValueField runat=server></{0}:EditValueField>" )]
    public class EditValueField : TemplateField, INotRowSelectedField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditValueField" /> class.
        /// </summary>
        public EditValueField()
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
            EditValueFieldTemplate editValueFieldTemplate = new EditValueFieldTemplate();
            editValueFieldTemplate.LinkButtonClick += editValueFieldTemplate_LinkButtonClick;
            this.ItemTemplate = editValueFieldTemplate;

            return base.Initialize( sortingEnabled, control );
        }

        /// <summary>
        /// Handles the LinkButtonClick event of the editValueFieldTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        void editValueFieldTemplate_LinkButtonClick( object sender, RowEventArgs e )
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
                Click( this, e );
        }
    }

    /// <summary>
    /// Template used by the <see cref="EditValueField"/> control
    /// </summary>
    public class EditValueFieldTemplate : ITemplate
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
                LinkButton lbEditValue = new LinkButton();
                lbEditValue.ToolTip = "Edit";
                lbEditValue.CssClass = "btn btn-default btn-sm";

                HtmlGenericControl buttonIcon = new HtmlGenericControl( "i" );
                buttonIcon.Attributes.Add( "class", "icon-pencil" );
                lbEditValue.Controls.Add( buttonIcon );

                lbEditValue.Click += lbEditValue_Click;

                cell.Controls.Add( lbEditValue );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void lbEditValue_Click( object sender, EventArgs e )
        {
            if ( LinkButtonClick != null )
            {
                GridViewRow row = ( GridViewRow )( ( LinkButton )sender ).Parent.Parent;
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