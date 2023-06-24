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

namespace Rock.ViewModels.Blocks.Connection.ConnectionRequestBoard
{
    /// <summary>
    /// A bag that contains selected connection request information for the connection request board.
    /// </summary>
    public class ConnectionRequestBoardSelectedRequestBag
    {
        /// <summary>
        /// Gets or sets the selected connection request.
        /// </summary>
        public ConnectionRequestBoardConnectionRequestBag ConnectionRequest { get; set; }

        /// <summary>
        /// Gets or sets the "requester" person's photo URL.
        /// </summary>
        public string PersonPhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the "requester" person's full name.
        /// </summary>
        public string PersonFullName { get; set; }

        /// <summary>
        /// Gets or sets the "requester" person's phone numbers.
        /// </summary>
        public List<ConnectionRequestBoardPhoneBag> PersonPhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the "requester" person's email HTML, which will link to the appropriate communications
        /// page and might provide additional, helpful information via a tooltip.
        /// </summary>
        public string PersonEmailHtml { get; set; }

        /// <summary>
        /// Gets or sets the link to the "requester" person's profile page.
        /// </summary>
        public string PersonProfileLink { get; set; }

        /// <summary>
        /// Gets or sets the header labels HTML.
        /// </summary>
        public string HeaderLabelsHtml { get; set; }

        /// <summary>
        /// Gets or sets the heading HTML, to be displayed above the "requester" person's name.
        /// </summary>
        public string HeadingHtml { get; set; }

        /// <summary>
        /// Gets or sets the custom badge bar HTML.
        /// </summary>
        public string CustomBadgeBarHtml { get; set; }

        /// <summary>
        /// Gets or sets the status icons HTML.
        /// </summary>
        public string StatusIconsHtml { get; set; }

        /// <summary>
        /// Gets or sets the side description HTML.
        /// </summary>
        public string SideDescriptionHtml { get; set; }

        /// <summary>
        /// Gets or sets the comments HTML.
        /// </summary>
        public string CommentsHtml { get; set; }

        /// <summary>
        /// Gets or sets any unmet group requirements for the "requester" person, related to the assigned placement group.
        /// </summary>
        public List<ConnectionRequestBoardGroupRequirementBag> GroupRequirements { get; set; }

        /// <summary>
        /// Gets or sets the connection request activities.
        /// </summary>
        public List<ConnectionRequestBoardConnectionRequestActivityBag> Activities { get; set; }

        /// <summary>
        /// Gets or sets any workflows that have already been instantiated against this connection request.
        /// </summary>
        public List<ConnectionRequestBoardWorkflowBag> Workflows { get; set; }

        /// <summary>
        /// Gets or sets whether viewing is enabled for this connection request.
        /// </summary>
        public bool IsViewEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether editing is enabled for this connection request.
        /// </summary>
        public bool IsEditEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether connecting is enabled for this connection request.
        /// </summary>
        public bool IsConnectEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether the connection button should be visible.
        /// </summary>
        public bool IsConnectButtonVisible { get; set; }

        /// <summary>
        /// Gets or sets whether transferring is enabled for this connection request.
        /// </summary>
        public bool IsTransferEnabled { get; set; }

        /// <summary>
        /// Gets or sets the options that may be assigned when editing the connection request.
        /// </summary>
        public ConnectionRequestBoardRequestOptionsBag RequestOptions { get; set; }

        /// <summary>
        /// Gets or sets the options that may be assigned to any new or existing connection request activities.
        /// </summary>
        public ConnectionRequestBoardActivityOptionsBag ActivityOptions { get; set; }

        /// <summary>
        /// Gets or sets the options that may be assigned when transferring this connection request.
        /// </summary>
        public ConnectionRequestBoardTransferOptionsBag TransferOptions { get; set; }
    }
}
