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
using System.Linq.Dynamic;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Model;
using static Rock.Model.StepProgram;

namespace Rock.Web.Cache
{
    /// <summary>
    /// <see cref="StepProgram"/>
    /// </summary>
    [Serializable]
    [DataContract]
    public class StepProgramCache : ModelCache<StepProgramCache, StepProgram>
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name of the program. This property is required.
        /// </summary>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets a description of the program.
        /// </summary>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the term used for steps within this program. This property is required.
        /// </summary>
        [DataMember]
        public string StepTerm { get; private set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        [DataMember]
        public string IconCssClass { get; private set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Category"/>.
        /// </summary>
        /// [DataMember]
        public int? CategoryId { get; private set; }

        /// <summary>
        /// Gets or sets the default view mode for the program. This value is required.
        /// </summary>
        [DataMember]
        public ViewMode DefaultListView { get; private set; }

        #endregion Properties

        #region Related Caches

        /// <summary>
        /// Gets the step types.
        /// </summary>
        public List<StepTypeCache> StepTypes => StepTypeCache.All().Where( st => st.StepProgramId == Id ).ToList();

        #endregion Related Caches

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var sourceModel = entity as StepProgram;
            if ( sourceModel == null )
            {
                return;
            }

            Name = sourceModel.Name;
            Description = sourceModel.Description;
            StepTerm = sourceModel.StepTerm;
            IconCssClass = sourceModel.IconCssClass;
            CategoryId = sourceModel.CategoryId;
            DefaultListView = sourceModel.DefaultListView;
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