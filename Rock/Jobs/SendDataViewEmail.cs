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
using System.Data.SqlClient;
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
    /// The job will send a Lava email template to a list of people returned from the dataview. 
    /// </summary>
    [SystemEmailField( "System Email", "The email template that will be sent.", true, "" )]
    [DataViewField( "DataView", "The dataview the email will be sent to.", true, "", "Rock.Model.Person" )]
    [IntegerField( "Database Timeout", "The number of seconds to wait before reporting a database timeout.", false, 180 )]
    [DisallowConcurrentExecution]
    public class SendDataViewEmail : IJob
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendGroupEmail"/> class.
        /// </summary>
        public SendDataViewEmail()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var emailTemplateGuid = dataMap.GetString( "SystemEmail" ).AsGuidOrNull();
            var dataViewGuid = dataMap.GetString( "DataView" ).AsGuidOrNull();

            if( dataViewGuid != null && emailTemplateGuid.HasValue )
            {
                var rockContext = new RockContext();
                var dataView = new DataViewService( rockContext ).Get( (Guid)dataViewGuid );

                List<IEntity> resultSet = null;
                var errorMessages = new List<string>();
                var dataTimeout = dataMap.GetString( "DatabaseTimeout" ).AsIntegerOrNull() ?? 180;
                try
                {
                    var qry = dataView.GetQuery( null, rockContext, dataTimeout, out errorMessages );
                    if( qry != null )
                    {
                        resultSet = qry.AsNoTracking().ToList();
                    }
                }
                catch( Exception exception )
                {
                    ExceptionLogService.LogException( exception, HttpContext.Current );
                    while( exception != null )
                    {
                        if( exception is SqlException && (exception as SqlException).Number == -2 )
                        {
                            // if there was a SQL Server Timeout, have the warning be a friendly message about that.
                            errorMessages.Add( "This dataview did not complete in a timely manner. You can try again or adjust the timeout setting of this block." );
                            exception = exception.InnerException;
                        }
                        else
                        {
                            errorMessages.Add( exception.Message );
                            exception = exception.InnerException;
                        }

                        return;
                    }
                }

                var recipients = new List<RecipientData>();
                if( resultSet.Any() )
                {
                    foreach( Person person in resultSet )
                    {
                        if ( !person.IsEmailActive || person.Email.IsNullOrWhiteSpace() || person.EmailPreference == EmailPreference.DoNotEmail )
                        {
                            continue;
                        }
                        var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null );
                        mergeFields.Add( "Person", person );
                        recipients.Add( new RecipientData( person.Email, mergeFields ) );
                    }
                }

                var emailMessage = new RockEmailMessage( emailTemplateGuid.Value );
                emailMessage.SetRecipients( recipients );

                var errors = new List<string>();
                emailMessage.Send(out errors);

                context.Result = string.Format( "{0} emails sent", recipients.Count() );

                if ( errors.Any() )
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine();
                    sb.Append( string.Format( "{0} Errors: ", errors.Count() ) );
                    errors.ForEach( e => { sb.AppendLine(); sb.Append( e ); } );
                    string errorMessage = sb.ToString();
                    context.Result += errorMessage;
                    var exception = new Exception( errorMessage );
                    HttpContext context2 = HttpContext.Current;
                    ExceptionLogService.LogException( exception, context2 );
                    throw exception;
                }
            }
        }
    }
}