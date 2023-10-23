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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Event.EventItemDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class EventItemBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the approved by Rock.Model.PersonAlias.
        /// </summary>
        public ListItemBag ApprovedByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the date this event was approved.
        /// </summary>
        public DateTime? ApprovedOnDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Description of the EventItem.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the URL for an external event.
        /// </summary>
        public string DetailsUrl { get; set; }

        /// <summary>
        /// Gets or sets the is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the event has been approved.
        /// </summary>
        public bool IsApproved { get; set; }

        /// <summary>
        /// Gets or sets the Name of the EventItem. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.BinaryFile that contains the EventItem's photo.
        /// </summary>
        public ListItemBag Photo { get; set; }

        /// <summary>
        /// Gets or sets the Summary of the EventItem.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the event occurence attributes.
        /// </summary>
        /// <value>
        /// The event occurence attributes.
        /// </value>
        public List<EventItemOccurenceAttributeBag> EventOccurenceAttributes { get; set; }

        /// <summary>
        /// Gets or sets the audiences.
        /// </summary>
        /// <value>
        /// The audiences.
        /// </value>
        public List<ListItemBag> Audiences { get; set; }

        /// <summary>
        /// Gets or sets the available calendars.
        /// </summary>
        /// <value>
        /// The available calendars.
        /// </value>
        public List<ListItemBag> AvailableCalendars { get; set; }

        /// <summary>
        /// Gets or sets the calendars.
        /// </summary>
        /// <value>
        /// The calendars.
        /// </value>
        public List<string> Calendars { get; set; }

        /// <summary>
        /// Gets or sets the selected calendar names.
        /// </summary>
        /// <value>
        /// The selected calendar names.
        /// </value>
        public List<string> SelectedCalendarNames { get; set; }

        /// <summary>
        /// Gets or sets the photo URL.
        /// </summary>
        /// <value>
        /// The photo URL.
        /// </value>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the event calendar item attributes.
        /// </summary>
        /// <value>
        /// The event calendar item attributes.
        /// </value>
        public List<EventCalendarItemAttributeBag> EventCalendarItemAttributes { get; set; }

        /// <summary>
        /// Gets or sets the approval text.
        /// </summary>
        /// <value>
        /// The approval text.
        /// </value>
        public string ApprovalText { get; set; }
    }
}
