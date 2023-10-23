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

namespace Rock.AI.Classes.Moderations
{
    /// <summary>
    /// The class for holding the response from a moderation.
    /// </summary>
    public class ModerationsResponse
    {
        /// <summary>
        /// A unique identifier for the completion.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Is the text flagged for moderation.
        /// </summary>
        public bool IsFlagged { get; set; }

        /// <summary>
        /// Moderation data by category
        /// </summary>
        public ModerationsResponseCategories ModerationsResponseCategories { get; set; }

        /// <summary>
        /// Determines if the request was successful.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Error messages from the request.
        /// </summary>
        public string ErrorMessage { get; set; }

    }
}
