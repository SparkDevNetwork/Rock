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
using System.Collections.Generic;
using System.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to edit text in Markdown format and rendered as processed Markdown
    /// </summary>
    public class MarkdownFieldType : MemoFieldType
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
            var result = value.ConvertMarkdownToHtml();

            if ( condensed )
            {
                return result.Truncate( 100 );
            }

            return result;
        }

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
            var result = value.ConvertMarkdownToHtml();

            if ( condensed )
            {
                return result.Truncate( 100 );
            }

            return result;
        }

        #endregion

        #region Edit Control

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
            var mdEditor = new MarkdownEditor { ID = id };
            int? rows = 3;
            bool allowHtml = false;

            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( NUMBER_OF_ROWS ) )
                {
                    rows = configurationValues[NUMBER_OF_ROWS].Value.AsIntegerOrNull() ?? 3;
                }
                if ( configurationValues.ContainsKey( ALLOW_HTML ) )
                {
                    allowHtml = configurationValues[ALLOW_HTML].Value.AsBoolean();
                }
            }

            mdEditor.Rows = rows.HasValue ? rows.Value : 3;
            mdEditor.ValidateRequestMode = allowHtml ? ValidateRequestMode.Disabled : ValidateRequestMode.Enabled;

            return mdEditor;
        }

        #endregion
    }
}