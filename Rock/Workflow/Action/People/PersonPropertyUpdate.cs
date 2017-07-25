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
    [Description( "Updates the property of a person." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Person Property Update" )]

    [WorkflowAttribute("Person", "Workflow attribute that contains the person to update.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [CustomDropdownListField("Property", "The property to update.", "Title^Title (Defined Value),FirstName^First Name (Text),NickName^Nick Name (Text),MiddleName^Middle Name (Text),LastName^Last Name (Text),Suffix^Suffix (Defined Value),Birthdate^Birthdate (Date/Text),Photo^Photo (Binary File/Integer of Binary File),Gender^Gender (Text or Integer [1=Male 2=Female 0=Unknown]),MaritalStatus^Marital Status (Defined Value),AnniversaryDate^Anniversary Date (Date),Email^Email (Text),EmailPreference^Email Preference (Text or Integer [0=EmailAllowed 1=NoMassEmails 2=DoNotEmail]),IsEmailActive^Is Email Active (Boolean),EmailNote^Email Note (Text),GraduationYear^Graduation Year (Integer),RecordStatus^Record Status (Defined Value),RecordStatusReason^Inactive Record Status Reason (Defined Value),InactiveReasonNote^Inactive Reason Note (Text),ConnectionStatus^Connection Status (Defined Value),IsDeceased^IsDeceased (Boolean),SystemNote^System Note (Text),", true, order: 1 )]
    [WorkflowTextOrAttribute( "Value", "Attribute Value", "The value or attribute value to set the person property to. <span class='tip tip-lava'></span>", false, "", "", 2, "Value" )]
    [BooleanField("Ignore Blank Values", "If a value is blank should it be ignored, or should it be used to wipe out the current value. This is helpful with working with defined values that can not be found.", true, order: 3)]
    public class PersonPropertyUpdate : ActionComponent
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
            Person person = null;
            string personAttributeValue = GetAttributeValue( action, "Person" );
            Guid guidPersonAttribute = personAttributeValue.AsGuid();
            if ( !guidPersonAttribute.IsEmpty() )
            {
                var attributePerson = AttributeCache.Read( guidPersonAttribute, rockContext );
                if ( attributePerson != null )
                {
                    string attributePersonValue = action.GetWorklowAttributeValue( guidPersonAttribute );
                    if ( !string.IsNullOrWhiteSpace( attributePersonValue ) )
                    {
                        if ( attributePerson.FieldType.Class == "Rock.Field.Types.PersonFieldType" )
                        {
                            Guid personAliasGuid = attributePersonValue.AsGuid();
                            if ( !personAliasGuid.IsEmpty() )
                            {
                                person = new PersonAliasService( rockContext ).Queryable()
                                    .Where( a => a.Guid.Equals( personAliasGuid ) )
                                    .Select( a => a.Person )
                                    .FirstOrDefault();
                                if ( person == null )
                                {
                                    errorMessages.Add( string.Format( "Person could not be found for selected value ('{0}')!", guidPersonAttribute.ToString() ) );
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            errorMessages.Add( "The attribute used to provide the person was not of type 'Person'." );
                            return false;
                        }
                    }
                }
            }

            if ( person == null )
            {
                errorMessages.Add( "The attribute used to provide the person was invalid, or not of type 'Person'." );
                return false;
            }

            // get value
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

            // determine the property to edit
            var propertySelected = GetAttributeValue( action, "Property" );

            // get the ignore blank setting
            var ignoreBlanks = GetActionAttributeValue( action, "IgnoreBlankValues" ).AsBoolean();

            // update the person
            switch ( propertySelected )
            {
                case "Title":
                    {
                        
                        var definedValueId = GetDefinedValueId( updateValue, SystemGuid.DefinedType.PERSON_TITLE.AsGuid(), rockContext );

                        if ( ignoreBlanks == false || definedValueId.HasValue )
                        {
                            person.TitleValueId = definedValueId;
                            rockContext.SaveChanges();
                        }

                        break;
                    }
                case "FirstName":
                    {
                        if ( ignoreBlanks == false || !string.IsNullOrWhiteSpace( updateValue ) )
                        {
                            person.FirstName = updateValue;
                            rockContext.SaveChanges();
                        }
                        break;
                    }
                case "NickName":
                    {
                        if ( ignoreBlanks == false || !string.IsNullOrWhiteSpace( updateValue ) )
                        {
                            person.NickName = updateValue;
                            rockContext.SaveChanges();
                        }
                        break;
                    }
                case "MiddleName":
                    {
                        if ( ignoreBlanks == false || !string.IsNullOrWhiteSpace( updateValue ) )
                        {
                            person.MiddleName = updateValue;
                            rockContext.SaveChanges();
                        }
                        break;
                    }
                case "LastName":
                    {
                        if ( ignoreBlanks == false || !string.IsNullOrWhiteSpace( updateValue ) )
                        {
                            person.LastName = updateValue;
                            rockContext.SaveChanges();
                        }
                        break;
                    }
                case "Suffix":
                    {
                        var definedValueId = GetDefinedValueId( updateValue, SystemGuid.DefinedType.PERSON_SUFFIX.AsGuid(), rockContext );

                        if ( ignoreBlanks == false || definedValueId.HasValue )
                        {
                            person.SuffixValueId = definedValueId;
                            rockContext.SaveChanges();
                        }
                        break;
                    }
                case "Birthdate":
                    {
                        var birthdate = updateValue.AsDateTime();

                        if ( ignoreBlanks == false || birthdate.HasValue )
                        {
                            if ( birthdate.HasValue )
                            {
                                person.BirthDay = birthdate.Value.Day;
                                person.BirthMonth = birthdate.Value.Month;
                                person.BirthYear = birthdate.Value.Year;
                            }
                            else
                            {
                                person.BirthDay = null;
                                person.BirthMonth = null;
                                person.BirthYear = null;
                            }
                            rockContext.SaveChanges();
                        }

                        break;
                    }
                case "Photo":
                    {
                        // could be interger of binary file or a guid

                        var binaryFileId = updateValue.AsIntegerOrNull();
                        if ( binaryFileId.HasValue )
                        {
                            person.PhotoId = binaryFileId;
                            rockContext.SaveChanges();
                        }
                        else
                        {
                            var binaryFileGuid = updateValue.AsGuidOrNull();
                            if ( binaryFileGuid.HasValue )
                            {
                                binaryFileId = new BinaryFileService( rockContext ).Queryable().Where( f => f.Guid == binaryFileGuid.Value ).Select( f => f.Id ).FirstOrDefault();

                                if ( ignoreBlanks == false || binaryFileId.HasValue )
                                {
                                    person.PhotoId = binaryFileId;
                                    rockContext.SaveChanges();
                                }
                            }
                        }

                        break;
                    }
                case "Gender":
                    {
                        var gender = updateValue.ConvertToEnumOrNull<Gender>();
                        if ( ignoreBlanks == false || gender.HasValue )
                        {
                            if ( !gender.HasValue )
                            {
                                gender = Gender.Unknown;
                            }
                            person.Gender = gender.Value;
                            rockContext.SaveChanges();
                        }
                        break;
                    }
                case "MaritalStatus":
                    {
                        var definedValueId = GetDefinedValueId( updateValue, SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid(), rockContext );

                        if ( ignoreBlanks == false || definedValueId.HasValue )
                        {
                            person.MaritalStatusValueId = definedValueId;
                            rockContext.SaveChanges();
                        }

                        break;
                    }
                case "AnniversaryDate":
                    {
                        var anniversarydate = updateValue.AsDateTime();

                        if ( ignoreBlanks == false || anniversarydate.HasValue )
                        {
                            person.AnniversaryDate = anniversarydate;
                            rockContext.SaveChanges();
                        }
                        break;
                    }
                case "Email":
                    {
                        if ( ignoreBlanks == false || !string.IsNullOrWhiteSpace( updateValue ) )
                        {
                            person.Email = updateValue;
                            rockContext.SaveChanges();
                        }
                        break;
                    }
                case "IsEmailActive":
                    {
                        var updateAsBoolean = updateValue.AsBooleanOrNull();

                        if ( ignoreBlanks == false || updateAsBoolean.HasValue )
                        {
                            if ( !updateAsBoolean.HasValue )
                            {
                                updateAsBoolean = true; // default to true
                            }
                            person.IsEmailActive = updateAsBoolean.Value;
                            rockContext.SaveChanges();
                        }
                        break;
                    }
                case "EmailNote":
                    {
                        if ( ignoreBlanks == false || !string.IsNullOrWhiteSpace( updateValue ) )
                        {
                            person.EmailNote = updateValue;
                            rockContext.SaveChanges();
                        }
                        break;
                    }
                case "EmailPreference":
                    {
                        var emailPreference = updateValue.ConvertToEnumOrNull<EmailPreference>();
                        if ( ignoreBlanks == false || emailPreference.HasValue )
                        {
                            if ( !emailPreference.HasValue )
                            {
                                emailPreference = EmailPreference.EmailAllowed;
                            }
                            person.EmailPreference = emailPreference.Value;
                            rockContext.SaveChanges();
                        }
                        break;
                    }
                case "GraduationYear":
                    {
                        var updateAsInt = updateValue.AsIntegerOrNull();

                        if ( ignoreBlanks == false || updateAsInt.HasValue )
                        {
                            person.GraduationYear = updateAsInt;
                            rockContext.SaveChanges();
                        }

                        break;
                    }
                case "RecordStatus":
                    {
                        var definedValueId = GetDefinedValueId( updateValue, SystemGuid.DefinedType.PERSON_RECORD_STATUS.AsGuid(), rockContext );

                        if ( ignoreBlanks == false || definedValueId.HasValue )
                        {
                            person.RecordStatusValueId = definedValueId;
                            rockContext.SaveChanges();
                        }
                        break;
                    }
                case "RecordStatusReason":
                    {
                        var definedValueId = GetDefinedValueId( updateValue, SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON.AsGuid(), rockContext );

                        if ( ignoreBlanks == false || definedValueId.HasValue )
                        {
                            person.RecordStatusReasonValueId = definedValueId;
                            rockContext.SaveChanges();
                        }
                        break;
                    }
                case "InactiveReasonNote":
                    {
                        if ( ignoreBlanks == false || !string.IsNullOrWhiteSpace( updateValue ) )
                        {
                            person.InactiveReasonNote = updateValue;
                            rockContext.SaveChanges();
                        }
                        break;
                    }
                case "ConnectionStatus":
                    {
                        var definedValueId = GetDefinedValueId( updateValue, SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid(), rockContext );

                        if ( ignoreBlanks == false || definedValueId.HasValue )
                        {
                            person.ConnectionStatusValueId = definedValueId;
                            rockContext.SaveChanges();
                        }
                        break;
                    }
                case "IsDeceased":
                    {
                        var updateAsBoolean = updateValue.AsBooleanOrNull();

                        if ( ignoreBlanks == false || updateAsBoolean.HasValue )
                        {
                            if ( !updateAsBoolean.HasValue )
                            {
                                updateAsBoolean = false; // default to false
                            }
                            person.IsDeceased = updateAsBoolean.Value;
                            rockContext.SaveChanges();
                        }
                        break;
                    }
                case "SystemNote":
                    {
                        if ( ignoreBlanks == false || !string.IsNullOrWhiteSpace( updateValue ) )
                        {
                            person.SystemNote = updateValue;
                            rockContext.SaveChanges();
                        }
                        break;
                    }
            }


            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return true;
        }

        /// <summary>
        /// Gets the defined value identifier.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="definedTypeGuid">The defined type unique identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public int? GetDefinedValueId( string value, Guid definedTypeGuid, RockContext rockContext )
        {
            // returns the guid of the matching defined value for the passed defined type
            // the value is first checked to see if it contains a guid for the defined value
            // if that is not found then we check to see if there is a value that matches it

            int? definedValueId = null;

            var definedValues = DefinedTypeCache.Read( definedTypeGuid ).DefinedValues;
            DefinedValueCache definedValue = null;

            value = value.Trim();

            var valueAsGuid = value.AsGuidOrNull();
            if ( valueAsGuid.HasValue )
            {
                // get defined value id
                definedValue = definedValues.Where(v => v.Guid == valueAsGuid.Value).FirstOrDefault();   
            }
            else
            {
                // try finding a defined value of that type with the same value
                definedValue = definedValues.Where( v => v.Value == value ).FirstOrDefault();
            }

            if (definedValue != null )
            {
                definedValueId = definedValue.Id;
            }

            return definedValueId;
        }

        /*
    Nimble Text Items

        Create Dropdown Items: <% $0.replace(/ /g,'') %>^$0 ($1),
        Create Switch Statement: 


Title,Defined Value
First Name,Text
Nick Name,Text
Middle Name,Text
Last Name,Text
Suffix,Defined Value
Birthdate,Date/Text
Photo,Binary File
Gender,Text or Integer (1=Male 2=Female 0=Unknown)
Marital Status, Defined Value
Anniversary Date, Date
Email, Text
IsEmailActive, Boolean
EmailNote, Text
Email Preference, Text or Integer (0=EmailAllowed 1=NoMassEmails 2=DoNotEmail)
Graduation Year, Integer
Record Status, Defined Value
Record Status Reason, Defined Value
Inactive Reason Note, Text
Connection Status, Defined Value
IsDeceased, Boolean
System Note, Text


    */
    }
}