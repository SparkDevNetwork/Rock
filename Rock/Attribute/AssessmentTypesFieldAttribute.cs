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
    /// Field Attribute to select 0 or more Assessments stored as a comma-delimited list of Assessments.Guid
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class AssessmentTypesFieldAttribute : SelectFieldAttribute
    {
        private const string INCLUDE_INACTIVE_KEY = "includeInactive";

        /// <summary>
        /// Initializes a new instance of the <see cref="AssessmentTypesFieldAttribute"/> class.
        /// Set values using object initializers.
        /// </summary>
        /// <param name="name">The name.</param>
        public AssessmentTypesFieldAttribute( string name )
            : base( name, description: "", required: true, defaultValue: "", category: "", order: 0, key: null, fieldTypeClass: typeof( Rock.Field.Types.AssessmentTypesFieldType ).FullName )
        {
            IncludeInactive = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [include inactive].
        /// The constructor sets a default of false.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include inactive]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeInactive {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( INCLUDE_INACTIVE_KEY ).AsBoolean();
            }
            set
            {
                FieldConfigurationValues.AddOrReplace( INCLUDE_INACTIVE_KEY , new Field.ConfigurationValue( value.ToTrueFalse() ) );
            }
        }
    }
}