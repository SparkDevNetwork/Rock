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
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Threading.Tasks;

using Rock.Communication;
using Rock.Data;
using Rock.Enums.Event;
using Rock.Event.InteractiveExperiences;
using Rock.Net;
using Rock.RealTime;
using Rock.RealTime.Topics;
using Rock.ViewModels.Event.InteractiveExperiences;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class InteractiveExperienceActionService
    {
        /// <summary>
        /// Gets the action render bag that describes the specified action. This
        /// contains all the information required for the UI to render the action.
        /// </summary>
        /// <param name="action">The action to be rendered.</param>
        /// <returns>An instance of <see cref="ActionRenderConfigurationBag"/> that contains the configuration required to render the action.</returns>
        internal ActionRenderConfigurationBag GetActionRenderBag( InteractiveExperienceAction action )
        {
            if ( action == null )
            {
                return null;
            }

            // Get the action component.
            var actionType = ActionTypeContainer.GetComponentFromEntityType( action.ActionEntityTypeId );

            if ( actionType == null )
            {
                return null;
            }

            if ( action.Attributes == null )
            {
                action.LoadAttributes( Context as RockContext );
            }

            return actionType.GetRenderConfiguration( action );
        }

        /// <summary>
        /// Gets the visualizer render bag that describes the specified visualizer. This
        /// contains all the information required for the UI to render the visualizer.
        /// </summary>
        /// <param name="action">The action to use when rendering the visualizer.</param>
        /// <returns>An instance of <see cref="VisualizerRenderConfigurationBag"/> that contains the configuration required to render the visualizer.</returns>
        internal VisualizerRenderConfigurationBag GetVisualizerRenderBag( InteractiveExperienceAction action )
        {
            if ( action == null || !action.ResponseVisualEntityTypeId.HasValue )
            {
                return null;
            }

            // Get the visualizer component.
            var visualizerType = VisualizerTypeContainer.GetComponentFromEntityType( action.ResponseVisualEntityTypeId.Value );

            if ( visualizerType == null )
            {
                return null;
            }

            if ( action.Attributes == null )
            {
                action.LoadAttributes( Context as RockContext );
            }

            return visualizerType.GetRenderConfiguration( action );
        }
    }
}