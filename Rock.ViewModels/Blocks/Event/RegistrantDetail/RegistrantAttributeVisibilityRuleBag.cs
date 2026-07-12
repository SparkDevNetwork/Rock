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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Event.RegistrantDetail
{
    /// <summary>
    /// A single field visibility condition for a registrant attribute. The governed
    /// attribute is shown or hidden based on the current value of the compared-to
    /// attribute. Rules are translated to attribute keys on the server so the client
    /// can evaluate them against the values it already has, without a round trip.
    /// </summary>
    public class RegistrantAttributeVisibilityRuleBag
    {
        /// <summary>
        /// Gets or sets the key of the registrant attribute whose value this rule is compared against.
        /// </summary>
        public string ComparedToAttributeKey { get; set; }

        /// <summary>
        /// Gets or sets the comparison type and value the compared-to attribute's value is evaluated with.
        /// </summary>
        public PublicComparisonValueBag ComparisonValue { get; set; }
    }
}
