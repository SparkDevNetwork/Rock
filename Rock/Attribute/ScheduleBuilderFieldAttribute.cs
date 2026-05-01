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
    /// Field Attribute to build an ad-hoc schedule.
    /// Stored as Schedule.iCalendarContent.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class ScheduleBuilderFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleBuilderFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">If set to <c>true</c> the value is required.</param>
        /// <param name="defaultValue">The default iCalendar value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public ScheduleBuilderFieldAttribute( string name = "Schedule", string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.ScheduleBuilderFieldType ).FullName )
        {
        }
    }
}
