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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Prayer.PrayerRequestDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class PrayerRequestBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets a flag indicating  whether or not comments can be made against the request.
        /// </summary>
        public bool? AllowComments { get; set; }

        /// <summary>
        /// Gets or sets a description of the way that God has answered the prayer.
        /// </summary>
        public string Answer { get; set; }

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        public ListItemBag Campus { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.Category that this prayer request belongs to.
        /// </summary>
        public ListItemBag Category { get; set; }

        /// <summary>
        /// Gets or sets the email address of the person requesting prayer.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the date that the prayer request expires. 
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the First Name of the person that this prayer request is about. This property is required.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the number of times this request has been flagged.
        /// </summary>
        public int? FlagCount { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        public ListItemBag Group { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this prayer request is active.
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the prayer request has been approved. 
        /// </summary>
        public bool? IsApproved { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating whether or not the request is public.
        /// </summary>
        public bool? IsPublic { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this is an urgent prayer request.
        /// </summary>
        public bool? IsUrgent { get; set; }

        /// <summary>
        /// Gets or sets the Last Name of the person that this prayer request is about. This property is required.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the number of times that this prayer request has been prayed for.
        /// </summary>
        public int? PrayerCount { get; set; }

        /// <summary>
        /// Gets or sets the requested by person alias.
        /// </summary>
        public ListItemBag RequestedByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the text/content of the request.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets the FullName as it needs to be sent to the frontend
        /// </summary>
        public string FullName { get; set; }
    }
}
