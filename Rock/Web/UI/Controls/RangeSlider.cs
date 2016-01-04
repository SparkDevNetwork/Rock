using System.Web.UI;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class RangeSlider : RockTextBox
    {
        /// <summary>
        /// Gets or sets the minimum value (defaults to 0)
        /// </summary>
        /// <value>
        /// The minimum.
        /// </value>
        public int? MinValue { get; set; }

        /// <summary>
        /// Gets or sets the maximum value (defaults to 100)
        /// </summary>
        /// <value>
        /// The maximum.
        /// </value>
        public int? MaxValue { get; set; }

        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        /// <value>
        /// The selected value.
        /// </value>
        public int? SelectedValue
        {
            get
            {
                return this.Text.AsIntegerOrNull();
            }

            set
            {
                if ( value.HasValue )
                {
                    this.Text = value.ToString();
                }
                else
                {
                    this.Text = string.Empty;
                }
            }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            RegisterJavascript();
            base.RenderControl( writer );
        }

        /// <summary>
        /// Registers the javascript.
        /// </summary>
        private void RegisterJavascript()
        {
            var script = string.Format(
                @"Rock.controls.rangeSlider.initialize({{ controlId: '{0}', min: '{1}', max: '{2}', from: '{3}' }});",
                this.ClientID,
                this.MinValue ?? 0,
                this.MaxValue ?? 100,
                this.SelectedValue ?? 0 );

            ScriptManager.RegisterStartupScript( this, this.GetType(), "range_slider-" + this.ClientID, script, true );
        }
    }
}
