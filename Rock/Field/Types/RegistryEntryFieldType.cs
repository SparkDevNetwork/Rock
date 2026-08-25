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
using System;
using System.Collections.Generic;
#if WEBFORMS
using System.Web.UI;
#endif
using Rock.Attribute;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{

    /// <summary>
    /// Field Type used for Volume / Page / Entry that is useful for sacraments or steps (<see cref="Rock.Model.Step"/>)
    /// </summary>
    /// <seealso cref="Rock.Field.FieldType" />
    [FieldTypeUsage( FieldTypeUsage.Administrative )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.REGISTRY_ENTRY )]
    public class RegistryEntryFieldType : FieldType
    {

        #region Value Hinting

        /// <inheritdoc/>
        /// <remarks>
        /// Three parts, and the editor refuses anything that is not exactly three,
        /// returning without setting a thing. So a value with a part missing is not
        /// partially loaded and not rejected with a message, it simply does not
        /// appear, which is why the count is stated as plainly as the order.
        /// </remarks>
        internal override FieldTypeHints GetFieldHints( Dictionary<string, string> privateConfigurationValues )
        {
            return new FieldTypeHints
            {
                IsCompleteList = false,
                ValueFormat = "Three whole numbers separated by commas, in the order volume, page, line, as in 12,204,7. This records where an entry sits in a physical register, so it is used for things like sacraments and steps. All three commas positions must be present even when a part is unknown, because a value that does not split into exactly three parts is ignored rather than partially read; leave the unknown part empty, as in 12,,7. A part that is not a whole number is dropped and comes back empty."
            };
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The id.</param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new RegistryEntry
            {
                ID = id
            };
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control != null && control is RegistryEntry )
            {
                return ( ( RegistryEntry ) control ).Text;
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( control != null && control is RegistryEntry )
            {
                ( ( RegistryEntry ) control ).Text = value;
            }
        }

        #region Filter Control

        /// <summary>
        /// Creates the control needed to filter (query) values using this field type.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override System.Web.UI.Control FilterControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, Rock.Reporting.FilterMode filterMode )
        {
            // This field type does not support filtering
            return null;
        }

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return false;
        }

        #endregion

#endif
        #endregion
    }
}
