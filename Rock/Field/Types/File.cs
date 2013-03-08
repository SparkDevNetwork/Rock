//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Model;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and dispaly a file 
    /// </summary>
    [Serializable]
    public class File : FieldType
    {
        public override List<string> ConfigurationKeys()
        {
            List<string> configKeys = new List<string>();
            configKeys.Add( "filetype" );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = new List<Control>();

            DropDownList ddl = new DropDownList();
            controls.Add( ddl );
            ddl.AutoPostBack = true;
            ddl.SelectedIndexChanged += OnQualifierUpdated;

            var service = new BinaryFileTypeService();
            foreach ( var binaryFileType in new BinaryFileTypeService()
                .Queryable()
                .OrderBy( b => b.Name ) )
            {
                ddl.Items.Add( new ListItem( binaryFileType.Name, binaryFileType.Id.ToString() ) );
            }

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( "filetype", new ConfigurationValue( "File Type", "The type of file", "" ) );

            if ( controls != null && controls.Count == 1 )
            {
                if ( controls[0] != null && controls[0] is DropDownList )
                    configurationValues["filetype"].Value = ( (DropDownList)controls[0] ).SelectedValue;
            }

            return configurationValues;
        }
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
            int binaryFileId = int.MinValue;
            if (Int32.TryParse(value, out binaryFileId))
            {
                var binaryFile = new BinaryFileService().Get(binaryFileId);
                if (binaryFile != null)
                {
                    return binaryFile.FileName;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="configurationValues"></param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls != null && controls.Count == 1 && configurationValues != null )
            {
                if ( controls[0] != null && controls[0] is DropDownList && configurationValues.ContainsKey( "filetype" ) )
                    ( (DropDownList)controls[0] ).SelectedValue = configurationValues["filetype"].Value;
            }
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
            return new Rock.Web.UI.Controls.FileSelector();
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control != null && control is Rock.Web.UI.Controls.FileSelector )
            {
                var fileSelector = (Rock.Web.UI.Controls.FileSelector)control;

                return fileSelector.BinaryFileId.HasValue ? fileSelector.BinaryFileId.Value.ToString() : string.Empty;
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
            if ( value != null && control != null && control is Rock.Web.UI.Controls.FileSelector )
            {
                var fileSelector = (Rock.Web.UI.Controls.FileSelector)control;

                int binaryFileId = 0;
                if ( Int32.TryParse( value, out binaryFileId ) )
                {
                    fileSelector.BinaryFileId = binaryFileId;
                }
            }
        }
    }
}