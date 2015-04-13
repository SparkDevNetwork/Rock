using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace church.ccv.SampleProject.Migrations
{
    [MigrationNumber( 4, "1.3.1" )]
    class AddForeignIdIndex : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( "ALTER TABLE [dbo].[_church_ccv_SampleProject_ReferralAgency] ALTER COLUMN [ForeignId] NVARCHAR(100)" );
            Sql( "CREATE NONCLUSTERED INDEX [IX_ForeignId] ON [dbo].[_church_ccv_SampleProject_ReferralAgency] ([ForeignId] ASC)" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}