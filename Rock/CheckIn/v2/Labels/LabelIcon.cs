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

namespace Rock.CheckIn.v2.Labels
{
    /// <summary>
    /// Identifies a single icon type that can be displayed on a check-in label.
    /// </summary>
    internal class LabelIcon
    {
        /// <summary>
        /// The standard set of icons available to use in labels.
        /// </summary>
        public static List<LabelIcon> StandardIcons { get; } = CreateStandardIcons();

        /// <summary>
        /// The value to store in the field configuration.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// The name of the icon to display in the UI.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The character code to use when rendering the icon.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// <c>true</c> if the bold font should be used; otherwise <c>false</c>.
        /// </summary>
        public bool IsBold { get; }

        /// <summary>
        /// Initializes a new instance of the LabelIcon class.
        /// </summary>
        /// <param name="value">The value to store in the field configuration.</param>
        /// <param name="name">The name of the icon to display in the UI.</param>
        /// <param name="code">The character code to use when rendering the icon.</param>
        /// <param name="isBold"><c>true</c> if the bold font should be used; otherwise <c>false</c>.</param>
        public LabelIcon( string value, string name, string code, bool isBold )
        {
            Value = value;
            Name = name;
            Code = code;
            IsBold = isBold;
        }

        /// <summary>
        /// Creates the standard set of icons available to labels.
        /// </summary>
        /// <returns>A list of icons.</returns>
        private static List<LabelIcon> CreateStandardIcons()
        {
            return new List<LabelIcon>
            {
                new LabelIcon( "birthday-cake", "Birthday Cake", "\uF1FD", true ),
                new LabelIcon( "briefcase-medical", "Briefcase Medical", "\uF469", true ),
                new LabelIcon( "gift", "Gift", "\uF06B", true),
                new LabelIcon( "star","Star", "\uF005", true ),
                new LabelIcon( "star-of-life", "Star of Life", "\uF621", true )
            };
        }
    }
}
