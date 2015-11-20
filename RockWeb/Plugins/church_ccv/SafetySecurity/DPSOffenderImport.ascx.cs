using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web.UI.WebControls;

using church.ccv.SafetySecurity.Model;
using Microsoft.AspNet.SignalR;
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

    [LinkedPage( "Detail Page" )]
    public partial class DPSOffenderImport : RockBlock
    {
        /// <summary>
        /// This holds the reference to the RockMessageHub SignalR Hub context.
        /// </summary>
        private IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<RockMessageHub>();

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gDpsOffender.GridRebind += gDpsOffender_GridRebind;

            RockPage.AddScriptLink( "~/Scripts/jquery.signalR-2.1.2.min.js", fingerprint: false );
        }

        /// <summary>
        /// Handles the GridRebind event of the gDpsOffender control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gDpsOffender_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !this.IsPostBack )
            {
                BindGrid();
            }
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
            var dpsOffenderService = new DPSOffenderService( rockContext );
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
                    a.ResZip == dpsOffender.ResZip ).Any();

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
            var qryDpsOffender = new DPSOffenderService( rockContext ).Queryable();
            var dpsOffenderList = qryDpsOffender.Where( a => !a.DpsLocationId.HasValue ).ToList();
            var country = GlobalAttributesCache.Read().OrganizationCountry;
            int progress = 0;
            int count = dpsOffenderList.Count();
            if ( count > 0 )
            {

                System.Threading.Tasks.Task.Run( () =>
                    {
                        foreach ( var dpsOffender in dpsOffenderList )
                        {

                            var lookupLocation = locationService.Get( dpsOffender.ResAddress, string.Empty, dpsOffender.ResCity, dpsOffender.ResState, dpsOffender.ResZip, country );
                            if ( lookupLocation != null )
                            {
                                dpsOffender.DpsLocationId = lookupLocation.Id;
                            }

                            progress++;

                            var percent = (double)( progress * 100 ) / count;
                            _hubContext.Clients.All.receiveNotification( string.Format( "matching addresses: {0}%", (int)percent ) );
                        }

                        rockContext.SaveChanges();

                        _hubContext.Clients.All.hideProgressBar();
                    } );
            }
            else
            {
                nbResult.Text = "All DPS Address records already matched.";
                nbResult.NotificationBoxType = NotificationBoxType.Success;
                nbResult.Visible = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnMatchPeople control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMatchPeople_Click( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();

            var groupTypeFamilyId = GroupTypeCache.GetFamilyGroupType().Id;
            var qryPerson = new PersonService( rockContext ).Queryable( true, false );
            var qryGroupLocations = new GroupLocationService( rockContext ).Queryable();
            var groupLocationTypeValueHomeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() ).Id;
            var familyGroupType = GroupTypeCache.GetFamilyGroupType();
            var groupTypeIdFamily = familyGroupType.Id;
            var groupRoleIdAdult = familyGroupType.Roles.Where( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).First().Id;

            var qryGroupMembers = new GroupMemberService( rockContext ).Queryable();

            var qryHomeAddress = new GroupLocationService( rockContext ).Queryable()
                .Where( a => a.GroupLocationTypeValueId.HasValue && a.GroupLocationTypeValueId.Value == groupLocationTypeValueHomeId && a.Group.GroupTypeId == groupTypeIdFamily );

            bool matchZip = cbMatchZip.Checked;
            bool matchAge = cbAge.Checked;

            DateTime today = RockDateTime.Today;

            var qryDpsOffender = new DPSOffenderService( rockContext ).Queryable()
                .Select( a => new
            {
                a.Id,
                a.PersonAlias.Person,
                a.FirstName,
                a.LastName,
                a.Age,
                DpsLocation = a.DpsLocation,
                FamiliesAtAddress = qryGroupLocations.Where( gl => gl.LocationId == a.DpsLocationId ).Count(),
                PotentialMatches = qryPerson
                    .Where( p => ( !p.BirthDate.HasValue || ( SqlFunctions.DateDiff( "year", p.BirthDate.Value, today ) > 17 ) ) )
                    .Where( p => p.FirstName.StartsWith( a.FirstName.Substring( 0, 1 ) ) || p.NickName.StartsWith( a.FirstName.Substring( 0, 1 ) ) )
                    .Where( p => a.LastName == p.LastName )
                    .Where( p => ( p.Gender == Gender.Unknown ) || a.Gender == ( p.Gender == Gender.Male ? "M" : "F" ) )
                    .Where( p => !matchZip || a.ResZip != "0" && qryHomeAddress.Any( x => x.Group.Members.Any( gm => gm.PersonId == p.Id ) && x.Location.PostalCode.StartsWith( a.ResZip ) ) )
                    .Where( p => !matchAge || !p.BirthDate.HasValue || ( p.BirthDate.HasValue && a.Age.HasValue &&
                        ( ( SqlFunctions.DateDiff( "year", p.BirthDate.Value, today ) ) > ( a.Age.Value - 3 ) && ( SqlFunctions.DateDiff( "year", p.BirthDate.Value, today ) ) < ( a.Age.Value + 3 ) )
                        ) )
                      ,
                a.DateConvicted,
                a.ConvictionState,
                a.Absconder
            } );

            if ( gDpsOffender.SortProperty != null )
            {
                if ( gDpsOffender.SortProperty.Property == "PotentialMatches" )
                {
                    qryDpsOffender = qryDpsOffender.OrderByDescending( a => a.PotentialMatches.Count() ).ThenBy( a => a.LastName ).ThenBy( a => a.FirstName );
                }
                else
                {
                    qryDpsOffender = qryDpsOffender.Sort( gDpsOffender.SortProperty );
                }
            }
            else
            {
                qryDpsOffender = qryDpsOffender.OrderBy( a => a.LastName ).ThenBy( a => a.FirstName );
            }

            if ( cbLimitToLocationMatches.Checked )
            {
                qryDpsOffender = qryDpsOffender.Where( a => a.FamiliesAtAddress > 0 );
            }

            if ( cbLimitToPotentialMatches.Checked )
            {
                qryDpsOffender = qryDpsOffender.Where( a => a.PotentialMatches.Any() );
            }

            using ( new QueryHintScope( rockContext, QueryHintType.RECOMPILE ) )
            {
                gDpsOffender.SetLinqDataSource( qryDpsOffender );
            }

            gDpsOffender.DataBind();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gDpsOffender control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gDpsOffender_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            string matchIcon = "<i class='fa fa-check-square-o'></i> ";
            if ( e.Row.DataItem != null )
            {
                Literal lPersonMatches = e.Row.FindControl( "lPersonMatches" ) as Literal;
                var matchedPerson = e.Row.DataItem.GetPropertyValue( "Person" ) as Person;
                if ( matchedPerson != null )
                {

                    var url = this.ResolveUrl( string.Format( "~/Person/{0}", matchedPerson.Id ) );
                    lPersonMatches.Text = string.Format( "Confirmed match to <a href='{0}'>{1}</a>", url, matchedPerson );
                    return;
                }

                var potentialMatches = e.Row.DataItem.GetPropertyValue( "PotentialMatches" ) as IEnumerable<Person>;
                var firstName = e.Row.DataItem.GetPropertyValue( "FirstName" ) as string;
                var lastName = e.Row.DataItem.GetPropertyValue( "LastName" ) as string;
                var age = e.Row.DataItem.GetPropertyValue( "Age" ) as int?;
                var dpsLocationId = e.Row.DataItem.GetPropertyValue( "DpsLocationId" ) as int?;
                if ( potentialMatches != null )
                {
                    string resultHtml = string.Empty;
                    var personList = potentialMatches.ToList();

                    var fullNameMatches = personList.Where( a => ( ( a.FirstName.Equals( firstName, StringComparison.OrdinalIgnoreCase ) ) || ( a.NickName.Equals( firstName, StringComparison.OrdinalIgnoreCase ) ) ) ).ToList();
                    var fullNameAndAddressMatches = new List<Person>();
                    foreach ( var person in fullNameMatches )
                    {
                        var address = person.GetHomeLocation( new RockContext() );
                        if ( address != null && address.Id == dpsLocationId )
                        {
                            fullNameAndAddressMatches.Add( person );
                            resultHtml += string.Format(
                                "<li><h4{0}{1} - {2}<h4> <h5>{0}{3}</h5> {4}</li>",
                                matchIcon, // {0} 
                                GetPersonName( person ), // {1}
                                person.Age == age ? matchIcon + age.ToString() : person.Age.ToString(), // {2}
                                address, // {3}
                                Person.GetPhotoImageTag( person, 100, 100 ) ); // {4}
                        }
                    }

                    foreach ( var person in fullNameMatches.Where( a => !fullNameAndAddressMatches.Any( x => x.Id == a.Id ) ) )
                    {
                        var address = person.GetHomeLocation( new RockContext() );
                        resultHtml += string.Format(
                            "<li><h4>{0}{1} - {2}<h4> <h5>{3}</h5> {4}</li>",
                            matchIcon, // {0} 
                            GetPersonName( person ), // {1}
                            person.Age == age ? matchIcon + age.ToString() : person.Age.ToString(), // {2}
                            address, // {3}
                            Person.GetPhotoImageTag( person, 100, 100 ) ); // {4}
                    }

                    int otherCount = personList.Where( a => !fullNameMatches.Any( x => x.Id == a.Id ) ).Count();
                    if ( otherCount > 0 )
                    {
                        resultHtml += string.Format( "<li>{0}</li> other potential matches", otherCount );
                    }

                    lPersonMatches.Text = "<ul>" + resultHtml + "</ul>";
                }
            }
        }

        /// <summary>
        /// Gets the name of the person including both NickName and FirstName if they are different
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        private string GetPersonName( Person person )
        {
            if ( person.NickName != person.FirstName )
            {
                var firstAndNick = string.Format( "{0} \"{1}\"", person.FirstName, person.NickName );
                return Person.FormatFullName( firstAndNick, person.LastName, person.SuffixValueId );
            }
            else
            {
                return person.FullName;
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gDpsOffender control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gDpsOffender_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "DPSOffenderId", e.RowKeyId );
        }

        #endregion
    }
}