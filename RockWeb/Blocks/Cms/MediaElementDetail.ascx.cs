// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Media;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Media Element Detail" )]
    [Category( "CMS" )]
    [Description( "Edit details of a Media Element" )]

    public partial class MediaElementDetail : RockBlock, IDetailBlock
    {
        #region Properties

        private List<MediaElementFileDataStateful> FileDataState { get; set; }

        private List<MediaElementThumbnailDataStateful> ThumbnailDataState { get; set; }

        #endregion

        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string MediaFolderId = "MediaFolderId";
            public const string MediaElementId = "MediaElementId";
        }

        #endregion PageParameterKey

        #region ViewStateKeys

        private static class ViewStateKey
        {
            public const string FileDataState = "FileDataState";
            public const string ThumbnailDataState = "ThumbnailDataState";
        }

        #endregion ViewStateKeys

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            FileDataState = ViewState[ViewStateKey.FileDataState] as List<MediaElementFileDataStateful> ?? new List<MediaElementFileDataStateful>();
            ThumbnailDataState = ViewState[ViewStateKey.ThumbnailDataState] as List<MediaElementThumbnailDataStateful> ?? new List<MediaElementThumbnailDataStateful>();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gMediaFiles.Actions.AddClick += gMediaFiles_Add;
            gMediaFiles.DataKeyNames = new string[] { "Guid" };
            gThumbnailFiles.Actions.AddClick += gThumbnailFiles_Add;
            gThumbnailFiles.DataKeyNames = new string[] { "Guid" };

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( PageParameterKey.MediaElementId ).AsInteger(), PageParameter( PageParameterKey.MediaFolderId ).AsIntegerOrNull() );
            }
            else
            {
                var disAllowManualEntry = hfDisallowManualEntry.Value.AsBoolean();
                IsAllowManualEntry( !disAllowManualEntry );
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState[ViewStateKey.FileDataState] = FileDataState;
            ViewState[ViewStateKey.ThumbnailDataState] = ThumbnailDataState;

            return base.SaveViewState();
        }

        /// <summary>
        /// Gets the bread crumbs.
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? mediaElementId = PageParameter( pageReference, PageParameterKey.MediaElementId ).AsIntegerOrNull();
            if ( mediaElementId != null )
            {
                var rockContext = new RockContext();

                var mediaElement = new MediaElementService( rockContext ).Get( mediaElementId.Value );

                if ( mediaElement != null )
                {
                    breadCrumbs.Add( new BreadCrumb( mediaElement.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Media Element", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var mediaElement = new MediaElementService( new RockContext() ).Get( hfId.Value.AsInteger() );
            ShowEditDetails( mediaElement );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( PageParameter( PageParameterKey.MediaElementId ).AsInteger(), PageParameter( PageParameterKey.MediaFolderId ).AsIntegerOrNull() );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            var rockContext = new RockContext();
            var mediaElementService = new MediaElementService( rockContext );
            var mediaElementId = hfId.Value.AsIntegerOrNull();
            MediaElement mediaElement = null;
            if ( mediaElementId.HasValue )
            {
                mediaElement = mediaElementService.Get( mediaElementId.Value );
            }

            var isNew = mediaElement == null;

            if ( isNew )
            {
                mediaElement = new MediaElement()
                {
                    Id = 0,
                    MediaFolderId = hfMediaFolderId.ValueAsInt()
                };
                mediaElementService.Add( mediaElement );
            }

            mediaElement.Name = tbName.Text;
            mediaElement.Description = tbDescription.Text;
            mediaElement.DurationSeconds = nbDuration.Text.AsIntegerOrNull();
            mediaElement.ThumbnailDataJson = ThumbnailDataState.ToJson();
            mediaElement.FileDataJson = FileDataState.ToJson();
            rockContext.SaveChanges();

            ShowDetail( mediaElement.Id );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfId.Value.Equals( "0" ) )
            {
                // Canceling on Add
                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString[PageParameterKey.MediaFolderId] = hfMediaFolderId.Value;
                NavigateToParentPage( qryString );
            }
            else
            {
                // Canceling on Edit
                var mediaElement = new MediaElementService( new RockContext() ).Get( int.Parse( hfId.Value ) );
                ShowReadonlyDetails( mediaElement );
            }
        }

        /// <summary>
        /// Handles the Add event of the gMediaFiles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gMediaFiles_Add( object sender, EventArgs e )
        {
            gMediaFiles_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gMediaFiles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gMediaFiles_Edit( object sender, RowEventArgs e )
        {
            var mediaFileGuid = ( Guid ) e.RowKeyValue;
            gMediaFiles_ShowEdit( mediaFileGuid );
        }

        /// <summary>
        /// Handles the Add event of the gThumbnailFiles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gThumbnailFiles_Add( object sender, EventArgs e )
        {
            gThumbnailFiles_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gThumbnailFiles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gThumbnailFiles_Edit( object sender, RowEventArgs e )
        {
            var thumbnailFileGuid = ( Guid ) e.RowKeyValue;
            gThumbnailFiles_ShowEdit( thumbnailFileGuid );
        }

        /// <summary>
        /// Handles the SaveClick event of the mdMediaFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdMediaFile_SaveClick( object sender, EventArgs e )
        {
            Guid? guid = hfMediaElementData.Value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                var mediaElementData = FileDataState.Where( t => t.Guid.Equals( guid.Value ) ).FirstOrDefault();
                if ( mediaElementData == null )
                {
                    mediaElementData = new MediaElementFileDataStateful();
                    FileDataState.Add( mediaElementData );
                }

                mediaElementData.PublicName = tbPublicName.Text;
                mediaElementData.AllowDownload = cbAllowDownload.Checked;
                mediaElementData.Link = urlLink.Text;
                mediaElementData.Quality = ddlQuality.SelectedValueAsEnum<MediaElementQuality>( MediaElementQuality.Other );
                mediaElementData.Format = tbFormat.Text;
                mediaElementData.Width = nbWidth.Text.AsInteger();
                mediaElementData.Height = nbHeight.Text.AsInteger();
                mediaElementData.FPS = nbFPS.Text.AsInteger();

                long size;
                if ( !long.TryParse( nbSize.Text, out size ) )
                {
                    size = 0;
                }
                mediaElementData.Size = size;

                BindMediaFileGrid();
            }

            HideDialog();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdThumbnailFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdThumbnailFile_SaveClick( object sender, EventArgs e )
        {
            Guid? guid = hfThumbnailFile.Value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                var thumbnailData = ThumbnailDataState.Where( t => t.Guid.Equals( guid.Value ) ).FirstOrDefault();
                if ( thumbnailData == null )
                {
                    thumbnailData = new MediaElementThumbnailDataStateful();
                    ThumbnailDataState.Add( thumbnailData );
                }

                thumbnailData.Link = urlThumbnailLink.Text;
                thumbnailData.Width = nbThumbnailWidth.Text.AsInteger();
                thumbnailData.Height = nbThumbnailHeight.Text.AsInteger();

                long size;
                if ( !long.TryParse( nbThumbnailSize.Text, out size ) )
                {
                    size = 0;
                }
                thumbnailData.Size = size;

                BindThumbnailDataGrid();
            }

            HideDialog();
        }

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="mediaElementId">The media element identifier.</param>
        public void ShowDetail( int mediaElementId )
        {
            ShowDetail( mediaElementId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="mediaElementId">The media element identifier.</param>
        /// <param name="mediaFolderId">The media folder identifier.</param>
        public void ShowDetail( int mediaElementId, int? mediaFolderId )
        {
            var rockContext = new RockContext();
            var mediaElementService = new MediaElementService( rockContext );
            MediaElement mediaElement = null;

            if ( !mediaElementId.Equals( 0 ) )
            {
                mediaElement = mediaElementService.Get( mediaElementId );
            }

            if ( mediaElement == null )
            {
                MediaFolder mediaFolder = null;
                if ( mediaFolderId.HasValue )
                {
                    mediaFolder = new MediaFolderService( rockContext ).Get( mediaFolderId.Value );
                }

                if ( mediaFolder != null )
                {
                    mediaElement = new MediaElement { Id = 0, MediaFolderId = mediaFolder.Id };
                }
                else
                {
                    pnlView.Visible = false;
                    return;
                }
            }

            hfId.SetValue( mediaElement.Id );
            hfMediaFolderId.SetValue( mediaElement.MediaFolderId );
            pnlView.Visible = true;

            bool readOnly = false;

            var mediaComponent = mediaElement.MediaFolder.MediaAccount.GetMediaAccountComponent();
            if ( mediaComponent != null && !mediaComponent.AllowsManualEntry )
            {
                readOnly = true;
            }

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( MediaFolder.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                ShowReadonlyDetails( mediaElement );
            }
            else
            {
                btnEdit.Visible = true;
                if ( mediaElement.Id > 0 )
                {
                    ShowReadonlyDetails( mediaElement );
                }
                else
                {
                    ShowEditDetails( mediaElement );
                }
            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="mediaAccount">The media element.</param>
        private void ShowReadonlyDetails( MediaElement mediaElement )
        {
            FileDataState = mediaElement.FileDataJson.FromJsonOrNull<List<MediaElementFileDataStateful>>();
            ThumbnailDataState = mediaElement.ThumbnailDataJson.FromJsonOrNull<List<MediaElementThumbnailDataStateful>>();

            SetEditMode( false );

            lActionTitle.Text = mediaElement.Name.FormatAsHtmlTitle();

            var descriptionList = new DescriptionList();
            descriptionList.Add( "Folder", mediaElement.MediaFolder.Name );
            if ( mediaElement.Description.IsNotNullOrWhiteSpace() )
            {
                descriptionList.Add( "Description", mediaElement.Description );
            }

            if ( mediaElement.DurationSeconds.HasValue )
            {
                descriptionList.Add( "Duration", mediaElement.DurationSeconds.ToFriendlyDuration() );
            }

            lDescription.Text = descriptionList.Html;

            lMetricData.Text = mediaElement.MediaFolder.MediaAccount.GetMediaAccountComponent()
                ?.GetMediaElementHtmlSummary( mediaElement );

            gViewMediaFiles.DataSource = FileDataState
              .ToList();
            gViewMediaFiles.DataBind();

            gViewThumbnailFiles.DataSource = ThumbnailDataState
              .ToList();
            gViewThumbnailFiles.DataBind();
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="mediaElement">The media element.</param>
        public void ShowEditDetails( MediaElement mediaElement )
        {
            if ( mediaElement.Id == 0 )
            {
                lActionTitle.Text = ActionTitle.Add( "Media Element" ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitle.Text = mediaElement.Name.FormatAsHtmlTitle();
            }

            SetEditMode( true );

            tbName.Text = mediaElement.Name;
            tbDescription.Text = mediaElement.Description;
            nbDuration.Text = mediaElement.DurationSeconds.ToStringSafe();

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( MediaFolder.FriendlyTypeName );
            }

            var mediaFolder = new MediaFolderService( new RockContext() ).Get( mediaElement.MediaFolderId );
            if ( mediaFolder != null )
            {
                var mediaAccountComponent = GetMediaAccountComponent( mediaFolder.MediaAccount );
                if ( mediaAccountComponent != null )
                {
                    readOnly = !mediaAccountComponent.AllowsManualEntry;
                }
            }

            hfDisallowManualEntry.Value = readOnly.ToString();
            IsAllowManualEntry( !readOnly );

            tbName.ReadOnly = readOnly;
            tbDescription.ReadOnly = readOnly;
            nbDuration.ReadOnly = readOnly;

            BindThumbnailDataGrid();
            BindMediaFileGrid();
        }

        /// <summary>
        /// Allows the manual entry.
        /// </summary>
        private void IsAllowManualEntry( bool allowManualEntry )
        {
            gMediaFiles.Actions.ShowAdd = allowManualEntry;
            var editField = gMediaFiles.ColumnsOfType<EditField>().FirstOrDefault();
            if ( editField != null )
            {
                editField.Visible = allowManualEntry;
            }

            gThumbnailFiles.Actions.ShowAdd = allowManualEntry;
            var thumbnailEditField = gThumbnailFiles.ColumnsOfType<EditField>().FirstOrDefault();
            if ( editField != null )
            {
                thumbnailEditField.Visible = allowManualEntry;
            }
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Shows the edit dialog for the specified media file.
        /// </summary>
        /// <param name="mediaFileGuid">The media file identifier.</param>
        protected void gMediaFiles_ShowEdit( Guid mediaFileGuid )
        {
            var mediaElementData = FileDataState.FirstOrDefault( l => l.Guid.Equals( mediaFileGuid ) );

            ddlQuality.BindToEnum<MediaElementQuality>( true );

            if ( mediaElementData != null )
            {
                hfMediaElementData.Value = mediaElementData.Guid.ToString();
                tbPublicName.Text = mediaElementData.PublicName;
                cbAllowDownload.Checked = mediaElementData.AllowDownload;
                urlLink.Text = mediaElementData.Link;
                ddlQuality.SetValue( mediaElementData.Quality.ConvertToInt() );
                tbFormat.Text = mediaElementData.Format;
                nbWidth.IntegerValue = mediaElementData.Width;
                nbHeight.IntegerValue = mediaElementData.Height;
                nbFPS.IntegerValue = mediaElementData.FPS;
                nbSize.Text = mediaElementData.Size.ToString();
            }
            else
            {
                hfMediaElementData.Value = Guid.Empty.ToString();
                tbPublicName.Text = string.Empty;
                urlLink.Text = string.Empty;
                ddlQuality.SelectedValue = string.Empty;
                tbFormat.Text = string.Empty;
                nbWidth.IntegerValue = null;
                nbHeight.IntegerValue = null;
                nbFPS.IntegerValue = null;
                nbSize.IntegerValue = null;
                cbAllowDownload.Checked = true;
            }

            ShowDialog( "MEDIAFILES", true );
        }

        /// <summary>
        /// Shows the edit dialog for the specified thumbnail file.
        /// </summary>
        /// <param name="thumbnailFileGuid">The thumbnail file identifier.</param>
        protected void gThumbnailFiles_ShowEdit( Guid thumbnailFileGuid )
        {
            var thumbnailData = ThumbnailDataState.FirstOrDefault( l => l.Guid.Equals( thumbnailFileGuid ) );

            if ( thumbnailData != null )
            {
                hfThumbnailFile.Value = thumbnailData.Guid.ToString();
                urlThumbnailLink.Text = thumbnailData.Link;
                nbThumbnailWidth.IntegerValue = thumbnailData.Width;
                nbThumbnailHeight.IntegerValue = thumbnailData.Height;
                nbThumbnailSize.Text = thumbnailData.Size.ToString();
            }
            else
            {
                hfThumbnailFile.Value = Guid.Empty.ToString();
                urlThumbnailLink.Text = string.Empty;
                nbThumbnailWidth.IntegerValue = null;
                nbThumbnailHeight.IntegerValue = null;
                nbThumbnailSize.IntegerValue = null;
            }

            ShowDialog( "THUMBNAILFILES", true );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "MEDIAFILES":
                    {
                        mdMediaFile.Show();
                        break;
                    }

                case "THUMBNAILFILES":
                    {
                        mdThumbnailFile.Show();
                        break;
                    }
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "MEDIAFILES":
                    {
                        mdMediaFile.Hide();
                        break;
                    }

                case "THUMBNAILFILES":
                    {
                        mdThumbnailFile.Hide();
                        break;
                    }
            }

            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Binds the media file grid.
        /// </summary>
        private void BindMediaFileGrid()
        {
            FileDataState = FileDataState ?? new List<MediaElementFileDataStateful>();
            gMediaFiles.DataSource = FileDataState
                .ToList();
            gMediaFiles.DataBind();
        }

        /// <summary>
        /// Binds the thumbnail file grid.
        /// </summary>
        private void BindThumbnailDataGrid()
        {
            ThumbnailDataState = ThumbnailDataState ?? new List<MediaElementThumbnailDataStateful>();
            gThumbnailFiles.DataSource = ThumbnailDataState
                .ToList();
            gThumbnailFiles.DataBind();
        }

        /// <summary>
        /// Gets the media account component.
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <returns></returns>
        private MediaAccountComponent GetMediaAccountComponent( MediaAccount mediaAccount )
        {
            var componentEntityTypeId = mediaAccount != null ? mediaAccount.ComponentEntityTypeId : ( int? ) null;

            if ( componentEntityTypeId.HasValue )
            {
                var componentEntityType = EntityTypeCache.Get( componentEntityTypeId.Value );
                return componentEntityType == null ? null : MediaAccountContainer.GetComponent( componentEntityType.Name );
            }

            return null;
        }

        #endregion Internal Methods

        #region Support Classes

        /// <summary>
        /// Provides an additional Guid property that can be used to track
        /// individual items in the grids when editing.
        /// </summary>
        /// <seealso cref="Rock.Media.MediaElementFileData" />
        [Serializable]
        private class MediaElementFileDataStateful : MediaElementFileData
        {
            [JsonIgnore]
            public Guid Guid { get; set; } = Guid.NewGuid();
        }

        /// <summary>
        /// Provides an additional Guid property that can be used to track
        /// individual items in the grids when editing.
        /// </summary>
        /// <seealso cref="Rock.Media.MediaElementThumbnailData" />
        [Serializable]
        private class MediaElementThumbnailDataStateful : MediaElementThumbnailData
        {
            [JsonIgnore]
            public Guid Guid { get; set; } = Guid.NewGuid();
        }

        #endregion
    }
}