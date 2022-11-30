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

using Rock.Attribute;
using Rock.Data;
using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Event.InteractiveExperiences
{
    /**
     * 10/18/2022 - DSH
     * 
     * Attributes for InteractiveExperienceActions are a little weird. We actually
     * put attributes from two different component types on the one entity. Because of
     * that we enforce that each component type starts with a specific prefix to
     * help us identify which attributes are which. The available prefixes are
     * "action" for ActionTypeComponent and "visualizer" for VisualizerTypeComponent.
     */

    /// <summary>
    /// MEF Container class for Interactive Experience Action Type Components.
    /// </summary>
    internal class ActionTypeContainer : Container<ActionTypeComponent, IComponentData>
    {
        #region Fields

        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<ActionTypeContainer> _instance =
            new Lazy<ActionTypeContainer>( () => new ActionTypeContainer() );

        #endregion

        #region Properties

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static ActionTypeContainer Instance => _instance.Value;

        /// <summary>
        /// Gets all action type components that have been found in the system.
        /// </summary>
        /// <value>All action type components that have been found in the system.</value>
        public IEnumerable<ActionTypeComponent> AllComponents => Components.Values.Select( v => v.Value );

        /// <summary>
        /// Gets or sets the MEF components.
        /// </summary>
        /// <value>
        /// The MEF components.
        /// </value>
        [ImportMany( typeof( ActionTypeComponent ) )]
        protected override IEnumerable<Lazy<ActionTypeComponent, IComponentData>> MEFComponents { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Uses reflection to find any <see cref="FieldAttribute" /> attributes
        /// for the specified type and will create and/or update a
        /// <see cref="Rock.Model.Attribute" /> record for each attribute defined.
        /// This is a custom variation of the standard one in <see cref="Helper.UpdateAttributes(Type, int?, string, string, RockContext)"/>
        /// that handles filtering by prefix.
        /// </summary>
        /// <param name="type">The type (should be a <see cref="IHasAttributes" /> object).</param>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="keyPrefix">The prefix that attribute keys must have in order to be included.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if any attributes were created/updated.</returns>
        /// <remarks>
        /// If a rockContext value is included, this method will save any previous changes made to the context
        /// </remarks>
        internal static bool UpdateAttributes( Type type, int? entityTypeId, string keyPrefix, string entityQualifierColumn, string entityQualifierValue, RockContext rockContext )
        {
            bool attributesUpdated = false;
            bool attributesDeleted = false;

            if ( type == null )
            {
                return false;
            }

            var entityProperties = new List<FieldAttribute>();

            // Add any property attributes that were defined for the entity
            var customFieldAttributes = type.GetCustomAttributes( typeof( FieldAttribute ), true )
                .Cast<FieldAttribute>()
                .Where( a => a.Key.StartsWith( keyPrefix ) );

            foreach ( var customAttribute in customFieldAttributes )
            {
                entityProperties.Add( customAttribute );
            }

            rockContext = rockContext ?? new RockContext();

            // Create any attributes that need to be created
            foreach ( var entityProperty in entityProperties )
            {
                try
                {
                    attributesUpdated = Helper.UpdateAttribute( entityProperty, entityTypeId, entityQualifierColumn, entityQualifierValue, rockContext ) || attributesUpdated;
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( new Exception( string.Format( "Could not update an entity attribute ( Entity Type Id: {0}; Property Name: {1} ). ", entityTypeId, entityProperty.Name ), ex ), null );
                }
            }

            // Remove any old attributes
            try
            {
                var attributeService = new Model.AttributeService( rockContext );
                var existingKeys = entityProperties.Select( a => a.Key ).ToList();

                foreach ( var a in attributeService.GetByEntityTypeQualifier( entityTypeId, entityQualifierColumn, entityQualifierValue, true ).ToList() )
                {
                    if ( !existingKeys.Contains( a.Key ) && a.Key.StartsWith( keyPrefix ) )
                    {
                        attributeService.Delete( a );
                        attributesDeleted = true;
                    }
                }

                if ( attributesDeleted )
                {
                    rockContext.SaveChanges();
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( new Exception( "Could not delete one or more old attributes.", ex ), null );
            }

            return attributesUpdated;
        }

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
                foreach ( var actionComponent in AllComponents )
                {
                    var actionComponentType = actionComponent.GetType();
                    var actionComponentEntityTypeId = EntityTypeCache.Get( actionComponentType ).Id;
                    UpdateAttributes( actionComponentType, actionEntityTypeId, "action", nameof( InteractiveExperienceAction.ActionEntityTypeId ), actionComponentEntityTypeId.ToString(), rockContext );
                }
            }
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns>An instance of <see cref="ActionTypeComponent"/> or <c>null</c> if it was not found.</returns>
        public static ActionTypeComponent GetComponent( string entityType )
        {
            return Instance.GetComponentByEntity( entityType );
        }

        /// <summary>
        /// Gets the component with the matching Entity Type identifier.
        /// </summary>
        /// <param name="entityTypeId">The <see cref="EntityType"/> identifier.</param>
        /// <returns>An instance of <see cref="ActionTypeComponent"/> or <c>null</c> if it was not found.</returns>
        public static ActionTypeComponent GetComponentFromEntityType( int entityTypeId )
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
        /// <returns>An instance of <see cref="ActionTypeComponent"/> or <c>null</c> if it was not found.</returns>
        public static ActionTypeComponent GetComponent<T>()
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
        public static string GetComponentName( ActionTypeComponent component )
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
