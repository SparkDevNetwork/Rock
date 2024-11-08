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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;
using System.Text;

namespace RockWeb.Blocks.Fundraising
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Fundraising List" )]
    [Category( "Fundraising" )]
    [Description( "Lists Fundraising Opportunities (Groups) that have the ShowPublic attribute set to true" )]

    [DefinedValueField( "53C8FFF6-3022-4A2D-9BAE-FD3435BEA43D", "Fundraising Opportunity Types", "Select which opportunity types are shown, or leave blank to show all", false, true, order:1 )]
    [LinkedPage( "Details Page", required: false, order: 2 )]
    [CodeEditorField( "Lava Template", "The lava template to use for the results", CodeEditorMode.Lava, CodeEditorTheme.Rock, defaultValue:
@"{% include '~~/Assets/Lava/FundraisingList.lava' %}", order: 3 )]
    [Rock.SystemGuid.BlockTypeGuid( "E664BB02-D501-40B0-AAD6-D8FA0E63438B" )]
    public partial class FundraisingList : RockBlock
    {
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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowView();
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
            ShowView();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void ShowView()
        {
            RockContext rockContext = new RockContext();
            GroupService groupService = new GroupService( rockContext );
            var groupTypeIdFundraising = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FUNDRAISINGOPPORTUNITY.AsGuid() ).Id;
            var fundraisingGroupTypeIdList = new GroupTypeService( rockContext ).Queryable().Where( a => a.Id == groupTypeIdFundraising || a.InheritedGroupTypeId == groupTypeIdFundraising ).Select( a => a.Id ).ToList();

            var fundraisingGroupList = groupService.Queryable()
                .Where( a =>
                            a.IsActive &&
                            fundraisingGroupTypeIdList.Contains( a.GroupTypeId ) )
                .WhereAttributeValue( rockContext, "ShowPublic", true.ToString() ).ToList();

            foreach ( var group in fundraisingGroupList )
            {
                group.LoadAttributes( rockContext );
            }

            var fundraisingOpportunityTypes = this.GetAttributeValue( "FundraisingOpportunityTypes" ).SplitDelimitedValues().AsGuidList();
            if ( fundraisingOpportunityTypes.Any() )
            {
                fundraisingGroupList = fundraisingGroupList.Where( a => fundraisingOpportunityTypes.Contains( a.GetAttributeValue( "OpportunityType" ).AsGuid() ) ).ToList();
            }

            fundraisingGroupList = fundraisingGroupList.OrderBy( g => DateRange.FromDelimitedValues( g.GetAttributeValue( "OpportunityDateRange" ) ).Start ).ThenBy( g => g.GetAttributeValue( "Opportunity Title" ) ).ToList();

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions() );
            mergeFields.Add( "GroupList", fundraisingGroupList );
            mergeFields.Add( "DetailsPage", LinkedPageRoute( "DetailsPage" ) );

            string lavaTemplate = this.GetAttributeValue( "LavaTemplate" );
            lViewHtml.Text = lavaTemplate.ResolveMergeFields( mergeFields );
        }

        #endregion
    }
}