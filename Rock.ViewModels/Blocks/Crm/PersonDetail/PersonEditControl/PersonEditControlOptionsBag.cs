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

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.PersonEditControl
{
    /// <summary>
    /// Contains all the initial configuration data required to render the
    /// Person Edit block.
    /// </summary>
    public class PersonEditControlOptionsBag
    {
        /// <summary>
        /// Gets or sets the URL of the person edit page. This is populated only
        /// when a person is in context and the current user is authorized to
        /// edit; otherwise it is <c>null</c> and no edit control is shown.
        /// </summary>
        /// <value>The URL of the person edit page.</value>
        public string EditPageUrl { get; set; }
    }
}
