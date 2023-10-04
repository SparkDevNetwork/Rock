using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

// ::: TODO
// Something funny going on with persistence of flags, also translation of flags to sql
// Are those TextFields needed?
namespace RockWeb.Plugins.org_lakepointe.Reporting
{
    /// <summary>
    /// Block to configure Birthday reports
    /// </summary>
    [DisplayName( "Configurable Birthday Report" )]
    [Category( "LPC > Reporting" )]
    [Description( "Block to generate configurable birthday reports." )]
    //[TextField( "Current Item Template", "Lava template for the current item. The only merge field is {{ CampusName }}.", true, "{{ CampusName }}", order: 1 )]
    //[TextField( "Dropdown Item Template", "Lava template for items in the dropdown. The only merge field is {{ CampusName }}.", true, "{{ CampusName }}", order: 2 )]
    //[TextField( "No Campus Text", "The text displayed when no campus context is selected.", true, "Select Campus", order: 3 )]
    //[TextField( "Clear Selection Text", "The text displayed when a campus can be unselected. This will not display when the text is empty.", false, "", order: 4 )]
    public partial class BirthdayConstraints : RockBlock
    {
        #region Fields

        enum Month { January, February, March, April, May, June, July, August, September, October, November, December };

        ListItem _cbGroupMember = new ListItem( "Member" );
        ListItem _cbGroupLeader = new ListItem( "Leader" );
        ListItem _cbKidsOnly = new ListItem( "Kids" );
        ListItem _cbAdultsOnly = new ListItem( "Adults" );
        ListItem _cbRequireAttendance = new ListItem( "Require Attendance" );

        DataTable _dataTable;

        #endregion

        #region Properties
        #endregion

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var groupOptionList = new List<ListItem>( 4 );
            groupOptionList.Add( _cbGroupMember );
            groupOptionList.Add( _cbGroupLeader );
            groupOptionList.Add( _cbKidsOnly );
            groupOptionList.Add( _cbAdultsOnly );
            groupOptionList.Add( _cbRequireAttendance );
            cblGroupOptions.DataSource = groupOptionList;
            cblGroupOptions.DataBind();
            foreach (ListItem item in cblGroupOptions.Items)
            {
                item.Selected = true;
            }

            bddlMonth.DataSource = Enum.GetNames( typeof( Month ) );
            bddlMonth.DataBind();
            bddlMonth.SelectedIndex = RockDateTime.Now.Month % 12; // Will actually select next month because of 0/1 indexing difference

            grData.GridRebind += GrData_GridRebind;
            grData.Actions.ShowBulkUpdate = false;
            grData.Actions.ShowMergePerson = false;
            grData.Actions.ShowMergeTemplate = true;

            LoadUserPreferences();
        }

        #endregion

        #region Events

        protected void LbGenerate_Click( object sender, EventArgs e )
        {
            LoadGrid();
        }

        #endregion

        #region Methods

        private void LoadGrid()
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add( "Month", bddlMonth.SelectedIndex + 1 );
            parameters.Add( "Member", cblGroupOptions.Items[0].Selected ? 1 : 0 );
            parameters.Add( "Leader", cblGroupOptions.Items[1].Selected ? 1 : 0 );
            parameters.Add( "Kids", cblGroupOptions.Items[2].Selected ? 1 : 0 );
            parameters.Add( "Adults", cblGroupOptions.Items[3].Selected ? 1 : 0 );
            parameters.Add( "RequireAttendance", cblGroupOptions.Items[4].Selected ? 1 : 0 );
            parameters.Add( "Saturday", gpSaturdayGroups.GroupId ?? 0 );
            parameters.Add( "Sunday", gpSundayGroups.GroupId ?? 0 );
            parameters.Add( "CurrentGraduationYear", PersonService.GetCurrentGraduationYear() );

            var listOfGroups = new StringBuilder(); // There isn't an efficient way to transfer list of integers to SQL
            foreach ( var group in gpGroups.SelectedValuesAsInt() )
            {
                listOfGroups.AppendFormat( ", {0}", group );
            }
            listOfGroups.Remove( 0, 2 ); // kill the ', ' at the beginning
            _dataTable = DbService.GetDataTable( _birthdaySearch.Replace( @"select val from @Groups", listOfGroups.ToString() ), System.Data.CommandType.Text, parameters );

            BindGrid();

            SaveUserPreferences( listOfGroups.ToString() );
        }

        private void GrData_GridRebind( object sender, GridRebindEventArgs e )
        {
            LoadGrid();
        }

        new private void SaveUserPreferences(string listOfGroups)
        {
            SetUserPreference( "org.lakePointe.birthdayConstraints.Member", cblGroupOptions.Items[0].Selected ? "1" : "0" );
            SetUserPreference( "org.lakePointe.birthdayConstraints.Leader", cblGroupOptions.Items[1].Selected ? "1" : "0" );
            SetUserPreference( "org.lakePointe.birthdayConstraints.Kids", cblGroupOptions.Items[2].Selected ? "1" : "0" );
            SetUserPreference( "org.lakePointe.birthdayConstraints.Adults", cblGroupOptions.Items[3].Selected ? "1" : "0" );
            SetUserPreference( "org.lakePointe.birthdayConstraints.RequireAttendance", cblGroupOptions.Items[4].Selected ? "1" : "0" );
            SetUserPreference( "org.lakePointe.birthdayConstraints.Saturday", gpSaturdayGroups.GroupId.ToString() );
            SetUserPreference( "org.lakePointe.birthdayConstraints.Sunday", gpSundayGroups.GroupId.ToString() );
            SetUserPreference( "org.lakePointe.birthdayConstraints.Groups", listOfGroups );
        }

        private void LoadUserPreferences()
        {
            string value = GetUserPreference( "org.lakePointe.birthdayConstraints.Member" );
            if ( value != null)
            {
                cblGroupOptions.Items[0].Selected = value.Equals( "1" );
            }
            value = GetUserPreference( "org.lakePointe.birthdayConstraints.Leader" );
            if ( value != null )
            {
                cblGroupOptions.Items[1].Selected = value.Equals( "1" );
            }
            value = GetUserPreference( "org.lakePointe.birthdayConstraints.Kids" );
            if ( value != null )
            {
                cblGroupOptions.Items[2].Selected = value.Equals( "1" );
            }
            value = GetUserPreference( "org.lakePointe.birthdayConstraints.Adults" );
            if ( value != null )
            {
                cblGroupOptions.Items[3].Selected = value.Equals( "1" );
            }
            value = GetUserPreference( "org.lakePointe.birthdayConstraints.RequireAttendance" );
            if ( value != null )
            {
                cblGroupOptions.Items[4].Selected = value.Equals( "1" );
            }
            value = GetUserPreference( "org.lakePointe.birthdayConstraints.Saturday" );
            if ( value.IsNotNullOrWhiteSpace())
            {
                gpSaturdayGroups.GroupId = int.Parse( value );
            }
            value = GetUserPreference( "org.lakePointe.birthdayConstraints.Sunday" );
            if ( value.IsNotNullOrWhiteSpace() )
            {
                gpSundayGroups.GroupId = int.Parse( value );
            }
            value = GetUserPreference( "org.lakePointe.birthdayConstraints.Groups" );
            if ( value.IsNotNullOrWhiteSpace() )
            {
                gpGroups.SetValues( value.Split( ',' ).Select( i => int.Parse( i ) ) );
            }
        }

        private void BindGrid()
        {
            grData.DataSource = _dataTable;
            grData.DataBind();
        }

        #endregion
        #region SQL
        private static string _birthdaySearch = @"
Declare @KidThreshold DateTime = DateAdd(YEAR, -12, GetDate());

-- These recursive queries build lists of all the groups that a child might
-- attend on either Saturday or Sunday. We'll include any of those the child
-- has actually attended in columns of the final report.
with SundayGroups (ParentGroupId, GroupId, GroupName) AS
(
	select g.ParentGroupId, g.Id, g.[Name]
	from [dbo].[Group] g
	where g.Id = @Sunday
	union all
		select e.ParentGroupId, e.Id, e.[Name]
		from [dbo].[Group] e
			inner join SundayGroups f
			on f.GroupId = e.ParentGroupId
)
,
SaturdayGroups (ParentGroupId, GroupId, GroupName) AS
(
	select g.ParentGroupId, g.Id, g.[Name]
	from [dbo].[Group] g
	where g.Id = @Saturday
	union all
		select e.ParentGroupId, e.Id, e.[Name]
		from [dbo].[Group] e
			inner join SaturdayGroups f
			on f.GroupId = e.ParentGroupId
)

select 
	p.NickName + ' ' + p.LastName as Name, p.BirthMonth, p.BirthDay, DateDiff(year, p.BirthDate, getDate()) + 1 as 'Age',
	stuff((
		select ', ' + GroupName 
		from 
			SundayGroups sunG
			join [dbo].[AttendanceOccurrence] ao on ao.GroupId = sunG.GroupId
			join [dbo].[Attendance] a on a.OccurrenceId = ao.Id
			join [dbo].[PersonAlias] pa on pa.Id = a.PersonAliasId
			join [dbo].[Person] p2 on p2.Id = pa.AliasPersonId
		where
			p2.Id = p.Id
			and a.DidAttend = 1
			and a.StartDateTime > DateAdd(Month, -6, getdate())
		group by
			GroupName
		for XML Path('') 
	),1,2,'') as 'Sunday',
	stuff((
		select ', ' + GroupName 
		from 
			SaturdayGroups satG
			join [dbo].[AttendanceOccurrence] ao on ao.GroupId = satG.GroupId
			join [dbo].[Attendance] a on a.OccurrenceId = ao.Id
			join [dbo].[PersonAlias] pa on pa.Id = a.PersonAliasId
			join [dbo].[Person] p2 on p2.Id = pa.AliasPersonId
		where
			p2.Id = p.Id
			and a.DidAttend = 1
			and a.StartDateTime > DateAdd(Month, -6, getdate())
		group by
			GroupName
		for XML Path('') 
	),1,2,'') as 'Saturday',
	12 - (p.GraduationYear - @CurrentGraduationYear) as 'Grade',
	(
		select top 1 convert(varchar(10), a.StartDateTime, 101) -- 101 is US mm/dd/yyyy style
		from [dbo].[Attendance] a
		where
			a.PersonAliasId = pa.Id
			and a.DidAttend = 1
		order by a.StartDateTime desc
	) as 'MostRecentAttendance'
from 
	[dbo].[Group] g
	join [dbo].[GroupMember] gm on gm.GroupId = g.Id
	join [dbo].[GroupTypeRole] gtr on g.GroupTypeId = gtr.GroupTypeId 
	join [dbo].[Person] p on gm.PersonId = p.Id
	join [dbo].[PersonAlias] pa on pa.PersonId = p.Id
	join [dbo].[Attendance] a on a.PersonAliasId = pa.Id
	join [dbo].[AttendanceOccurrence] ao on ao.Id = a.OccurrenceId
where 
	g.id in (select val from @Groups)  -- member of one of these groups
	and gm.GroupMemberStatus = 1  -- active member of the group
	and 
	(
		(@Member = 1 and gtr.Name = 'Member')
		or (@Leader = 1 and gtr.Name = 'Leader')
		or (@Member = 1 and @Leader = 1)  -- select everyone regardless of role
	)
	and
	(
		(@Kids = 1 and p.BirthDate > @KidThreshold) --  < 12 years old
		or (@Adults = 1 and p.AgeClassification = 1) -- proxy for adult
		or (@Kids = 1 and @Adults = 1) -- capture the teenagers too if both are selected
	)
	and p.BirthMonth = (@Month)  -- born in the selected month
	and p.RecordStatusValueId = 3  -- an active person
	and
	(
		(@RequireAttendance = 0) or -- don't require attendance
		(	-- do require attendance
			ao.GroupId in (select val from @Groups)  -- attended one of these groups
			and a.StartDateTime > DateAdd(MONTH, -6, GetDate())  -- in the last six months
			and a.DidAttend = 1
		)
	)
group by
	p.Id, p.LastName, p.NickName, p.BirthMonth, p.BirthDate, p.BirthDay, p.GraduationYear, pa.Id  -- collects attendances so we can count them
having
	Count(p.Id) >= 1  -- attended at least one time over the interval set above
order by
	p.BirthDay, p.NickName, p.LastName
option (MAXRECURSION 5)
";

        #endregion
    }
}