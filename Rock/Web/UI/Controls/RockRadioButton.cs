using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class RockRadioButton : RadioButton
    {
        /// <summary>
        /// Gets or sets a value indicating whether [display inline].
        /// Defaults to True
        /// True will render the label with class="radio-inline"
        /// </summary>
        /// <value>
        ///   <c>true</c> if [display inline]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayInline
        {
            get
            {
                return this.ViewState["DisplayInline"] as bool? ?? true;
            }

            set
            {
                this.ViewState["DisplayInline"] = value;
            }
        }
    }
}
