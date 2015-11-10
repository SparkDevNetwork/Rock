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
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Security.BackgroundCheck
{
    [DisplayName( "Protect My Ministry Settings" )]
    [Category( "Security > Background Check" )]
    [Description( "Block for updating the settings used by the Protect My Ministry integration." )]

    public partial class ProtectMyMinistrySettings : Rock.Web.UI.RockBlock
    {
        private const string GET_STARTED_URL = "http://www.rockrms.com/Redirect/PMMSignup";
        private const string PROMOTION_IMAGE_URL = "https://rockrms.blob.core.windows.net/resources/pmm-integration/pmm-integration-banner.png";

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
            base.OnLoad( e );

            nbNotification.Visible = false;

            if ( !Page.IsPostBack )
            {
                ShowDetail();
            }
            else
            {
                ShowDialog();
            }
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
                    SetSettingValue( rockContext, settings, "Password", tbPasswordNew.Text );
                    rockContext.SaveChanges();

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
                    SetSettingValue( rockContext, settings, "Password", tbPassword.Text );
                    SetSettingValue( rockContext, settings, "TestMode", cbTestMode.Checked.ToString() );
                    SetSettingValue( rockContext, settings, "Active", cbActive.Checked.ToString() );
                    rockContext.SaveChanges();

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
            var definedType = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_PACKAGES.AsGuid() );
            if ( definedType != null )
            {
                var changedIds = new List<int>();

                using ( var rockContext = new RockContext() )
                {
                    var definedValueService = new DefinedValueService( rockContext );
                    var definedValues = definedValueService.Queryable().Where( a => a.DefinedTypeId == definedType.Id ).OrderBy( a => a.Order ).ThenBy( a => a.Value );
                    changedIds = definedValueService.Reorder( definedValues.ToList(), e.OldIndex, e.NewIndex );
                    rockContext.SaveChanges();
                }

                DefinedTypeCache.Flush( definedType.Id );
                foreach ( int id in changedIds )
                {
                    Rock.Web.Cache.DefinedValueCache.Flush( id );
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

                    DefinedTypeCache.Flush( value.DefinedTypeId );
                    DefinedValueCache.Flush( value.Id );
                }

                BindPackageGrid();
            }
        }

        protected void dlgPackage_SaveClick( object sender, EventArgs e )
        {
            int definedValueId = hfDefinedValueId.Value.AsInteger();

            var definedType = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_PACKAGES.AsGuid() );
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

                    definedValue.Value = tbTitle.Text;
                    definedValue.Description = tbDescription.Text;
                    rockContext.SaveChanges();

                    definedValue.LoadAttributes( rockContext );

                    definedValue.SetAttributeValue( "PMMPackageName", tbPackageName.Text );
                    definedValue.SetAttributeValue( "DefaultCounty", tbDefaultCounty.Text );
                    definedValue.SetAttributeValue( "SendHomeCounty", cbSendCounty.Checked.ToString() );
                    definedValue.SetAttributeValue( "DefaultState", tbDefaultState.Text );
                    definedValue.SetAttributeValue( "SendHomeState", cbSendState.Checked.ToString() );
                    definedValue.SetAttributeValue( "IncludeMVR", cbIncludeMVR.Checked.ToString() );
                    definedValue.SaveAttributeValues( rockContext );

                    DefinedTypeCache.Flush( definedType.Id );
                    DefinedValueCache.Flush( definedValue.Id );
                }
            }

            BindPackageGrid();

            HideDialog();

        }

        #endregion

        #endregion

        #region Internal Methods

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
                    string username = GetSettingValue( settings, "UserName" );
                    string password = GetSettingValue( settings, "Password" );

                    if ( !string.IsNullOrWhiteSpace( username ) ||
                        !string.IsNullOrWhiteSpace( password ))
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
            hlMode.Visible = false;
            hlActive.Visible = false;

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
            ShowHighlightLabels( settings );

            lUserName.Text = GetSettingValue( settings, "UserName" );
            lPassword.Text = "********";

            using ( var rockContext = new RockContext() )
            {
                var packages = new DefinedValueService( rockContext )
                    .GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_PACKAGES.AsGuid() )
                    .Select( v => v.Value )
                    .ToList();
                lPackages.Text = packages.AsDelimited( "<br/>" );
            }

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
            ShowHighlightLabels( settings );

            tbUserName.Text = GetSettingValue( settings, "UserName" );
            tbPassword.Text = GetSettingValue( settings, "Password" );
            cbActive.Checked = GetSettingValue( settings, "Active" ).AsBoolean();
            cbTestMode.Checked = GetSettingValue( settings, "TestMode" ).AsBoolean();

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
                    .GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_PACKAGES.AsGuid() )
                    .ToList();

                foreach( var definedValue in definedValues )
                {
                    definedValue.LoadAttributes( rockContext );
                }

                gDefinedValues.DataSource = definedValues.Select( v => new
                {
                    v.Id,
                    v.Value,
                    v.Description,
                    PackageName = v.GetAttributeValue( "PMMPackageName" ),
                    DefaultCounty = v.GetAttributeValue( "DefaultCounty" ),
                    SendAddressCounty = v.GetAttributeValue( "SendHomeCounty" ).AsBoolean(),
                    DefaultState = v.GetAttributeValue( "DefaultState" ),
                    SendAddressState = v.GetAttributeValue( "SendHomeState" ).AsBoolean(),
                    IncludeMVRInfo = v.GetAttributeValue( "IncludeMVR" ).AsBoolean()
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
            var definedType = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_PACKAGES.AsGuid() );
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
                    dlgPackage.Title = definedValue.Value;
                }
                else
                {
                    definedValue = new DefinedValue();
                    definedValue.DefinedTypeId = definedType.Id;
                    hfDefinedValueId.Value = string.Empty;
                    dlgPackage.Title = "New Package";
                }

                tbTitle.Text = definedValue.Value;
                tbDescription.Text = definedValue.Description;

                definedValue.LoadAttributes();

                tbPackageName.Text = definedValue.GetAttributeValue( "PMMPackageName" );
                tbDefaultCounty.Text = definedValue.GetAttributeValue( "DefaultCounty" );
                cbSendCounty.Checked = definedValue.GetAttributeValue( "SendHomeCounty" ).AsBoolean();
                tbDefaultState.Text = definedValue.GetAttributeValue( "DefaultState" );
                cbSendState.Checked = definedValue.GetAttributeValue( "SendHomeState" ).AsBoolean();
                cbIncludeMVR.Checked = definedValue.GetAttributeValue( "IncludeMVR" ).AsBoolean( false );

                ShowDialog( "Package" );
            }
        }

        /// <summary>
        /// Shows the highlight labels.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public void ShowHighlightLabels( List<AttributeValue> settings )
        {
            bool testMode = GetSettingValue( settings, "TestMode" ).AsBoolean();
            hlMode.LabelType = testMode ? LabelType.Primary : LabelType.Success;
            hlMode.Text = testMode ? "In Test Mode" : "In Live Mode";
            hlMode.Visible = true;

            bool active = GetSettingValue( settings, "Active" ).AsBoolean();
            hlActive.LabelType = active ? LabelType.Success : LabelType.Danger;
            hlActive.Text = active ? "Active" : "Inactive";
            hlActive.Visible = true;
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
            var pmmEntityType = EntityTypeCache.Read( typeof( Rock.Security.BackgroundCheck.ProtectMyMinistry ) );
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
        private string GetSettingValue( List<AttributeValue> values, string key )
        {
            return values
                .Where( v => v.AttributeKey == key )
                .Select( v => v.Value )
                .FirstOrDefault();
        }

        /// <summary>
        /// Sets the setting value.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="values">The values.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        private void SetSettingValue( RockContext rockContext, List<AttributeValue> values, string key, string value )
        {
            var attributeValue = values
                .Where( v => v.AttributeKey == key )
                .FirstOrDefault();
            if ( attributeValue != null )
            {
                attributeValue.Value = value;
            }
            else
            {
                var pmmEntityType = EntityTypeCache.Read( typeof( Rock.Security.BackgroundCheck.ProtectMyMinistry ) );
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