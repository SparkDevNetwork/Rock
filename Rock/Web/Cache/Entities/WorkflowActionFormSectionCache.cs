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

namespace Rock.Web.Cache
{
    /// <summary>
    /// Cache for <see cref="Rock.Model.WorkflowActionFormSection"/>
    /// </summary>
    [Serializable]
    [DataContract]
    public class WorkflowActionFormSectionCache : ModelCache<WorkflowActionFormSectionCache, WorkflowActionFormSection>
    {
        #region Properties

        /// <inheritdoc cref="WorkflowActionFormSection.Title"/>
        [DataMember]
        public string Title { get; private set; }

        /// <inheritdoc cref="WorkflowActionFormSection.Description"/>
        [DataMember]
        public string Description { get; private set; }

        /// <inheritdoc cref="WorkflowActionFormSection.ShowHeadingSeparator"/>
        [DataMember]
        public bool ShowHeadingSeparator { get; private set; }

        /// <inheritdoc cref="WorkflowActionFormSection.SectionVisibilityRulesJSON"/>
        [DataMember]
        public string SectionVisibilityRulesJSON
        {
            get => SectionVisibilityRules?.ToJson();
            private set => SectionVisibilityRules = value?.FromJsonOrNull<FieldVisibilityRules>();
        }

        /// <inheritdoc cref="WorkflowActionFormSection.Order"/>
        [DataMember]
        public int Order { get; private set; }

        /// <inheritdoc cref="WorkflowActionFormSection.WorkflowActionFormId"/>
        [DataMember]
        public int WorkflowActionFormId { get; private set; }

        /// <inheritdoc cref="WorkflowActionFormSection.SectionTypeValueId"/>
        [DataMember]
        public int? SectionTypeValueId { get; private set; }

        /// <inheritdoc cref="WorkflowActionFormSection.WorkflowActionForm"/>
        public WorkflowActionFormCache WorkflowActionForm => WorkflowActionFormCache.Get( WorkflowActionFormId );

        /// <inheritdoc cref="WorkflowActionFormSection.SectionType"/>
        public DefinedValueCache SectionType => SectionTypeValueId.HasValue
            ? DefinedValueCache.Get( SectionTypeValueId.Value )
            : null;

        /// <inheritdoc cref="WorkflowActionFormSection.SectionVisibilityRulesJSON"/>
        public FieldVisibilityRules SectionVisibilityRules { get; private set; }

        #endregion Properties

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            WorkflowActionFormSection workflowActionFormSection = entity as WorkflowActionFormSection;
            if ( workflowActionFormSection == null )
            {
                return;
            }

            this.Title = workflowActionFormSection.Title;
            this.Description = workflowActionFormSection.Description;
            this.ShowHeadingSeparator = workflowActionFormSection.ShowHeadingSeparator;
            this.SectionVisibilityRulesJSON = workflowActionFormSection.SectionVisibilityRulesJSON;
            this.Order = workflowActionFormSection.Order;
            this.WorkflowActionFormId = workflowActionFormSection.WorkflowActionFormId;
            this.SectionTypeValueId = workflowActionFormSection.SectionTypeValueId;
        }
    }
}
