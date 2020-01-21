// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
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
        /// Gets or sets the step value (defaults to 1)
        /// </summary>
        /// <value>
        /// The maximum.
        /// </value>
        public decimal? StepValue { get; set; }

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
            string disable = ( !this.Enabled || this.ReadOnly ) ? "true" : "false";
            var script = $@"Rock.controls.rangeSlider.initialize({{ controlId: '{ClientID}', min: '{MinValue ?? 0}', max: '{MaxValue ?? 100}', step:'{StepValue ?? 1}', from: '{SelectedValue ?? 0}', disable: {disable} }};";

            ScriptManager.RegisterStartupScript( this, this.GetType(), "range_slider-" + this.ClientID, script, true );
        }
    }
}
