//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.FieldTypes
{
    /// <summary>
    /// Field used to save and dispaly a text value
    /// </summary>
    public class Image : Field
    {
        /// <summary>
        /// Creates the control(s) neccessary for prompting user for a new value
        /// </summary>
        /// <param name="value">The current value</param>
        /// <param name="setValue">Should the control's value be set</param>
        /// <returns>
        /// The control
        /// </returns>
        public override System.Web.UI.Control CreateControl( string value, bool setValue )
        {
            Rock.Web.UI.Controls.ImageSelector imageSelector = new Web.UI.Controls.ImageSelector();
            imageSelector.ImageId = value;
            return imageSelector;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public override string ReadValue( Control control )
        {
            if ( control != null && control is Rock.Web.UI.Controls.ImageSelector )
                return ( ( Rock.Web.UI.Controls.ImageSelector )control ).ImageId;
            return null;
        }
    }
}