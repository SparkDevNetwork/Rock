using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Migrations
{
    class _20130224130600_AddFinancialBatchType : RockMigration_2
    {
        public override void Up()
        {
            AddColumn( "dbo.FinancialBatch", "BatchTypeValueId", c => c.Int() );
            
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.FinancialBatch", "BatchTypeValueId" );
        }
    }
}
