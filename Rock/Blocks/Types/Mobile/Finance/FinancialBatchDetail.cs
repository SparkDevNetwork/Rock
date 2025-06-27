using System;
using System.ComponentModel;

using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;

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

        /// <summary>
        /// Saves the financial batch.
        /// </summary>
        /// <param name="financialBatchBag"></param>
        /// <returns></returns>
        [BlockAction( "SaveFinancialBatch" )]
        public BlockActionResult SaveFinancialBatch( AddFinancialBatchBag financialBatchBag )
        {
            if ( financialBatchBag == null )
            {
                return ActionBadRequest( "Financial batch data is required." );
            }

            var campusGuid = financialBatchBag.Campus.AsGuidOrNull();
            int? campusId = null;
            if ( campusGuid.HasValue )
            {
                campusId = CampusCache.GetId( campusGuid.Value );
            }

            var status = financialBatchBag.Status.ConvertToEnum<BatchStatus>();

            var financialBatch = new FinancialBatch
            {
                Name = financialBatchBag.Name,
                Status = status,
                BatchStartDateTime = financialBatchBag.BatchStartDate,
                BatchEndDateTime = financialBatchBag.BatchEndDate,
                ControlAmount = financialBatchBag.ControlAmount ?? 0,
                ControlItemCount = financialBatchBag.ControlItemCount,
                CampusId = campusId,
                Note = financialBatchBag.Note
            };

            var financialBatchService = new FinancialBatchService( RockContext );
            financialBatchService.Add( financialBatch );
            RockContext.SaveChanges();

            return ActionOk();
        }

        public class AddFinancialBatchBag
        {
            public string Name { get; set; }
            public string Status { get; set; }
            public DateTime? BatchStartDate { get; set; }
            public DateTime? BatchEndDate { get; set; }
            public decimal? ControlAmount { get; set; }
            public int? ControlItemCount { get; set; }
            public string Campus { get; set; }
            public string Note { get; set; }
        }
    }
}
