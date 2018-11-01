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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using RestSharp;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Security;
using Rock.VersionInfo;
using System.Runtime.Caching;
using System.Web.UI.WebControls;
using System.Data.Entity.Core.Objects;
using Rock.Web.UI;

namespace RockWeb.Plugins.cc_newspring.Groups
{
    /// <summary>
    /// Block that syncs selected people to an exchange server.
    /// </summary>
    [DisplayName( "Attendance Badges" )]
    [Category( "NewSpring > Groups" )]
    [Description( "Displays badges for each child group that was attended." )]

    [TextField( "Block Title", "The title to use for the block header.", true, "Group Attendance", "", 0 )]
    [TextField( "Block Icon CSS Class", "The CSS class name to use for the block title icon.", true, "fa fa-check-circle", "", 1 )]
    [GroupField( "Parent Group", "The parent group of groups to consider attendance for.", true, "", "", 2 )]
    [IntegerField( "Number Of Opportunities", "The total number of groups that can be attended.", true, 0, "", 3 )]
    [DateRangeField( "Date Range", "The date range to consider attendance for.", true, "", "", 4 )]
    [TextField( "Icon Attribute Key", "The group attribute key that contains the image to display when somone attends the group.", true, "", "", 5 )]
    [FileField( Rock.SystemGuid.BinaryFiletype.DEFAULT, "Blank Image", "The image to display when somone has not yet attended a group.", false, "", "", 6 )]
    [IntegerField( "Max Image Width", "The max width of the images to display.", false, 150, "", 7)]
    [IntegerField( "Max Image Height", "The max height of the images to display.", false, 150, "", 8 )]
    [ContextAware( typeof( Person ) )]
    public partial class AttendanceBadges : Rock.Web.UI.RockBlock
    {

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbError.Visible = false;

            if ( !Page.IsPostBack )
            {
                BindData();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindData();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the data.
        /// </summary>
        private void BindData()
        {
            // Set the block title and icon
            string iconCss = GetAttributeValue( "BlockIconCSSClass" );
            lBlockIcon.Text = iconCss.IsNotNullOrWhiteSpace() ? string.Format( "<i class='{0}'></i>", iconCss ) : string.Empty;
            lBlockTitle.Text = GetAttributeValue( "BlockTitle" );

            using ( var rockContext = new RockContext() )
            {
                // Get/Validate the Person
                var person = this.ContextEntity<Person>();
                if ( person == null )
                {
                    int? personId = PageParameter( "PersonId" ).AsIntegerOrNull();
                    if ( personId.HasValue )
                    {
                        person = new PersonService( rockContext ).Get( personId.Value );
                    }
                }
                if ( person == null )
                {
                    DisplayError( "Unknown Person", "Could not determine the person that attendance should be displayed for." );
                    return;
                }

                // Get all the child groups
                Guid? parentGroupGuid = GetAttributeValue( "ParentGroup" ).AsGuidOrNull();
                var groups = new GroupService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( g =>
                        g.ParentGroup != null &&
                        g.ParentGroup.Guid == parentGroupGuid.Value &&
                        g.IsActive )
                    .ToList();

                // Get the groups that person has attended
                var dateRange = DateRange.FromDelimitedValues( GetAttributeValue( "DateRange" ) );
                var groupIds = groups.Select( g => g.Id ).ToList();
                var groupAttendance = new AttendanceService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( a =>
                        a.PersonAlias != null &&
                        a.Occurrence.GroupId.HasValue &&
                        a.PersonAlias.PersonId == person.Id &&
                        groupIds.Contains( a.Occurrence.GroupId.Value ) &&
                        a.DidAttend.HasValue &&
                        a.DidAttend.Value &&
                        a.StartDateTime >= dateRange.Start &&
                        a.StartDateTime <= dateRange.End )
                    .GroupBy( a => a.Occurrence.GroupId.Value )
                    .Select( a => new GroupAttended
                    {
                        GroupId = a.Key,
                        LastAttended = a.Max( b => b.StartDateTime )
                    } )
                    .ToList();
                groupIds = groupAttendance.Select( a => a.GroupId ).ToList();

                // Format the root url of each image
                string imageRoot = string.Format( "{0}://{1}/GetImage.ashx?maxwidth={2}&maxheight={3}&guid=",
                    Request.Url.Scheme, Request.Url.Authority,
                    GetAttributeValue( "MaxImageWidth"), GetAttributeValue("MaxImageHeight") );

                // Get the blank image's guid
                Guid? blankImageGuid = GetAttributeValue( "BlankImage" ).AsGuidOrNull();

                // Get the name and icon guid for each group that person attended
                string iconAttributeKey = GetAttributeValue( "IconAttributeKey" );
                foreach ( var group in groups.Where( g => groupIds.Contains( g.Id ) ) )
                {
                    var groupAttended = groupAttendance.Where( a => a.GroupId == group.Id ).First();
                    groupAttended.GroupName = group.Name;

                    group.LoadAttributes();
                    Guid? groupImageGuid = group.GetAttributeValue( iconAttributeKey ).AsGuidOrNull() ?? blankImageGuid;
                    if ( groupImageGuid.HasValue )
                    {
                        groupAttended.IconUrl = string.Format( "{0}{1}", imageRoot, groupImageGuid );
                    }
                }

                // Order the groups by date attended
                groupAttendance = groupAttendance.OrderBy( g => g.LastAttended ).ToList();

                // Add any blank images
                if ( blankImageGuid != null )
                {
                    int numberOfOpportunities = GetAttributeValue( "NumberOfOpportunities" ).AsInteger();
                    for ( int i = groupAttendance.Count; i < numberOfOpportunities; i++ )
                    {
                        groupAttendance.Add( new GroupAttended
                        {
                            IconUrl = string.Format( "{0}{1}", imageRoot, blankImageGuid )
                    } );
                    }
                }

                // Bind the repeater
                rptIcons.DataSource = groupAttendance;
                rptIcons.DataBind();
            }
        }

        /// <summary>
        /// Displays the error.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        private void DisplayError( string title, string message )
        {
            nbError.Title = title;
            nbError.Text = string.Format( "<p>{0}</p>", message );
            nbError.Visible = true;
        }

        #endregion
    }

    #region Helper Class

    public class GroupAttended
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public DateTime? LastAttended { get; set; }
        public string IconUrl { get; set; }
        public string Title
        {
            get
            {
                return LastAttended.HasValue ? string.Format( "{0}: {1}", LastAttended.Value.ToShortDateString(), GroupName ) : string.Empty;
            }
        }
    }

            #endregion

        }