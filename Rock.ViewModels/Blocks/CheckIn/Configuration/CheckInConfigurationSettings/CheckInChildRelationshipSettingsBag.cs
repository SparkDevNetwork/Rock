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

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInConfigurationSettings
{
    /// <summary>
    /// The child relationship settings for a check-in configuration. Configures the relationship types that
    /// are displayed when adding a child, and specifies which of those add the child to the parent's family
    /// and which add them to a new family with a relationship back to the parent's family.
    /// </summary>
    public class CheckInChildRelationshipSettingsBag
    {
        /// <summary>
        /// Gets or sets the known relationship types that are displayed when adding a child during
        /// registration.
        /// </summary>
        public List<string> ChildRelationshipTypes { get; set; }

        /// <summary>
        /// Gets or sets the known relationship types for which the child is added to the parent's existing
        /// family.
        /// </summary>
        public List<string> AddChildToParentsFamilyRelationshipTypes { get; set; }

        /// <summary>
        /// Gets or sets the known relationship types for which the child is added to a new family with a
        /// relationship back to the parent's family.
        /// </summary>
        public List<string> AddChildToNewFamilyRelationshipTypes { get; set; }
    }
}
