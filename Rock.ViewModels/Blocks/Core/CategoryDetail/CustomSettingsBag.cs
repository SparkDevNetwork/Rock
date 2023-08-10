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
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using Rock.ViewModels.Blocks.Cms.ContentCollectionView;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.CategoryDetail
{
    /// <summary>
    /// The settings that will be edited in the custom settings panel for the
    /// Category Detail block.
    /// </summary>
    public class CustomSettingsBag
    {
        /// <summary>
        /// Gets or sets the root category.
        /// </summary>
        public ListItemBag RootCategory { get; set; }

        /// <summary>
        /// Gets or sets the exclude categories.
        /// </summary>
        public List<ListItemBag> ExcludeCategories { get; set; }

        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        public Guid? EntityTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier.
        /// </summary>
        public string EntityTypeQualifierProperty { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier value.
        /// </summary>
        public string EntityTypeQualifierValue { get; set; }
    }
}
