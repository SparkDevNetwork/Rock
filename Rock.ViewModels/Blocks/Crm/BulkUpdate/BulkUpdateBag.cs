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

using Rock.Enums.Communication;
using Rock.Enums.Crm;
using Rock.Model;

namespace Rock.ViewModels.Blocks.Crm.BulkUpdate
{
    /// <summary>
    /// The save payload for a bulk update operation.
    /// </summary>
    public class BulkUpdateBag
    {
        /// <summary>
        /// Gets or sets the list of persons to update.
        /// </summary>
        public List<BulkUpdatePersonBag> UpdatePersons { get; set; }

        /// <summary>
        /// Gets or sets the post update workflow type guids.
        /// </summary>
        public List<Guid> PostUpdateWorkflowTypeGuids { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of fields toggled for update.
        /// </summary>
        /// <remarks>
        /// Only fields with a value of <c>true</c> are applied. This distinguishes
        /// clearing a field from leaving it unchanged.
        /// </remarks>
        public Dictionary<string, bool> UpdatedFields { get; set; }

        #region Core Profile Fields

        /// <summary>
        /// Gets or sets the title value unique identifier.
        /// </summary>
        public Guid? TitleValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the suffix value unique identifier.
        /// </summary>
        public Guid? SuffixValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the connection status value unique identifier.
        /// </summary>
        public Guid? ConnectionStatusValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the record status value unique identifier.
        /// </summary>
        public Guid? RecordStatusValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the record source value unique identifier.
        /// </summary>
        public Guid? RecordSourceValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the inactive reason value unique identifier.
        /// </summary>
        public Guid? InactiveReasonValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the inactive reason note.
        /// </summary>
        public string InactiveReasonNote { get; set; }

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        public Gender? Gender { get; set; }

        /// <summary>
        /// Gets or sets the grade defined value unique identifier.
        /// </summary>
        public Guid? GradeValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the graduation year.
        /// </summary>
        public int? GraduationYear { get; set; }

        /// <summary>
        /// Gets or sets the marital status value unique identifier.
        /// </summary>
        public Guid? MaritalStatusValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the campus unique identifier.
        /// </summary>
        public Guid? CampusGuid { get; set; }

        #endregion Core Profile Fields

        #region Communication Fields

        /// <summary>
        /// Gets or sets the is email active value.
        /// </summary>
        public bool? IsEmailActive { get; set; }

        /// <summary>
        /// Gets or sets the communication preference.
        /// </summary>
        public CommunicationType? CommunicationPreference { get; set; }

        /// <summary>
        /// Gets or sets the email preference.
        /// </summary>
        public EmailPreference? EmailPreference { get; set; }

        /// <summary>
        /// Gets or sets the email note.
        /// </summary>
        public string EmailNote { get; set; }

        #endregion Communication Fields

        #region System Fields

        /// <summary>
        /// Gets or sets the action to perform for following.
        /// </summary>
        public BulkUpdateActionSpecifier? Following { get; set; }

        /// <summary>
        /// Gets or sets the review reason value unique identifier.
        /// </summary>
        public Guid? ReviewReasonValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the review reason note.
        /// </summary>
        public string ReviewReasonNote { get; set; }

        /// <summary>
        /// Gets or sets the system note.
        /// </summary>
        public string SystemNote { get; set; }

        #endregion System Fields

        #region Complex Attributes and Dependencies

        /// <summary>
        /// Gets or sets the person attributes to update.
        /// </summary>
        public Dictionary<string, string> PersonAttributes { get; set; }

        /// <summary>
        /// Gets or sets the group update configuration.
        /// </summary>
        public BulkUpdateGroupBag GroupUpdate { get; set; }

        /// <summary>
        /// Gets or sets the step update configuration.
        /// </summary>
        public BulkUpdateStepBag StepUpdate { get; set; }

        /// <summary>
        /// Gets or sets the note update configuration.
        /// </summary>
        public BulkUpdateNoteBag NoteUpdate { get; set; }

        /// <summary>
        /// Gets or sets the tag update configuration.
        /// </summary>
        public BulkUpdateTagBag TagUpdate { get; set; }

        #endregion Complex Attributes and Dependencies
    }
}
