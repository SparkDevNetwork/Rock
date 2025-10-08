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
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Rock.Attribute;
using Rock.Common.Mobile;
using Rock.Common.Mobile.Blocks.Finance.FinancialBatchList;
using Rock.Common.Mobile.ViewModel;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Finance
{
    /// <summary>
    /// The Rock Mobile Financial Batch List block, used to display
    /// </summary>
    [DisplayName( "Financial Batch List" )]
    [Category( "Mobile > Finance" )]
    [Description( "The Financial Batch List block." )]
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [CodeEditorField( "Item Template",
        Key = AttributeKeys.ResultContent,
        Description = "The template used to display the financial batch results.",
        IsRequired = false,
        DefaultValue = @"
<Rock:StyledBorder StyleClass=""border, border-interface-soft, rounded, bg-interface-softest, p-16, mb-8"">
    <Grid ColumnDefinitions=""Auto, Auto, *, Auto"" 
        RowDefinitions=""Auto, Auto, Auto""
        StyleClass=""gap-column-8"" >
        <Label StyleClass=""body, bold, text-interface-strongest, mb-8"" 
            Grid.Row=""0""
            Grid.Column=""0""
            Text=""{{ FinancialBatch.Name }}"" />
        
        <Label StyleClass=""footnote""  
            Grid.Row=""1""
            Grid.ColumnSpan=""3""
            Text=""Batch Date: {{ FinancialBatch.BatchStartDateTime | Date:'MMM dd, yyyy' }}"" />
        
        <Label StyleClass=""footnote"" 
            Grid.Row=""2""
            Text=""{{ FinancialBatch.Status }}"" />
        
        <Rock:Icon 
            Grid.Column=""3""
            Grid.RowSpan=""3""
            VerticalOptions=""Center""
            StyleClass=""footnote""
            IconClass=""chevron-right"" />
    </Grid>
    
    <Rock:StyledBorder.Behaviors>
        <Rock:TouchBehavior 
            PressedOpacity=""0.6"" 
            DefaultOpacity=""1"" 
            HoveredOpacity=""0.6"" 
            Command=""{Binding PushPage}"" 
            CommandParameter=""{{ DetailPage }}?FinancialBatch={{ FinancialBatchIdKey }}"" />
    </Rock:StyledBorder.Behaviors>
</Rock:StyledBorder>",
        Order = 1 )]

    [CustomCheckboxListField( "Status",
        Description = "Filter the batch based on their status.",
        ListSource = @"
        Pending^Pending,
        Open^Open,
        Closed^Closed",
        Key = AttributeKeys.BatchStatus,
        DefaultValue = "Pending,Open,Closed",
        RepeatDirection = System.Web.UI.WebControls.RepeatDirection.Vertical,
        Order = 2 )]

    [BooleanField( "Allow Add",
        Description = "whether a new financial batch can be added.",
        IsRequired = false,
        DefaultBooleanValue = true,
        Key = AttributeKeys.AllowAdd,
        Order = 3 )]

    [LinkedPage( "Detail Page",
        Description = "The page to link to when user taps on a Financial Batch List. FinancialBatchGuid is passed in the query string.",
        IsRequired = false,
        Key = AttributeKeys.DetailPage,
        Order = 4 )]

    [BooleanField( "Allow Filter By Campus",
        Description = "Whether or not filtering by campus should be enabled or not.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Key = AttributeKeys.AllowFilterByCampus,
        Order = 5 )]

    [DefinedValueField( "Display Campus Types",
        Description = "The campus types to filter to",
        DefinedTypeGuid = SystemGuid.DefinedType.CAMPUS_TYPE,
        IsRequired = true,
        DefaultValue = SystemGuid.DefinedValue.CAMPUS_TYPE_PHYSICAL,
        AllowMultiple = true,
        Key = AttributeKeys.DisplayCampusTypes,
        Order = 6 )]

    [DefinedValueField( "Display Campus Statuses",
        Description = "The campus statuses to filter to.",
        DefinedTypeGuid = SystemGuid.DefinedType.CAMPUS_STATUS,
        IsRequired = true,
        DefaultValue = SystemGuid.DefinedValue.CAMPUS_STATUS_OPEN,
        AllowMultiple = true,
        Key = AttributeKeys.DisplayCampusStatuses,
        Order = 7 )]

    [IntegerField( "Page Load Size",
        Description = "Determines the amount of batches to show in the initial page load and when scrolling to load more.",
        IsRequired = true,
        DefaultIntegerValue = 100,
        Key = AttributeKeys.PageLoadSize,
        Order = 8 )]

    [MobileNavigationActionField( "Post Batch Save Action",
        Description = "The navigation action to perform when you save a batch",
        IsRequired = false,
        DefaultValue = MobileNavigationActionFieldAttribute.PopSinglePageValue,
        Key = AttributeKeys.PostBatchSaveAction,
        Order = 0 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_FINANCE_FINANCIAL_BATCH_LIST_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_FINANCE_FINANCIAL_BATCH_LIST )]
    public class FinancialBatchList : RockBlockType
    {
        #region Fields

        /// <summary>
        /// Gets the financial batch list template.
        /// </summary>
        private string FinancialBatchListTemplate => GetAttributeValue( AttributeKeys.ResultContent );

        /// <summary>
        /// Gets the rock context for the block.
        /// </summary>
        public List<Guid> DisplayCampusStatusGuids => GetAttributeValue( AttributeKeys.DisplayCampusStatuses ).SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// Gets the display campus type guids.
        /// </summary>
        public List<Guid> DisplayCampusTypeGuids => GetAttributeValue( AttributeKeys.DisplayCampusTypes ).SplitDelimitedValues().AsGuidList();

        #endregion

        #region Attribute Keys

        /// <summary>
        /// Keys for the block attributes.
        /// </summary>
        private static class AttributeKeys
        {
            public const string ResultContent = "HeaderContent";
            public const string BatchStatus = "BatchStatus";
            public const string AllowAdd = "AllowAdd";
            public const string AllowFilterByCampus = "AllowFilterByCampus";
            public const string DetailPage = "DetailPage";
            public const string DisplayCampusTypes = "DisplayCampusTypes";
            public const string DisplayCampusStatuses = "DisplayCampusStatuses";
            public const string PageLoadSize = "PageLoadSize";
            public const string PostBatchSaveAction = "PostBatchSaveAction";
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the add button should be enabled or not.
        /// </summary>
        /// <returns></returns>
        private bool GetIsAddEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the list of campuses that should be displayed to the user to pick from.
        /// </summary>
        /// <returns>
        /// A list of <see cref="MobileCampus" /> objects.
        /// </returns>
        private List<MobileCampus> GetDisplayCampuses()
        {
            // Get the campus status identifiers that will be used for
            // filtering.
            var campusStatusIds = DisplayCampusStatusGuids
                .Select( a => DefinedValueCache.Get( a, RockContext )?.Id )
                .Where( a => a != null )
                .Cast<int>()
                .ToList();

            // Get the campus type identifiers that will be used for filtering.
            var campusTypeIds = DisplayCampusTypeGuids
                .Select( a => DefinedValueCache.Get( a, RockContext )?.Id )
                .Where( a => a != null )
                .Cast<int>()
                .ToList();

            // Get all the campuses that match the filters and then cast them
            // to a MobileCampus type.
            return CampusCache.All()
                .Where( a => a.CampusStatusValueId.HasValue && campusStatusIds.Contains( a.CampusStatusValueId.Value ) )
                .Where( a => a.CampusTypeValueId.HasValue && campusTypeIds.Contains( a.CampusTypeValueId.Value ) )
                .Select( a => new MobileCampus
                {
                    Guid = a.Guid,
                    Name = a.Name
                } )
                .ToList();
        }

        private List<ListItemViewModel> GetBatches( int startIndex, int count, string CampusFilterGuid = null )
        {
            var qry = new FinancialBatchService( RockContext )
                .Queryable()
                .Where( b => b.BatchStartDateTime.HasValue );

            // Filter the batch by campus if the CampusFilterGuid is provided.
            if ( CampusFilterGuid.IsNotNullOrWhiteSpace() )
            {
                var campusGuid = CampusFilterGuid.AsGuid();
                qry = qry.Where( b => b.Campus.Guid == campusGuid );
            }

            // Filter the batch by status.
            var selectedStatus = GetAttributeValue( AttributeKeys.BatchStatus );
            var filterStatus = selectedStatus.SplitDelimitedValues()
                .Select( v => v.ConvertToEnum<BatchStatus>() )
                .ToList();

            qry = qry.Where( b => filterStatus.Contains( b.Status ) );

            // Get the detail page Guid to use in the merge fields.
            var detailPageGuid = GetAttributeValue( AttributeKeys.DetailPage ).AsGuidOrNull();

            // Get the financial batches based on the provided start index and count.
            var financialBatches = qry
                .OrderByDescending( fb => fb.BatchStartDateTime )
                .Skip( startIndex )
                .Take( count )
                .ToList();

            var templates = financialBatches.Select( ( bag ) =>
            {
                var mergeFiled = RequestContext.GetCommonMergeFields();
                mergeFiled.Add( "FinancialBatch", bag );
                mergeFiled.Add( "FinancialBatchIdKey", bag.IdKey );
                mergeFiled.Add( "DetailPage", detailPageGuid );

                return new ListItemViewModel
                {
                    Text = bag.Guid.ToString(),
                    Value = FinancialBatchListTemplate.ResolveMergeFields( mergeFiled ),
                };
            } );

            return templates.ToList();
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Get initial Data
        /// </summary>
        /// <returns></returns>
        [BlockAction( "GetInitialData" )]
        public BlockActionResult GetInitialData( MobileFinancialBatchListOptionsBag options )
        {
            return ActionOk( new InitialDataResultBag
            {
                AllowAdd = GetIsAddEnabled() && GetAttributeValue( AttributeKeys.AllowAdd ).AsBoolean(),
                Batches = GetBatches( 0, options.Count, options.CampusFilterGuid )
            } );
        }

        /// <summary>
        /// Gets the financial batch list.
        /// </summary>
        /// <returns></returns>
        [BlockAction( "GetFinancialBatches" )]
        public BlockActionResult GetFinancialBatches( GetBatchesOptionsBag options )
        {
            return ActionOk( GetBatches( options.StartIndex, options.Count, options.CampusFilterGuid ) );
        }

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc />
        public override object GetMobileConfigurationValues()
        {
            return new Rock.Common.Mobile.Blocks.Finance.FinancialBatchList.Configuration
            {
                DetailPage = GetAttributeValue( AttributeKeys.DetailPage ).AsGuidOrNull(),
                AllowFilterByCampus = GetAttributeValue( AttributeKeys.AllowFilterByCampus ).AsBoolean(),
                Campuses = GetDisplayCampuses(),
                PageLoadSize = GetAttributeValue( AttributeKeys.PageLoadSize ).AsInteger(),
                PostBatchSaveAction = GetAttributeValue( AttributeKeys.PostBatchSaveAction ).FromJsonOrNull<MobileNavigationActionViewModel>() ?? new MobileNavigationActionViewModel(),
            };
        }

        #endregion
    }
}
