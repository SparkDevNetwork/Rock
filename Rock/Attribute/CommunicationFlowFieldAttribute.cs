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

using Rock.Enums.Communication;
using Rock.Model;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to select a <see cref="CommunicationFlow" />. Stored as the CommunicationFlow's Guid.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class CommunicationFlowFieldAttribute : FieldAttribute
    {
        private const string FILTER_TRIGGER_TYPES = "filterTriggerTypes";
            
        /// <summary>
        /// Gets or sets the campus types filter.
        /// </summary>
        /// <value>
        /// The campus types filter.
        /// </value>
        public CommunicationFlowTriggerType[] FilterTriggerTypes
        {
            get
            {
                return ( FieldConfigurationValues.GetValueOrNull( FILTER_TRIGGER_TYPES ) ?? string.Empty )
                    .SplitDelimitedValues(",")
                    .AsIntegerList()
                    .Select( t => ( CommunicationFlowTriggerType ) t )
                    .ToArray();
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( FILTER_TRIGGER_TYPES, new Field.ConfigurationValue( ( value ?? new CommunicationFlowTriggerType[] { } ).Select( t => ( int ) t ).ToList().AsDelimited( "," ) ) );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationFlowFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">If set to <see langword="true"/> [required].</param>
        /// <param name="defaultCommunicationFlowGuid">The default communication flow unique identifier.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public CommunicationFlowFieldAttribute( string name, string description = "", bool required = true, string defaultCommunicationFlowGuid = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultCommunicationFlowGuid, category, order, key, typeof( Rock.Field.Types.CommunicationFlowFieldType ).FullName )
        {
        }
    }
}