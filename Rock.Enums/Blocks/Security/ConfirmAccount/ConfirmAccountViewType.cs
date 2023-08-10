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

namespace Rock.Enums.Blocks.Security.ConfirmAccount
{
    /// <summary>
    /// Represents the view type for the Confirm Account Block.
    /// </summary>
    public enum ConfirmAccountViewType
    {
        /// <summary>
        /// The primary Confirm Account block view where the individual can enter a confirmation code and decide which action to take.
        /// </summary>
        AccountConfirmation = 0,
        
        /// <summary>
        /// The view showing an alert.
        /// </summary>
        Alert = 1,

        /// <summary>
        /// The view where the individual can confirm account deletion.
        /// </summary>
        DeleteConfirmation = 2,

        /// <summary>
        /// The view where the individual can change their account password.
        /// </summary>
        ChangePassword = 3,

        /// <summary>
        /// The view showing arbitrary content. 
        /// </summary>
        /// <remarks>At the time of implementation, this view type was used as a backwards-compatible option to inject block settings content into the rendered block upon successful account deletion.</remarks>
        Content = 4
    }
}
