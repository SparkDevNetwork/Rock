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

namespace Rock.Enums.Workflow
{
    /// <summary>
    /// The type of message to be displayed with an interactive workflow element.
    /// </summary>
    public enum InteractiveMessageType
    {
        /// <summary>
        /// A hard error that prevents further action.
        /// </summary>
        Error = 0,

        /// <summary>
        /// A warning that could indicate a problem but will not prevent the
        /// workflow from moving forward.
        /// </summary>
        Warning = 1,

        /// <summary>
        /// Informational text that does not affect processing but could give
        /// the person some hints about what is going on.
        /// </summary>
        Information = 2,

        /// <summary>
        /// The content contains XAML markup for use on the mobile shell.
        /// </summary>
        Xaml = 3,

        /// <summary>
        /// The content contains information to trigger a redirect to another
        /// page.
        /// </summary>
        Redirect = 4,

        /// <summary>
        /// The content contains HTML markup for use on the website. A unique
        /// identifier means this is a mobile web page reference.
        /// </summary>
        Html = 5,

        /// <summary>
        /// Success text to indicate that the action has completed.
        /// </summary>
        Success = 6,
    }
}
