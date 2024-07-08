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
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Enums.Event;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Event.InteractiveExperiences;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about an interactive experience required to obtain fast access
    /// to the details.
    /// </summary>
    [Serializable]
    [DataContract]
    internal class InteractiveExperienceCache : ModelCache<InteractiveExperienceCache, InteractiveExperience>
    {
        #region Fields

        /// <summary>
        /// Contains cached information for this interactive experience.
        /// </summary>
        /// <remarks>
        /// The purpose of this cache is to reduce load on the server when
        /// an experience starts and a few hundred people try to join the
        /// experience
        /// </remarks>
        private readonly MemoryCache _localCache = new MemoryCache( "Rock.Web.Cache.InteractiveExperienceCache._localCache", null, true );

        private static readonly CacheItemPolicy _cacheItemPolicy1Minute = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes( 1 ) };

        #endregion

        #region Properties

        /// <inheritdoc cref="InteractiveExperience.Name"/>
        [DataMember]
        public string Name { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.IsActive"/>
        [DataMember]
        public bool IsActive { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.Description"/>
        [DataMember]
        public string Description { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.PublicLabel"/>
        [DataMember]
        public string PublicLabel { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.PhotoBinaryFileId"/>
        [DataMember]
        public int? PhotoBinaryFileId { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.PushNotificationType"/>
        [DataMember]
        public InteractiveExperiencePushNotificationType PushNotificationType { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.PushNotificationTitle"/>
        [DataMember]
        public string PushNotificationTitle { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.PushNotificationDetail"/>
        [DataMember]
        public string PushNotificationDetail { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.WelcomeTitle"/>
        [DataMember]
        public string WelcomeTitle { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.WelcomeMessage"/>
        [DataMember]
        public string WelcomeMessage { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.WelcomeHeaderImageBinaryFileId"/>
        [DataMember]
        public int? WelcomeHeaderImageBinaryFileId { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.NoActionTitle"/>
        [DataMember]
        public string NoActionTitle { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.NoActionMessage"/>
        [DataMember]
        public string NoActionMessage { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.NoActionHeaderImageBinaryFileId"/>
        [DataMember]
        public int? NoActionHeaderImageBinaryFileId { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.ActionBackgroundColor"/>
        [DataMember]
        public string ActionBackgroundColor { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.ActionTextColor"/>
        [DataMember]
        public string ActionTextColor { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.ActionPrimaryButtonColor"/>
        [DataMember]
        public string ActionPrimaryButtonColor { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.ActionPrimaryButtonTextColor"/>
        [DataMember]
        public string ActionPrimaryButtonTextColor { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.ActionSecondaryButtonColor"/>
        [DataMember]
        public string ActionSecondaryButtonColor { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.ActionSecondaryButtonTextColor"/>
        [DataMember]
        public string ActionSecondaryButtonTextColor { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.ActionBackgroundImageBinaryFileId"/>
        [DataMember]
        public int? ActionBackgroundImageBinaryFileId { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.ActionCustomCss"/>
        [DataMember]
        public string ActionCustomCss { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.AudienceBackgroundColor"/>
        [DataMember]
        public string AudienceBackgroundColor { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.AudienceTextColor"/>
        [DataMember]
        public string AudienceTextColor { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.AudiencePrimaryColor"/>
        [DataMember]
        public string AudiencePrimaryColor { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.AudienceSecondaryColor"/>
        [DataMember]
        public string AudienceSecondaryColor { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.AudienceAccentColor"/>
        [DataMember]
        public string AudienceAccentColor { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.AudienceBackgroundImageBinaryFileId"/>
        [DataMember]
        public int? AudienceBackgroundImageBinaryFileId { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.AudienceCustomCss"/>
        [DataMember]
        public string AudienceCustomCss { get; private set; }

        /// <inheritdoc cref="InteractiveExperience.ExperienceSettingsJson"/>
        [DataMember]
        public string ExperienceSettingsJson { get; private set; }

        /// <summary>
        /// Gets the experience settings representing the additional settings
        /// not stored as database columns.
        /// </summary>
        /// <value>
        /// The experience settings representing the additional settings not
        /// stored as database columns.
        /// </value>
        [DataMember]
        public ExperienceSettingsBag ExperienceSettings { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the current occurrence identifiers for the experience. If any
        /// occurrences don't exist that should exist then they will be created.
        /// </summary>
        /// <returns>A list of integer identifiers for the current occurrences of the experience.</returns>
        internal IEnumerable<int> GetOrCreateAllCurrentOccurrenceIds()
        {
            var now = RockDateTime.Now;
            var cacheKey = $"occurrenceIds_{now.ToShortDateTimeString()}";

            if ( !( _localCache.Get( cacheKey ) is List<int> occurrenceIds ) )
            {
                occurrenceIds = InteractiveExperienceService.GetOrCreateAllCurrentOccurrenceIds( Id ).ToList();

                _localCache.Add( cacheKey, occurrenceIds, _cacheItemPolicy1Minute );
            }

            return occurrenceIds;
        }

        /// <summary>
        /// Gets the style bag for an interactive experience.
        /// </summary>
        /// <returns>An instance of <see cref="ExperienceStyleBag"/> that contains all the style information for the experience.</returns>
        internal ExperienceStyleBag GetExperienceStyleBag()
        {
            return new ExperienceStyleBag
            {
                Welcome = GetExperienceWelcomeStyleBag(),
                NoAction = GetExperienceNoActionStyleBag(),
                Action = GetExperienceActionStyleBag(),
                Visualizer = GetExperienceVisualizerStyleBag()
            };
        }

        /// <summary>
        /// Gets the welcome style bag for an interactive experience.
        /// </summary>
        /// <returns>An instance of <see cref="ExperiencePlaceholderStyleBag"/> that contains the welcome style information for the experience.</returns>
        internal ExperiencePlaceholderStyleBag GetExperienceWelcomeStyleBag()
        {
            return new ExperiencePlaceholderStyleBag
            {
                Title = WelcomeTitle,
                Message = WelcomeMessage,
                HeaderImage = WelcomeHeaderImageBinaryFileId.HasValue
                    ? FileUrlHelper.GetImageUrl( WelcomeHeaderImageBinaryFileId.Value )
                    : null
            };
        }

        /// <summary>
        /// Gets the no-action style bag for an interactive experience.
        /// </summary>
        /// <returns>An instance of <see cref="ExperiencePlaceholderStyleBag"/> that contains the no-action style information for the experience.</returns>
        internal ExperiencePlaceholderStyleBag GetExperienceNoActionStyleBag()
        {
            return new ExperiencePlaceholderStyleBag
            {
                Title = NoActionTitle,
                Message = NoActionMessage,
                HeaderImage = NoActionHeaderImageBinaryFileId.HasValue
                    ? FileUrlHelper.GetImageUrl( NoActionHeaderImageBinaryFileId.Value )
                    : null
            };
        }

        /// <summary>
        /// Gets the action style bag for an interactive experience.
        /// </summary>
        /// <returns>An instance of <see cref="ExperienceActionStyleBag"/> that contains the action style information for the experience.</returns>
        internal ExperienceActionStyleBag GetExperienceActionStyleBag()
        {
            return new ExperienceActionStyleBag
            {
                BackgroundColor = ActionBackgroundColor,
                BackgroundImage = ActionBackgroundImageBinaryFileId.HasValue
                    ? FileUrlHelper.GetImageUrl( ActionBackgroundImageBinaryFileId.Value )
                    : null,
                CustomCss = ActionCustomCss,
                PrimaryButtonColor = ActionPrimaryButtonColor,
                PrimaryButtonTextColor = ActionPrimaryButtonTextColor,
                SecondaryButtonColor = ActionSecondaryButtonColor,
                SecondaryButtonTextColor = ActionSecondaryButtonTextColor,
                TextColor = ActionTextColor
            };
        }

        /// <summary>
        /// Gets the visualizer style bag for an interactive experience.
        /// </summary>
        /// <returns>An instance of <see cref="ExperienceVisualizerStyleBag"/> that contains the visualizer style information for the experience.</returns>
        internal ExperienceVisualizerStyleBag GetExperienceVisualizerStyleBag()
        {
            return new ExperienceVisualizerStyleBag
            {
                AccentColor = AudienceAccentColor,
                BackgroundColor = AudienceBackgroundColor,
                BackgroundImage = AudienceBackgroundImageBinaryFileId.HasValue
                    ? FileUrlHelper.GetImageUrl( AudienceBackgroundImageBinaryFileId.Value )
                    : null,
                CustomCss = AudienceCustomCss,
                PrimaryColor = AudiencePrimaryColor,
                SecondaryColor = AudienceSecondaryColor,
                TextColor = AudienceTextColor
            };
        }

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( !( entity is InteractiveExperience interactiveExperience ) )
            {
                return;
            }

            Name = interactiveExperience.Name;
            IsActive = interactiveExperience.IsActive;
            Description = interactiveExperience.Description;
            PublicLabel = interactiveExperience.PublicLabel;
            PhotoBinaryFileId = interactiveExperience.PhotoBinaryFileId;
            PushNotificationType = interactiveExperience.PushNotificationType;
            PushNotificationTitle = interactiveExperience.PushNotificationTitle;
            PushNotificationDetail = interactiveExperience.PushNotificationDetail;
            WelcomeTitle = interactiveExperience.WelcomeTitle;
            WelcomeMessage = interactiveExperience.WelcomeMessage;
            WelcomeHeaderImageBinaryFileId = interactiveExperience.WelcomeHeaderImageBinaryFileId;
            NoActionTitle = interactiveExperience.NoActionTitle;
            NoActionMessage = interactiveExperience.NoActionMessage;
            NoActionHeaderImageBinaryFileId = interactiveExperience.NoActionHeaderImageBinaryFileId;
            ActionBackgroundColor = interactiveExperience.ActionBackgroundColor;
            ActionTextColor = interactiveExperience.ActionTextColor;
            ActionPrimaryButtonColor = interactiveExperience.ActionPrimaryButtonColor;
            ActionPrimaryButtonTextColor = interactiveExperience.ActionPrimaryButtonTextColor;
            ActionSecondaryButtonColor = interactiveExperience.ActionSecondaryButtonColor;
            ActionSecondaryButtonTextColor = interactiveExperience.ActionSecondaryButtonTextColor;
            ActionBackgroundImageBinaryFileId = interactiveExperience.ActionBackgroundImageBinaryFileId;
            ActionCustomCss = interactiveExperience.ActionCustomCss;
            AudienceBackgroundColor = interactiveExperience.AudienceBackgroundColor;
            AudienceTextColor = interactiveExperience.AudienceTextColor;
            AudiencePrimaryColor = interactiveExperience.AudiencePrimaryColor;
            AudienceSecondaryColor = interactiveExperience.AudienceSecondaryColor;
            AudienceAccentColor = interactiveExperience.AudienceAccentColor;
            AudienceBackgroundImageBinaryFileId = interactiveExperience.AudienceBackgroundImageBinaryFileId;
            AudienceCustomCss = interactiveExperience.AudienceCustomCss;
            ExperienceSettingsJson = interactiveExperience.ExperienceSettingsJson;
            ExperienceSettings = interactiveExperience.ExperienceSettingsJson.FromJsonOrNull<ExperienceSettingsBag>() ?? new ExperienceSettingsBag();
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}