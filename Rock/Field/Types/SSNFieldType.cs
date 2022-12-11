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
using System.Linq;
#if WEBFORMS
using System.Web.UI;
#endif
using Rock.Attribute;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a social security number
    /// </summary>
    [Serializable]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [IconSvg( @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><path d=""M13.83,2.56H2.17A1.16,1.16,0,0,0,1,3.72v8.56a1.16,1.16,0,0,0,1.17,1.16H13.83A1.16,1.16,0,0,0,15,12.28V3.72A1.16,1.16,0,0,0,13.83,2.56Zm0,9.72H8.37c0-.11,0,.09,0-.55a1.53,1.53,0,0,0-1.63-1.4,6,6,0,0,1-1.09.2,5.44,5.44,0,0,1-1.09-.2,1.54,1.54,0,0,0-1.64,1.4c0,.64,0,.44,0,.55H2.17v-7H13.83ZM9.75,10.33h2.72a.19.19,0,0,0,.2-.19V9.75a.19.19,0,0,0-.2-.19H9.75a.18.18,0,0,0-.19.19v.39A.18.18,0,0,0,9.75,10.33Zm0-1.55h2.72a.2.2,0,0,0,.2-.2V8.19a.2.2,0,0,0-.2-.19H9.75a.19.19,0,0,0-.19.19v.39A.19.19,0,0,0,9.75,8.78Zm0-1.56h2.72a.2.2,0,0,0,.2-.19V6.64a.2.2,0,0,0-.2-.2H9.75a.19.19,0,0,0-.19.2V7A.19.19,0,0,0,9.75,7.22ZM5.67,9.56A1.56,1.56,0,1,0,4.11,8,1.56,1.56,0,0,0,5.67,9.56Z""/></svg>" )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.SSN )]
    public class SSNFieldType : FieldType
    {

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            string ssn = UnencryptAndClean( privateValue );

            if ( ssn.Length == 9 )
            {
                return string.Format( "xxx-xx-{0}", ssn.Substring( 5, 4 ) );
            }

            return string.Empty;
        }

        /// <summary>
        /// Setting to determine whether the value from this control is sensitive.  This is used for determining
        /// whether or not the value of this attribute is logged when changed.
        /// </summary>
        /// <returns>
        ///   <c>false</c> By default, any field is not sensitive.
        /// </returns>
        public override bool IsSensitive()
        {
            return true;
        }

        #endregion

        #region Edit Control

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            // Return the masked value so we aren't sending the full value.
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return UnencryptAndClean( privateValue );
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            return Security.Encryption.EncryptString( publicValue );
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
            string ssn = UnencryptAndClean( value );
            if ( ssn.Length == 9 )
            {
                message = string.Empty;
                return true;
            }

            message = "The input provided is not a valid SSN number.";
            return true;
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return false;
        }

        #endregion

        /// <summary>
        /// Unencrypts and strips any non-numeric characters from value.
        /// </summary>
        /// <param name="encryptedValue">The encrypted value.</param>
        /// <returns></returns>
        public static string UnencryptAndClean( string encryptedValue )
        {
            if ( encryptedValue.IsNotNullOrWhiteSpace() )
            {
                string ssn = Rock.Security.Encryption.DecryptString( encryptedValue );
                if ( !string.IsNullOrEmpty( ssn ) )
                {
                    return ssn.AsNumeric();
                    ;
                }
            }

            return string.Empty;
        }

        #region WebForms
#if WEBFORMS

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
            return GetTextValue( value, configurationValues.ToDictionary( k => k.Key, k => k.Value.Value ) );
        }

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new SSNBox { ID = id };
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control != null && control is SSNBox )
            {
                return ( ( SSNBox ) control ).TextEncrypted;
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
            if ( control != null && control is SSNBox )
            {
                ( ( SSNBox ) control ).TextEncrypted = value;
            }
        }

        // Note: Even though this is a 'text' type field, the comparisons like 'Starts with', 'Contains', etc. can't be performed
        // on the encrypted text. Every time the same value is encrypted, the value is different. So a binary comparison cannot be performed.

        /// <summary>
        /// Creates the control needed to filter (query) values using this field type.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, Rock.Reporting.FilterMode filterMode )
        {
            // This field type does not support filtering
            return null;
        }

#endif
        #endregion
    }
}