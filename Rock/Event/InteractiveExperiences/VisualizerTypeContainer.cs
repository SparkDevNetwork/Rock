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
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Data;
using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Event.InteractiveExperiences
{
    /// <summary>
    /// MEF Container class for Interactive Experience Visualizer Type Components.
    /// </summary>
    internal class VisualizerTypeContainer : Container<VisualizerTypeComponent, IComponentData>
    {
        #region Fields

        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<VisualizerTypeContainer> _instance =
            new Lazy<VisualizerTypeContainer>( () => new VisualizerTypeContainer() );

        #endregion

        #region Properties

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static VisualizerTypeContainer Instance => _instance.Value;

        /// <summary>
        /// Gets all visualizer type components that have been found in the system.
        /// </summary>
        /// <value>All visualizer type components that have been found in the system.</value>
        public IEnumerable<VisualizerTypeComponent> AllComponents => Components.Values.Select( v => v.Value );

        /// <summary>
        /// Gets or sets the MEF components.
        /// </summary>
        /// <value>
        /// The MEF components.
        /// </value>
        [ImportMany( typeof( VisualizerTypeComponent ) )]
        protected override IEnumerable<Lazy<VisualizerTypeComponent, IComponentData>> MEFComponents { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Forces a reloading of all the components
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();

            // Create any attributes that need to be created
            var actionEntityTypeId = EntityTypeCache.Get<InteractiveExperienceAction>().Id;

            using ( var rockContext = new RockContext() )
            {
                foreach ( var visualizerComponent in AllComponents )
                {
                    var visualizerComponentType = visualizerComponent.GetType();
                    var visualzierComponentEntityTypeId = EntityTypeCache.Get( visualizerComponentType ).Id;
                    ActionTypeContainer.UpdateAttributes( visualizerComponentType, actionEntityTypeId, "visualizer", nameof( InteractiveExperienceAction.ResponseVisualEntityTypeId ), visualzierComponentEntityTypeId.ToString(), rockContext );
                }
            }
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns>An instance of <see cref="VisualizerTypeComponent"/> or <c>null</c> if it was not found.</returns>
        public static VisualizerTypeComponent GetComponent( string entityType )
        {
            return Instance.GetComponentByEntity( entityType );
        }

        /// <summary>
        /// Gets the component with the matching Entity Type identifier.
        /// </summary>
        /// <param name="entityTypeId">The <see cref="EntityType"/> identifier.</param>
        /// <returns>An instance of <see cref="VisualizerTypeComponent"/> or <c>null</c> if it was not found.</returns>
        public static VisualizerTypeComponent GetComponentFromEntityType( int entityTypeId )
        {
            var entityTypeCache = EntityTypeCache.Get( entityTypeId );
            var actionTypeType = entityTypeCache?.GetEntityType();

            if ( actionTypeType == null )
            {
                return null;
            }

            return GetComponent( actionTypeType.FullName );
        }

        /// <summary>
        /// Gets the component matching the specified type.
        /// </summary>
        /// <returns>An instance of <see cref="VisualizerTypeComponent"/> or <c>null</c> if it was not found.</returns>
        public static VisualizerTypeComponent GetComponent<T>()
        {
            return GetComponent( typeof( T ).FullName );
        }

        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns>A <see cref="string"/> that represents the name of the component or an empty string if not found.</returns>
        public static string GetComponentName( string entityType )
        {
            return Instance.GetComponentNameByEntity( entityType );
        }

        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the name of the component or an empty string if not found.</returns>
        public static string GetComponentName<T>()
        {
            return GetComponentName( typeof( T ).FullName );
        }

        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        /// <param name="component">The component whose name is to be determined.</param>
        /// <returns>A <see cref="string"/> that represents the name of the component or an empty string if not found.</returns>
        public static string GetComponentName( VisualizerTypeComponent component )
        {
            if ( component == null )
            {
                throw new ArgumentNullException( nameof( component ) );
            }

            return GetComponentName( component.GetType().FullName );
        }

        #endregion
    }
}
