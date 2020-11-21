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

using System;

namespace Rock.Attribute
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Attribute.FieldAttribute" />
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class DocumentTypeFieldAttribute : FieldAttribute
    {
        private const string ALLOW_MULTIPLE_KEY = "allowmultiple";

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentTypeFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        /// <param name="fieldTypeClass">The field type class.</param>
        /// <param name="fieldTypeAssembly">The field type assembly.</param>
        public DocumentTypeFieldAttribute( string name, string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null, string fieldTypeClass = null, string fieldTypeAssembly = "Rock" )
            : base( name, description, required, defaultValue, category, order, key, typeof(Rock.Field.Types.DocumentTypeFieldType).FullName, fieldTypeAssembly )
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether [allow multiple].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow multiple]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowMultiple
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( ALLOW_MULTIPLE_KEY ).AsBoolean();
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( ALLOW_MULTIPLE_KEY, new Field.ConfigurationValue( value.ToString() ) );
            }
        }
    }
}
