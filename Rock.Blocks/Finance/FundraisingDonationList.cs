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

using Rock.Attribute;
using Rock.Blocks.Types.Mobile.Crm;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Fundraising.FundraisingDonationList;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Fundraising
{
    /// <summary>
    /// Displays a list of financial transaction details.
    /// </summary>
    [DisplayName( "Fundraising Donation List" )]
    [Category( "Fundraising" )]
    [Description( "Lists donations in a grid for the current fundraising opportunity or participant." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [CustomCheckboxListField( "Hide Grid Columns",
        Key = AttributeKey.HideGridColumns,
        Description = "The grid columns that should be hidden from the user.",
        ListSource = "Amount, Donor Address, Donor Email, Participant",
        IsRequired = false,
        DefaultValue = "",
        Category = "Advanced",
        Order = 0 )]
    [CustomCheckboxListField( "Hide Grid Actions",
        Key = AttributeKey.HideGridActions,
        Description = "The grid actions that should be hidden from the user.",
        ListSource = "Communicate, Merge Person, Bulk Update, Excel Export, Merge Template",
        IsRequired = false,
        DefaultValue = "",
        Category = "Advanced",
        Order = 1 )]
    [CodeEditorField( "Donor Column",
        Key = AttributeKey.DonorColumn,
        Description = "The value that should be displayed for the Donor column. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = true,
        DefaultValue = @"<a href=""/Person/{{ Donor.Id }}"">{{ Donor.FullName }}</a>",
        Category = "Advanced",
        Order = 2 )]
    [CodeEditorField( "Participant Column",
        Key = AttributeKey.ParticipantColumn,
        Description = "The value that should be displayed for the Participant column. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = true,
        DefaultValue = @"<a href=""/Person/{{ Participant.PersonId }}"" class=""pull-right margin-l-sm btn btn-sm btn-default"">
    <i class=""fa fa-user""></i>
</a>
<a href=""/GroupMember/{{ Participant.Id }}"">{{ Participant.Person.FullName }}</a>",
        Category = "Advanced",
        Order = 3 )]
    [SystemGuid.EntityTypeGuid( "b80410e7-53d7-4ab1-8b17-39ff8b3e708f" )]
    [SystemGuid.BlockTypeGuid( "054a8469-a838-4708-b18f-9f2819346298" )]
    [CustomizedGrid]
    [ContextAware]
    public class FundraisingDonationList : RockEntityListBlockType<FinancialTransactionDetail>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string HideGridColumns = "HideGridColumns";
            public const string HideGridActions = "HideGridActions";
            public const string DonorColumn = "DonorColumn";
            public const string ParticipantColumn = "ParticipantColumn";
        }

        #endregion Keys

        #region Fields

        private Dictionary<int, GroupMember> _groupMembers;
        private Rock.Model.Group _group;

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<FundraisingDonationListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private FundraisingDonationListOptionsBag GetBoxOptions()
        {
            var currencyInfo = new RockCurrencyCodeInfo();
            var options = new FundraisingDonationListOptionsBag()
            {
                ColumnsToHide = GetAttributeValue( AttributeKey.HideGridColumns ).Split( ',' ).ToList(),
                ActionsToHide = GetAttributeValue( AttributeKey.HideGridActions ).Split( ',' ).ToList(),
                IsContextEntityGroupMember = RequestContext.GetContextEntity<GroupMember>() != null,
                IsBlockVisible = IsContextGroupFundraisingGroupType(),
                CurrencyInfo = new ViewModels.Utility.CurrencyInfoBag
                {
                    Symbol = currencyInfo.Symbol,
                    DecimalPlaces = currencyInfo.DecimalPlaces,
                    SymbolLocation = currencyInfo.SymbolLocation
                }
            };
            return options;
        }

        /// <inheritdoc/>
        protected override IQueryable<FinancialTransactionDetail> GetListQueryable( RockContext rockContext )
        {
            var groupMemberService = new GroupMemberService( rockContext );
            var financialTransactionDetailService = new FinancialTransactionDetailService( rockContext );
            var entityTypeIdGroupMember = EntityTypeCache.GetId<GroupMember>();
            List<int> groupMemberIds = new List<int>();

            //
            // Get the donations for the entire opportunity group or for just the
            // one individual being viewed.
            //
            if ( RequestContext.GetContextEntity<Rock.Model.Group>() != null )
            {
                _group = RequestContext.GetContextEntity<Rock.Model.Group>();

                groupMemberIds = groupMemberService.Queryable()
                    .Where( m => m.GroupId == _group.Id )
                    .Select( m => m.Id )
                    .ToList();
            }
            else
            {
                var groupMember = RequestContext.GetContextEntity<GroupMember>();
                if ( groupMember != null )
                {
                    _group = groupMember.Group;
                    groupMemberIds = new List<int> { groupMember.Id };
                }
            }

            var queryable = financialTransactionDetailService.Queryable()
                .Where( d => d.EntityTypeId == entityTypeIdGroupMember && groupMemberIds.Contains( d.EntityId.Value ) );

            return queryable;
        }

        /// <inheritdoc/>
        protected override IQueryable<FinancialTransactionDetail> GetOrderedListQueryable( IQueryable<FinancialTransactionDetail> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( f => f.Transaction.AuthorizedPersonAlias.Person.LastName )
                .ThenBy( f => f.Transaction.AuthorizedPersonAlias.Person.NickName );
        }

        /// <inheritdoc/>
        protected override List<FinancialTransactionDetail> GetListItems( IQueryable<FinancialTransactionDetail> queryable, RockContext rockContext )
        {
            //
            // Get the donations for the entire opportunity group or for just the
            // one individual being viewed.
            //
            if ( RequestContext.GetContextEntity<Rock.Model.Group>() != null )
            {
                var group = RequestContext.GetContextEntity<Rock.Model.Group>();

                _groupMembers = new GroupMemberService( rockContext ).Queryable()
                    .Where( m => m.GroupId == group.Id )
                    .ToDictionary( m => m.Id );
            }
            else
            {
                var groupMember = RequestContext.GetContextEntity<GroupMember>();
                if ( groupMember != null )
                {
                    _groupMembers = new Dictionary<int, GroupMember> { { groupMember.Id, groupMember } };
                }
            }

            return base.GetListItems( queryable, rockContext );
        }

        /// <inheritdoc/>
        protected override GridBuilder<FinancialTransactionDetail> GetGridBuilder()
        {
            return new GridBuilder<FinancialTransactionDetail>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "donorIdKey", a => a.Transaction.AuthorizedPersonAlias.Person.IdKey )
                .AddTextField( "donor", a => GetDonorText( a ) )
                .AddTextField( "donorEmail", a => a.Transaction.AuthorizedPersonAlias.Person.Email )
                .AddTextField( "participant", a => GetParticipantText( a ) )
                .AddField( "amount", a => a.Amount )
                .AddTextField( "donorAddress", a => a.Transaction.AuthorizedPersonAlias.Person.GetHomeLocation().ToStringSafe().ConvertCrLfToHtmlBr() )
                .AddDateTimeField( "date", a => a.Transaction.TransactionDateTime );
        }

        /// <summary>
        /// Gets the resolved configured text to be displayed in the participant column.
        /// </summary>
        /// <param name="transactionDetail">The transaction detail.</param>
        /// <returns></returns>
        private string GetDonorText( FinancialTransactionDetail transactionDetail )
        {
            var mergeFields = GetMergeFields( transactionDetail );
            var donorText = GetAttributeValue( AttributeKey.DonorColumn ).ResolveMergeFields( mergeFields );
            return donorText;
        }

        /// <summary>
        /// Gets the resolved configured text to be displayed in the donor column.
        /// </summary>
        /// <param name="transactionDetail">The transaction detail.</param>
        /// <returns></returns>
        private string GetParticipantText( FinancialTransactionDetail transactionDetail )
        {
            var mergeFields = GetMergeFields( transactionDetail );
            var participantText = GetAttributeValue( AttributeKey.ParticipantColumn ).ResolveMergeFields( mergeFields );
            return participantText;
        }

        /// <summary>
        /// Gets the merge fields.
        /// </summary>
        /// <param name="transactionDetail">The transaction detail.</param>
        /// <returns></returns>
        private Dictionary<string, object> GetMergeFields( FinancialTransactionDetail transactionDetail )
        {
            var mergeFields = RequestContext.GetCommonMergeFields( GetCurrentPerson() );

            mergeFields.AddOrReplace( "Group", GetContextEntityGroup() );
            mergeFields.AddOrReplace( "Donor", transactionDetail.Transaction.AuthorizedPersonAlias.Person );

            if ( _groupMembers.TryGetValue( transactionDetail.EntityId.Value, out GroupMember groupMember ) )
            {
                mergeFields.AddOrReplace( "Participant", groupMember );
            }

            return mergeFields;
        }

        /// <summary>
        /// Gets the group from the current context entity, if the context entity is a Group Member their group is returned.
        /// </summary>
        /// <returns></returns>
        private Rock.Model.Group GetContextEntityGroup()
        {
            if ( _group == null )
            {
                if ( RequestContext.GetContextEntity<Rock.Model.Group>() != null )
                {
                    _group = RequestContext.GetContextEntity<Rock.Model.Group>();
                }
                else
                {
                    var groupMember = RequestContext.GetContextEntity<GroupMember>();
                    _group = groupMember?.Group;
                }
            }

            return _group;
        }

        /// <summary>
        /// Determines whether [is context group fundraising group type].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is context group fundraising group type]; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private bool IsContextGroupFundraisingGroupType()
        {
            var rockContext = new RockContext();
            var group = GetContextEntityGroup();
            var groupTypeIdFundraising = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FUNDRAISINGOPPORTUNITY.AsGuid() ).Id;
            var fundraisingGroupTypeIdList = new GroupTypeService( rockContext ).Queryable()
                .Where( a => a.Id == groupTypeIdFundraising || a.InheritedGroupTypeId == groupTypeIdFundraising )
                .Select( a => a.Id )
                .ToList();

            return group != null && fundraisingGroupTypeIdList.Contains( group.GroupTypeId );
        }

        #endregion Methods
    }
}