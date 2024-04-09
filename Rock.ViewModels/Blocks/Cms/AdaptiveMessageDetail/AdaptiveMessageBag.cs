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

using System.Collections.Generic;
using Rock.ViewModels.Blocks.Cms.AdaptiveMessageAdaptationDetail;
using Rock.ViewModels.Blocks.Cms.ContentCollectionDetail;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.AdaptiveMessageDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class AdaptiveMessageBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the collection of Categories that this Rock.Model.AdaptiveMessage is associated with.
        /// NOTE: Since changes to Categories isn't tracked by ChangeTracker, set the ModifiedDateTime if Categories are modified.
        /// </summary>
        public List<ListItemBag> Categories { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the adaptations grid data.
        /// </summary>
        /// <value>
        /// The adaptations grid data.
        /// </value>
        public GridDataBag AdaptationsGridData { get; set; }

        /// <summary>
        /// Gets or sets the step type adaptations grid definition.
        /// </summary>
        /// <value>
        /// The adaptations grid definition.
        /// </value>
        public GridDefinitionBag AdaptationsGridDefinition { get; set; }

        /// <summary>
        /// The adaptation shared Attributes
        /// </summary>
        public List<PublicEditableAttributeBag> AdaptationSharedAttributes { get; set; }

        /// <summary>
        /// The adaptation Attributes
        /// </summary>
        public List<PublicEditableAttributeBag> AdaptationAttributes { get; set; }
    }
}
