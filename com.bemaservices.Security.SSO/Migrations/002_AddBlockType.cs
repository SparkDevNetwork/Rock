using Rock.Plugin;
using com.bemaservices.Security.SSO.SystemGuids;
using Rock.Web.Cache;

namespace com.bemaservices.Security.SSO.Migrations
{
    [MigrationNumber( 2, "1.8.0" )]
    public class SSO_AddBlockType : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            var name = "SSO Initiator";
            var description = "Logs person into Rock via Specified Authentication Provider";
            var path = "~/Plugins/com_bemaservices/Security/SSOInitiator.ascx";
            var category = "BEMA Services > Security";
            var guid = BlockType.SSOAuthentication;

            Sql( string.Format( @"

                DECLARE @Id int
                SET @Id = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '{4}')
                IF @Id IS NULL
                BEGIN
                    INSERT INTO [BlockType] (
                        [IsSystem],[Path],[Category],[Name],[Description],
                        [Guid])
                    VALUES(
                        1,'{0}','{1}','{2}','{3}',
                        '{4}')
                END
                ELSE
                BEGIN
                    UPDATE [BlockType] SET
                        [IsSystem] = 1,
                        [Category] = '{1}',
                        [Name] = '{2}',
                        [Description] = '{3}',
                        [Path] = '{0}'
                    WHERE [Guid] = '{4}'
                END
                ",
                    path,
                    category,
                    name,
                    description.Replace( "'", "''" ),
                    guid
                )
            );
        }

        public override void Down()
        {
            RockMigrationHelper.DeleteBlockType( BlockType.SSOAuthentication );
        }
    }

}