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

namespace Rock.ViewModels.CheckIn.Labels
{
    /// <summary>
    /// The configuration options for a text field.
    /// </summary>
    public class TextFieldConfigurationBag
    {
        /// <summary>
        /// The key that identifies the data source to use for the value to
        /// display in the text field.
        /// </summary>
        public string SourceKey { get; set; }

        /// <summary>
        /// The key that identifiers the option to use in the data source
        /// formatter. This may be null.
        /// </summary>
        public string FormatterOptionKey { get; set; }

        /// <summary>
        /// How to format the collection when multiple values are provided by
        /// the data source.
        /// </summary>
        /// <value>
        /// This should be a string containing the integer value of the
        /// TextCollectionFormat enumeration.
        /// </value>
        public string CollectionFormat { get; set; }

        /// <summary>
        /// The placeholder text to use when the text field uses either dynamic
        /// Lava or a data source. This is displayed in the designer as well as
        /// when generating the preview.
        /// </summary>
        public string PlaceholderText { get; set; }

        /// <summary>
        /// Determines if the custom text field is using the dynamic text
        /// template or the static text template.
        /// </summary>
        /// <value>
        /// This should be the string "False" or "True".
        /// </value>
        public string IsDynamicText { get; set; }

        /// <summary>
        /// The static text to use for a custom text field.
        /// </summary>
        public string StaticText { get; set; }

        /// <summary>
        /// The dynamic text template to use for a custom text field. The text
        /// is used as a lava template to generate the final text to render.
        /// </summary>
        public string DynamicTextTemplate { get; set; }

        /// <summary>
        /// The default font size to use when rendering text. This is in
        /// points per inch with 72 meaning 1 inch in height.
        /// </summary>
        /// <value>
        /// This is a string value representing a floating point number.
        /// </value>
        public string FontSize { get; set; }

        /// <summary>
        /// Contains a lookup table of string length and font sizes as key and
        /// value pairs. The key represents the string length and the value is
        /// the font size. The value from the largest key that is less than or
        /// equal to the length of the text is used.
        /// </summary>
        /// <value>
        /// This is encoded as a set "key=value" pairs joined by semi-colon.
        /// Such as <c>10=16;20=14;30=12</c>.
        /// </value>
        public string AdaptiveFontSize { get; set; }

        /// <summary>
        /// The horizontal alignment of the text in the field.
        /// </summary>
        /// <value>
        /// The value should be a string containing the integer value of the
        /// HorizontalTextAlignment enumeration.
        /// </value>
        public string HorizontalAlignment { get; set; }

        /// <summary>
        /// Determines if the text is drawn bolder than normal text.
        /// </summary>
        /// <value>
        /// This should be the string "False" or "True".
        /// </value>
        public string IsBold { get; set; }

        /// <summary>
        /// Determines if the color is inverted when drawing. On Zebra printers
        /// will result in an already black background turning white and an
        /// already white background turning black. Other printers may simply
        /// switch to white mode.
        /// </summary>
        /// <value>
        /// This should be the string "False" or "True".
        /// </value>
        public string IsColorInverted { get; set; }

        /// <summary>
        /// Determines if the text is drawn in a more condensed font than
        /// normal text.
        /// </summary>
        /// <value>
        /// This should be the string "False" or "True".
        /// </value>
        public string IsCondensed { get; set; }

        /// <summary>
        /// The maximum length of the text in the field. Any text of greater
        /// length will be truncated without any ellipsis. For collections
        /// of strings this maximum applies to each individual string.
        /// </summary>
        /// <value>
        /// The value is a string representation of an integer.
        /// </value>
        public string MaxLength { get; set; }
    }
}
