using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.UI;

using Rock.Attribute;

namespace Rock.Web
{
    /// <summary>
    /// Defines a UI provider for custom settings. Subclasses of this should also tag their implementation
    /// with the <see cref="Rock.Blocks.TargetTypeAttribute"/> to specify the class that will which we
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
        /// <remarks>Do not save the entity, it will be automatically saved later.</remarks>
        public abstract void WriteSettingsToEntity( IHasAttributes attributeEntity, Control control );

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

            if ( !_cachedProviders.ContainsKey( type ) )
            {
                yield break;
            }

            foreach ( var t in _cachedProviders[type] )
            {
                yield return (RockCustomSettingsProvider)Activator.CreateInstance( t );
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
