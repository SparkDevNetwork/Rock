//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:System.Web.UI.WebControls.TextBox"/> control with numerical validation 
    /// </summary>
    [ToolboxData( "<{0}:NumberBox runat=server></{0}:NumberBox>" )]
    public class NumberBox : TextBox
    {
        private RangeValidator rangeValidator;

        public string FieldName
        {
            get { return ViewState["FieldName"] as string ?? string.Empty; }
            set { ViewState["FieldName"] = value; }
        }
            
        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        /// <value>
        /// The minimum value.
        /// </value>
        public string MinimumValue
        {
            get
            {
                return rangeValidator.MinimumValue;
            }
            set
            {
                rangeValidator.MinimumValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        /// <value>
        /// The maximum value.
        /// </value>
        public string MaximumValue
        {
            get
            {
                return rangeValidator.MaximumValue;
            }
            set
            {
                rangeValidator.MaximumValue = value;
            }
        }

        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );

            rangeValidator = new RangeValidator();
            rangeValidator.ControlToValidate = this.ID;
            rangeValidator.ID = this.ID + "_RV";
            rangeValidator.Display = ValidatorDisplay.Dynamic;
            rangeValidator.CssClass = "validation-error help-inline";
            rangeValidator.Type = ValidationDataType.Integer;
            rangeValidator.MinimumValue = int.MinValue.ToString();
            rangeValidator.MaximumValue = int.MaxValue.ToString();
            rangeValidator.ErrorMessage = "Numerical value is required";

            Controls.Add( rangeValidator );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            base.RenderControl( writer );

            if ( !string.IsNullOrWhiteSpace( FieldName ) )
            {
                rangeValidator.ErrorMessage = string.Format( "Numerical value is required for '{0}'", FieldName );
            }
            rangeValidator.Type = rangeValidator.MinimumValue.Contains( "." ) ? ValidationDataType.Double : ValidationDataType.Integer;
            rangeValidator.Enabled = true;
            rangeValidator.RenderControl( writer );
        }
    }
}