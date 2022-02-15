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

namespace Rock.ViewModel.Blocks.Workflow.FormBuilder
{
    /// <summary>
    /// Contains details about a confirmation e-mail for a Form Builder form.
    /// This specifies if one should be sent, who receives it and the content
    /// it will contain.
    /// </summary>
    public class FormConfirmationEmailViewModel
    {
        /// <summary>
        /// Specifies if the confirmation e-mail has been enabled and should be
        /// sent.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Specifies which workflow attribute will be used to determine the
        /// recipient of the confirmation e-mail.
        /// </summary>
        public Guid? RecipientAttributeGuid { get; set; }

        /// <summary>
        /// Determines how the content of the e-mail will be generated.
        /// </summary>
        public FormEmailSourceViewModel Source { get; set; }
    }
}
