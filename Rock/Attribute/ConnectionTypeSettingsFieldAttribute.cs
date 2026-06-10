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

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute that captures a Connection Type plus optional Opportunity,
    /// Status, and Type Source selections in a single composite value.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class ConnectionTypeSettingsFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionTypeSettingsFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The display name for the attribute.</param>
        /// <param name="description">The description shown beneath the editor.</param>
        /// <param name="required">If <c>true</c>, the configuration UI flags an empty value as invalid.</param>
        /// <param name="defaultValue">The persisted default value in the composite pipe-delimited shape.</param>
        /// <param name="category">The attribute category used to group fields in the editor.</param>
        /// <param name="order">The display order within its category.</param>
        /// <param name="key">The attribute key. Defaults to the name with whitespace removed.</param>
        public ConnectionTypeSettingsFieldAttribute( string name = "Connection Type Settings", string description = "", bool required = false, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.ConnectionTypeSettingsFieldType ).FullName )
        {
        }
    }
}
