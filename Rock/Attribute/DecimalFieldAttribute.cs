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
    /// Field Attribute for setting a non-integer number with decimals.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class DecimalFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecimalFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public DecimalFieldAttribute( string name, string description = "", bool required = true, double defaultValue = double.MinValue, string category = "", int order = 0, string key = null )
            : base( name, description, required, ( defaultValue == double.MinValue ? "" : defaultValue.ToString() ), category, order, key, typeof( Rock.Field.Types.DecimalFieldType ).FullName )
        {
        }
       
        /// <summary>
        /// Gets or sets the default numeric value of the attribute. This is the value that will be used if a specific value has not yet been created.
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public double DefaultDecimalValue
        {
            // NOTE: using double instead decimal because decimal can't be used as a Named parameter
            // https://github.com/dotnet/csharplang/blob/master/spec/attributes.md#attribute-parameter-types
            get
            {
                return base.DefaultValue.AsDoubleOrNull() ?? double.MinValue;
            }

            set
            {
                base.DefaultValue = value == double.MinValue ? string.Empty : value.ToString();
            }
        }
    }
}