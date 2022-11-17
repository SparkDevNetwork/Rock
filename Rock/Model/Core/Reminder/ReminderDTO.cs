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
using Rock.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Model
{
    /// <summary>
    /// DTO for Reminders.
    /// </summary>
    public class ReminderDTO
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
        /// The reminder type.
        /// </summary>
        public string ReminderType { get; set; }

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
        /// The entity identifier.
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// The Entity URL.
        /// </summary>
        public string EntityUrl { get; set; }

        /// <summary>
        /// Initializes the <see cref="ReminderDTO"/> instance.
        /// </summary>
        /// <param name="reminder">The <see cref="Reminder"/>.</param>
        /// <param name="entity">The <see cref="IEntity"/> that the reminder is associated with.</param>
        public ReminderDTO( Reminder reminder, IEntity entity )
        {
            this.Id = reminder.Id;
            this.IsComplete = reminder.IsComplete;
            this.IsRenewing = reminder.IsRenewing;
            this.EntityDescription = entity.ToString();
            this.ReminderType = reminder.ReminderType.Name;
            this.HighlightColor = reminder.ReminderType.HighlightColor;
            this.ReminderDate = reminder.ReminderDate.ToShortDateString();
            this.Note = reminder.Note;
            this.IsPersonReminder = ( reminder.ReminderType.EntityType.FriendlyName == "Person" );
            this.EntityId = reminder.EntityId;
            this.EntityUrl = string.Empty;

            var entityUrlPattern = reminder.ReminderType.EntityType.LinkUrlLavaTemplate;
            if ( !string.IsNullOrWhiteSpace( entityUrlPattern ) )
            {
                var entityUrlMergeFields = new Dictionary<string, object>();
                entityUrlMergeFields.Add( "Entity", entity );
                this.EntityUrl = reminder.ReminderType.EntityType.LinkUrlLavaTemplate.ResolveMergeFields( entityUrlMergeFields );
                this.EntityDescription = $"<a href=\"{this.EntityUrl}\">{this.EntityDescription}</a>";
            }
        }
    }
}
