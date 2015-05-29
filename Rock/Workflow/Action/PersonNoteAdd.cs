// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Data.Entity;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sets an attribute's value to the selected person 
    /// </summary>
    [Description( "Adds a note the selected person." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Person Note Add" )]

    [WorkflowAttribute("Person", "Workflow attribute that contains the person to add the note to.", true, "", "", 0, null, new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [DefinedValueField("504BE755-2919-4738-952F-3EDF8B0F561A", "Source Type", "The note source type to use. If none is provided the default will be used.", false, false, "", "", 1)]
    [TextField( "Caption", "The title/caption of the note. If none is provided then the author's name will be displayed. <span class='tip tip-lava'></span>", false, "", "", 2 )]
    [MemoField("Text", "The body of the note. <span class='tip tip-lava'></span>", true, "", "", 3)]
    [WorkflowAttribute("Author", "Workflow attribute that contains the person to use as the author of the note. While not required it is recommended.", false, "", "", 4, null, new string[] { "Rock.Field.Types.PersonFieldType" })]
    [BooleanField("Alert", "Determines if the note should be flagged as an alert.", false, "", 5)]
    public class PersonNoteAdd : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var person = GetPersonAliasFromActionAttribute("Person", rockContext, action, errorMessages);
            if (person != null)
            {
                var mergeFields = GetMergeFields(action);
                
                var timeLineNoteType = new NoteTypeService(rockContext).Get(Rock.SystemGuid.NoteType.PERSON_TIMELINE.AsGuid());
                NoteService noteService = new NoteService(rockContext);

                Note note = new Note();
                note.NoteType = timeLineNoteType;
                note.EntityId = person.Id;
                note.SourceTypeValueId = DefinedValueCache.Read(GetAttributeValue(action, "SourceType")).Id;
                note.Caption = GetAttributeValue( action, "Caption" ).ResolveMergeFields(mergeFields);
                note.IsAlert = GetAttributeValue(action, "Alert").AsBoolean();
                note.Text = GetAttributeValue(action, "Text").ResolveMergeFields(mergeFields); ;

                // get author
                var author = GetPersonAliasFromActionAttribute("Author", rockContext, action, errorMessages);
                if (author != null)
                {
                    note.CreatedByPersonAliasId = author.PrimaryAlias.Id;
                }

                noteService.Add(note);
                rockContext.SaveChanges();

                return true;
            }
            else
            {
                errorMessages.Add("No person was provided for the note.");
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return true;
        }

        private Person GetPersonAliasFromActionAttribute(string key, RockContext rockContext, WorkflowAction action, List<string> errorMessages)
        {
            string value = GetAttributeValue( action, key );
            Guid guidPersonAttribute = value.AsGuid();
            if (!guidPersonAttribute.IsEmpty())
            {
                var attributePerson = AttributeCache.Read( guidPersonAttribute, rockContext );
                if (attributePerson != null)
                {
                    string attributePersonValue = action.GetWorklowAttributeValue(guidPersonAttribute);
                    if (!string.IsNullOrWhiteSpace(attributePersonValue))
                    {
                        if (attributePerson.FieldType.Class == "Rock.Field.Types.PersonFieldType")
                        {
                            Guid personAliasGuid = attributePersonValue.AsGuid();
                            if (!personAliasGuid.IsEmpty())
                            {
                                PersonAliasService personAliasService = new PersonAliasService(rockContext);
                                return personAliasService.Queryable().AsNoTracking()
                                    .Where(a => a.Guid.Equals(personAliasGuid))
                                    .Select(a => a.Person)
                                    .FirstOrDefault();
                            }
                            else
                            {
                                errorMessages.Add(string.Format("Person could not be found for selected value ('{0}')!", guidPersonAttribute.ToString()));
                                return null;
                            }
                        }
                        else
                        {
                            errorMessages.Add(string.Format("The attribute used for {0} to provide the person was not of type 'Person'.", key));
                            return null;
                        }
                    }
                }
            }

            return null;
        }

    }
}