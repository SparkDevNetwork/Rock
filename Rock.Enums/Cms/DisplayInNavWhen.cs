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

namespace Rock.Model
{
    /// <summary>
    /// Represents how a <see cref="Rock.Model.Page"/> should be displayed in the page navigation controls.
    /// </summary>
    [Enums.EnumDomain( "Cms" )]
    public enum DisplayInNavWhen
    {
        /// <summary>
        /// Display this page in navigation controls when allowed by security
        /// </summary>
        WhenAllowed = 0,

        /// <summary>
        /// Always display this page in navigation controls, regardless of security
        /// </summary>
        Always = 1,

        /// <summary>
        /// Never display this page in navigation controls
        /// </summary>
        Never = 2
    }
}
