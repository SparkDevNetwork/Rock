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

using System.Collections.Generic;

namespace Rock.Utility.GroupFinder
{
    /// <summary>
    /// A single filter value used by <see cref="GroupFinderHelper"/>.
    /// </summary>
    internal class GroupFinderFilter
    {
        #region Properties

        /// <summary>
        /// Valid values are 'campus', 'attribute', 'dayofweek', 'timeofday', 'meetingstyle'.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// If <see cref="Type"/> is 'attribute', this will be the attributekey
        /// to filter on.
        /// </summary>
        public string Key { get; }

        // attr: 'con', 'sw', 'ew', 'in', 'ne', 'eq'
        // timeofday: 'lte', 'lt', 'gt', 'eq', 'ne', 'gte'
        /// <summary>
        /// The type of filter to apply. Valid values depend on the value of <see cref="Type"/>:
        /// <list type="bullet">
        /// <item><strong>attribute:</strong> 'con', 'sw', 'ew', 'in', 'ne', 'eq'</item>
        /// <item><strong>timeofday:</strong> 'lte', 'lt', 'gt', 'eq', 'ne', 'gte'</item>
        /// </list>
        /// For other <see cref="Type"/> values, this property is ignored.
        /// </summary>
        public string Operator { get; }

        /// <summary>
        /// The content that describes the filter value.
        /// <list type="bullet">
        /// <item><strong>campus:</strong> A comma separated list of campus id numbers.</item>
        /// <item><strong>attribute:</strong> The raw attribute value to filter on for the specified attribute key.</item>
        /// <item><strong>dayofweek:</strong> The day of the week to filter on: 'Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'.</item>
        /// <item><strong>timeofday:</strong> The time of day to filter on, such as '9:00 PM', '5:00 AM'.</item>
        /// </list>
        /// </summary>
        public string Content { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="GroupFinderFilter"/> class.
        /// </summary>
        /// <param name="type">The type of the filter.</param>
        /// <param name="key">The key of the filter.</param>
        /// <param name="operator">The operator of the filter.</param>
        /// <param name="content">The content of the filter.</param>
        public GroupFinderFilter( string type, string key, string @operator, string content )
        {
            Type = type;
            Key = key;
            Operator = @operator;
            Content = content;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="GroupFinderFilter"/> class.
        /// </summary>
        /// <param name="parameters">The Lava parameters that contain the filter values.</param>
        /// <param name="content">The content of the filter.</param>
        public GroupFinderFilter( Dictionary<string, string> parameters, string content )
            : this( parameters.GetValueOrNull( "type" ), parameters.GetValueOrNull( "key" ), parameters.GetValueOrNull( "operator" ), content )
        {
        }

        #endregion
    }
}
