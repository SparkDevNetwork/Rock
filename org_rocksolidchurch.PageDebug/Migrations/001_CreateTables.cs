using org_rocksolidchurch.PageDebug.Model;
using Rock.Plugin;

namespace org_rocksolidchurch.PageDebug.Migrations
{
    [MigrationNumber( 1, "1.13.0" )]
    public class CreateTables : Migration
    {
        public override void Down()
        {
            DropTable( $"dbo.{PluginWidget.TableName}" );
        }

        public override void Up()
        {
            Sql( $@"IF NOT EXISTS(SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID('[dbo].[{PluginWidget.TableName}]'))
                BEGIN
	                CREATE TABLE [dbo].[{PluginWidget.TableName}](
		                [Id] [int] IDENTITY(1,1) NOT NULL,
		                [{nameof( PluginWidget.ThisIsTheString )}] [nvarchar](max) NOT NULL,
		                [{nameof( PluginWidget.ThisIsTheSecret )}] [nvarchar](100) NOT NULL,
		                [{nameof( PluginWidget.ThisIsTheInt )}] [int] NOT NULL,
		                [CreatedDateTime] [datetime] NULL,
		                [ModifiedDateTime] [datetime] NULL,
		                [CreatedByPersonAliasId] [int] NULL,
		                [ModifiedByPersonAliasId] [int] NULL,
		                [Guid] [uniqueidentifier] NOT NULL,
		                [ForeignId] [int] NULL,
		                [ForeignGuid] [uniqueidentifier] NULL,
		                [ForeignKey] [nvarchar](100) NULL,
		                CONSTRAINT [PK_{PluginWidget.TableName}] PRIMARY KEY CLUSTERED ( [Id] ASC )
	                );

	                CREATE UNIQUE NONCLUSTERED INDEX [IX_{PluginWidget.TableName}_GuiId] 
		                ON [dbo].[{PluginWidget.TableName}] ( [Guid] ASC );

                    INSERT INTO [{PluginWidget.TableName}] (
                        [{nameof( PluginWidget.ThisIsTheString )}],
		                [{nameof( PluginWidget.ThisIsTheSecret )}],
		                [{nameof( PluginWidget.ThisIsTheInt )}],
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
