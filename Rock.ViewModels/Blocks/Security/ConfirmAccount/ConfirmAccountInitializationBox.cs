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

namespace Rock.ViewModels.Blocks.Security.ConfirmAccount
{
    /// <summary>
    /// A box containing the required information to display the Confirm Account block.
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Blocks.BlockBox" />
    public class ConfirmAccountInitializationBox : BlockBox
    {
        /// <summary>
        /// The bag containing the available actions for the Confirm Account block.
        /// </summary>
        public ConfirmAccountActionNamesBag ActionNames { get; set; }

        /// <summary>
        /// The box containing the required information to display a Confirm Account block view.
        /// </summary>
        public ConfirmAccountViewBox View { get; set; }
    }
}
