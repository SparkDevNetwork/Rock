using System;
using System.Linq;
using System.Web.UI;
using Rock.Web.UI;

using System.Collections.Generic;
using com.mychurch.MyFirstRockLibrary.Data;
using com.mychurch.MyFirstRockLibrary.Model;

namespace com.mychurch.Blocks
{
    public partial class PotluckDinnerList : RockBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gPotluckDinner.GridRebind += gPotluckDinner_GridRebind;
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                gPotluckDinner_GridRebind( null, null );
            }
        }

        private void gPotluckDinner_GridRebind( object sender, EventArgs e )
        {
            MyFirstRockLibraryContext myContext = new MyFirstRockLibraryContext();
            gPotluckDinner.DataSource = myContext.PotluckDinners.AsQueryable().ToList();
            gPotluckDinner.DataBind();
        }

        private void GetExpiredDinners()
        {
            MyFirstRockLibraryContext myContext = new MyFirstRockLibraryContext();

            // Get Potluck Dinners that occurred in the past
            List<PotluckDinner> dinners = myContext.PotluckDinners.Where(a => a.StartDateTime < DateTime.Now).ToList();

            // do something with this list
            // todo
        }

        private void SaveChanges()
        {
            // get an instance of our Context
            MyFirstRockLibraryContext myContext = new MyFirstRockLibraryContext();

            // Get the Potluck Dinner that we want to edit
            int currentId = int.Parse(hfPotluckDinnerId.Value);
            PotluckDinner currentDinner = myContext.PotluckDinners.First( a => a.Id.Equals( currentId ) );

            // Update the values
            currentDinner.Name = "Homemade Pizzas";
            currentDinner.StartDateTime = DateTime.Parse( "2/14/2035 7:00 PM" );

            // Mark the currentDinner entry as Modified and Save
            myContext.Entry( currentDinner ).State = System.Data.Entity.EntityState.Modified;
            myContext.SaveChanges();
        }
    }
}