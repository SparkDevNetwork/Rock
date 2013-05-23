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
                    gPersonList.DataKeyNames = new string[] { "ListId" };
                    CreateGridDataSource();
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
            column.ColumnName = "Room";
            column.ReadOnly = false;
            dt.Columns.Add( column );

            column = new System.Data.DataColumn();
            column.DataType = System.Type.GetType( "System.String" );
            column.ColumnName = "Time";
            column.ReadOnly = false;
            dt.Columns.Add( column );

            var TAListIndex = 0;
            foreach ( var TAList in CheckInTimeAndActivityList )
            {
                var thingCount = 0;
                System.Data.DataRow row;
                row = dt.NewRow();
                foreach ( var thing in TAList )
                {
                    thingCount++;
                    if ( thingCount <= TAList.Count )
                    {
                        switch (thingCount )
                        {
                            case 1:
                                row["ListId"] = TAListIndex;
                                var person = new PersonService().Get( thing );
                                row["Name"] = person.FullName;
                                break;
                            case 2:
                                var activity = new GroupTypeService().Get( thing );
                                row["Room"] = activity.Name;
                                var parentId = GetParent(activity.Id, 0);
                                var parent1 = new GroupTypeService().Get( parentId );
                                row["AssignedTo"] = parent1.Name;
                                break;
                            case 3:
                                var schedule = new ScheduleService().Get( thing );
                                row["Time"] = schedule.Name;
                                break;
                        }
                    }
                }
                dt.Rows.Add( row );
                TAListIndex++;
            }
            gPersonList.DataSource = dt;
            gPersonList.DataBind();

            EditValueField evf = new EditValueField();
            evf.ControlStyle.CssClass = "test";
            gPersonList.CssClass = "";
            gPersonList.AddCssClass( "grid-table" );
            gPersonList.AddCssClass( "table" );
            //gPersonList.AddCssClass( "table-bordered" );
            //gPersonList.AddCssClass( "table-striped" );
            //gPersonList.AddCssClass( "table-hover" );
            //gPersonList.AddCssClass( "table-full" );
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

        /// <summary>
        /// Handles the Edit event of the gPersonList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPerson_Edit( object sender, RowEventArgs e )
        {
            // Put some edit code here.
        }

        /// <summary>
        /// Handles the Delete event of the gPersonList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPerson_Delete( object sender, RowEventArgs e )
        {
            // Put some delete code here
        }

        protected void gPersonList_RowCommand( object sender, GridViewCommandEventArgs e )
        {
            if ( e.CommandName == "Print" )
            {
                // Retrieve the row index stored in the 
                // CommandArgument property.
                int index = Convert.ToInt32( e.CommandArgument );

                // Retrieve the row that contains the button 
                // from the Rows collection.
                GridViewRow row = gPersonList.Rows[index];

                // Add code here to print a label or something
            }
        }

        protected void lbPrintAll_Click( object sender, EventArgs e )
        {
            // Do some crazy printing crap in here where you can print labels for everyone listed in the grid.
        }

        #endregion

        #region Internal Methods

        private void GoBack()
        {
            CurrentCheckInState.CheckIn.SearchType = null;
            CurrentCheckInState.CheckIn.SearchValue = string.Empty;
            CurrentCheckInState.CheckIn.Families = new List<CheckInFamily>();

            SaveState();

        //    if ( CurrentCheckInState.CheckIn.UserEnteredSearch )
        //    {
        //        GoToSearchPage( true );
        //    }
        //    else
        //    {
        //        GoToWelcomePage();
        //    }
            NavigateToPreviousPage();
        }

        private void GoNext()
        {
            SaveState();
            //GoToSearchPage();
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

        protected void gPersonList_Edit( object sender, RowEventArgs e )
        {
        }

        protected void gPersonList_Delete( object sender, RowEventArgs e )
        {
            var something = CheckInTimeAndActivityList[int.Parse( gPersonList.DataKeys[e.RowIndex]["ListId"].ToString() )];
            CheckInTimeAndActivityList.Remove( something );
            CreateGridDataSource();
        }
}
}