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

using Rock.Model;
using Rock.ViewModels.Event.InteractiveExperiences;

namespace Rock.Event.InteractiveExperiences
{
    /// <summary>
    /// Base class for interactive experience visualizer type components
    /// </summary>
    internal abstract class VisualizerTypeComponent : Rock.Extension.Component
    {
        #region Properties

        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults
        {
            get => new Dictionary<string, string>
            {
                { "Active", "True" },
                { "Order", "0" }
            };
        }

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public override bool IsActive
        {
            get => true;
        }

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public override int Order
        {
            get => 0;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizerTypeComponent" /> class.
        /// </summary>
        public VisualizerTypeComponent() : base( false )
        {
            // Override default constructor of Component that loads attributes (needs to be done by each instance)
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the attributes for the <see cref="InteractiveExperienceAction" />.
        /// </summary>
        /// <param name="action"></param>
        public void LoadAttributes( InteractiveExperienceAction action )
        {
            if ( action == null )
            {
                throw new ArgumentNullException( nameof( action ) );
            }

            if ( action.Attributes == null )
            {
                action.LoadAttributes();
            }
        }

        /// <summary>
        /// Gets the value of an attribute key. Do not use this method. Use <see cref="GetAttributeValue(InteractiveExperienceAction, string)" />
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public override string GetAttributeValue( string key )
        {
            throw new Exception( "Use the GetAttributeValue( InteractiveExperienceAction, key ) method instead." );
        }

        /// <summary>
        /// Gets the attribute value for the action.
        /// </summary>
        /// <param name="action">The action whose attribute value should be retrieved.</param>
        /// <param name="key">The key of the attribute to retrieve.</param>
        /// <returns>A string that represents the value of the attribute.</returns>
        protected string GetAttributeValue( InteractiveExperienceAction action, string key )
        {
            if ( action == null )
            {
                throw new ArgumentNullException( nameof( action ) );
            }

            if ( key == null )
            {
                throw new ArgumentNullException( nameof( key ) );
            }

            return action.GetAttributeValue( key );
        }

        /// <summary>
        /// Gets the action render configuration. This data is used by clients
        /// to display the custom UI for this action.
        /// </summary>
        /// <param name="action">The action instance to use when getting configuraiton data.</param>
        /// <returns>An instance of <see cref="VisualizerRenderConfigurationBag"/> that represents this action.</returns>
        public virtual VisualizerRenderConfigurationBag GetRenderConfiguration( InteractiveExperienceAction action )
        {
            var actionComponent = ActionTypeContainer.GetComponentFromEntityType( action.ActionEntityTypeId );

            var configuration = new VisualizerRenderConfigurationBag
            {
                Title = actionComponent?.GetDisplayTitle( action ),
                ActionId = action.Id,
                VisualizerTypeGuid = EntityType.Guid,
                ConfigurationValues = new Dictionary<string, string>()
            };

            return configuration;
        }

        #endregion
    }
}
