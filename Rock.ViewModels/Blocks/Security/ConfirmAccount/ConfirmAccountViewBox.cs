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

using Rock.Enums.Blocks.Security.ConfirmAccount;

namespace Rock.ViewModels.Blocks.Security.ConfirmAccount
{
    /// <summary>
    /// A box containing the required information to display a Confirm Account block view.
    /// </summary>
    public class ConfirmAccountViewBox
    {
        /// <summary>
        /// The view type to display.
        /// <para>The corresponding view property will be set while the others will remain <c>null</c>.</para>
        /// </summary>
        public ConfirmAccountViewType ViewType { get; set; }

        /// <summary>
        /// The bag containing the required information to display the Confirm Account Block's account confirmation view.
        /// <para>Set when <see cref="ViewType"/> == <see cref="ConfirmAccountViewType.AccountConfirmation"/>; otherwise, <c>null</c>.</para>
        /// </summary>
        public ConfirmAccountAccountConfirmationViewOptionsBag AccountConfirmationViewOptions { get; set; }

        /// <summary>
        /// The bag containing the required information to display the Confirm Account Block's alert view.
        /// <para>Set when <see cref="ViewType"/> == <see cref="ConfirmAccountViewType.Alert"/>; otherwise, <c>null</c>.</para>
        /// </summary>
        public ConfirmAccountAlertViewOptionsBag AlertViewOptions { get; set; }

        /// <summary>
        /// The bag containing the required information to display the Confirm Account Block's delete confirmation view.
        /// <para>Set when <see cref="ViewType"/> == <see cref="ConfirmAccountViewType.DeleteConfirmation"/>; otherwise, <c>null</c>.</para>
        /// </summary>
        public ConfirmAccountDeleteConfirmationViewOptionsBag DeleteConfirmationViewOptions { get; set; }

        /// <summary>
        /// The bag containing the required information to display the Confirm Account Block's change password view.
        /// <para>Set when <see cref="ViewType"/> == <see cref="ConfirmAccountViewType.ChangePassword"/>; otherwise, <c>null</c>.</para>
        /// </summary>
        public ConfirmAccountChangePasswordViewOptionsBag ChangePasswordViewOptions { get; set; }

        /// <summary>
        /// The bag containing the required information to display the Confirm Account Block's content view.
        /// <para>Set when <see cref="ViewType"/> == <see cref="ConfirmAccountViewType.Content"/>; otherwise, <c>null</c>.</para>
        /// </summary>
        public ConfirmAccountContentViewOptionsBag ContentViewOptions { get; set; }
    }
}
