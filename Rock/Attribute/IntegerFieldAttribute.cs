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
using System.Linq;
using System.Web;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to set an integer.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class IntegerFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntegerFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public IntegerFieldAttribute( string name, string description = "", bool required = true, int defaultValue = int.MinValue, string category = "", int order = 0, string key = null )
            : base( name, description, required, ( defaultValue == int.MinValue ? "" : defaultValue.ToString() ), category, order, key, typeof( Rock.Field.Types.IntegerFieldType ).FullName )
        {
        }

        /// <summary>
        /// Gets or sets the default integer value of the attribute.  This is the value that will be used if a specific value has not yet been created
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public int DefaultIntegerValue
        {
            get
            {
                return base.DefaultValue.AsIntegerOrNull() ?? int.MinValue;
            }

            set
            {
                base.DefaultValue = value == int.MinValue ? string.Empty : value.ToString();
            }
        }

    }
}