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

using Rock.Attribute;

namespace org.newpointe.RockU.Attribute
{
    /// <summary>
    /// Field Attribute to select 0 or 1 EventCalendar
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class EventItemOccurrenceFieldAttribute : FieldAttribute
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="EventItemOccurrenceFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultEventItemId">The default event item identifier.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public EventItemOccurrenceFieldAttribute( string name = "EventItemOccurrence", string description = "", bool required = true, string defaultEventItemOccurrenceId = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultEventItemOccurrenceId, category, order, key, typeof( Field.Types.EventItemOccurrenceFieldType ).FullName, typeof( Field.Types.EventItemOccurrenceFieldType ).Assembly.GetName().Name )
        {
        }
    }
}