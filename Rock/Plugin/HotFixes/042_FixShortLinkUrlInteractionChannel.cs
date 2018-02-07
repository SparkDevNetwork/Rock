using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 42, "1.7.0" )]
    public class FixShortLinkUrlInteractionChannel : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
    DECLARE @OldEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.SiteUrlMap' )
    DECLARE @NewEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.PageShortLink' )

    IF @OldEntityTypeId IS NOT NULL AND @NewEntityTypeId IS NOT NULL
    BEGIN
	    UPDATE [InteractionChannel]
	    SET [ComponentEntityTypeId] = @NewEntityTypeId
	    WHERE [ComponentEntityTypeId] = @OldEntityTypeId

	    DELETE [EntityType]
	    WHERE [Id] = @OldEntityTypeId
    END
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
