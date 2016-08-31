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

namespace Rock.Extension
{
    /// <summary>
    /// Helper class for wrapping the properties of a MEF class to use in databinding
    /// </summary>
    public class ComponentDescription
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ComponentDescription"/> is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentDescription" /> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="service">The service.</param>
        public ComponentDescription( int id, KeyValuePair<string, Component> service )
        {
            Id = id;

            Type type = service.Value.GetType();

            Name = service.Key;
            Order = service.Value.Order;
            IsActive = service.Value.IsActive;
            Type = type;

            // Look for a DescriptionAttribute on the class and if found, use its value for the description
            // property of this class
            var descAttributes = type.GetCustomAttributes( typeof( System.ComponentModel.DescriptionAttribute ), false );
            if ( descAttributes != null )
            {
                foreach ( System.ComponentModel.DescriptionAttribute descAttribute in descAttributes )
                {
                    Description = descAttribute.Description;
                }
            }
        }
    }
}