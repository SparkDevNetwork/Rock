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

using Rock.Enums.Communication;

namespace Rock.ViewModels.Blocks.Communication.CommunicationDetail
{
    /// <summary>
    /// A bag that contains information about the request to get communication analytics.
    /// </summary>
    public class CommunicationAnalyticsRequestBag
    {
        /// <summary>
        /// Gets or sets the optional <see cref="CommunicationType"/> for which to get analytics.
        /// </summary>
        public CommunicationType? Type { get; set; }
    }
}
