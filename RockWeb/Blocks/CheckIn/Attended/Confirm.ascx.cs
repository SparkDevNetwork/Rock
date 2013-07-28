//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.CheckIn;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn.Attended
{
    [Description( "Attended Check-In Confirmation Block" )]
    public partial class Confirm : CheckInBlock
    {
        #region Control Methods

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
                        
            if ( CurrentWorkflow == null || CurrentCheckInState == null )
            {
                NavigateToHomePage();
            }
            else
            {
                if ( !Page.IsPostBack )
                {
                    //bool bestFitComplete = ProcessBestFit();
                    ProcessBestFit();
                    //if ( bestFitComplete )
                    //{
                    gPersonList.DataKeyNames = new string[] { "ListId" };
                    CreateGridDataSource();
                    //}
                    //else
                    //{
                    //    //NavigateToPage( Activity Select
                    //}                    
                }
            }
        }

        protected void CreateGridDataSource()
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            
            // add the columns to the datatable
            var column = new System.Data.DataColumn();
            column.DataType = System.Type.GetType( "System.String" );
            column.ColumnName = "ListId";
            column.ReadOnly = true;
            dt.Columns.Add( column );

            column = new System.Data.DataColumn();
            column.DataType = System.Type.GetType( "System.String" );
            column.ColumnName = "Name";
            column.ReadOnly = false;
            dt.Columns.Add( column );

            column = new System.Data.DataColumn();
            column.DataType = System.Type.GetType( "System.String" );
            column.ColumnName = "AssignedTo";
            column.ReadOnly = false;
            dt.Columns.Add( column );

            column = new System.Data.DataColumn();
            column.DataType = System.Type.GetType( "System.String" );
            column.ColumnName = "Time";
            column.ReadOnly = false;
            dt.Columns.Add( column );

            // foreach thing selected
            // add to datasource

            System.Data.DataView dv = new System.Data.DataView( dt );
            dv.Sort = "Name ASC, Time ASC";
            System.Data.DataTable dt2 = dv.ToTable();
            gPersonList.DataSource = dt2;
            gPersonList.DataBind();

            gPersonList.CssClass = string.Empty;
            gPersonList.AddCssClass( "grid-table" );
            gPersonList.AddCssClass( "table" );
        }

        #endregion

        #region Edit Events

        protected void lbBack_Click( object sender, EventArgs e )
        {
            GoBack();
        }

        protected void lbDone_Click( object sender, EventArgs e )
        {
            GoNext();
        }

        protected void gPersonList_RowCommand( object sender, GridViewCommandEventArgs e )
        {
            if ( e.CommandName == "Print" )
            {
                // Retrieve the row index stored in the CommandArgument property.
                int index = Convert.ToInt32( e.CommandArgument );

                // Retrieve the row that contains the button from the Rows collection.
                GridViewRow row = gPersonList.Rows[index];

                // Add code here to print a label or something
                maWarning.Show( "If there was any code in here you would have just printed a label", ModalAlertType.Information );
            }
        }

        protected void lbPrintAll_Click( object sender, EventArgs e )
        {
            // Do some crazy printing crap in here where you can print labels for everyone listed in the grid.
            maWarning.Show( "If there was any code in here you would have just printed all the labels", ModalAlertType.Information );
        }

        /// <summary>
        /// Handles the Edit event of the gPersonList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPersonList_Edit( object sender, RowEventArgs e )
        {
            // throw the user back to the activity select page for the person they want to edit.
            
            
            
            
            SaveState();
            var temp = new Dictionary<string,string>();
            NavigateToPage( new Guid( "C87916FE-417E-4A11-8831-5CFA7678A228" ), temp);
        }

        /// <summary>
        /// Handles the Delete event of the gPersonList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPersonList_Delete( object sender, RowEventArgs e )
        {
            // remove person
            CreateGridDataSource();
        }

        #endregion

        #region Internal Methods

        private void ProcessBestFit()
        {

            var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
            if ( family != null )
            {
                foreach ( var person in family.People.Where( f => f.Selected ) )
                {
                    //var personId = CheckInPeopleIds.FirstOrDefault();
                    var chkperson = new PersonService().Get( person.Person.Id );
                    //LoadMinistries( chkperson );
                    //LoadTimes();
                    //LoadActivities();
                    if ( person.LastCheckIn != null && person.GroupTypes.Count > 1 )
                    {
                        var groupType = person.GroupTypes.Where( g => g.LastCheckIn == person.LastCheckIn ).FirstOrDefault();
                        if ( groupType != null )
                        {
                            groupType.Selected = true;
                            var attendService = new AttendanceService();
                            Attendance attend = attendService.Queryable().Where( a => a.StartDateTime == person.LastCheckIn 
                                && a.PersonId == person.Person.Id ).FirstOrDefault();
                            List<int> temp = new List<int>();
                            temp.Add( person.Person.Id );
                            temp.Add( (int)attend.ScheduleId );
                            temp.Add( (int)attend.Group.GroupTypeId );
                            //CheckInTimeAndActivityList.Add( temp );

                            //var location = groupType.Locations.Where( l => l.LastCheckIn == person.LastCheckIn ).FirstOrDefault();
                            //if ( location != null )
                            //{
                            //    location.Selected = true;
                            //    var group = location.Groups.Where( g => g.LastCheckIn == person.LastCheckIn ).FirstOrDefault();
                            //    if ( group != null )
                            //    {
                            //        group.Selected = true;
                            //    }
                            //}
                        }
                    }
                }
            }


        }
        
        private void GoBack()
        {
            var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                        
            List<int> peopleIds = new List<int>();
            foreach ( var person in family.People.Where( p => p.Selected ) )
            {
                peopleIds.Add( person.Person.Id );
            }

            
            SaveState();
            NavigateToPreviousPage();
        }

        private void GoNext()
        {
            CurrentCheckInState.CheckIn.Families.Clear();
            CurrentCheckInState.CheckIn.SearchType = null;
            CurrentCheckInState.CheckIn.SearchValue = string.Empty;
            SaveState();
            NavigateToNextPage();
        }

        protected int GetParent( int childGroupTypeId, int parentId )
        {
            GroupType childGroupType = new GroupTypeService().Get( childGroupTypeId );
            List<int> parentGroupTypes = childGroupType.ParentGroupTypes.Select( a => a.Id ).ToList();
            foreach ( var parentGroupType in parentGroupTypes )
            {
                GroupType theChildGroupType = new GroupTypeService().Get( parentGroupType );
                if ( theChildGroupType.ParentGroupTypes.Count > 0 )
                {
                    parentId = GetParent( theChildGroupType.Id, parentId );
                }
                else
                {
                    parentId = theChildGroupType.Id;
                }
            }

            return parentId;
        }

        #endregion
    }
}