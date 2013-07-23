//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display an image value
    /// </summary>
    [Serializable]
    public class ImageFieldType : FieldType
    {
        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues"></param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                var imagePath = Path.Combine( parentControl.ResolveUrl( "~" ), "Image.ashx" );
                int imgSize = 100;
                if ( condensed )
                {
                    imgSize = 50;
                }

                string imageUrlFormat = "<img src='" + imagePath + "?id={0}&width={1}&height={1}' />";
                return string.Format( imageUrlFormat, value, imgSize );
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Creates the control(s) neccessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new Web.UI.Controls.ImageUploader { ID = id }; 
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control != null && control is Rock.Web.UI.Controls.ImageUploader )
            {
                int? imageId = ( (Rock.Web.UI.Controls.ImageUploader)control ).ImageId;
                return imageId.HasValue ? imageId.Value.ToString() : string.Empty;
            }
            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( value != null && control != null && control is Rock.Web.UI.Controls.ImageUploader )
            {
                int imageId = 0;
                if ( Int32.TryParse( value, out imageId ) )
                {
                    ( (Rock.Web.UI.Controls.ImageUploader)control ).ImageId = imageId;
                }
                else
                {
                    ( (Rock.Web.UI.Controls.ImageUploader)control ).ImageId = null;
                }
            }
        }
    }
}