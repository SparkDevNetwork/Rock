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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.SmsPipelineDetail
{
    /// <summary>
    /// A configured SMS action attached to a pipeline.
    /// </summary>
    /// <remarks>
    /// Per-instance attribute values are carried in <see cref="AttributeValues"/>;
    /// the corresponding schema lives on the matching <see cref="SmsActionComponentBag"/>
    /// (resolved by <see cref="ComponentEntityTypeGuid"/>).
    /// </remarks>
    public class SmsActionBag
    {
        /// <summary>
        /// Gets or sets the action's stable IdKey for cross-request reference.
        /// </summary>
        /// <remarks>Null for actions that have not yet been persisted.</remarks>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the action's GUID.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the action's display name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this action processes inbound messages.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether pipeline processing should continue
        /// to subsequent actions after this action successfully handles a message.
        /// </summary>
        public bool ContinueAfterProcessing { get; set; }

        /// <summary>
        /// Gets or sets the date on which the Rock clean-up job should remove this action.
        /// </summary>
        /// <remarks>Null when the action does not expire.</remarks>
        public DateTime? ExpireDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an interaction record is written
        /// each time the action processes a message.
        /// </summary>
        public bool IsInteractionLoggedAfterProcessing { get; set; }

        /// <summary>
        /// Gets or sets the zero-based ordering of this action within the parent pipeline.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the entity type that backs this action's <c>SmsActionComponent</c>.
        /// </summary>
        /// <remarks>Drives which per-instance attribute schema the editor renders.</remarks>
        public Guid ComponentEntityTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the per-instance attribute schema for this action, keyed by attribute key.
        /// </summary>
        /// <remarks>
        /// Populated only for edit; lets the editor render the form without a separate
        /// component-schema lookup. Ignored on save.
        /// </remarks>
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the attribute values for this action, keyed by attribute key.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }
    }
}
