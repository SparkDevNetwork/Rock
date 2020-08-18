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
using System.Text;
using System.Threading.Tasks;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action.People
{
    /// <summary>
    /// Adds the selected Person to the same family as the 'Add to Family With' person
    /// </summary>
    [ActionCategory( "People" )]
    [Description( "Adds the selected Person to the same family as the 'Add to Family With' person." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Person Family Add" )]

    [WorkflowAttribute(
        "Person",
        Description = "The attribute that contains the person you wish to move to the new family.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.Person,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [WorkflowAttribute(
        "Add to Family With",
        Description = "The attribute that contains an individual in the family that you want to move the person  to.",
        IsRequired = true,
        Order = 1,
        Key = AttributeKey.AddToFamilyWith,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [WorkflowTextOrAttribute(
        "Family Role",
        attributeLabel: "Attribute Value",
        fieldTypeClassNames: new string[] { "Rock.Field.Types.GroupRoleFieldType" },
        Description = "The group role GUID or attribute that contains the family role to assign the person to.",
        IsRequired = true,
        Order = 2,
        Key = AttributeKey.FamilyRole )]
    [WorkflowTextOrAttribute(
        "Remove Person From Current Family",
        attributeLabel: "Attribute Value",
        fieldTypeClassNames: new string[] { "Rock.Field.Types.BooleanFieldType" },
        Description = "The Value True/False or an attribute that determines whether the person should be kept in their current family.",
        IsRequired = true,
        Order = 3,
        Key = AttributeKey.RemovePersonFromCurrentFamily )]
    public class AddPersonToFamily : ActionComponent
    {
        #region Workflow Attributes

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The person you wish to move to new family.
            /// </summary>
            public const string Person = "Person";

            /// <summary>
            /// The Add To Family With Person Key
            /// </summary>
            public const string AddToFamilyWith = "AddToFamilyWith";

            /// <summary>
            /// The remove from current family Key
            /// </summary>
            public const string RemovePersonFromCurrentFamily = "RemovePersonFromCurrentFamily";

            /// <summary>
            /// The family role Key
            /// </summary>
            public const string FamilyRole = "FamilyRole";
        }

        #endregion Workflow Attributes

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

            // get the Attribute.Guid for this workflow' AddToFamilyWiths Person Attribute so that we can lookup the value
            var guidAddToFamilyPersonAttribute = GetAttributeValue( action, AttributeKey.AddToFamilyWith ).AsGuid();
            Person addToFamilyWithPerson = GetPersonFromWorkflowAttribute( rockContext, action, guidAddToFamilyPersonAttribute );

            if ( addToFamilyWithPerson == null )
            {
                errorMessages.Add( string.Format( "Add To Family With Person could not be found for selected value ('{0}')!", guidAddToFamilyPersonAttribute.ToString() ) );
            }

            // get the Attribute.Guid for this workflow's Person Attribute so that we can lookup the value
            var guidPersonAttribute = GetAttributeValue( action, AttributeKey.Person ).AsGuid();
            Person person = GetPersonFromWorkflowAttribute( rockContext, action, guidPersonAttribute );

            if ( person == null )
            {
                errorMessages.Add( string.Format( "Person could not be found for selected value ('{0}')!", person.ToString() ) );
            }

            bool removePersonFromCurrentFamily = GetRemovePersonFromCurrentFamilyValue( action );


            Guid familyRoleGuid = GetAttributeValue( action, AttributeKey.FamilyRole ).AsGuid();
            var familyRoles = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() ).Roles;

            if ( !familyRoles.Any( a => a.Guid == familyRoleGuid ) )
            {
                var workflowAttributeValue = action.GetWorkflowAttributeValue( familyRoleGuid );

                if ( workflowAttributeValue != null )
                {
                    familyRoleGuid = workflowAttributeValue.AsGuid();
                }
            }

            if ( !familyRoles.Any( a => a.Guid == familyRoleGuid ) )
            {
                errorMessages.Add( string.Format( "Family Role could not be found for selected value ('{0}')!", familyRoleGuid.ToString() ) );
            }

            // Add Person to Group
            if ( !errorMessages.Any() )
            {
                // Determine which family to add the person to
                Group family = addToFamilyWithPerson.GetFamily( rockContext );
                var familyRole = familyRoles.First( a => a.Guid == familyRoleGuid );

                // Check if 
                if ( !family.Members
                        .Any( m =>
                                m.PersonId == person.Id ) )
                {

                    PersonService.AddPersonToFamily( person, false, family.Id, familyRole.Id, rockContext );
                    if ( removePersonFromCurrentFamily )
                    {
                        PersonService.RemovePersonFromOtherFamilies( family.Id, person.Id, rockContext );
                    }
                }
                else
                {
                    action.AddLogEntry( $"{person.FullName} was already a member of the family.", true );
                }
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return true;
        }

        private Person GetPersonFromWorkflowAttribute( RockContext rockContext, WorkflowAction action, Guid guidPersonAttribute )
        {
            Person person = null;

            var attributePerson = AttributeCache.Get( guidPersonAttribute, rockContext );
            if ( attributePerson != null )
            {
                string attributePersonValue = action.GetWorkflowAttributeValue( guidPersonAttribute );
                if ( !string.IsNullOrWhiteSpace( attributePersonValue ) )
                {
                    Guid personAliasGuid = attributePersonValue.AsGuid();
                    if ( !personAliasGuid.IsEmpty() )
                    {
                        person = new PersonAliasService( rockContext ).Queryable()
                            .Where( a => a.Guid.Equals( personAliasGuid ) )
                            .Select( a => a.Person )
                            .FirstOrDefault();
                    }
                }
            }

            return person;
        }

        private bool GetRemovePersonFromCurrentFamilyValue( WorkflowAction action )
        {
            bool removePersonFromCurrentFamily = false;
            // get Remove Person From Current Family
            string removePersonFromCurrentFamilyString = GetAttributeValue( action, AttributeKey.RemovePersonFromCurrentFamily );
            var guid = removePersonFromCurrentFamilyString.AsGuid();
            if ( guid.IsEmpty() )
            {
                removePersonFromCurrentFamily = removePersonFromCurrentFamilyString.AsBoolean();
            }
            else
            {
                var workflowAttributeValue = action.GetWorkflowAttributeValue( guid );

                if ( workflowAttributeValue != null )
                {
                    removePersonFromCurrentFamily = workflowAttributeValue.AsBoolean();
                }
            }

            return removePersonFromCurrentFamily;
        }
    }
}