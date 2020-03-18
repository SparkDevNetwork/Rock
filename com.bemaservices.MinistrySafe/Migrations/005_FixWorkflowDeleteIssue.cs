using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.bemaservices.MinistrySafe.Migrations
{
    [MigrationNumber( 5, "1.8.5" )]
    public class FixWorkflowDeleteIssue : Migration
    {
        public override void Up()
        {
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_MinistrySafe_MinistrySafeUser] DROP CONSTRAINT [FK__com_bemaservices_MinistrySafe_MinistrySafeUser_WorkflowId]

                ALTER TABLE [dbo].[_com_bemaservices_MinistrySafe_MinistrySafeUser]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_MinistrySafe_MinistrySafeUser_WorkflowId] FOREIGN KEY([WorkflowId])
                REFERENCES [dbo].[Workflow] ([Id])
                ON DELETE CASCADE
                

                ALTER TABLE [dbo].[_com_bemaservices_MinistrySafe_MinistrySafeUser] CHECK CONSTRAINT [FK__com_bemaservices_MinistrySafe_MinistrySafeUser_WorkflowId]
" );          
            
        }
        public override void Down()
        {
        }
    }
}
