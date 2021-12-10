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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Model;

namespace Rock.Follow.Event
{
    /// <summary>
    /// Person Note Added Following Event
    /// </summary>
    [Description( "Person Note Added" )]
    [Export( typeof( EventComponent ) )]
    [ExportMetadata( "ComponentName", "PersonNoteAdded" )]

    [NoteTypeField( "Note Types",
        Description = "List of note types that may be followed.  Selecting none allows for all types to be followed.",
        AllowMultiple = true,
        IsRequired = false,
        Order = 0,
        EntityTypeName = "Rock.Model.Person",
        Key = AttributeKey.NoteTypes )]
    public class PersonNoteAdded : EventComponent, IEventComponentAdditionalMergeFields
    {
        /// <summary>
        /// The Follow Event setting attribute keys for PersonNoteAdded.
        /// </summary>
        public static class AttributeKey
        {
            /// <summary>
            /// The note types key
            /// </summary>
            public const string NoteTypes = "NoteTypes";
        }

        /// <summary>
        /// Gets the followed entity type identifier.
        /// </summary>
        /// <value>
        /// The followed entity type identifier.
        /// </value>
        public override Type FollowedType
        {
            get { return typeof( Rock.Model.PersonAlias ); }
        }

        /// <summary>
        /// Formats the entity notification with additional merge fields.
        /// </summary>
        /// <param name="followingEvent">The following event.</param>
        /// <param name="entity">The entity being followed.</param>
        /// <param name="additionalMergeFields">The collection of additional merge fields and values for the following event.</param>
        /// <returns>An HTML string from the Following Event (Lava formatted) to include in the notification email.</returns>
        public string FormatEntityNotification( FollowingEventType followingEvent, IEntity entity, Dictionary<string, List<object>> additionalMergeFields )
        {
            var sb = new StringBuilder();
            if ( followingEvent != null )
            {
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Entity", entity );
                foreach ( var mergeFieldPair in additionalMergeFields )
                {
                    // Add a new section to the email content for each note notification with merge fields.
                    foreach ( var mergeFieldValue in mergeFieldPair.Value )
                    {
                        var noteData = mergeFieldValue as NoteData;
                        mergeFields.AddOrReplace( mergeFieldPair.Key, noteData );
                        sb.Append( followingEvent.EntityNotificationFormatLava.ResolveMergeFields( mergeFields ) );
                    }
                }

                return sb.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Determines whether <see cref="PersonNoteAdded"/> has happened for <paramref name="entity"/> since <paramref name="lastNotified"/> date.
        /// This method will return a bool whether the aforementioned event(s) occurred.
        /// It will also output a dictionary including a list of Note Data POCOs that can be added to in the future.
        /// </summary>
        /// <param name="followingEvent">The following event.</param>
        /// <param name="entity">The entity being followed.</param>
        /// <param name="lastNotified">The last notified date.</param>
        /// <param name="followedEventObjects">The collection of the followed event's objects.</param>
        /// <returns>A bool representing whether the followed event(s) for the followed entity occurred since last notified.</returns>
        public bool HasEventHappened( FollowingEventType followingEvent, IEntity entity, DateTime? lastNotified, out Dictionary<string, List<object>> followedEventObjects )
        {
            followedEventObjects = new Dictionary<string, List<object>>();

            // If block is configured to only allow certain note types, limit notes to those types.
            var configuredNoteTypes = GetAttributeValue( followingEvent, AttributeKey.NoteTypes ).SplitDelimitedValues().AsGuidList();

            var personAlias = entity as PersonAlias;
            bool doEventEntityAndPersonHaveValues = followingEvent != null &&
                entity != null &&
                personAlias != null &&
                personAlias.Person != null;
            if ( doEventEntityAndPersonHaveValues )
            {
                var person = personAlias.Person;

                var newNotesForPersonSinceLastNotified = new NoteService( new RockContext() )
                    .Queryable().AsNoTracking()
                    .Where( n =>
                        n.EntityId == personAlias.PersonId
                        && !n.IsPrivateNote
                        && ( !lastNotified.HasValue || n.CreatedDateTime > lastNotified )
                        && ( configuredNoteTypes.Count() == 0 || configuredNoteTypes.Contains( n.NoteType.Guid ) ) ).ToList();

                if ( newNotesForPersonSinceLastNotified.Any() )
                {
                    followedEventObjects.Add( "NoteData", new List<object>( newNotesForPersonSinceLastNotified.Select( n => new NoteData { AddedBy = n.CreatedByPersonName, NoteType = n.NoteType.Name } ) ) );
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// (Not Implemented) Determines whether <see cref="PersonNoteAdded"/> has happened for <paramref name="entity"/> since <paramref name="lastNotified"/> date.
        /// </summary>
        /// <param name="followingEvent">The following event.</param>
        /// <param name="entity">The entity being followed.</param>
        /// <param name="lastNotified">The last notified date.</param>
        /// <returns>A bool representing whether the followed event(s) for the followed entity occurred since last notified.</returns>
        public override bool HasEventHappened( FollowingEventType followingEvent, IEntity entity, DateTime? lastNotified )
        {
            throw new NotImplementedException( "Exception occurred because this EventComponent implements an interface to extend the HasEventHappened method.  Please use HasEventHappened( FollowingEventType followingEvent, IEntity entity, DateTime? lastNotified, out Dictionary<string, List<object>> followedEventObjects )." );
        }

        /// <summary>
        /// A local object to extend LavaDataObject for use in collecting multiple new notes for a followed person.
        /// </summary>
        [Serializable]
        protected class NoteData : LavaDataObject
        {
            /// <summary>
            /// The name of the person who added the Note.
            /// </summary>
            public string AddedBy { get; set; }

            /// <summary>
            /// The name of the type of Note.
            /// </summary>
            public string NoteType { get; set; }
        }
    }
}
