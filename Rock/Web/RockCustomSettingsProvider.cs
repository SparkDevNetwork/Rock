﻿// <copyright>
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
using System.Reflection;
using System.Web.UI;

using Rock.Attribute;
using Rock.Data;

namespace Rock.Web
{
    /// <summary>
    /// Defines a UI provider for custom settings. Subclasses of this should also tag their implementation
    /// with the <see cref="Rock.Attribute.TargetTypeAttribute"/> to specify the class that will which we
    /// are defining a custom UI for.
    /// </summary>
    public abstract class RockCustomSettingsProvider
    {
        #region Private Fields

        /// <summary>
        /// The cached providers found through reflection.
        /// </summary>
        private static Dictionary<Type, List<Type>> _cachedProviders;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the custom settings title. Used when displaying tabs or links to these settings.
        /// </summary>
        /// <value>
        /// The custom settings title.
        /// </value>
        public virtual string CustomSettingsTitle => "Custom Settings";

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Gets the custom settings control. The returned control will be added to the parent automatically.
        /// </summary>
        /// <param name="attributeEntity">The attribute entity.</param>
        /// <param name="parent">The parent control that will eventually contain the returned control.</param>
        /// <returns>A control that contains all the custom UI.</returns>
        public abstract Control GetCustomSettingsControl( IHasAttributes attributeEntity, Control parent );

        /// <summary>
        /// Update the custom UI to reflect the current settings found in the entity.
        /// </summary>
        /// <param name="attributeEntity">The attribute entity.</param>
        /// <param name="control">The control returned by GetCustomSettingsControl() method.</param>
        public abstract void ReadSettingsFromEntity( IHasAttributes attributeEntity, Control control );

        /// <summary>
        /// Update the entity with values from the custom UI.
        /// </summary>
        /// <param name="attributeEntity">The attribute entity.</param>
        /// <param name="control">The control returned by the GetCustomSettingsControl() method.</param>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <remarks>
        /// Do not save the entity, it will be automatically saved later. This call will be made inside
        /// a SQL transaction for the passed rockContext. If you need to make changes to the database
        /// do so on this context so they can be rolled back if something fails during the final save.
        /// </remarks>
        public abstract void WriteSettingsToEntity( IHasAttributes attributeEntity, Control control, RockContext rockContext );

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets all the custom settings providers for the given type.
        /// </summary>
        /// <param name="type">The type whose custom settings providers we want to retrieve.</param>
        /// <returns>An enumerable collection of providers.</returns>
        public static IEnumerable<RockCustomSettingsProvider> GetProvidersForType( Type type )
        {
            if ( _cachedProviders == null )
            {
                InitializeCachedProviders();
            }

            while ( type != null )
            {
                if ( _cachedProviders.TryGetValue( type, out var providers ) )
                {
                    foreach ( var t in providers )
                    {
                        yield return ( RockCustomSettingsProvider ) Activator.CreateInstance( t );
                    }
                }

                type = type.BaseType;
            }
        }

        /// <summary>
        /// Initializes the cached providers.
        /// </summary>
        private static void InitializeCachedProviders()
        {
            var providers = new Dictionary<Type, List<Type>>();

            var types = Rock.Reflection.FindTypes( typeof( RockCustomSettingsProvider ) );
            foreach ( var kvp in types )
            {
                var targetType = kvp.Value.GetCustomAttribute<TargetTypeAttribute>()?.TargetType;

                if ( targetType != null )
                {
                    if ( providers.ContainsKey( targetType ) )
                    {
                        providers[targetType].Add( kvp.Value );
                    }
                    else
                    {
                        providers.Add( targetType, new List<Type> { kvp.Value } );
                    }
                }
            }

            _cachedProviders = providers;
        }

        #endregion
    }
}
