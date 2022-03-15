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

using Rock.ViewModel.NonEntities;

namespace Rock.ViewModel.Blocks.Workflow.FormBuilder
{
    /// <summary>
    /// Contains the general settings about this form. These loosely correlate
    /// to the UI of the General tab when viewing the form.
    /// </summary>
    public class FormGeneralViewModel
    {
        /// <summary>
        /// The name of the form. This is used internally to identify the form
        /// and not normally displayed to the user filling out the form.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A description of the purpose this form fills and the reason it exists.
        /// This is primarily for internal use by staff.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The unique identifier of the template that is being used by this
        /// form to provide a set of overrides.
        /// </summary>
        public Guid? Template { get; set; }

        /// <summary>
        /// The category that this form belongs to for organization purposes.
        /// </summary>
        public ListItemViewModel Category { get; set; }

        /// <summary>
        /// The date and time this form will begin to allow entries.
        /// </summary>
        public DateTimeOffset? EntryStarts { get; set; }

        /// <summary>
        /// The date and time at which point this form will no longer accept new
        /// entries.
        /// </summary>
        public DateTimeOffset? EntryEnds { get; set; }

        /// <summary>
        /// Determines if this form requires the person to be logged in before
        /// they can begin filling it out.
        /// </summary>
        public bool IsLoginRequired { get; set; }
    }
}
