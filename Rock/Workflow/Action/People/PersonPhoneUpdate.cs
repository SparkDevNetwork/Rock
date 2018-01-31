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

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Adds person to organization tag
    /// </summary>
    [ActionCategory( "People" )]
    [Description( "Updates the phone number of a person." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Person Phone Update" )]

    [WorkflowAttribute("Person", "Workflow attribute that contains the person to update.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [WorkflowAttribute( "Phone Type (From Attribute)", "The attribute that contains the phone number type to update.", false, "", "", 1, "PhoneTypeAttribute",
        new string[] { "Rock.Field.Types.DefinedValueFieldType" } )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE, "Phone Type", "The type of phone numer to update (if attribute is not specified or is an invalid value).", true, false, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME, "", 2 )]
    [WorkflowTextOrAttribute( "Phone Number", "Attribute Value", "The value or attribute value to set the phone number to. <span class='tip tip-lava'></span>", false, "", "", 3, "PhoneNumber" )]
    [WorkflowTextOrAttribute( "Unlisted", "Attribute Value", "The value or attribute value to indicate if number should be unlisted. Only valid values are 'True' or 'False' any other value will be ignored. <span class='tip tip-lava'></span>", false, "", "", 4, "Unlisted" )]
    [WorkflowTextOrAttribute( "Messaging Enabled", "Attribute Value", "The value or attribute value to indicate if messaging (SMS) should be enabled for phone. Only valid values are 'True' or 'False' any other value will be ignored. <span class='tip tip-lava'></span>", false, "", "", 5, "MessagingEnabled" )]
    [BooleanField("Ignore Blank Values", "If a value is blank should it be ignored, or should it be used to wipe out the current phone number?", true, order: 6)]
    public class PersonPhoneUpdate : ActionComponent
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

            // get person
            int? personId = null;

            string personAttributeValue = GetAttributeValue( action, "Person" );
            Guid? guidPersonAttribute = personAttributeValue.AsGuidOrNull();
            if ( guidPersonAttribute.HasValue )
            {
                var attributePerson = AttributeCache.Read( guidPersonAttribute.Value, rockContext );
                if ( attributePerson != null || attributePerson.FieldType.Class != "Rock.Field.Types.PersonFieldType" )
                {
                    string attributePersonValue = action.GetWorklowAttributeValue( guidPersonAttribute.Value );
                    if ( !string.IsNullOrWhiteSpace( attributePersonValue ) )
                    {
                        Guid personAliasGuid = attributePersonValue.AsGuid();
                        if ( !personAliasGuid.IsEmpty() )
                        {
                            personId = new PersonAliasService( rockContext ).Queryable()
                                .Where( a => a.Guid.Equals( personAliasGuid ) )
                                .Select( a => a.PersonId )
                                .FirstOrDefault();
                            if ( personId == null )
                            {
                                errorMessages.Add( string.Format( "Person could not be found for selected value ('{0}')!", guidPersonAttribute.ToString() ) );
                                return false;
                            }
                        }
                    }
                }
            }

            if ( personId == null )
            {
                errorMessages.Add( "The attribute used to provide the person was invalid, or not of type 'Person'." );
                return false;
            }

            // determine the phone type to edit
            DefinedValueCache phoneType = null;
            var phoneTypeAttributeValue = action.GetWorklowAttributeValue( GetAttributeValue( action, "PhoneTypeAttribute" ).AsGuid() );
            if ( phoneTypeAttributeValue != null )
            {
                phoneType = DefinedValueCache.Read( phoneTypeAttributeValue.AsGuid() );
            }
            if ( phoneType == null )
            {
                phoneType = DefinedValueCache.Read( GetAttributeValue( action, "PhoneType" ).AsGuid() );
            }
            if ( phoneType == null )
            {
                errorMessages.Add( "The phone type to be updated was not selected." );
                return false;
            }

            // get the ignore blank setting
            var ignoreBlanks = GetActionAttributeValue( action, "IgnoreBlankValues" ).AsBoolean( true );

            // get the new phone number value
            string phoneNumberValue = GetAttributeValue( action, "PhoneNumber" );
            Guid? phoneNumberValueGuid = phoneNumberValue.AsGuidOrNull();
            if ( phoneNumberValueGuid.HasValue )
            {
                phoneNumberValue = action.GetWorklowAttributeValue( phoneNumberValueGuid.Value );
            }
            else
            {
                phoneNumberValue = phoneNumberValue.ResolveMergeFields( GetMergeFields( action ) );
            }
            phoneNumberValue = PhoneNumber.CleanNumber( phoneNumberValue );

            // gets value indicating if phone number is unlisted
            string unlistedValue = GetAttributeValue( action, "Unlisted" );
            Guid? unlistedValueGuid = unlistedValue.AsGuidOrNull();
            if ( unlistedValueGuid.HasValue )
            {
                unlistedValue = action.GetWorklowAttributeValue( unlistedValueGuid.Value );
            }
            else
            {
                unlistedValue = unlistedValue.ResolveMergeFields( GetMergeFields( action ) );
            }
            bool? unlisted = unlistedValue.AsBooleanOrNull();

            // gets value indicating if messaging should be enabled for phone number
            string smsEnabledValue = GetAttributeValue( action, "MessagingEnabled" );
            Guid? smsEnabledValueGuid = smsEnabledValue.AsGuidOrNull();
            if ( smsEnabledValueGuid.HasValue )
            {
                smsEnabledValue = action.GetWorklowAttributeValue( smsEnabledValueGuid.Value );
            }
            else
            {
                smsEnabledValue = smsEnabledValue.ResolveMergeFields( GetMergeFields( action ) );
            }
            bool? smsEnabled = smsEnabledValue.AsBooleanOrNull();

            bool updated = false;
            var phoneNumberService = new PhoneNumberService( rockContext );
            var phoneNumber = phoneNumberService.Queryable()
                .Where( n =>
                    n.PersonId == personId.Value &&
                    n.NumberTypeValueId == phoneType.Id )
                .FirstOrDefault();
            string oldValue = string.Empty;
            if ( phoneNumber == null )
            {
                phoneNumber = new PhoneNumber { NumberTypeValueId = phoneType.Id, PersonId = personId.Value };
                phoneNumberService.Add( phoneNumber );
                updated = true;
            }
            else
            {
                oldValue = phoneNumber.NumberFormattedWithCountryCode;
            }

            if ( !string.IsNullOrWhiteSpace( phoneNumberValue ) || !ignoreBlanks )
            {
                updated = updated || phoneNumber.Number != phoneNumberValue;
                phoneNumber.Number = phoneNumberValue;
            }
            if ( unlisted.HasValue )
            {
                updated = updated || phoneNumber.IsUnlisted != unlisted.Value;
                phoneNumber.IsUnlisted = unlisted.Value;
            }
            if ( smsEnabled.HasValue )
            {
                updated = updated || phoneNumber.IsMessagingEnabled != smsEnabled.Value;
                phoneNumber.IsMessagingEnabled = smsEnabled.Value;
            }

            if ( updated )
            {
                var changes = new List<string>();
                History.EvaluateChange(
                    changes,
                    string.Format( "{0} Phone", phoneType.Value ),
                    oldValue,
                    phoneNumber.NumberFormattedWithCountryCode );

                if ( changes.Any() )
                {
                    changes.Add( string.Format( "<em>(Updated by the '{0}' workflow)</em>", action.ActionTypeCache.ActivityType.WorkflowType.Name ) );
                    HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(), personId.Value, changes, false );
                }

                rockContext.SaveChanges();

                if ( action.Activity != null && action.Activity.Workflow != null )
                {
                    var workflowType = action.Activity.Workflow.WorkflowTypeCache;
                    if ( workflowType != null && workflowType.LoggingLevel == WorkflowLoggingLevel.Action )
                    {
                        var person = new PersonService( rockContext ).Get( personId.Value );
                        action.AddLogEntry( string.Format( "Updated {0} phone for {1} to {2}.", phoneType.Value, person.FullName, phoneNumber.NumberFormattedWithCountryCode ) );
                    }
                }
            }

            return true;
        }
    }
}