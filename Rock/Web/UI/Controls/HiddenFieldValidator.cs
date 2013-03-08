//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// from http://stackoverflow.com/questions/6607984/asprequiredfieldvalidator-does-not-validate-hidden-fields
    /// </summary>
    public class HiddenFieldValidator : RequiredFieldValidator
    {
        /// <summary>
        /// Determines whether the control specified by the <see cref="P:System.Web.UI.WebControls.BaseValidator.ControlToValidate" /> property is a valid control.
        /// </summary>
        /// <returns>
        /// true if the control specified by <see cref="P:System.Web.UI.WebControls.BaseValidator.ControlToValidate" /> is a valid control; otherwise, false.
        /// </returns>
        protected override bool ControlPropertiesValid()
        {
            return true;
        }
    }
}
