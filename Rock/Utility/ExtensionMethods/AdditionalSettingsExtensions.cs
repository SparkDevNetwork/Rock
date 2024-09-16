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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Attribute;
using Rock.Data;

namespace Rock
{
    /// <summary>
    /// Extension methods to facilitate the consistent saving and retrieval of a model's categorized, additional settings property value.
    /// </summary>
    public static class AdditionalSettingsExtensions
    {
        #region Properties

        /// <summary>
        /// The settings that should be used when serializing and deserializing additional settings JSON objects,
        /// silently handling errors and minimizing whitespace.
        /// </summary>
        private static JsonSerializerSettings JsonSerializerSettings
        {
            get
            {
                if ( _jsonSerializerSettings == null )
                {
                    _jsonSerializerSettings = JsonExtensions.GetSerializeSettings( indentOutput: false, ignoreErrors: true, camelCase: false );
                }

                return _jsonSerializerSettings;
            }
        }

        private static JsonSerializerSettings _jsonSerializerSettings;

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Gets the deserialized settings object matching the provided <typeparamref name="TSettings"/> <see cref="System.Type"/> from
        /// <see cref="IHasReadOnlyAdditionalSettings.AdditionalSettingsJson"/>.
        /// <para>
        /// This will never return <c>null</c>. If <see cref="IHasReadOnlyAdditionalSettings.AdditionalSettingsJson"/> doesn't already
        /// have a settings object of this type, a new instance will be created and returned.
        /// </para>
        /// </summary>
        /// <typeparam name="TSettings">
        /// The <see cref="System.Type"/> of category settings object into which the underlying JSON string should be deserialized.
        /// </typeparam>
        /// <param name="settings">The <see cref="IHasReadOnlyAdditionalSettings"/> instance containing the desired, categorized settings.</param>
        /// <returns>The deserialized settings object or a new instance if one didn't already exist.</returns>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.16.4" )]
        public static TSettings GetAdditionalSettings<TSettings>( this IHasReadOnlyAdditionalSettings settings ) where TSettings : class, new()
        {
            return settings.GetAdditionalSettingsOrNull<TSettings>( typeof( TSettings ).Name ) ?? new TSettings();
        }

        /// <summary>
        /// Gets the deserialized settings object matching the provided <typeparamref name="TSettings"/> <see cref="System.Type"/> from
        /// <see cref="IHasReadOnlyAdditionalSettings.AdditionalSettingsJson"/>.
        /// </summary>
        /// <typeparam name="TSettings">
        /// The <see cref="System.Type"/> of category settings object into which the underlying JSON string should be deserialized.
        /// </typeparam>
        /// <param name="settings">The <see cref="IHasReadOnlyAdditionalSettings"/> instance containing the desired, categorized settings.</param>
        /// <returns>The deserialized settings object or <see langword="null"/> if one didn't already exist.</returns>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.16.4" )]
        public static TSettings GetAdditionalSettingsOrNull<TSettings>( this IHasReadOnlyAdditionalSettings settings ) where TSettings : class, new()
        {
            return settings.GetAdditionalSettingsOrNull<TSettings>( typeof( TSettings ).Name );
        }

        /// <summary>
        /// <para>
        /// Gets the deserialized settings object matching the provided <paramref name="categoryKey"/> from
        /// <see cref="IHasReadOnlyAdditionalSettings.AdditionalSettingsJson"/>.
        /// </para>
        /// <para>
        /// This will never return <c>null</c>. If <see cref="IHasReadOnlyAdditionalSettings.AdditionalSettingsJson"/> doesn't already
        /// have a settings object of this type, a new instance will be created and returned.
        /// </para>
        /// </summary>
        /// <typeparam name="TSettings">
        /// The <see cref="System.Type"/> of category settings object into which the underlying JSON string should be deserialized.
        /// </typeparam>
        /// <param name="settings">The <see cref="IHasReadOnlyAdditionalSettings"/> instance containing the desired, categorized settings.</param>
        /// <param name="categoryKey">The category key of the settings object to be returned.</param>
        /// <returns>The deserialized settings object or a new instance if one didn't already exist.</returns>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.16.4" )]
        public static TSettings GetAdditionalSettings<TSettings>( this IHasReadOnlyAdditionalSettings settings, string categoryKey ) where TSettings : class, new()
        {
            return settings.GetAdditionalSettingsOrNull<TSettings>( categoryKey ) ?? new TSettings();
        }

        /// <summary>
        /// Gets the deserialized settings object matching the provided <paramref name="categoryKey"/> from
        /// <see cref="IHasReadOnlyAdditionalSettings.AdditionalSettingsJson"/>. If it can't be found or deserialized,
        /// returns <see langword="null"/>.
        /// </summary>
        /// <typeparam name="TSettings">
        /// The <see cref="System.Type"/> of category settings object into which the underlying JSON string should be deserialized.
        /// </typeparam>
        /// <param name="settings">The <see cref="IHasReadOnlyAdditionalSettings"/> instance containing the desired, categorized settings.</param>
        /// <param name="categoryKey">The category key of the settings object to be returned.</param>
        /// <returns>The deserialized settings object or <see langword="null"/> if not found or deserialization fails.</returns>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.16.4" )]
        public static TSettings GetAdditionalSettingsOrNull<TSettings>( this IHasReadOnlyAdditionalSettings settings, string categoryKey ) where TSettings : class, new()
        {
            if ( categoryKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var root = GetAdditionalSettingsRoot( settings );
            if ( root != null && root.TryGetValue( categoryKey, out var value ) )
            {
                // Ignore any deserialization errors (this might return null).
                return value.ToObject<TSettings>( JsonSerializer.Create( JsonSerializerSettings ) );
            }

            return null;
        }

        /// <summary>
        /// Serializes and sets the provided settings object within <see cref="IHasAdditionalSettings.AdditionalSettingsJson"/>,
        /// assigning it to a property matching the name of the <typeparamref name="TSettings"/> <see cref="System.Type"/>.
        /// <para>
        /// Changes made to the database are not saved until you call <see cref="DbContext.SaveChanges()"/>.
        /// </para>
        /// <para>
        /// If the provided <paramref name="categorySettings"/> object serialization fails, <see cref="IHasAdditionalSettings.AdditionalSettingsJson"/>
        /// will not be modified.
        /// </para>
        /// <para>
        /// If <paramref name="settings"/> is <see langword="null"/> then the setting will be removed.
        /// </para>
        /// </summary>
        /// <typeparam name="TSettings">The <see cref="System.Type"/> of category settings object.</typeparam>
        /// <param name="settings">The <see cref="IHasAdditionalSettings"/> instance into which the settings should be set.</param>
        /// <param name="categorySettings">The settings object to be to be serialized and set.</param>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.16.4" )]
        public static void SetAdditionalSettings<TSettings>( this IHasAdditionalSettings settings, TSettings categorySettings ) where TSettings : class, new()
        {
            settings.SetAdditionalSettings( typeof( TSettings ).Name, categorySettings );
        }

        /// <summary>
        /// Serializes and sets the provided settings object within <see cref="IHasAdditionalSettings.AdditionalSettingsJson"/>,
        /// assigning it to a property matching the provided <paramref name="categoryKey"/>.
        /// <para>
        /// Changes made to the database are not saved until you call <see cref="DbContext.SaveChanges()"/>.
        /// </para>
        /// <para>
        /// If the provided <paramref name="categorySettings"/> object serialization fails, <see cref="IHasAdditionalSettings.AdditionalSettingsJson"/>
        /// will not be modified.
        /// </para>
        /// <para>
        /// If <paramref name="settings"/> is <see langword="null"/> then the setting will be removed.
        /// </para>
        /// </summary>
        /// <param name="settings">The <see cref="IHasAdditionalSettings"/> instance into which the settings should be set.</param>
        /// <param name="categoryKey">The category key of the settings to be set within the <see cref="IHasAdditionalSettings"/> instance.</param>
        /// <param name="categorySettings">The settings object to be to be serialized and set.</param>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.16.4" )]
        public static void SetAdditionalSettings( this IHasAdditionalSettings settings, string categoryKey, object categorySettings )
        {
            if ( categoryKey.IsNullOrWhiteSpace() )
            {
                return;
            }

            if ( categorySettings == null )
            {
                settings.RemoveAdditionalSettings( categoryKey );
                return;
            }

            var root = GetAdditionalSettingsRoot( settings ) ?? new JObject();

            // Ignore any serialization errors (this might return null);
            var token = JToken.FromObject( categorySettings, JsonSerializer.Create( JsonSerializerSettings ) );
            if ( token == null )
            {
                return;
            }

            root.AddOrReplace( categoryKey, token );

            settings.AdditionalSettingsJson = JsonConvert.SerializeObject( root, JsonSerializerSettings );
        }

        /// <summary>
        /// Removes the category settings object matching the provided <typeparamref name="TSettings"/> <see cref="System.Type"/>
        /// from <see cref="IHasAdditionalSettings.AdditionalSettingsJson"/> if it exists.
        /// </summary>
        /// <typeparam name="TSettings">The <see cref="System.Type"/> of category settings object to be removed.</typeparam>
        /// <param name="settings">The <see cref="IHasAdditionalSettings"/> instance containing the category settings to be removed.</param>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.16.4" )]
        public static void RemoveAdditionalSettings<TSettings>( this IHasAdditionalSettings settings ) where TSettings : class, new()
        {
            settings.RemoveAdditionalSettings( typeof( TSettings ).Name );
        }

        /// <summary>
        /// Removes the category settings object matching the specified <paramref name="categoryKey"/> from
        /// <see cref="IHasAdditionalSettings.AdditionalSettingsJson"/> if it exists.
        /// </summary>
        /// <param name="settings">The <see cref="IHasAdditionalSettings"/> instance containing the category settings to be removed.</param>
        /// <param name="categoryKey">The category key of the settings object to be removed.</param>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.16.4" )]
        public static void RemoveAdditionalSettings( this IHasAdditionalSettings settings, string categoryKey )
        {
            if ( categoryKey.IsNullOrWhiteSpace() )
            {
                return;
            }

            var root = GetAdditionalSettingsRoot( settings );
            if ( root == null || !root.ContainsKey( categoryKey ) )
            {
                return;
            }

            root.Remove( categoryKey );

            settings.AdditionalSettingsJson = JsonConvert.SerializeObject( root, JsonSerializerSettings );
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Gets the deserialized root object from <see cref="IHasReadOnlyAdditionalSettings.AdditionalSettingsJson"/>.
        /// </summary>
        /// <param name="settings">The <see cref="IHasReadOnlyAdditionalSettings"/> object containing the serialized additional settings.</param>
        /// <returns>The deserialized root object from <see cref="IHasReadOnlyAdditionalSettings.AdditionalSettingsJson"/>.</returns>
        private static JObject GetAdditionalSettingsRoot( this IHasReadOnlyAdditionalSettings settings )
        {
            if ( settings?.AdditionalSettingsJson.IsNotNullOrWhiteSpace() != true )
            {
                return null;
            }

            // Ignore any deserialization errors (this might return null).
            var root = JsonConvert.DeserializeObject<JObject>( settings.AdditionalSettingsJson, JsonSerializerSettings );

            return root;
        }

        #endregion Private Methods
    }
}
