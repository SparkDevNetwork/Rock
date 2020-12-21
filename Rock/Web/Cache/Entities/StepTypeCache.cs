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
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// <see cref="StepType"/>
    /// </summary>
    [Serializable]
    [DataContract]
    public class StepTypeCache : ModelCache<StepTypeCache, StepType>
    {
        #region Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="StepProgram"/> to which this step type belongs. This property is required.
        /// </summary>
        [DataMember]
        public int StepProgramId { get; private set; }

        /// <summary>
        /// Gets or sets the name of the step type. This property is required.
        /// </summary>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets a description of the step type.
        /// </summary>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        [DataMember]
        public string IconCssClass { get; private set; }

        /// <summary>
        /// Gets or sets a flag indicating if this step type allows multiple step records per person.
        /// </summary>
        [DataMember]
        public bool AllowMultiple { get; private set; }

        /// <summary>
        /// Gets or sets a flag indicating if this step type happens over time (like being in a group) or is it achievement based (like attended a class).
        /// </summary>
        [DataMember]
        public bool HasEndDate { get; private set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="DataView"/> associated with this step type. The data view reveals the people that are allowed to be
        /// considered for this step type.
        /// </summary>
        [DataMember]
        public int? AudienceDataViewId { get; private set; }

        /// <summary>
        /// Gets or sets a flag indicating if the number of occurences should be shown on the badge.
        /// </summary>
        [DataMember]
        public bool ShowCountOnBadge { get; private set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="DataView"/> associated with this step type. The data view reveals the people that should be considered
        /// as having completed this step.
        /// </summary>
        [DataMember]
        public int? AutoCompleteDataViewId { get; private set; }

        /// <summary>
        /// Gets or sets a flag indicating if this item can be edited by a person.
        /// </summary>
        [DataMember]
        public bool AllowManualEditing { get; private set; }

        /// <summary>
        /// Gets or sets the highlight color for badges and cards.
        /// </summary>
        [DataMember]
        public string HighlightColor { get; private set; }

        /// <summary>
        /// Gets or sets the lava template used to render custom card details.
        /// </summary>
        [DataMember]
        public string CardLavaTemplate { get; private set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="MergeTemplate"/> associated with this step type. This template can represent things like
        /// certificates or letters.
        /// </summary>
        [DataMember]
        public int? MergeTemplateId { get; private set; }

        /// <summary>
        /// Gets or sets the name used to describe the merge template (e.g. Certificate).
        /// </summary>
        [DataMember]
        public string MergeTemplateDescriptor { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; private set; }

        #endregion Properties

        #region Related Caches

        /// <summary>
        /// Gets the step types.
        /// </summary>
        public StepProgramCache StepProgram => StepProgramCache.Get( StepProgramId );

        #endregion Related Caches

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var sourceModel = entity as StepType;
            if ( sourceModel == null )
            {
                return;
            }

            Name = sourceModel.Name;
            Description = sourceModel.Description;
            StepProgramId = sourceModel.StepProgramId;
            IconCssClass = sourceModel.IconCssClass;
            AllowMultiple = sourceModel.AllowMultiple;
            HasEndDate = sourceModel.HasEndDate;
            AudienceDataViewId = sourceModel.AudienceDataViewId;
            ShowCountOnBadge = sourceModel.ShowCountOnBadge;
            AutoCompleteDataViewId = sourceModel.AutoCompleteDataViewId;
            AllowManualEditing = sourceModel.AllowManualEditing;
            HighlightColor = sourceModel.HighlightColor;
            CardLavaTemplate = sourceModel.CardLavaTemplate;
            MergeTemplateId = sourceModel.MergeTemplateId;
            MergeTemplateDescriptor = sourceModel.MergeTemplateDescriptor;
            IsActive = sourceModel.IsActive;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion Public Methods
    }
}