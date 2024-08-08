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
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Checkr.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Security;
using Rock.SystemKey;

namespace RockWeb.Blocks.Security.BackgroundCheck
{
    [DisplayName( "Protect My Ministry Settings" )]
    [Category( "Security > Background Check" )]
    [Description( "Block for updating the settings used by the Protect My Ministry integration." )]

    [Rock.SystemGuid.BlockTypeGuid( "AF36FA7E-BD2A-42A3-AF30-2FEBC1C46663" )]
    public partial class ProtectMyMinistrySettings : Rock.Web.UI.RockBlock
    {
        private const string GET_STARTED_URL = "https://www.rockrms.com/Redirect/PMMSignup";
        private const string PROMOTION_IMAGE_URL = "https://rockrms.blob.core.windows.net/resources/pmm-integration/pmm-integration-banner.png";
        private const string TYPENAME_PREFIX = "PMM - ";

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gDefinedValues.DataKeyNames = new string[] { "Id" };
            gDefinedValues.Actions.ShowAdd = true;
            gDefinedValues.Actions.AddClick += gDefinedValues_Add;
            gDefinedValues.GridRebind += gDefinedValues_GridRebind;
            gDefinedValues.GridReorder += gDefinedValues_GridReorder;
            gDefinedValues.Actions.ShowAdd = true;
            gDefinedValues.IsDeleteEnabled = true;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbNotification.Visible = false;

            if ( !Page.IsPostBack )
            {
                ShowDetail();
            }
            else
            {
                ShowDialog();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the lbSaveNew control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSaveNew_Click( object sender, EventArgs e )
        {
            if ( !string.IsNullOrWhiteSpace( tbUserNameNew.Text ) && !string.IsNullOrWhiteSpace( tbPasswordNew.Text ) )
            {
                using ( var rockContext = new RockContext() )
                {
                    var settings = GetSettings( rockContext );
                    SetSettingValue( rockContext, settings, "UserName", tbUserNameNew.Text );
                    SetSettingValue( rockContext, settings, "Password", tbPasswordNew.Text, true );

                    string defaultReturnUrl = string.Format( "{0}Webhooks/ProtectMyMinistry.ashx",
                        GlobalAttributesCache.Value( "PublicApplicationRoot" ) );
                    SetSettingValue( rockContext, settings, "ReturnURL", defaultReturnUrl );

                    rockContext.SaveChanges();

                    BackgroundCheckContainer.Instance.Refresh();

                    ShowView( settings );
                }
            }
            else
            {
                nbNotification.Text = "<p>Username and Password are both required.</p>";
                nbNotification.Visible = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                ShowEdit( GetSettings( rockContext ) );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            if ( !string.IsNullOrWhiteSpace( tbUserName.Text ) && !string.IsNullOrWhiteSpace( tbPassword.Text ) )
            {
                using ( var rockContext = new RockContext() )
                {
                    var settings = GetSettings( rockContext );
                    SetSettingValue( rockContext, settings, "UserName", tbUserName.Text );
                    SetSettingValue( rockContext, settings, "Password", tbPassword.Text, true );
                    SetSettingValue( rockContext, settings, "ReturnURL", urlWebHook.Text );
                    SetSettingValue( rockContext, settings, "Active", cbActive.Checked.ToString() );
                    rockContext.SaveChanges();

                    BackgroundCheckContainer.Instance.Refresh();

                    ShowView( settings );
                }
            }
            else
            {
                nbNotification.Text = "<p>Username and Password are both required.</p>";
                nbNotification.Visible = true;
            }

        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                ShowView( GetSettings( rockContext ) );
            }
        }

        #endregion

        #region Package Grid Events

        /// <summary>
        /// Handles the GridRebind event of the gDefinedValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gDefinedValues_GridRebind( object sender, EventArgs e )
        {
            BindPackageGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the gDefinedValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gDefinedValues_RowSelected( object sender, RowEventArgs e )
        {
            ShowPackageEdit( e.RowKeyId );
        }

        /// <summary>
        /// Handles the GridReorder event of the gDefinedValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gDefinedValues_GridReorder( object sender, GridReorderEventArgs e )
        {
            var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.BACKGROUND_CHECK_TYPES.AsGuid() );
            if ( definedType != null )
            {
                var changedIds = new List<int>();

                using ( var rockContext = new RockContext() )
                {
                    var definedValueService = new DefinedValueService( rockContext );
                    var definedValues = definedValueService.Queryable().Where( a => a.DefinedTypeId == definedType.Id ).Where( a => a.ForeignId == 1 ).OrderBy( a => a.Order ).ThenBy( a => a.Value );
                    changedIds = definedValueService.Reorder( definedValues.ToList(), e.OldIndex, e.NewIndex );
                    rockContext.SaveChanges();
                }
            }

            BindPackageGrid();
        }

        /// <summary>
        /// Handles the Add event of the gDefinedValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gDefinedValues_Add( object sender, EventArgs e )
        {
            ShowPackageEdit( 0 );
        }

        /// <summary>
        /// Handles the Delete event of the gDefinedValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gDefinedValues_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var definedValueService = new DefinedValueService( rockContext );
                var value = definedValueService.Get( e.RowKeyId );
                if ( value != null )
                {
                    string errorMessage;
                    if ( !definedValueService.CanDelete( value, out errorMessage ) )
                    {
                        mdGridWarningValues.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    definedValueService.Delete( value );
                    rockContext.SaveChanges();
                }

                BindPackageGrid();
            }
        }

        protected void dlgPackage_SaveClick( object sender, EventArgs e )
        {
            int definedValueId = hfDefinedValueId.Value.AsInteger();

            var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.BACKGROUND_CHECK_TYPES.AsGuid() );
            if ( definedType != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var service = new DefinedValueService( rockContext );

                    DefinedValue definedValue = null;
                    if ( !definedValueId.Equals( 0 ) )
                    {
                        definedValue = service.Get( definedValueId );
                    }

                    if ( definedValue == null )
                    {
                        definedValue = new DefinedValue();
                        definedValue.DefinedTypeId = definedType.Id;
                        service.Add( definedValue );
                    }

                    definedValue.Value = TYPENAME_PREFIX + tbTitle.Text;
                    definedValue.Description = tbDescription.Text;
                    definedValue.IsActive = true;
                    definedValue.ForeignId = 1;
                    rockContext.SaveChanges();

                    definedValue.LoadAttributes( rockContext );

                    Guid? dvJurisdicationCodeGuid = null;
                    int? dvJurisdictionCodeId = dvpMVRJurisdiction.SelectedValueAsInt();
                    if ( dvJurisdictionCodeId.HasValue && dvJurisdictionCodeId.Value > 0 )
                    {
                        var dvJurisdicationCode = DefinedValueCache.Get( dvJurisdictionCodeId.Value );
                        if ( dvJurisdicationCode != null )
                        {
                            dvJurisdicationCodeGuid = dvJurisdicationCode.Guid;
                        }
                    }

                    definedValue.SetAttributeValue( "PMMPackageName", tbPackageName.Text );
                    definedValue.SetAttributeValue( "DefaultCounty", tbDefaultCounty.Text );
                    definedValue.SetAttributeValue( "SendHomeCounty", cbSendCounty.Checked.ToString() );
                    definedValue.SetAttributeValue( "DefaultState", tbDefaultState.Text );
                    definedValue.SetAttributeValue( "SendHomeState", cbSendState.Checked.ToString() );
                    definedValue.SetAttributeValue( "MVRJurisdiction", dvJurisdicationCodeGuid.HasValue ? dvJurisdicationCodeGuid.Value.ToString() : string.Empty );
                    definedValue.SetAttributeValue( "SendHomeStateMVR", cbSendStateMVR.Checked.ToString() );
                    definedValue.SaveAttributeValues( rockContext );
                }
            }

            BindPackageGrid();
            HideDialog();
        }

        /// <summary>
        /// Handles the Click event of the btnDefault control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDefault_Click( object sender, EventArgs e )
        {
            var bioBlock = BlockCache.Get( Rock.SystemGuid.Block.BIO.AsGuid() );
            // Record an exception if the stock Bio block has been deleted but continue processing
            // the remaining settings.
            if ( bioBlock == null )
            {
                var errorMessage = string.Format( "Stock Bio block ({0}) is missing.", Rock.SystemGuid.Block.BIO );
                ExceptionLogService.LogException( new Exception( errorMessage ) );
            }
            else
            {
                List<Guid> workflowActionGuidList = bioBlock.GetAttributeValues( "WorkflowActions" ).AsGuidList();
                if ( workflowActionGuidList == null || workflowActionGuidList.Count == 0 )
                {
                    // Add to Bio Workflow Actions
                    bioBlock.SetAttributeValue( "WorkflowActions", Rock.SystemGuid.WorkflowType.PROTECTMYMINISTRY );
                    ///BackgroundCheckContainer.Instance.Components
                }
                else
                {
                    //var workflowActionValues = workflowActionValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                    Guid guid = Rock.SystemGuid.WorkflowType.PROTECTMYMINISTRY.AsGuid();
                    if ( !workflowActionGuidList.Any( w => w == guid ) )
                    {
                        // Add Checkr to Bio Workflow Actions
                        workflowActionGuidList.Add( guid );
                    }

                    // Remove PMM from Bio Workflow Actions
                    guid = CheckrSystemGuid.CHECKR_WORKFLOW_TYPE.AsGuid();
                    workflowActionGuidList.RemoveAll( w => w == guid );
                    bioBlock.SetAttributeValue( "WorkflowActions", workflowActionGuidList.AsDelimited( "," ) );
                }

                bioBlock.SaveAttributeValue( "WorkflowActions" );
            }

            string pmmTypeName = ( typeof( Rock.Security.BackgroundCheck.ProtectMyMinistry ) ).FullName;
            var pmmComponent = BackgroundCheckContainer.Instance.Components.Values.FirstOrDefault( c => c.Value.TypeName == pmmTypeName );
            pmmComponent.Value.SetAttributeValue( "Active", "True" );
            pmmComponent.Value.SaveAttributeValue( "Active" );
            // Set as the default provider in the system setting
            SystemSettings.SetValue( Rock.SystemKey.SystemSetting.DEFAULT_BACKGROUND_CHECK_PROVIDER, pmmTypeName );

            using ( var rockContext = new RockContext() )
            {
                WorkflowTypeService workflowTypeService = new WorkflowTypeService( rockContext );
                // Rename PMM Workflow
                var pmmWorkflowAction = workflowTypeService.Get( Rock.SystemGuid.WorkflowType.PROTECTMYMINISTRY.AsGuid() );
                pmmWorkflowAction.Name = "Background Check";

                var checkrWorkflowAction = workflowTypeService.Get( CheckrSystemGuid.CHECKR_WORKFLOW_TYPE.AsGuid() );
                // Rename Checkr Workflow
                checkrWorkflowAction.Name = CheckrConstants.CHECKR_WORKFLOW_TYPE_NAME;

                rockContext.SaveChanges();

                // Enable PMM packages and disable Checkr packages
                DefinedValueService definedValueService = new DefinedValueService( rockContext );
                var packages = definedValueService
                    .GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.BACKGROUND_CHECK_TYPES.AsGuid() )
                    .ToList();

                foreach ( var package in packages )
                {
                    package.IsActive = package.ForeignId == 1;
                }

                rockContext.SaveChanges();
            }

            ShowDetail();
        }

        #endregion

        #endregion

        #region Internal Methods
        /// <summary>
        /// Determines whether PMM is the default provider.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if PMM is the default provider; otherwise, <c>false</c>.
        /// </returns>
        private bool IsDefaultProvider()
        {
            string providerTypeName = ( typeof( Rock.Security.BackgroundCheck.ProtectMyMinistry ) ).FullName;
            string defaultProvider = Rock.Web.SystemSettings.GetValue( SystemSetting.DEFAULT_BACKGROUND_CHECK_PROVIDER ) ?? string.Empty;
            return providerTypeName == defaultProvider;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="restUserId">The rest user identifier.</param>
        public void ShowDetail()
        {
            using ( var rockContext = new RockContext() )
            {
                var settings = GetSettings( rockContext );
                if ( settings != null )
                {
                    if ( IsDefaultProvider() )
                    {
                        btnDefault.Visible = false;
                        lbEdit.Enabled = true;
                    }
                    else
                    {
                        btnDefault.Visible = true;
                        lbEdit.Enabled = false;
                    }

                    string username = GetSettingValue( settings, "UserName" );
                    string password = GetSettingValue( settings, "Password" );
                    if ( !string.IsNullOrWhiteSpace( username ) ||
                        !string.IsNullOrWhiteSpace( password ) )
                    {
                        ShowView( settings );
                    }
                    else
                    {
                        ShowNew();
                    }
                }
                else
                {
                    ShowNew();
                }
            }
        }

        /// <summary>
        /// Shows the new.
        /// </summary>
        public void ShowNew()
        {
            imgPromotion.ImageUrl = PROMOTION_IMAGE_URL;
            hlGetStarted.NavigateUrl = GET_STARTED_URL;

            tbUserNameNew.Text = string.Empty;
            tbPasswordNew.Text = string.Empty;

            pnlNew.Visible = true;
            pnlViewDetails.Visible = false;
            pnlEditDetails.Visible = false;
            pnlPackages.Visible = false;

            HideSecondaryBlocks( true );
        }

        /// <summary>
        /// Shows the view.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public void ShowView( List<AttributeValue> settings )
        {
            lUserName.Text = GetSettingValue( settings, "UserName" );
            lPassword.Text = "********";

            using ( var rockContext = new RockContext() )
            {
                var packages = new DefinedValueService( rockContext )
                    .GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.BACKGROUND_CHECK_TYPES.AsGuid() )
                    .Where( v => v.ForeignId == 1 && v.IsActive && v.Value != null && v.Value.StartsWith( TYPENAME_PREFIX ) )
                    .Select( v => v.Value.Substring( TYPENAME_PREFIX.Length) )
                    .ToList();

                packages.AddRange( new DefinedValueService( rockContext )
                    .GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.BACKGROUND_CHECK_TYPES.AsGuid() )
                    .Where( v => v.ForeignId == 1 && v.IsActive && (v.Value == null || !v.Value.StartsWith( TYPENAME_PREFIX ) ) )
                    .Select( v => v.Value )
                    .ToList() );
                lPackages.Text = packages.AsDelimited( "<br/>" );
            }

            nbSSLWarning.Visible = !GetSettingValue( settings, "ReturnURL" ).StartsWith( "https://" );
            nbSSLWarning.NotificationBoxType = NotificationBoxType.Warning;

            pnlNew.Visible = false;
            pnlViewDetails.Visible = true;
            pnlEditDetails.Visible = false;
            pnlPackages.Visible = false;

            HideSecondaryBlocks( false );
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public void ShowEdit( List<AttributeValue> settings )
        {
            tbUserName.Text = GetSettingValue( settings, "UserName" );
            tbPassword.Text = GetSettingValue( settings, "Password", true );
            urlWebHook.Text = GetSettingValue( settings, "ReturnURL" );
            cbActive.Checked = GetSettingValue( settings, "Active" ).AsBoolean();

            BindPackageGrid();

            pnlNew.Visible = false;
            pnlViewDetails.Visible = false;
            pnlEditDetails.Visible = true;
            pnlPackages.Visible = true;

            HideSecondaryBlocks( true );
        }

        /// <summary>
        /// Binds the package grid.
        /// </summary>
        public void BindPackageGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var definedValues = new DefinedValueService( rockContext )
                    .GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.BACKGROUND_CHECK_TYPES.AsGuid() )
                    .Where( a => a.ForeignId == 1 )
                    .ToList();

                foreach( var definedValue in definedValues )
                {
                    definedValue.LoadAttributes( rockContext );
                }

                gDefinedValues.DataSource = definedValues.Select( v => new
                {
                    v.Id,
                    Value = v.Value.IsNotNullOrWhiteSpace() && v.Value.StartsWith( TYPENAME_PREFIX ) ? v.Value.Substring( TYPENAME_PREFIX.Length ) : v.Value ?? string.Empty,
                    v.Description,
                    PackageName = v.GetAttributeValue( "PMMPackageName" ),
                    DefaultCounty = v.GetAttributeValue( "DefaultCounty" ),
                    SendAddressCounty = v.GetAttributeValue( "SendHomeCounty" ).AsBoolean(),
                    DefaultState = v.GetAttributeValue( "DefaultState" ),
                    SendAddressState = v.GetAttributeValue( "SendHomeState" ).AsBoolean(),
                    MVRJurisdiction = v.GetAttributeValue("MVRJurisdiction"),
                    SendAddressStateMVR = v.GetAttributeValue( "SendHomeStateMVR" ).AsBoolean()
                } )
                .ToList();
                gDefinedValues.DataBind();
            }
        }

        /// <summary>
        /// Shows the package edit.
        /// </summary>
        /// <param name="definedValueId">The defined value identifier.</param>
        public void ShowPackageEdit( int definedValueId )
        {
            var mvrJurisdicationCodes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_MVR_JURISDICTION_CODES.AsGuid() );
            if ( mvrJurisdicationCodes != null )
            {
                dvpMVRJurisdiction.DefinedTypeId = mvrJurisdicationCodes.Id;
            }

            var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.BACKGROUND_CHECK_TYPES.AsGuid() );
            if ( definedType != null )
            {
                DefinedValue definedValue = null;
                if ( !definedValueId.Equals( 0 ) )
                {
                    definedValue = new DefinedValueService( new RockContext() ).Get( definedValueId );
                }

                if ( definedValue != null )
                {
                    hfDefinedValueId.Value = definedValue.Id.ToString();
                    dlgPackage.Title = definedValue.Value.IsNotNullOrWhiteSpace() && definedValue.Value.StartsWith( TYPENAME_PREFIX ) ? definedValue.Value.Substring( TYPENAME_PREFIX.Length ) : definedValue.Value ?? string.Empty;
                }
                else
                {
                    definedValue = new DefinedValue();
                    definedValue.DefinedTypeId = definedType.Id;
                    hfDefinedValueId.Value = string.Empty;
                    dlgPackage.Title = "New Package";
                }

                tbTitle.Text = definedValue.Value.IsNotNullOrWhiteSpace() && definedValue.Value.StartsWith( TYPENAME_PREFIX ) ? definedValue.Value.Substring( TYPENAME_PREFIX.Length ) : definedValue.Value ?? string.Empty;
                tbDescription.Text = definedValue.Description;

                definedValue.LoadAttributes();

                dvpMVRJurisdiction.SetValue( 0 );
                Guid? mvrJurisdictionGuid = definedValue.GetAttributeValue( "MVRJurisdiction" ).AsGuidOrNull();
                if ( mvrJurisdictionGuid.HasValue )
                {
                    var mvrJurisdiction = DefinedValueCache.Get( mvrJurisdictionGuid.Value );
                    if ( mvrJurisdiction != null )
                    {
                        dvpMVRJurisdiction.SetValue( mvrJurisdiction.Id );
                    }
                }

                tbPackageName.Text = definedValue.GetAttributeValue( "PMMPackageName" );
                tbDefaultCounty.Text = definedValue.GetAttributeValue( "DefaultCounty" );
                cbSendCounty.Checked = definedValue.GetAttributeValue( "SendHomeCounty" ).AsBoolean();
                tbDefaultState.Text = definedValue.GetAttributeValue( "DefaultState" );
                cbSendState.Checked = definedValue.GetAttributeValue( "SendHomeState" ).AsBoolean();
                cbSendStateMVR.Checked = definedValue.GetAttributeValue( "SendHomeStateMVR" ).AsBoolean();

                ShowDialog( "Package" );
            }
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        private void ShowDialog( string dialog )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog();
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        private void ShowDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "PACKAGE":
                    dlgPackage.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "PACKAGE":
                    dlgPackage.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<AttributeValue> GetSettings( RockContext rockContext )
        {
            var pmmEntityType = EntityTypeCache.Get( typeof( Rock.Security.BackgroundCheck.ProtectMyMinistry ) );
            if ( pmmEntityType != null )
            {
                var service = new AttributeValueService( rockContext );
                return service.Queryable( "Attribute" )
                    .Where( v => v.Attribute.EntityTypeId == pmmEntityType.Id )
                    .ToList();
            }

            return null;
        }

        /// <summary>
        /// Gets the setting value.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string GetSettingValue( List<AttributeValue> values, string key, bool encryptedValue = false )
        {
            string value = values
                .Where( v => v.AttributeKey == key )
                .Select( v => v.Value )
                .FirstOrDefault();
            if ( encryptedValue && !string.IsNullOrWhiteSpace( value ))
            {
                try { value = Encryption.DecryptString( value ); }
                catch { }
            }

            return value;
        }

        /// <summary>
        /// Sets the setting value.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="values">The values.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        private void SetSettingValue( RockContext rockContext, List<AttributeValue> values, string key, string value, bool encryptValue = false )
        {
            if ( encryptValue && !string.IsNullOrWhiteSpace( value ) )
            {
                try { value = Encryption.EncryptString( value ); }
                catch { }
            }

            var attributeValue = values
                .Where( v => v.AttributeKey == key )
                .FirstOrDefault();
            if ( attributeValue != null )
            {
                attributeValue.Value = value;
            }
            else
            {
                var pmmEntityType = EntityTypeCache.Get( typeof( Rock.Security.BackgroundCheck.ProtectMyMinistry ) );
                if ( pmmEntityType != null )
                {
                    var attribute = new AttributeService( rockContext )
                        .Queryable()
                        .Where( a =>
                            a.EntityTypeId == pmmEntityType.Id &&
                            a.Key == key
                        )
                        .FirstOrDefault();

                    if ( attribute != null )
                    {
                        attributeValue = new AttributeValue();
                        new AttributeValueService( rockContext ).Add( attributeValue );
                        attributeValue.AttributeId = attribute.Id;
                        attributeValue.Value = value;
                        attributeValue.EntityId = 0;
                    }
                }
            }

        }

        #endregion

    }
}