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

namespace Rock.Enums.Reporting
{
    /// <summary>
    /// The sources of current value that can be used when building custom
    /// filtering logic.This currently has only Attribute, but would later be
    /// expanded to include other sources (such as properties).
    /// </summary>
    public enum FieldFilterSourceType
    {
        /// <summary>
        /// The value will be read from an entity attribute value.
        /// </summary>
        Attribute = 0,

        /// <summary>
        /// The value will be read from an entity property.
        /// </summary>
        Property = 1,
    }
}
