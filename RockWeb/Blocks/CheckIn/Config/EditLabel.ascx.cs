﻿// <copyright>
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
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.CheckIn.Config
{
    [DisplayName( "Edit Label" )]
    [Category( "Check-in > Configuration" )]
    [Description( "Allows editing contents of a label and printing test labels." )]
    public partial class EditLabel : RockBlock
    {
        #region Properties
        Regex regexPrintWidth = new Regex( @"\^PW(\d+)" );
        Regex regexPrintHeight = new Regex( @"\^LL(\d+)" );
        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                using ( var rockContext = new RockContext() )
                {
                    int? binaryFileId = PageParameter( "BinaryFileId" ).AsIntegerOrNull();
                    if ( binaryFileId.HasValue )
                    {
                        hfBinaryFileId.Value = binaryFileId.Value.ToString();
                        pnlOpenFile.Visible = false;
                        btnSave.Visible = true;
                        btnCancel.Visible = true;

                        var binaryFile = new BinaryFileService( rockContext ).Get( binaryFileId.Value );
                        if ( binaryFile != null )
                        {
                            lTitle.Text = binaryFile.FileName;
                            ceLabel.Text = binaryFile.ContentsToString();
                            SetLabelSize( ceLabel.Text );
                        }
                    }
                    else
                    {
                        pnlOpenFile.Visible = true;


                        ddlLabel.Items.Clear();
                        Guid labelTypeGuid = Rock.SystemGuid.BinaryFiletype.CHECKIN_LABEL.AsGuid();
                        foreach ( var labelFile in new BinaryFileService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( f => f.BinaryFileType.Guid == labelTypeGuid )
                            .OrderBy( f => f.FileName ) )
                        {
                            ddlLabel.Items.Add( new ListItem( labelFile.FileName, labelFile.Id.ToString() ) );
                        }
                        ddlLabel.SelectedIndex = 0;
                    }

                    ddlDevice.Items.Clear();
                    var printerDeviceType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_PRINTER.AsGuid() );
                    if ( printerDeviceType != null )
                    {
                        foreach ( var device in new DeviceService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( d =>
                                d.DeviceTypeValueId == printerDeviceType.Id &&
                                d.IPAddress != "" ) )
                        {
                            ddlDevice.Items.Add( new ListItem( device.Name, device.Id.ToString() ) );
                        }

                        if ( ddlDevice.Items.Count > 0 )
                        {
                            ddlDevice.SelectedIndex = 0;
                        }
                    }
                }

                SetLabelImage();

            }
        }

        #endregion

        #region Events

        protected void btnOpen_Click( object sender, EventArgs e )
        {
            ceLabel.Label = "Label Contents";
            btnSave.Visible = false;

            int? fileId = ddlLabel.SelectedValueAsInt();
            if ( fileId.HasValue )
            {
                hfBinaryFileId.Value = fileId.Value.ToString();
                using ( var rockContext = new RockContext() )
                {
                    var file = new BinaryFileService( rockContext ).Get( fileId.Value );
                    if ( file != null )
                    {
                        ceLabel.Text = file.ContentsToString();
                        SetLabelSize( ceLabel.Text );
                        ceLabel.Label = string.Format( file.FileName );
                        btnSave.Text = "Save " + file.FileName;
                        btnSave.Visible = true;
                    }
                }
            }
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            int? binaryFileId = hfBinaryFileId.Value.AsIntegerOrNull();
            if ( binaryFileId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var binaryFileService = new BinaryFileService( rockContext );
                    var binaryFile = binaryFileService.Get( binaryFileId.Value );
                    if ( binaryFile != null )
                    {
                        using ( var stream = new MemoryStream() )
                        {
                            var writer = new StreamWriter( stream );
                            writer.Write( ceLabel.Text );
                            writer.Flush();
                            stream.Position = 0;
                            binaryFile.ContentStream = stream;
                            rockContext.SaveChanges();
                        }
                    }
                }

                int? pageParamFileId = PageParameter( "BinaryFileId" ).AsIntegerOrNull();
                if ( pageParamFileId.HasValue )
                {
                    NavigateToParentPage( new Dictionary<string, string> { { "BinaryFileId", pageParamFileId.Value.ToString() } } );
                }
            }
        }

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            int? pageParamFileId = PageParameter( "BinaryFileId" ).AsIntegerOrNull();
            if ( pageParamFileId.HasValue )
            {
                NavigateToParentPage( new Dictionary<string, string> { { "BinaryFileId", pageParamFileId.Value.ToString() } } );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnPrint_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var device = new DeviceService( rockContext ).Get( ddlDevice.SelectedValueAsInt() ?? 0 );
                if ( device != null )
                {
                    string currentIp = device.IPAddress;
                    var printerIp = new IPEndPoint( IPAddress.Parse( currentIp ), 9100 );

                    var socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
                    IAsyncResult result = socket.BeginConnect( printerIp, null, null );
                    bool success = result.AsyncWaitHandle.WaitOne( 5000, true );

                    if ( socket.Connected )
                    {
                        var ns = new NetworkStream( socket );
                        byte[] toSend = System.Text.Encoding.ASCII.GetBytes( ceLabel.Text );
                        ns.Write( toSend, 0, toSend.Length );

                        socket.Shutdown( SocketShutdown.Both );
                        socket.Close();
                    }
                }
            }
        }

        protected void btnRedraw_Click( object sender, EventArgs e )
        {
            SetLabelImage();
        }

        /// <summary>
        /// Tries to set the size of the label using the ZPL contents to calculate width and height.
        /// </summary>
        /// <param name="zpl">The ZPL.</param>
        private void SetLabelSize( string zpl )
        {
            try
            {
                int dpi = 203;
                switch ( ddlPrintDensity.SelectedValue.AsInteger() )
                {
                    case 6:
                        dpi = 152;
                        break;
                    case 8:
                        dpi = 203;
                        break;
                    case 12:
                        dpi = 300;
                        break;
                    case 24:
                        dpi = 600;
                        break;
                    default:
                        break;
                }

                var widthMatch = regexPrintWidth.Match( zpl );
                if ( widthMatch.Success )
                {
                    var widthInches = ( double ) widthMatch.Groups[1].Value.AsInteger() / ( double ) dpi;
                    var widthRounded = Math.Round( widthInches, 2, MidpointRounding.AwayFromZero );
                    nbLabelWidth.Text = widthRounded.ToString();
                }

                var heightMatch = regexPrintHeight.Match( zpl );
                if ( heightMatch.Success )
                {
                    var heightInches = ( double ) heightMatch.Groups[1].Value.AsInteger() / ( double ) dpi;
                    var heightRounded = Math.Round( heightInches, 2, MidpointRounding.AwayFromZero );
                    nbLabelHeight.Text = heightRounded.ToString();
                }
            }
            catch { };
        }

        private void SetLabelImage()
        {
            string dpmm = ddlPrintDensity.SelectedValue;
            string width = nbLabelWidth.Text;
            string height = nbLabelHeight.Text;
            string labelIndex = nbShowLabel.Text;

            imgLabelary.ImageUrl = string.Format(
                "http://api.labelary.com/v1/printers/{0}dpmm/labels/{1}x{2}/{3}/{4}",
                dpmm, width, height, labelIndex, ceLabel.Text.UrlEncode() );
        }

        #endregion

        }
    }