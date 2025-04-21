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

namespace Rock.ViewModels.Blocks.Reporting.InteractionComponentDetail
{
    /// <summary>
    /// The box that contains all the initialization information for the Interaction Component Detail block.
    /// </summary>
    public class InteractionComponentDetailInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the name of the component.
        /// </summary>
        /// <value>
        /// The name of the component.
        /// </value>
        public string ComponentName { get; set; }

        /// <summary>
        /// Gets or sets the content. The content is generated using either the Component's LavaTemplate of the block's DefaultLavaTemplate attribute.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public string Content { get; set; }
    }
}
