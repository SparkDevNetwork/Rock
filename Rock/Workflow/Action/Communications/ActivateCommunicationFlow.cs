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
using Rock.Enums.Communication;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sends email
    /// </summary>
    [ActionCategory( "Communications" )]
    [Description( "Triggers an On-Demand type of Communication Flow for the list of people you supply" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Activate Communication Flow" )]

    #region Block Attributes

    [WorkflowAttribute(
        "Communication Flow",
        Description = "The communication flow to trigger.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.CommunicationFlow,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.CommunicationFlowFieldType" }
    )]

    [WorkflowAttribute(
        "Person",
        Description = "The person to add to the communication flow.",
        IsRequired = true,
        Order = 1,
        Key = AttributeKey.Person,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" }
    )]

    //[WorkflowTextOrAttribute( "From Name",
    //    "From Name Attribute",
    //    Description = "The name or an attribute that contains the person or name that email should be sent from. <span class='tip tip-lava'></span>",
    //    IsRequired = false,
    //    Order = 0,
    //    Key = AttributeKey.FromName,
    //    FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.PersonFieldType" } )]

    //[WorkflowTextOrAttribute( "From Email Address",
    //    "From Attribute",
    //    "The email address or an attribute that contains the person or email address that email should be sent from (will default to organization email). <span class='tip tip-lava'></span>",
    //    false,
    //    "",
    //    "",
    //    1,
    //    AttributeKey.From,
    //    new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.EmailFieldType", "Rock.Field.Types.PersonFieldType" } )]

    //[WorkflowTextOrAttribute( "Reply To Address",
    //    "Reply To Attribute",
    //    Description = "The email address or an attribute that contains the person or email address that email replies should be sent to (will default to 'From' email). <span class='tip tip-lava'></span>",
    //    IsRequired = false,
    //    Order = 2,
    //    Key = AttributeKey.ReplyTo,
    //    FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.EmailFieldType", "Rock.Field.Types.PersonFieldType" } )]

    //[WorkflowTextOrAttribute( "Send To Email Addresses",
    //    "To Attribute",
    //    "The email addresses or an attribute that contains the person, email address, group or security role that the email should be sent to. <span class='tip tip-lava'></span>",
    //    true,
    //    "",
    //    "",
    //    3,
    //    AttributeKey.To,
    //    new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.EmailFieldType", "Rock.Field.Types.PersonFieldType", "Rock.Field.Types.GroupFieldType", "Rock.Field.Types.SecurityRoleFieldType" } )]

    //[WorkflowAttribute( "Send to Group Role",
    //    Key = AttributeKey.GroupRole,
    //    Description = "An optional Group Role attribute to limit recipients to if the 'Send to Email Addresses' is a group or security role.",
    //    IsRequired = false,
    //    Order = 4,
    //    FieldTypeClassNames = new string[] { "Rock.Field.Types.GroupRoleFieldType" } )]

    //[TextField( "Subject",
    //    Key = AttributeKey.Subject,
    //    Description = "The subject that should be used when sending email. <span class='tip tip-lava'></span>",
    //    IsRequired = false,
    //    Order = 5 )]

    //[CodeEditorField( "Body",
    //    Key = AttributeKey.Body,
    //    Description = "The body of the email that should be sent. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",
    //    EditorMode = Web.UI.Controls.CodeEditorMode.Html,
    //    EditorTheme = Web.UI.Controls.CodeEditorTheme.Rock,
    //    EditorHeight = 200,
    //    IsRequired = false,
    //    Order = 6 )]

    //[WorkflowTextOrAttribute( "CC Email Addresses",
    //    "CC Attribute",
    //    "The email addresses or an attribute that contains the person, email address, group or security role that the email should be CC'd (carbon copied) to. Any address in this field will be copied on the email sent to every recipient. <span class='tip tip-lava'></span>",
    //    false,
    //    "",
    //    "",
    //    7,
    //    AttributeKey.Cc,
    //    new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.EmailFieldType", "Rock.Field.Types.PersonFieldType", "Rock.Field.Types.GroupFieldType", "Rock.Field.Types.SecurityRoleFieldType" } )]

    //[WorkflowTextOrAttribute( "BCC Email Addresses",
    //    "BCC Attribute",
    //    "The email addresses or an attribute that contains the person, email address, group or security role that the email should be BCC'd (blind carbon copied) to. Any address in this field will be copied on the email sent to every recipient. <span class='tip tip-lava'></span>",
    //    false,
    //    "",
    //    "",
    //    8,
    //    AttributeKey.Bcc,
    //    new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.EmailFieldType", "Rock.Field.Types.PersonFieldType", "Rock.Field.Types.GroupFieldType", "Rock.Field.Types.SecurityRoleFieldType" } )]


    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "4DDFE046-F05D-4857-BA9B-22391A605ED7" )]
    public class ActivateCommunicationFlow : ActionComponent
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string CommunicationFlow = "CommunicationFlow";
            public const string Person = "Person";
            //public const string ReplyTo = "ReplyTo";
            //public const string GroupRole = "GroupRole";
            //public const string Subject = "Subject";
            //public const string Body = "Body";
            //public const string Cc = "CC";
            //public const string Bcc = "BCC";
            //public const string AttachmentOne = "AttachmentOne";
            //public const string AttachmentTwo = "AttachmentTwo";
            //public const string AttachmentThree = "AttachmentThree";
            //public const string SaveCommunicationHistory = "SaveCommunicationHistory";
            //public const string FromName = "FromName";
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

            var communicationFlowService = new CommunicationFlowService( rockContext );
            var personAliasService = new PersonAliasService( rockContext );

            var communicationFlowGuid = GetAttributeValue( action, AttributeKey.CommunicationFlow, true ).AsGuidOrNull();
            var personAliasGuid = GetAttributeValue( action, AttributeKey.Person, true ).AsGuidOrNull();

            var communicationFlowId = communicationFlowGuid.HasValue
                ? communicationFlowService.GetId( communicationFlowGuid.Value )
                : null;

            if ( !communicationFlowId.HasValue )
            {
                errorMessages.Add( "Communication Flow is required" );
            }

            var personId = personAliasGuid.HasValue
                ? personAliasService
                    .Queryable()
                    .Where( a => a.Guid.Equals( personAliasGuid.Value ) )
                    .Select( a => ( int? ) a.PersonId )
                    .FirstOrDefault()
                : null;

            if ( !personId.HasValue )
            {
                errorMessages.Add( "Person is required" );
            }

            if ( errorMessages.Any() )
            {
                // Stop early if there are any errors so far.
                return false;
            }

            communicationFlowService.AutoAssignPersonToOnDemandCommunicationFlowInstance( communicationFlowId.Value, personId.Value, action.CreatedByPersonAliasId );

            rockContext.SaveChanges();

            return true;
        }
    }
}