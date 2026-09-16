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
namespace Rock.ViewModels.Blocks.Crm.PersonDetail.Bio
{
    /// <summary>
    /// Describes a single workflow action displayed in the actions menu of the
    /// Person Bio block.
    /// </summary>
    public class BioWorkflowActionBag
    {
        /// <summary>
        /// Gets or sets the name of the workflow type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class of the workflow type.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the URL that launches the workflow for the person.
        /// </summary>
        public string Url { get; set; }
    }
}
