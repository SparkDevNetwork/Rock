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
namespace Rock.ViewModels.Blocks.Lms.PublicLearningClassEnrollment
{
    /// <summary>
    /// The box containing all the necessary information for the Learning Class Enrollment block.
    /// </summary>
    public class PublicLearningClassEnrollmentBlockBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the HTML to be rendered for the completion content.
        /// </summary>
        public string CompletionHtml { get; set; }

        /// <summary>
        /// Gets or sets the HTML to be rendered for the confirmation content.
        /// </summary>
        public string ConfirmationHtml { get; set; }

        /// <summary>
        /// Gets or sets the HTML to be rendered for the enrollment error content.
        /// </summary>
        public string EnrollmentErrorHtml { get; set; }

        /// <summary>
        /// Gets or sets the HTML to be rendered for the header content.
        /// </summary>
        public string HeaderHtml { get; set; }
    }
}
