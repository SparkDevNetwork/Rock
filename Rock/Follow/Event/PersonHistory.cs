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
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Follow.Event
{
    /// <summary>
    /// Following Event based on person history change
    /// </summary>
    /// <seealso cref="Rock.Follow.EventComponent" />
    [Description( "Person History" )]
    [Export( typeof( EventComponent ) )]
    [ExportMetadata( "ComponentName", "PersonHistory" )]

    [TextField( "Fields", "Field name(s) to monitor in history data. Separate multiple items by a comma. If you look at a person's history data it would be in the format of 'Modified FIELD value from OLD to NEW'.", true, order: 0 )]
    [IntegerField( "Max Days Back", "Maximum number of days back to look at a person's history.", true, 30, "", order: 1 )]

    [BooleanField( "Match Both", "Require a match on both the Old Value and the New Value. This equates to an AND comparison, otherwise it equates to an OR comparison on the values.", true, category: "Values", order: 0 )]
    [TextField( "Old Value", "Value to be matched as the old value or leave blank to match any old value.", false, category: "Values", order: 1 )]
    [TextField( "New Value", "Value to be matched as the new value or leave blank to match any new value.", false, category: "Values", order: 2 )]

    [BooleanField( "Negate Person", "Changes the Person match to a NOT Person match. If you want to trigger events only when it is NOT the specified person making the change then turn this option on.", category: "Changed By", order: 0 )]
    [PersonField( "Person", "Filter by the person who changed the value. This is always an AND condition with the two value changes. If the Negate Changed By option is also set then this becomes and AND NOT condition.", false, category: "Changed By", order: 1 )]
    public class PersonHistory : EventComponent
    {
        /// <summary>
        /// Gets the followed entity type identifier.
        /// </summary>
        /// <value>
        /// The followed entity type identifier.
        /// </value>
        public override Type FollowedType
        {
            get
            {
                return typeof( PersonAlias );
            }
        }

        /// <summary>
        /// Determines whether [has event happened] [the specified entity].
        /// </summary>
        /// <param name="followingEvent">The following event.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="lastNotified">The last notified.</param>
        /// <returns></returns>
        public override bool HasEventHappened( FollowingEventType followingEvent, IEntity entity, DateTime? lastNotified )
        {
            if ( followingEvent != null && entity != null )
            {
                var personAlias = entity as PersonAlias;

                if ( personAlias != null && personAlias.Person != null )
                {
                    //
                    // Get all the attributes/settings we need.
                    //
                    int daysBack = GetAttributeValue( followingEvent, "MaxDaysBack" ).AsInteger();
                    string targetOldValue = GetAttributeValue( followingEvent, "OldValue" ) ?? string.Empty;
                    string targetNewValue = GetAttributeValue( followingEvent, "NewValue" ) ?? string.Empty;
                    string targetPersonGuid = GetAttributeValue( followingEvent, "Person" );
                    bool negateChangedBy = GetAttributeValue( followingEvent, "NegatePerson" ).AsBoolean();
                    bool matchBothValues = GetAttributeValue( followingEvent, "MatchBoth" ).AsBoolean();
                    var attributes = GetAttributeValue( followingEvent, "Fields" ).Split( ',' ).Select( a => a.Trim() );

                    //
                    // Populate all the other random variables we need for processing.
                    //
                    PersonAlias targetPersonAlias = new PersonAliasService( new RockContext() ).Get( targetPersonGuid.AsGuid() );
                    DateTime daysBackDate = RockDateTime.Now.AddDays( -daysBack );
                    var person = personAlias.Person;
                    int personEntityTypeId = EntityTypeCache.Get( typeof( Person ) ).Id;
                    int categoryId = CategoryCache.Get( Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid() ).Id;

                    //
                    // Start building the basic query. We want all History items that are for
                    // people objects and use the Demographic Changes category.
                    //
                    var qry = new HistoryService( new RockContext() ).Queryable()
                        .Where( h => h.EntityTypeId == personEntityTypeId && h.EntityId == person.Id && h.CategoryId == categoryId );

                    //
                    // Put in our limiting dates. We always limit by our days back date,
                    // and conditionally limit based on the last time we notified the
                    // stalker - I mean the follower.
                    //
                    if ( lastNotified.HasValue )
                    {
                        qry = qry.Where( h => h.CreatedDateTime >= lastNotified.Value );
                    }
                    qry = qry.Where( h => h.CreatedDateTime >= daysBackDate );

                    //
                    // Walk each history item found that matches our filter.
                    //
                    Dictionary<string, List<PersonHistoryChange>> changes = new Dictionary<string, List<PersonHistoryChange>>();
                    foreach ( var history in qry.ToList() )
                    {
                        //
                        // Check what kind of change this was.
                        //
                        History.HistoryVerb? historyVerb = history.Verb.ConvertToEnumOrNull<History.HistoryVerb>();
                        string title = history.ValueName;

                        //
                        // Walk each attribute entered by the user to match against.
                        //
                        foreach ( var attribute in attributes )
                        {
                            PersonHistoryChange change = new PersonHistoryChange();
                            
                            change.Old = history.OldValue;
                            change.New = history.NewValue;

                            //
                            // Check if this is one of the attributes we are following.
                            //
                            if ( title != null && title.Trim() == attribute )
                            {
                                //
                                // Get the ValuePair object to work with.
                                //
                                if ( !changes.ContainsKey( attribute ) )
                                {
                                    changes.Add( attribute, new List<PersonHistoryChange>() );
                                }

                                change.PersonId = history.CreatedByPersonId;
                                changes[attribute].Add( change );

                                //
                                // If the value has been changed back to what it was then ignore the change.
                                //
                                if ( changes[attribute].Count >= 2 )
                                {
                                    var changesList = changes[attribute].ToList();

                                    if ( changesList[changesList.Count - 2].Old == changesList[changesList.Count - 1].New )
                                    {
                                        changes.Remove( title );
                                    }
                                }
                            }
                        }
                    }

                    //
                    // Walk the list of final changes and see if we need to notify.
                    //
                    foreach ( var items in changes.Values )
                    {
                        foreach ( PersonHistoryChange change in items )
                        {
                            //
                            // Check for a match on the person who made the change.
                            //
                            if ( targetPersonAlias == null
                                 || targetPersonAlias.Id == 0
                                 || ( !negateChangedBy && targetPersonAlias.PersonId == change.PersonId )
                                 || ( negateChangedBy && targetPersonAlias.PersonId != change.PersonId ) )
                            {
                                bool oldMatch = ( string.IsNullOrEmpty( targetOldValue ) || targetOldValue == change.Old );
                                bool newMatch = ( string.IsNullOrEmpty( targetNewValue ) || targetNewValue == change.New );

                                //
                                // If the old value and the new value match then trigger the event.
                                //
                                if ( ( matchBothValues && oldMatch && newMatch )
                                     || ( !matchBothValues && ( oldMatch || newMatch ) ) )
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Contains a list of changes for an attribute. This allows us to compile
        /// a list of changes and remove ones that were undone (e.g. in changes A, B, and C.
        /// Change C changes the value back to what it was before change A happened, therefore
        /// it becomes a non-op).
        /// </summary>
        class PersonHistoryChange
        {
            public string Old = string.Empty;

            public string New = string.Empty;

            public int? PersonId = 0;
        }
    }
}
