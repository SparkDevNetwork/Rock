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
using Rock.Field.Types;

namespace Rock.Attribute
{
    /// <summary>
    /// Attribute to specify AI Provider field.
    /// </summary>
    /// <seealso cref="Rock.Attribute.FieldAttribute" />
    public class AIProviderFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AIProviderFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="description">The description of the attribute.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value of the attribute.</param>
        /// <param name="category">The category of the attribute.</param>
        /// <param name="order">The order of the attribute.</param>
        /// <param name="key">The key of the attribute.</param>
        public AIProviderFieldAttribute( string name, string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null ) :
            base( name, description, required, defaultValue, category, order, key, typeof( AIProviderFieldType ).FullName )
        {

        }
    }
}