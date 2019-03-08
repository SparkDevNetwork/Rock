namespace Rock.Plugin.HotFixes
{
    /// <summary>
    ///MigrationRollupsForV8_7_5
    /// </summary>
    [MigrationNumber( 68, "1.8.6" )]
    public class MigrationRollupsForV8_7_5 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //UpdatedGoogleMapsShortCode();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {

        }

        /// <summary>
        /// SK: Fixed Typo in Google Maps Lava Shortcode Marker
        /// </summary>
        private void UpdatedGoogleMapsShortCode()
        {
            Sql( @"UPDATE
	[LavaShortcode]
SET
	[Documentation] = REPLACE([Documentation],'markerannimation','markeranimation'),
	[Markup] = REPLACE([Markup],'markerannimation','markeranimation'),
	[Parameters] = REPLACE([Parameters],'markerannimation','markeranimation')
WHERE
	[Guid]='FE298210-1307-49DF-B28B-3735A414CCA0'
" );
        }

    }
}
