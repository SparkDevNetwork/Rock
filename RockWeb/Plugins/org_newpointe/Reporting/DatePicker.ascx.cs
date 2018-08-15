using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;



namespace RockWeb.Plugins.org_newpointe.Reporting
{

    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName("Date Picker")]
    [Category("NewPointe Reporting")]
    [Description("Adds a Date Picker that places a StartDate and EndDate in the URL querystring.")]
    [EmailField("Email")]


public partial class DatePicker : Rock.Web.UI.RockBlock

{
    private DateTime StartDate;
    private DateTime EndDate;
    

    protected void Page_Load(object sender, EventArgs e)
    {
        //Get the current StartDate and EndDate
        string StartDate = PageParameter("StartDate");
        string EndDate = PageParameter("EndDate");

        //Set form fields to dates in URL
        if (!IsPostBack)
        {
            if (!string.IsNullOrWhiteSpace(StartDate) && !string.IsNullOrWhiteSpace(EndDate))
            {
                drpDateRange.LowerValue = Convert.ToDateTime(StartDate);
                drpDateRange.UpperValue = Convert.ToDateTime(EndDate);
            }
        }

         }

    protected void btnSave_Click(object sender, EventArgs e)
    {
       
        //Get the current URL
        string url = Request.Url.AbsolutePath;

        //Get the dates from the form fields
        StartDate = drpDateRange.LowerValue ?? DateTime.MinValue;
        EndDate = drpDateRange.UpperValue ?? DateTime.MaxValue;

        //Redirect to current URL with new dates in querystring
        Response.Redirect(url + "?StartDate=" + StartDate.ToString("yyyy-MM-dd") + "&EndDate=" + EndDate.ToString("yyyy-MM-dd"));

    }



}
}