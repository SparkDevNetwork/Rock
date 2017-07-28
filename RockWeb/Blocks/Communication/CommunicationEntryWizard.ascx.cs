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
using System.Data.Entity;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Text;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Communication Entry Wizard" )]
    [Category( "Communication" )]
    [Description( "Used for creating and sending a new communications such as email, SMS, etc. to recipients." )]

    [BinaryFileTypeField( "Binary File Type", "The FileType to use for images that are added to the email using the image component", true, Rock.SystemGuid.BinaryFiletype.DEFAULT )]
    [IntegerField( "Character Limit", "Set this to show a character limit countdown for SMS communications. Set to 0 to disable", false, 160 )]
    public partial class CommunicationEntryWizard : Rock.Web.UI.RockBlock
    {
        #region fields


        #endregion

        /// <summary>
        /// Gets or sets the individual recipient person ids.
        /// </summary>
        /// <value>
        /// The individual recipient person ids.
        /// </value>
        protected List<int> IndividualRecipientPersonIds
        {
            get
            {
                var recipients = ViewState["IndividualRecipientPersonIds"] as List<int>;
                if ( recipients == null )
                {
                    recipients = new List<int>();
                    ViewState["IndividualRecipientPersonIds"] = recipients;
                }
                return recipients;
            }

            set { ViewState["IndividualRecipientPersonIds"] = value; }
        }

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            componentImageUploader.BinaryFileTypeGuid = this.GetAttributeValue( "BinaryFileType" ).AsGuidOrNull() ?? Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid();
            hfSMSCharLimit.Value = ( this.GetAttributeValue( "CharacterLimit" ).AsIntegerOrNull() ?? 160 ).ToString();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                LoadDropDowns();

                tglRecipientSelection_CheckedChanged( null, null );

                // DEBUG
                ddlCommunicationGroupList.Items[1].Selected = true;
                tbCommunicationName.Text = "DEBUG";
                dtpSendDateTime.SelectedDateTime = RockDateTime.Now;
            }
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            var rockContext = new RockContext();

            // load communication group list
            var groupTypeCommunicationGroupId = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST.AsGuid() ).Id;
            var groupService = new GroupService( rockContext );

            var communicationGroupList = groupService.Queryable().Where( a => a.GroupTypeId == groupTypeCommunicationGroupId ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            var authorizedCommunicationGroupList = communicationGroupList.Where( g => g.IsAuthorized( Rock.Security.Authorization.VIEW, this.CurrentPerson ) ).ToList();

            ddlCommunicationGroupList.Items.Clear();
            ddlCommunicationGroupList.Items.Add( new ListItem() );
            foreach ( var communicationGroup in authorizedCommunicationGroupList )
            {
                ddlCommunicationGroupList.Items.Add( new ListItem( communicationGroup.Name, communicationGroup.Id.ToString() ) );
            }

            if ( !authorizedCommunicationGroupList.Any() )
            {
                nbCommunicationGroupWarning.Visible = true;
                if ( !communicationGroupList.Any() )
                {
                    // there are no communication groups in the system, so explain how to add them
                    nbCommunicationGroupWarning.Text = "No communications lists. Go to Home / Communications / Communication Lists to add a communication list.";
                }
                else
                {
                    // there are communication groups in the system, but the user isn't authorized to see any of them
                    nbCommunicationGroupWarning.Text = "No communications lists available.";
                }
            }
            else
            {
                nbCommunicationGroupWarning.Visible = false;
            }

            LoadCommunicationSegmentFilters();

            rblCommunicationGroupSegmentFilterType.Items.Clear();
            rblCommunicationGroupSegmentFilterType.Items.Add( new ListItem( "All segment filters", FilterExpressionType.GroupAll.ToString() ) { Selected = true } );
            rblCommunicationGroupSegmentFilterType.Items.Add( new ListItem( "Any segment filters", FilterExpressionType.GroupAny.ToString() ) );

            UpdateRecipientFromListCount();

            BindTemplatePicker();

            btnMediumUserPreference.Attributes["data-val"] = Rock.Model.CommunicationType.UserPreference.ConvertToInt().ToString();
            btnMediumEmail.Attributes["data-val"] = Rock.Model.CommunicationType.Email.ConvertToInt().ToString();
            btnMediumSMS.Attributes["data-val"] = Rock.Model.CommunicationType.SMS.ConvertToInt().ToString();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            // TODO
        }

        #endregion

        #region Recipient Selection

        /// <summary>
        /// Loads the common communication segment filters along with any additional filters that are defined for the selected communication list
        /// </summary>
        private void LoadCommunicationSegmentFilters()
        {
            var rockContext = new RockContext();

            // load common communication segments (each communication list may have additional segments)
            var dataviewService = new DataViewService( rockContext );
            var categoryIdCommunicationSegments = CategoryCache.Read( Rock.SystemGuid.Category.DATAVIEW_COMMUNICATION_SEGMENTS.AsGuid() ).Id;
            var commonSegmentDataViewList = dataviewService.Queryable().Where( a => a.CategoryId == categoryIdCommunicationSegments ).OrderBy( a => a.Name ).ToList();

            cblCommunicationGroupSegments.Items.Clear();
            foreach ( var commonSegmentDataView in commonSegmentDataViewList )
            {
                if ( commonSegmentDataView.IsAuthorized( Rock.Security.Authorization.VIEW, this.CurrentPerson ) )
                {
                    cblCommunicationGroupSegments.Items.Add( new ListItem( commonSegmentDataView.Name, commonSegmentDataView.Id.ToString() ) );
                }
            }

            pnlCommunicationGroupSegments.Visible = cblCommunicationGroupSegments.Items.Count > 0;
        }

        /// <summary>
        /// Handles the Click event of the btnRecipientSelectionNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRecipientSelectionNext_Click( object sender, EventArgs e )
        {
            pnlRecipientSelection.Visible = false;
            pnlMediumSelection.Visible = true;
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglRecipientSelection control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglRecipientSelection_CheckedChanged( object sender, EventArgs e )
        {
            pnlRecipientSelectionList.Visible = tglRecipientSelection.Checked;
            pnlRecipientSelectionIndividual.Visible = !tglRecipientSelection.Checked;
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppAddPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppAddPerson_SelectPerson( object sender, EventArgs e )
        {
            if ( ppAddPerson.PersonId.HasValue )
            {
                if ( !IndividualRecipientPersonIds.Contains( ppAddPerson.PersonId.Value ) )
                {
                    IndividualRecipientPersonIds.Add( ppAddPerson.PersonId.Value );
                }
            }

            var individualRecipientCount = this.IndividualRecipientPersonIds.Count();
            lIndividualRecipientCount.Text = string.Format( "{0} {1} selected", individualRecipientCount, "recipient".PluralizeIf( individualRecipientCount != 1 ) );
        }

        /// <summary>
        /// Handles the Click event of the btnViewIndividualRecipients control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnViewIndividualRecipients_Click( object sender, EventArgs e )
        {
            // TODO
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblCommunicationGroupSegments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblCommunicationGroupSegments_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateRecipientFromListCount();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlCommunicationGroupList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlCommunicationGroupList_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateRecipientFromListCount();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblCommunicationGroupSegmentFilterType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblCommunicationGroupSegmentFilterType_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateRecipientFromListCount();
        }

        /// <summary>
        /// Updates the recipient from list count.
        /// </summary>
        private void UpdateRecipientFromListCount()
        {
            IQueryable<GroupMember> groupMemberQuery = null;

            groupMemberQuery = GetRecipientFromListSelection( groupMemberQuery );

            if ( groupMemberQuery != null )
            {
                int groupMemberCount = groupMemberQuery.Count();
                lRecipientFromListCount.Visible = true;

                lRecipientFromListCount.Text = string.Format( "{0} {1} selected", groupMemberCount, "recipient".PluralizeIf( groupMemberCount != 1 ) );
            }
            else
            {
                lRecipientFromListCount.Visible = false;
            }
        }

        /// <summary>
        /// Gets the GroupMember Query for the recipients selected on the 'Select From List' tab
        /// </summary>
        /// <param name="groupMemberQuery">The group member query.</param>
        /// <returns></returns>
        private IQueryable<GroupMember> GetRecipientFromListSelection( IQueryable<GroupMember> groupMemberQuery )
        {
            var rockContext = new RockContext();
            var groupMemberService = new GroupMemberService( rockContext );
            var personService = new PersonService( rockContext );
            var dataViewService = new DataViewService( rockContext );
            int? communicationGroupId = ddlCommunicationGroupList.SelectedValue.AsIntegerOrNull();
            if ( communicationGroupId.HasValue )
            {
                groupMemberQuery = groupMemberService.Queryable().Where( a => a.GroupId == communicationGroupId.Value && a.GroupMemberStatus == GroupMemberStatus.Active );

                var segmentFilterType = rblCommunicationGroupSegmentFilterType.SelectedValueAsEnum<FilterExpressionType>();
                var segmentDataViewIds = cblCommunicationGroupSegments.Items.OfType<ListItem>().Where( a => a.Selected ).Select( a => a.Value.AsInteger() ).ToList();

                Expression segmentExpression = null;
                ParameterExpression paramExpression = personService.ParameterExpression;
                var segmentDataViewList = dataViewService.GetByIds( segmentDataViewIds ).AsNoTracking().ToList();
                foreach ( var segmentDataView in segmentDataViewList )
                {
                    List<string> errorMessages;

                    var exp = segmentDataView.GetExpression( personService, paramExpression, out errorMessages );
                    if ( exp != null )
                    {
                        if ( segmentExpression == null )
                        {
                            segmentExpression = exp;
                        }
                        else
                        {

                            if ( segmentFilterType == FilterExpressionType.GroupAll )
                            {
                                segmentExpression = Expression.AndAlso( segmentExpression, exp );
                            }
                            else
                            {
                                segmentExpression = Expression.OrElse( segmentExpression, exp );
                            }
                        }
                    }
                }

                if ( segmentExpression != null )
                {
                    var personQry = personService.Get( paramExpression, segmentExpression );
                    groupMemberQuery = groupMemberQuery.Where( a => personQry.Any( p => p.Id == a.PersonId ) );
                }
            }

            return groupMemberQuery;
        }

        #endregion Recipient Selection

        #region Medium Selection

        /// <summary>
        /// Handles the Click event of the btnMediumSelectionPrevious control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMediumSelectionPrevious_Click( object sender, EventArgs e )
        {
            pnlMediumSelection.Visible = false;
            pnlRecipientSelection.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnMediumSelectionNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMediumSelectionNext_Click( object sender, EventArgs e )
        {
            if ( dtpSendDateTime.Visible )
            {
                if ( !dtpSendDateTime.SelectedDateTime.HasValue )
                {
                    nbSendDateTimeWarning.NotificationBoxType = NotificationBoxType.Danger;
                    nbSendDateTimeWarning.Text = "Send Date Time is required";
                    nbSendDateTimeWarning.Visible = true;
                    return;
                }
                else
                {
                    // TODO: In case we want to check for Date too far into the past or too far in the future
                }
            }

            nbSendDateTimeWarning.Visible = false;

            pnlMediumSelection.Visible = false;
            pnlTemplateSelection.Visible = true;

        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglSendDateTime control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglSendDateTime_CheckedChanged( object sender, EventArgs e )
        {
            dtpSendDateTime.Visible = !tglSendDateTime.Checked;
        }

        #endregion Medium Selection

        #region Template Selection


        /// <summary>
        /// Binds the template picker.
        /// </summary>
        private void BindTemplatePicker()
        {
            var rockContext = new RockContext();

            // TODO: Limit to 'non-legacy' templates
            var templateQuery = new CommunicationTemplateService( rockContext ).Queryable().OrderBy( a => a.Name );

            // get list of templates that the current user is authorized to View
            var templateList = templateQuery.AsNoTracking().ToList().Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, this.CurrentPerson ) ).ToList();

            rptSelectTemplate.DataSource = templateList;
            rptSelectTemplate.DataBind();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptSelectTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptSelectTemplate_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            // TODO
            CommunicationTemplate communicationTemplate = e.Item.DataItem as CommunicationTemplate;

            if ( communicationTemplate != null )
            {
                Literal lTemplateImagePreview = e.Item.FindControl( "lTemplateImagePreview" ) as Literal;
                Literal lTemplateName = e.Item.FindControl( "lTemplateName" ) as Literal;
                Literal lTemplateDescription = e.Item.FindControl( "lTemplateDescription" ) as Literal;
                LinkButton btnSelectTemplate = e.Item.FindControl( "btnSelectTemplate" ) as LinkButton;

                lTemplateImagePreview.Text = this.GetImageTag( communicationTemplate.ImageFileId );
                lTemplateName.Text = communicationTemplate.Name;
                lTemplateDescription.Text = communicationTemplate.Description;
                btnSelectTemplate.CommandName = "CommunicationTemplateId";
                btnSelectTemplate.CommandArgument = communicationTemplate.Id.ToString();
            }

        }

        /// <summary>
        /// Handles the Click event of the btnSelectTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSelectTemplate_Click( object sender, EventArgs e )
        {
            hfSelectedCommunicationTemplateId.Value = ( sender as LinkButton ).CommandArgument;
            pnlTemplateSelection.Visible = false;

            var communicationTemplate = new CommunicationTemplateService( new RockContext() ).Get( hfSelectedCommunicationTemplateId.Value.AsInteger() );

            string templateHtml;
            if ( string.IsNullOrEmpty(communicationTemplate.Message) )
            {
                // TODO, shouldn't need to do this after CommunicationTemplateDetails block is fixed up
                templateHtml = communicationTemplate.MediumData["HtmlMessage"];
            }
            else
            {
                templateHtml = communicationTemplate.Message;
            }
            
            ifEmailDesigner.Attributes["srcdoc"] = templateHtml;

            Rock.Model.CommunicationType communicationType = ( Rock.Model.CommunicationType ) hfMediumType.Value.AsInteger();
            if ( communicationType == CommunicationType.Email || communicationType == CommunicationType.UserPreference )
            {
                pnlEmailSummary.Visible = true;
            }
            else if (communicationType == CommunicationType.SMS)
            {
                pnlMobileTextEditor.Visible = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnTemplateSelectionPrevious control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnTemplateSelectionPrevious_Click( object sender, EventArgs e )
        {
            pnlTemplateSelection.Visible = false;
            pnlMediumSelection.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnTemplateSelectionNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnTemplateSelectionNext_Click( object sender, EventArgs e )
        {
            //pnlTemplateSelection.Visible = false;

            // TODO
            //pnlEmailEditor.Visible = true;
        }

        #endregion Template Selection

        #region Email Editor

        /// <summary>
        /// Handles the Click event of the btnEmailEditorPrevious control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEmailEditorPrevious_Click( object sender, EventArgs e )
        {
            pnlEmailEditor.Visible = false;
            Rock.Model.CommunicationType communicationType = ( Rock.Model.CommunicationType ) hfMediumType.Value.AsInteger();
            if ( communicationType == CommunicationType.SMS || communicationType == CommunicationType.UserPreference )
            {
                pnlMobileTextEditor.Visible = true;
            }
            else
            {
                pnlEmailSummary.Visible = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnEmailEditorNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEmailEditorNext_Click( object sender, EventArgs e )
        {
            pnlEmailEditor.Visible = false;
            pnlConfirmation.Visible = true;
        }

        #endregion Email Editor

        #region Email Summary

        /// <summary>
        /// Handles the Click event of the btnEmailSummaryPrevious control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEmailSummaryPrevious_Click( object sender, EventArgs e )
        {
            pnlEmailEditor.Visible = true;
            pnlEmailSummary.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the btnEmailSummaryNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEmailSummaryNext_Click( object sender, EventArgs e )
        {
            pnlEmailSummary.Visible = false;
            
            Rock.Model.CommunicationType communicationType = ( Rock.Model.CommunicationType ) hfMediumType.Value.AsInteger();
            if ( communicationType == CommunicationType.SMS || communicationType == CommunicationType.UserPreference )
            {
                // TODO: is this the right person and the right time to initialize?
                InitializeSMSFromSender( this.CurrentPerson );
                pnlMobileTextEditor.Visible = true;

                // TODO: Init Merge Field Picker?
            }
            else if ( communicationType == CommunicationType.Email )
            {
                pnlEmailEditor.Visible = true;
            }
        }

        /// <summary>
        /// Handles the FileUploaded event of the fupAttachments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FileUploaderEventArgs"/> instance containing the event data.</param>
        protected void fupAttachments_FileUploaded( object sender, FileUploaderEventArgs e )
        {
            List<int> attachmentList = hfAttachedBinaryFileIds.Value.SplitDelimitedValues().AsIntegerList();
            if ( fupAttachments.BinaryFileId.HasValue )
            {
                attachmentList.Add( fupAttachments.BinaryFileId.Value );
            }

            hfAttachedBinaryFileIds.Value = attachmentList.AsDelimited( "," );
            fupAttachments.BinaryFileId = null;

            // pre-populate dictionary so that the attachments are listed in the order they were added
            Dictionary<int, string> binaryFileAttachments = attachmentList.ToDictionary( k => k, v => string.Empty);
            using ( var rockContext = new RockContext() )
            {
                var binaryFileInfoList = new BinaryFileService( rockContext ).Queryable()
                        .Where( f => attachmentList.Contains( f.Id ) )
                        .Select( f => new
                        {
                            f.Id,
                            f.FileName
                        } );

                foreach ( var binaryFileInfo in binaryFileInfoList )
                {
                    binaryFileAttachments[binaryFileInfo.Id] = binaryFileInfo.FileName;
                }
            }

            StringBuilder sbAttachmentsHtml = new StringBuilder();
            sbAttachmentsHtml.AppendLine( "<div class='attachment'>" );
            sbAttachmentsHtml.AppendLine( "  <ul class='attachment-content'>" );

            foreach (var binaryFileAttachment in binaryFileAttachments )
            {
                var attachmentUrl = string.Format( "{0}GetFile.ashx?id={1}", System.Web.VirtualPathUtility.ToAbsolute( "~" ), binaryFileAttachment.Key );
                var removeAttachmentJS = string.Format( "removeAttachment( this, '{0}', '{1}' );", hfAttachedBinaryFileIds.ClientID, binaryFileAttachment.Key );
                sbAttachmentsHtml.AppendLine( string.Format( "    <li><a href='{0}' target='_blank'>{1}</a> <a><i class='fa fa-times' onclick=\"{2}\"></i></a></li>", attachmentUrl, binaryFileAttachment.Value, removeAttachmentJS ) );
            }
            
            sbAttachmentsHtml.AppendLine( "  </ul>" );
            sbAttachmentsHtml.AppendLine( "</div>" );

            lAttachmentListHtml.Text = sbAttachmentsHtml.ToString();
        }

        #endregion Email Summary

        #region Mobile Text Editor

        /// <summary>
        /// Handles the Click event of the btnMobileTextEditorPrevious control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMobileTextEditorPrevious_Click( object sender, EventArgs e )
        {
            pnlMobileTextEditor.Visible = false;
            pnlEmailSummary.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnMobileTextEditorNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMobileTextEditorNext_Click( object sender, EventArgs e )
        {
            pnlMobileTextEditor.Visible = false;
            Rock.Model.CommunicationType communicationType = ( Rock.Model.CommunicationType ) hfMediumType.Value.AsInteger();
            if ( communicationType == CommunicationType.Email || communicationType == CommunicationType.UserPreference )
            {
                pnlEmailEditor.Visible = true;
            }
            else
            {
                pnlConfirmation.Visible = true;
            }
        }

        /// <summary>
        /// Initializes the SMS from sender.
        /// </summary>
        /// <param name="sender">The sender.</param>
        public void InitializeSMSFromSender( Person sender )
        {
            ddlSMSFrom.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM ) ), false, true );
            var numbers = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM.AsGuid() );
            if ( numbers != null )
            {
                foreach ( var number in numbers.DefinedValues )
                {
                    var personAliasGuid = number.GetAttributeValue( "ResponseRecipient" ).AsGuidOrNull();
                    if ( personAliasGuid.HasValue && sender.Aliases.Any( a => a.Guid == personAliasGuid.Value ) )
                    {
                        ddlSMSFrom.SetValue( number.Id );
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the SelectItem event of the mfpMessage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mfpMessage_SelectItem( object sender, EventArgs e )
        {
            tbSMSTextMessage.Text += mfpSMSMessage.SelectedMergeField;
            mfpSMSMessage.SetValue( string.Empty );
        }

        /// <summary>
        /// Handles the Click event of the btnSMSSendTest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSMSSendTest_Click( object sender, EventArgs e )
        {

        }

        #endregion Mobile Text Editor

        #region Confirmation

        protected void btnConfirmationPrevious_Click( object sender, EventArgs e )
        {

        }

        protected void btnSend_Click( object sender, EventArgs e )
        {

        }

        protected void btnSaveAsDraft_Click( object sender, EventArgs e )
        {

        }

        protected void btnConfirmationCancel_Click( object sender, EventArgs e )
        {

        }

        protected void tglSendDateTimeConfirmation_CheckedChanged( object sender, EventArgs e )
        {

        }

        #endregion

        #region Methods

        #endregion

        // remove before flight
        string sampleTemplate = @"<!DOCTYPE html>
<html>
<head>
<title>A Responsive Email Template</title>

<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
<meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
<style type=""text/css"">
    /* CLIENT-SPECIFIC STYLES */
    body, table, td, a{-webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%;} /* Prevent WebKit and Windows mobile changing default text sizes */
    table, td{mso-table-lspace: 0pt; mso-table-rspace: 0pt;} /* Remove spacing between tables in Outlook 2007 and up */
    img{-ms-interpolation-mode: bicubic;} /* Allow smoother rendering of resized image in Internet Explorer */

    /* RESET STYLES */
    img{border: 0; height: auto; line-height: 100%; outline: none; text-decoration: none;}
    table{border-collapse: collapse !important;}
    body{height: 100% !important; margin: 0 !important; padding: 0 !important; width: 100% !important;}

    /* iOS BLUE LINKS */
    a[x-apple-data-detectors] {
        color: inherit !important;
        text-decoration: none !important;
        font-size: inherit !important;
        font-family: inherit !important;
        font-weight: inherit !important;
        line-height: inherit !important;
    }

    /* MOBILE STYLES */
    @media screen and (max-width: 525px) {

        /* ALLOWS FOR FLUID TABLES */
        .wrapper {
          width: 100% !important;
        	max-width: 100% !important;
        }

        /* ADJUSTS LAYOUT OF LOGO IMAGE */
        .logo img {
          margin: 0 auto !important;
        }

        /* USE THESE CLASSES TO HIDE CONTENT ON MOBILE */
        .mobile-hide {
          display: none !important;
        }

        .img-max {
          max-width: 100% !important;
          width: 100% !important;
          height: auto !important;
        }

        /* FULL-WIDTH TABLES */
        .responsive-table {
          width: 100% !important;
        }

        /* UTILITY CLASSES FOR ADJUSTING PADDING ON MOBILE */
        .padding {
          padding: 10px 5% 15px 5% !important;
        }

        .padding-meta {
          padding: 30px 5% 0px 5% !important;
          text-align: center;
        }

        .padding-copy {
     		padding: 10px 5% 10px 5% !important;
          text-align: center;
        }

        .no-padding {
          padding: 0 !important;
        }

        .section-padding {
          padding: 50px 15px 50px 15px !important;
        }

        /* ADJUST BUTTONS ON MOBILE */
        .mobile-button-container {
            margin: 0 auto;
            width: 100% !important;
        }

        .mobile-button {
            padding: 15px !important;
            border: 0 !important;
            font-size: 16px !important;
            display: block !important;
        }

    }

    /* ANDROID CENTER FIX */
    div[style*=""margin: 16px 0;""] { margin: 0 !important; }
</style>
<!--[if gte mso 12]>
<style type=""text/css"">
.mso-right {
	padding-left: 20px;
}
</style>
<![endif]-->
</head>
<body style=""margin: 0 !important; padding: 0 !important;"">

<!-- HIDDEN PREHEADER TEXT -->
<div style=""display: none; font-size: 1px; color: #fefefe; line-height: 1px; font-family: Helvetica, Arial, sans-serif; max-height: 0px; max-width: 0px; opacity: 0; overflow: hidden;"">
    Entice the open with some amazing preheader text. Use a little mystery and get those subscribers to read through...
</div>

<!-- HEADER -->
<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
    <tr>
        <td bgcolor=""#333333"" align=""center"">
            <!--[if (gte mso 9)|(IE)]>
            <table align=""center"" border=""0"" cellspacing=""0"" cellpadding=""0"" width=""500"">
            <tr>
            <td align=""center"" valign=""top"" width=""500"">
            <![endif]-->
            <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 500px;"" class=""wrapper"">
                <tr>
                    <td align=""center"" valign=""top"" style=""padding: 15px 0;"" class=""logo"">
                        <a href=""http://litmus.com"" target=""_blank"">
                            <img alt=""Logo"" src=""http://www.minecartstudio.com/Content/Misc/logo-1.jpg"" width=""60"" height=""60"" style=""display: block; font-family: Helvetica, Arial, sans-serif; color: #ffffff; font-size: 16px;"" border=""0"">
                        </a>
                    </td>
                </tr>
            </table>
            <!--[if (gte mso 9)|(IE)]>
            </td>
            </tr>
            </table>
            <![endif]-->
        </td>
    </tr>
    <tr>
        <td bgcolor=""#D8F1FF"" align=""center"" style=""padding: 70px 15px 70px 15px;"" class=""section-padding"">
            <!--[if (gte mso 9)|(IE)]>
            <table align=""center"" border=""0"" cellspacing=""0"" cellpadding=""0"" width=""500"">
            <tr>
            <td align=""center"" valign=""top"" width=""500"">
            <![endif]-->
            <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 500px;"" class=""responsive-table"">
                <tr>
                    <td style=""font-size: 25px; font-family: Helvetica, Arial, sans-serif; color: #333333; padding-top: 30px;"">
					    <div class=""structure-dropzone"">
					
                            <div class=""dropzone"">

							    <div class=""component component-text"" data-content=""<h1>Hello There!</h1>"" data-state=""component"">
								    <h1>Hello There!</h1>
							    </div>

						    </div>
                        </div>
					
					</td>
                </tr>
            </table>
            <!--[if (gte mso 9)|(IE)]>
            </td>
            </tr>
            </table>
            <![endif]-->
        </td>
    </tr>
    <tr>
        <td bgcolor=""#ffffff"" align=""center"" style=""padding: 70px 15px 25px 15px;"" class=""section-padding"">
            <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""500"" style=""padding:0 0 20px 0;"" class=""responsive-table"">
                <tr>
                    <td align=""center"" height=""100%"" valign=""top"" width=""100%"" style=""padding-bottom: 35px;"">
                        <!--[if (gte mso 9)|(IE)]>
                        <table align=""center"" border=""0"" cellspacing=""0"" cellpadding=""0"" width=""500"">
                        <tr>
                        <td align=""center"" valign=""top"" width=""500"">
                        <![endif]-->
                        <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width:500;"">
                            <tr>
                                <td align=""center"" valign=""top"">
                                    <!--[if (gte mso 9)|(IE)]>
                                    <table align=""center"" border=""0"" cellspacing=""0"" cellpadding=""0"" width=""500"">
                                    <tr>
                                    <td align=""left"" valign=""top"" width=""150"">
                                    <![endif]-->
                                    
                                    <div class=""structure-dropzone"">
									
									<div class=""dropzone""></div>

                                    </div>

                                </td>
                            </tr>
                        </table>
                        <!--[if (gte mso 9)|(IE)]>
                        </td>
                        </tr>
                        </table>
                        <![endif]-->
                    </td>
                </tr>
                
            </table>
        </td>
    </tr>
    
        
    <tr>
        <td bgcolor=""#ffffff"" align=""center"" style=""padding: 20px 0px;"">
            <!--[if (gte mso 9)|(IE)]>
            <table align=""center"" border=""0"" cellspacing=""0"" cellpadding=""0"" width=""500"">
            <tr>
            <td align=""center"" valign=""top"" width=""500"">
            <![endif]-->
            <!-- UNSUBSCRIBE COPY -->
            <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"" style=""max-width: 500px;"" class=""responsive-table"">
                <tr>
                    <td align=""center"" style=""font-size: 12px; line-height: 18px; font-family: Helvetica, Arial, sans-serif; color:#666666;"">
                        1234 Main Street, Anywhere, MA 01234, USA
                        <br>
                        <a href=""http://litmus.com"" target=""_blank"" style=""color: #666666; text-decoration: none;"">Unsubscribe</a>
                        <span style=""font-family: Arial, sans-serif; font-size: 12px; color: #444444;"">&nbsp;&nbsp;|&nbsp;&nbsp;</span>
                        <a href=""http://litmus.com"" target=""_blank"" style=""color: #666666; text-decoration: none;"">View this email in your browser</a>
                    </td>
                </tr>
            </table>
            <!--[if (gte mso 9)|(IE)]>
            </td>
            </tr>
            </table>
            <![endif]-->
        </td>
    </tr>
</table>
</body>
</html>
";












       
    }
}