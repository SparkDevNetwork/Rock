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

using System.ComponentModel;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.ViewModels.Blocks.Finance.FundraisingList;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays a list of groups.
    /// </summary>
    [DisplayName( "Fundraising List" )]
    [Category( "Fundraising" )]
    [Description( "Lists Fundraising Opportunities (Groups) that have the ShowPublic attribute set to true" )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( SiteType.Web )]

    [DefinedValueField( "Fundraising Opportunity Types",
        DefinedTypeGuid = "53C8FFF6-3022-4A2D-9BAE-FD3435BEA43D",
        Description = "Select which opportunity types are shown, or leave blank to show all",
        IsRequired = false,
        AllowMultiple = true,
        Order = 1,
        Key = AttributeKey.FundraisingOpportunityTypes )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the group details.",
        Order = 2,
        Key = AttributeKey.DetailPage )]

    [CodeEditorField( "Lava Template",
        Description = "The lava template to use for the results",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        DefaultValue = @"{% include '~~/Assets/Lava/FundraisingList.lava' %}",
        Order = 3,
        Key = AttributeKey.LavaTemplate )]

    [SystemGuid.EntityTypeGuid( "ff4f82ec-2f12-4d60-90b9-6ccc7855b4b3" )]
    [SystemGuid.BlockTypeGuid( "699ed6d1-e23a-4757-a0a2-83c5406b658a" )]
    [CustomizedGrid]
    public class FundraisingList : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string FundraisingOpportunityTypes = "FundraisingOpportunityTypes";
            public const string LavaTemplate = "LavaTemplate";
            public const string OpportunityType = "OpportunityType";
            public const string OpportunityDateRange = "OpportunityDateRange";
            public const string ShowPublic = "ShowPublic";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new FundraisingListInitializationBox()
            {
                BlockContent = GetBlockContent()
            };

            return box;
        }

        /// <summary>
        /// Gets the content of the block.
        /// </summary>
        /// <returns></returns>
        private string GetBlockContent()
        {
            GroupService groupService = new GroupService( RockContext );
            var groupTypeIdFundraising = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FUNDRAISINGOPPORTUNITY.AsGuid() ).Id;
            var fundraisingGroupTypeIdList = new GroupTypeService( RockContext ).Queryable()
                .Where( a => a.Id == groupTypeIdFundraising || a.InheritedGroupTypeId == groupTypeIdFundraising )
                .Select( a => a.Id ).ToList();

            var fundraisingGroupList = groupService.Queryable()
                .Where( a =>
                            a.IsActive &&
                            fundraisingGroupTypeIdList.Contains( a.GroupTypeId ) )
                .WhereAttributeValue( RockContext, AttributeKey.ShowPublic, true.ToString() ).ToList();

            foreach ( var group in fundraisingGroupList )
            {
                group.LoadAttributes( RockContext );
            }

            var fundraisingOpportunityTypes = this.GetAttributeValue( AttributeKey.FundraisingOpportunityTypes ).SplitDelimitedValues().AsGuidList();
            if ( fundraisingOpportunityTypes.Any() )
            {
                fundraisingGroupList = fundraisingGroupList.Where( a => fundraisingOpportunityTypes.Contains( a.GetAttributeValue( AttributeKey.OpportunityType ).AsGuid() ) ).ToList();
            }

            fundraisingGroupList = fundraisingGroupList.OrderBy( g => DateRange.FromDelimitedValues( g.GetAttributeValue( AttributeKey.OpportunityDateRange ) ).Start )
                .ThenBy( g => g.GetAttributeValue( "Opportunity Title" ) )
                .ToList();

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, GetCurrentPerson(), new Rock.Lava.CommonMergeFieldsOptions() );
            mergeFields.Add( "GroupList", fundraisingGroupList );
            mergeFields.Add( "DetailsPage", this.GetLinkedPageUrl( AttributeKey.DetailPage ) );

            var lavaTemplate = this.GetAttributeValue( AttributeKey.LavaTemplate );

            return lavaTemplate.ResolveMergeFields( mergeFields );
        }

        #endregion
    }
}
