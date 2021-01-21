// <copyright>
// Copyright by BEMA Information Technologies
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

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Workflow;

namespace com.bemaservices.WorkflowExtensions.Workflow.Action
{
    /// <summary>
    /// Adds a new person to a family.
    /// </summary>
    [ActionCategory( "BEMA Services > Workflow Extensions" )]
    [Description( "Sets an attribute to a person with matching name and email. If single match is not found a new person will be created." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Reset Password" )]

    [WorkflowAttribute( "Person Attribute", "The person attribute to set the value to the person found or created.",
        true, "", "", 1, PERSON_ATTRIBUTE_KEY, new string[] { "Rock.Field.Types.PersonFieldType" } )]

    [SystemCommunicationField( "Forgot Username Email Template", "Email Template to send", false, Rock.SystemGuid.SystemCommunication.SECURITY_FORGOT_USERNAME, "", 2, EMAIL_TEMPLATE_KEY )]

    [WorkflowTextOrAttribute( "Url", "Url Attribute", "The full Url for the user to confirm their account, for example: http://www.rockrms.com/ConfirmAccount  <span class='tip tip-lava'></span>", true, "", "", 3, CONFIRMATION_URL_KEY, new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.UrlLinkFieldType" } )]


    public class ResetPassword : ActionComponent
    {

        private const string PERSON_ATTRIBUTE_KEY = "PersonAttribute";
        private const string EMAIL_TEMPLATE_KEY = "EmailTemplate";
        private const string CONFIRMATION_URL_KEY = "Url";

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

            // get url
            string url = GetAttributeValue( action, "Url" );
            Guid guid = url.AsGuid();
            if ( guid.IsEmpty() )
            {
                url = url.ResolveMergeFields( GetMergeFields( action ) );
            }
            else
            {
                url = action.GetWorklowAttributeValue( guid );
            }

            // get person
            Person person = null;

            string personAttributeValue = GetAttributeValue( action, PERSON_ATTRIBUTE_KEY );
            Guid? guidPersonAttribute = personAttributeValue.AsGuidOrNull();
            if ( guidPersonAttribute.HasValue )
            {
                var attributePerson = AttributeCache.Get( guidPersonAttribute.Value, rockContext );
                if ( attributePerson != null || attributePerson.FieldType.Class != "Rock.Field.Types.PersonFieldType" )
                {
                    string attributePersonValue = action.GetWorklowAttributeValue( guidPersonAttribute.Value );
                    if ( !string.IsNullOrWhiteSpace( attributePersonValue ) )
                    {
                        Guid personAliasGuid = attributePersonValue.AsGuid();
                        if ( !personAliasGuid.IsEmpty() )
                        {
                            person = new PersonAliasService( rockContext ).Queryable()
                                .Where( a => a.Guid.Equals( personAliasGuid ) )
                                .Select( a => a.Person )
                                .FirstOrDefault();
                            if ( person == null )
                            {
                                errorMessages.Add( string.Format( "Person could not be found for selected value ('{0}')!", guidPersonAttribute.ToString() ) );
                                return false;
                            }
                        }
                    }
                }
            }

            if ( person == null )
            {
                errorMessages.Add( "The attribute used to provide the person was invalid, or not of type 'Person'." );
                return false;
            }
            else if ( string.IsNullOrWhiteSpace( person.Email ) )
            {
                errorMessages.Add( "Password not reset: Recipient does not have an email address" );
            }
            else if ( !( person.IsEmailActive ) )
            {
                errorMessages.Add( "Password not reset: Recipient email is not active" );
            }
            else if ( person.EmailPreference == EmailPreference.DoNotEmail )
            {
                errorMessages.Add( "Password not reset: Recipient has requested 'Do Not Email'" );
            }

            var mergeFields = GetMergeFields( action );
            mergeFields.Add( "ConfirmAccountUrl", url );
            //    mergeFields.Add( "ConfirmAccountUrl", RootPath + url.TrimStart( new char[] { '/' } ) );
            var results = new List<IDictionary<string, object>>();

            var personService = new PersonService( rockContext );
            var userLoginService = new UserLoginService( rockContext );

            bool hasAccountWithPasswordResetAbility = false;
            List<string> accountTypes = new List<string>();

            var users = new List<UserLogin>();
            foreach ( UserLogin user in userLoginService.GetByPersonId( person.Id ) )
            {
                if ( user.EntityType != null )
                {
                    var component = AuthenticationContainer.GetComponent( user.EntityType.Name );
                    if ( component != null && !component.RequiresRemoteAuthentication )
                    {
                        users.Add( user );
                        hasAccountWithPasswordResetAbility = true;
                    }

                    accountTypes.Add( user.EntityType.FriendlyName );
                }
            }

            var resultsDictionary = new Dictionary<string, object>();
            resultsDictionary.Add( "Person", person );
            resultsDictionary.Add( "Users", users );
            results.Add( resultsDictionary );

            if( results.Count == 0 )
            {
                errorMessages.Add( "No User Logins were found for this person." );
            }
            else if ( results.Count > 0 && !hasAccountWithPasswordResetAbility )
            {
                // the person has user accounts but none of them are allowed to have their passwords reset (Facebook/Google/etc)
                errorMessages.Add( string.Format( "The following accounts for this person, but none of them are able to be reset from this website. Accounts:{0}</p>"
                                            , string.Join( ",", accountTypes ) ) );
            }

            if ( errorMessages.Any() )
            {
                return false;
            }
            else
            { 
                mergeFields.Add( "Results", results.ToArray() );

                var emailMessage = new RockEmailMessage( GetAttributeValue( action, EMAIL_TEMPLATE_KEY ).AsGuid() );
                emailMessage.AddRecipient( new RockEmailMessageRecipient( person, mergeFields ) );
                emailMessage.CreateCommunicationRecord = false;
                emailMessage.Send();

            }

                return true;
        }

    }
}