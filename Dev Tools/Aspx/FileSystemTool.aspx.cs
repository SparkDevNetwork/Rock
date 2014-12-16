using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

//using Microsoft.SqlServer.Management.Smo;

/// <summary>
/// Tool for quickly creating website backup and restore (to/from a .bak folder).
/// </summary>
public partial class FileSystemTool : System.Web.UI.Page
{
    /// <summary>
    /// A list of any errors encountered.
    /// </summary>
    private static List<string> _errors = new List<string>();

    protected static readonly string _backupFolder = ".bak";

    private static int _fileCount = 0;
    private static int _folderCount = 0;

    protected override void OnLoad( EventArgs e )
    {
        base.OnLoad( e );

        if ( !Page.IsPostBack )
        {
            _errors = new List<string>();
            _fileCount = 0;
            _folderCount = 0;
        }
    }
    
    /// <summary>
    /// Handles the Click event of the btnBackupFiles control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void btnBackupFiles_Click( object sender, EventArgs e )
    {
        _fileCount = 0;
        _folderCount = 0;

        try
        {
            var path = Path.Combine( System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, _backupFolder );
            if ( !Directory.Exists( path ) )
            {
                DirectoryInfo di = Directory.CreateDirectory( path );
                //di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }

            bool noErrors = Copy( Path.Combine( System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath ),
                        Path.Combine( System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, _backupFolder )
                        );

            pnlAlert.Visible = true;

            var summary = string.Format( "<ul><li>{0} files</li><li>{1} folders</li></ul>", _fileCount, _folderCount );

            if ( noErrors )
            {
                pnlAlert.CssClass = "alert alert-success";
                lMessage.Text = string.Format( "<p>The filesystem was successfully backed up to the ~/.bak folder.</p>{0}", summary );
            }
            else
            {
                pnlAlert.CssClass = "alert alert-warning";
                lMessage.Text = string.Format( "The filesystem was backed up to the ~/.bak folder with errors. {0} <p>Problem copying to:</p><pre>{1}</pre>",
                    summary, string.Join( System.Environment.NewLine, _errors ) );
            }
        }
        catch ( Exception ex )
        {
            pnlAlert.Visible = true;
            pnlAlert.CssClass = "alert alert-danger";
            lMessage.Text = string.Format( "<strong>Something Went Wrong:</strong><p>{0}</p><hr><p>{1}</p>", ex.Message, ex.StackTrace );
        }
    }


    /// <summary>
    /// Handles the Click event of the btnRestoreFiles control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void btnRestoreFiles_Click( object sender, EventArgs e )
    {
        _fileCount = 0;
        _folderCount = 0;

        var path = Path.Combine( System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, _backupFolder );
        if ( ! Directory.Exists( path ) )
        {
            pnlAlert.Visible = true;
            pnlAlert.CssClass = "alert alert-info";
            lMessage.Text = string.Format( "<strong>No backup found.</strong><p>I could not find any {0} folder. Are you sure you created a backup?</p>", path );
            return;
        }

        try
        {
            bool noErrors = Copy( Path.Combine( System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, _backupFolder ),
                Path.Combine( System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath ),
                restore: true
                );

            pnlAlert.Visible = true;

            var summary = string.Format( "<ul><li>{0} files</li><li>{1} folders</li></ul>", _fileCount, _folderCount );

            if ( noErrors )
            {
                pnlAlert.CssClass = "alert alert-success";
                lMessage.Text = string.Format( "<p>The filesystem was successfully restored from the ~/.bak folder.</p>{0}", summary );
            }
            else
            {
                pnlAlert.CssClass = "alert alert-warning";
                lMessage.Text = string.Format( "The filesystem was restored from the ~/.bak folder with errors. {0} <p>Problem copying to:</p><pre>{1}</pre>",
                    summary, string.Join( System.Environment.NewLine, _errors ) );
            }
        }
        catch ( Exception ex )
        {
            pnlAlert.Visible = true;
            pnlAlert.CssClass = "alert alert-danger";
            lMessage.Text = string.Format( "<strong>Something Went Wrong:</strong><p>{0}</p><hr><pre>{1}</pre>", ex.Message, ex.StackTrace );
        }
    }

    #region Methods

    /// <summary>
    /// Copy all files from the given source directory into the target directory.
    /// </summary>
    /// <param name="sourceDirectory"></param>
    /// <param name="targetDirectory"></param>
    /// <param name="restore">Set to true if performing a restore (othwerwise the .bak folder will be skipped.)</param>
    /// <returns>True if no errors were encountered</returns>
    /// <exception cref="Exception">If </exception>
    private bool Copy( string sourceDirectory, string targetDirectory, bool restore = false )
    {
        DirectoryInfo diSource = new DirectoryInfo( sourceDirectory );
        DirectoryInfo diTarget = new DirectoryInfo( targetDirectory );

        return CopyAll( diSource, diTarget, restore );
    }

    /// <summary>
    /// Copy all files from the given source directory into the target directory.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <param name="restore">set to true if performing a restore (othwerwise the .bak folder will be skipped.)</param>
    /// <returns>true if no errors were encountered</returns>
    private static bool CopyAll( DirectoryInfo source, DirectoryInfo target, bool restore = false )
    {
        bool noErrors = true;

        if ( Directory.Exists( target.FullName ) == false )
        {
            Directory.CreateDirectory( target.FullName );
        }

        // Copy each file into the new directory.
        foreach ( FileInfo fi in source.GetFiles() )
        {
            try
            {
                //throw new Exception( "fake error." );
                fi.CopyTo( Path.Combine( target.FullName, fi.Name ), true );
                _fileCount++;
            }
            catch (Exception ex)
            {
                _errors.Add( string.Format( "{0} -- ({1})", Path.Combine( target.FullName, fi.Name ), ex.Message ) );
                noErrors = false;
            }
        }

        // Copy each subdirectory using recursion.
        foreach ( DirectoryInfo diSourceSubDir in source.GetDirectories().OfType<DirectoryInfo>()
                     .Where( d => restore || !d.Name.Equals( _backupFolder ) ) )
        {
            if ( diSourceSubDir.Name != "Cache" )
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory( diSourceSubDir.Name );
                CopyAll( diSourceSubDir, nextTargetSubDir );
                _folderCount++;
            }
        }

        return noErrors;
    }

    //// helper functional methods (like BindGrid(), etc.)
    //public void BackupDatabase()
    //{
    //    string dbName = "Rock2014_1208b";

    //    Server myServer = new Server( "(local)" );
    //    Microsoft.SqlServer.Management.Common.ServerConnection con = new Microsoft.SqlServer.Management.Common.ServerConnection( myServer.Name, "RockUser", "rRUZew6tpsYBhXuZ" );
    //    //Server server = new Server( con );

    //    Backup bkpDBFull = new Backup();
    //    /* Specify whether you want to back up database or files or log */
    //    bkpDBFull.Action = BackupActionType.Database;
    //    /* Specify the name of the database to back up */
    //    bkpDBFull.Database = dbName;
    //    /* You can take backup on several media type (disk or tape), here I am
    //     * using File type and storing backup on the file system */
    //    bkpDBFull.Devices.AddDevice( Path.Combine( System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, "App_Data", "db.bak"), DeviceType.File );
    //    bkpDBFull.BackupSetName = "Rock database backup";
    //    bkpDBFull.BackupSetDescription = "Rock database - full backup " + RockDateTime.Now.ToLongDateString();

    //    /* You can specify the expiration date for your backup data
    //     * after that date backup data would not be relevant */
    //   // bkpDBFull.ExpirationDate = DateTime.Today.AddDays( 10 );

    //    /* You can specify Initialize = false (default) to create a new 
    //     * backup set which will be appended as last backup set on the media. You
    //     * can specify Initialize = true to make the backup as first set on the
    //     * medium and to overwrite any other existing backup sets if the all the
    //     * backup sets have expired and specified backup set name matches with
    //     * the name on the medium */
    //    bkpDBFull.Initialize = false;

    //    /* Wiring up events for progress monitoring */
    //    //bkpDBFull.PercentComplete += CompletionStatusInPercent;
    //    //bkpDBFull.Complete += Backup_Completed;

    //    /* SqlBackup method starts to take back up
    //     * You can also use SqlBackupAsync method to perform the backup 
    //     * operation asynchronously */
    //    bkpDBFull.SqlBackup( myServer );
    //}

    #endregion

}