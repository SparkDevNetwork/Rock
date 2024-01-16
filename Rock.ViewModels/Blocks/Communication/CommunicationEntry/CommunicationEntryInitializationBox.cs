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

using Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduleToolbox;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.CommunicationEntry
{
    /// <summary>
    /// Box containing initialization information for the Communication Entry block.
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduleToolbox.InitializationBox" />
    public class CommunicationEntryInitializationBox : InitializationBox
    {
        /// <summary>
        /// Gets or sets a value indicating whether the block should use full mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the block should use full mode; otherwise, <c>false</c>.
        /// </value>
        public bool IsFullMode { get; set; }

        /// <summary>
        /// Gets or sets the mediums for the tab control.
        /// </summary>
        /// <value>
        /// The mediums for the tab control.
        /// </value>
        public List<ListItemBag> Mediums { get; set; }

        /// <summary>
        /// Gets or sets the authorization details for the block.
        /// </summary>
        /// <value>
        /// The authorization details for the block.
        /// </value>
        public CommunicationEntryAuthorizationBag Authorization { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entire Communication Entry block is hidden.
        /// <para>There should be another block on the page that will display the Communication details in view mode when this is true.</para>
        /// </summary>
        /// <value>
        ///   <c>true</c> if the entire Communication Entry block is hidden; otherwise, <c>false</c>.
        /// </value>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; }

        // TODO JMH What are these for?
        /// <summary>
        /// Gets or sets the additional merge fields.
        /// </summary>
        /// <value>
        /// The additional merge fields.
        /// </value>
        public List<string> AdditionalMergeFields { get; set; }

        /// <summary>
        /// Gets or sets the communication details being created/edited.
        /// </summary>
        /// <value>
        /// The communication details being created/edited.
        /// </value>
        public CommunicationEntryCommunicationBag Communication { get; set; }

        /// <summary>
        /// Gets or sets the options for the selected communication medium.
        /// </summary>
        /// <value>
        /// The options for the selected communication medium.
        /// </value>
        public CommunicationEntryMediumOptionsBaseBag MediumOptions { get; set; }
    }
}
