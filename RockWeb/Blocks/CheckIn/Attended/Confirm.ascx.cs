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
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn.Attended
{
    [Description( "Attended Check-In Confirmation Block" )]
    [LinkedPage("Activity Select Page")]
    public partial class Confirm : CheckInBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
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
                    if ( CurrentCheckInState.CheckIn.Families.All( f => f.People.Count == 0 ) )
                    {
                        string errorMsg = "<ul><li>No one in that family is eligible to check-in.</li></ul>";
                        maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
                    }
                    else
                    {
                        //bool bestFitComplete = ProcessBestFit();
                        //ProcessBestFit();
                        //if ( bestFitComplete )
                        //{
                        BindGrid();
                        //}
                        //else
                        //{
                        //    //NavigateToPage( Activity Select
                        //}
                    }
                }
            }
        }

        /// <summary>
        /// Creates the grid data source.
        /// </summary>
        protected void BindGrid()
        {
            var selectedPeopleList = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault().People.Where( p => p.Selected ).OrderBy( p => p.Person.FullNameLastFirst ).ToList();
            var checkInGrid = new System.Data.DataTable();
            checkInGrid.Columns.Add( "Id", typeof(int) );
            checkInGrid.Columns.Add( "Name", typeof(string) );
            checkInGrid.Columns.Add( "AssignedTo", typeof(string) );
            checkInGrid.Columns.Add( "Time", typeof(string) );

            foreach ( var person in selectedPeopleList )
            {
                int personId = person.Person.Id;
                string personName = person.Person.FullName;
                var assignments = person.GroupTypes.Where( gt => gt.Selected )
                    .SelectMany( gt => gt.Groups ).Where( g => g.Selected )
                    .Select( g => new {
                        Id = personId,
                        Name = personName,
                        AssignedTo = g.Group.Name,
                        Time = g.Locations.Where( l => l.Selected )
                          .Where( l => l.Schedules.Any( s => s.Selected ) )
                          .SelectMany( l => l.Schedules.Select( s => s.Schedule.Name ) ).FirstOrDefault()
                    } ).ToList();

                foreach ( var assignment in assignments )
                {
                    checkInGrid.Rows.Add( assignment.Id, assignment.Name, assignment.AssignedTo, assignment.Time );
                }

                if ( !assignments.Any() )
                {
                    checkInGrid.Rows.Add( personId, personName, string.Empty, string.Empty );   
                }                        
            }

            gPersonList.DataSource = checkInGrid;
            gPersonList.DataBind();
            
            gPersonList.CssClass = string.Empty;
            gPersonList.AddCssClass( "grid-table" );
            gPersonList.AddCssClass( "table" );
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the lbBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbBack_Click( object sender, EventArgs e )
        {
            GoBack();
        }

        /// <summary>
        /// Handles the Click event of the lbDone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDone_Click( object sender, EventArgs e )
        {
            GoNext();
        }

        /// <summary>
        /// Handles the RowCommand event of the gPersonList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewCommandEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles the Click event of the lbPrintAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
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
            var personId = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                .People.Where( p => p.Person.Id == e.RowKeyId ).FirstOrDefault().Person.Id.ToString();
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "personId", personId );
            NavigateToLinkedPage( "ActivitySelectPage", queryParams);
        }

        /// <summary>
        /// Handles the Delete event of the gPersonList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPersonList_Delete( object sender, RowEventArgs e )
        {
            // remove person
            CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                .People.Where( p => p.Person.Id == e.RowKeyId ).FirstOrDefault().Selected = false;
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Processes the best fit.
        /// </summary>
        private void ProcessBestFit()
        {
            var errors = new List<string>();
            if ( ProcessActivity( "Assign Best Fit", out errors ) )
            {
                //if ( CurrentCheckInState.CheckIn.Families.All( f => f.People.Count == 0 ) )
                //{
                //    string errorMsg = "<ul><li>No one in that family is eligible to check-in.</li></ul>";
                //    maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
                //}
                //else
                //{
                //    SaveState();                    
                //}
                SaveState();
            }
            else
            {   // not able to assign everyone, please assign manually
                string errorMsg = "<ul><li>" + errors.AsDelimited( "</li><li>" ) + "</li></ul>";
                maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Goes the back.
        /// </summary>
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

        /// <summary>
        /// Goes the next.
        /// </summary>
        private void GoNext()
        {
            CurrentCheckInState.CheckIn.Families.Clear();
            CurrentCheckInState.CheckIn.SearchType = null;
            CurrentCheckInState.CheckIn.SearchValue = string.Empty;
            SaveState();
            NavigateToNextPage();
        }

        #endregion
    }
}