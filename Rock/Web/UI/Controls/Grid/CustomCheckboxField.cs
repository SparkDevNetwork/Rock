using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// <see cref="Grid"/> Column to display a boolean value.
    /// </summary>
    [ToolboxData( "<{0}:CustomCheckboxField runat=server></{0}:CustomCheckboxField>" )]
    public class CustomCheckboxField : BoundField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoolField" /> class.
        /// </summary>
        public CustomCheckboxField()
            : base()
        {
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        }

        public bool IsChecked
        {
            get
            {
                bool b = true;
                if ( this.ViewState["IsChecked"] != null )
                    b = (bool)this.ViewState["IsChecked"];
                return b;
            }
            set { this.ViewState["IsChecked"] = value; }
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

        protected override void InitializeDataCell( DataControlFieldCell cell, DataControlRowState rowState )
        {
            if ( !string.IsNullOrEmpty( this.DataField ) )
            {
                LabeledToggle toggle = new LabeledToggle();
                toggle.OnText = "yes";
                toggle.OffText = "no";
                toggle.CssClass = cssClass;
                toggle.DataBinding += OnDataBindField;
                toggle.CheckedChanged += toggle_CheckedChanged;
                toggle.AutoPostBack = true;
                cell.Controls.Add( toggle );
            }
            else
            {
                base.InitializeDataCell( cell, rowState );
            }
        }

        protected void toggle_CheckedChanged( object sender, EventArgs e )
        {
            if ( CheckChanged != null )
            {
                GridViewRow row = (GridViewRow)( (CheckBox)sender ).Parent.Parent;
                RowEventArgs args = new RowEventArgs( row );
                CheckChanged( sender, args );
            }
        }

        /// <summary>
        /// Occurs when [CheckChanged].
        /// </summary>
        public event EventHandler<RowEventArgs> CheckChanged;

        /// <summary>
        /// Raises the <see cref="E:CheckChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        public virtual void OnCheckChanged( RowEventArgs e )
        {
            if ( CheckChanged != null )
                CheckChanged( this, e );
        }

        protected override void OnDataBindField( object sender, EventArgs e )
        {
            //base.OnDataBindField( sender, e );

            Control control = (Control)sender;
            Control namingContainer = control.NamingContainer;
            object dataValue = this.GetValue( namingContainer );

            if ( control is LabeledToggle )
            {
                LabeledToggle toggle = (LabeledToggle) control;

                toggle.Checked = ( (Boolean)dataValue );
            }
        }
    }
}
