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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInAreasAndGroups
{
    /// <summary>
    /// The editable detail of a check-in group, exchanged with the Group editor in the right pane. Also used as the
    /// payload shape for both saving an existing group and creating a new one (the server treats <see cref="IdKey"/>
    /// as the discriminator).
    /// </summary>
    public class GroupDetailBag
    {
        /// <summary>
        /// Gets or sets the hashed identifier of the underlying group, or null/empty for a new (unsaved) group.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the group name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this group is flagged as a special-needs group. Only used by
        /// next-gen check-in.
        /// </summary>
        public bool IsSpecialNeeds { get; set; }

        /// <summary>
        /// Gets or sets the display name of the ancestor area that owns this group.
        /// </summary>
        public string ParentAreaName { get; set; }

        /// <summary>
        /// Gets or sets the name of the area's "Inherit Check-in Setup Type From" selection. Suffixed onto the
        /// "Check-in Filters" section title so the individual can see which setup type drives the filter attributes.
        /// Null when the area inherits from nothing.
        /// </summary>
        public string InheritedGroupTypeName { get; set; }

        /// <summary>
        /// Gets or sets the main (non-overflow) named locations attached to this group, in display order.
        /// </summary>
        public List<NamedLocationBag> Locations { get; set; }

        /// <summary>
        /// Gets or sets the overflow named locations attached to this group, in display order. Overflow locations
        /// are only honored by next-gen check-in: when every main location is at capacity, attendees spill into
        /// overflow locations in order.
        /// </summary>
        public List<NamedLocationBag> OverflowLocations { get; set; }

        /// <summary>
        /// Gets or sets the public attribute schema for the group's check-in filter attributes, keyed by attribute
        /// key. These are inherited from an ancestor group type (the check-in setup type the area inherits from) and
        /// drive the "Check-in Filters" section.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the public attribute schema for the group's own attributes, keyed by attribute key. These are
        /// defined directly on the group's own group type (not inherited) and drive the "Group Attributes" section.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> GroupAttributes { get; set; }

        /// <summary>
        /// Gets or sets the current values for all of the group's attributes (both filter and own), keyed by
        /// attribute key.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }
    }
}
