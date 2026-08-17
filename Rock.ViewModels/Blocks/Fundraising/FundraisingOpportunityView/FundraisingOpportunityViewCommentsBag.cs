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

using Rock.ViewModels.Controls;

namespace Rock.ViewModels.Blocks.Fundraising.FundraisingOpportunityView
{
    /// <summary>
    /// The opportunity comments (notes) data used to render the comments tab.
    /// </summary>
    public class FundraisingOpportunityViewCommentsBag
    {
        /// <summary>
        /// Gets or sets the comments to display.
        /// </summary>
        public List<NoteBag> Notes { get; set; }

        /// <summary>
        /// Gets or sets the note types available for commenting.
        /// </summary>
        public List<NoteTypeBag> NoteTypes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person may add a comment.
        /// </summary>
        public bool IsAddAllowed { get; set; }

        /// <summary>
        /// Gets or sets the URL used to log in to comment. This is only populated when the
        /// current person is not logged in; otherwise it is <c>null</c>.
        /// </summary>
        public string LoginUrl { get; set; }
    }
}
