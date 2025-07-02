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

namespace Rock.ViewModels.Blocks.Group.GroupPlacement
{
    /// <summary>
    /// Represents a collection of contextual keys and identifiers used to scope a group placement session.
    /// </summary>
    public class GroupPlacementKeysBag
    {
        /// <summary>
        /// The encrypted key representing the specific registration template placement being used.
        /// </summary>
        public string RegistrationTemplatePlacementIdKey { get; set; }

        /// <summary>
        /// The encrypted key for the registration instance associated with the placement.
        /// </summary>
        public string RegistrationInstanceIdKey { get; set; }

        /// <summary>
        /// The GUID of the registration instance, if available.
        /// </summary>
        public Guid? RegistrationInstanceGuid { get; set; }

        /// <summary>
        /// The encrypted key for the registration template associated with the placement.
        /// </summary>
        public string RegistrationTemplateIdKey { get; set; }

        /// <summary>
        /// The GUID of the registration template, if available.
        /// </summary>
        public Guid? RegistrationTemplateGuid { get; set; }

        /// <summary>
        /// The encrypted key of the source group where individuals are initially associated.
        /// </summary>
        public string SourceGroupIdKey { get; set; }

        /// <summary>
        /// The encrypted key of the source group type used to categorize or filter source groups.
        /// </summary>
        public string SourceGroupTypeIdKey { get; set; }

        /// <summary>
        /// The GUID of the source group, if available.
        /// </summary>
        public Guid? SourceGroupGuid { get; set; }

        /// <summary>
        /// The encrypted key for the entity set that may define the scope of people or groups involved in the placement.
        /// </summary>
        public string EntitySetIdKey { get; set; }

        /// <summary>
        /// The encrypted key of the destination group type that defines what kind of group people will be placed into.
        /// </summary>
        public string DestinationGroupTypeIdKey { get; set; }
    }

}
