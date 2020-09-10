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
    /// Attribute that selects an integer value using a <see cref="Rock.Web.UI.Controls.RangeSlider"/>
    /// </summary>
    /// <seealso cref="Rock.Attribute.FieldAttribute" />
    public class RangeSliderAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecimalRangeFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public RangeSliderAttribute( string name )
            : base( name )
        {
            FieldTypeClass = typeof( Rock.Field.Types.RangeSliderFieldType ).FullName;
        }

        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        /// <value>
        /// The minimum value.
        /// </value>
        public int MinValue
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( "min" ).AsIntegerOrNull() ?? int.MaxValue;
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( "min", new Field.ConfigurationValue( value.ToString() ) );
            }
        }

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        /// <value>
        /// The maximum value.
        /// </value>
        public int MaxValue
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( "max" ).AsIntegerOrNull() ?? int.MaxValue;
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( "max", new Field.ConfigurationValue( value.ToString() ) );
            }
        }

        /// <summary>
        /// Gets or sets the default integer value of the attribute.  This is the value that will be used if a specific value has not yet been created.
        /// To have a default integer value of null, use <see cref="FieldAttribute.DefaultValue" >DefaultValue</see>  instead and set that to null
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public int DefaultIntegerValue
        {
            get
            {
                // Named Arguments have to have a public get/set and can't be nullable types. So, we have to implement this, even though Rock won't use this get
                return base.DefaultValue.AsIntegerOrNull() ?? int.MinValue;
            }

            set
            {
                // Named arguments can't be nullable types, so use int.MinValue as a magic number to indicate null
                if ( value == int.MinValue )
                {
                    base.DefaultValue = null;
                }
                else
                {
                    base.DefaultValue = value.ToString();
                }
            }
        }
    }
}