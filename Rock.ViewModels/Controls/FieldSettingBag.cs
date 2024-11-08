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

using Rock.Enums.Controls;

namespace Rock.ViewModels.Controls
{
    /// <summary>
    /// POCO to store the settings for the fields.
    /// This is copied from Rock/Mobile/JsonFields/FieldSetting.cs. If any changes are made here,
    /// they may need to be copied there as well.
    /// </summary>
    public class FieldSettingBag
    {
        /// <summary>
        /// Creates an identifier based off the key. This is used for grid operations.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id => Key.GetHashCode();

        /// <summary>
        /// Gets or sets the field key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the field value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>
        /// The name of the property.
        /// </value>
        public string FieldName { get; set; }

        /// <summary>
        /// Gets or sets the field source.
        /// </summary>
        /// <value>
        /// The field source.
        /// </value>
        public FieldSource FieldSource { get; set; }

        /// <summary>
        /// Gets or sets the attribute format.
        /// </summary>
        /// <value>
        /// The attribute format.
        /// </value>
        public AttributeFormat AttributeFormat { get; set; }

        /// <summary>
        /// Gets or sets the field format.
        /// </summary>
        /// <value>
        /// The field format.
        /// </value>
        public FieldFormat FieldFormat { get; set; }
    }
}
