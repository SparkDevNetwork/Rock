// <copyright>
// Copyright by Central Christian Church
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
using System.Linq;
using System.Web;
using System.IO;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Data;
using Rock.Web.Cache;
using Rock.Web;
using Rock.Web.UI;
using Rock.Communication;

namespace com.centralaz.Calendar.Jobs
{
    [LinkedPage( "Registration Details Page", "The page that the link directs the user to.", true )]
    [SystemEmailField( "Request Registration Details Email", "The system email to send.", true )]
    [DisallowConcurrentExecution]
    public class RequestRegistrationDetails : IJob
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestRegistrationDetails"/> class.
        /// </summary>
        public RequestRegistrationDetails()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            var rockContext = new RockContext();
            var registrationService = new RegistrationService( rockContext );

            var qry = registrationService.Queryable();

            SystemEmailService emailService = new SystemEmailService( rockContext );

            SystemEmail systemEmail = null;
            Guid? systemEmailGuid = dataMap.GetString( "RequestRegistrationDetailsEmail" ).AsGuidOrNull();
            var recipients = new List<RecipientData>();

            if ( systemEmailGuid != null )
            {
                systemEmail = emailService.Get( systemEmailGuid.Value );
            }

            if ( systemEmail != null )
            {
                Guid? updatePageGuid = dataMap.GetString( "RegistrationDetailsPage" ).AsGuidOrNull();
                if ( updatePageGuid != null )
                {
                    int? pageId = ( new PageService( new RockContext() ).Get( updatePageGuid.Value ) ).Id;
                    if ( pageId != null )
                    {
                        foreach ( var registration in qry.ToList() )
                        {
                            foreach ( var registrant in registration.Registrants )
                            {
                                bool additionalDetails = false;

                                var person = registrant.Person;

                                // Set any of the template's person fields
                                foreach ( var field in registration.RegistrationInstance.RegistrationTemplate.Forms
                                    .SelectMany( f => f.Fields
                                        .Where( t => t.FieldSource == RegistrationFieldSource.PersonField && t.IsRequired ) ) )
                                {

                                    switch ( field.PersonFieldType )
                                    {
                                        case RegistrationPersonFieldType.Campus:
                                            {
                                                if ( person.GetCampus() == null )
                                                {
                                                    additionalDetails = true;
                                                }
                                                break;
                                            }

                                        case RegistrationPersonFieldType.Address:
                                            {
                                                if ( person.GetHomeLocation() == null )
                                                {
                                                    additionalDetails = true;
                                                }
                                                break;
                                            }

                                        case RegistrationPersonFieldType.Birthdate:
                                            {
                                                if ( !person.BirthDate.HasValue )
                                                {
                                                    additionalDetails = true;
                                                }
                                                break;
                                            }

                                        case RegistrationPersonFieldType.Gender:
                                            {
                                                if ( person.Gender == Gender.Unknown )
                                                {
                                                    additionalDetails = true;
                                                }
                                                break;
                                            }

                                        case RegistrationPersonFieldType.MaritalStatus:
                                            {
                                                if ( person.MaritalStatusValue == null )
                                                {
                                                    additionalDetails = true;
                                                }
                                                break;
                                            }

                                        case RegistrationPersonFieldType.MobilePhone:
                                            {
                                                if ( person.PhoneNumbers.Where( p => p.NumberTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ).FirstOrDefault() == null )
                                                {
                                                    additionalDetails = true;
                                                }
                                                break;
                                            }

                                        case RegistrationPersonFieldType.HomePhone:
                                            {
                                                if ( person.PhoneNumbers.Where( p => p.NumberTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() ).FirstOrDefault() == null )
                                                {
                                                    additionalDetails = true;
                                                } break;
                                            }

                                        case RegistrationPersonFieldType.WorkPhone:
                                            {
                                                if ( person.PhoneNumbers.Where( p => p.NumberTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() ).FirstOrDefault() == null )
                                                {
                                                    additionalDetails = true;
                                                } break;
                                            }
                                    }
                                }

                                person.LoadAttributes();

                                // Set any of the template's person fields
                                foreach ( var field in registration.RegistrationInstance.RegistrationTemplate.Forms
                                    .SelectMany( f => f.Fields
                                        .Where( t =>
                                            t.FieldSource == RegistrationFieldSource.PersonAttribute &&
                                            t.AttributeId.HasValue &&
                                            t.IsRequired ) ) )
                                {

                                    var attribute = AttributeCache.Read( field.AttributeId.Value );
                                    if ( attribute != null )
                                    {
                                        string originalValue = person.GetAttributeValue( attribute.Key );
                                        if ( String.IsNullOrWhiteSpace( originalValue ) )
                                        {
                                            additionalDetails = true;
                                        }
                                    }
                                }

                                if ( registration.Group != null )
                                {
                                    var groupMember = registration.Group.Members.Where( m => m.PersonId == person.Id ).FirstOrDefault();
                                    if ( groupMember != null )
                                    {
                                        groupMember.LoadAttributes();

                                        foreach ( var field in registration.RegistrationInstance.RegistrationTemplate.Forms
                                            .SelectMany( f => f.Fields
                                                .Where( t =>
                                                    t.FieldSource == RegistrationFieldSource.GroupMemberAttribute &&
                                                    t.AttributeId.HasValue ) ) )
                                        {
                                            var attribute = AttributeCache.Read( field.AttributeId.Value );
                                            if ( attribute != null )
                                            {
                                                string originalValue = groupMember.GetAttributeValue( attribute.Key );
                                                if ( String.IsNullOrWhiteSpace( originalValue ) )
                                                {
                                                    additionalDetails = true;
                                                }
                                            }
                                        }
                                    }

                                }

                                // Set any of the template's registrant attributes
                                registrant.LoadAttributes();
                                foreach ( var field in registration.RegistrationInstance.RegistrationTemplate.Forms
                                    .SelectMany( f => f.Fields
                                        .Where( t =>
                                            t.FieldSource == RegistrationFieldSource.RegistrationAttribute &&
                                            t.AttributeId.HasValue ) ) )
                                {
                                    var attribute = AttributeCache.Read( field.AttributeId.Value );
                                    if ( attribute != null )
                                    {
                                        string originalValue = registrant.GetAttributeValue( attribute.Key );
                                        if ( String.IsNullOrWhiteSpace( originalValue ) )
                                        {
                                            additionalDetails = true;
                                        }
                                    }
                                }

                                if ( additionalDetails )
                                {
                                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                                    mergeFields.Add( "Registrant", registrant );
                                    mergeFields.Add( "Registration", registration );

                                    Byte[] b = System.Text.Encoding.UTF8.GetBytes( registrant.Email );
                                    string encodedEmail = Convert.ToBase64String( b );

                                    String relativeUrl = String.Format( "page/{0}?RegistrationId={1}&Guid={2}&Key={3}", pageId, registration.Id, registration.Guid, encodedEmail );

                                    mergeFields.Add( "MagicUrl", relativeUrl );

                                    recipients.Add( new RecipientData( registrant.Email, mergeFields ) );
                                }
                            }
                        }

                        var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "ExternalApplicationRoot" );
                        Email.Send( systemEmail.Guid, recipients, appRoot );
                    }
                }
            }
        }
    }
}