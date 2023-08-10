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

using System;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Allow configuring custom action buttons on a grid with an EventHandler defined in the calling block.
    /// </summary>
    public class CustomActionConfigEvent : CustomActionConfig, ICustomActionEventHandler
    {
        /// <summary>
        /// The <see cref="EventHandler"/> to set for the config button action.
        /// </summary>
        /// <remarks>
        /// The <see cref="CustomActionConfig.Route" /> parameter determines the security associated with this action.
        /// The action will be unavailable if the associated route is not accessible to the current user.
        /// </remarks>
        public EventHandler EventHandler { get; set; }
    }
}
