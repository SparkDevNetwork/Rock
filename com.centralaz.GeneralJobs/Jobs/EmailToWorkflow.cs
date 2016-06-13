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
using OpenPop.Pop3;
using OpenPop.Common.Logging;
using OpenPop.Mime;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Data;
using Rock.Web.Cache;
using Rock.Web;
using Rock.Web.UI;
using Rock.Communication;

namespace com.centralaz.GeneralJobs.Jobs
{
    /// <summary>
    /// Creates Workflows from emails in a POP3 inbox.  See https://github.com/CentralAZ/Rock-CentralAZ/wiki/Jobs for details.
    /// </summary>
    [WorkflowTypeField( "Workflow", "Type to use when creating workflows.  The Workflow should have the following attributes: Summary (type: Text), Details (type: Text), Requester (type Person), Notes (type: Text).", false, true, "51FE9641-FB8F-41BF-B09E-235900C3E53E", "", 0 )]
    [TextField( "Mailserver", "Hostname of the mail server", true, "", "", 1 )]
    [TextField( "Mail Username", "POP3 account to login to", true, "", "", 2 )]
    [TextField( "Mail Password", "Password of the POP3 account", true, "", "", 3, isPassword:true )]
    [IntegerField( "Mail Port", "", true, 110, "", 4 )]
    [IntegerField( "Message Batch Size", "Max number of emails to process with each running of the job (Recommended 30)", true, 30, "", 5 )]
    [BooleanField( "Use SSL", "Does your email system use SSL?", false, "", 6 )]
    [BooleanField( "Testing Email", "Prevents job from deleting email", false, "", 7 )]
    [BooleanField( "Enable Logging", "Enable logging", false, "", 8 )]
    [PersonField( "Anonymous Sender", "Default Requester when the job cannot find one", true, "", "", 9 )]
    [DisallowConcurrentExecution]
    public class EmailToWorkflow : IJob
    {
        private bool _loggingActive = false;
        private int _inboxCount = 0;
        private int _messageBatchSize = 0;
        private Guid _workflowType = Guid.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendCommunications"/> class.
        /// </summary>
        public EmailToWorkflow()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            String root = System.Web.Hosting.HostingEnvironment.MapPath( "~/App_Data/Logs/" );
            String pathName = Path.Combine( root, "EmailToWorkflow.log" );

            //Get Mail
            try
            {
                if ( dataMap.Get( "Mailserver" ).ToString() != String.Empty )
                {
                    _messageBatchSize = dataMap.Get( "MessageBatchSize" ).ToString().AsInteger();

                    using ( Pop3Client pop3Client = new Pop3Client() )
                    {
                        if ( dataMap.Get( "UseSSL" ).ToString().AsBoolean() )
                        {
                            pop3Client.Connect( dataMap.Get( "Mailserver" ).ToString(), dataMap.Get( "MailPort" ).ToString().AsInteger(), true );
                            pop3Client.Authenticate( dataMap.Get( "MailUsername" ).ToString(), dataMap.Get( "MailPassword" ).ToString() );
                        }
                        else
                        {
                            pop3Client.Connect( dataMap.Get( "Mailserver" ).ToString(), dataMap.Get( "MailPort" ).ToString().AsInteger(), false );
                            pop3Client.Authenticate( dataMap.Get( "MailUsername" ).ToString(), dataMap.Get( "MailPassword" ).ToString() );
                        }

                        if ( dataMap.Get( "EnableLogging" ).ToString().AsBoolean() )
                        {
                            _loggingActive = true;
                            LogToFile( String.Format( "Batch size set to {0}", _messageBatchSize ), pathName );
                        }

                        _inboxCount = pop3Client.GetMessageCount();
                        if ( _loggingActive )
                        {
                            LogToFile( String.Format( "Connected to {0} as {1}", dataMap.Get( "Mailserver" ).ToString(), dataMap.Get( "MailUsername" ).ToString() ), pathName );
                            LogToFile( String.Format( "There are {0} messages in {1}", _inboxCount, dataMap.Get( "MailUsername" ).ToString() ), pathName );
                        }

                        if ( _inboxCount < _messageBatchSize )
                        {
                            _messageBatchSize = _inboxCount;
                        }

                        if ( _loggingActive )
                        {
                            LogToFile( String.Format( "Batch size set to {0}", _messageBatchSize ), pathName );
                        }

                        if ( _messageBatchSize > 0 )
                        {
                            _workflowType = dataMap.Get( "Workflow" ).ToString().AsGuid();
                            Message msg;
                            for ( int i = 1; i <= _messageBatchSize; i++ )
                            {
                                if ( _loggingActive )
                                {
                                    LogToFile( String.Format( "Attempting to read message {0}", i ), pathName );
                                }
                                try
                                {
                                    msg = pop3Client.GetMessage( i );
                                    LogToFile( String.Format( "read message {0}  Subject:\"{1}\"", i, msg.Headers.Subject ), pathName );
                                    String body;

                                    if ( msg.MessagePart.IsText == false )
                                    {
                                        var bodyText = msg.FindFirstHtmlVersion().GetBodyAsText();
                                        if ( _loggingActive )
                                        {
                                            LogToFile( String.Format( "body: {0}", i ), bodyText );
                                        }
                                        body = bodyText.SanitizeHtml( strict: false );
                                    }
                                    else
                                    {
                                        body = msg.MessagePart.GetBodyAsText();
                                    }

                                    if ( MakeWorkflow( msg.Headers.From.MailAddress, msg.Headers.Subject, body, pathName, dataMap.Get( "AnonymousSender" ).ToString() ) )
                                    {
                                        //Delete Message
                                        if ( !dataMap.Get( "TestingEmail" ).ToString().AsBoolean() )
                                        {
                                            try
                                            {
                                                pop3Client.DeleteMessage( i );
                                                if ( _loggingActive )
                                                {
                                                    LogToFile( String.Format( "Deleted message {0}", i ), pathName );
                                                }
                                            }
                                            catch ( Exception ex )
                                            {
                                                if ( _loggingActive )
                                                {
                                                    LogToFile( String.Format( "Unable to delete message {0} from {1} with subject '{2}'.", i.ToString(), msg.Headers.From.MailAddress, msg.Headers.Subject ), pathName );
                                                    LogToFile( String.Format( "\n\nMessage\n------------------------\n{0}\n\nStack Trace\n------------------------\n{1}", ex.Message, ex.StackTrace ), pathName );
                                                }
                                                throw;
                                            }
                                        }
                                    }
                                }
                                catch ( Exception ex )
                                {
                                    if ( _loggingActive )
                                    {
                                        LogToFile( String.Format( "Failed to read message {0}", i ), pathName );
                                        LogToFile( String.Format( "\n\nMessage\n------------------------\n{0}\n\nStack Trace\n------------------------\n{1}", ex.Message, ex.StackTrace ), pathName );
                                    }
                                    throw;
                                }
                            }
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                if ( _loggingActive )
                {
                    LogToFile( String.Format( "An error occured while processing POP3 mail.\n\nMessage\n------------------------\n{0}\n\nStack Trace\n------------------------\n{1}", ex.Message, ex.StackTrace ), pathName );
                }
                throw;
            }
        }

        /// <summary>
        /// Makes the workflow.
        /// </summary>
        /// <param name="fromAddress">From address.</param>
        /// <param name="emailSubject">The email subject.</param>
        /// <param name="emailBody">The email body.</param>
        /// <param name="pathName">Name of the path.</param>
        /// <param name="anonymousSenderGuid">The anonymous sender unique identifier.</param>
        /// <returns></returns>
        protected Boolean MakeWorkflow( System.Net.Mail.MailAddress fromAddress, String emailSubject, String emailBody, String pathName, String anonymousSenderGuid )
        {
            //Make Workflow
            try
            {
                var rockContext = new RockContext();
                var workflowType = new WorkflowTypeService( rockContext ).Get( _workflowType );
                var workflowService = new Rock.Model.WorkflowService( rockContext );

                if ( workflowType != null )
                {
                    var workflow = Workflow.Activate( workflowType, "Email To Workflow" );
                    workflow.Name = emailSubject;
                    workflow.Status = "Active";

                    //Try to find the person from the email
                    Person entity = null;
                    entity = new PersonService( rockContext ).GetByEmail( fromAddress.Address ).FirstOrDefault();
                    if ( entity == null )
                    {
                        try
                        {
                            entity = new PersonService( rockContext ).GetByFullName( fromAddress.DisplayName, false ).FirstOrDefault();
                        }
                        catch
                        { }
                    }

                    if ( entity == null )
                    {
                        String[] nameArray = fromAddress.Address.Split( '.', '@' );
                        String fullName = String.Format( "{0} {1}", nameArray[0], nameArray[1] );
                        entity = new PersonService( rockContext ).GetByFullName( fullName, false ).FirstOrDefault();
                    }

                    //Set each attribute from the email
                    workflow.SetAttributeValue( "Summary", emailSubject.SanitizeHtml( true ) );
                    workflow.SetAttributeValue( "Details", emailBody.SanitizeHtml( true ) );

                    if ( entity != null )
                    {
                        workflow.SetAttributeValue( "Requester", entity.Guid.ToString() );
                    }
                    else
                    {
                        workflow.SetAttributeValue( "Requester", anonymousSenderGuid );
                        workflow.SetAttributeValue( "Notes", String.Format( "From: {0}, {1}", fromAddress.DisplayName, fromAddress.Address ) );
                    }

                    var errorMessages = new List<string>();
                    if ( !workflowService.Process( workflow, out errorMessages ) && _loggingActive )
                    {
                       // Log errors
                       LogToFile( string.Format( "Errors occurred trying to process the workflow: {0}", String.Join( ", ", errorMessages.ToArray() ) ), pathName );
                    }
                }

                return true;
            }
            catch ( Exception ex )
            {
                if ( _loggingActive )
                {
                    LogToFile( String.Format( "Error creating assignment for type {0}", _workflowType ), pathName );
                    LogToFile( String.Format( "\n\nMessage\n------------------------\n{0}\n\nStack Trace\n------------------------\n{1}", ex.Message, ex.StackTrace ), pathName );
                }
                throw;
            }
        }

        /// <summary>
        /// Logs to file.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="pathName">Name of the path.</param>
        protected void LogToFile( String message, String pathName )
        {
            String now = DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss" );

            using ( var str = new StreamWriter( pathName, true ) )
            {
                str.WriteLine( string.Format( "{0} {1}", now, message ) );
                str.Flush();
            }
        }
    }
}