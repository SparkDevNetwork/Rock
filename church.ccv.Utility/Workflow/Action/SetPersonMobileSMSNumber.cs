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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;

namespace church.ccv.Utility.Workflow.Action
{
    /// <summary>
    /// Sets an attribute's value to the selected person 
    /// </summary>
    [ActionCategory( "Person Update" )]
    [Description( "Sets a phone number to be that person's SMS-enabled mobile number. Will add the phone number if it isn't there." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Set Person Attribute" )]

    [WorkflowAttribute("Person", "Workflow attribute that contains the person to update.", true, "", "", 0, null, 
        new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [WorkflowTextOrAttribute( "Value", "Attribute Value", "The value or attribute value to set the person attribute to. <span class='tip tip-lava'></span>", false, "", "", 2, "Value" )]
    public class SetPersonMobileSMSNumber : ActionComponent
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

            string updateValue = GetAttributeValue( action, "Value" );
            Guid? valueGuid = updateValue.AsGuidOrNull();
            if ( valueGuid.HasValue )
            {
                updateValue = action.GetWorklowAttributeValue( valueGuid.Value );
            }
            else
            {
                updateValue = updateValue.ResolveMergeFields( GetMergeFields( action ) );
            }

            // make sure what they typed in qualifies as a phone number
            string phoneNumber = PhoneNumber.CleanNumber( updateValue );
            if( string.IsNullOrWhiteSpace( phoneNumber ) == false )
            {
                string value = GetAttributeValue( action, "Person" );
                Guid guid = value.AsGuid();
                if ( !guid.IsEmpty() )
                {
                    var attributePerson = AttributeCache.Read( guid, rockContext );
                    if ( attributePerson != null )
                    {
                        string attributePersonValue = action.GetWorklowAttributeValue( guid );
                        if ( !string.IsNullOrWhiteSpace( attributePersonValue ) )
                        {
                            if ( attributePerson.FieldType.Class == "Rock.Field.Types.PersonFieldType" )
                            {
                                Guid personAliasGuid = attributePersonValue.AsGuid();
                                if ( !personAliasGuid.IsEmpty() )
                                {
                                    var person = new PersonAliasService( rockContext ).Queryable()
                                        .Where( a => a.Guid.Equals( personAliasGuid ) )
                                        .Select( a => a.Person )
                                        .FirstOrDefault();
                                    if ( person != null )
                                    {
                                        var mobileNumberType = Rock.Web.Cache.DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) );
                                        person.UpdatePhoneNumber( mobileNumberType.Id, PhoneNumber.DefaultCountryCode(), phoneNumber, true, null, rockContext );

                                        action.AddLogEntry( string.Format( "Enabled SMS on phone number '{0}' for person '{1}'.", updateValue, person.FullName ) );

                                        rockContext.SaveChanges();

                                        return true;
                                    }
                                    else
                                    {
                                        errorMessages.Add( string.Format( "Person could not be found for selected value ('{0}')!", guid.ToString() ) );
                                    }
                                }
                            }
                            else
                            {
                                errorMessages.Add( "The attribute used to provide the person was not of type 'Person'." );
                            }
                        }
                    }
                }
            }
            else
            {
                errorMessages.Add( "The phone number provid was not in the correct format." );
            }
             

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return true;
        }

    }
}