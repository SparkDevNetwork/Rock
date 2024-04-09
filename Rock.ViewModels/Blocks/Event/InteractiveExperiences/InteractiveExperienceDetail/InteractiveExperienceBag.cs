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

using System.Collections.Generic;

using Rock.ViewModels.Utility;
using Rock.Enums.Event;

namespace Rock.ViewModels.Blocks.Event.InteractiveExperiences.InteractiveExperienceDetail
{
    /// <summary>
    /// Class InteractiveExperienceBag.
    /// Implements the <see cref="Rock.ViewModels.Utility.EntityBagBase" />
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class InteractiveExperienceBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the background color of the action.
        /// </summary>
        public string ActionBackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the action background image binary file.
        /// </summary>
        public ListItemBag ActionBackgroundImageBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the custom css for the action.
        /// </summary>
        public string ActionCustomCss { get; set; }

        /// <summary>
        /// Gets or sets the primary button color of the action.
        /// </summary>
        public string ActionPrimaryButtonColor { get; set; }

        /// <summary>
        /// Gets or sets the primary button text color of the action.
        /// </summary>
        public string ActionPrimaryButtonTextColor { get; set; }

        /// <summary>
        /// Gets or sets the secondary button color of the action.
        /// </summary>
        public string ActionSecondaryButtonColor { get; set; }

        /// <summary>
        /// Gets or sets the secondary button text color of the action.
        /// </summary>
        public string ActionSecondaryButtonTextColor { get; set; }

        /// <summary>
        /// Gets or sets the text color of the action.
        /// </summary>
        public string ActionTextColor { get; set; }

        /// <summary>
        /// Gets or sets the accent color for the audience.
        /// </summary>
        public string AudienceAccentColor { get; set; }

        /// <summary>
        /// Gets or sets the background color for the audience.
        /// </summary>
        public string AudienceBackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the audience background image binary file.
        /// </summary>
        public ListItemBag AudienceBackgroundImageBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the custom css for the audience.
        /// </summary>
        public string AudienceCustomCss { get; set; }

        /// <summary>
        /// Gets or sets the primary color for the audience.
        /// </summary>
        public string AudiencePrimaryColor { get; set; }

        /// <summary>
        /// Gets or sets the secondary color for the audience.
        /// </summary>
        public string AudienceSecondaryColor { get; set; }

        /// <summary>
        /// Gets or sets the text color for the audience.
        /// </summary>
        public string AudienceTextColor { get; set; }

        /// <summary>
        /// The behavior of campus choices for this experience. See the description
        /// of the individual enum values for specific functionality.
        /// </summary>
        public InteractiveExperienceCampusBehavior CampusBehavior { get; set; }

        /// <summary>
        /// The default campus to use when recording an Interaction if no other
        /// campus could be determined.
        /// </summary>
        public ListItemBag DefaultCampus { get; set; }

        /// <summary>
        /// Gets or sets the Description of the Rock.Model.InteractiveExperience
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the lava template to use after the experience has ended.
        /// </summary>
        public string ExperienceEndedTemplate { get; set; }

        /// <summary>
        /// Gets or sets the InteractiveExperienceActions  for this Interactive Experience.
        /// </summary>
        public List<InteractiveExperienceActionBag> Actions { get; set; }

        /// <summary>
        /// Gets or sets the schedule attributes.
        /// </summary>
        /// <value>
        /// The schedule attributes.
        /// </value>
        [TypeScriptType( "Record<string, PublicAttribute> | null", "import { PublicAttribute } from './publicAttribute';" )]
        public Dictionary<string, PublicAttributeBag> ScheduleAttributes { get; set; }

        /// <summary>
        /// Gets or sets the schedules associated with this interactive experience.
        /// </summary>
        public List<InteractiveExperienceScheduleBag> Schedules { get; set; }

        /// <summary>
        /// Gets or sets the IsActive flag for the Rock.Model.InteractiveExperience.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the Name of the InteractiveExperience. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the no action header image binary file.
        /// </summary>
        public ListItemBag NoActionHeaderImageBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the no action message.
        /// </summary>
        public string NoActionMessage { get; set; }

        /// <summary>
        /// Gets or sets the no action title.
        /// </summary>
        public string NoActionTitle { get; set; }

        /// <summary>
        /// Gets or sets the photo binary file.
        /// </summary>
        public ListItemBag PhotoBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the Public Label of the InteractiveExperience.
        /// </summary>
        public string PublicLabel { get; set; }

        /// <summary>
        /// Gets or sets the detail message of the push notification.
        /// </summary>
        public string PushNotificationDetail { get; set; }

        /// <summary>
        /// Gets or sets the title of the push notification.
        /// </summary>
        public string PushNotificationTitle { get; set; }

        /// <summary>
        /// Gets or sets the push notification type.
        /// </summary>
        public InteractiveExperiencePushNotificationType PushNotificationType { get; set; }

        /// <summary>
        /// Gets or sets the welcome header image binary file.
        /// </summary>
        public ListItemBag WelcomeHeaderImageBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the welcome message.
        /// </summary>
        public string WelcomeMessage { get; set; }

        /// <summary>
        /// Gets or sets the welcome title.
        /// </summary>
        public string WelcomeTitle { get; set; }
    }
}
