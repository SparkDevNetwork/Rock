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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;

using church.ccv.Utility.SystemGuids;

namespace church.ccv.Utility.Groups
{
    [DisplayName( "Toolbox Group Member Detail" )]
    [Category( "CCV > Groups" )]
    [Description( "Displays the details of the given Next Steps group member " )]
    public abstract class ToolboxGroupMemberDetail : RockBlock
    {
        #region Control Methods
        public abstract Literal MemberName { get; }
        public abstract Literal GroupRole { get; }

        public abstract Rock.Web.UI.Controls.RockRadioButtonList ActivePendingStatus { get; }
        public abstract Rock.Web.UI.Controls.EmailBox EmailAddress { get; }
        public abstract Rock.Web.UI.Controls.PhoneNumberBox MobileNumber { get; }
        public abstract CheckBox SMSEnabled { get; }
        public abstract Rock.Web.UI.Controls.RockDropDownList OptOutReason { get; }
        public abstract Rock.Web.UI.Controls.DatePicker FollowUpDatePicker { get; }
        public abstract Rock.Web.UI.Controls.RockTextBox ReassignReason { get; }
        public abstract Rock.Web.UI.Controls.ImageEditor ProfilePicEditor { get; }

        public abstract Literal EmailReceipt { get; }

        const int ProfilePicSizePixels = 200;

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
        }

        #endregion

        #region Edit Events
        
        /// <summary>
        /// Saves the group member.
        /// </summary>
        protected virtual void SaveGroupMember( GroupMember groupMember, RockContext rockContext )
        {
            // load existing group member
            if ( string.IsNullOrWhiteSpace( OptOutReason.SelectedValue ) == true )
            {
                groupMember.GroupMemberStatus = ActivePendingStatus.SelectedValueAsEnum<GroupMemberStatus>();
            }
            else
            {
                groupMember.GroupMemberStatus = GroupMemberStatus.Inactive;
            }

            // update their opt out reason
            groupMember.LoadAttributes();
            if ( string.IsNullOrWhiteSpace( OptOutReason.SelectedValue ) == false )
            {
                groupMember.SetAttributeValue( "OptOutReason", OptOutReason.SelectedValue );
                if ( FollowUpDatePicker.Visible && FollowUpDatePicker.SelectedDate.HasValue )
                {
                    groupMember.SetAttributeValue( "FollowUpDate", FollowUpDatePicker.SelectedDate.Value.ToString( "o" ) );
                }
                else
                {
                    groupMember.SetAttributeValue( "FollowUpDate", null );
                }
            }
            else
            {
                groupMember.SetAttributeValue( "OptOutReason", null );
                groupMember.SetAttributeValue( "FollowUpDate", null );
            }

           
            // update their email address
            if ( string.IsNullOrEmpty( EmailAddress.Text ) == true || EmailAddress.Text.IsValidEmail() )
            {
                groupMember.Person.Email = EmailAddress.Text;
            }

            // update their phone number
            var mobileNumberType = Rock.Web.Cache.DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) );
            groupMember.Person.UpdatePhoneNumber( mobileNumberType.Id, MobileNumber.CountryCode, MobileNumber.Text, SMSEnabled.Checked, null, rockContext );

            // save changes to their profile picture
            SaveProfilePicture( groupMember.Person, rockContext );
        }
        #endregion

        #region Internal Methods
                
        protected void ShowDetail( Person adminPerson, GroupMember groupMember, DefinedTypeCache optOutReasonList, RockContext rockContext )
        {
            // set the active/pending status
            ActivePendingStatus.Items.Clear();
            ActivePendingStatus.Items.Add( new ListItem( GroupMemberStatus.Active.ConvertToString(), GroupMemberStatus.Active.ConvertToInt().ToString() ) );
            ActivePendingStatus.Items.Add( new ListItem( GroupMemberStatus.Pending.ConvertToString(), GroupMemberStatus.Pending.ConvertToInt().ToString() ) );
            
            // get the person's value
            ActivePendingStatus.SetValue( groupMember.GroupMemberStatus.ConvertToInt() );


            // get the name / role
            MemberName.Text = groupMember.Person.ToString();
            GroupRole.Text = groupMember.GroupRole.ToString();

            // load their attribs
            groupMember.LoadAttributes();


            // populate the opt-out reasons
            foreach ( var definedValue in optOutReasonList.DefinedValues )
            {
                OptOutReason.Items.Add( new ListItem( definedValue.Value, definedValue.Guid.ToString( ).ToUpper( ) ) );
            }
            OptOutReason.Items.Insert( 0, string.Empty );
            OptOutReason.SelectedIndex = 0;

            // load the person's opt out reason
            var optOutReasonValue = groupMember.GetAttributeValue( "OptOutReason" );
            if ( optOutReasonValue != null )
            {
                OptOutReason.SetValue( optOutReasonValue );
            }
            else
            {
                OptOutReason.SetValue( string.Empty );
            }


            // get their email
            EmailAddress.Text = groupMember.Person.Email;


            // get their home and mobile phone numbers
            var mobileNumberType = Rock.Web.Cache.DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) );
            
            var mobilephone = groupMember.Person.PhoneNumbers.Where( p => p.NumberTypeValueId == mobileNumberType.Id ).FirstOrDefault();
            if ( mobilephone != null )
            {
                MobileNumber.Text = mobilephone.NumberFormatted;
                SMSEnabled.Checked = mobilephone.IsMessagingEnabled;
            }
            
            // set the follow up date
            var followUpDate = groupMember.GetAttributeValue( "FollowUpDate" );

            FollowUpDatePicker.SelectedDate = followUpDate.AsDateTime();

            // set the photo
            ProfilePicEditor.BinaryFileId = groupMember.Person.PhotoId;
            ProfilePicEditor.NoPictureUrl = Person.GetPersonPhotoUrl( groupMember.Person, ProfilePicSizePixels, ProfilePicSizePixels);
        }

        protected void SendEmail( Dictionary<string, object> mergeObjects, Guid emailTemplateGuid, RockContext rockContext )
        {
            // we need to send off a communcation email. 
            EmailReceipt.Text = GetAttributeValue( "ReceiptText" ).ResolveMergeFields( mergeObjects );

            // Resolve any dynamic url references
            string appRoot = ResolveRockUrl( "~/" );
            string themeRoot = ResolveRockUrl( "~~/" );
            EmailReceipt.Text = EmailReceipt.Text.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );

            // show liquid help for debug
            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
            {
                EmailReceipt.Text += mergeObjects.lavaDebugInfo();
            }
            EmailReceipt.Visible = true;
            
            // get the email service and send the email
            SystemEmailService emailService = new SystemEmailService( rockContext );
            SystemEmail reassignEmail = emailService.Get( emailTemplateGuid );

            // build a recipient list using the "To" from the system email
            var recipients = new List<Rock.Communication.RecipientData>();

            // add person and the mergeObjects (same mergeobjects as receipt)
            recipients.Add( new Rock.Communication.RecipientData( reassignEmail.To, mergeObjects ) );

            Rock.Communication.Email.Send( emailTemplateGuid, recipients, ResolveRockUrl( "~/" ), ResolveRockUrl( "~~/" ) );
        }

        protected void SaveProfilePicture( Person person, RockContext rockContext )
        {
            // set their picture
            int? orphanedPhotoId = null;
            if ( person.PhotoId != ProfilePicEditor.BinaryFileId )
            {
                orphanedPhotoId = person.PhotoId;
                person.PhotoId = ProfilePicEditor.BinaryFileId;
            }

            // save so we commit the new picture
            rockContext.SaveChanges( );


            // now clean up any old pictures.

            // if their picture is being replaced, clean up the "old", orphaned pic.
            if ( orphanedPhotoId.HasValue )
            {
                BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                var binaryFile = binaryFileService.Get( orphanedPhotoId.Value );
                if ( binaryFile != null )
                {
                    string errorMessage;
                    if ( binaryFileService.CanDelete( binaryFile, out errorMessage ) )
                    {
                        binaryFileService.Delete( binaryFile );
                    }
                }
            }

            // if they used the ImageEditor, and cropped it, the uncropped file is still in BinaryFile. So clean it up
            if ( ProfilePicEditor.CropBinaryFileId.HasValue )
            {
                if ( ProfilePicEditor.CropBinaryFileId != person.PhotoId )
                {
                    BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                    var binaryFile = binaryFileService.Get( ProfilePicEditor.CropBinaryFileId.Value );
                    if ( binaryFile != null && binaryFile.IsTemporary )
                    {
                        string errorMessage;
                        if ( binaryFileService.CanDelete( binaryFile, out errorMessage ) )
                        {
                            binaryFileService.Delete( binaryFile );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Starts the workflow if one was defined in the block setting.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        protected void StartWorkflow( string workflowName, object entity, Dictionary<string, string> attributes, RockContext rockContext )
        {
            WorkflowType workflowType = null;
            Guid? workflowTypeGuid = GetAttributeValue( workflowName ).AsGuidOrNull();
            if ( workflowTypeGuid.HasValue )
            {
                var workflowTypeService = new WorkflowTypeService( rockContext );
                workflowType = workflowTypeService.Get( workflowTypeGuid.Value );
                if ( workflowType != null )
                {
                    try
                    {
                        var workflow = Rock.Model.Workflow.Activate( workflowType, workflowName );

                        // set optional attributes for the workflow
                        if( attributes != null )
                        {
                            foreach ( KeyValuePair<string, string> kvp in attributes )
                            {
                                workflow.SetAttributeValue( kvp.Key, kvp.Value );
                            }
                        }

                        List<string> workflowErrors;
                        new WorkflowService( rockContext ).Process( workflow, entity, out workflowErrors );
                    }
                    catch ( Exception ex )

                    {
                        ExceptionLogService.LogException( ex, this.Context );
                    }
                }
            }
        }
        
        #endregion
    }
}