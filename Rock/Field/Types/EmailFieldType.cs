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
using System.Text.RegularExpressions;
using System.Web.UI;
using Rock.Communication;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display an email address
    /// </summary>
    [Serializable]
    [Rock.Attribute.RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    public class EmailFieldType : FieldType
    {

        #region Formatting

        /// <summary>
        /// Formats the value as HTML.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public override string FormatValueAsHtml( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            return HtmlFormat( FormatValue( parentControl, value, configurationValues, condensed ) );
        }

        /// <summary>
        /// Formats the value as HTML.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public override string FormatValueAsHtml( Control parentControl, int? entityTypeId, int? entityId, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            return HtmlFormat( FormatValue( parentControl, entityTypeId, entityId, value, configurationValues, condensed ) );
        }

        /// <summary>
        /// HTMLs the format.
        /// </summary>
        /// <param name="formattedValue">The formatted value.</param>
        /// <returns></returns>
        private string HtmlFormat( string formattedValue )
        {
            if ( string.IsNullOrWhiteSpace( formattedValue ) )
            {
                return string.Empty;
            }
            else
            {
                return string.Format( "<a href='mailto:{0}'>{0}</a>", formattedValue );
            }
        }

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues"></param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( System.Web.UI.Control parentControl, string value, System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            if ( string.IsNullOrWhiteSpace( value ) )
            {
                return string.Empty;
            }
            return value;
        }

        /// <summary>
        /// Returns the value that should be used for sorting, using the most appropriate datatype
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override object SortValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            // use the plain email address as the sort value
            return value;
        }

        #endregion

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
            return new EmailBox { ID = id }; 
        }

        /// <summary>
        /// Tests the value to ensure that it is a valid value.  If not, message will indicate why
        /// </summary>
        /// <param name="value"></param>
        /// <param name="required"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool IsValid( string value, bool required, out string message )
        {
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                var result = EmailAddressFieldValidator.Validate( value, allowMultipleAddresses: false, allowLava: false );

                if ( result == EmailFieldValidationResultSpecifier.Valid )
                { 
                    message = "The email address provided is valid.";
                    return true;
                }
            }

            return base.IsValid( value, required, out message );
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Gets the filter value control with the specified FilterMode
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            var control = new RockTextBox { ID = id };
            control.ID = string.Format( "{0}_ctlCompareValue", id );
            control.AddCssClass( "js-filter-control" );
            return control;
        }

        /// <summary>
        /// Gets the type of the filter comparison.
        /// </summary>
        /// <value>
        /// The type of the filter comparison.
        /// </value>
        public override Model.ComparisonType FilterComparisonType
        {
            get
            {
                return ComparisonHelper.StringFilterComparisonTypes;
            }
        }

        #endregion

    }
}