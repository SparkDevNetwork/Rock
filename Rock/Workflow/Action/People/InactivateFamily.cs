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

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Inactivates a given person's entire family
    /// </summary>
    [ActionCategory( "People" )]
    [Description( "Inactivates a given person's entire family." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Family Inactivate" )]

    #region Attributes

    [WorkflowAttribute(
        "Person",
        Description = "The attribute that contains the person to use for the inactivation.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.Person,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" } )]

    [WorkflowTextOrAttribute(
        textLabel: "Inactive Reason",
        attributeLabel: "Attribute Value",
        description: "The value (guid of an existing defined value) or attribute to use for the inactivate reason. The attribute should be defined value.",
        required: false,
        order: 1,
        key: AttributeKey.InactiveReason,
        fieldTypeClassNames: new string[] { "Rock.Field.Types.DefinedValueFieldType" } )]

    [WorkflowTextOrAttribute(
        textLabel: "Inactive Note",
        attributeLabel: "Attribute Value",
        description: "The value or attribute to use for the inactivate reason note.",
        required: false,
        order: 2,
        key: AttributeKey.InactiveNote,
        fieldTypeClassNames: new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.MemoFieldType" } )]

    [CustomDropdownListField(
        "Multi-Family Logic",
        Description = "This determines what to do if the person is in more than one family.",
        ListSource = "0^Inactivate individuals in the primary family,1^Inactivate individuals in all families,2^Inactivate just the individual",
        IsRequired = true,
        Order = 3,
        Key = AttributeKey.MultiFamilyLogic )]

    #endregion Attributes

    public class InactivateFamily : ActionComponent
    {
        #region Workflow Attributes

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The attribute that contains the person use for the inactivation.
            /// </summary>
            public const string Person = "Person";

            /// <summary>
            /// The value (guid of an existing defined value) or attribute to use for the inactivate reason.
            /// </summary>
            public const string InactiveReason = "InactiveReason";

            /// <summary>
            /// The value or attribute to use for the inactivate reason note
            /// </summary>
            public const string InactiveNote = "InactiveNote";

            /// <summary>
            /// The family role Key
            /// </summary>
            public const string MultiFamilyLogic = "MultiFamilyLogic";
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
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var inactiveStatusValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ).Id;
            var guidPersonAttribute = GetAttributeValue( action, AttributeKey.Person ).AsGuid();
            Person person = GetPersonFromWorkflowAttribute( rockContext, action, guidPersonAttribute );

            if ( person == null )
            {
                var message = string.Format( "Person could not be found for selected value ('{0}')!", person.ToString() );
                errorMessages.Add( message );
                action.AddLogEntry( message, true );
                return false;
            }

            // If the configured inactiveReasonGuid is not in the inactiveReasonDefinedValues, then
            // that guid is probably from an attribute and therefore we need to get the inactiveReasonGuid
            // from the workflow's attribute value instead.
            Guid inactiveReasonGuid = GetAttributeValue( action, AttributeKey.InactiveReason ).AsGuid();
            var inactiveReasonDefinedValues = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON.AsGuid() ).DefinedValues;
            if ( !inactiveReasonDefinedValues.Any( a => a.Guid == inactiveReasonGuid ) )
            {
                var workflowAttributeValue = action.GetWorkflowAttributeValue( inactiveReasonGuid );

                if ( workflowAttributeValue.IsNotNullOrWhiteSpace() )
                {
                    inactiveReasonGuid = workflowAttributeValue.AsGuid();
                }
            }

            // Now can we do our final check to ensure the guid is a valid inactive reason.
            if ( !inactiveReasonDefinedValues.Any( a => a.Guid == inactiveReasonGuid ) )
            {
                var message = string.Format( "Inactive Reason could not be found for selected value ('{0}')!", inactiveReasonGuid.ToString() );
                errorMessages.Add( message );
                action.AddLogEntry( message, true );
                return false;
            }

            var inactiveReasonValueId = inactiveReasonDefinedValues.First( a => a.Guid == inactiveReasonGuid ).Id;

            string inactiveNote = GetAttributeValue( action, AttributeKey.InactiveNote );
            Guid guid = inactiveNote.AsGuid();

            if ( !guid.IsEmpty() )
            {
                inactiveNote = action.GetWorkflowAttributeValue( guid );
            }

            var personService = new PersonService( rockContext );
            var multiFamilyLogic = GetAttributeValue( action, AttributeKey.MultiFamilyLogic ).AsInteger();
            if ( multiFamilyLogic == (int) LogicOption.InactivateIndividualsPrimaryFamily )
            {
                if ( person.PrimaryFamily != null )
                {
                    var familyMembers = person.PrimaryFamily.Members.Select( a => a.Person );
                    foreach ( var familyMember in familyMembers.Where( a => a.RecordStatusValueId != inactiveStatusValueId ) )
                    {
                        personService.InactivatePerson( familyMember, DefinedValueCache.Get( inactiveReasonValueId ), inactiveNote );
                    }
                }
            }
            else if ( multiFamilyLogic == ( int ) LogicOption.InactivateIndividualsAllFamilies )
            {
                var familyMembers = person.GetFamilyMembers( true, rockContext ).Select( a => a.Person ).Distinct();
                foreach ( var familyMember in familyMembers.Where( a => a.RecordStatusValueId != inactiveStatusValueId ) )
                {
                    personService.InactivatePerson( familyMember, DefinedValueCache.Get( inactiveReasonValueId ), inactiveNote );
                }
            }
            else
            {
                personService.InactivatePerson( person, DefinedValueCache.Get( inactiveReasonValueId ), inactiveNote );
            }

            rockContext.SaveChanges();

            action.AddLogEntry( "Inactivate Family ran successfully." );
            return true;
        }

        /// <summary>
        /// Get Person from workflow attribute.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="guidPersonAttribute">The person attribute guid.</param>
        /// <returns></returns>
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

        /// <summary>
        /// The enum that corresponds with the MultiFamilyLogic block setting.
        /// </summary>
        private enum LogicOption
        {
            InactivateIndividualsPrimaryFamily = 0,
            InactivateIndividualsAllFamilies = 1,
            InactivateJustIndividual = 2
        }
    }
}