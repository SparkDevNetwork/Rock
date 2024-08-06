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

namespace Rock.ViewModels.Blocks.Core.TaggedItemList
{
    public class TaggedItemListOptionsBag
    {
        public string Title { get; set; }

        public string TagName { get; set; }

        public string EntityTypeName { get; set; }

        public string EntityTypeGuid { get; set; }

        public bool IsPersonTag { get; internal set; }

        public bool IsBlockHidden { get; set; }

        public int TagId { get; set; }
    }
}
