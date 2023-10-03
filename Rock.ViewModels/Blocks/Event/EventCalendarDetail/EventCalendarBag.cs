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

using Rock.ViewModels.Utility;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Event.EventCalendarDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class EventCalendarBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the Description of the EventCalendar.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class name for a font vector based icon.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is index enabled.
        /// </summary>
        public bool IsIndexEnabled { get; set; }

        /// <summary>
        /// Gets or sets the Name of the EventCalendar. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ExportFeedUrl { get; set; }

        /// <summary>
        /// Gets or sets the saved content channels.
        /// </summary>
        /// <value>
        /// The saved content channels.
        /// </value>
        public List<ListItemBag> SavedContentChannels { get; set; }

        /// <summary>
        /// Gets or sets the content channels.
        /// </summary>
        /// <value>
        /// The content channels.
        /// </value>
        public List<ListItemBag> ContentChannels { get; set; }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public List<EventAttributeBag> EventAttributes { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether the current user has Administrate Authorization.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can administrate; otherwise, <c>false</c>.
        /// </value>
        public bool CanAdministrate { get; set; }
    }
}
