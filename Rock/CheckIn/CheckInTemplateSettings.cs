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
using Rock.Attribute;
using Rock.Enums.CheckIn;
using Rock.Enums.Controls;

namespace Rock.CheckIn
{
    /// <summary>
    /// Stores the configuration data for a Check-in Template group type. This
    /// is used when processing next-gen check-in configuration settings.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal( "17.3" )]
    public class CheckInTemplateSettings
    {
        /// <summary>
        /// Determines the requirement level for displaying the mobile phone
        /// input on children. The field can be set to be hidden, be visible
        /// but optional, or be visible and required.
        /// </summary>
        public RequirementLevel DisplayMobilePhoneOnChildren { get; set; } = RequirementLevel.Optional;

        /// <summary>
        /// Determines for which people the name suffix should be displayed
        /// when adding a new individual.
        /// </summary>
        public AdultsOrChildrenSelectionMode DisplaySuffix { get; set; } = AdultsOrChildrenSelectionMode.AdultsAndChildren;

        /// <summary>
        /// Forces the selection of known relationship types when adding a new
        /// child to a family in the check-in registration process. Normally the
        /// "Child" option is selected by default, but this setting can be used
        /// to require the user to explicitly select a relationship type.
        /// </summary>
        public bool ForceSelectionOfKnownRelationshipType { get; set; }

        /// <summary>
        /// Defines the minimum age for a child to be considered at an age where
        /// the grade is expected to be filled in. If a child is greater than or
        /// equal to this age and a grade has not been filled in, then a dialog
        /// should pop up to prompt the person to either fill in a grade or to
        /// verify that the child is not yet in school.
        /// </summary>
        public decimal? GradeConfirmationAge { get; set; }
    }
}
