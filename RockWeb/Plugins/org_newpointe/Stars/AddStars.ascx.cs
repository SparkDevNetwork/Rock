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
    [DisplayName("Add Stars")]
    [Category("NewPointe Stars")]
    [Description(
        "Block to add Stars transactions for a person.")]


    public partial class AddStars : Rock.Web.UI.PersonBlock
    {
        private RockContext rockContext = new RockContext();
        private StarsProjectContext starsProjectContext = new StarsProjectContext();
        
        public Person SelectedPerson;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Populate drop down list with Defined Values
                var frequencyTypeGuid = new Guid(org.newpointe.Stars.SystemGuid.DefinedType.STARS_TYPE);
                ddlStars.BindToDefinedType(DefinedTypeCache.Read(frequencyTypeGuid));
            }

        }



        protected void btnSaveStars_OnClick(object sender, EventArgs e)
        {
            // ddl
            var x = Convert.ToInt32(ddlStars.SelectedItem.Value);

            DefinedValueService definedValueService = new DefinedValueService(rockContext);

            var definedValue = definedValueService.Queryable().FirstOrDefault(a => a.Id == x);
            definedValue.LoadAttributes();
            var attributeValue = definedValue.GetAttributeValue("StarValue");
            var starsValue = Convert.ToDecimal(attributeValue);

            var pa = Person.PrimaryAliasId;
            //var pa = ppPerson.PersonAliasId;
            //var value = Decimal.Parse(tbValue.Text);

            
            StarsService starsService = new StarsService(starsProjectContext);

            org.newpointe.Stars.Model.Stars stars = new org.newpointe.Stars.Model.Stars();

            stars.PersonAliasId = pa.GetValueOrDefault();
            stars.CampusId = 1;
            stars.TransactionDateTime = DateTime.Now;
            stars.Value = starsValue;
            stars.Note = ddlStars.SelectedItem.Text + ". Manually added by " + CurrentPerson.FullName;

            starsService.Add(stars);

            starsProjectContext.SaveChanges();


            //Refresh Page to update grids
            Response.Redirect(Request.RawUrl);
        }


    }
}