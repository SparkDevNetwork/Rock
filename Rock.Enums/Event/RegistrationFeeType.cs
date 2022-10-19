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
    /// Flag for how fee items should be displayed/required by user
    /// </summary>
    [Enums.EnumDomain( "Event" )]
    public enum RegistrationFeeType
    {
        /// <summary>
        /// There is one only one option for this fee
        /// </summary>
        Single = 0,

        /// <summary>
        /// There are multiple options available for this fee
        /// </summary>
        Multiple = 1,
    }
}
