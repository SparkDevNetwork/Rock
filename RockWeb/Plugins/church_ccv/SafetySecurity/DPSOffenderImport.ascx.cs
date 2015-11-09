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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;
using church.ccv.SafetySecurity.Model;
using OfficeOpenXml;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.church_ccv.SafetySecurity
{
    /// <summary>
    /// Imports DPS Offender records
    /// </summary>
    [DisplayName( "DPS Offender Import" )]
    [Category( "CCV > Safety and Security" )]
    [Description( "Imports DPS Offender records" )]

    public partial class DPSOffenderImport : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the FileUploaded event of the fuImport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fuImport_FileUploaded( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            int leaderRoleId = 123;
            int groupId = 456;
            var qryGroupMembers = new GroupMemberService( rockContext ).Queryable().Where( a => a.GroupId == groupId );
            var groupMembers = qryGroupMembers.Where( a => a.GroupRoleId == leaderRoleId ).OrderBy( a => a.Person.NickName ).ThenBy( a => a.Person.LastName )
                .Union( qryGroupMembers.Where( a => a.GroupRoleId != leaderRoleId ).OrderBy( a => a.Person.NickName ).ThenBy( a => a.Person.LastName ) );

            var binaryFileService = new BinaryFileService( rockContext );
            var binaryFile = binaryFileService.Get( fuImport.BinaryFileId ?? 0 );
            var importData = binaryFile.ContentStream;

            ExcelPackage excelPackage = new ExcelPackage( importData );
            var worksheet = excelPackage.Workbook.Worksheets.FirstOrDefault();

            int colIndex = 1;
            var columnLookup = new Dictionary<string, int>();
            while ( colIndex <= worksheet.Dimension.Columns )
            {
                columnLookup.Add( worksheet.Cells[1, colIndex].Text, colIndex );
                colIndex++;
            }

            int rowIndex = 2;
            var dpsOffenderService = new Service<DPSOffender>( rockContext );
            var qryDpsOffender = dpsOffenderService.Queryable();
            
            int rowsInserted = 0;
            int rowsAlreadyExist = 0;
            var rowsToInsert = new List<DPSOffender>();
            while ( rowIndex <= worksheet.Dimension.Rows )
            {
                var dpsOffender = new DPSOffender();
                dpsOffender.Level = worksheet.Cells[rowIndex, columnLookup["Level"]].Text.AsInteger();
                dpsOffender.FirstName = worksheet.Cells[rowIndex, columnLookup["First_Name"]].Text;
                dpsOffender.LastName = worksheet.Cells[rowIndex, columnLookup["Last_Name"]].Text;
                dpsOffender.MiddleInitial = worksheet.Cells[rowIndex, columnLookup["MI"]].Text;

                dpsOffender.Age = worksheet.Cells[rowIndex, columnLookup["Age"]].Text.AsIntegerOrNull();
                dpsOffender.Height = worksheet.Cells[rowIndex, columnLookup["HT"]].Text.AsIntegerOrNull();
                dpsOffender.Weight = worksheet.Cells[rowIndex, columnLookup["WT"]].Text.AsIntegerOrNull();
                dpsOffender.Race = worksheet.Cells[rowIndex, columnLookup["Race"]].Text;
                dpsOffender.Gender = worksheet.Cells[rowIndex, columnLookup["Sex"]].Text;
                dpsOffender.Hair = worksheet.Cells[rowIndex, columnLookup["Hair"]].Text;
                dpsOffender.Eyes = worksheet.Cells[rowIndex, columnLookup["Eyes"]].Text;
                dpsOffender.ResAddress = worksheet.Cells[rowIndex, columnLookup["Res_Add"]].Text;
                dpsOffender.ResCity = worksheet.Cells[rowIndex, columnLookup["Res_City"]].Text;
                dpsOffender.ResState = worksheet.Cells[rowIndex, columnLookup["Res_State"]].Text;
                dpsOffender.ResZip = worksheet.Cells[rowIndex, columnLookup["Res_Zip"]].Text;
                dpsOffender.Offense = worksheet.Cells[rowIndex, columnLookup["Offense"]].Text;
                dpsOffender.DateConvicted = worksheet.Cells[rowIndex, columnLookup["Date_Convicted"]].Text.AsDateTime();
                dpsOffender.ConvictionState = worksheet.Cells[rowIndex, columnLookup["Conviction_State"]].Text;
                dpsOffender.Absconder = worksheet.Cells[rowIndex, columnLookup["Absconder"]].Text.AsBoolean();

                // see if we have that record already
                var existingRecord = qryDpsOffender.Where( a =>
                    a.FirstName == dpsOffender.FirstName &&
                    a.LastName == dpsOffender.LastName &&
                    a.Race == dpsOffender.Race &&
                    a.Gender == dpsOffender.Gender &&
                    a.ResAddress == dpsOffender.ResAddress &&
                    a.ResCity == dpsOffender.ResCity &&
                    a.ResState == dpsOffender.ResState &&
                    a.ResZip == dpsOffender.ResZip
                    ).Any();

                if ( !existingRecord )
                {
                    rowsInserted++;
                    rowsToInsert.Add( dpsOffender );
                    
                }
                else
                {
                    rowsAlreadyExist++;
                }

                rowIndex++;
            }

            dpsOffenderService.AddRange( rowsToInsert );
            rockContext.SaveChanges();

            nbResult.Text = string.Format( "Records added: {0}<br />Records already existed: {1}", rowsInserted, rowsAlreadyExist );
            nbResult.NotificationBoxType = NotificationBoxType.Success;
            nbResult.Visible = true;

        }

        /// <summary>
        /// Handles the Click event of the btnMatchAddresses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMatchAddresses_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var locationService = new LocationService( rockContext );
            var qryDpsOffender = new Service<DPSOffender>( rockContext ).Queryable();
            foreach (var dpsOffender in qryDpsOffender.Where(a => !a.DpsLocationId.HasValue))
            {
                var lookupLocation = locationService.Get( dpsOffender.ResAddress, string.Empty, dpsOffender.ResCity, dpsOffender.ResState, dpsOffender.ResZip, string.Empty );
                if (lookupLocation != null)
                {
                    dpsOffender.DpsLocationId = lookupLocation.Id;
                    
                }
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Handles the Click event of the btnMatchPeople control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMatchPeople_Click( object sender, EventArgs e )
        {

        }

        #endregion
    }
}