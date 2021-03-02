using com_rocksolidchurchdemo.PageDebug.Model;
using Rock.Plugin;

namespace com_rocksolidchurchdemo.PageDebug.Migrations
{
    [MigrationNumber( 1, "1.13.0" )]
    public class CreateTables : Migration
    {
        public override void Down()
        {
            DropTable( $"dbo.{Widget.TableName}" );
        }

        public override void Up()
        {
            Sql( $@"IF NOT EXISTS(SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID('[dbo].[{Widget.TableName}]'))
                BEGIN
	                CREATE TABLE [dbo].[{Widget.TableName}](
		                [Id] [int] IDENTITY(1,1) NOT NULL,
		                [{nameof( Widget.ThisIsTheString )}] [nvarchar](max) NOT NULL,
		                [{nameof( Widget.ThisIsTheSecret )}] [nvarchar](100) NOT NULL,
		                [{nameof( Widget.ThisIsTheInt )}] [int] NOT NULL,
		                [CreatedDateTime] [datetime] NULL,
		                [ModifiedDateTime] [datetime] NULL,
		                [CreatedByPersonAliasId] [int] NULL,
		                [ModifiedByPersonAliasId] [int] NULL,
		                [Guid] [uniqueidentifier] NOT NULL,
		                [ForeignId] [int] NULL,
		                [ForeignGuid] [uniqueidentifier] NULL,
		                [ForeignKey] [nvarchar](100) NULL,
		                CONSTRAINT [PK_{Widget.TableName}] PRIMARY KEY CLUSTERED ( [Id] ASC )
	                );

	                CREATE UNIQUE NONCLUSTERED INDEX [IX_{Widget.TableName}_GuiId] 
		                ON [dbo].[{Widget.TableName}] ( [Guid] ASC );

                    INSERT INTO [{Widget.TableName}] (
                        [{nameof( Widget.ThisIsTheString )}],
		                [{nameof( Widget.ThisIsTheSecret )}],
		                [{nameof( Widget.ThisIsTheInt )}],
                        [Guid]
                    ) VALUES (
                        'String 1', 'Secret 1', 111, NEWID()
                    ), (
                        'String 2', 'Secret 2', 222, NEWID()
                    ), (
                        'String 3', 'Secret 3', 333, NEWID()
                    );
                END" );
        }
    }
}
