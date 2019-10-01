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
namespace Rock.UniversalSearch.Crawler.RobotsTxt.Enums
{
    /// <summary>
    /// Robots Allow Rule
    /// </summary>
    public enum AllowRuleImplementation
    {
        /// <summary>
        /// First matching rule will win.
        /// </summary>
        Standard,

        /// <summary>
        /// Disallow rules will only be checked if no allow rule matches.
        /// </summary>
        AllowOverrides,

        /// <summary>
        /// The more specific (the longer) rule will apply.
        /// </summary>
        MoreSpecific
    }
}
