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
    [Description( "Sends an email. The recipient can either be a group, person or email address determined by the 'To Attribute' value, or an email address entered in the 'To' field. Only people with an active email address without the 'Do Not Email' preference are included. If attribute is a group, only members with an <em>Active</em> member status are included." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Email Send" )]

    [WorkflowTextOrAttribute( "From Email Address", "Attribute Value", "The email address or an attribute that contains the person or email address that email should be sent from (will default to organization email). <span class='tip tip-lava'></span>", false, "", "", 0, "From",
        new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.EmailFieldType", "Rock.Field.Types.PersonFieldType" } )]
    [WorkflowTextOrAttribute( "Send To Email Addresses", "Attribute Value", "The email addresses or an attribute that contains the person or email address that email should be sent to. <span class='tip tip-lava'></span>", true, "", "", 1, "To",
        new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.EmailFieldType", "Rock.Field.Types.PersonFieldType", "Rock.Field.Types.GroupFieldType", "Rock.Field.Types.SecurityRoleFieldType" } )]
    [WorkflowAttribute( "Send to Group Role", "An optional Group Role attribute to limit recipients to if the 'Send to Email Address' is a group or security role.", false, "", "", 2, "GroupRole",
        new string[] { "Rock.Field.Types.GroupRoleFieldType" } )]
    [TextField( "Subject", "The subject that should be used when sending email. <span class='tip tip-lava'></span>", false, "", "", 3 )]
    [CodeEditorField( "Body", "The body of the email that should be sent. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", Web.UI.Controls.CodeEditorMode.Html, Web.UI.Controls.CodeEditorTheme.Rock, 200, false, "", "", 4 )]
    [BooleanField( "Save Communication History", "Should a record of this communication be saved to the recipient's profile", false, "", 5 )]
    public class SendEmail : ActionComponent
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

            var mergeFields = GetMergeFields( action );

            string to = GetAttributeValue( action, "To" );
            string fromValue = GetAttributeValue( action, "From" );
            string subject = GetAttributeValue( action, "Subject" );
            string body = GetAttributeValue( action, "Body" );

            bool createCommunicationRecord = GetAttributeValue( action, "SaveCommunicationHistory" ).AsBoolean();

            string fromEmail = string.Empty;
            string fromName = string.Empty;
            Guid? fromGuid = fromValue.AsGuidOrNull();
            if ( fromGuid.HasValue )
            {
                var attribute = AttributeCache.Read( fromGuid.Value, rockContext );
                if ( attribute != null )
                {
                    string fromAttributeValue = action.GetWorklowAttributeValue( fromGuid.Value );
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
                                    fromEmail = person.Email;
                                    fromName = person.FullName;
                                }
                            }
                        }
                        else
                        {
                            fromEmail = fromAttributeValue;
                        }
                    }
                }
            }
            else
            {
                fromEmail = fromValue;
            }

            Guid? guid = to.AsGuidOrNull();
            if ( guid.HasValue )
            {
                var attribute = AttributeCache.Read( guid.Value, rockContext );
                if ( attribute != null )
                {
                    string toValue = action.GetWorklowAttributeValue( guid.Value );
                    if ( !string.IsNullOrWhiteSpace( toValue ) )
                    {
                        switch ( attribute.FieldType.Class )
                        {
                            case "Rock.Field.Types.TextFieldType":
                            case "Rock.Field.Types.EmailFieldType":
                                {
                                    Send( toValue, fromEmail, fromName, subject, body, mergeFields, rockContext, createCommunicationRecord );
                                    break;
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
                                            action.AddLogEntry( "Invalid Recipient: Person not found", true );
                                        }
                                        else if ( string.IsNullOrWhiteSpace( person.Email ) )
                                        {
                                            action.AddLogEntry( "Email was not sent: Recipient does not have an email address", true );
                                        }
                                        else if ( !person.IsEmailActive )
                                        {
                                            action.AddLogEntry( "Email was not sent: Recipient email is not active", true );
                                        }
                                        else if ( person.EmailPreference == EmailPreference.DoNotEmail )
                                        {
                                            action.AddLogEntry( "Email was not sent: Recipient has requested 'Do Not Email'", true );
                                        }
                                        else
                                        {
                                            var personDict = new Dictionary<string, object>( mergeFields );
                                            personDict.Add( "Person", person );
                                            Send( person.Email, fromEmail, fromName, subject, body, personDict, rockContext, createCommunicationRecord );
                                        }
                                    }
                                    break;
                                }
                            case "Rock.Field.Types.GroupFieldType":
                            case "Rock.Field.Types.SecurityRoleFieldType":
                                {
                                    int? groupId = toValue.AsIntegerOrNull();
                                    Guid? groupGuid = toValue.AsGuidOrNull();

                                    //Get the Group Role attribute value
                                    Guid? groupRoleValueGuid = GetGroupRoleValue( action );

                                    IQueryable<GroupMember> qry = null;

                                    // Handle situations where the attribute value is the ID
                                    if ( groupId.HasValue )
                                    {
                                        qry = new GroupMemberService( rockContext ).GetByGroupId( groupId.Value );
                                    }

                                    // Handle situations where the attribute value stored is the Guid
                                    else if ( groupGuid.HasValue )
                                    {
                                        qry = new GroupMemberService( rockContext ).GetByGroupGuid( groupGuid.Value );
                                    }
                                    else
                                    {
                                        action.AddLogEntry( "Invalid Recipient: No valid group id or Guid", true );
                                    }


                                    if ( groupRoleValueGuid.HasValue )
                                    {
                                        qry = qry.Where( m => m.GroupRole != null && m.GroupRole.Guid.Equals( groupRoleValueGuid.Value ) );
                                    }

                                    if ( qry != null )
                                    {
                                        foreach ( var person in qry
                                            .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active )
                                            .Select( m => m.Person ) )
                                        {
                                            if ( ( person.IsEmailActive ) &&
                                                person.EmailPreference != EmailPreference.DoNotEmail &&
                                                !string.IsNullOrWhiteSpace( person.Email ) )
                                            {
                                                var personDict = new Dictionary<string, object>( mergeFields );
                                                personDict.Add( "Person", person );
                                                Send( person.Email, fromEmail, fromName, subject, body, personDict, rockContext, createCommunicationRecord );
                                            }
                                        }
                                    }
                                    break;
                                }
                        }
                    }
                }
            }
            else
            {
                Send( to.ResolveMergeFields( mergeFields ), fromEmail, fromName, subject, body, mergeFields, rockContext, createCommunicationRecord );
            }

            return true;
        }

        private Guid? GetGroupRoleValue( WorkflowAction action )
        {
            Guid? groupRoleGuid = null;

            string groupRole = GetAttributeValue( action, "GroupRole" );
            Guid? groupRoleAttributeGuid = GetAttributeValue( action, "GroupRole" ).AsGuidOrNull();

            if ( groupRoleAttributeGuid.HasValue )
            {
                groupRoleGuid = action.GetWorklowAttributeValue( groupRoleAttributeGuid.Value ).AsGuidOrNull();
            }

            return groupRoleGuid;
        }

        private void Send( string recipients, string fromEmail, string fromName, string subject, string body, Dictionary<string, object> mergeFields, RockContext rockContext, bool createCommunicationRecord )
        {
            var emailMessage = new RockEmailMessage();
            foreach( string recipient in recipients.SplitDelimitedValues().ToList() )
            {
                emailMessage.AddRecipient( new RecipientData( recipient, mergeFields ) );
            }
            emailMessage.FromEmail = fromEmail;
            emailMessage.FromName = fromName;
            emailMessage.Subject = subject;
            emailMessage.Message = body;
            emailMessage.CreateCommunicationRecord = createCommunicationRecord;

            emailMessage.Send();
        }
    }
}