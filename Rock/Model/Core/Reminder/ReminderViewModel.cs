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
    /// ViewModel for Reminders.
    /// </summary>
    public class ReminderViewModel : RockDynamic
    {
        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <value>The entity.</value>
        public IEntity Entity { get; private set; }

        /// <summary>
        /// Gets the reminder.
        /// </summary>
        /// <value>The reminder.</value>
        public Reminder Reminder { get; private set; }

        /// <summary>
        /// The identifier.
        /// </summary>
        public int Id { get { return this.Reminder.Id; } }

        /// <summary>
        /// Is Complete.
        /// </summary>
        public bool IsComplete { get { return this.Reminder.IsComplete; } }

        /// <summary>
        /// Is Renewing.
        /// </summary>
        public bool IsRenewing { get { return this.Reminder.IsRenewing; } }

        /// <summary>
        /// The entity description.
        /// </summary>
        public string EntityDescription { get { return Entity.ToString(); } }

        /// <summary>
        /// The entity type (friendly) name.
        /// </summary>
        public string EntityTypeName { get { return this.Reminder.ReminderType.EntityType.FriendlyName; } }

        /// <summary>
        /// The reminder type.
        /// </summary>
        public string ReminderTypeName { get { return this.Reminder.ReminderType.Name; } }

        /// <summary>
        /// The highlight color.
        /// </summary>
        public string HighlightColor { get { return this.Reminder.ReminderType.HighlightColor; } }

        /// <summary>
        /// The reminder date.
        /// </summary>
        public string ReminderDate { get { return this.Reminder.ReminderDate.ToShortDateString(); } }

        /// <summary>
        /// The note.
        /// </summary>
        public string Note { get { return this.Reminder.Note; } }

        /// <summary>
        /// A value indicating whether or not this reminder is attached to a PersonAlais entity.
        /// </summary>
        public bool IsPersonReminder { get { return ( this.Reminder.ReminderType.EntityType.Guid == Rock.SystemGuid.EntityType.PERSON_ALIAS.AsGuid() ); } }

        /// <summary>
        /// A value indicating whether or not this reminder is attached to a Group entity.
        /// </summary>
        public bool IsGroupReminder { get { return ( this.Reminder.ReminderType.EntityType.Guid == Rock.SystemGuid.EntityType.GROUP.AsGuid()); } }

        /// <summary>
        /// The entity identifier.
        /// </summary>
        public int EntityId { get { return this.Reminder.EntityId; } }

        /// <summary>
        /// The entity type identifier.
        /// </summary>
        public int EntityTypeId { get { return this.Reminder.ReminderType.EntityTypeId; } }

        /// <summary>
        /// The Entity URL.
        /// </summary>
        public string EntityUrl { get; private set; }

        /// <summary>
        /// The Person Profile Picture URL (must be set in the constructor, and should only be included if the entity is a Person).
        /// </summary>
        public string PersonProfilePictureUrl { get; private set; }

        /// <summary>
        /// Performs the Lava merge required to get the entity URL from the <see cref="EntityType.LinkUrlLavaTemplate"/> property.
        /// </summary>
        /// <returns>System.String.</returns>
        private string GetEntityUrl()
        {
            string entityUrl = string.Empty;
            var reminderEntityType = EntityTypeCache.Get( this.Reminder.ReminderType.EntityType );

            if ( reminderEntityType.Id == EntityTypeCache.GetId<PersonAlias>() )
            {
                // Get the entity URL pattern from Person instead of PersonAlias.
                reminderEntityType = EntityTypeCache.Get<Person>();
            }

            var entityUrlPattern = reminderEntityType.LinkUrlLavaTemplate;
            if ( !string.IsNullOrWhiteSpace( entityUrlPattern ) )
            {
                var entityUrlMergeFields = new Dictionary<string, object>
                {
                    { "Entity", this.Entity }
                };

                entityUrl = entityUrlPattern.ResolveMergeFields( entityUrlMergeFields );

                if ( entityUrl.StartsWith( "~/" ) )
                {
                    var baseUrl = GlobalAttributesCache.Value( "InternalApplicationRoot" );
                    entityUrl = entityUrl.Replace( "~/", baseUrl.EnsureTrailingForwardslash() );
                }
            }

            return entityUrl;
        }

        /// <summary>
        /// Initializes the <see cref="ReminderViewModel"/> instance.
        /// </summary>
        /// <param name="reminder">The <see cref="Rock.Model.Reminder"/>.</param>
        /// <param name="entity">The <see cref="IEntity"/> that the reminder is associated with.</param>
        /// <param name="personProfilePictureUrl">The person profile picture URL.  This optional parameter should be supplied
        /// if the entity attached to the reminder is a <see cref="Rock.Model.Person"/>).</param>
        public ReminderViewModel( Reminder reminder, IEntity entity, string personProfilePictureUrl = "" )
        {
            this.Reminder = reminder;
            this.Entity = entity;
            this.PersonProfilePictureUrl = personProfilePictureUrl;
            this.EntityUrl = GetEntityUrl();
        }
    }
}
