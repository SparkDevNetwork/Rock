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
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;

using Quartz;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Sends a birthday email
    /// </summary>
    [SystemEmailField( "Birthday Email", required: true )]
    [IntegerRangeField( "Age Range",
        @"The age range to include. For example, if you specify a range of 4-18, people will get the email on their 4th birthday and up till their 18th birthday. 
         Leave blank to include all ages. Note: If a person's birth year is blank, they will get an email regardless of the age range." )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Connection Statuses", "To limit to people by connection status, specify the connection status to include", false, true )]

    [DisallowConcurrentExecution]
    public class SendBirthdayEmail : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SendBirthdayEmail()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            JobDataMap dataMap = context.JobDetail.JobDataMap;
            Guid? systemEmailGuid = dataMap.GetString( "BirthdayEmail" ).AsGuidOrNull();

            SystemEmailService emailService = new SystemEmailService( rockContext );

            SystemEmail systemEmail = null;
            if ( systemEmailGuid.HasValue )
            {
                systemEmail = emailService.Get( systemEmailGuid.Value );
            }

            if ( systemEmail == null )
            {
                // no email specified, so nothing to do
                return;
            }

            var activeStatusGuid = Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid();

            // only include alive people that have record status of Active
            var personQry = personService.Queryable( false, false ).Where( a => a.RecordStatusValue.Guid == activeStatusGuid && a.IsDeceased == false );
            var ageRange = ( dataMap.GetString( "AgeRange" ) ?? string.Empty ).Split( ',' );
            if ( ageRange.Length == 2 )
            {
                int? minimumAge = ageRange[0].AsIntegerOrNull();
                int? maximumAge = ageRange[1].AsIntegerOrNull();
                personQry = personQry.WhereAgeRange( minimumAge, maximumAge, true );
            }

            // only include people whose birthday is today (which can be determined from the computed DaysUntilBirthday column)
            personQry = personQry.Where( a => a.DaysUntilBirthday.HasValue && a.DaysUntilBirthday == 0 );

            var connectionStatusGuids = ( dataMap.GetString( "ConnectionStatuses" ) ?? string.Empty ).Split( ',' ).AsGuidList();

            if ( connectionStatusGuids.Any() )
            {
                personQry = personQry.Where( a => connectionStatusGuids.Contains( a.ConnectionStatusValue.Guid ) );
            }

            // only include people that have an email address and want an email
            personQry = personQry.Where( a => ( a.Email != null ) && ( a.Email != string.Empty ) && ( a.EmailPreference != EmailPreference.DoNotEmail ) && (a.IsEmailActive) );

            var recipients = new List<RecipientData>();

            var personList = personQry.AsNoTracking().ToList();
            foreach ( var person in personList )
            {
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                mergeFields.Add( "Person", person );

                recipients.Add( new RecipientData( person.Email, mergeFields ) );
            }

            var emailMessage = new RockEmailMessage( systemEmail.Guid );
            emailMessage.SetRecipients( recipients );
            var errors = new List<string>();

            emailMessage.Send(out errors);
            context.Result = string.Format( "{0} birthday emails sent", recipients.Count() );

            if (errors.Any())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.Append( string.Format( "{0} Errors: ", errors.Count() ) );
                errors.ForEach( e => { sb.AppendLine(); sb.Append( e ); } );
                string errorMessage = sb.ToString();
                context.Result += errorMessage;
                var exception = new Exception( errorMessage);
                HttpContext context2 = HttpContext.Current;
                ExceptionLogService.LogException( exception, context2 );
                throw exception;
            }
        }
    }
}
