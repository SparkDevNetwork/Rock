// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using System.Linq;
using Rock.Web.Cache;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Rock.Web.UI.Controls;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using System.Diagnostics;
using Rock.Web;
using System.ComponentModel;

namespace RockWeb.Plugins.com_mineCartStudio.StorageMover
{
    [DisplayName( "Storage Mover" )]
    [Category( "Mine Cart Studio > Storage Mover" )]
    [Description( "Moves files from one storage provider to another." )]

    public partial class StorageMover : RockBlock
    {

        #region Fields
        private BinaryFileType _currentBinaryFileType = null;

        private const string STORAGE_TYPE_FILE = "a97b6002-454e-4890-b529-b99f8f2f376a";
        private const string SESSION_CONFIRMATION_CONFIRMED = "StorageMoverConfirmed";

        /// <summary>
        /// This holds the reference to the RockMessageHub SignalR Hub context.
        /// </summary>
        private IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<RockMessageHub>();

        /// <summary>
        /// Gets the signal r notification key.
        /// </summary>
        /// <value>
        /// The signal r notification key.
        /// </value>
        public string SignalRNotificationKey
        {
            get
            {
                return string.Format( "StorageMover_BlockId:{0}_SessionId:{1}", this.BlockId, Session.SessionID );
            }
        }

        #endregion Fields

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/jquery.signalR-2.2.0.min.js", fingerprint: false );
            this.AddConfigurationUpdateTrigger( upnlContent );
        }


        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( Session[SESSION_CONFIRMATION_CONFIRMED] != null )
            {
                var warningConfirmed = Session[SESSION_CONFIRMATION_CONFIRMED].ToString().AsBoolean();

                if ( warningConfirmed )
                {
                    ProcessConfirmation();
                }
            }

            if ( !IsPostBack )
            {
                btnMove.Visible = true;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirm_Click( object sender, EventArgs e )
        {
            ProcessConfirmation();

            Session[SESSION_CONFIRMATION_CONFIRMED] = "true";
        }

        /// <summary>
        /// Handles the Click event of the btnMove control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMove_Click( object sender, EventArgs e )
        {
            pnlProgress.Visible = true;
            pnlSelectCriteria.Visible = true;
            btnMove.Visible = false;

            var migrateTask = new Task( () =>
            {
                // wait a little so the browser can render and start listening to events
                System.Threading.Thread.Sleep( 500 );

                var rockContext = new RockContext();
                var binaryFileTypeId = btpFileType.SelectedValueAsInt();
                var sourceStorageTypeId = ddlSourceStorage.SelectedValueAsInt();
                try
                {
                    if ( binaryFileTypeId.HasValue && sourceStorageTypeId.HasValue )
                    {
                        var binaryFileService = new BinaryFileService( rockContext );
                        var binaryFilesQry = binaryFileService.Queryable()
                                            .Where( a =>
                                                a.BinaryFileTypeId == binaryFileTypeId.Value
                                                && a.StorageEntityTypeId == sourceStorageTypeId.Value );

                        // Limit query to the number of files that they have requested
                        var fileCount = nbMaxFiles.Text.AsIntegerOrNull();

                        if ( fileCount.HasValue )
                        {
                            binaryFilesQry = binaryFilesQry.Take( fileCount.Value );
                        }

                        var binaryFiles = binaryFilesQry.ToList();


                        var binaryFileType = new BinaryFileTypeService( rockContext ).Get( binaryFileTypeId.Value );
                        var totalFileSize = binaryFiles.Where( a => a.FileSize.HasValue ).Sum( a => a.FileSize );
                        var processedFileSize = default( long );
                        var totalFileCount = binaryFiles.Count();

                        var panelMessage = string.Format( "<p> Moving {0} Files from the {1} storage provider to the {2} storage provider </p>", binaryFiles.Count, EntityTypeCache.Get( sourceStorageTypeId.Value ).FriendlyName, binaryFileType.StorageEntityType.FriendlyName );
                        WriteProgressMessage( panelMessage, string.Empty );

                        var descriptionList = new DescriptionList();
                        int processedFileCount = 0;

                        var binaryFileIds = binaryFiles.Select(b => b.Id).ToList();
                        rockContext.Dispose();

                        // At this point we have closed the original rock context so we can make a new one inside
                        // of the loop. This improves performance, reduces memory and also helps from locking files
                        // when an exception occurs.

                        foreach ( var binaryFileId in binaryFileIds )
                        {
                            using ( rockContext = new RockContext() )
                            {
                                binaryFileService = new BinaryFileService( rockContext );
                                var binaryFile = binaryFileService.Get( binaryFileId );

                                // Possible that the file was deleted since getting the list from the database
                                if ( binaryFile == null )
                                {
                                    continue;
                                }

                                try
                                {
                                    binaryFile.ModifiedDateTime = RockDateTime.Now;

                                    // Check that the file is not missing any required fields. 
                                    // This can happen when data was imported incorrectly :(
                                    if (binaryFile.MimeType.IsNullOrWhiteSpace())
                                    {
                                        // Based on RFC 2046 states in section 4.5.1:
                                        // The "octet-stream" subtype is used to indicate that a body contains arbitrary binary data.
                                        // The best default value for an unknown type.
                                        binaryFile.MimeType = "application/octet-stream";
                                    }

                                    if (binaryFile.FileName.IsNullOrWhiteSpace())
                                    {
                                        binaryFile.FileName = "unknown";
                                    }

                                    rockContext.SaveChanges();
                                    processedFileCount += 1;
                                    if (binaryFile.FileSize.HasValue)
                                    {
                                        processedFileSize += binaryFile.FileSize.Value;
                                    }
                                    var progressMessage = string.Format("{0} out of {1} complete", processedFileCount, binaryFiles.Count);
                                    _hubContext.Clients.All.updateProgress(decimal.Round((decimal)processedFileCount / (decimal)totalFileCount * 100), progressMessage);

                                }
                                catch (Exception ex)
                                {
                                    descriptionList.Add(binaryFile.FileName, ex.Message);
                                    WriteProgressMessage(panelMessage, descriptionList.Html);
                                }
                            }
                        }

                        if ( binaryFiles.Count == processedFileCount )
                        {
                            var successMessage = string.Format( @"<div class='alert alert-success' role='alert'> Results: {0} {1} {2} successfully transfered from the {3} storage provider to the {4} storage provider. <br> <a href=""#"" class=""btn btn-success btn-xs"" onclick=""window.location.reload(false);"">Move Additional Files</a>",
                                                binaryFiles.Count,
                                                "file".PluralizeIf( binaryFiles.Count > 1),
                                                binaryFiles.Count > 1 ? "were" : "was",
                                                EntityTypeCache.Get( sourceStorageTypeId.Value ).FriendlyName.SplitCase(),
                                                binaryFileType.StorageEntityType.FriendlyName.SplitCase() );

                            if ( binaryFiles.Count == 1 )
                            {
                                successMessage += string.Format( "<p>You can test the moved file with the this link: <a target='_blank' href='{0}GetFile.ashx?Guid={1}'>Test Link</a></p>", System.Web.VirtualPathUtility.ToAbsolute( "~" ), binaryFiles.First().Guid );
                            }

                            successMessage += "</div>";

                            _hubContext.Clients.All.showResults( this.SignalRNotificationKey, successMessage );
                        }
                        else
                        {
                            var warningMessage = string.Format( "<div class='alert alert-warning' role='alert'> {0} Files were successfully transfered from the {1} storage provider to the {2} storage provider <br/> {3}</div>",
                                                processedFileCount,
                                                EntityTypeCache.Get( sourceStorageTypeId.Value ).FriendlyName.SplitCase(),
                                                binaryFileType.StorageEntityType.FriendlyName.SplitCase(),
                                                descriptionList.Html );

                            _hubContext.Clients.All.showResults( this.SignalRNotificationKey, warningMessage );
                        }
                    }
                }
                catch ( Exception ex )
                {
                    var errorMessage = string.Format( "<div class='alert alert-danger' role='alert'>Error in the middle of processing. {0}  </div>", ex.Message );
                    _hubContext.Clients.All.showResults( this.SignalRNotificationKey, errorMessage );
                }
            } );

            migrateTask.Start();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the btpFileType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btpFileType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var binaryFileTypeId = btpFileType.SelectedValueAsInt();
            if ( binaryFileTypeId.HasValue )
            {
                _currentBinaryFileType = new BinaryFileTypeService( rockContext ).Get( binaryFileTypeId.Value );
                var currentStorageType = _currentBinaryFileType.StorageEntityType;
                var storageTypes = GetStorageTypes( rockContext, binaryFileTypeId, currentStorageType );
                ShowSelectCriteria( currentStorageType, storageTypes );
            }
            else
            {
                pnlSelectCriteria.Visible = false;
            }

            pnlProgress.Visible = false;
        }


        #endregion

        #region Private Methods

        /// <summary>
        /// Processes the confirmation.
        /// </summary>
        private void ProcessConfirmation()
        {
            pnlWarning.Visible = false;
            pnlActions.Visible = true;
            pnlConfirmed.Visible = true;
        }

        /// <summary>
        /// Shows the select criteria.
        /// </summary>
        /// <param name="currentStorageType">Type of the current storage.</param>
        /// <param name="storageTypes">The storage types.</param>
        private void ShowSelectCriteria( EntityType currentStorageType, List<StorageType> storageTypes )
        {
            pnlSelectCriteria.Visible = true;

            nbMessages.Visible = false;

            gStorageLists.DataSource = storageTypes;
            gStorageLists.DataBind();

            BindSourceStorageDropDown( currentStorageType, storageTypes );


        }

        /// <summary>
        /// Binds the source storage drop down.
        /// </summary>
        /// <param name="currentStorageType">Type of the current storage.</param>
        /// <param name="storageTypes">The storage types.</param>
        private void BindSourceStorageDropDown( EntityType currentStorageType, List<StorageType> storageTypes )
        {
            var sourceStorageTypes = storageTypes.Where( a => a.Id != currentStorageType.Id );

            if ( sourceStorageTypes.Any() )
            {
                lDestinationStorage.Text = string.Format( "Current Settings ({0})", currentStorageType.FriendlyName );
                ddlSourceStorage.Items.Clear();
                ddlSourceStorage.DataSource = sourceStorageTypes;
                ddlSourceStorage.DataTextField = "Name";
                ddlSourceStorage.DataValueField = "Id";
                ddlSourceStorage.DataBind();
                ddlSourceStorage.Items.Insert( 0, new ListItem() );

                pnlAction.Visible = true;
                btnMove.Visible = true;
            }
            else
            {
                nbMessages.Visible = true;
                nbMessages.NotificationBoxType = NotificationBoxType.Info;
                nbMessages.Text = string.Format("Currently there are no files stored in the '{0}' file type to transition to the current storage provider '{1}'. To transition files you must first set the file type's storage provider to the new type with the appropriate settings.", _currentBinaryFileType.Name, _currentBinaryFileType.StorageEntityType.FriendlyName );
                pnlAction.Visible = false;
                btnMove.Visible = false;
            }
        }

        /// <summary>
        /// Gets the storage types.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="binaryFileTypeId">The binary file type identifier.</param>
        /// <param name="currentStorageType">Type of the current storage.</param>
        /// <returns></returns>
        private List<StorageType> GetStorageTypes( RockContext rockContext, int? binaryFileTypeId, EntityType currentStorageType )
        {
            var storageTypes = new BinaryFileService( rockContext )
                                    .Queryable()
                                    .Where( a => a.BinaryFileTypeId == binaryFileTypeId.Value &&
                                    a.StorageEntityTypeId.HasValue )
                                    .GroupBy( a => a.StorageEntityTypeId.Value )
                                    .Select( a => new StorageType
                                    {
                                        Id = a.Key,
                                        Files = a.Count(),
                                        StorageSize = a.Sum( b => b.FileSize ?? 0 )
                                    } ).ToList();

            foreach ( var storage in storageTypes )
            {
                storage.Name = EntityTypeCache.Get( storage.Id ).FriendlyName.SplitCase();
            }

            if ( !storageTypes.Any( a => a.Id == currentStorageType.Id ) )
            {
                storageTypes.Insert( 0, new StorageType
                {
                    Id = currentStorageType.Id,
                    Name = currentStorageType.FriendlyName.SplitCase(),
                    Files = 0,
                    StorageSize = default( long )
                } );
            }

            if ( storageTypes.Any() )
            {
                var index = storageTypes.FindIndex( a => a.Id == currentStorageType.Id );
                if ( index != 0 )
                {
                    var item = storageTypes[index];
                    storageTypes.RemoveAt( index );
                    storageTypes.Insert( 0, item );
                }

                storageTypes[0].Name = string.Format( "{0} (Current)", storageTypes[0].Name );
            }

            return storageTypes;
        }

        /// <summary>
        /// Writes the progress message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void WriteProgressMessage( string message, string result )
        {
            _hubContext.Clients.All.receiveNotification( this.SignalRNotificationKey, message, result );
        }

        #endregion

        #region Helper Class

        /// <summary>
        /// Class to hold statistics on the storage providers
        /// </summary>
        public class StorageType
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Files { get; set; }
            public long StorageSize { get; set; }
            public string StorageFormatted
            {
                get
                {
                    return String.Format( "{0:0.00} {1}", (decimal)((decimal)StorageSize / (decimal)1048576), "MB" );
                }
            }
        }

        #endregion
    }
}