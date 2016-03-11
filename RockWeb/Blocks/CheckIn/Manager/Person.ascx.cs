﻿// <copyright>
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
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace RockWeb.Blocks.CheckIn.Manager
{
    /// <summary>
    /// Block used to display person and details about recent check-ins
    /// </summary>
    [DisplayName( "Person Profile" )]
    [Category( "Check-in > Manager" )]
    [Description( "Displays person and details about recent check-ins." )]

    [LinkedPage("Manager Page", "Page used to manage check-in locations")]
    public partial class Person : Rock.Web.UI.RockBlock
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

            gHistory.RowDataBound += gHistory_RowDataBound;
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
                if ( IsUserAuthorized( Authorization.VIEW ) )
                {
                    Guid? personGuid = PageParameter( "Person" ).AsGuidOrNull();
                    if ( personGuid.HasValue )
                    {
                        ShowDetail( personGuid.Value );
                    }
                }
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
        /// Handles the RowDataBound event of the gHistory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        void gHistory_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var attendanceInfo = e.Row.DataItem as AttendanceInfo;
                if ( attendanceInfo != null && attendanceInfo.IsActive )
                {
                    e.Row.AddCssClass( "success" );
                    Literal lActive = (Literal)e.Row.FindControl( "lActive" );
                    lActive.Text = "<span class='label label-success'>Current</span>";
                }
            }
        }

        protected void rptrFamily_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                dynamic familyMember = e.Item.DataItem as dynamic;
                Literal lFamilyIcon = (Literal)e.Item.FindControl( "lFamilyIcon" );

                if ( familyMember.FamilyRole.ToString() == "Child" )
                {
                    lFamilyIcon.Text = "<i class='fa fa-child'></i>";
                }
                else if ( familyMember.Gender == Gender.Female )
                {
                    lFamilyIcon.Text = "<i class='fa fa-female'></i>";
                }
                else
                {
                    lFamilyIcon.Text = "<i class='fa fa-male'></i>";
                }
            }
        }

        #endregion

        #region Methods

        private void ShowDetail(Guid personGuid)
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );

                var person = personService.Queryable( "PhoneNumbers.NumberTypeValue,RecordTypeValue", true, true )
                    .FirstOrDefault( a => a.Guid == personGuid );

                if ( person != null )
                {
                    lName.Text = person.FullName;

                    string photoTag = Rock.Model.Person.GetPersonPhotoImageTag( person, 120, 120 );
                    if ( person.PhotoId.HasValue )
                    {
                        lPhoto.Text = string.Format( "<div class='photoframe'><a href='{0}'>{1}</a></div>", person.PhotoUrl, photoTag );
                    }
                    else
                    {
                        lPhoto.Text = photoTag;
                    }


                    lGender.Text = person.Gender != Gender.Unknown ? person.Gender.ConvertToString() : "";
                    
                    if ( person.BirthDate.HasValue )
                    {
                        string ageText = ( person.BirthYear.HasValue && person.BirthYear != DateTime.MinValue.Year ) ?
                            string.Format( "{0} yrs old ", person.BirthDate.Value.Age() ) : string.Empty;
                        lAge.Text = string.Format( "{0} <small>({1})</small><br/>", ageText, person.BirthDate.Value.ToShortDateString() );
                    }
                    else
                    {
                        lAge.Text = string.Empty;
                    }

                    lGrade.Text = person.GradeFormatted;
                    
                    lEmail.Visible = !string.IsNullOrWhiteSpace( person.Email );
                    lEmail.Text = person.GetEmailTag( ResolveRockUrl( "/" ), "btn btn-default", "<i class='fa fa-envelope'></i>" );

                    // Get all family member from all families ( including self )
                    var allFamilyMembers = personService.GetFamilyMembers( person.Id, true ).ToList();

                    // Add flag for this person in each family indicating if they are a child in family.
                    var childGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();
                    var isFamilyChild = new Dictionary<int, bool>();
                    foreach ( var thisPerson in allFamilyMembers.Where( m => m.PersonId == person.Id ) )
                    {
                        isFamilyChild.Add( thisPerson.GroupId, thisPerson.GroupRole.Guid.Equals( childGuid ) );
                    }

                    // Get the current url's root (without the person's guid)
                    string urlRoot = Request.Url.ToString().ReplaceCaseInsensitive( personGuid.ToString(), "" );

                    // Get the other family members and the info needed for rendering
                    var familyMembers = allFamilyMembers.Where( m => m.PersonId != person.Id )
                        .OrderBy( m => m.GroupId )
                        .ThenBy( m => m.Person.BirthDate )
                        .Select( m => new
                        {
                            Url = urlRoot + m.Person.Guid.ToString(),
                            FullName = m.Person.FullName,
                            Gender = m.Person.Gender,
                            FamilyRole = m.GroupRole,
                            Note = isFamilyChild[m.GroupId] ?
                                ( m.GroupRole.Guid.Equals( childGuid ) ? " (Sibling)" : "(Parent)" ) :
                                ( m.GroupRole.Guid.Equals( childGuid ) ? " (Child)" : "" )
                        } )
                        .ToList();

                    rcwFamily.Visible = familyMembers.Any();
                    rptrFamily.DataSource = familyMembers;
                    rptrFamily.DataBind();

                    rptrPhones.DataSource = person.PhoneNumbers.Where( p => !p.IsUnlisted ).ToList();
                    rptrPhones.DataBind();

                    var schedules = new ScheduleService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( s => s.CheckInStartOffsetMinutes.HasValue )
                        .ToList();
                    
                    var scheduleIds = schedules.Select( s => s.Id ).ToList();

                    var activeScheduleIds = new List<int>();
                    foreach ( var schedule in schedules )
                    {
                        if ( schedule.IsScheduleOrCheckInActive )
                        {
                            activeScheduleIds.Add( schedule.Id );
                        }
                    }

                    int? personAliasId = person.PrimaryAliasId;
                    if ( !personAliasId.HasValue )
                    {
                        personAliasId = new PersonAliasService( rockContext ).GetPrimaryAliasId( person.Id );
                    }

                    var attendances = new AttendanceService( rockContext )
                        .Queryable( "Schedule,Group,Location,AttendanceCode" )
                        .Where( a =>
                            a.PersonAliasId.HasValue &&
                            a.PersonAliasId == personAliasId &&
                            a.ScheduleId.HasValue &&
                            a.GroupId.HasValue &&
                            a.LocationId.HasValue &&
                            a.DidAttend.HasValue &&
                            a.DidAttend.Value &&
                            scheduleIds.Contains( a.ScheduleId.Value ) )
                        .OrderByDescending( a => a.StartDateTime )
                        .Take( 20 )
                        .ToList()                                                   // Run query to get recent most 20 checkins
                        .OrderByDescending( a => a.StartDateTime )                  // Then sort again by startdatetime and schedule start (which is not avail to sql query )
                        .ThenByDescending( a => a.Schedule.StartTimeOfDay )
                        .Select( a => new AttendanceInfo
                        {
                            Date = a.StartDateTime,
                            GroupId = a.Group.Id,
                            Group = a.Group.Name,
                            LocationId = a.LocationId.Value,
                            Location = a.Location.Name,
                            Schedule = a.Schedule.Name,
                            IsActive =
                                a.StartDateTime > DateTime.Today &&
                                activeScheduleIds.Contains( a.ScheduleId.Value ),
                            Code = a.AttendanceCode != null ? a.AttendanceCode.Code : ""
                        } ).ToList();

                    // Set active locations to be a link to the room in manager page
                    var qryParam = new Dictionary<string, string>();
                    qryParam.Add( "Group", "" );
                    qryParam.Add( "Location", "" );
                    foreach ( var attendance in attendances.Where( a => a.IsActive ) )
                    {
                        qryParam["Group"] = attendance.GroupId.ToString();
                        qryParam["Location"] = attendance.LocationId.ToString();
                        attendance.Location = string.Format( "<a href='{0}'>{1}</a>",
                            LinkedPageUrl( "ManagerPage", qryParam ), attendance.Location );
                    }

                    rcwCheckinHistory.Visible = attendances.Any();
                    gHistory.DataSource = attendances;
                    gHistory.DataBind();
                }
            }
        }

        #endregion

        #region Helper Classes

        public class AttendanceInfo
        {
            public DateTime Date { get; set; }
            public int GroupId { get; set; }
            public string Group { get; set; }
            public int LocationId { get; set; }
            public string Location { get; set; }
            public string Schedule { get; set; }
            public bool IsActive { get; set; }
            public string Code { get; set; }
        }

        #endregion
        
}
}