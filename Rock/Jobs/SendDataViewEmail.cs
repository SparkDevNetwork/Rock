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
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;

namespace Rock.Jobs
{
    /// <summary>
    /// This job will send a Lava email template to a list of people returned from the dataview.
    /// </summary>
    [DisplayName( "Send Data View Email" )]
    [Description( "This job will send a Lava email template to a list of people returned from the dataview." )]

    [SystemCommunicationField( "System Email", "The email template that will be sent.", true, "" )]
    [DataViewField( "DataView", "The dataview the email will be sent to.", true, "", "Rock.Model.Person" )]
    [IntegerField( "Database Timeout", "The number of seconds to wait before reporting a database timeout.", false, 180 )]
    public class SendDataViewEmail : RockJob
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendGroupEmail"/> class.
        /// </summary>
        public SendDataViewEmail()
        {
        }

        /// <inheritdoc cref="RockJob.Execute()" />
        public override void Execute()
        {
            var emailTemplateGuid = GetAttributeValue( "SystemEmail" ).AsGuidOrNull();
            var dataViewGuid = GetAttributeValue( "DataView" ).AsGuidOrNull();

            if ( dataViewGuid == null || emailTemplateGuid == null )
            {
                return;
            }

            var rockContext = new RockContext();
            var dataView = new DataViewService( rockContext ).Get( ( Guid ) dataViewGuid );

            List<IEntity> resultSet;
            Exception dataViewException = null;
            try
            {
                var dataViewGetQueryArgs = new DataViewGetQueryArgs
                {
                    DatabaseTimeoutSeconds = GetAttributeValue( "DatabaseTimeout" ).AsIntegerOrNull() ?? 180
                };

                var qry = dataView.GetQuery( dataViewGetQueryArgs );
                resultSet = qry.AsNoTracking().ToList();
            }
            catch ( Exception exception )
            {
                dataViewException = exception;
                var sqlTimeoutException = ReportingHelper.FindSqlTimeoutException( exception );

                if ( sqlTimeoutException != null )
                {
                    var exceptionMessage = $"The dataview did not complete in a timely manner. You can try again or adjust the timeout setting of this job.";
                    dataViewException = new RockDataViewFilterExpressionException( ( IDataViewFilterDefinition ) dataView.DataViewFilter, exceptionMessage, sqlTimeoutException );
                }

                HttpContext context2 = HttpContext.Current;
                ExceptionLogService.LogException( dataViewException, context2 );
                this.Result = dataViewException.Message;
                throw dataViewException;
            }

            var recipients = new List<RockEmailMessageRecipient>();
            if ( resultSet.Any() )
            {
                foreach ( Person person in resultSet )
                {
                    if ( !person.IsEmailActive || person.Email.IsNullOrWhiteSpace() || person.EmailPreference == EmailPreference.DoNotEmail )
                    {
                        continue;
                    }

                    var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null );
                    mergeFields.Add( "Person", person );
                    recipients.Add( new RockEmailMessageRecipient( person, mergeFields ) );
                }
            }

            var emailMessage = new RockEmailMessage( emailTemplateGuid.Value );
            emailMessage.SetRecipients( recipients );

            var emailSendErrors = new List<string>();
            emailMessage.Send( out emailSendErrors );

            this.Result = string.Format( "{0} emails sent", recipients.Count() );

            if ( emailSendErrors.Any() )
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.Append( string.Format( "{0} Errors: ", emailSendErrors.Count() ) );
                emailSendErrors.ForEach( e => { sb.AppendLine(); sb.Append( e ); } );
                string errorMessage = sb.ToString();
                this.Result += errorMessage;
                var exception = new Exception( errorMessage );
                HttpContext context2 = HttpContext.Current;
                ExceptionLogService.LogException( exception, context2 );
                throw exception;
            }
        }
    }
}