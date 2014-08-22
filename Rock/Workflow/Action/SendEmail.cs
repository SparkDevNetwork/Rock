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
    [Description( "Sends an email.  The recipient can either be a person or email address determined by the 'To Attribute' value, or an email address entered in the 'To' field." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Send Email" )]

    [WorkflowTextOrAttribute("Send To Email Address", "Attribute Value", "The email address or an attribute that contains the person or email address that email should be sent to", true, "", "", 0, "To")]
    [TextField( "From", "The From address that email should be sent from  (will default to organization email). <span class='tip tip-liquid'></span>", false, "", "", 1 )]
    [TextField( "Subject", "The subject that should be used when sending email. <span class='tip tip-liquid'></span>", false, "", "", 2 )]
    [CodeEditorField( "Body", "The body of the email that should be sent. <span class='tip tip-liquid'></span> <span class='tip tip-html'></span>", Web.UI.Controls.CodeEditorMode.Html, Web.UI.Controls.CodeEditorTheme.Rock, 200, false, "", "", 3 )]
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
            string from = GetAttributeValue( action, "From" ).ResolveMergeFields( mergeFields );
            string subject = GetAttributeValue( action, "Subject" ).ResolveMergeFields( mergeFields );
            string body = GetAttributeValue( action, "Body" );

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
                                {
                                    Send( toValue, from, subject, body, mergeFields, rockContext );
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
                                        if ( person != null && !string.IsNullOrWhiteSpace(person.Email) )
                                        {
                                            var personDict = new Dictionary<string, object>(mergeFields);
                                            personDict.Add("Person", person);
                                            Send( person.Email, from, subject, body, personDict, rockContext );
                                        }
                                    }
                                    break;
                                }
                            case "Rock.Field.Types.GroupFieldType":
                                {
                                    int? groupId = toValue.AsIntegerOrNull();
                                    if ( !groupId.HasValue )
                                    {
                                        foreach ( var person in new GroupMemberService( rockContext )
                                            .GetByGroupId( groupId.Value )
                                            .Select( m => m.Person ) )
                                        {
                                            if ( person != null && !string.IsNullOrWhiteSpace( person.Email ) )
                                            {
                                                var personDict = new Dictionary<string, object>( mergeFields );
                                                personDict.Add( "Person", person );
                                                Send( person.Email, from, subject, body, personDict, rockContext );
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
                Send( to, from, subject, body, mergeFields, rockContext );
            }

            return true;
        }

        private void Send( string recipient, string from, string subject, string body, Dictionary<string, object> mergeFields, RockContext rockContext )
        {
            var recipients = new List<string>();
            recipients.Add( recipient );
             
            var channelData = new Dictionary<string, string>();
            channelData.Add( "From", from.ResolveMergeFields( mergeFields ) );
            channelData.Add( "Subject", subject.ResolveMergeFields( mergeFields ) );
            channelData.Add( "Body", System.Text.RegularExpressions.Regex.Replace( body.ResolveMergeFields( mergeFields ), @"\[\[\s*UnsubscribeOption\s*\]\]", string.Empty ) );

            var channelEntity = EntityTypeCache.Read( Rock.SystemGuid.EntityType.COMMUNICATION_CHANNEL_EMAIL.AsGuid(), rockContext );
            if ( channelEntity != null )
            {
                var channel = ChannelContainer.GetComponent( channelEntity.Name );
                if ( channel != null && channel.IsActive )
                {
                    var transport = channel.Transport;
                    if ( transport != null && transport.IsActive )
                    {
                        var appRoot = GlobalAttributesCache.Read( rockContext ).GetValue( "InternalApplicationRoot" );
                        transport.Send( channelData, recipients, appRoot, string.Empty );
                    }
                }
            }
        }
    }
}