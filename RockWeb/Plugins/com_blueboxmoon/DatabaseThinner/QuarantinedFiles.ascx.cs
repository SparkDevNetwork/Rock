using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;

using com.blueboxmoon.DatabaseThinner;
using Rock;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_blueboxmoon.DatabaseThinner
{
    [DisplayName( "Quarantined Files" )]
    [Category( "Blue Box Moon > Database Thinner" )]
    [Description( "View all currently quarantined files that are waiting to be deleted." )]
    public partial class QuarantinedFiles : RockBlock
    {
        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gFiles.DataKeyNames = new[] { "Id" };
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
                if ( !string.IsNullOrWhiteSpace( PageParameter( "ViewFileId" ) ) )
                {
                    PreviewFile( PageParameter( "ViewFileId" ).AsInteger() );
                }
                else
                {
                    BindGrid();
                }

                tbFileNameFilter.Text = gfFiles.GetUserPreference( "FileName" );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Previews the file.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        protected void PreviewFile( int fileId )
        {
            var quarantinedFile = Helper.FindQuarantinedFiles().Where( f => f.Id == fileId ).Single();
            var filePath = System.Web.Hosting.HostingEnvironment.MapPath( string.Format( "~/App_Data/DatabaseThinnerQuarantines/{0}_{1}", quarantinedFile.Id, quarantinedFile.Guid ) );

            using ( var fileStream = File.OpenRead( filePath ) )
            {
                using ( var stream = Crypto.DecryptStream( fileStream ) )
                {
                    HttpContext.Current.Response.Clear();
                    HttpContext.Current.Response.ContentType = quarantinedFile.MimeType;
                    stream.CopyTo( HttpContext.Current.Response.OutputStream );
                    HttpContext.Current.Response.End();
                }
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        protected void BindGrid()
        {
            var files = Helper.FindQuarantinedFiles().AsQueryable();

            // Filter by File Name
            var nameFilter = gfFiles.GetUserPreference( "FileName" );
            if ( !string.IsNullOrEmpty( nameFilter.Trim() ) )
            {
                files = files.Where( f => f.FileName.Contains( nameFilter.Trim() ) );
            }

            if ( gFiles.SortProperty != null )
            {
                files = files.Sort( gFiles.SortProperty );
            }
            else
            {
                files = files.OrderByDescending( f => f.QuarantinedDateTime );
            }

            gFiles.DataSource = files.ToList();
            gFiles.DataBind();
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
            var quarantinedFile = Helper.FindQuarantinedFiles().Where( f => f.Id == e.RowKeyId ).Single();

            Helper.DeleteQuarantinedFile( quarantinedFile );

            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the gFilesRestore control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gFilesRestore_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var quarantinedFile = Helper.FindQuarantinedFiles().Where( f => f.Id == e.RowKeyId ).Single();

            if ( Helper.RestoreQuarantinedFile( quarantinedFile ) )
            {
                hfDialogMessage.Value = "File has been restored.";

                BindGrid();
            }
            else
            {
                hfDialogMessage.Value = "Failed to restore the quarantined file.";
            }
        }

        #endregion
    }
}
