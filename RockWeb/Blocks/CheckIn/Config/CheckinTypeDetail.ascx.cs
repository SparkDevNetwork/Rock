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

using Rock;
using Rock.Attribute;
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
    public partial class CheckinTypeDetail : RockBlock, IDetailBlock
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
                            BuildAttributeEdits( groupType, true );
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

                GroupTypeCache.Flush( groupTypeId );
                Rock.CheckIn.KioskDevice.FlushAll();
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

                var templatePurpose = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid() );
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

                groupType.LoadAttributes( rockContext );
                Rock.Attribute.Helper.GetEditValues( phAttributeEdits, groupType );

                groupType.SetAttributeValue( "core_checkin_AgeRequired", cbAgeRequired.Checked.ToString() );
                groupType.SetAttributeValue( "core_checkin_GradeRequired", cbGradeRequired.Checked.ToString() );
                groupType.SetAttributeValue( "core_checkin_HidePhotos", cbHidePhotos.Checked.ToString() );
                groupType.SetAttributeValue( "core_checkin_PreventDuplicateCheckin", cbPreventDuplicateCheckin.Checked.ToString() );
                groupType.SetAttributeValue( "core_checkin_PreventInactivePeople", cbPreventInactivePeople.Checked.ToString() );
                groupType.SetAttributeValue( "core_checkin_CheckInType", ddlType.SelectedValue );
                groupType.SetAttributeValue( "core_checkin_DisplayLocationCount", cbDisplayLocCount.Checked.ToString() );
                groupType.SetAttributeValue( "core_checkin_EnableManagerOption", cbEnableManager.Checked.ToString() );
                groupType.SetAttributeValue( "core_checkin_EnableOverride", cbEnableOverride.Checked.ToString() );
                groupType.SetAttributeValue( "core_checkin_MaximumPhoneSearchLength", nbMaxPhoneLength.Text );
                groupType.SetAttributeValue( "core_checkin_MaxSearchResults", nbMaxResults.Text );
                groupType.SetAttributeValue( "core_checkin_MinimumPhoneSearchLength", nbMinPhoneLength.Text );
                groupType.SetAttributeValue( "core_checkin_UseSameOptions", cbUseSameOptions.Checked.ToString() );
                groupType.SetAttributeValue( "core_checkin_PhoneSearchType", ddlPhoneSearchType.SelectedValue );
                groupType.SetAttributeValue( "core_checkin_RefreshInterval", nbRefreshInterval.Text );
                groupType.SetAttributeValue( "core_checkin_RegularExpressionFilter", tbSearchRegex.Text );
                groupType.SetAttributeValue( "core_checkin_ReuseSameCode", cbReuseCode.Checked.ToString() );

                var searchType = DefinedValueCache.Read( ddlSearchType.SelectedValueAsInt() ?? 0 );
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
                groupType.SetAttributeValue( "core_checkin_SecurityCodeNumericRandom", cbCodeRandom.Checked.ToString());
                groupType.SetAttributeValue( "core_checkin_AutoSelectDaysBack", nbAutoSelectDaysBack.Text );

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

                GroupTypeCache.Flush( groupType.Id );
                Rock.CheckIn.KioskDevice.FlushAll();
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
            qryParams.Add( "groupTypeId", hfGroupTypeId.Value );
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

                var rockContext = new RockContext();

                groupType.LoadAttributes( rockContext );

                cbAgeRequired.Checked = groupType.GetAttributeValue( "core_checkin_AgeRequired" ).AsBoolean( true );
                cbGradeRequired.Checked = groupType.GetAttributeValue( "core_checkin_GradeRequired" ).AsBoolean( true );
                cbHidePhotos.Checked = groupType.GetAttributeValue( "core_checkin_HidePhotos" ).AsBoolean( true );
                cbPreventDuplicateCheckin.Checked = groupType.GetAttributeValue( "core_checkin_PreventDuplicateCheckin" ).AsBoolean( true );
                cbPreventInactivePeople.Checked = groupType.GetAttributeValue( "core_checkin_PreventInactivePeople" ).AsBoolean( true );
                ddlType.SetValue( groupType.GetAttributeValue( "core_checkin_CheckInType" ) );
                cbDisplayLocCount.Checked = groupType.GetAttributeValue( "core_checkin_DisplayLocationCount" ).AsBoolean( true );
                cbEnableManager.Checked = groupType.GetAttributeValue( "core_checkin_EnableManagerOption" ).AsBoolean( true );
                cbEnableOverride.Checked = groupType.GetAttributeValue( "core_checkin_EnableOverride" ).AsBoolean( true );
                nbMaxPhoneLength.Text = groupType.GetAttributeValue( "core_checkin_MaximumPhoneSearchLength" );
                nbMaxResults.Text = groupType.GetAttributeValue( "core_checkin_MaxSearchResults" );
                nbMinPhoneLength.Text = groupType.GetAttributeValue( "core_checkin_MinimumPhoneSearchLength" );
                cbUseSameOptions.Checked = groupType.GetAttributeValue( "core_checkin_UseSameOptions" ).AsBoolean( false );
                ddlPhoneSearchType.SetValue( groupType.GetAttributeValue( "core_checkin_PhoneSearchType" ) );
                nbRefreshInterval.Text = groupType.GetAttributeValue( "core_checkin_RefreshInterval" );
                tbSearchRegex.Text = groupType.GetAttributeValue( "core_checkin_RegularExpressionFilter" );
                cbReuseCode.Checked = groupType.GetAttributeValue( "core_checkin_ReuseSameCode" ).AsBoolean( false );

                var searchType = DefinedValueCache.Read( groupType.GetAttributeValue( "core_checkin_SearchType" ).AsGuid() );
                if ( searchType != null )
                {
                    ddlSearchType.SetValue( searchType.Id.ToString() );
                }

                nbCodeAlphaNumericLength.Text = groupType.GetAttributeValue( "core_checkin_SecurityCodeLength" );
                nbCodeAlphaLength.Text = groupType.GetAttributeValue( "core_checkin_SecurityCodeAlphaLength" );
                nbCodeNumericLength.Text = groupType.GetAttributeValue( "core_checkin_SecurityCodeNumericLength" );
                cbCodeRandom.Checked = groupType.GetAttributeValue( "core_checkin_SecurityCodeNumericRandom" ).AsBoolean(true);
                nbAutoSelectDaysBack.Text = groupType.GetAttributeValue( "core_checkin_AutoSelectDaysBack" );

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
            excludeList.Add( "core_checkin_DisplayLocationCount" );
            excludeList.Add( "core_checkin_EnableManagerOption" );
            excludeList.Add( "core_checkin_EnableOverride" );
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

        private void SetFieldVisibility()
        {
            bool familyType = ddlType.SelectedValue == "1";
            nbAutoSelectDaysBack.Visible = familyType;
            cbReuseCode.Visible = familyType;
            cbHidePhotos.Visible = familyType;
            cbUseSameOptions.Visible = familyType;
            cbPreventDuplicateCheckin.Visible = familyType;

            bool showPhoneFields = true;
            int? searchTypeId = ddlSearchType.SelectedValueAsId();
            if ( searchTypeId.HasValue )
            {
                var nameSearch = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME.AsGuid() );
                showPhoneFields = nameSearch == null || searchTypeId.Value != nameSearch.Id;
            }

            nbMinPhoneLength.Visible = showPhoneFields;
            nbMaxPhoneLength.Visible = showPhoneFields;
            ddlPhoneSearchType.Visible = showPhoneFields;
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
                    var descendantGroupTypeIds = new GroupTypeService( rockContext ).GetAllAssociatedDescendents( groupType.Id ).Select( a => a.Id );
                    scheduleList = new GroupLocationService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( a =>
                            a.Group.GroupType.Id == groupType.Id ||
                            descendantGroupTypeIds.Contains( a.Group.GroupTypeId ) )
                        .SelectMany( a => a.Schedules )
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
                    rightDetailsDescList.Add( "Search Type", groupType.AttributeValues["core_checkin_SearchType"].ValueFormatted );
                }
                if ( groupType.AttributeValues.ContainsKey( "core_checkin_PhoneSearchType" ) )
                {
                    rightDetailsDescList.Add( "Phone Number Compare", groupType.AttributeValues["core_checkin_PhoneSearchType"].ValueFormatted );
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
            var searchTypes = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.CHECKIN_SEARCH_TYPE.AsGuid() );
            if ( searchTypes != null )
            {
                ddlSearchType.BindToDefinedType( searchTypes );
            }
        }

        #endregion

    }
}