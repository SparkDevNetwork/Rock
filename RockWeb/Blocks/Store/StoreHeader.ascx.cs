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
using System.ComponentModel;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Store;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Store
{
    [DisplayName( "Store Header" )]
    [Category( "Store" )]
    [Description( "Shows the Organization information used by the Rock Shop." )]
    [CodeEditorField( "Lava Template",
        Description = "Lava template to use to display the packages",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = @"{% include '~/Assets/Lava/Store/StoreHeader.lava' %}",
        Order = 1,
        Key = AttributeKey.LavaTemplate )]

    [LinkedPage( "Link Organization Page",
        Description = "Page to allow the user to link an organization to the store.",
        IsRequired = false,
        Key = AttributeKey.LinkOrganizationPage )]
    [Rock.SystemGuid.BlockTypeGuid( "91355804-4B64-434F-949B-6180E5CC31D9" )]
    public partial class StoreHeader : Rock.Web.UI.RockBlock
    {
        private static class AttributeKey
        {
            public const string LavaTemplate = "LavaTemplate";
            public const string LinkOrganizationPage = "LinkOrganizationPage";
        }

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                LoadBlock();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadBlock();
        }

        #endregion

        #region Methods

        private void LoadBlock()
        {
            var storeOrganizationKey = StoreService.GetOrganizationKey();

            if ( storeOrganizationKey.IsNullOrWhiteSpace() )
            {
                pnlConfigureOrganization.Visible = true;
                litOrganizationLava.Visible = false;
            }
            else
            {
                SetOrganizationLava( storeOrganizationKey );
            }
        }

        /// <summary>
        /// Sets the organization lava.
        /// </summary>
        private void SetOrganizationLava( string storeOrganizationKey )
        {
            var organizationResult = new OrganizationService().GetOrganization( storeOrganizationKey );

            if ( organizationResult.Result == null || organizationResult.Result.AverageWeeklyAttendance == 0 )
            {
                pnlConfigureOrganization.Visible = true;
                litOrganizationLava.Visible = false;
                return;
            }

            pnlConfigureOrganization.Visible = false;
            litOrganizationLava.Visible = true;

            // TODO: We will need to call the spark dev network api to get this data based on the storeOrganizationKey.
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
            mergeFields.Add( "Organization", organizationResult.Result );

            var rssTemplate = GetAttributeValue( AttributeKey.LavaTemplate );
            var outputContent = rssTemplate.ResolveMergeFields( mergeFields );

            litOrganizationLava.Text = outputContent;
        }
        #endregion

        protected void btnConfigureRockShop_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.LinkOrganizationPage );
        }
    }
}