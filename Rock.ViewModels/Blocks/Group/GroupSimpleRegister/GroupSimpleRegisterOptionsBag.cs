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

namespace Rock.ViewModels.Blocks.Group.GroupSimpleRegister
{
    /// <summary>
    /// The configuration sent to the Group Simple Register block on initialization.
    /// A populated <see cref="Rock.ViewModels.Blocks.BlockBox.ErrorMessage"/> indicates a
    /// configuration problem that prevents the form from being shown.
    /// </summary>
    public class GroupSimpleRegisterOptionsBag : BlockBox
    {
        /// <summary>
        /// Gets or sets the initial form values. These are prefilled from the current person
        /// when the block is configured to load the current person; otherwise they are empty.
        /// </summary>
        public GroupSimpleRegisterBag Registrant { get; set; }

        /// <summary>
        /// Gets or sets the text to display on the submit button.
        /// </summary>
        public string SaveButtonText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the CAPTCHA verification step is skipped.
        /// </summary>
        public bool DisableCaptchaSupport { get; set; }
    }
}
