// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Web.UI;

using Rock.Security;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a text value
    /// </summary>
    [Serializable]
    public class EncryptedTextFieldType : TextFieldType
    {

        #region Formatting

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
            return base.FormatValue( parentControl, Encryption.DecryptString( value ), configurationValues, condensed );
        }

        #endregion

        #region Edit Control

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            return Encryption.EncryptString( base.GetEditValue( control, configurationValues ) );
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            base.SetEditValue( control, configurationValues, Encryption.DecryptString( value ) );
        }

        #endregion

        #region Filter Control

        // Note: Even though this is a 'text' type field, the base default binary comparison is used instead of being overridden with 
        // string comparison type like other 'text' fields, because comparisons like 'Starts with', 'Contains', etc. can't be performed
        // on the encrypted text.  Only a binary comparison can be performed.

        #endregion

    }
}