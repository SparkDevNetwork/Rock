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
    /// Field Attribute to select a System Communication. The selection is stored as a SystemCommunication.Guid value.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class SystemCommunicationFieldAttribute : FieldAttribute
    {
        private const string INCLUDE_INACTIVE_KEY = "includeInactive";

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemCommunicationFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultSystemCommunicationGuid">The default System Communication unique identifier.</param>
        /// <param name="category">The name of the category to which this item is assigned.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public SystemCommunicationFieldAttribute( string name, string description = "", bool required = true, string defaultSystemCommunicationGuid = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultSystemCommunicationGuid, category, order, key, typeof( Rock.Field.Types.SystemCommunicationFieldType ).FullName )
        {

            IncludeInactive = false;
        }


        /// <summary>
        /// Gets or sets the default System Communication template unique identifier.
        /// </summary>
        public string DefaultSystemCommunicationGuid
        {
            get
            {
                return this.DefaultValue;
            }
            set
            {
                this.DefaultValue = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if inactive items will be included.
        /// </summary>
        /// <value>
        ///   <c>true</c> if inactive items will be included; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeInactive
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( INCLUDE_INACTIVE_KEY ).AsBoolean();
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( INCLUDE_INACTIVE_KEY, new Field.ConfigurationValue( value.ToString() ) );
            }
        }
    }
}