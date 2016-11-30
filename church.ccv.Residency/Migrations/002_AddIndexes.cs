using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace church.ccv.Residency.Migrations
{
    [MigrationNumber( 2, "1.0.8" )]
    public class AddIndexes : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
CREATE UNIQUE NONCLUSTERED INDEX [IX_CompetencyId_PersonId] ON [dbo].[_com_ccvonline_Residency_CompetencyPerson]
(
	[CompetencyId] ASC,
	[PersonId] ASC
)

CREATE UNIQUE NONCLUSTERED INDEX [IX_CompetencyPersonId_ProjectId] ON [dbo].[_com_ccvonline_Residency_CompetencyPersonProject]
(
	[CompetencyPersonId] ASC,
	[ProjectId] ASC
)

CREATE UNIQUE NONCLUSTERED INDEX [IX_ProjectId_AssessmentOrder] ON [dbo].[_com_ccvonline_Residency_ProjectPointOfAssessment]
(
	[ProjectId] ASC,
	[AssessmentOrder] ASC
)
" );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // intentionally blank
        }
    }
}
