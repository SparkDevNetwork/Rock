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
    /// Sends push notification
    /// </summary>
    [ActionCategory( "Communications" )]
    [Description( "Sends a push notification. The recipient can either be a person or group attribute." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Push Notification Send" )]

    [WorkflowTextOrAttribute( "Recipient",
        "Attribute Value",
        "An attribute that contains the person should be sent to. <span class='tip tip-lava'></span>",
        true,
        "",
        "",
        1,
        AttributeKey.To,
        new string[] { "Rock.Field.Types.PersonFieldType", "Rock.Field.Types.GroupFieldType", "Rock.Field.Types.SecurityRoleFieldType" } )]

    [WorkflowTextOrAttribute( "Title",
        "Attribute Value",
        "The title or an attribute that contains the title that should be sent.",
        false,
        "",
        "",
        2,
        AttributeKey.Title,
        new string[] { "Rock.Field.Types.TextFieldType" } )]

    [WorkflowAttribute( "Sound",
        "The choice of sound or an attribute that contains the choice of sound that should be sent.",
        false,
        "True",
        "",
        2,
        AttributeKey.Sound,
        new string[] { "Rock.Field.Types.BooleanFieldType" } )]

    [WorkflowTextOrAttribute( "Message",
        "Attribute Value",
        "The message or an attribute that contains the message that should be sent. <span class='tip tip-lava'></span>",
        true,
        "",
        "",
        3,
        AttributeKey.Message,
        new string[] { "Rock.Field.Types.TextFieldType" } )]

    [SiteField( "Application",
        Key = AttributeKey.MobileApplication,
        Description = "Select the mobile application that the push notification should open. Leave blank to send to a non-Rock Mobile app, use an external URL, or send to all applications.",
        Order = 4,
        IsRequired = false,
        MobileSitesOnly = true )]

    [LinkedPage( "Mobile Page",
        Key = AttributeKey.MobilePage,
        Description = "Select the page within the mobile application to open when the notification is tapped. Leave blank to open the app without a specific page, or when using an external URL.",
        IsRequired = false,
        Order = 5 )]

    [WorkflowTextOrAttribute( "URL",
        "Attribute Value",
        "Enter a URL (or select an attribute containing a URL) to open when the notification is tapped. Leave blank when using Application or Mobile Page. <span class='tip tip-lava'></span>",
        false,
        "",
        "",
        6,
        AttributeKey.Url,
        new string[] { "Rock.Field.Types.TextFieldType" } )]

    [Rock.SystemGuid.EntityTypeGuid( "22CAA82F-7AE2-430C-AE88-FA7401981F60")]
    public class SendPushNotification : ActionComponent
    {
        private static class AttributeKey
        {
            public const string MobileApplication = "MobileApplication";
            public const string MobilePage = "MobilePage";
            public const string Url = "Url";
            public const string Sound = "Sound";
            public const string Title = "Title";
            public const string Message = "Message";
            public const string To = "To";
        }

        /// <summary>
        /// Gets the active device registration identifiers for the provided person alias.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="siteId">The optional site identifier used to scope Rock Mobile devices.</param>
        /// <returns>A list of active device registration identifiers.</returns>
        private static List<string> GetDeviceRegistrationIds( RockContext rockContext, int? personAliasId, int? siteId )
        {
            if ( !personAliasId.HasValue )
            {
                return new List<string>();
            }

            var deviceQuery = new PersonalDeviceService( rockContext ).Queryable()
                .Where( p => p.PersonAliasId.HasValue
                    && p.PersonAliasId.Value == personAliasId.Value
                    && p.IsActive
                    && p.NotificationsEnabled
                    && !string.IsNullOrEmpty( p.DeviceRegistrationId ) );

            if ( siteId.HasValue )
            {
                deviceQuery = deviceQuery.Where( p => p.SiteId == siteId.Value );
            }

            return deviceQuery
                .Select( p => p.DeviceRegistrationId )
                .ToList();
        }

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
            var recipients = new List<RockPushMessageRecipient>();
            var personAliasService = new PersonAliasService( rockContext );

            var siteId = GetAttributeValue( action, AttributeKey.MobileApplication ).AsIntegerOrNull();
            var mobilePageValue = GetAttributeValue( action, AttributeKey.MobilePage );
            var mobilePageGuid = mobilePageValue.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).FirstOrDefault().AsGuidOrNull();
            var mobilePage = mobilePageGuid.HasValue ? PageCache.Get( mobilePageGuid.Value ) : null;
            var url = GetAttributeValue( action, AttributeKey.Url );
            var urlGuid = url.AsGuid();
            if ( !urlGuid.IsEmpty() )
            {
                var attribute = AttributeCache.Get( urlGuid, rockContext );
                if ( attribute != null )
                {
                    var urlAttributeValue = action.GetWorkflowAttributeValue( urlGuid );
                    if ( !string.IsNullOrWhiteSpace( urlAttributeValue ) )
                    {
                        if ( attribute.FieldType.Class == "Rock.Field.Types.TextFieldType" )
                        {
                            url = urlAttributeValue;
                        }
                    }
                }
            }

            var resolvedPage = ResolveMobilePage( mobilePage, url );
            var resolvedSiteId = ResolveMobileApplication( siteId, resolvedPage );

            if ( resolvedPage?.SiteId != null && resolvedPage?.SiteId != resolvedSiteId )
            {
                errorMessages.Add( "The selected Mobile Page must belong to the selected Application." );
                return false;
            }

            var toValue = GetAttributeValue( action, AttributeKey.To );
            var guid = toValue.AsGuid();
            if ( !guid.IsEmpty() )
            {
                var attribute = AttributeCache.Get( guid, rockContext );
                if ( attribute != null )
                {
                    var toAttributeValue = action.GetWorkflowAttributeValue( guid );
                    if ( !string.IsNullOrWhiteSpace( toAttributeValue ) )
                    {
                        switch ( attribute.FieldType.Class )
                        {
                            case "Rock.Field.Types.PersonFieldType":
                                {
                                    var personAliasGuid = toAttributeValue.AsGuid();
                                    if ( !personAliasGuid.IsEmpty() )
                                    {
                                        var personAlias = personAliasService.Get( personAliasGuid );
                                        var person = personAliasService.GetPerson( personAliasGuid );

                                        if ( person == null )
                                        {
                                            action.AddLogEntry( "Invalid Recipient: Person was not found", true );
                                        }
                                        else
                                        {
                                            var deviceIds = GetDeviceRegistrationIds( rockContext, personAlias?.Id, resolvedSiteId );
                                            if ( deviceIds.Any() )
                                            {
                                                var recipient = new RockPushMessageRecipient( person, string.Join( ",", deviceIds ), new Dictionary<string, object>( mergeFields ) );
                                                recipients.Add( recipient );
                                                recipient.MergeFields.Add( recipient.PersonMergeFieldKey, person );
                                            }
                                            else
                                            {
                                                action.AddLogEntry( "Invalid Recipient: Person does not have devices that support notifications", true );
                                            }
                                        }
                                    }
                                    break;
                                }

                            case "Rock.Field.Types.GroupFieldType":
                            case "Rock.Field.Types.SecurityRoleFieldType":
                                {
                                    var groupId = toAttributeValue.AsIntegerOrNull();
                                    var groupGuid = toAttributeValue.AsGuidOrNull();
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

                                    if ( qry != null )
                                    {
                                        foreach ( var person in qry
                                            .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active )
                                            .Select( m => m.Person ) )
                                        {
                                            if ( person != null )
                                            {
                                                var deviceIds = GetDeviceRegistrationIds( rockContext, person.PrimaryAliasId, resolvedSiteId );
                                                if ( deviceIds.Any() )
                                                {
                                                    var recipient = new RockPushMessageRecipient( person, string.Join( ",", deviceIds ), new Dictionary<string, object>( mergeFields ) );
                                                    recipients.Add( recipient );
                                                    recipient.MergeFields.Add( recipient.PersonMergeFieldKey, person );
                                                }
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
                if ( !string.IsNullOrWhiteSpace( toValue ) )
                {
                    recipients.Add( RockPushMessageRecipient.CreateAnonymous( toValue.ResolveMergeFields( mergeFields ), new Dictionary<string, object>( mergeFields ) ) );
                }
            }

            var message = GetAttributeValue( action, AttributeKey.Message );
            var messageGuid = message.AsGuid();
            if ( !messageGuid.IsEmpty() )
            {
                var attribute = AttributeCache.Get( messageGuid, rockContext );
                if ( attribute != null )
                {
                    var messageAttributeValue = action.GetWorkflowAttributeValue( messageGuid );
                    if ( !string.IsNullOrWhiteSpace( messageAttributeValue ) )
                    {
                        if ( attribute.FieldType.Class == "Rock.Field.Types.TextFieldType" )
                        {
                            message = messageAttributeValue;
                        }
                    }
                }
            }

            var title = GetAttributeValue( action, AttributeKey.Title );
            var titleGuid = title.AsGuid();
            if ( !titleGuid.IsEmpty() )
            {
                var attribute = AttributeCache.Get( titleGuid, rockContext );
                if ( attribute != null )
                {
                    var titleAttributeValue = action.GetWorkflowAttributeValue( titleGuid );
                    if ( !string.IsNullOrWhiteSpace( titleAttributeValue ) )
                    {
                        if ( attribute.FieldType.Class == "Rock.Field.Types.TextFieldType" )
                        {
                            title = titleAttributeValue;
                        }
                    }
                }
            }

            var sound = GetAttributeValue( action, AttributeKey.Sound );
            var soundGuid = sound.AsGuid();
            if ( !soundGuid.IsEmpty() )
            {
                var attribute = AttributeCache.Get( soundGuid, rockContext );
                if ( attribute != null )
                {
                    var soundAttributeValue = action.GetWorkflowAttributeValue( soundGuid );
                    if ( !string.IsNullOrWhiteSpace( soundAttributeValue ) )
                    {
                        if ( attribute.FieldType.Class == "Rock.Field.Types.BooleanFieldType" )
                        {
                            sound = soundAttributeValue;
                        }
                    }
                }
            }
            sound = sound.AsBoolean() ? "default" : "";

            if ( recipients.Any() && !string.IsNullOrWhiteSpace( message ) )
            {
                var pushMessage = new RockPushMessage();
                pushMessage.SetRecipients( recipients );
                pushMessage.Title = title;
                pushMessage.Message = message;
                pushMessage.Sound = sound;
                pushMessage.Data = new PushData();

                if ( resolvedSiteId.HasValue )
                {
                    pushMessage.OpenAction = Utility.PushOpenAction.LinkToMobilePage;
                    pushMessage.Data.MobileApplicationId = resolvedSiteId.Value;
                    pushMessage.Data.MobilePageId = resolvedPage?.Id;

                    // Support legacy mobile-style URLs to populate query string values.
                    if ( url.Length >= 38 && Guid.TryParse( url.Substring( 0, 36 ), out var pageGuid ) && url[36] == '?' )
                    {
                        var pageId = PageCache.GetId( pageGuid );

                        if ( pageId.HasValue && pageId.Value == resolvedPage?.Id )
                        {
                            var queryString = url.Substring( 37 ).ParseQueryString();

                            pushMessage.Data.MobilePageQueryString = new Dictionary<string, string>();

                            foreach ( string key in queryString.Keys )
                            {
                                pushMessage.Data.MobilePageQueryString.AddOrReplace( key, queryString[key].ToString() );
                            }
                        }
                    }
                }
                else if ( url.IsNotNullOrWhiteSpace() )
                {
                    pushMessage.OpenAction = Utility.PushOpenAction.LinkToUrl;
                    pushMessage.Data.Url = url;
                }

                pushMessage.Send( out errorMessages );
            }

            return true;
        }

        /// <summary>
        /// Resolves the site identifier for a mobile application based on the specified site or page context.
        /// </summary>
        /// <param name="siteId">An optional site identifier to use as the primary source. If specified, this value is returned.</param>
        /// <param name="resolvedPage">A page context from which to derive the site identifier if <paramref name="siteId"/> is not specified. If
        /// not null, the site's identifier associated with this page is used.</param>
        /// <returns>The resolved site identifier if available; otherwise, null if neither <paramref name="siteId"/> nor
        /// <paramref name="resolvedPage"/> provides a value.</returns>
        private static int? ResolveMobileApplication( int? siteId, PageCache resolvedPage )
        {
            if ( siteId.HasValue )
            {
                // Use the selected site first.
                return siteId.Value;
            }

            if ( resolvedPage != null )
            {
                // Otherwise, derive the site from the resolved page.
                return resolvedPage.SiteId;
            }

            // No specific app selected.
            return null;
        }

        /// <summary>
        /// Resolves the appropriate mobile page to use based on the provided page cache or a page identifier in the URL.
        /// </summary>
        /// <remarks>If a mobile page is provided, it is returned directly. If not, and the URL contains a
        /// valid GUID at the start, the corresponding page is retrieved. Otherwise, the method returns null.</remarks>
        /// <param name="mobilePage">The preselected mobile page to use, or null to attempt resolution from the URL.</param>
        /// <param name="url">The URL that may contain a page identifier as a GUID in its first 36 characters. Can be null.</param>
        /// <returns>A PageCache instance representing the resolved mobile page, or null if no suitable page is found.</returns>
        private static PageCache ResolveMobilePage( PageCache mobilePage, string url )
        {
            if ( mobilePage != null )
            {
                // Use selected page first.
                return mobilePage;
            }

            if ( url != null && url.Length >= 36 && Guid.TryParse( url.Substring( 0, 36 ), out var pageGuid ) )
            {
                // Otherwise, use the page derived from the URL.
                return PageCache.Get( pageGuid );
            }

            // No specific page selected.
            return null;
        }
    }
}
