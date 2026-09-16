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

namespace Rock.ViewModels.Blocks.WorkFlow.FormBuilder
{
    /// <summary>
    /// Maps a single Form Builder form field onto a target attribute on the
    /// selected Connection Opportunity. A null <see cref="TargetAttributeGuid"/>
    /// signals "append to the Connection Request's Comment field" (default).
    /// </summary>
    public class FormFieldAttributeMappingViewModel
    {
        /// <summary>
        /// The unique identifier of the form attribute (workflow form field)
        /// being mapped.
        /// </summary>
        public Guid FormAttributeGuid { get; set; }

        /// <summary>
        /// The target attribute on the Connection Opportunity. <c>null</c>
        /// means the form field's value is appended to the Connection
        /// Request's Comment field as <c>"{Label}: {Value}"</c>.
        /// </summary>
        public Guid? TargetAttributeGuid { get; set; }
    }
}
