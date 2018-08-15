using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Entity;
using System.Data;
using System.Diagnostics;
using System.Text;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Workflow;


using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.HtmlControls;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Microsoft.Ajax.Utilities;
using org.newpointe.Stars.Data;
using org.newpointe.Stars.Model;
using Quartz.Util;
using Rock.Security;
using Rock.Web.UI;
using WebGrease.Css.Extensions;
using HttpResponse = RestSharp.HttpResponse;

namespace RockWeb.Plugins.org_newpointe.Stars
{
    /// <summary>
    /// Template block for a TreeView.
    /// </summary>
    [DisplayName("Stars Person Total")]
    [Category("NewPointe Stars")]
    [Description(
        "Shows total number of stars for a person.")]


    public partial class TotalStarsPerson : PersonBlock
    {
        private RockContext rockContext = new RockContext();
        private StarsProjectContext starsProjectContext = new StarsProjectContext();
        
        public Person SelectedPerson;

        protected void Page_Load(object sender, EventArgs e)
        {


            SelectedPerson = Person;
            StarsService starsService = new StarsService(starsProjectContext);

           var starsList = starsService.Queryable().GroupBy(a => a.PersonAlias.Id)
                .Select(g => new
                   {
                       Person = g.Key,
                       Sum = g.Sum(a => a.Value)
                   }
                ).Where(ab => ab.Person == SelectedPerson.PrimaryAliasId.Value).ToList();


            var starsListThisMonth = starsService.Queryable()
                .Where(a => a.TransactionDateTime.Month == DateTime.Now.Month && a.TransactionDateTime.Year == DateTime.Now.Year)
                .GroupBy(a => a.PersonAlias.Id)
                .Select(g => new
                {
                    Person = g.Key,
                    Sum = g.Sum(a => a.Value)
                }
                ).Where(ab => ab.Person == SelectedPerson.PrimaryAliasId.Value).ToList();


            var starsListLastMonth = starsService.Queryable()
                .Where(a => a.TransactionDateTime.Month == DateTime.Now.Month - 1 && a.TransactionDateTime.Year == DateTime.Now.Year)
                .GroupBy(a => a.PersonAlias.Id)
                .Select(g => new
                {
                    Person = g.Key,
                    Sum = g.Sum(a => a.Value)
                }
                ).Where(ab => ab.Person == SelectedPerson.PrimaryAliasId.Value).ToList();






            if (starsList.Any())
            {
                hlStarsTotal.Text = starsList.FirstOrDefault().Sum.ToString() + " Stars All Time";
            }
            else
            {
                hlStarsTotal.Text = "0 Stars All Time";
                hlStarsTotal.LabelType = LabelType.Default;
            }



            if (starsListThisMonth.Any())
            {
                hlStarsThisMonth.Text = starsListThisMonth.FirstOrDefault().Sum.ToString() + " Stars This Month";
            }
            else
            {
                hlStarsThisMonth.Text = "0 Stars This Month";
                hlStarsThisMonth.LabelType = LabelType.Default;
            }





            if (starsListLastMonth.Any())
            {
                hlStarsLastMonth.Text = starsListLastMonth.FirstOrDefault().Sum.ToString() + " Stars Last Month";
            }
            else
            {
                hlStarsLastMonth.Text = "0 Stars Last Month";
                hlStarsLastMonth.LabelType = LabelType.Default;
            }


        }


        protected void gStars_OnRowSelected(object sender, RowEventArgs e)
        {
            Response.Redirect("~/Person/" + e.RowKeyValue);
        }
    }
}