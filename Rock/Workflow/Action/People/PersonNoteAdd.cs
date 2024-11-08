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
    [ActionCategory( "People" )]
    [Description( "Adds a note the selected person." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Person Note Add" )]

    #region Workflow Attributes

    [WorkflowAttribute( "Person",
        Description = "Workflow attribute that contains the person to add the note to.",
        Key = AttributeKey.Person,
        IsRequired = true,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" },
        Order = 0 )]

    [NoteTypeField( "Note Type",
        Description = "The type of note to add.",
        Key = AttributeKey.NoteType,
        AllowMultiple = false,
        EntityTypeName = "Rock.Model.Person",
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.NoteType.PERSON_TIMELINE_NOTE,
        Order = 1 )]

    [TextField( "Caption",
        Description = "The title/caption of the note. If none is provided then the author's name will be displayed. <span class='tip tip-lava'></span>",
        Key = AttributeKey.Caption,
        IsRequired = false,
        Order = 2 )]

    [MemoField( "Text",
        Description = "The body of the note. <span class='tip tip-lava'></span>",
        Key = AttributeKey.Text,
        IsRequired = true,
        Order = 3 )]

    [WorkflowAttribute( "Author",
        Description = "Workflow attribute that contains the person to use as the author of the note. While not required it is recommended.",
        Key = AttributeKey.Author,
        IsRequired = false,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" },
        Order = 4 )]

    [BooleanField( "Alert",
        Description = "Determines if the note should be flagged as an alert.",
        Key = AttributeKey.Alert,
        IsRequired = false,
        Order = 5 )]

    #endregion Workflow Attributes

    [Rock.SystemGuid.EntityTypeGuid( "B2C8B951-C41E-4DFB-9F92-F183223448AA")]
    public class PersonNoteAdd : ActionComponent
    {
        private static class AttributeKey
        {
            public const string Person = "Person";
            public const string NoteType = "NoteType";
            public const string Caption = "Caption";
            public const string Text = "Text";
            public const string Author = "Author";
            public const string Alert = "Alert";
        }

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var person = GetPersonAliasFromActionAttribute( AttributeKey.Person, rockContext, action, errorMessages );
            if ( person != null )
            {
                var mergeFields = GetMergeFields( action );

                NoteService noteService = new NoteService( rockContext );

                Note note = new Note();
                note.EntityId = person.Id;
                note.Caption = GetAttributeValue( action, AttributeKey.Caption ).ResolveMergeFields( mergeFields );
                note.IsAlert = GetAttributeValue( action, AttributeKey.Alert ).AsBoolean();
                note.IsPrivateNote = false;
                note.Text = GetAttributeValue( action, AttributeKey.Text ).ResolveMergeFields( mergeFields );
                note.EditedDateTime = RockDateTime.Now;

                // Add a NoteUrl for a person based on the existing Person Profile page, routes, and parameters.
                var personPageService = new PageService( rockContext );
                var personPage = personPageService.Get( SystemGuid.Page.PERSON_PROFILE_PERSON_PAGES.AsGuid() );

                // If the Person Profile Page has at least one route and at least one Page Context, use those to build the Note URL.
                if ( personPage.PageRoutes.Count > 0 && personPage.PageContexts.Count > 0 )
                {
                    var personPageRoute = personPage.PageRoutes.First();
                    var personPageContext = personPage.PageContexts.First();
                    var personIdParameter = personPageContext.IdParameter;

                    var personPageParameter = new Dictionary<string, string> { { personIdParameter, person.Id.ToString() } };
                    var personPageReference = new Web.PageReference( personPage.Id, personPageRoute.Id );
                    note.NoteUrl = personPageReference.BuildRouteURL( personPageParameter );
                }

                var noteType = NoteTypeCache.Get( GetAttributeValue( action, AttributeKey.NoteType ).AsGuid() );
                if ( noteType != null )
                {
                    note.NoteTypeId = noteType.Id;
                }

                // get author
                var author = GetPersonAliasFromActionAttribute( AttributeKey.Author, rockContext, action, errorMessages );
                if ( author != null )
                {
                    note.CreatedByPersonAliasId = author.PrimaryAlias.Id;
                    note.EditedByPersonAliasId = author.PrimaryAlias.Id;
                }

                noteService.Add( note );
                rockContext.SaveChanges();

                return true;
            }
            else
            {
                errorMessages.Add( "No person was provided for the note." );
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return true;
        }

        private Person GetPersonAliasFromActionAttribute( string key, RockContext rockContext, WorkflowAction action, List<string> errorMessages )
        {
            string value = GetAttributeValue( action, key );
            Guid guidPersonAttribute = value.AsGuid();
            if ( guidPersonAttribute.IsEmpty() )
            {
                return null;
            }

            var attributePerson = AttributeCache.Get( guidPersonAttribute, rockContext );
            if ( attributePerson == null )
            {
                return null;
            }

            string attributePersonValue = action.GetWorkflowAttributeValue( guidPersonAttribute );
            if ( string.IsNullOrWhiteSpace( attributePersonValue ) )
            {
                return null;
            }

            if ( attributePerson.FieldType.Class != "Rock.Field.Types.PersonFieldType" )
            {
                errorMessages.Add( $"The attribute used for {key} to provide the person was not of type 'Person'." );
                return null;
            }

            Guid personAliasGuid = attributePersonValue.AsGuid();
            if ( personAliasGuid.IsEmpty() )
            {
                errorMessages.Add( $"Person could not be found for selected value ('{guidPersonAttribute}')!" );
                return null;
            }

            PersonAliasService personAliasService = new PersonAliasService( rockContext );
            return personAliasService.Queryable().AsNoTracking()
                .Where( a => a.Guid.Equals( personAliasGuid ) )
                .Select( a => a.Person )
                .FirstOrDefault();
        }
    }
}