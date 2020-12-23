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

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sends email
    /// </summary>
    [ActionCategory( "Communications" )]
    [Description( "Sends an email. The recipient(s) can either be a person, email address, group or security role determined by the 'To Attribute' value, or one or more email addresses entered in the 'Send To Email Addresses' field. Only people with an active email address without the 'Do Not Email' preference are included. If the 'To Attribute' value is a group, only members with an <em>Active</em> member status are included." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Email Send" )]

    #region Block Attributes

    [WorkflowTextOrAttribute( "From Email Address",
        "From Attribute",
        "The email address or an attribute that contains the person or email address that email should be sent from (will default to organization email). <span class='tip tip-lava'></span>",
        false,
        "",
        "",
        0,
        AttributeKey.From,
        new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.EmailFieldType", "Rock.Field.Types.PersonFieldType" } )]

    [WorkflowTextOrAttribute( "Send To Email Addresses",
        "To Attribute",
        "The email addresses or an attribute that contains the person, email address, group or security role that the email should be sent to. <span class='tip tip-lava'></span>",
        true,
        "",
        "",
        1,
        AttributeKey.To,
        new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.EmailFieldType", "Rock.Field.Types.PersonFieldType", "Rock.Field.Types.GroupFieldType", "Rock.Field.Types.SecurityRoleFieldType" } )]

    [WorkflowAttribute( "Send to Group Role",
        Key = AttributeKey.GroupRole,
        Description = "An optional Group Role attribute to limit recipients to if the 'Send to Email Addresses' is a group or security role.",
        IsRequired = false,
        Order = 2,
        FieldTypeClassNames =  new string[] { "Rock.Field.Types.GroupRoleFieldType" } )]

    [TextField( "Subject",
        Key = AttributeKey.Subject,
        Description = "The subject that should be used when sending email. <span class='tip tip-lava'></span>",
        IsRequired = false,
        Order = 3 )]

    [CodeEditorField( "Body",
        Key = AttributeKey.Body,
        Description = "The body of the email that should be sent. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",
        EditorMode = Web.UI.Controls.CodeEditorMode.Html,
        EditorTheme = Web.UI.Controls.CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        Order = 4 )]

    [WorkflowTextOrAttribute( "CC Email Addresses",
        "CC Attribute",
        "The email addresses or an attribute that contains the person, email address, group or security role that the email should be CC'd (carbon copied) to. Any address in this field will be copied on the email sent to every recipient. <span class='tip tip-lava'></span>",
        false,
        "",
        "",
        5,
        AttributeKey.Cc,
        new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.EmailFieldType", "Rock.Field.Types.PersonFieldType", "Rock.Field.Types.GroupFieldType", "Rock.Field.Types.SecurityRoleFieldType" } )]

    [WorkflowTextOrAttribute( "BCC Email Addresses",
        "BCC Attribute",
        "The email addresses or an attribute that contains the person, email address, group or security role that the email should be BCC'd (blind carbon copied) to. Any address in this field will be copied on the email sent to every recipient. <span class='tip tip-lava'></span>",
        false,
        "",
        "",
        6,
        AttributeKey.Bcc,
        new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.EmailFieldType", "Rock.Field.Types.PersonFieldType", "Rock.Field.Types.GroupFieldType", "Rock.Field.Types.SecurityRoleFieldType" } )]

    [WorkflowAttribute( "Attachment One",
        Key = AttributeKey.AttachmentOne,
        Description = "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.",
        IsRequired = false,
        Order = 7,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.FileFieldType", "Rock.Field.Types.ImageFieldType" } )]

    [WorkflowAttribute( "Attachment Two",
        Key = AttributeKey.AttachmentTwo,
        Description = "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.",
        IsRequired = false,
        Order = 8,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.FileFieldType", "Rock.Field.Types.ImageFieldType" } )]

    [WorkflowAttribute( "Attachment Three",
        Key = AttributeKey.AttachmentThree,
        Description = "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.",
        IsRequired = false,
        Order = 9,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.FileFieldType", "Rock.Field.Types.ImageFieldType" } )]

    [BooleanField( "Save Communication History",
        Key = AttributeKey.SaveCommunicationHistory,
        Description = "Should a record of this communication be saved to the recipient's profile?",
        DefaultBooleanValue = false,
        Order = 10 )]

    #endregion

    public class SendEmail : ActionComponent
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string From = "From";
            public const string To = "To";
            public const string GroupRole = "GroupRole";
            public const string Subject = "Subject";
            public const string Body = "Body";
            public const string Cc = "CC";
            public const string Bcc = "BCC";
            public const string AttachmentOne = "AttachmentOne";
            public const string AttachmentTwo = "AttachmentTwo";
            public const string AttachmentThree = "AttachmentThree";
            public const string SaveCommunicationHistory = "SaveCommunicationHistory";
        }

        #endregion

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

            var mergeFields = GetMergeFields( action );

            string to = GetAttributeValue( action, AttributeKey.To );
            string fromValue = GetAttributeValue( action, AttributeKey.From );
            string subject = GetAttributeValue( action, AttributeKey.Subject );
            string body = GetAttributeValue( action, AttributeKey.Body );
            string cc = GetActionAttributeValue( action, AttributeKey.Cc );
            string bcc = GetActionAttributeValue( action, AttributeKey.Bcc );
            var attachmentOneGuid = GetAttributeValue( action, AttributeKey.AttachmentOne, true ).AsGuid();
            var attachmentTwoGuid = GetAttributeValue( action, AttributeKey.AttachmentTwo, true ).AsGuid();
            var attachmentThreeGuid = GetAttributeValue( action, AttributeKey.AttachmentThree, true ).AsGuid();

            var attachmentList = new List<BinaryFile>();
            if (!attachmentOneGuid.IsEmpty())
            {
                attachmentList.Add( new BinaryFileService( rockContext ).Get( attachmentOneGuid ) );
            }

            if ( !attachmentTwoGuid.IsEmpty() )
            {
                attachmentList.Add( new BinaryFileService( rockContext ).Get( attachmentTwoGuid ) );
            }

            if ( !attachmentThreeGuid.IsEmpty() )
            {
                attachmentList.Add( new BinaryFileService( rockContext ).Get( attachmentThreeGuid ) );
            }

            var attachments = attachmentList.ToArray();

            bool createCommunicationRecord = GetAttributeValue( action, AttributeKey.SaveCommunicationHistory ).AsBoolean();

            string fromEmailAddress = string.Empty;
            string fromName = string.Empty;
            Guid? fromGuid = fromValue.AsGuidOrNull();
            if ( fromGuid.HasValue )
            {
                var attribute = AttributeCache.Get( fromGuid.Value, rockContext );
                if ( attribute != null )
                {
                    string fromAttributeValue = action.GetWorkflowAttributeValue( fromGuid.Value );
                    if ( !string.IsNullOrWhiteSpace( fromAttributeValue ) )
                    {
                        if ( attribute.FieldType.Class == "Rock.Field.Types.PersonFieldType" )
                        {
                            Guid personAliasGuid = fromAttributeValue.AsGuid();
                            if ( !personAliasGuid.IsEmpty() )
                            {
                                var person = new PersonAliasService( rockContext ).Queryable()
                                    .Where( a => a.Guid.Equals( personAliasGuid ) )
                                    .Select( a => a.Person )
                                    .FirstOrDefault();
                                if ( person != null && !string.IsNullOrWhiteSpace( person.Email ) )
                                {
                                    fromEmailAddress = person.Email;
                                    fromName = person.FullName;
                                }
                            }
                        }
                        else
                        {
                            fromEmailAddress = fromAttributeValue;
                        }
                    }
                }
            }
            else
            {
                fromEmailAddress = fromValue.ResolveMergeFields( mergeFields );
            }

            // to
            if ( GetEmailsFromAttributeValue( RecipientType.SendTo, to, action, mergeFields, rockContext, out string toDelimitedEmails, out List<RockEmailMessageRecipient> toRecipients ) )
            {
                // cc
                GetEmailsFromAttributeValue( RecipientType.CC, cc, action, mergeFields, rockContext, out string ccDelimitedEmails, out List<RockEmailMessageRecipient> ccRecipients );
                List<string> ccEmails = BuildEmailList( ccDelimitedEmails, mergeFields, ccRecipients );

                // bcc
                GetEmailsFromAttributeValue( RecipientType.BCC, bcc, action, mergeFields, rockContext, out string bccDelimitedEmails, out List<RockEmailMessageRecipient> bccRecipients );
                List<string> bccEmails = BuildEmailList( bccDelimitedEmails, mergeFields, bccRecipients );

                if ( !string.IsNullOrWhiteSpace( toDelimitedEmails ) )
                {
                    Send( toDelimitedEmails, fromEmailAddress, fromName, subject, body, ccEmails, bccEmails, mergeFields, createCommunicationRecord, attachments, out errorMessages );
                }
                else if ( toRecipients != null )
                {
                    Send( toRecipients, fromEmailAddress, fromName, subject, body, ccEmails, bccEmails, createCommunicationRecord, attachments, out errorMessages );
                }
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );
            return true;
        }

        /// <summary>
        /// The recipient type for a given email recipient.
        /// </summary>
        private enum RecipientType
        {
            SendTo = 0,
            CC = 1,
            BCC = 2
        }

        /// <summary>
        /// Gets either a delimited string of email addresses or a list of recipients from an attribute value.
        /// </summary>
        /// <param name="recipientType">The recipient type.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="action">The action.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="delimitedEmails">The delimited emails.</param>
        /// <param name="recipientList">The recipient list.</param>
        /// <returns></returns>
        private bool GetEmailsFromAttributeValue( RecipientType recipientType, string attributeValue, WorkflowAction action, Dictionary<string, object> mergeFields, RockContext rockContext, out string delimitedEmails, out List<RockEmailMessageRecipient> recipientList )
        {
            delimitedEmails = null;
            recipientList = null;

            Guid? guid = attributeValue.AsGuidOrNull();
            if ( guid.HasValue )
            {
                var attribute = AttributeCache.Get( guid.Value, rockContext );
                if ( attribute != null )
                {
                    var recipientTypePrefix = string.Empty;
                    var groupRoleAttributeKey = AttributeKey.GroupRole;
                    switch ( recipientType )
                    {
                        case RecipientType.CC:
                            recipientTypePrefix = "CC ";
                            break;
                        case RecipientType.BCC:
                            recipientTypePrefix = "BCC ";
                            break;
                    }

                    string toValue = action.GetWorkflowAttributeValue( guid.Value );
                    if ( !string.IsNullOrWhiteSpace( toValue ) )
                    {
                        switch ( attribute.FieldType.Class )
                        {
                            case "Rock.Field.Types.TextFieldType":
                            case "Rock.Field.Types.EmailFieldType":
                                {
                                    delimitedEmails = toValue;
                                    return true;
                                }

                            case "Rock.Field.Types.PersonFieldType":
                                {
                                    Guid personAliasGuid = toValue.AsGuid();
                                    if ( !personAliasGuid.IsEmpty() )
                                    {
                                        var person = new PersonAliasService( rockContext ).Queryable()
                                            .Where( a => a.Guid.Equals( personAliasGuid ) )
                                            .Select( a => a.Person )
                                            .FirstOrDefault();
                                        if ( person == null )
                                        {
                                            action.AddLogEntry( $"Invalid {recipientTypePrefix}Recipient: Person not found", true );
                                        }
                                        else if ( string.IsNullOrWhiteSpace( person.Email ) )
                                        {
                                            action.AddLogEntry( $"{recipientTypePrefix}Email was not sent: Recipient does not have an email address", true );
                                        }
                                        else if ( !person.IsEmailActive )
                                        {
                                            action.AddLogEntry( $"{recipientTypePrefix}Email was not sent: Recipient email is not active", true );
                                        }
                                        else if ( person.EmailPreference == EmailPreference.DoNotEmail )
                                        {
                                            action.AddLogEntry( $"{recipientTypePrefix}Email was not sent: Recipient has requested 'Do Not Email'", true );
                                        }
                                        else
                                        {
                                            var personDict = new Dictionary<string, object>( mergeFields )
                                            {
                                                { "Person", person }
                                            };

                                            recipientList = new List<RockEmailMessageRecipient> { new RockEmailMessageRecipient( person, personDict ) };
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                            case "Rock.Field.Types.GroupFieldType":
                            case "Rock.Field.Types.SecurityRoleFieldType":
                                {
                                    int? groupId = toValue.AsIntegerOrNull();
                                    Guid? groupGuid = toValue.AsGuidOrNull();

                                    /*
                                     * 2020-03-25 - JPH
                                     *
                                     * Per Jon, even though the user may select a Group or Security Role Attribute for the CC and BCC
                                     * Attributes, we only want to allow Group Role filtering for the 'Send To Email Addresses' Attribute.
                                     * Otherwise, the UI starts to become too overwhelming for the end user making the selections.
                                     */
                                    Guid? groupRoleValueGuid = recipientType == RecipientType.SendTo ?
                                        GetGroupRoleValueGuid( action, groupRoleAttributeKey ) :
                                        null;

                                    IQueryable<GroupMember> qry = null;

                                    // Handle situations where the attribute value is the ID
                                    if ( groupId.HasValue )
                                    {
                                        qry = new GroupMemberService( rockContext ).GetByGroupId( groupId.Value );
                                    }
                                    else if ( groupGuid.HasValue )
                                    {
                                        // Handle situations where the attribute value stored is the Guid
                                        qry = new GroupMemberService( rockContext ).GetByGroupGuid( groupGuid.Value );
                                    }
                                    else
                                    {
                                        action.AddLogEntry( $"Invalid {recipientTypePrefix}Recipient: No valid group id or Guid", true );
                                    }

                                    if ( groupRoleValueGuid.HasValue )
                                    {
                                        qry = qry.Where( m => m.GroupRole != null && m.GroupRole.Guid.Equals( groupRoleValueGuid.Value ) );
                                    }

                                    if ( qry != null )
                                    {
                                        recipientList = new List<RockEmailMessageRecipient>();
                                        foreach ( var person in qry
                                            .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active )
                                            .Select( m => m.Person ) )
                                        {
                                            if ( person.IsEmailActive &&
                                                person.EmailPreference != EmailPreference.DoNotEmail &&
                                                !string.IsNullOrWhiteSpace( person.Email ) )
                                            {
                                                var personDict = new Dictionary<string, object>( mergeFields )
                                                {
                                                    { "Person", person }
                                                };

                                                recipientList.Add( new RockEmailMessageRecipient( person, personDict ) );
                                            }
                                        }

                                        return true;
                                    }

                                    return false;
                                }
                        }
                    }
                }
            }
            else if ( !string.IsNullOrWhiteSpace( attributeValue ) )
            {
                delimitedEmails = attributeValue;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the group role attribute value Guid.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="attributeKey">The group role attribute key.</param>
        /// <returns></returns>
        private Guid? GetGroupRoleValueGuid( WorkflowAction action, string attributeKey )
        {
            Guid? groupRoleGuid = null;

            Guid? groupRoleAttributeGuid = GetAttributeValue( action, attributeKey ).AsGuidOrNull();

            if ( groupRoleAttributeGuid.HasValue )
            {
                groupRoleGuid = action.GetWorkflowAttributeValue( groupRoleAttributeGuid.Value ).AsGuidOrNull();
            }

            return groupRoleGuid;
        }

        /// <summary>
        /// Builds a list of email addresses from either a delimited string of emails or a list of recipients.
        /// </summary>
        /// <param name="delimitedEmails">The delimited emails.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <param name="recipients">The recipients.</param>
        /// <returns></returns>
        private List<string> BuildEmailList( string delimitedEmails, Dictionary<string, object> mergeFields, List<RockEmailMessageRecipient> recipients )
        {
            if ( !String.IsNullOrWhiteSpace( delimitedEmails ) )
            {
                return delimitedEmails.ResolveMergeFields( mergeFields ).SplitDelimitedValues().Select( e => e ).ToList();
            }

            if ( recipients != null )
            {
                return recipients.Where( r => !string.IsNullOrWhiteSpace( r.EmailAddress ) ).Select( r => r.EmailAddress ).ToList();
            }

            return new List<string>();
        }

        /// <summary>
        /// Sends the specified recipient emails.
        /// </summary>
        /// <param name="recipientEmails">The recipient emails.</param>
        /// <param name="fromEmail">From email.</param>
        /// <param name="fromName">From name.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="ccEmails">The CC emails.</param>
        /// <param name="bccEmails">The BCC emails.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <param name="createCommunicationRecord">if set to <c>true</c> [create communication record].</param>
        /// <param name="attachments">The attachments.</param>
        /// <param name="errorMessages">The error messages.</param>
        private void Send( string recipientEmails, string fromEmail, string fromName, string subject, string body, List<string> ccEmails, List<string> bccEmails, Dictionary<string, object> mergeFields, bool createCommunicationRecord, BinaryFile[] attachments, out List<string> errorMessages )
        {
            var recipients = recipientEmails.ResolveMergeFields( mergeFields ).SplitDelimitedValues().Select( e => RockEmailMessageRecipient.CreateAnonymous( e, mergeFields ) ).ToList();
            Send( recipients, fromEmail, fromName, subject, body, ccEmails, bccEmails, createCommunicationRecord, attachments, out errorMessages );
        }

        /// <summary>
        /// Sends the specified recipients.
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        /// <param name="fromEmail">From email.</param>
        /// <param name="fromName">From name.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="ccEmails">The CC emails.</param>
        /// <param name="bccEmails">The BCC emails.</param>
        /// <param name="createCommunicationRecord">if set to <c>true</c> [create communication record].</param>
        /// <param name="attachments">The attachments.</param>
        /// <param name="errorMessages">The error messages.</param>
        private void Send( List<RockEmailMessageRecipient> recipients, string fromEmail, string fromName, string subject, string body, List<string> ccEmails, List<string> bccEmails, bool createCommunicationRecord, BinaryFile[] attachments, out List<string> errorMessages )
        {
            var emailMessage = new RockEmailMessage();
            emailMessage.SetRecipients( recipients );
            emailMessage.FromEmail = fromEmail;
            emailMessage.FromName = fromName.IsNullOrWhiteSpace() ? fromEmail : fromName;
            emailMessage.Subject = subject;
            emailMessage.Message = body;

            emailMessage.CCEmails = ccEmails ?? new List<string>();
            emailMessage.BCCEmails = bccEmails ?? new List<string>();

            foreach (BinaryFile b in attachments)
            {
                if ( b != null )
                {
                    emailMessage.Attachments.Add( b );
                }
            }

            emailMessage.CreateCommunicationRecord = createCommunicationRecord;
            emailMessage.AppRoot = Rock.Web.Cache.GlobalAttributesCache.Get().GetValue( "InternalApplicationRoot" ) ?? string.Empty;

            emailMessage.Send(out errorMessages);
        }
    }
}