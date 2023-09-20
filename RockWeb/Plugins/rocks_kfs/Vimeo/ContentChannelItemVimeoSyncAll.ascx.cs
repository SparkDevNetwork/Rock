// <copyright>
// Copyright 2019 by Kingdom First Solutions
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;

using rocks.kfs.Vimeo;
using VimeoDotNet;

namespace RockWeb.Plugins.rocks_kfs.Vimeo
{
    #region Block Attributes

    [DisplayName( "Content Channel Item Vimeo Sync All" )]
    [Category( "KFS > Vimeo" )]
    [Description( "Syncs all Vimeo data into Content Channel Item Attributes." )]

    #endregion

    #region Block Settings

    [TextField( "Vimeo Id Key", "The attribute key containing the Vimeo Id", true, "", "", 0 )]
    [TextField( "Vimeo User Id", "The User Id of the account to sync.", true, "", "", 1 )]
    [EncryptedTextField( "Access Token", "The authentication token for Vimeo.", true, "", "", 2 )]
    [BooleanField( "Sync Name", "Flag indicating if video name should be stored.", true, "", 3 )]
    [BooleanField( "Sync Description", "Flag indicating if video description should be stored.", true, "", 4 )]
    [TextField( "Image Attribute Key", "The Image Attribute Key that the Vimeo Image URL should be stored in. Leave blank to never sync.", false, "", "", 5 )]
    [IntegerField( "Image Width", "The desired width of image to store link to.", false, 1920, "", 6 )]
    [TextField( "Duration Attribute Key", "The Duration Attribute Key that the Vimeo Duration should be stored in. Leave blank to never sync.", false, "", "", 7 )]
    [TextField( "HD Video Attribute Key", "The HD Video Attribute Key that the HD Video should be stored in. Leave blank to never sync.", false, "", "", 8 )]
    [TextField( "SD Video Attribute Key", "The SD Video Attribute Key that the SD Video should be stored in. Leave blank to never sync.", false, "", "", 9 )]
    [TextField( "HLS Video Attribute Key", "The HLS Video Attribute Key that the HLS Video should be stored in. Leave blank to never sync.", false, "", "", 10 )]
    [IntegerField( "Batch Size", "The number of Content Channel Items to process per context save.", true, 25, "Advanced", 11 )]

    #endregion

    public partial class VimeoSyncAll : Rock.Web.UI.RockBlock
    {
        #region Fields

        private int _batchSize = 25;
        private int _contentChannelId = 0;
        private string _accessToken = string.Empty;
        private string _imageAttributeKey = string.Empty;
        private string _durationAttributeKey = string.Empty;
        private string _hdVideoAttributeKey = string.Empty;
        private string _sdVideoAttributeKey = string.Empty;
        private string _hlsVideoAttributeKey = string.Empty;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            _accessToken = Encryption.DecryptString( GetAttributeValue( "AccessToken" ) );
            _contentChannelId = PageParameter( "contentChannelId" ).AsInteger();
            _batchSize = GetAttributeValue( "BatchSize" ).AsInteger();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( _contentChannelId );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            ShowDetail( _contentChannelId );
        }

        #endregion

        #region Methods

        protected void ShowDetail( int contentChannelId )
        {
            var contentChannel = new ContentChannel();

            if ( contentChannelId != 0 )
            {
                contentChannel = new ContentChannelService( new RockContext() )
                    .Queryable()
                    .FirstOrDefault( c => c.Id == contentChannelId );
            }

            if ( GetAttributeValue( "VimeoUserId" ).IsNullOrWhiteSpace() || GetAttributeValue( "VimeoIdKey" ).IsNullOrWhiteSpace() || contentChannel == null || contentChannel.Id == 0 )
            {
                pnlVimeoSync.Visible = false;
            }
            else
            {
                cblSyncOptions.Items.Clear();

                _imageAttributeKey = GetAttributeValue( "ImageAttributeKey" );
                _durationAttributeKey = GetAttributeValue( "DurationAttributeKey" );
                _hdVideoAttributeKey = GetAttributeValue( "HDVideoAttributeKey" );
                _sdVideoAttributeKey = GetAttributeValue( "SDVideoAttributeKey" );
                _hlsVideoAttributeKey = GetAttributeValue( "HLSVideoAttributeKey" );

                if ( GetAttributeValue( "SyncName" ).AsBoolean() )
                {
                    var item = new ListItem();
                    item.Text = "Name";
                    item.Selected = true;
                    cblSyncOptions.Items.Add( item );
                }

                if ( GetAttributeValue( "SyncDescription" ).AsBoolean() )
                {
                    var item = new ListItem();
                    item.Text = "Description";
                    item.Selected = true;
                    cblSyncOptions.Items.Add( item );
                }

                if ( !string.IsNullOrWhiteSpace( _imageAttributeKey ) )
                {
                    var item = new ListItem();
                    item.Text = "Image";
                    item.Selected = true;
                    cblSyncOptions.Items.Add( item );
                }
                if ( !string.IsNullOrWhiteSpace( _durationAttributeKey ) )
                {
                    var item = new ListItem();
                    item.Text = "Duration";
                    item.Selected = true;
                    cblSyncOptions.Items.Add( item );
                }
                if ( !string.IsNullOrWhiteSpace( _hdVideoAttributeKey ) )
                {
                    var item = new ListItem();
                    item.Text = "HD Video";
                    item.Selected = true;
                    cblSyncOptions.Items.Add( item );
                }
                if ( !string.IsNullOrWhiteSpace( _sdVideoAttributeKey ) )
                {
                    var item = new ListItem();
                    item.Text = "SD Video";
                    item.Selected = true;
                    cblSyncOptions.Items.Add( item );
                }
                if ( !string.IsNullOrWhiteSpace( _hlsVideoAttributeKey ) )
                {
                    var item = new ListItem();
                    item.Text = "HLS Video";
                    item.Selected = true;
                    cblSyncOptions.Items.Add( item );
                }

                if ( cblSyncOptions.Items.Count > 0 )
                {
                    pnlVimeoSync.Visible = true;
                }
                else
                {
                    pnlVimeoSync.Visible = false;
                }
            }
        }

        protected void btnVimeoSync_Click( object sender, EventArgs e )
        {
            SyncVimeo();
            Response.Redirect( Request.RawUrl );
        }

        private void SyncVimeo()
        {
            var lookupContext = new RockContext();
            var contentChannelItems = new List<ContentChannelItem>();
            var completed = 0;

            var client = new VimeoClient( _accessToken );
            var vimeo = new Video();
            var width = GetAttributeValue( "ImageWidth" ).AsInteger();
            long userId = GetAttributeValue( "VimeoUserId" ).AsInteger();
            var videos = vimeo.GetVideos( client, userId, width );

            var vimeoIdKey = GetAttributeValue( "VimeoIdKey" );

            foreach ( var video in videos )
            {
                var contentChannelItem = new ContentChannelItemService( lookupContext )
                    .Queryable( "ContentChannel,ContentChannelType" )
                    .WhereAttributeValue( lookupContext, vimeoIdKey, video.vimeoId.ToString() )
                    .FirstOrDefault( i => i.ContentChannelId == _contentChannelId );

                if ( contentChannelItem != null && contentChannelItem.Id != 0 )
                {
                    if ( contentChannelItem.Attributes == null )
                    {
                        contentChannelItem.LoadAttributes();
                    }

                    var cbName = cblSyncOptions.Items.FindByValue( "Name" );
                    if ( cbName != null && cbName.Selected == true )
                    {
                        contentChannelItem.Title = video.name;
                    }

                    var cbDescription = cblSyncOptions.Items.FindByValue( "Description" );
                    if ( cbDescription != null && cbDescription.Selected == true )
                    {
                        contentChannelItem.Content = video.description;
                    }

                    var cbImage = cblSyncOptions.Items.FindByValue( "Image" );
                    if ( cbImage != null && cbImage.Selected == true )
                    {
                        contentChannelItem.AttributeValues[_imageAttributeKey].Value = video.imageUrl;
                    }

                    var cbDuration = cblSyncOptions.Items.FindByValue( "Duration" );
                    if ( cbDuration != null && cbDuration.Selected == true )
                    {
                        contentChannelItem.AttributeValues[_durationAttributeKey].Value = video.duration.ToString();
                    }

                    var cbHDVideo = cblSyncOptions.Items.FindByValue( "HD Video" );
                    if ( cbHDVideo != null && cbHDVideo.Selected == true && !string.IsNullOrWhiteSpace( video.hdLink ) )
                    {
                        contentChannelItem.AttributeValues[_hdVideoAttributeKey].Value = video.hdLink;
                    }

                    var cbSDVideo = cblSyncOptions.Items.FindByValue( "SD Video" );
                    if ( cbSDVideo != null && cbSDVideo.Selected == true && !string.IsNullOrWhiteSpace( video.sdLink ) )
                    {
                        contentChannelItem.AttributeValues[_sdVideoAttributeKey].Value = video.sdLink;
                    }

                    var cbHLSVideo = cblSyncOptions.Items.FindByValue( "HLS Video" );
                    if ( cbHLSVideo != null && cbHLSVideo.Selected == true && !string.IsNullOrWhiteSpace( video.hlsLink ) )
                    {
                        contentChannelItem.AttributeValues[_hlsVideoAttributeKey].Value = video.hlsLink;
                    }

                    contentChannelItems.Add( contentChannelItem );
                    completed++;

                    if ( completed % _batchSize < 1 )
                    {
                        SaveContentChannelItems( contentChannelItems );
                        contentChannelItems.Clear();
                    }
                }
            }

            // Save leftovers
            if ( contentChannelItems.Any() )
            {
                SaveContentChannelItems( contentChannelItems );
                contentChannelItems.Clear();
            }
        }

        private void SaveContentChannelItems( List<ContentChannelItem> contentChannelItems )
        {
            var rockContext = new RockContext();
            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();
                foreach ( var contentChannelItem in contentChannelItems )
                {
                    contentChannelItem.SaveAttributeValues( rockContext );
                }
            } );
        }

        #endregion
    }
}
