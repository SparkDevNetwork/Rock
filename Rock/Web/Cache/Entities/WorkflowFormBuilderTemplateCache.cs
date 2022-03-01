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
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Field;
using Rock.Model;
using Rock.Workflow.FormBuilder;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Cache for <see cref="Rock.Model.WorkflowFormBuilderTemplate"/>
    /// </summary>
    [Serializable]
    [DataContract]
    public class WorkflowFormBuilderTemplateCache : ModelCache<WorkflowFormBuilderTemplateCache, WorkflowFormBuilderTemplate>
    {
        #region Properties

        /// <inheritdoc cref="WorkflowFormBuilderTemplate.Name"/>
        [DataMember]
        public string Name { get; private set; }

        /// <inheritdoc cref="WorkflowFormBuilderTemplate.Description"/>
        [DataMember]
        public string Description { get; private set; }

        /// <inheritdoc cref="WorkflowFormBuilderTemplate.IsActive"/>
        [DataMember]
        public bool IsActive { get; private set; } = true;

        /// <inheritdoc cref="WorkflowFormBuilderTemplate.FormHeader"/>
        [DataMember]
        public string FormHeader { get; private set; }

        /// <inheritdoc cref="WorkflowFormBuilderTemplate.FormFooter"/>
        [DataMember]
        public string FormFooter { get; private set; }

        /// <inheritdoc cref="WorkflowFormBuilderTemplate.AllowPersonEntry"/>
        [DataMember]
        public bool AllowPersonEntry { get; private set; }

        /// <inheritdoc cref="WorkflowFormBuilderTemplate.PersonEntrySettingsJson"/>
        [DataMember]
        public string PersonEntrySettingsJson
        {
            get => PersonEntrySettings?.ToJson();
            private set => PersonEntrySettings = value?.FromJsonOrNull<FormPersonEntrySettings>();
        }

        /// <inheritdoc cref="WorkflowFormBuilderTemplate.ConfirmationEmailSettingsJson"/>
        [DataMember]
        public string ConfirmationEmailSettingsJson
        {
            get => ConfirmationEmailSettings?.ToJson();
            private set => ConfirmationEmailSettings = value?.FromJsonOrNull<FormConfirmationEmailSettings>();
        }

        /// <inheritdoc cref="WorkflowFormBuilderTemplate.CompletionSettingsJson"/>
        [DataMember]
        public string CompletionSettingsJson
        {
            get => CompletionActionSettings?.ToJson();
            private set => CompletionActionSettings = value?.FromJsonOrNull<FormCompletionActionSettings>();
        }

        /// <inheritdoc cref="FormPersonEntrySettings"/>
        public FormPersonEntrySettings PersonEntrySettings { get; private set; }

        /// <inheritdoc cref="FormConfirmationEmailSettings"/>
        public FormConfirmationEmailSettings ConfirmationEmailSettings { get; private set; }

        /// <inheritdoc cref="FormCompletionActionSettings"/>
        public FormCompletionActionSettings CompletionActionSettings { get; private set; }

        /// <inheritdoc cref="WorkflowFormBuilderTemplate.IsLoginRequired"/>
        [DataMember]
        public bool IsLoginRequired { get; private set; }

        #endregion Properties

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            WorkflowFormBuilderTemplate workflowFormBuilderTemplate = entity as WorkflowFormBuilderTemplate;
            if ( workflowFormBuilderTemplate == null )
            {
                return;
            }

            this.Name = workflowFormBuilderTemplate.Name;
            this.Description = workflowFormBuilderTemplate.Name;
            this.IsActive = workflowFormBuilderTemplate.IsActive;
            this.FormHeader = workflowFormBuilderTemplate.FormHeader;
            this.FormFooter = workflowFormBuilderTemplate.FormFooter;
            this.AllowPersonEntry = workflowFormBuilderTemplate.AllowPersonEntry;
            this.PersonEntrySettingsJson = workflowFormBuilderTemplate.PersonEntrySettingsJson;
            this.ConfirmationEmailSettingsJson = workflowFormBuilderTemplate.ConfirmationEmailSettingsJson;
            this.CompletionSettingsJson = workflowFormBuilderTemplate.CompletionSettingsJson;
        }
    }
}
