using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.LogViewer;
using Rock.Web.UI;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Used to view the <see cref="Rock.Logging.RockLogEvent"/> logged via RockLogger.
    /// </summary>
    [DisplayName( "Logs" )]
    [Category( "Core" )]
    [Description( "Block to view system logs." )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "DB6A13D0-964D-4839-9E32-BF1E522D176A" )]
    [Rock.SystemGuid.BlockTypeGuid( "E35992D6-C175-4C35-9DA6-A9A7115E1FFD" )]
    public class LogViewer : RockListBlockType<RockLogEventsBag>, ICustomGridColumns
    {
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<RockLogEventsBag>();

            var builder = GetGridBuilder();
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        protected override GridBuilder<RockLogEventsBag> GetGridBuilder()
        {
            return new GridBuilder<RockLogEventsBag>()
                .WithBlock( this )
                .AddDateTimeField( "datetime", a => a.DateTime.DateTime )
                .AddTextField( "level", a => a.Level )
                .AddTextField( "domain", a => a.Domain )
                .AddTextField( "message", a => a.Message )
                .AddTextField( "exception", a => a.Exception );
        }

        [BlockAction]
        public BlockActionResult DownloadLogs()
        {
            var logFiles = Rock.Logging.RockLogger.Log.LogFiles;
            var ms = new MemoryStream();
            ms.Seek( 0, SeekOrigin.Begin );
            using ( var zip = new ZipArchive( ms, ZipArchiveMode.Create, true ) )
            {
                foreach ( var file in logFiles )
                {
                    var entryName = Path.GetFileName( file );
                    var entry = zip.CreateEntry( entryName );
                    entry.LastWriteTime = System.IO.File.GetLastWriteTime( file );
                    using ( var fileStream = new FileStream( file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
                        using ( var entryStream = entry.Open() )
                        {
                            fileStream.CopyTo( entryStream );
                        }
                }
            }
            return new FileBlockActionResult( ms, "application/zip", "RockLogs.zip" );
        }

        protected override IQueryable<RockLogEventsBag> GetListQueryable( RockContext rockContext )
        {
            var logReader = Rock.Logging.RockLogger.LogReader;
            var events = logReader.GetEvents( 0, logReader.RecordCount );
            var rockLogEvents = events.AsQueryable()
                .Select( e => new RockLogEventsBag
            {
                DateTime = e.DateTime,
                Level = e.Level.ToStringSafe(),
                Domain = e.Domain,
                Message = e.Message,
                Exception = e.SerializedException,
            } );

            return rockLogEvents;
        }
    }
}
