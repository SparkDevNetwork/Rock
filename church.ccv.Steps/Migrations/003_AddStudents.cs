using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace church.ccv.Steps.Migrations
{
    [MigrationNumber( 3, "1.4.0" )]
    public class AddStudents : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @" 
                  ALTER TABLE [dbo].[_church_ccv_Steps_StepMeasureValue]
                  ADD [ActiveStudents] int NULL
                " );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
