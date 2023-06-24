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

namespace Rock.ViewModels.Blocks.Connection.ConnectionRequestBoard
{
    /// <summary>
    /// A bag that contains phone number information for the connection request board.
    /// </summary>
    public class ConnectionRequestBoardPhoneBag
    {
        /// <summary>
        /// Gets or sets the formatted phone number.
        /// </summary>
        public string FormattedPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the phone type.
        /// </summary>
        public string PhoneType { get; set; }

        /// <summary>
        /// Gets or sets the SMS link HTML, if the phone number is SMS-enabled.
        /// </summary>
        public string SmsLinkHtml { get; set; }
    }
}
