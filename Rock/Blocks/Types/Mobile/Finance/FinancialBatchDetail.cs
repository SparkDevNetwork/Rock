using System.ComponentModel;

using Rock.Attribute;
using Rock.Model;

namespace Rock.Blocks.Types.Mobile.Finance
{
    /// <summary>
    /// The Rock Mobile Financial Batch Detail block, used to display
    /// </summary>
    [DisplayName( "Financial Batch Detail" )]
    [Category( "Mobile > Finance" )]
    [Description( "The Financial Batch Detail block." )]
    [IconCssClass( "fa fa-money-bills" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_FINANCE_FINANCIAL_BATCH_DETAIL_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_FINANCE_FINANCIAL_BATCH_DETAIL )]
    public class FinancialBatchDetail : RockBlockType
    {
    }
}
