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

using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.GroupPlacement
{
    /// <summary>
    /// Contains all the initialization data needed to configure and display the group placement block,
    /// including placement settings, roles, attributes, context, and navigation.
    /// </summary>
    public class GroupPlacementInitializationBox
    {
        /// <summary>
        /// Contextual keys that define the placement scope.
        /// </summary>
        public GroupPlacementKeysBag GroupPlacementKeys { get; set; }

        /// <summary>
        /// A list of group type roles that are available for destination groups during placement.
        /// </summary>
        public List<DestinationGroupTypeRoleBag> DestinationGroupTypeRoles { get; set; }

        /// <summary>
        /// The title to be displayed on the placement interface.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// A set of configuration options that influence how placement is performed and displayed.
        /// </summary>
        public PlacementConfigurationSettingOptionsBag PlacementConfigurationSettingOptions { get; set; }

        /// <summary>
        /// Indicates the mode of placement.
        /// </summary>
        public PlacementMode PlacementMode { get; set; }

        /// <summary>
        /// Indicates whether multiple group placements are allowed per person.
        /// </summary>
        public bool IsPlacementAllowingMultiple { get; set; }

        /// <summary>
        /// The source person passed in by page parameters, typically the registrant or individual being placed.
        /// </summary>
        public PersonBag SourcePerson { get; set; }

        /// <summary>
        /// Attributes that can be applied to new groups added during placement.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> AttributesForGroupAdd { get; set; }

        /// <summary>
        /// Gets or sets the navigation URLs for common actions like saving, canceling, or navigating to other blocks.
        /// </summary>
        public Dictionary<string, string> NavigationUrls { get; set; }

        /// <summary>
        /// The URL of the page to return to when navigating "back" from the placement UI.
        /// </summary>
        public string BackPageUrl { get; set; }

        /// <summary>
        /// Gets or sets the error message, if any, to be displayed to the user.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// A list of placement options configured for the current registration template.
        /// </summary>
        public List<ListItemBag> RegistrationTemplatePlacements { get; set; }
    }

}
