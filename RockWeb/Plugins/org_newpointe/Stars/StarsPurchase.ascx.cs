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
    [DisplayName("Stars Purchase")]
    [Category("NewPointe Stars")]
    [Description(
        "Block to add Stars Purchases.")]


    public partial class StarsPurchase : Rock.Web.UI.RockBlock
    {
        private RockContext rockContext = new RockContext();
        private StarsProjectContext starsProjectContext = new StarsProjectContext();
        
        public Person SelectedPerson;

        protected void Page_Load(object sender, EventArgs e)
        {
            PersonAliasService personAliasService = new PersonAliasService(rockContext);

            StarsService starsService = new StarsService(starsProjectContext);

            var starsList = starsService.Queryable().GroupBy(a => a.PersonAlias.Person).Select(g => new { Person = g.Key, Sum = g.Sum(a => a.Value)}).ToList();



            gStars.DataSource = starsList;
            gStars.DataBind();
            
        }



        protected void btnSaveStars_OnClick(object sender, EventArgs e)
        {
            var pa = ppPerson.PersonAliasId;
            var value = Decimal.Parse(tbValue.Text);

            StarsService starsService = new StarsService(starsProjectContext);

            org.newpointe.Stars.Model.Stars stars = new org.newpointe.Stars.Model.Stars();

            stars.PersonAliasId = pa.GetValueOrDefault();
            stars.CampusId = 1;
            stars.TransactionDateTime = DateTime.Now;
            stars.Value = value;

            starsService.Add(stars);

            starsProjectContext.SaveChanges();
        }


        protected void gStars_OnRowSelected(object sender, RowEventArgs e)
        {
            Response.Redirect("~/Person/" + e.RowKeyValue);
        }
    }
}