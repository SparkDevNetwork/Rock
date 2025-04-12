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

namespace Rock.ViewModels.Blocks.Group.GroupPlacement
{
    /// <summary>
    /// The SMS Conversations Initialization Box
    /// </summary>
    public class GroupPlacementInitializationBox
    {
        public int? RegistrationTemplatePlacementId { get; set; }

        public int? RegistrationInstanceId { get; set; }

        public int? RegistrationTemplateId { get; set; }

        public int? RegistrantId { get; set; }

        public PlacementBag SelectedPlacement { get; set; }

        public PlacementGroupTypeBag PlacementGroupType { get; set; }

        public List<PlacementGroupBag> PlacementGroups { get; set; }

        public List<PersonBag> PeopleToPlace { get; set; }

        //public List<PersonBag> PlacementPeople { get; set; }

        public string Title { get; set; }

        public PlacementConfigurationSettingOptionsBag PlacementConfigurationSettingOptions { get; set; }

        public bool InTemplateMode { get; set; }

        public bool IsPlacementAllowingMultiple { get; set; }

        public List<PlacementGroupTypeRoleBag> PlacementGroupTypeRoles { get; set; }

        //public List<PlacementGroupDetailsBag> PlacementGroupDetails { get; set; }

        /// <summary>
        /// Gets or sets the error message, if any.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
