using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.bemaservices.MinistrySafe.Migrations
{
    [MigrationNumber( 4, "1.8.5" )]
    public partial class WorkflowFix : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.UpdateWorkflowActivityType( "5876314A-FC4F-4A07-8CA0-A02DE26E55BE", true, "Cancel Request", "Cancels the request prior to submitting to provider and completes the workflow.", false, 7, "748423B3-508B-4FDF-A200-3F3E86BF9182" ); // Ministry Safe Training Request:Cancel Request
            RockMigrationHelper.UpdateWorkflowActionType( "748423B3-508B-4FDF-A200-3F3E86BF9182", "Cancel Workflow", 0, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "", 1, "", "0C48DBA6-1F6D-4AD8-9A72-F09C135A1AF1" ); // Ministry Safe Training Request:Cancel Request:Cancel Workflow
            RockMigrationHelper.AddActionTypeAttributeValue( "0C48DBA6-1F6D-4AD8-9A72-F09C135A1AF1", "3327286F-C1A9-4624-949D-33E9F9049356", @"Cancelled" ); // Ministry Safe Training Request:Cancel Request:Cancel Workflow:Status|Status Attribute     
        }
        public override void Down()
        {
         }
    }
}
