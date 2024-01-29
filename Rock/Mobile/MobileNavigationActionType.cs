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
namespace Rock.Mobile
{
    /// <summary>
    /// The type of navigation action to perform on the mobile shell.
    /// </summary>
    /// <remarks>
    /// This needs to move somewhere else before it is marked as public.
    /// </remarks>
    internal enum MobileNavigationActionType
    {
        /// <summary>
        /// No navigation action should be performed.
        /// </summary>
        None = 0,

        /// <summary>
        /// The current page should be popped off the navigation stack.
        /// </summary>
        PopPage = 1,

        /// <summary>
        /// The navigation stack should be cleared and replaced with a new page.
        /// </summary>
        ResetToPage = 2,

        /// <summary>
        /// The current page should be replaced with a new page.
        /// </summary>
        ReplacePage = 3,

        /// <summary>
        /// A new page should be pushed onto the navigation stack.
        /// </summary>
        PushPage = 4,

        /// <summary>
        /// Dismiss the cover sheet if the current page is currently in one.
        /// </summary>
        DismissCoverSheet = 5
    }
}
