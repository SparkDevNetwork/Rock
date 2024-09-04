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

namespace Rock.ViewModels.Reporting
{
    /// <summary>
    /// Identifies a single property that can be used to filter an object. This
    /// is most often used when displaying the filter editor UI.
    /// </summary>
    public class FieldFilterPublicPropertyBag
    {
        /// <summary>
        /// The name of the property.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The title of the property, this is the value that is displayed in
        /// the UI so it should be a friendly version of the name.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The unique identifier of the field type that will handle the UI for
        /// editing the filter value.
        /// </summary>
        public Guid FieldTypeGuid { get; set; }

        /// <summary>
        /// The configuration values that will be passed to the field to allow
        /// it to render the UI control correctly. These must be already encoded
        /// as public configuration values from the field type.
        /// </summary>
        public Dictionary<string, string> ConfigurationValues { get; set; }
    }
}
