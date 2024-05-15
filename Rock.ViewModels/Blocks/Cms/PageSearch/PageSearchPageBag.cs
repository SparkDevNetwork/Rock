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

namespace Rock.ViewModels.Blocks.Cms.PageSearch
{
    public class PageSearchPageBag
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public int? ParentPageId { get; set; }

        public string Description { get; set; }

        public string Icon { get; set; }

        public bool PageDisplayDescription { get; set; }

        public List<PageSearchPageBag> Children { get; set; }

        public string Url { get; set; }

    }
}
