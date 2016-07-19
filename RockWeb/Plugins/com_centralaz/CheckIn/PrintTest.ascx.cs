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
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;

using com.centralaz.CheckInLabels;

namespace RockWeb.Plugins.com_centralaz.CheckIn
{
    /// <summary>
    /// Used to quickly print a test label to a printer.
    /// </summary>
    [DisplayName( "Print Test" )]
    [Category( "com_centralaz > Check-in" )]
    [Description( "Used to quickly print a test label to a printer." )]
    public partial class PrintTest : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        
        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

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
                var rockContext = new RockContext();
                
                // Bind Devices
                var deviceService = new DeviceService( rockContext);
                var devices = deviceService.Queryable().AsNoTracking().OrderBy( p => p.Name).ToList();
                ddlDevice.DataTextField = "Name";
                ddlDevice.DataValueField = "Guid";
                ddlDevice.DataSource = devices;
                ddlDevice.DataBind();

                // Bind Locations
                Guid binaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.CHECKIN_LABEL.AsGuid();
                var binaryFileService = new BinaryFileService( rockContext );
                var queryable = binaryFileService.Queryable().AsNoTracking().Where( f => f.BinaryFileType.Guid == binaryFileTypeGuid );

                ddlLabel.DataTextField = "FileName";
                ddlLabel.DataValueField = "Guid";
                ddlLabel.DataSource = queryable.ToList();
                ddlLabel.DataBind();
            }
            else
            {
                nbMessage.CssClass = "alert";
                nbMessage.Text = "";
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        /// <summary>
        /// Handles the Click event of the bbtnPrint control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bbtnPrint_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            var label = ddlLabel.SelectedItem;
            var selectedDevice = ddlDevice.SelectedItem;

            DeviceService deviceService = new DeviceService( rockContext );
            Device device = deviceService.Get( selectedDevice.Value.AsGuid() );

            CheckInPerson checkinPerson = new CheckInPerson();
            if ( ppPerson.PersonId.HasValue )
            {
                PersonService personService = new PersonService( rockContext );
                var person = personService.Get( ppPerson.PersonId.Value );
                checkinPerson.Person = person;
            }
            else
            {
                checkinPerson.Person = CurrentPerson;
                checkinPerson.Person.NickName = "TEST";
                checkinPerson.Person.LastName = "LABEL";
                checkinPerson.Person.BirthMonth = RockDateTime.Now.Month;
                checkinPerson.Person.BirthDay = RockDateTime.Now.Day;
                checkinPerson.Person.GradeOffset = 6;

                checkinPerson.Person.LoadAttributes();

                if ( checkinPerson.Person.AttributeValues.ContainsKey( "Allergy" ) )
                {
                    checkinPerson.Person.AttributeValues["Allergy"].Value = "This is a test... health notes (if any) would be pulled form a child's record and placed here.";
                }

                if ( checkinPerson.Person.AttributeValues.ContainsKey( "LegalNotes" ) )
                {
                    checkinPerson.Person.AttributeValues["LegalNotes"].Value = "True";
                }
            }

            checkinPerson.SecurityCode = "AB1234";

            CheckInGroupType checkinGroupType = new CheckInGroupType();
            checkinGroupType.Groups = new List<CheckInGroup>();
            var groupTypeCache = GroupTypeCache.Read( "6E7AD783-7614-4721-ABC1-35842113EF59".AsGuid() );
            checkinGroupType.GroupType = groupTypeCache;

            var labelCache = KioskLabel.Read( label.Value.AsGuid() );
            var commonMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
            var mergeObjects = new Dictionary<string, object>();
            foreach ( var keyValue in commonMergeFields )
            {
                mergeObjects.Add( keyValue.Key, keyValue.Value );
            }


            mergeObjects.Add( "Person", checkinPerson.Person );
            //mergeObjects.Add( "People", people );
            //mergeObjects.Add( "GroupType", groupType );
            //var groupMembers = groupMemberService.Queryable().AsNoTracking()
            //    .Where( m =>
            //        m.PersonId == person.Person.Id &&
            //        m.GroupId == group.Group.Id )
            //    .ToList();
            //mergeObjects.Add( "GroupMembers", groupMembers );
            CheckInLabel checkinLabel = new CheckInLabel( labelCache, mergeObjects );

            checkinLabel.FileGuid = label.Value.AsGuid();
            checkinLabel.PrinterAddress = device.IPAddress;
            checkinLabel.PrinterDeviceId = device.Id;
            checkinLabel.PrintFrom = PrintFrom.Server;

            List<CheckInLabel> labels = new List<CheckInLabel>();
            labels.Add( checkinLabel );

            checkinGroupType.Labels = labels;
            PrintFromServerLabels( checkinPerson, checkinGroupType, labels );

            bbtnPrint.DataLoadingText = "";
        }

        #endregion

        #region Methods

        /// <summary>
        /// TODO: Move this code to somewhere shareable between the Success.ascx block and this block.
        /// Prints the labels that are the "from server" ones.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="printFromServer">The print from server.</param>
        private void PrintFromServerLabels( CheckInPerson person, CheckInGroupType groupType, IEnumerable<CheckInLabel> printFromServer )
        {
            Socket socket = null;
            bool hasCutter = true;
            string currentIp = string.Empty;
            int numOfLabels = printFromServer.Count();
            int labelIndex = 0;
            foreach ( var label in printFromServer.OrderBy( l => l.Order ) )
            {
                labelIndex++;
                var labelCache = KioskLabel.Read( label.FileGuid );
                if ( labelCache != null )
                {
                    if ( !string.IsNullOrWhiteSpace( label.PrinterAddress ) )
                    {
                        if ( label.PrinterAddress != currentIp )
                        {
                            if ( socket != null && socket.Connected )
                            {
                                socket.Shutdown( SocketShutdown.Both );
                                socket.Close();
                            }

                            currentIp = label.PrinterAddress;
                            var printerIp = new IPEndPoint( IPAddress.Parse( currentIp ), 9100 );
                            var deviceId = label.PrinterDeviceId;
                            hasCutter = GetPrinterCutterOption( deviceId );

                            socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
                            IAsyncResult result = socket.BeginConnect( printerIp, null, null );
                            bool success = result.AsyncWaitHandle.WaitOne( 5000, true );
                        }

                        string printContent = labelCache.FileContent;
                        // This is documented in <\IT\Projects\Rock RMS\CustomProjects\Check-in\Rock Central Check-in Setup and Design.docx>
                        if ( printContent.StartsWith( "Assembly:" ) )
                        {
                            if ( socket != null && socket.Connected )
                            {
                                socket.Shutdown( SocketShutdown.Both );
                                socket.Close();
                            }
                            LoadPrintLabelAndPrint( printContent, label, new CheckInState( label.PrinterDeviceId ?? -1, null, new List<int>() ), person, groupType );
                        }
                        else
                        {
                            foreach ( var mergeField in label.MergeFields )
                            {
                                if ( !string.IsNullOrWhiteSpace( mergeField.Value ) )
                                {
                                    printContent = Regex.Replace( printContent, string.Format( @"(?<=\^FD){0}(?=\^FS)", mergeField.Key ), ZebraFormatString( mergeField.Value ) );
                                }
                                else
                                {
                                    // Remove the box preceding merge field
                                    printContent = Regex.Replace( printContent, string.Format( @"\^FO.*\^FS\s*(?=\^FT.*\^FD{0}\^FS)", mergeField.Key ), string.Empty );
                                    // Remove the merge field
                                    printContent = Regex.Replace( printContent, string.Format( @"\^FD{0}\^FS", mergeField.Key ), "^FD^FS" );
                                }
                            }

                            // Inject the cut command on the last label (if the printer is has a cutter)
                            // otherwise supress the backfeed (^XB)
                            if ( labelIndex == numOfLabels && hasCutter )
                            {
                                printContent = Regex.Replace( printContent.Trim(), @"\" + @"^PQ1,0,1,Y", string.Empty );
                                printContent = Regex.Replace( printContent.Trim(), @"\" + @"^MMT", @"^MMC" );
                            }
                            else
                            {
                                printContent = Regex.Replace( printContent.Trim(), @"\" + @"^XZ$", @"^XB^XZ" );
                            }

                            if ( socket.Connected )
                            {
                                var ns = new NetworkStream( socket );
                                byte[] toSend = System.Text.Encoding.ASCII.GetBytes( printContent );
                                ns.Write( toSend, 0, toSend.Length );
                            }
                            else
                            {
                                //phResults.Controls.Add( new LiteralControl( "<br/>NOTE: Could not connect to printer!" ) );
                                nbMessage.CssClass = "alert alert-danger";
                                nbMessage.Text = "Could not connect to printer!";
                            }
                        }
                    }
                } // labelCache != null
            }

            if ( socket != null && socket.Connected )
            {
                socket.Shutdown( SocketShutdown.Both );
                socket.Close();
            }
        }

        /// <summary>
        /// TODO: Move this code to somewhere shareable between the Success.ascx block and this block.
        /// Gets the printer cutter option from either a "HasCutter" (boolean) attribute
        /// on the printer device or the words "w/Cutter" in the printer's Description.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>true if printer has a cutter; false otherwise</returns>
        protected bool GetPrinterCutterOption( int? deviceId )
        {
            bool hasCutter = false;

            // Get the device from cache
            var currentGroupTypeIds = ( Session["CheckInGroupTypeIds"] != null ) ? Session["CheckInGroupTypeIds"] as List<int> : new List<int>();
            KioskDevice kioskDevice = KioskDevice.Read( deviceId.GetValueOrDefault(), currentGroupTypeIds );
            hasCutter = kioskDevice.Device.GetAttributeValue( "HasCutter" ).AsBoolean();

            // also check the Description for the w/Cutter keywords
            if ( !hasCutter )
            {
                hasCutter = Regex.IsMatch( kioskDevice.Device.Description, "w/Cutter", RegexOptions.IgnoreCase );
            }

            return hasCutter;
        }

        /// <summary>
        /// TODO: Move this code to somewhere shareable between the Success.ascx block and this block.
        /// Zebras the format string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="isJson">if set to <c>true</c> [is json].</param>
        /// <returns></returns>
        private string ZebraFormatString( string input, bool isJson = false )
        {
            if ( isJson )
            {
                return input.Replace( "é", @"\\82" );  // fix acute e
            }
            else
            {
                return input.Replace( "é", @"\82" );  // fix acute e
            }
        }

        #endregion


        #region Helper Methods

        /// <summary>
        /// TODO: Move this code to somewhere shareable between the Success.ascx block and this block.
        /// Loads the print label and print.
        /// </summary>
        /// <param name="assemblyString">The assembly string.</param>
        /// <param name="label">The label.</param>
        /// <param name="checkInState">State of the check in.</param>
        /// <param name="person">The person.</param>
        /// <param name="groupType">Type of the group.</param>
        private void LoadPrintLabelAndPrint( string assemblyString, CheckInLabel label, CheckInState checkInState, CheckInPerson person, CheckInGroupType groupType )
        {
            // Use only the first line
            string line1 = assemblyString.Split( new[] { '\r', '\n' } ).FirstOrDefault();
            // Remove the "Assembly:" prefix
            var assemblyParts = line1.ReplaceCaseInsensitive( "Assembly:", "" ).Trim().Split( ',' );
            var assemblyName = assemblyParts[0];
            var assemblyClass = assemblyParts[1];

            var printLabel = PrintLabelHelper.GetPrintLabelClass( assemblyName, assemblyClass );

            printLabel.Print( label, person, checkInState, groupType );
        }

        #endregion
    }
}