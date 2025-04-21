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
using System.Collections.Generic;
using System.Linq;

using Rock.Attribute;
using Rock.Reporting;
using Rock.Web.UI.Controls;
#if WEBFORMS
using System.Web.UI.WebControls;
using System.Web.UI;
#endif

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a currency amount
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.Advanced )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [IconSvg( @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><path d=""M11.88,10.78c-.25,1.31-1.47,2.11-3.22,2.23v1.33A.66.66,0,0,1,8,15a.65.65,0,0,1-.65-.66V13l-.28,0a13.63,13.63,0,0,1-2.22-.57l-.33-.11A.66.66,0,1,1,4.92,11l.33.1a11.61,11.61,0,0,0,2,.53c1.52.21,3.14-.05,3.34-1.12s-.52-1.33-2.75-1.9L7.4,8.52c-1.31-.34-3.74-1-3.28-3.3C4.37,3.91,5.59,3.11,7.35,3V1.66A.65.65,0,0,1,8,1a.66.66,0,0,1,.66.66V3l.27,0a12.45,12.45,0,0,1,1.72.41.66.66,0,1,1-.38,1.26,10.14,10.14,0,0,0-1.52-.37C7.23,4.14,5.61,4.4,5.4,5.47c-.16.86.38,1.27,2.33,1.78l.43.11C9.92,7.81,12.34,8.43,11.88,10.78Z""/></svg>" )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.CURRENCY )]
    public class CurrencyFieldType : DecimalFieldType
    {
        /// <summary>
        /// Gets or sets the currency code defined value id.
        /// </summary>
        /// <value>
        /// The currency code defined value identifier.
        /// </value>
        public int? CurrencyCodeDefinedValueId { get; set; }

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string value, Dictionary<string, string> configurationValues )
        {
            if ( string.IsNullOrWhiteSpace( value ) )
            {
                return string.Empty;
            }

            return value.AsDecimal().FormatAsCurrency( CurrencyCodeDefinedValueId );
        }

        #endregion

        #region Edit Control

        /// <summary>
        /// Tests the value to ensure that it is a valid value.  If not, message will indicate why
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="message">The message.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is valid; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsValid( string value, bool required, out string message )
        {
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                decimal result;
                if ( !decimal.TryParse( value, out result ) )
                {
                    message = "The input provided is not a valid currency value.";
                    return true;
                }
            }

            return base.IsValid( value, required, out message );
        }

        #endregion

        #region Filter Control

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
                return ComparisonHelper.NumericFilterComparisonTypes;
            }
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Gets the align value that should be used when displaying value
        /// </summary>
        public override HorizontalAlign AlignValue
        {
            get
            {
                return HorizontalAlign.Right;
            }
        }

        /// <summary>
        /// Formats the value.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( k => k.Key, k => k.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( k => k.Key, k => k.Value.Value ) );
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
            return new CurrencyBox { ID = id };
        }

        /// <summary>
        /// Returns the value using the most appropriate datatype
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override object ValueAsFieldType( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            return value.AsDecimalOrNull();
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
            // return ValueAsFieldType which returns the value as a Decimal
            return this.ValueAsFieldType( parentControl, value, configurationValues );
        }

#endif
        #endregion

    }
}