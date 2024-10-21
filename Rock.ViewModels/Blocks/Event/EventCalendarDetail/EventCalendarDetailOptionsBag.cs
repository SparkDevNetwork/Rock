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

using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Event.EventCalendarDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class EventCalendarDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public List<EventAttributeBag> EventAttributes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether /[indexing enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [indexing enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool IndexingEnabled { get; set; }
    }
}
