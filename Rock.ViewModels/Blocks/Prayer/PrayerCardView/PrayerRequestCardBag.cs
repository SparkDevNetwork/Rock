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

namespace Rock.ViewModels.Blocks.Prayer.PrayerCardView
{
    /// <summary>
    /// The display data for a single prayer request card in the Prayer Card
    /// View block.
    /// </summary>
    public class PrayerRequestCardBag
    {
        /// <summary>
        /// Gets or sets the hashed identifier of the prayer request. Used when
        /// praying for or flagging the request.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the first name of the person the request is for.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the person the request is for.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the text of the prayer request.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the name of the request's category, or <c>null</c>
        /// when the request has no category.
        /// </summary>
        public string CategoryName { get; set; }
    }
}
