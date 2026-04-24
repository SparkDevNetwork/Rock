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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.ContentChannelView
{
    public class ContentChannelViewCustomSettingsOptionsBag
    {
        public List<ListItemBag> ContentChannels { get; set; }
        public List<ListItemBag> ContentChannelItemStatuses { get; set; }
        public List<ListItemBag> CacheTags { get; set; }
        public List<ListItemBag> ContextFilterAttributes { get; set; }
        public List<ListItemBag> MetaDescriptionAttributes { get; set; }
        public List<ListItemBag> MetaImageAttributes { get; set; }
        public bool IsSetRssAutodiscoverLinkVisible { get; set; }
        public bool IsPersonalizationVisible { get; set; }
        public List<ListItemBag> PersonalizationFilterTypes { get; set; }
        public List<ListItemBag> OrderItemsByKeyOptions { get; set; }
        public List<ListItemBag> OrderItemsByValueOptions { get; set; }
    }
}
