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
    /// <see cref="Grid"/> Column for toggling the bool/checkbox value of a row in a grid
    /// </summary>
    [ToolboxData( "<{0}:ToggleField BoundField=server runat=server></{0}:ToggleField>" )]
    public class ToggleField : TemplateField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToggleField" /> class.
        /// </summary>
        public ToggleField()
            : base()
        {
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string CssClass
        {
            get { return cssClass; }
            set { cssClass = value; }
        }
        private string cssClass = "switch-mini";

        public string DataField { get; set; }

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
            ToggleFieldTemplate toggleFieldTemplate = new ToggleFieldTemplate();
            toggleFieldTemplate.CheckedChanged += toggleFieldTemplate_CheckedChanged;
            this.ItemTemplate = toggleFieldTemplate;
            this.ParentGrid = control as Grid;
            return base.Initialize( sortingEnabled, control );
        }

        /// <summary>
        /// Handles the CheckedChanged event of the toggleFieldTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        void toggleFieldTemplate_CheckedChanged( object sender, RowEventArgs e )
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
        public event EventHandler<RowEventArgs> CheckedChanged;

        /// <summary>
        /// Raises the <see cref="E:Click"/> event.
        /// </summary>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        public virtual void OnClick( RowEventArgs e )
        {
            if ( CheckedChanged != null )
            {
                CheckedChanged( this, e );
            }
        }
    }

    /// <summary>
    /// Template used by the <see cref="ToggleField"/> control
    /// </summary>
    public class ToggleFieldTemplate : ITemplate
    {
        /// <summary>
        /// Gets or sets the parent grid.
        /// </summary>
        /// <value>
        /// The parent grid.
        /// </value>
        private Grid ParentGrid { get; set; }

        /// <summary>
        /// Gets or sets the DataField to bind to.
        /// </summary>
        /// <value>
        /// The DataField to bind to.
        /// </value>
        private string DataField { get; set; }

        /// <summary>
        /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control"/>
        /// object that child controls and templates belong to. These child controls are in
        /// turn defined within an inline template.
        /// </summary>
        /// <param name="container">The <see cref="T:System.Web.UI.Control"/> object to contain the instances of controls from the inline template.</param>
        public void InstantiateIn( Control container )
        {
            DataControlFieldCell cell = container as DataControlFieldCell;
            if ( cell != null )
            {
                ToggleField toggleField = cell.ContainingField as ToggleField;
                ParentGrid = toggleField.ParentGrid;
                DataField = toggleField.DataField;

                LabeledToggle labledToggle = new LabeledToggle();
                labledToggle.OnText = "yes";
                labledToggle.OffText = "no";
                labledToggle.EnableViewState = true; // TODO remove if unnecessary
                labledToggle.AddCssClass( toggleField.CssClass );
                labledToggle.CheckedChanged += labledToggle_CheckedChanged;
                labledToggle.DataBinding += labledToggle_DataBinding;
                labledToggle.PreRender += labledToggle_PreRender;

                cell.Controls.Add( labledToggle );
            }
        }

        void labledToggle_PreRender( object sender, EventArgs e )
        {
            LabeledToggle labledToggle = sender as LabeledToggle;
            if ( labledToggle.Enabled && !ParentGrid.Enabled )
            {
                labledToggle.AddCssClass( "disabled" );
                labledToggle.Enabled = false;
                labledToggle.AutoPostBack = false; // TODO remove if unnecessary
            }
        }

        /// <summary>
        /// Handles the DataBinding event of the labledToggle control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void labledToggle_DataBinding( object sender, EventArgs e )
        {
            LabeledToggle labledToggle = sender as LabeledToggle;
            GridViewRow dgi = labledToggle.NamingContainer as GridViewRow;
            if ( dgi.DataItem != null )
            {
                object dataValue = DataBinder.Eval( dgi.DataItem, DataField );

                labledToggle.Checked = ( (Boolean)dataValue );
                
            }
            labledToggle.AutoPostBack = true;  // TODO remove if unnecessary
        }

        /// <summary>
        /// Handles the CheckedChanged event of the labledToggle control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void labledToggle_CheckedChanged( object sender, EventArgs e )
        {
            if ( CheckedChanged != null )
            {
                GridViewRow row = (GridViewRow)( (LabeledToggle)sender ).Parent.Parent;
                RowEventArgs args = new RowEventArgs( row );
                CheckedChanged( sender, args );
            }
        }

        /// <summary>
        /// Occurs when checkbox [checked changed].
        /// </summary>
        internal event EventHandler<RowEventArgs> CheckedChanged;
    }
}