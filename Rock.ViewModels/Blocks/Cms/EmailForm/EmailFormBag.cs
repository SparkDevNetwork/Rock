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

namespace Rock.ViewModels.Blocks.Cms.EmailForm
{
    /// <summary>
    /// Holds initialization data for the Email Form block.
    /// </summary>
    public class EmailFormBag : BlockBox
    {
        /// <summary>
        /// The content of the email form.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The text shown on the submit button.
        /// </summary>
        public string SubmitButtonText { get; set; }

        /// <summary>
        /// The CSS class for the submit button.
        /// </summary>
        public string SubmitButtonCssClass { get; set; }

        /// <summary>
        /// The CSS class for the container that wraps the submit button.
        /// </summary>
        public string SubmitButtonWrapCssClass { get; set; }

        /// <summary>
        /// True if captcha is disabled, otherwise false.
        /// </summary>
        public bool DisableCaptchaSupport { get; set; }
    }
}
