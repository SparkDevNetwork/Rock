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

namespace RockWeb.Blocks.CheckIn.Attended
{
    [Description( "Attended Check-In Activity Select Block" )]
    public partial class ActivitySelect : CheckInBlock
    {
        #region Control Methods

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            //if ( CurrentWorkflow == null || CurrentCheckInState == null )
            //{
            //    GoToWelcomePage();
            //}
            //else
            //{
            //    // check for activities that match the search criteria                
            //    if ( !Page.IsPostBack )
            //    {
            //        if ( CurrentCheckInState.CheckIn.Families.Count == 1 &&
            //            !CurrentCheckInState.CheckIn.ConfirmSingleFamily )
            //        {
            //            if ( UserBackedUp )
            //            {
            //                GoBack();
            //            }
            //            else
            //            {
            //                foreach ( var family in CurrentCheckInState.CheckIn.Families )
            //                {
            //                    family.Selected = true;
            //                    // set button selected 
            //                }

            //                ProcessFamily();
            //            }
            //        }
            //        else
            //        {
            //            rFamily.DataSource = CurrentCheckInState.CheckIn.Families;
            //            rFamily.DataBind();
            //            // if 1 family, set button selected
            //        }
            //    }
            //}
        }

        #endregion

        #region Edit Events

        protected void rMinistry_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
        }

        protected void rTime_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
        }

        protected void rActivity_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
        }

        //protected void rFamily_ItemCommand( object source, RepeaterCommandEventArgs e )
        //{
        //    if ( KioskCurrentlyActive )
        //    {
        //        int id = Int32.Parse( e.CommandArgument.ToString() );
        //        var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Group.Id == id ).FirstOrDefault();
        //        if ( family != null )
        //        {
        //            family.Selected = true;
        //            ( (HtmlControl)e.Item.FindControl( "lbSelectFamily" ) ).Attributes.Add( "class", "active" );
        //            ProcessPerson();
        //            // set button selected 
        //        }
        //    }
        //}

        //protected void rPerson_ItemCommand( object source, RepeaterCommandEventArgs e )
        //{
        //    if ( KioskCurrentlyActive )
        //    {
        //        var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
        //        if ( family != null )
        //        {
        //            int id = Int32.Parse( e.CommandArgument.ToString() );
        //            var familyMember = family.People.Where( m => m.Person.Id == id ).FirstOrDefault();
        //            if ( familyMember != null )
        //            {
        //                familyMember.Selected = true;
        //                // set button selected 
                        
        //            }
        //        }
        //    }
        //}

        protected void lbBack_Click( object sender, EventArgs e )
        {
            GoBack();
        }
                
        protected void lbNext_Click( object sender, EventArgs e )
        {
            GoNext();   
        }

        #endregion

        #region Internal Methods 

        private void GoBack()
        {
            CurrentCheckInState.CheckIn.SearchType = null;
            CurrentCheckInState.CheckIn.SearchValue = string.Empty;
            CurrentCheckInState.CheckIn.Families = new List<CheckInFamily>();

            SaveState();

            if ( CurrentCheckInState.CheckIn.UserEnteredSearch )
            {
                GoToSearchPage( true );
            }
            else
            {
                GoToWelcomePage();
            }
        }

        //private void ProcessFamily()
        //{
        //    var errors = new List<string>();
        //    if ( ProcessActivity( "Person Search", out errors ) )
        //    {   
        //        ProcessPerson();
        //    }
        //    else
        //    {
        //        string errorMsg = "<ul><li>" + errors.AsDelimited( "</li><li>" ) + "</li></ul>";
        //        maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
        //    }
        //}

        //private void ProcessPerson()
        //{
        //    var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
        //    if ( family != null )
        //    {
        //        if ( family.People.Count == 1 )
        //        {
        //            if ( UserBackedUp )
        //            {
        //                GoBack();
        //            }
        //            else
        //            {
        //                foreach ( var familyMember in family.People )
        //                {
        //                    familyMember.Selected = true;
        //                    // set button selected 
        //                }
        //            }
        //        }

        //        rPerson.DataSource = family.People;
        //        rPerson.DataBind();               
        //    }
        //}

        private void GoNext()
        {
            SaveState();
            GoToSuccessPage();
        }

        #endregion
    }
}