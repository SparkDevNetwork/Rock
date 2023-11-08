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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn.Config
{
    [DisplayName( "Check-in Type Detail" )]
    [Category( "Check-in > Configuration" )]
    [Description( "Displays the details of a particular Check-in Type." )]

    [LinkedPage( "Schedule Page", "Page used to manage schedules for the check-in type." )]
    [Rock.SystemGuid.BlockTypeGuid( "6CB1416A-3B25-41FD-8E60-1B94F4A64AE6" )]
    public partial class CheckinTypeDetail : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            btnDelete.Attributes["onclick"] = "javascript: return Rock.dialogs.confirmDelete(event, 'Check-in Configuration');";
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
                string groupTypeId = PageParameter( "CheckinTypeId" );

                if ( !string.IsNullOrWhiteSpace( groupTypeId ) )
                {
                    wpGeneral.Expanded = true;
                    LoadDropdowns();
                    ShowDetail( groupTypeId.AsInteger() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
            else
            {
                // Rebuild the attribute controls on postback based on group type
                if ( pnlEditDetails.Visible )
                {
                    int? groupTypeId = PageParameter( "CheckinTypeId" ).AsIntegerOrNull();
                    if ( groupTypeId.HasValue && groupTypeId.Value > 0 )
                    {
                        var groupType = new GroupTypeService( new RockContext() ).Get( groupTypeId.Value );
                        if ( groupType != null )
                        {
                            groupType.LoadAttributes();
                            BuildAttributeEdits( groupType, false );
                        }
                    }

                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlSearchType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlSearchType_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetFieldVisibility();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlType_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetFieldVisibility();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlSuccessTemplateOverrideDisplayMode control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlSuccessTemplateOverrideDisplayMode_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetFieldVisibility();
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            GroupTypeService groupTypeService = new GroupTypeService( new RockContext() );
            GroupType groupType = groupTypeService.Get( int.Parse( hfGroupTypeId.Value ) );
            ShowEditDetails( groupType );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            GroupTypeService groupTypeService = new GroupTypeService( rockContext );
            GroupType groupType = groupTypeService.Get( hfGroupTypeId.Value.AsInteger() );

            if ( groupType != null )
            {
                string errorMessage;
                if ( !groupTypeService.CanDelete( groupType, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                int groupTypeId = groupType.Id;

                groupType.ParentGroupTypes.Clear();
                groupType.ChildGroupTypes.Clear();

                groupTypeService.Delete( groupType );
                rockContext.SaveChanges();

                Rock.CheckIn.KioskDevice.Clear();
            }

            var pageRef = new PageReference( CurrentPageReference.PageId, CurrentPageReference.RouteId );
            NavigateToPage( pageRef );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            GroupType groupType = null;

            var rockContext = new RockContext();
            GroupTypeService groupTypeService = new GroupTypeService( rockContext );
            AttributeService attributeService = new AttributeService( rockContext );
            AttributeQualifierService attributeQualifierService = new AttributeQualifierService( rockContext );

            int? groupTypeId = hfGroupTypeId.ValueAsInt();
            if ( groupTypeId.HasValue && groupTypeId.Value > 0 )
            {
                groupType = groupTypeService.Get( groupTypeId.Value );
            }

            bool newGroupType = false;

            if ( groupType == null )
            {
                groupType = new GroupType();
                groupTypeService.Add( groupType );

                var templatePurpose = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid() );
                if ( templatePurpose != null )
                {
                    groupType.GroupTypePurposeValueId = templatePurpose.Id;
                }

                newGroupType = true;
            }

            if ( groupType != null )
            {
                groupType.Name = tbName.Text;
                groupType.Description = tbDescription.Text;
                groupType.IconCssClass = tbIconCssClass.Text;
                groupType.LoadAttributes( rockContext );
                Rock.Attribute.Helper.GetEditValues( phAttributeEdits, groupType );

                groupType.SetAttributeValue( "core_checkin_AgeRequired", cbAgeRequired.Checked.ToString() );
                groupType.SetAttributeValue( "core_checkin_GradeRequired", cbGradeRequired.Checked.ToString() );
                groupType.SetAttributeValue( "core_checkin_HidePhotos", cbHidePhotos.Checked.ToString() );
                groupType.SetAttributeValue( "core_checkin_PreventDuplicateCheckin", cbPreventDuplicateCheckin.Checked.ToString() );
                groupType.SetAttributeValue( "core_checkin_PreventInactivePeople", cbPreventInactivePeople.Checked.ToString() );
                groupType.SetAttributeValue( "core_checkin_CheckInType", ddlType.SelectedValue );
                groupType.SetAttributeValue( "core_checkin_DisplayLocationCount", cbDisplayLocCount.Checked.ToString() );
                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ABILITY_LEVEL_DETERMINATION, rblAbilityLevelDetermination.SelectedValue );
                groupType.SetAttributeValue( "core_checkin_EnableManagerOption", cbEnableManager.Checked.ToString() );
                groupType.SetAttributeValue( "core_checkin_EnableOverride", cbEnableOverride.Checked.ToString() );
                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ACHIEVEMENT_TYPES, listboxAchievementTypes.SelectedValues.AsDelimited(",") );

                groupType.SetAttributeValue( "core_checkin_MaximumPhoneSearchLength", nbMaxPhoneLength.Text );
                groupType.SetAttributeValue( "core_checkin_MaxSearchResults", nbMaxResults.Text );
                groupType.SetAttributeValue( "core_checkin_MinimumPhoneSearchLength", nbMinPhoneLength.Text );
                groupType.SetAttributeValue( "core_checkin_UseSameOptions", cbUseSameOptions.Checked.ToString() );
                groupType.SetAttributeValue( "core_checkin_PhoneSearchType", ddlPhoneSearchType.SelectedValue );
                groupType.SetAttributeValue( "core_checkin_RefreshInterval", nbRefreshInterval.Text );
                groupType.SetAttributeValue( "core_checkin_RegularExpressionFilter", tbSearchRegex.Text );
                groupType.SetAttributeValue( "core_checkin_ReuseSameCode", cbReuseCode.Checked.ToString() );

                var searchType = DefinedValueCache.Get( ddlSearchType.SelectedValueAsInt() ?? 0 );
                if ( searchType != null )
                {
                    groupType.SetAttributeValue( "core_checkin_SearchType", searchType.Guid.ToString() );
                }
                else
                {
                    groupType.SetAttributeValue( "core_checkin_SearchType", Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER );
                }

                groupType.SetAttributeValue( "core_checkin_SecurityCodeLength", nbCodeAlphaNumericLength.Text );
                groupType.SetAttributeValue( "core_checkin_SecurityCodeAlphaLength", nbCodeAlphaLength.Text );
                groupType.SetAttributeValue( "core_checkin_SecurityCodeNumericLength", nbCodeNumericLength.Text );
                groupType.SetAttributeValue( "core_checkin_SecurityCodeNumericRandom", cbCodeRandom.Checked.ToString() );
                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_KIOSK, cbAllowCheckoutAtKiosk.Checked.ToString() );
                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_MANAGER, cbAllowCheckoutInManager.Checked.ToString() );
                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PRESENCE, cbEnablePresence.Checked.ToString() );
                groupType.SetAttributeValue( "core_checkin_AutoSelectDaysBack", nbAutoSelectDaysBack.Text );
                groupType.SetAttributeValue( "core_checkin_AutoSelectOptions", ddlAutoSelectOptions.SelectedValueAsInt() );

                // Registration Settings

                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORADULTS, cbRegistrationDisplayAlternateIdFieldForAdults.Checked.ToTrueFalse() );
                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORCHILDREN, cbRegistrationDisplayAlternateIdFieldForChildren.Checked.ToTrueFalse() );
                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYSMSBUTTON, cbRegistrationDisplaySmsEnabled.Checked.ToTrueFalse() );
                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTSMSENABLED, cbRegistrationSmsEnabledByDefault.Checked.ToTrueFalse() );

                groupType.SetAttributeValue(
                    Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORADULTS,
                     lbRegistrationRequiredAttributesForAdults.SelectedValues.AsDelimited( "," ) );

                groupType.SetAttributeValue(
                    Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORADULTS,
                     lbRegistrationOptionalAttributesForAdults.SelectedValues.AsDelimited( "," ) );

                groupType.SetAttributeValue(
                    Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORCHILDREN,
                    lbRegistrationRequiredAttributesForChildren.SelectedValues.AsDelimited( "," ) );

                groupType.SetAttributeValue(
                    Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORCHILDREN,
                    lbRegistrationOptionalAttributesForChildren.SelectedValues.AsDelimited( "," ) );

                groupType.SetAttributeValue(
                    Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORFAMILIES,
                    lbRegistrationRequiredAttributesForFamilies.SelectedValues.AsDelimited( "," ) );

                groupType.SetAttributeValue(
                    Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORFAMILIES,
                    lbRegistrationOptionalAttributesForFamilies.SelectedValues.AsDelimited( "," ) );

                groupType.SetAttributeValue(
                    Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONCHILDREN,
                    ddlRegistrationDisplayBirthdateOnChildren.SelectedValue );

                groupType.SetAttributeValue(
                    Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONADULTS,
                    ddlRegistrationDisplayBirthdateOnAdults.SelectedValue );

                groupType.SetAttributeValue(
                    Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYGRADEONCHILDREN,
                    ddlRegistrationDisplayGradeOnChildren.SelectedValue );

                groupType.SetAttributeValue(
                    Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONADULTS,
                    ddlRegistrationDisplayRaceOnAdults.SelectedValue );

                groupType.SetAttributeValue(
                    Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONADULTS,
                    ddlRegistrationDisplayEthnicityOnAdults.SelectedValue );

                groupType.SetAttributeValue(
                    Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONCHILDREN,
                    ddlRegistrationDisplayRaceOnChildren.SelectedValue );

                groupType.SetAttributeValue(
                    Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONCHILDREN,
                    ddlRegistrationDisplayEthnicityOnChildren.SelectedValue );

                Guid? defaultPersonConnectionStatusValueGuid = null;
                var defaultPersonConnectionStatusValueId = dvpRegistrationDefaultPersonConnectionStatus.SelectedValue.AsIntegerOrNull();
                if ( defaultPersonConnectionStatusValueId.HasValue )
                {
                    var defaultPersonConnectionStatusValue = DefinedValueCache.Get( defaultPersonConnectionStatusValueId.Value );
                    if ( defaultPersonConnectionStatusValue != null )
                    {
                        defaultPersonConnectionStatusValueGuid = defaultPersonConnectionStatusValue.Guid;
                    }
                }

                groupType.SetAttributeValue(
                    Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTPERSONCONNECTIONSTATUS,
                    defaultPersonConnectionStatusValueGuid.ToString() );

                var workflowTypeService = new WorkflowTypeService( rockContext );

                groupType.SetAttributeValue(
                    Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDFAMILYWORKFLOWTYPES,
                    workflowTypeService.GetByIds( wftpRegistrationAddFamilyWorkflowTypes.SelectedValuesAsInt().ToList() ).Select( a => a.Guid ).ToList().AsDelimited( "," ) );

                groupType.SetAttributeValue(
                    Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDPERSONWORKFLOWTYPES,
                    workflowTypeService.GetByIds( wftpRegistrationAddPersonWorkflowTypes.SelectedValuesAsInt().ToList() ).Select( a => a.Guid ).ToList().AsDelimited( "," ) );

                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ENABLECHECKINAFTERREGISTRATION, cbEnableCheckInAfterRegistration.Checked.ToTrueFalse() );

                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_KNOWNRELATIONSHIPTYPES, lbKnownRelationshipTypes.SelectedValues.AsDelimited( "," ) );
                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_SAMEFAMILYKNOWNRELATIONSHIPTYPES, lbSameFamilyKnownRelationshipTypes.SelectedValues.AsDelimited( "," ) );
                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_CANCHECKINKNOWNRELATIONSHIPTYPES, lbCanCheckInKnownRelationshipTypes.SelectedValues.AsDelimited( "," ) );

                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_ACTION_SELECT_HEADER_LAVA_TEMPLATE, ceActionSelectHeaderTemplate.Text );
                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_CHECKOUT_PERSON_SELECT_HEADER_LAVA_TEMPLATE, ceCheckoutPersonSelectHeaderTemplate.Text );
                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_HEADER_LAVA_TEMPLATE, cePersonSelectHeaderTemplate.Text );
                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_MULTI_PERSON_SELECT_HEADER_LAVA_TEMPLATE, ceMultiPersonSelectHeaderTemplate.Text );
                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUP_TYPE_SELECT_HEADER_LAVA_TEMPLATE, ceGroupTypeSelectHeaderTemplate.Text );
                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_TIME_SELECT_HEADER_LAVA_TEMPLATE, ceTimeSelectHeaderTemplate.Text );
                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_ABILITY_LEVEL_SELECT_HEADER_LAVA_TEMPLATE, ceAbilityLevelSelectHeaderTemplate.Text );
                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_LOCATION_SELECT_HEADER_LAVA_TEMPLATE, ceLocationSelectHeaderTemplate.Text );
                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUP_SELECT_HEADER_LAVA_TEMPLATE, ceGroupSelectHeaderTemplate.Text );

                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_START_LAVA_TEMPLATE, ceStartTemplate.Text );
                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_FAMILYSELECT_LAVA_TEMPLATE, ceFamilySelectTemplate.Text );
                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_ADDITIONAL_INFORMATION_LAVA_TEMPLATE, cePersonSelectTemplate.Text );

                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE_OVERRIDE_DISPLAY_MODE, ddlSuccessTemplateOverrideDisplayMode.SelectedValue );
                groupType.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE, ceSuccessTemplate.Text );

                // Save group type and attributes
                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    groupType.SaveAttributeValues( rockContext );
                } );

                if ( newGroupType )
                {
                    var pageRef = new PageReference( CurrentPageReference.PageId, CurrentPageReference.RouteId );
                    pageRef.Parameters.Add( "CheckinTypeId", groupType.Id.ToString() );
                    NavigateToPage( pageRef );
                }
                else
                {
                    groupType = groupTypeService.Get( groupType.Id );
                    ShowReadonlyDetails( groupType );
                }

                Rock.CheckIn.KioskDevice.Clear();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfGroupTypeId.Value.Equals( "0" ) )
            {
                var pageRef = new PageReference( CurrentPageReference.PageId, CurrentPageReference.RouteId );
                NavigateToPage( pageRef );
            }
            else
            {
                var pageRef = new PageReference( CurrentPageReference.PageId, CurrentPageReference.RouteId );
                pageRef.Parameters.Add( "CheckinTypeId", hfGroupTypeId.Value );
                NavigateToPage( pageRef );
            }
        }

        protected void btnSchedules_Click( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "GroupTypeId", hfGroupTypeId.Value );
            NavigateToLinkedPage( "SchedulePage", qryParams );
        }


        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="groupTypeId">The groupType identifier.</param>
        public void ShowDetail( int groupTypeId )
        {
            pnlDetails.Visible = false;

            bool editAllowed = true;

            GroupType groupType = null;

            if ( !groupTypeId.Equals( 0 ) )
            {
                groupType = new GroupTypeService( new RockContext() ).Get( groupTypeId );
                pdAuditDetails.SetEntity( groupType, ResolveRockUrl( "~" ) );
            }

            if ( groupType == null )
            {
                groupType = new GroupType { Id = 0 };
                var templatePurpose = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid() );
                if ( templatePurpose != null )
                {
                    groupType.GroupTypePurposeValueId = templatePurpose.Id;
                }

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            if ( groupType != null )
            {
                editAllowed = groupType.IsAuthorized( Authorization.EDIT, CurrentPerson );

                pnlDetails.Visible = true;
                hfGroupTypeId.Value = groupType.Id.ToString();

                // render UI based on Authorized and IsSystem
                bool readOnly = false;

                nbEditModeMessage.Text = string.Empty;
                if ( !editAllowed || !IsUserAuthorized( Authorization.EDIT ) )
                {
                    readOnly = true;
                    nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( GroupType.FriendlyTypeName );
                }

                if ( readOnly )
                {
                    btnEdit.Visible = false;
                    btnDelete.Visible = false;
                    ShowReadonlyDetails( groupType );
                }
                else
                {
                    btnEdit.Visible = true;
                    btnDelete.Visible = true;

                    if ( groupType.Id > 0 )
                    {
                        ShowReadonlyDetails( groupType );
                    }
                    else
                    {
                        ShowEditDetails( groupType );
                    }
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="groupType">The groupType.</param>
        private void ShowEditDetails( GroupType groupType )
        {
            if ( groupType != null )
            {
                if ( groupType.Id == 0 )
                {
                    lReadOnlyTitle.Text = ActionTitle.Add( "Check-in Configuration" ).FormatAsHtmlTitle();
                }
                else
                {
                    lReadOnlyTitle.Text = groupType.ToString().FormatAsHtmlTitle();
                }

                SetEditMode( true );

                tbName.Text = groupType.Name;
                tbDescription.Text = groupType.Description;
                tbIconCssClass.Text = groupType.IconCssClass;
                var rockContext = new RockContext();

                groupType.LoadAttributes( rockContext );

                cbAgeRequired.Checked = groupType.GetAttributeValue( "core_checkin_AgeRequired" ).AsBoolean( true );
                cbGradeRequired.Checked = groupType.GetAttributeValue( "core_checkin_GradeRequired" ).AsBoolean( true );
                cbHidePhotos.Checked = groupType.GetAttributeValue( "core_checkin_HidePhotos" ).AsBoolean( true );
                cbPreventDuplicateCheckin.Checked = groupType.GetAttributeValue( "core_checkin_PreventDuplicateCheckin" ).AsBoolean( true );
                cbPreventInactivePeople.Checked = groupType.GetAttributeValue( "core_checkin_PreventInactivePeople" ).AsBoolean( true );
                ddlType.SetValue( groupType.GetAttributeValue( "core_checkin_CheckInType" ) );
                cbDisplayLocCount.Checked = groupType.GetAttributeValue( "core_checkin_DisplayLocationCount" ).AsBoolean( true );
                rblAbilityLevelDetermination.SelectedValue = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ABILITY_LEVEL_DETERMINATION );
                cbEnableManager.Checked = groupType.GetAttributeValue( "core_checkin_EnableManagerOption" ).AsBoolean( true );
                cbEnableOverride.Checked = groupType.GetAttributeValue( "core_checkin_EnableOverride" ).AsBoolean( true );
                listboxAchievementTypes.SetValues( groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ACHIEVEMENT_TYPES ).SplitDelimitedValues() );
                nbMaxPhoneLength.Text = groupType.GetAttributeValue( "core_checkin_MaximumPhoneSearchLength" );
                nbMaxResults.Text = groupType.GetAttributeValue( "core_checkin_MaxSearchResults" );
                nbMinPhoneLength.Text = groupType.GetAttributeValue( "core_checkin_MinimumPhoneSearchLength" );
                cbUseSameOptions.Checked = groupType.GetAttributeValue( "core_checkin_UseSameOptions" ).AsBoolean( false );
                ddlPhoneSearchType.SetValue( groupType.GetAttributeValue( "core_checkin_PhoneSearchType" ) );
                nbRefreshInterval.Text = groupType.GetAttributeValue( "core_checkin_RefreshInterval" );
                tbSearchRegex.Text = groupType.GetAttributeValue( "core_checkin_RegularExpressionFilter" );
                cbReuseCode.Checked = groupType.GetAttributeValue( "core_checkin_ReuseSameCode" ).AsBoolean( false );

                var searchType = DefinedValueCache.Get( groupType.GetAttributeValue( "core_checkin_SearchType" ).AsGuid() );
                if ( searchType != null )
                {
                    ddlSearchType.SetValue( searchType.Id.ToString() );
                }

                nbCodeAlphaNumericLength.Text = groupType.GetAttributeValue( "core_checkin_SecurityCodeLength" );
                nbCodeAlphaLength.Text = groupType.GetAttributeValue( "core_checkin_SecurityCodeAlphaLength" );
                nbCodeNumericLength.Text = groupType.GetAttributeValue( "core_checkin_SecurityCodeNumericLength" );
                cbCodeRandom.Checked = groupType.GetAttributeValue( "core_checkin_SecurityCodeNumericRandom" ).AsBoolean( true );
                cbAllowCheckoutAtKiosk.Checked = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_KIOSK ).AsBoolean();
                cbAllowCheckoutInManager.Checked = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_MANAGER ).AsBoolean();
                cbEnablePresence.Checked = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PRESENCE ).AsBoolean();
                nbAutoSelectDaysBack.Text = groupType.GetAttributeValue( "core_checkin_AutoSelectDaysBack" );
                ddlAutoSelectOptions.SetValue( groupType.GetAttributeValue( "core_checkin_AutoSelectOptions" ) );

                // Registration Settings
                cbRegistrationDisplayAlternateIdFieldForAdults.Checked = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORADULTS ).AsBoolean();
                cbRegistrationDisplayAlternateIdFieldForChildren.Checked = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORCHILDREN ).AsBoolean();

                cbRegistrationDisplaySmsEnabled.Checked = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYSMSBUTTON ).AsBoolean();
                cbRegistrationSmsEnabledByDefault.Checked = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTSMSENABLED ).AsBoolean();

                lbRegistrationRequiredAttributesForAdults.SetValues( groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORADULTS ).SplitDelimitedValues() );
                lbRegistrationOptionalAttributesForAdults.SetValues( groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORADULTS ).SplitDelimitedValues() );

                lbRegistrationRequiredAttributesForChildren.SetValues( groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORCHILDREN ).SplitDelimitedValues() );
                lbRegistrationOptionalAttributesForChildren.SetValues( groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORCHILDREN ).SplitDelimitedValues() );

                lbRegistrationRequiredAttributesForFamilies.SetValues( groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORFAMILIES ).SplitDelimitedValues() );
                lbRegistrationOptionalAttributesForFamilies.SetValues( groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORFAMILIES ).SplitDelimitedValues() );

                ddlRegistrationDisplayBirthdateOnChildren.SetValue( groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONCHILDREN ) );
                ddlRegistrationDisplayBirthdateOnAdults.SetValue( groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONADULTS ) );
                ddlRegistrationDisplayGradeOnChildren.SetValue( groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYGRADEONCHILDREN ) );

                ddlRegistrationDisplayRaceOnAdults.SetValue( groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONADULTS ) );
                ddlRegistrationDisplayEthnicityOnAdults.SetValue( groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONADULTS ) );
                ddlRegistrationDisplayRaceOnChildren.SetValue( groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONCHILDREN ) );
                ddlRegistrationDisplayEthnicityOnChildren.SetValue( groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONCHILDREN ) );

                int? defaultPersonConnectionStatusValueId = null;
                Guid? defaultPersonConnectionStatusValueGuid = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTPERSONCONNECTIONSTATUS ).AsGuidOrNull();
                if ( defaultPersonConnectionStatusValueGuid.HasValue )
                {
                    var defaultPersonRecordStatusValue = DefinedValueCache.Get( defaultPersonConnectionStatusValueGuid.Value );
                    if ( defaultPersonRecordStatusValue != null)
                    {
                        defaultPersonConnectionStatusValueId = defaultPersonRecordStatusValue.Id;
                    }
                }

                dvpRegistrationDefaultPersonConnectionStatus.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() ).Id;
                dvpRegistrationDefaultPersonConnectionStatus.SetValue( defaultPersonConnectionStatusValueId );

                var workflowTypeService = new WorkflowTypeService( rockContext );
                wftpRegistrationAddFamilyWorkflowTypes.SetValues( workflowTypeService.GetByGuids( groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDFAMILYWORKFLOWTYPES ).SplitDelimitedValues().AsGuidList() ) );
                wftpRegistrationAddPersonWorkflowTypes.SetValues( workflowTypeService.GetByGuids( groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDPERSONWORKFLOWTYPES ).SplitDelimitedValues().AsGuidList() ) );

                cbEnableCheckInAfterRegistration.Checked = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ENABLECHECKINAFTERREGISTRATION ).AsBoolean();

                lbKnownRelationshipTypes.SetValues( groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_KNOWNRELATIONSHIPTYPES ).SplitDelimitedValues() );
                lbSameFamilyKnownRelationshipTypes.SetValues( groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_SAMEFAMILYKNOWNRELATIONSHIPTYPES ).SplitDelimitedValues() );
                lbCanCheckInKnownRelationshipTypes.SetValues( groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_CANCHECKINKNOWNRELATIONSHIPTYPES ).SplitDelimitedValues() );

                ceActionSelectHeaderTemplate.Text = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_ACTION_SELECT_HEADER_LAVA_TEMPLATE );
                ceCheckoutPersonSelectHeaderTemplate.Text = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_CHECKOUT_PERSON_SELECT_HEADER_LAVA_TEMPLATE );
                cePersonSelectHeaderTemplate.Text = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_HEADER_LAVA_TEMPLATE );
                ceMultiPersonSelectHeaderTemplate.Text = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_MULTI_PERSON_SELECT_HEADER_LAVA_TEMPLATE );
                ceGroupTypeSelectHeaderTemplate.Text = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUP_TYPE_SELECT_HEADER_LAVA_TEMPLATE );
                ceTimeSelectHeaderTemplate.Text = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_TIME_SELECT_HEADER_LAVA_TEMPLATE );
                ceAbilityLevelSelectHeaderTemplate.Text = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_ABILITY_LEVEL_SELECT_HEADER_LAVA_TEMPLATE );
                ceLocationSelectHeaderTemplate.Text = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_LOCATION_SELECT_HEADER_LAVA_TEMPLATE );
                ceGroupSelectHeaderTemplate.Text = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUP_SELECT_HEADER_LAVA_TEMPLATE );

                ceStartTemplate.Text = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_START_LAVA_TEMPLATE );
                ceFamilySelectTemplate.Text = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_FAMILYSELECT_LAVA_TEMPLATE );
                cePersonSelectTemplate.Text = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_ADDITIONAL_INFORMATION_LAVA_TEMPLATE );

                ddlSuccessTemplateOverrideDisplayMode.SetValue( groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE_OVERRIDE_DISPLAY_MODE ) );
                ceSuccessTemplate.Text = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE );

                // Other GroupType Attributes
                BuildAttributeEdits( groupType, true );

                SetFieldVisibility();
            }
        }

        private void BuildAttributeEdits( GroupType groupType, bool setValues )
        {
            var excludeList = new List<string>();
            excludeList.Add( "core_checkin_AgeRequired" );
            excludeList.Add( "core_checkin_GradeRequired" );
            excludeList.Add( "core_checkin_HidePhotos" );
            excludeList.Add( "core_checkin_PreventDuplicateCheckin" );
            excludeList.Add( "core_checkin_PreventInactivePeople" );
            excludeList.Add( "core_checkin_CheckInType" );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ABILITY_LEVEL_DETERMINATION );
            excludeList.Add( "core_checkin_DisplayLocationCount" );
            excludeList.Add( "core_checkin_EnableManagerOption" );
            excludeList.Add( "core_checkin_EnableOverride" );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ACHIEVEMENT_TYPES );
            excludeList.Add( "core_checkin_MaximumPhoneSearchLength" );
            excludeList.Add( "core_checkin_MaxSearchResults" );
            excludeList.Add( "core_checkin_MinimumPhoneSearchLength" );
            excludeList.Add( "core_checkin_UseSameOptions" );
            excludeList.Add( "core_checkin_PhoneSearchType" );
            excludeList.Add( "core_checkin_RefreshInterval" );
            excludeList.Add( "core_checkin_RegularExpressionFilter" );
            excludeList.Add( "core_checkin_ReuseSameCode" );
            excludeList.Add( "core_checkin_SearchType" );
            excludeList.Add( "core_checkin_SecurityCodeLength" );
            excludeList.Add( "core_checkin_SecurityCodeAlphaLength" );
            excludeList.Add( "core_checkin_SecurityCodeNumericLength" );
            excludeList.Add( "core_checkin_SecurityCodeNumericRandom" );
            excludeList.Add( "core_checkin_AutoSelectDaysBack" );
            excludeList.Add( "core_checkin_AutoSelectOptions" );
#pragma warning disable CS0618 // Type or member is obsolete
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT );
#pragma warning restore CS0618 // Type or member is obsolete
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_MANAGER );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_KIOSK );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PRESENCE );

            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_CANCHECKINKNOWNRELATIONSHIPTYPES );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORADULTS );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORCHILDREN );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYSMSBUTTON );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTSMSENABLED );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ENABLECHECKINAFTERREGISTRATION );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_KNOWNRELATIONSHIPTYPES );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORADULTS );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORCHILDREN );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORFAMILIES );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORADULTS );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORCHILDREN );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORFAMILIES );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_SAMEFAMILYKNOWNRELATIONSHIPTYPES );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDFAMILYWORKFLOWTYPES );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDPERSONWORKFLOWTYPES );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTPERSONCONNECTIONSTATUS );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONCHILDREN );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONADULTS );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYGRADEONCHILDREN );

            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_ACTION_SELECT_HEADER_LAVA_TEMPLATE );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_CHECKOUT_PERSON_SELECT_HEADER_LAVA_TEMPLATE );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_HEADER_LAVA_TEMPLATE );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_MULTI_PERSON_SELECT_HEADER_LAVA_TEMPLATE );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUP_TYPE_SELECT_HEADER_LAVA_TEMPLATE );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_TIME_SELECT_HEADER_LAVA_TEMPLATE );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_ABILITY_LEVEL_SELECT_HEADER_LAVA_TEMPLATE );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_LOCATION_SELECT_HEADER_LAVA_TEMPLATE );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUP_SELECT_HEADER_LAVA_TEMPLATE );

            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_START_LAVA_TEMPLATE );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_FAMILYSELECT_LAVA_TEMPLATE );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE_OVERRIDE_DISPLAY_MODE );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_ADDITIONAL_INFORMATION_LAVA_TEMPLATE );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONCHILDREN );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONCHILDREN );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONADULTS );
            excludeList.Add( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONADULTS );

            if ( groupType.Attributes.Any( t => !excludeList.Contains( t.Value.Key ) ) )
            {
                wpCustom.Visible = true;
                Rock.Attribute.Helper.AddEditControls( groupType, phAttributeEdits, setValues, BlockValidationGroup, excludeList );
            }
            else
            {
                wpCustom.Visible = false;
            }
        }

        /// <summary>
        /// Sets the field visibility.
        /// </summary>
        private void SetFieldVisibility()
        {
            bool familyType = ddlType.SelectedValue == "1";
            nbAutoSelectDaysBack.Visible = familyType;
            ddlAutoSelectOptions.Visible = familyType;
            cbReuseCode.Visible = familyType;
            cbHidePhotos.Visible = familyType;
            cbUseSameOptions.Visible = familyType;
            cbPreventDuplicateCheckin.Visible = familyType;

            bool showPhoneFields = true;
            int? searchTypeId = ddlSearchType.SelectedValueAsId();
            if ( searchTypeId.HasValue )
            {
                var nameSearch = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME.AsGuid() );
                showPhoneFields = nameSearch == null || searchTypeId.Value != nameSearch.Id;
            }

            nbMinPhoneLength.Visible = showPhoneFields;
            nbMaxPhoneLength.Visible = showPhoneFields;
            ddlPhoneSearchType.Visible = showPhoneFields;

            var successLavaTemplateDisplayMode = ddlSuccessTemplateOverrideDisplayMode.SelectedValueAsEnum<SuccessLavaTemplateDisplayMode>( SuccessLavaTemplateDisplayMode.Never );
            ceSuccessTemplate.Visible = successLavaTemplateDisplayMode != SuccessLavaTemplateDisplayMode.Never;
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="groupType">The groupType.</param>
        private void ShowReadonlyDetails( GroupType groupType )
        {
            SetEditMode( false );

            if ( groupType != null )
            {
                hfGroupTypeId.SetValue( groupType.Id );
                lReadOnlyTitle.Text = groupType.ToString().FormatAsHtmlTitle();

                lDescription.Text = groupType.Description;

                groupType.LoadAttributes();

                hlType.Text = groupType.GetAttributeValue( "CheckInType" );
                hlType.Visible = true;

                DescriptionList mainDetailsDescList = new DescriptionList();
                DescriptionList leftDetailsDescList = new DescriptionList();
                DescriptionList rightDetailsDescList = new DescriptionList();

                string scheduleList = string.Empty;
                using ( var rockContext = new RockContext() )
                {
                    var descendantGroupTypeIds = new GroupTypeService( rockContext ).GetCheckinAreaDescendants( groupType.Id ).Select( a => a.Id );
                    scheduleList = new GroupLocationService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( a =>
                            a.Group.GroupType.Id == groupType.Id ||
                            descendantGroupTypeIds.Contains( a.Group.GroupTypeId ) )
                        .SelectMany( a => a.Schedules )
                        .Where( s => s.IsActive )
                        .Select( s => s.Name )
                        .Distinct()
                        .OrderBy( s => s )
                        .ToList()
                        .AsDelimited( ", " );
                }

                if ( !string.IsNullOrWhiteSpace( scheduleList ) )
                {
                    mainDetailsDescList.Add( "Scheduled Times", scheduleList );
                }

                groupType.LoadAttributes();

                if ( groupType.AttributeValues.ContainsKey( "core_checkin_CheckInType" ) )
                {
                    leftDetailsDescList.Add( "Check-in Type", groupType.AttributeValues["core_checkin_CheckInType"].ValueFormatted );
                }

                if ( groupType.AttributeValues.ContainsKey( "core_checkin_SearchType" ) )
                {
                    var searchType = groupType.AttributeValues["core_checkin_SearchType"];
                    rightDetailsDescList.Add( "Search Type", searchType.ValueFormatted );

                    var searchTypeGuid = searchType.Value.AsGuid();
                    if ( searchTypeGuid.Equals( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME_AND_PHONE.AsGuid() ) ||
                        searchTypeGuid.Equals( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER.AsGuid() ) )
                    {
                        rightDetailsDescList.Add( "Phone Number Compare", groupType.AttributeValues["core_checkin_PhoneSearchType"].ValueFormatted );
                    }
                }

                lblMainDetails.Text = mainDetailsDescList.Html;
                lblLeftDetails.Text = leftDetailsDescList.Html;
                lblRightDetails.Text = rightDetailsDescList.Html;
            }
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        private void LoadDropdowns()
        {
            ddlSearchType.Items.Clear();

            var searchTypes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.CHECKIN_SEARCH_TYPE.AsGuid() );
            if ( searchTypes != null )
            {
                foreach ( var searchType in searchTypes.DefinedValues )
                {
                    if ( searchType.GetAttributeValue( "UserSelectable" ).AsBooleanOrNull() ?? true )
                    {
                        ddlSearchType.Items.Add( new System.Web.UI.WebControls.ListItem( searchType.Value, searchType.Id.ToString() ) );
                    }
                }
            }

            var achievementTypes = AchievementTypeCache.All()
                .Where( stat => stat.IsActive )
                .OrderBy( stat => stat.Name )
                .ToList();

            listboxAchievementTypes.Items.Clear();

            foreach ( var achievementType in achievementTypes )
            {
                listboxAchievementTypes.Items.Add( new ListItem( achievementType.Name, achievementType.Guid.ToString() ) );
            }

            lbKnownRelationshipTypes.Items.Clear();
            lbKnownRelationshipTypes.Items.Add( new ListItem( "Child", "0" ) );
            lbSameFamilyKnownRelationshipTypes.Items.Clear();
            lbSameFamilyKnownRelationshipTypes.Items.Add( new ListItem( "Child", "0" ) );
            lbCanCheckInKnownRelationshipTypes.Items.Clear();
            var knownRelationShipRoles = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS ).Roles;
            foreach ( var knownRelationShipRole in knownRelationShipRoles.Where( a => a.Name != "Child" ) )
            {
                lbKnownRelationshipTypes.Items.Add( new ListItem( knownRelationShipRole.Name, knownRelationShipRole.Id.ToString() ) );
                lbSameFamilyKnownRelationshipTypes.Items.Add( new ListItem( knownRelationShipRole.Name, knownRelationShipRole.Id.ToString() ) );
                lbCanCheckInKnownRelationshipTypes.Items.Add( new ListItem( knownRelationShipRole.Name, knownRelationShipRole.Id.ToString() ) );
            }

            lbRegistrationRequiredAttributesForAdults.Items.Clear();
            lbRegistrationOptionalAttributesForAdults.Items.Clear();
            lbRegistrationRequiredAttributesForChildren.Items.Clear();
            lbRegistrationOptionalAttributesForChildren.Items.Clear();

            var fakePerson = new Person();
            fakePerson.LoadAttributes();
            foreach ( var personAttribute in fakePerson.Attributes.Select( a => new { Name = a.Value.Name, Value = a.Value.Guid.ToString() } ) )
            {
                lbRegistrationRequiredAttributesForAdults.Items.Add( new ListItem( personAttribute.Name, personAttribute.Value ) );
                lbRegistrationOptionalAttributesForAdults.Items.Add( new ListItem( personAttribute.Name, personAttribute.Value ) );
                lbRegistrationRequiredAttributesForChildren.Items.Add( new ListItem( personAttribute.Name, personAttribute.Value ) );
                lbRegistrationOptionalAttributesForChildren.Items.Add( new ListItem( personAttribute.Name, personAttribute.Value ) );
            }

            lbRegistrationOptionalAttributesForFamilies.Items.Clear();
            lbRegistrationRequiredAttributesForFamilies.Items.Clear();

            var fakeFamily = new Group { GroupTypeId = GroupTypeCache.GetFamilyGroupType().Id };
            fakeFamily.LoadAttributes();

            foreach ( var groupTypeFamilyAttribute in fakeFamily.Attributes.Select( a => new { Name = a.Value.Name, Value = a.Value.Guid.ToString() } ) )
            {
                lbRegistrationRequiredAttributesForFamilies.Items.Add( new ListItem( groupTypeFamilyAttribute.Name, groupTypeFamilyAttribute.Value ) );
                lbRegistrationOptionalAttributesForFamilies.Items.Add( new ListItem( groupTypeFamilyAttribute.Name, groupTypeFamilyAttribute.Value ) );
            }

            ddlSuccessTemplateOverrideDisplayMode.Items.Clear();
            ddlSuccessTemplateOverrideDisplayMode.BindToEnum<SuccessLavaTemplateDisplayMode>();

            ddlRegistrationDisplayBirthdateOnChildren.Items.Clear();
            ddlRegistrationDisplayBirthdateOnChildren.Items.Add( ControlOptions.HIDE );
            ddlRegistrationDisplayBirthdateOnChildren.Items.Add( ControlOptions.OPTIONAL );
            ddlRegistrationDisplayBirthdateOnChildren.Items.Add( ControlOptions.REQUIRED );

            ddlRegistrationDisplayBirthdateOnAdults.Items.Clear();
            ddlRegistrationDisplayBirthdateOnAdults.Items.Add( ControlOptions.HIDE );
            ddlRegistrationDisplayBirthdateOnAdults.Items.Add( ControlOptions.OPTIONAL );
            ddlRegistrationDisplayBirthdateOnAdults.Items.Add( ControlOptions.REQUIRED );

            ddlRegistrationDisplayGradeOnChildren.Items.Clear();
            ddlRegistrationDisplayGradeOnChildren.Items.Add( ControlOptions.HIDE );
            ddlRegistrationDisplayGradeOnChildren.Items.Add( ControlOptions.OPTIONAL );
            ddlRegistrationDisplayGradeOnChildren.Items.Add( ControlOptions.REQUIRED );

            ddlRegistrationDisplayRaceOnChildren.Items.Clear();
            ddlRegistrationDisplayRaceOnChildren.Items.Add( ControlOptions.HIDE );
            ddlRegistrationDisplayRaceOnChildren.Items.Add( ControlOptions.OPTIONAL );
            ddlRegistrationDisplayRaceOnChildren.Items.Add( ControlOptions.REQUIRED );

            ddlRegistrationDisplayEthnicityOnChildren.Items.Clear();
            ddlRegistrationDisplayEthnicityOnChildren.Items.Add( ControlOptions.HIDE );
            ddlRegistrationDisplayEthnicityOnChildren.Items.Add( ControlOptions.OPTIONAL );
            ddlRegistrationDisplayEthnicityOnChildren.Items.Add( ControlOptions.REQUIRED );

            ddlRegistrationDisplayRaceOnAdults.Items.Clear();
            ddlRegistrationDisplayRaceOnAdults.Items.Add( ControlOptions.HIDE );
            ddlRegistrationDisplayRaceOnAdults.Items.Add( ControlOptions.OPTIONAL );
            ddlRegistrationDisplayRaceOnAdults.Items.Add( ControlOptions.REQUIRED );

            ddlRegistrationDisplayEthnicityOnAdults.Items.Clear();
            ddlRegistrationDisplayEthnicityOnAdults.Items.Add( ControlOptions.HIDE );
            ddlRegistrationDisplayEthnicityOnAdults.Items.Add( ControlOptions.OPTIONAL );
            ddlRegistrationDisplayEthnicityOnAdults.Items.Add( ControlOptions.REQUIRED );
        }

        #endregion
    }
}