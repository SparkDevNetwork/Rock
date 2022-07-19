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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the GetInteractionComponents API action of
    /// the InteractionComponentPicker control.
    /// </summary>
    public class InteractionComponentPickerGetInteractionComponentsOptionsBag
    {
        /// <summary>
        /// ID of the container the components are within. The <see cref="InteractionChannelGuid"/>
        /// takes precedence over this if present.
        /// </summary>
        public int? InteractionChannelId { get; set; }

        /// <summary>
        /// GUID of the container the components are within.  Can use <see cref="InteractionChannelId"/>
        /// instead if easier, but this takes precedence if present.
        /// </summary>
        public Guid? InteractionChannelGuid { get; set; }
    }
}
