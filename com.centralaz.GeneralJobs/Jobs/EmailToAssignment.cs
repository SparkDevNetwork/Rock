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
    /// Job to send reminders to accountability group members to submit a report.
    /// </summary>
    [WorkflowTypeField( "Workflow", "Type to use when creating workflows", false, true, "51FE9641-FB8F-41BF-B09E-235900C3E53E", "", 0 )]
    [TextField( "Mailserver", "Hostname of the mail server", true, "", "", 1 )]
    [TextField( "Mail Username", "POP3 account to login to", true, "", "", 2 )]
    [TextField( "Mail Password", "Password of the POP3 account", true, "", "", 3 )]
    [IntegerField( "Mail Port", "", true, 110, "", 4 )]
    [IntegerField( "Message Batch Size", "Max number of emails to process with each running of the job (Recommended 30)", true, 30, "", 5 )]
    [BooleanField( "Use SSL", "Does your email system use SSL?", false, "", 6 )]
    [BooleanField( "Testing Email", "Prevents job from deleting email", false, "", 7 )]
    [BooleanField( "Enable Logging", "Enable logging", false, "", 8 )]
    [PersonField( "Anonymous Sender", "Default Requester when the job cannot find one", true, "", "", 9 )]
    [DisallowConcurrentExecution]
    public class EmailToAssignment : IJob
    {
        private bool _loggingActive = false;
        private int _inboxCount = 0;
        private int _messageBatchSize = 0;
        private Guid _workflowType = Guid.Empty;
        WorkflowAction _action;
        WorkflowActionType _actionType;
        int? _actionTypeId;
        WorkflowActivity _activity;
        /// <summary>
        /// Initializes a new instance of the <see cref="SendCommunications"/> class.
        /// </summary>
        public EmailToAssignment()
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
            String now = DateTime.Now.ToString( "yyyy-MM-dd-HH-mm-ss" );
            String pathName = String.Format( "{0}{1}-EmailToAssignment.log", root, now );

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
                                        body = msg.FindFirstHtmlVersion().GetBodyAsText().SanitizeHtml( strict: false );
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

        protected Boolean MakeWorkflow( System.Net.Mail.MailAddress fromAddress, String emailSubject, String emailBody, String pathName, String anonymousSenderGuid )
        {
            //Make Workflow
            try
            {
                var rockContext = new RockContext();
                var workflowType = new WorkflowTypeService( rockContext ).Get( _workflowType );
                int? workflowId;

                if ( workflowType != null )
                {
                    var workflow = Workflow.Activate( workflowType, "Workflow" );
                    workflow.Name = emailSubject;
                    workflow.Status = "Active";

                    var workflowService = new Rock.Model.WorkflowService( rockContext );
                    List<string> errorMessages;
                    if ( workflow.Process( rockContext, out errorMessages ) )
                    {
                        // If the workflow type is persisted, save the workflow
                        if ( workflow.IsPersisted || workflowType.IsPersisted )
                        {
                            workflowService.Add( workflow );

                            rockContext.SaveChanges();
                            workflow.SaveAttributeValues( rockContext );
                            foreach ( var activity in workflow.Activities )
                            {
                                activity.SaveAttributeValues( rockContext );
                            }

                            workflowId = workflow.Id;
                        }
                    }

                    //HydrateObjects code
                    if ( workflow.IsActive )
                    {
                        // Find first active action form
                        foreach ( var activity in workflow.Activities
                            .Where( a =>
                                a.IsActive
                            )
                            .OrderBy( a => a.ActivityType.Order ) )
                        {

                            foreach ( var action in activity.ActiveActions )
                            {
                                if ( action.ActionType.WorkflowForm != null && action.IsCriteriaValid )
                                {
                                    _activity = activity;
                                    _activity.LoadAttributes();

                                    _action = action;
                                    _actionType = _action.ActionType;
                                    _actionTypeId = _actionType.Id;
                                }
                            }

                        }
                    }

                    //CompleteFormAction onLoad code
                    Guid activityTypeGuid = Guid.Empty;
                    foreach ( var action in _actionType.WorkflowForm.Actions.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ) )
                    {
                        var actionDetails = action.Split( new char[] { '^' } );
                        if ( actionDetails.Length > 0 && actionDetails[0] == "Submit" )
                        {
                            if ( actionDetails.Length > 2 )
                            {
                                activityTypeGuid = actionDetails[2].AsGuid();
                            }
                            break;
                        }
                    }
                    _action.MarkComplete();
                    _action.AddLogEntry( "Form Action Selected: " + _action.FormAction );

                    if ( _action.ActionType.IsActivityCompletedOnSuccess )
                    {
                        _action.Activity.MarkComplete();
                    }
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
                    workflow.LoadAttributes();
                    //Set each attribute from the email
                    workflow.SetAttributeValue( "Summary", emailSubject );
                    workflow.SetAttributeValue( "Details", emailBody );
                    if ( entity != null )
                    {
                        workflow.SetAttributeValue( "Requester", entity.Guid.ToString() );
                    }
                    else
                    {
                        workflow.SetAttributeValue( "Requester", anonymousSenderGuid );
                        workflow.SetAttributeValue( "Notes", String.Format( "From: {0}, {1}", fromAddress.DisplayName, fromAddress.Address ) );
                    }

                    if ( !activityTypeGuid.IsEmpty() )
                    {
                        var activityType = workflowType.ActivityTypes.Where( a => a.Guid.Equals( activityTypeGuid ) ).FirstOrDefault();
                        if ( activityType != null )
                        {
                            WorkflowActivity.Activate( activityType, workflow );
                        }
                    }

                    errorMessages = new List<string>();
                    if ( workflow.Process( rockContext, out errorMessages ) )
                    {
                        if ( workflow.IsPersisted || workflowType.IsPersisted )
                        {
                            if ( workflow.Id == 0 )
                            {
                                workflowService.Add( workflow );
                            }

                            rockContext.WrapTransaction( () =>
                            {
                                rockContext.SaveChanges();
                                workflow.SaveAttributeValues( rockContext );
                                foreach ( var activity in workflow.Activities )
                                {
                                    activity.SaveAttributeValues( rockContext );
                                }
                            } );
                        }

                        int? previousActionId = null;
                        if ( _action != null )
                        {
                            previousActionId = _action.Id;
                        }

                        _actionTypeId = null;
                        _action = null;
                        _actionType = null;
                        _activity = null;
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

        protected void LogToFile( String message, String pathName )
        {

            using ( var str = new StreamWriter( pathName, true ) )
            {
                str.WriteLine( message );
                str.Flush();
            }
        }

        protected WorkflowAction ActivateWorkflowAction( WorkflowActionType actionType, WorkflowActivity activity )
        {
            var action = new WorkflowAction();
            action.Activity = activity;
            action.ActionType = actionType;
            action.LoadAttributes();

            action.AddLogEntry( "Activated" );

            return action;
        }
    }
}