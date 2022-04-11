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

namespace Rock.ViewModels.Blocks
{
    public class DetailBlockBox<TEntityBag, TOptions> : IValidPropertiesBox
        where TOptions : new()
    {
        public TEntityBag Entity { get; set; }

        public TOptions Options { get; set; } = new TOptions();

        public string ErrorMessage { get; set; }

        public bool IsEditable { get; set; }

        public Dictionary<string, string> NavigationUrls { get; set; } = new Dictionary<string, string>();

        public List<string> ValidProperties { get; set; }
    }
}
