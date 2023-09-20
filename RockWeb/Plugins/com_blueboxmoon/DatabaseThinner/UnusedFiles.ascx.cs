using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microsoft.AspNet.SignalR;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_blueboxmoon.DatabaseThinner
{
    [DisplayName( "Unused Files" )]
    [Category( "Blue Box Moon > Database Thinner" )]
    [Description( "Scan and view all unsued files might be candidates for deletion." )]
    [BooleanField( "Quarantine Files", "Quarantines files instead of immediately deleting them.", true, order: 0 )]
    [BinaryFileTypesField( "Ignored File Types", "The File Types to ignore when looking for unused files.", false, Rock.SystemGuid.BinaryFiletype.CHECKIN_LABEL, order: 1 )]
    [IntegerField( "Days Old", "The number of days old a file must be to be considered a possible unused file.", true, 90, order: 2 )]
    public partial class UnusedFiles : RockBlock
    {
        #region Properties

        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>
        /// The files.
        /// </value>
        protected List<UnusedFile> Files
        {
            get
            {
                return ( List<UnusedFile> ) ViewState["Files"];
            }
            set
            {
                ViewState["Files"] = value;
            }
        }

        /// <summary>
        /// This holds the reference to the RockMessageHub SignalR Hub context.
        /// </summary>
        private IHubContext HubContext = GlobalHost.ConnectionManager.GetHubContext<RockMessageHub>();

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gFiles.DataKeyNames = new[] { "Id" };

            var lbBulkSave = new LinkButton
            {
                ID = "lbBulkSave",
                CssClass = "btn btn-default btn-sm js-unused-bulk-save",
                Text = "<i class='fa fa-save'></i>",
                ToolTip = "Save"
            };
            lbBulkSave.Click += lbBulkSave_Click;
            gFiles.Actions.AddCustomActionControl( lbBulkSave );

            var lbBulkDelete = new LinkButton
            {
                ID = "lbBulkDelete",
                CssClass = "btn btn-default btn-sm js-unused-bulk-delete",
                Text = "<i class='fa fa-trash'></i>",
                ToolTip = "Delete"
            };
            lbBulkDelete.Click += lbBulkDelete_Click;
            gFiles.Actions.AddCustomActionControl( lbBulkDelete );

            RockPage.AddScriptLink( "~/Scripts/jquery.signalR-2.2.0.min.js", false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                try
                {
                    tbFileNameFilter.Text = gfFiles.GetUserPreference( "FileName" );
                    ftpFileTypeFilter.SelectedValue = gfFiles.GetUserPreference( "FileType" );
                }
                catch
                {
                    /* Ignore errors so we don't crash out if the filter is invalid. */
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        protected void BindGrid()
        {
            IEnumerable<UnusedFile> sortedFiles;

            var files = GetFilteredFiles();

            if ( gFiles.SortProperty != null )
            {
                sortedFiles = files.Sort( gFiles.SortProperty );
            }
            else
            {
                sortedFiles = files.ToList().OrderBy( f => f.CreatedDateTime );
            }

            gFiles.DataSource = sortedFiles.ToList();
            gFiles.DataBind();
        }

        /// <summary>
        /// Saves the file.
        /// </summary>
        /// <param name="file">The file.</param>
        private void SaveFile( UnusedFile file )
        {
            using ( var rockContext = new RockContext() )
            {
                var definedValueService = new DefinedValueService( rockContext );
                var definedValue = new DefinedValue
                {
                    DefinedTypeId = DefinedTypeCache.Get( com.blueboxmoon.DatabaseThinner.SystemGuid.DefinedType.RESTORED_FILES ).Id,
                    Value = string.Format( "#{0}: {1}", file.Id, file.FileName ),
                    Description = string.Format( "Saved on {0} by {1}", RockDateTime.Now, CurrentPerson.FullName )
                };

                definedValue.LoadAttributes( rockContext );
                definedValue.SetAttributeValue( "File", file.Guid.ToString() );

                definedValueService.Add( definedValue );

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    definedValue.SaveAttributeValues( rockContext );
                } );
            }
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="file">The file.</param>
        private void DeleteFile( UnusedFile file )
        {
            if ( GetAttributeValue( "QuarantineFiles" ).AsBoolean() )
            {
                com.blueboxmoon.DatabaseThinner.Helper.QuarantineBinaryFile( file.Id );
            }
            else
            {
                using ( var rockContext = new RockContext() )
                {
                    var binaryFileService = new BinaryFileService( rockContext );
                    var binaryFile = binaryFileService.Get( file.Id );

                    binaryFileService.Delete( binaryFile );

                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Gets the filtered files.
        /// </summary>
        /// <returns></returns>
        private IQueryable<UnusedFile> GetFilteredFiles()
        {
            var files = Files.AsQueryable();

            // Filter by File Name
            var nameFilter = gfFiles.GetUserPreference( "FileName" );
            if ( !string.IsNullOrEmpty( nameFilter.Trim() ) )
            {
                files = files.Where( f => f.FileName.Contains( nameFilter.Trim() ) );
            }

            // Filter by File Type
            var typeFilter = gfFiles.GetUserPreference( "FileType" ).AsIntegerOrNull();
            if ( typeFilter.HasValue )
            {
                files = files.Where( f => f.FileTypeId == typeFilter.Value );
            }

            return files;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Apply Filter event for the GridFilter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gfFiles_ApplyFilterClick( object sender, EventArgs e )
        {
            gfFiles.SaveUserPreference( "FileName", tbFileNameFilter.Text );
            gfFiles.SaveUserPreference( "FileType", ftpFileTypeFilter.SelectedValue );

            BindGrid();
        }

        /// <summary>
        /// Gfs the files display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfFiles_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            if ( e.Key == "FileName" )
            {
                e.Name = "File Name";
            }
            else if ( e.Key == "FileType" )
            {
                e.Name = "File Type";

                var type = new BinaryFileTypeService( new RockContext() ).Get( e.Value.AsInteger() );
                e.Value = type != null ? type.Name : e.Value;
            }
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfFiles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfFiles_ClearFilterClick( object sender, EventArgs e )
        {
            gfFiles.DeleteUserPreferences();

            tbFileNameFilter.Text = string.Empty;
            ftpFileTypeFilter.SelectedValue = string.Empty;
        }

        /// <summary>
        /// Handles the Click event of the lbScan control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbScan_Click( object sender, EventArgs e )
        {
            int daysOld = GetAttributeValue( "DaysOld" ).AsInteger();
            var ignoredFileTypes = GetAttributeValue( "IgnoredFileTypes" ).SplitDelimitedValues().AsGuidList();

            //
            // Define the task that will run to process the data.
            //
            var task = new Task( () =>
            {
                //
                // Wait for the postback to settle.
                //
                Task.Delay( 1000 ).Wait();

                HubContext.Clients.Client( hfConnectionId.Value ).unusedFileScanProgress( 0, 0, "Scanning files..." );

                var fileIds = com.blueboxmoon.DatabaseThinner.Helper.FindUnreferencedFiles( daysOld, ignoredFileTypes, (c, t, s) =>
                {
                    HubContext.Clients.Client( hfConnectionId.Value ).unusedFileScanProgress( c, t, s );
                } );

                var files = new List<UnusedFile>();

                using ( var rockContext = new RockContext() )
                {
                    var binaryFileService = new BinaryFileService( rockContext );

                    while ( fileIds.Any() )
                    {
                        var nextFileIds = fileIds.Take( 100 );
                        fileIds = fileIds.Skip( 100 ).ToList();

                        var binaryFiles = binaryFileService.Queryable()
                            .Where( f => nextFileIds.Contains( f.Id ) )
                            .ToList();

                        var newFiles = binaryFiles.Select( f => new UnusedFile
                        {
                            Id = f.Id,
                            Guid = f.Guid,
                            FileName = f.FileName,
                            FileType = f.BinaryFileType.Name,
                            FileTypeId = f.BinaryFileType.Id,
                            FileSize = f.FileSize,
                            Url = ResolveRockUrl( string.Format( "~/GetFile.ashx?guid={0}", f.Guid ) ),
                            CreatedDateTime = f.CreatedDateTime
                        } ).ToList();

                        files.AddRange( newFiles );
                    }
                }

                //
                // Show the final status.
                //
                HubContext.Clients.Client( hfConnectionId.Value ).unusedFileScanStatus( files.ToJson() );
            } );

            //
            // Define an error handler for the task.
            //
            task.ContinueWith( ( t ) =>
            {
                if ( t.IsFaulted )
                {
                    ExceptionLogService.LogException( t.Exception );
                    HubContext.Clients.Client( hfConnectionId.Value ).unusedFileScanStatus( string.Empty, t.Exception.Message );
                }
            } );

            task.Start();

            lbScan.Enabled = false;
        }

        /// <summary>
        /// Handles the Click event of the lbScanCompleted control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbScanCompleted_Click( object sender, EventArgs e )
        {
            Files = hfScanState.Value.FromJsonOrNull<List<UnusedFile>>();

            pnlScan.Visible = false;
            pnlDetails.Visible = true;

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gFiles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gFiles_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the gFilesDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gFilesDelete_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var files = Files;
            var file = files.Single( f => f.Id == e.RowKeyId );

            DeleteFile( file );

            files.Remove( file );
            Files = files;

            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the gFilesRestore control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gFilesSave_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var files = Files;
            var file = files.Single( f => f.Id == e.RowKeyId );

            SaveFile( file );

            files.Remove( file );
            Files = files;

            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the lbBulkSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void lbBulkSave_Click( object sender, EventArgs e )
        {
            var files = Files;
            var filesToSave = GetFilteredFiles().Where( f => gFiles.SelectedKeys.Count == 0 || gFiles.SelectedKeys.Contains( f.Id ) ).ToList();

            foreach ( var file in filesToSave )
            {
                SaveFile( file );

                files.Remove( file );
            }

            Files = files;
            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the lbBulkDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void lbBulkDelete_Click( object sender, EventArgs e )
        {
            var files = Files;
            var filesToDelete = GetFilteredFiles().Where( f => gFiles.SelectedKeys.Count == 0 || gFiles.SelectedKeys.Contains( f.Id ) ).ToList();

            foreach ( var file in filesToDelete )
            {
                DeleteFile( file );

                files.Remove( file );
            }

            Files = files;
            BindGrid();
        }

        #endregion

        #region Support Classes

        [Serializable]
        protected class UnusedFile
        {
            public int Id { get; set; }

            public Guid Guid { get; set; }

            public string FileName { get; set; }

            public string FileType { get; set; }

            public int FileTypeId { get; set; }

            public long? FileSize { get; set; }

            public string Url { get; set; }

            public DateTime? CreatedDateTime { get; set; }
        }

        #endregion
    }
}
