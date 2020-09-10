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
using System.Linq;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about an AssessmentType that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    /// <seealso cref="T:Rock.Web.Cache.ModelCache{Rock.Web.Cache.AssessmentTypeCache, Rock.Model.AssessmentType}" />
    [Serializable]
    [DataContract]
    public class AssessmentTypeCache : ModelCache<AssessmentTypeCache, Rock.Model.AssessmentType>
    {
        #region Properties

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        [DataMember]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the assessment path.
        /// </summary>
        /// <value>
        /// The assessment path.
        /// </value>
        [DataMember]
        public string AssessmentPath { get; set; }

        /// <summary>
        /// Gets or sets the assessment results path.
        /// </summary>
        /// <value>
        /// The assessment results path.
        /// </value>
        [DataMember]
        public string AssessmentResultsPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public Boolean IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [requires request].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [requires request]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public Boolean RequiresRequest { get; set; }

        /// <summary>
        /// Gets or sets the minimum days to retake.
        /// </summary>
        /// <value>
        /// The minimum days to retake.
        /// </value>
        [DataMember]
        public int MinimumDaysToRetake { get; set; }

        /// <summary>
        /// Gets or sets the duration (in days) that the assessment is considered valid.
        /// </summary>
        /// <value>
        /// The duration (in days) the <see cref="Rock.Model.AssessmentType"/> is considered valid.
        /// </value>
        [DataMember]
        public int ValidDuration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the color of the badge.
        /// </summary>
        /// <value>
        /// The color of the badge.
        /// </value>
        [DataMember]
        public string BadgeColor { get; set; }

        /// <summary>
        /// Gets or sets the badge summary lava.
        /// </summary>
        /// <value>
        /// The badge summary lava.
        /// </value>
        [DataMember]
        public string BadgeSummaryLava { get; set; }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Set's the cached AssessmentType properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( ( entity is AssessmentType assessmentType ) )
            {
                Title = assessmentType.Title;
                Description = assessmentType.Description;
                AssessmentPath = assessmentType.AssessmentPath;
                AssessmentResultsPath = assessmentType.AssessmentResultsPath;
                IsActive = assessmentType.IsActive;
                RequiresRequest = assessmentType.RequiresRequest;
                MinimumDaysToRetake = assessmentType.MinimumDaysToRetake;
                ValidDuration = assessmentType.ValidDuration;
                IsSystem = assessmentType.IsSystem;
                IconCssClass = assessmentType.IconCssClass;
                BadgeColor = assessmentType.BadgeColor;
                BadgeSummaryLava = assessmentType.BadgeSummaryLava;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance Title.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance Title.
        /// </returns>
        public override string ToString()
        {
            return this.Title;
        }

        #endregion
    }
}
