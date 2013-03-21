//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a dropdown list of binary file types
    /// </summary>
    [Serializable]
    public class BinaryFileTypeFieldType : FieldType
    {
        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            Guid binaryFileTypeGuid = Guid.Empty;
            if (Guid.TryParse( value, out binaryFileTypeGuid ))
            {
                var binaryFiletype = new BinaryFileTypeService().Get( binaryFileTypeGuid );
                if ( binaryFiletype != null )
                {
                    return binaryFiletype.Name;
                }
            }
                
            return string.Empty;

        }

        /// <summary>
        /// Creates the control(s) neccessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues )
        {
            return new BinaryFileTypePicker();
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control != null && control is BinaryFileTypePicker )
            {
                int id = int.MinValue;
                if ( Int32.TryParse( ( (BinaryFileTypePicker)control ).SelectedValue, out id ) )
                {
                    var binaryFiletype = new BinaryFileTypeService().Get( id );
                    if ( binaryFiletype != null )
                    {
                        return binaryFiletype.Guid.ToString();
                    }
                }
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
            Guid binaryFileTypeGuid = Guid.Empty;
            if (Guid.TryParse( value, out binaryFileTypeGuid ))
            {
                if ( control != null && control is BinaryFileTypePicker )
                {
                    var binaryFiletype = new BinaryFileTypeService().Get( binaryFileTypeGuid );
                    if ( binaryFiletype != null )
                    {
                        ( (BinaryFileTypePicker)control ).SetValue( binaryFiletype.Id.ToString() );
                    }
                }
            }
        }

    }
}