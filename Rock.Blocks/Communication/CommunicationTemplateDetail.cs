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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.Http.Results;

using Rock.Attribute;
using Rock.Cms.StructuredContent;
using Rock.Communication;
using Rock.Constants;
using Rock.Enums.Blocks.Communication.CommunicationTemplateDetail;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Communication.CommunicationTemplateDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// A block for editing a communication template that can be selected when creating a new communication, SMS, etc. to people.
    /// </summary>
    [DisplayName( "Communication Template Detail" )]
    [Category( "Communication" )]
    [Description( "Used for editing a communication template that can be selected when creating a new communication, SMS, etc. to people." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [BooleanField( "Personal Templates View",
        Key = AttributeKey.PersonalTemplatesView,
        Description = "Is this block being used to display personal templates (only templates that current user is allowed to edit)?",
        DefaultBooleanValue = false,
        Order = 0 )]

    [BinaryFileTypeField( "Attachment Binary File Type",
        Key = AttributeKey.AttachmentBinaryFileType,
        Description = "The FileType to use for files that are attached to an sms or email communication",
        IsRequired = true,
        DefaultBinaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.COMMUNICATION_ATTACHMENT,
        Order = 1 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "017EEC30-BDDA-4159-8249-2852AF4ADCF2" )]
    [Rock.SystemGuid.BlockTypeGuid( "FBAB4EB2-B180-4A76-9B5B-C75E2255F691" )]
    public class CommunicationTemplateDetail : RockBlockType
    {
        #region Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string PersonalTemplatesView = "PersonalTemplatesView";
            public const string AttachmentBinaryFileType = "AttachmentBinaryFileType";
        }

        private static class PageParameterKey
        {
            public const string TemplateId = "TemplateId";
            public const string CommunicationTemplate = "CommunicationTemplate";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Properties

        private string CommunicationTemplateKeyPageParameter
        {
            get
            {
                var communicationTemplatePageParameter = PageParameter( PageParameterKey.CommunicationTemplate );

                if ( communicationTemplatePageParameter.IsNotNullOrWhiteSpace() )
                {
                    return communicationTemplatePageParameter;
                }

                return PageParameter( PageParameterKey.TemplateId );
            }
        }

        private Guid AttachmentsBinaryFileTypeGuidOrDefault => GetAttributeValue( AttributeKey.AttachmentBinaryFileType ).AsGuidOrNull()
            ?? SystemGuid.BinaryFiletype.DEFAULT.AsGuid();

        private bool IsPersonalTemplatesView => GetAttributeValue( AttributeKey.PersonalTemplatesView ).AsBoolean();

        #endregion Properties

        public override object GetObsidianBlockInitialization()
        {
            var communicationTemplateKey = this.CommunicationTemplateKeyPageParameter;
            
            CommunicationTemplate communicationTemplate = null;
            var isNewTemplate = false;
            var pushCommunication = new CommunicationDetails();
            var binaryFileService = new BinaryFileService( this.RockContext );
            var currentPerson = GetCurrentPerson();

            var box = new CommunicationTemplateDetailInitializationBox
            {
                NavigationUrls =
                {
                    [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
                }
            };

            if ( communicationTemplateKey.IsNotNullOrWhiteSpace() && communicationTemplateKey.AsInteger() != 0 )
            {
                communicationTemplate = new CommunicationTemplateService( this.RockContext ).GetQueryableByKey( communicationTemplateKey )
                    .Include( c => c.Attachments.Select( a => a.BinaryFile ) )
                    .FirstOrDefault();

                if ( communicationTemplate != null )
                {
                    box.Title = "Edit Communication Template";
                    box.Guid = communicationTemplate.Guid;

                    pushCommunication = new CommunicationDetails
                    {
                        PushData = communicationTemplate.PushData,
                        PushImageBinaryFileId = communicationTemplate.PushImageBinaryFileId,
                        PushMessage = communicationTemplate.PushMessage,
                        PushTitle = communicationTemplate.PushTitle,
                        PushOpenMessage = communicationTemplate.PushOpenMessage,
                        PushOpenMessageJson = communicationTemplate.PushOpenMessageJson,
                        PushOpenAction = communicationTemplate.PushOpenAction
                    };
                }
            }

            if ( communicationTemplate == null )
            {
                box.Title = "New Communication Template";
                communicationTemplate = new CommunicationTemplate();
                isNewTemplate = true;
            }

            box.IsNew = isNewTemplate;
            box.Name = communicationTemplate.Name;
            box.IsActive = communicationTemplate.IsActive;
            box.IsStarter = communicationTemplate.IsStarter;
            box.Description = communicationTemplate.Description;
            box.Category = communicationTemplate.Category.ToListItemBag();
            box.ImageFile = communicationTemplate.ImageFile.ToListItemBag();
            box.LogoBinaryFile = communicationTemplate.LogoBinaryFile.ToListItemBag();
            box.Version = isNewTemplate ? CommunicationTemplateVersion.Beta : communicationTemplate.Version;

            // Email Fields
            box.FromName = communicationTemplate.FromName;
            box.FromEmail = communicationTemplate.FromEmail;
            box.ReplyToEmail = communicationTemplate.ReplyToEmail;
            box.CcEmails = communicationTemplate.CCEmails;
            box.BccEmails = communicationTemplate.BCCEmails;
            box.IsCssInliningEnabled = communicationTemplate.CssInliningEnabled;
            box.LavaFields = communicationTemplate.LavaFields;
            box.Subject = communicationTemplate.Subject;
            box.Message = communicationTemplate.Message;
            box.Attachments = communicationTemplate.Attachments.Select( a => a.BinaryFile ).ToListItemBagList();
            box.AttachmentsBinaryFileTypeGuid = this.AttachmentsBinaryFileTypeGuidOrDefault;

            // SMS Fields
            box.SmsFromSystemPhoneNumber = communicationTemplate.SmsFromSystemPhoneNumber.ToListItemBag();
            box.SmsMessage = communicationTemplate.SMSMessage;
            box.SmsFromSystemPhoneNumbers =  SystemPhoneNumberCache.All( false )
                .Where( spn => spn.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .OrderBy( spn => spn.Order )
                .ThenBy( spn => spn.Name )
                .ThenBy( spn => spn.Id )
                .ToListItemBagList();

            // Push Fields
            var pushTransport = MediumContainer.GetActiveMediumComponentsWithActiveTransports().FirstOrDefault( m => m.TypeGuid == SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() )?.Transport;
            box.IsRockMobilePushTransportConfigured = pushTransport != null && pushTransport is IRockMobilePush;
            box.PushTitle = pushCommunication.PushTitle;
            box.PushMessage = pushCommunication.PushMessage;
            box.PushMobileApplications = SiteCache.GetAllActiveSites().Where( s => s.SiteType == SiteType.Mobile ).ToListItemBagList();

            if ( communicationTemplate.PushOpenAction != null )
            {
                box.PushOpenAction = ( PushOpenAction )( int )communicationTemplate.PushOpenAction.Value;
            }

            PushData pushData;
            if ( communicationTemplate.PushData.IsNotNullOrWhiteSpace() )
            {
                pushData = communicationTemplate.PushData.FromJsonOrNull<PushData>() ?? new PushData();
            }
            else
            {
                pushData = new PushData();
            }

            if ( communicationTemplate.PushOpenAction == Rock.Utility.PushOpenAction.ShowDetails )
            {
                if ( pushData.MobileApplicationId.HasValue )
                {
                    var mobileApplication = SiteCache.Get( pushData.MobileApplicationId.Value );

                    if ( mobileApplication != null )
                    {
                        box.PushMobileApplication = mobileApplication.ToListItemBag();
                    }
                }

                box.PushOpenMessageJson = communicationTemplate.PushOpenMessageJson;

                if ( communicationTemplate.PushOpenMessageJson.IsNotNullOrWhiteSpace() )
                {
                    // Ensure the HTML is up-to-date with the structured content.
                    box.PushOpenMessage = new StructuredContentHelper( communicationTemplate.PushOpenMessageJson ).Render();
                }
                else
                {
                    box.PushOpenMessage = communicationTemplate.PushOpenMessage;
                }
            }
            else if ( communicationTemplate.PushOpenAction == Rock.Utility.PushOpenAction.LinkToMobilePage )
            {
                if ( pushData.MobilePageId.HasValue )
                {
                    box.PushMobilePage = PageCache.GetGuid( pushData.MobilePageId.Value )?.ToString()?.ToPageRouteValueBag();
                }

                box.PushMobilePageQueryString = pushData.MobilePageQueryString;
            }
            else if ( communicationTemplate.PushOpenAction == Rock.Utility.PushOpenAction.LinkToUrl )
            {
                box.PushUrl = pushData.Url;
            }

            // Authorization
            if ( !isNewTemplate && !communicationTemplate.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                box.IsEditRestricted = true;
                box.IsEditRestrictedUnauthorized = true;
                box.IsReadOnly = true;
            }

            if ( communicationTemplate.IsSystem )
            {
                box.IsEditRestricted = true;
                box.IsEditRestrictedSystem = true;
            }

            return box;
        }

        #region Block Actions

        [BlockAction( "SaveTemplate" )]
        public BlockActionResult SaveTemplate( CommunicationTemplateDetailCommunicationTemplateBag bag )
        {
            if ( !bag.Validate( "Communication Template" ).IsNotNull( out var validationResult ) )
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }

            var currentPerson = GetCurrentPerson();
            var communicationTemplateService = new CommunicationTemplateService( this.RockContext );
            var communicationTemplateAttachmentService = new CommunicationTemplateAttachmentService( this.RockContext );
            var binaryFileService = new BinaryFileService( this.RockContext );

            CommunicationTemplate communicationTemplate = null;

            var communicationTemplateKey = this.CommunicationTemplateKeyPageParameter;

            if ( communicationTemplateKey.IsNotNullOrWhiteSpace() )
            {
                communicationTemplate = communicationTemplateService.GetQueryableByKey( communicationTemplateKey )
                    .Include( c => c.Attachments )
                    .FirstOrDefault();
            }

            var isNewTemplate = false;
            if ( communicationTemplate == null )
            {
                isNewTemplate = true;
                communicationTemplate = new CommunicationTemplate
                {
                    // Only set the version for new templates;
                    // existing template versions are read-only.
                    Version = bag.Version
                };
                communicationTemplateService.Add( communicationTemplate );
            }

            if ( !isNewTemplate && !communicationTemplate.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                return ActionForbidden( "You do not have permission to edit this communication template." );
            }

            // Allow these edits even if this is a system communication template.
            communicationTemplate.Description = bag.Description;
            communicationTemplate.LavaFields = bag.LavaFields;
            communicationTemplate.CssInliningEnabled = bag.IsCssInliningEnabled;

            var newImageFileGuid = bag.ImageFile?.Value?.AsGuidOrNull();
            if ( communicationTemplate.ImageFile?.Guid != newImageFileGuid )
            {
                if ( communicationTemplate.ImageFile != null )
                {
                    // The old image template preview won't be needed anymore, so make it IsTemporary and have it get cleaned up later.
                    communicationTemplate.ImageFile.IsTemporary = true;
                }

                if ( newImageFileGuid.HasValue )
                {
                    communicationTemplate.ImageFile = binaryFileService.Get( newImageFileGuid.Value );
                    communicationTemplate.ImageFileId = communicationTemplate.ImageFile?.Id;
                }
                else
                {
                    communicationTemplate.ImageFile = null;
                    communicationTemplate.ImageFileId = null;
                }
            }

            if ( communicationTemplate.ImageFile != null )
            {
                // Ensure that the ImagePreview is not set as IsTemporary=True so it does not get deleted,
                // whether it is a new image or an existing image.
                communicationTemplate.ImageFile.IsTemporary = false;
            }

            // Note: If the logo has changed, we can't get rid of it since existing communications might use it.
            var newLogoBinaryFileGuid = bag.LogoBinaryFile?.Value?.AsGuidOrNull();
            if ( communicationTemplate.LogoBinaryFile?.Guid != newLogoBinaryFileGuid )
            {
                if ( newLogoBinaryFileGuid.HasValue )
                {
                    communicationTemplate.LogoBinaryFile = binaryFileService.Get( newLogoBinaryFileGuid.Value );
                    communicationTemplate.LogoBinaryFileId = communicationTemplate.LogoBinaryFile?.Id;
                }
                else
                {
                    communicationTemplate.LogoBinaryFile = null;
                    communicationTemplate.LogoBinaryFileId = null;
                }
            }

            if ( communicationTemplate.LogoBinaryFile != null )
            {
                // Ensure that the LogoBinaryFile is not set as IsTemporary=True so it does not get deleted,
                // whether it is a new image or an existing image.
                communicationTemplate.LogoBinaryFile.IsTemporary = false;
            }

            var newCategoryGuid = bag.Category?.Value?.AsGuidOrNull();
            if ( communicationTemplate.Category?.Guid != newCategoryGuid )
            {
                if ( newCategoryGuid.HasValue )
                {
                    communicationTemplate.Category = new CategoryService( this.RockContext ).Get( newCategoryGuid.Value );
                    communicationTemplate.CategoryId = communicationTemplate.Category?.Id;
                }
                else
                {
                    communicationTemplate.Category = null;
                    communicationTemplate.CategoryId = null;
                }
            }

            // Only allow these edits if this is not a system communication.
            if ( !communicationTemplate.IsSystem )
            {
                communicationTemplate.Name = bag.Name;
                communicationTemplate.IsActive = bag.IsActive;
                communicationTemplate.IsStarter = bag.IsStarter;
                communicationTemplate.FromName = bag.FromName;
                communicationTemplate.FromEmail = bag.FromEmail;
                communicationTemplate.ReplyToEmail = bag.ReplyToEmail;
                communicationTemplate.CCEmails = bag.CcEmails;
                communicationTemplate.BCCEmails = bag.BccEmails;
                communicationTemplate.Subject = bag.Subject;
                communicationTemplate.Message = bag.Message;

                var binaryFileGuids = bag.Attachments?.Select( a => a.Value ).AsGuidList();
                var binaryFileIds = binaryFileService.Queryable()
                    .Where( b => binaryFileGuids.Contains( b.Guid ) )
                    .Select( b => b.Id )
                    .ToList();

                // Delete any attachments that are no longer included.
                foreach ( var attachment in communicationTemplate.Attachments
                    .Where( a => !binaryFileIds.Contains( a.BinaryFileId ) ).ToList() )
                {
                    communicationTemplate.Attachments.Remove( attachment );
                    communicationTemplateAttachmentService.Delete( attachment );
                }

                // Add any new attachments that were added.
                foreach ( var binaryFileId in binaryFileIds
                    .Where( a => communicationTemplate.Attachments.All( x => x.BinaryFileId != a ) ) )
                {
                    communicationTemplate.Attachments.Add( new CommunicationTemplateAttachment
                    {
                        BinaryFileId = binaryFileId,
                        CommunicationType = CommunicationType.Email
                    } );
                }
                
                var newSmsFromSystemPhoneNumberGuid = bag.SmsFromSystemPhoneNumber?.Value?.AsGuidOrNull();
                if ( communicationTemplate.SmsFromSystemPhoneNumber?.Guid != newSmsFromSystemPhoneNumberGuid )
                {
                    if ( newSmsFromSystemPhoneNumberGuid.HasValue )
                    {
                        communicationTemplate.SmsFromSystemPhoneNumber = new SystemPhoneNumberService( this.RockContext ).Get( newSmsFromSystemPhoneNumberGuid.Value );
                        communicationTemplate.SmsFromSystemPhoneNumberId = communicationTemplate.SmsFromSystemPhoneNumber?.Id;
                    }
                    else
                    {
                        communicationTemplate.SmsFromSystemPhoneNumber = null;
                        communicationTemplate.SmsFromSystemPhoneNumberId = null;
                    }
                }
                
                communicationTemplate.SMSMessage = bag.SmsMessage;
            }

            // No "Is System" push restrictions in the old block,
            // so we'll just allow edits if the user can edit the communication template.
            communicationTemplate.PushTitle = bag.PushTitle;
            communicationTemplate.PushMessage = bag.PushMessage;
            communicationTemplate.PushOpenAction = ( Rock.Utility.PushOpenAction ) ( int ) bag.PushOpenAction;

            var pushData = new PushData();
            var newPushOpenMessageJson = default( string );
            var newPushOpenMessage = default( string );

            if ( communicationTemplate.PushOpenAction == Rock.Utility.PushOpenAction.ShowDetails )
            {
                if ( bag.PushOpenMessageJson.IsNotNullOrWhiteSpace() )
                {
                    newPushOpenMessageJson = bag.PushOpenMessageJson;
                    newPushOpenMessage = new StructuredContentHelper( bag.PushOpenMessageJson ).Render();
                }
                else
                {
                    newPushOpenMessage = bag.PushOpenMessage;
                }

                var newMobileApplicationGuid = bag.PushMobileApplication?.Value?.AsGuidOrNull();
                if ( newMobileApplicationGuid.HasValue )
                {
                    pushData.MobileApplicationId = SiteCache.GetId( newMobileApplicationGuid.Value );
                }
                else
                {
                    pushData.MobileApplicationId = null;
                }
            }
            else if ( communicationTemplate.PushOpenAction == Rock.Utility.PushOpenAction.LinkToMobilePage )
            {
                pushData.MobilePageQueryString = bag.PushMobilePageQueryString;

                var newMobilePageGuid = bag.PushMobilePage?.Page?.Value.AsGuid();
                if ( newMobilePageGuid.HasValue )
                {
                    var newMobilePage = PageCache.Get( newMobilePageGuid.Value ); 
                    pushData.MobilePageId = newMobilePage?.Id;
                    pushData.MobileApplicationId = newMobilePage?.SiteId;
                }
                else
                {
                    pushData.MobilePageId = null;
                    pushData.MobileApplicationId = null;
                }
            }
            else if ( communicationTemplate.PushOpenAction == Rock.Utility.PushOpenAction.LinkToUrl )
            {
                pushData.Url = bag.PushUrl;
            }

            communicationTemplate.PushOpenMessage = newPushOpenMessage;
            communicationTemplate.PushOpenMessageJson = newPushOpenMessageJson;
            communicationTemplate.PushData = pushData.ToJson();

            // Validate entity before saving changes.
            if ( !communicationTemplate.IsValid )
            {
                var validationErrorMessage = communicationTemplate.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "\n" );
                return ActionBadRequest( validationErrorMessage );
            }

            this.RockContext.SaveChanges();

            if ( isNewTemplate )
            {
                // Read the saved communication template to ensure persisted properties are populated.
                communicationTemplate = communicationTemplateService.Get( communicationTemplate.Id );
                if ( communicationTemplate != null )
                {
                    if ( this.IsPersonalTemplatesView )
                    {
                        // If editing personal templates, make the new template private/personal to the logged-in person.
                        communicationTemplate.MakePrivate( Authorization.VIEW, currentPerson );
                        communicationTemplate.MakePrivate( Authorization.EDIT, currentPerson );
                        communicationTemplate.MakePrivate( Authorization.ADMINISTRATE, currentPerson );
                    }
                    else
                    {
                        // Otherwise, make sure logged-in person can view and edit the new template.
                        if ( !communicationTemplate.IsAuthorized( Authorization.VIEW, currentPerson ) )
                        {
                            communicationTemplate.AllowPerson( Authorization.VIEW, currentPerson );
                        }

                        // Make sure logged-in person can edit the new template.
                        if ( !communicationTemplate.IsAuthorized( Authorization.EDIT, currentPerson ) )
                        {
                            communicationTemplate.AllowPerson( Authorization.EDIT, currentPerson );
                        }
                    }

                    // Always make sure RSR-Admin and Communication Admin can see the new template.
                    var groupService = new GroupService( this.RockContext );
                    var communicationAdministrators = groupService.Get( SystemGuid.Group.GROUP_COMMUNICATION_ADMINISTRATORS.AsGuid() );
                    if ( communicationAdministrators != null )
                    {
                        communicationTemplate.AllowSecurityRole( Authorization.VIEW, communicationAdministrators, this.RockContext );
                        communicationTemplate.AllowSecurityRole( Authorization.EDIT, communicationAdministrators, this.RockContext );
                        communicationTemplate.AllowSecurityRole( Authorization.ADMINISTRATE, communicationAdministrators, this.RockContext );
                    }
                }
            }

            return ActionOk();
        }

        [BlockAction( "GetPreviewMessage" )]
        public BlockActionResult GetPreviewMessage( CommunicationTemplateDetailGetPreviewMessageRequestBag bag )
        {
            var mergeFields = this.RequestContext.GetCommonMergeFields();
            var resolvedPreviewHtml = bag.Message.ResolveMergeFields( mergeFields );

            if ( bag.IsCssInlined )
            {
                resolvedPreviewHtml = resolvedPreviewHtml.ConvertHtmlStylesToInlineAttributes();
            }

            return ActionOk( resolvedPreviewHtml );
        }

        #endregion Block Actions
    }
}