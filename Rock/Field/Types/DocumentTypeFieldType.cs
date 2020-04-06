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

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;


namespace Rock.Field.Types
{
    class DocumentTypeFieldType : FieldType
    {

        #region Configuration

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            List<string> configKeys = new List<string>();
            return configKeys;
        }

        #endregion Configuration

        #region Formatting

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            if ( string.IsNullOrWhiteSpace( value ) )
            {
                return base.FormatValue( parentControl, value, configurationValues, condensed );

            }

            // This is a list of IDs, we'll want it to be document type names instead
            var selectedValues = value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

            return DocumentTypeCache.All()
                .Where( v => selectedValues.Contains( v.Id ) )
                .Select( v => v.Name )
                .ToList()
                .AsDelimited( ", " );;
        }

        #endregion Formatting

        #region Edit Control

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            RockListBox editControl = new RockListBox { ID = id, DisplayDropAsAbsolute = true };

            var values = DocumentTypeCache.All().Select( v => new { v.Id, v.Name } ).OrderBy( v => v.Name ).ToList();
            foreach ( var value in values )
            {
                editControl.Items.Add( new ListItem( value.Name, value.Id.ToString() ) );
            }

            if ( editControl.Items.Count > 0 )
            {
                return editControl;
            }

            return null;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            List<string> values = new List<string>();

            if ( control != null && control is ListControl )
            {
                ListControl editControl = ( ListControl ) control;
                foreach ( ListItem li in editControl.Items )
                {
                    if ( li.Selected )
                    {
                        values.Add( li.Value );
                    }
                }

                return values.AsDelimited<string>( "," );
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
            if ( value == null ||  control == null || !( control is ListControl ) )
            {
                return;
            }

            var values = value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            ListControl editControl = ( ListControl ) control;
            foreach ( ListItem li in editControl.Items )
            {
                li.Selected = values.Contains( li.Value );
            }
        }

        #endregion Edit Control

        #region Filter Control

        #endregion Filter Control

    }
}
