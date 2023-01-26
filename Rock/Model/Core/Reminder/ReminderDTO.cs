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
using Rock.Data;
using Rock.Utility;
using Rock.Web.Cache;

using System.Collections.Generic;

namespace Rock.Model
{
    /// <summary>
    /// DTO for Reminders.
    /// </summary>
    public class ReminderDTO : RockDynamic
    {
        /// <summary>
        /// The identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Is Complete.
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Is Renewing.
        /// </summary>
        public bool IsRenewing { get; set; }

        /// <summary>
        /// The entity description.
        /// </summary>
        public string EntityDescription { get; set; }

        /// <summary>
        /// The entity type (friendly) name.
        /// </summary>
        public string EntityTypeName { get; set; }

        /// <summary>
        /// The reminder type.
        /// </summary>
        public string ReminderTypeName { get; set; }

        /// <summary>
        /// The highlight color.
        /// </summary>
        public string HighlightColor { get; set; }

        /// <summary>
        /// The reminder date.
        /// </summary>
        public string ReminderDate { get; set; }

        /// <summary>
        /// The note.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// A value indicating whether or not this reminder is attached to a Person entity.
        /// </summary>
        public bool IsPersonReminder { get; set; }

        /// <summary>
        /// A value indicating whether or not this reminder is attached to a Group entity.
        /// </summary>
        public bool IsGroupReminder { get; set; }

        /// <summary>
        /// The entity identifier.
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// The entity type identifier.
        /// </summary>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// The Entity URL.
        /// </summary>
        public string EntityUrl { get; set; }

        /// <summary>
        /// The Person Profile Picture URL (must be set in the constructor, and should only be included if the entity is a Person).
        /// </summary>
        public string PersonProfilePictureUrl { get; set; }

        /// <summary>
        /// Initializes the <see cref="ReminderDTO"/> instance.
        /// </summary>
        /// <param name="reminder">The <see cref="Reminder"/>.</param>
        /// <param name="entity">The <see cref="IEntity"/> that the reminder is associated with.</param>
        /// <param name="personProfilePictureUrl">The </param>
        public ReminderDTO( Reminder reminder, IEntity entity, string personProfilePictureUrl = "" )
        {
            this.Id = reminder.Id;
            this.IsComplete = reminder.IsComplete;
            this.IsRenewing = reminder.IsRenewing;
            this.EntityDescription = entity.ToString();
            this.ReminderTypeName = reminder.ReminderType.Name;
            this.HighlightColor = reminder.ReminderType.HighlightColor;
            this.ReminderDate = reminder.ReminderDate.ToShortDateString();
            this.Note = reminder.Note;
            this.IsPersonReminder = ( reminder.ReminderType.EntityType.Guid == Rock.SystemGuid.EntityType.PERSON.AsGuid() );
            this.IsGroupReminder = (reminder.ReminderType.EntityType.Guid == Rock.SystemGuid.EntityType.GROUP.AsGuid());
            this.EntityId = reminder.EntityId;
            this.EntityTypeName = reminder.ReminderType.EntityType.FriendlyName;
            this.EntityTypeId = reminder.ReminderType.EntityTypeId;
            this.EntityUrl = string.Empty;
            this.PersonProfilePictureUrl = personProfilePictureUrl;

            var entityUrlPattern = reminder.ReminderType.EntityType.LinkUrlLavaTemplate;
            if ( !string.IsNullOrWhiteSpace( entityUrlPattern ) )
            {
                var entityUrlMergeFields = new Dictionary<string, object>();
                entityUrlMergeFields.Add( "Entity", entity );
                this.EntityUrl = reminder.ReminderType.EntityType.LinkUrlLavaTemplate.ResolveMergeFields( entityUrlMergeFields );
                if ( this.EntityUrl.StartsWith( "~/" ) )
                {
                    var baseUrl = GlobalAttributesCache.Value("PublicApplicationRoot");
                    this.EntityUrl = this.EntityUrl.Replace( "~/", baseUrl.EnsureTrailingForwardslash() );;
                }
            }
        }
    }
}
