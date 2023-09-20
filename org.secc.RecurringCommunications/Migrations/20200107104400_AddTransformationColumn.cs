using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.secc.RecurringCommunications.Migrations
{
    using Rock.Plugin;

    [MigrationNumber(2, "1.8.0")]
    public partial class AddTransformationColumn : Migration
    {
        public override void Up()
        {
            //Add TransformationEntityTypeId Column
            AddColumn("dbo._org_secc_RecurringCommunications_RecurringCommunication", "TransformationEntityTypeId", c => c.Int(nullable: true));

            //Add Foreign Key to Entity Table
            Sql(@"
                IF NOT EXISTS (
                    SELECT * 
                    FROM sys.foreign_keys 
                    WHERE object_id = OBJECT_ID(N'dbo.FK__org_secc_RecurringCommunications_RecurringCommunication_TransformationEntityType')
                        AND parent_object_id = OBJECT_ID(N'dbo._org_secc_RecurringCommunications_RecurringCommunication'))
                BEGIN
                    ALTER TABLE [dbo].[_org_secc_RecurringCommunications_RecurringCommunication]  WITH CHECK ADD  CONSTRAINT [FK__org_secc_RecurringCommunications_RecurringCommunication_TransformationEntityType] FOREIGN KEY([TransformationEntityTypeId])
                    REFERENCES [dbo].[EntityType] ([Id])

                    ALTER TABLE [dbo].[_org_secc_RecurringCommunications_RecurringCommunication] CHECK CONSTRAINT [FK__org_secc_RecurringCommunications_RecurringCommunication_TransformationEntityType]

                END");

            //Add Index for TransformationEntityTypeId column
            Sql(@"
                IF NOT EXISTS(
	                SELECT * 
	                FROM sys.indexes
	                WHERE 
		                [name] = 'IX__org_secc_RecurringCommunications_RecurringCommunication_TransformationEntityTypeId' 
		                AND [object_id] = OBJECT_ID(N'dbo._org_secc_RecurringCommunications_RecurringCommunication'))
                BEGIN 
	                CREATE NONCLUSTERED INDEX [IX__org_secc_RecurringCommunications_RecurringCommunication_TransformationEntityTypeId] ON [dbo].[_org_secc_RecurringCommunications_RecurringCommunication]
	                (
		                [TransformationEntityTypeId] ASC
	                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                END");


        }

        public override void Down()
        {
            //Drop Index
            Sql(@"
                IF EXISTS(
	                SELECT * 
	                FROM sys.indexes
	                WHERE 
		                [name] = 'IX__org_secc_RecurringCommunications_RecurringCommunication_TransformationEntityTypeId' 
		                AND [object_id] = OBJECT_ID(N'dbo._org_secc_RecurringCommunications_RecurringCommunication'))
                BEGIN
	                DROP INDEX [IX__org_secc_RecurringCommunications_RecurringCommunication_TransformationEntityTypeId] ON [dbo].[_org_secc_RecurringCommunications_RecurringCommunication]
                END");

            //Drop Foreign Key
            Sql(@"
                IF NOT EXISTS (SELECT * 
                  FROM sys.foreign_keys 
                   WHERE object_id = OBJECT_ID(N'dbo.FK__org_secc_RecurringCommunications_RecurringCommunication_TransformationEntityType')
                   AND parent_object_id = OBJECT_ID(N'dbo._org_secc_RecurringCommunications_RecurringCommunication'))
                BEGIN
	                ALTER TABLE [dbo].[_org_secc_RecurringCommunications_RecurringCommunication] DROP CONSTRAINT [FK__org_secc_RecurringCommunications_RecurringCommunication_TransformationEntityType]
                END");

            DropColumn("dbo._org_secc_RecurringCommunications_RecurringCommunication", "TransformationEntityTypeId");
        }


    }
}
