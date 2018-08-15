using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Text;
using System.Diagnostics;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.CheckIn;
using System.Data;



namespace RockWeb.Plugins.org_newpointe.PostAttendance
{

    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName("Post Attendance")]
    [Category("NewPointe Attendance")]
    [Description("Add attendance to a person after the event.")]


public partial class PostAttendance : Rock.Web.UI.RockBlock
{

    public RockContext rockContext = new RockContext();
    public PersonPicker person;
    public LocationPicker location;
    public CampusPicker campus;
    public string campusString;
    public DateTime startDateTime;
    public SchedulePicker schedule;
    public GroupPicker group;

    public String selectedCampus;
    public String selectedGroup;
    public String selectedLocation;
    public String selectedStartDateTimeString;
    public String eventName;
    public String campusName;





        protected void Page_Load(object sender, EventArgs e)
    {
        
        if (!Page.IsPostBack)
        {
            //Generate Campus List
            cpCampus.Campuses = CampusCache.All();

            //Set Event List (static for now)
            string[] eventList = { "", "Discover Groups (Combined)", "Discover Group - Intimacy with God", "Discover Group - Involvement in Community", "Discover Group - Influence In Your World", "New to NewPointe", "KidsLife" };

            ddlEvent.DataSource = eventList;
            ddlEvent.DataBind();

        }


        if (Page.IsPostBack) {

        }

 
    }



    protected void btnSaveEvent_Click(object sender, EventArgs e)
    {

        //Set Variables from form
        person = ppPerson;
        //campusString = cpCampus.SelectedCampusId.ToString();
        startDateTime = Convert.ToDateTime(dtpDateTime.SelectedDateTime);
        eventName = ddlEvent.SelectedValue.ToString();


        int? newCampusId = cpCampus.SelectedCampusId;
        if (newCampusId.HasValue)
        {
            var campus = CampusCache.Read(newCampusId.Value);
            if (campus != null)
            {
                campusString = newCampusId.ToString();
                campusName = cpCampus.SelectedItem.ToString();
            }
        }


        selectedCampus = campusString;
        //selectedGroup = group.SelectedValue.ToString();
        //selectedLocation = location.Location.ToStringSafe();
        selectedStartDateTimeString = startDateTime.ToString();

        Session["person"] = person.SelectedValue.ToString();
        //Session["location"] = location;
        Session["campus"] = campusString;
        Session["campusName"] = campusName;
        Session["startDateTime"] = startDateTime;
        Session["eventName"] = eventName;
        //Session["schedule"] = schedule;
        //Session["group"] = group;


        //Set Panel Visability
        pnlEventDetails.Visible = true;
        pnlEvent.Visible = false;
        pnlPeople.Visible = true;


            //Set Variables based on Campus and Event selected


            //Discover Groups
            if (Session["eventName"].ToString() == "Discover Groups (Combined)")
            {
                //schedule = 10;
                Session["schedule"] = 10;

                if (Session["campusName"].ToString() == "Akron Campus")
                {
                    nbWarningMessage.Text = "Discover Groups (Combined) is not available at this Campus.";
                    nbWarningMessage.Visible = true;
                }
                else if (Session["campusName"].ToString() == "Canton Campus")
                {
                    Session["group"] = 60035;
                    Session["location"] = 18;
                }
                else if (Session["campusName"].ToString() == "Coshocton Campus")
                {
                    nbWarningMessage.Text = "Discover Groups (Combined) is not available at this Campus.";
                    nbWarningMessage.Visible = true;
                }
                else if (Session["campusName"].ToString() == "Dover Campus")
                {
                    Session["group"] = 62721;
                    Session["location"] = 2;
                }
                else if (Session["campusName"].ToString() == "Millersburg Campus")
                {
                    Session["group"] = 68287;
                    Session["location"] = 23;
                }
                else if (Session["campusName"].ToString() == "Wooster Campus")
                {
                    nbWarningMessage.Text = "Discover Groups (Combined) is not available at this Campus.";
                    nbWarningMessage.Visible = true;
                }

                Session["person"] = person.SelectedValue.ToString();

            }


            //Discover Group - Intimacy with God
            if (Session["eventName"].ToString() == "Discover Group - Intimacy with God")
            {
                //schedule = 10;
                Session["schedule"] = 10;

                if (Session["campusName"].ToString() == "Akron Campus")
                {
                    nbWarningMessage.Text = "Discover Group - Intimacy with God is not available at this Campus.";
                    nbWarningMessage.Visible = true;
                }
                else if (Session["campusName"].ToString() == "Canton Campus")
                {
                    Session["group"] = 37264;
                    Session["location"] = 18;
                }
                else if (Session["campusName"].ToString() == "Coshocton Campus")
                {
                    Session["group"] = 37282;
                    Session["location"] = 20;
                }
                else if (Session["campusName"].ToString() == "Dover Campus")
                {
                    Session["group"] = 58652;
                    Session["location"] = 2;
                }
                else if (Session["campusName"].ToString() == "Millersburg Campus")
                {
                    Session["group"] = 37295;
                    Session["location"] = 23;
                }
                else if (Session["campusName"].ToString() == "Wooster Campus")
                {
                    Session["group"] = 72674;
                    Session["location"] = 60;
                }

                Session["person"] = person.SelectedValue.ToString();

            }




            //Discover Group - Involvement in Community
            if (Session["eventName"].ToString() == "Discover Group - Involvement in Community")
            {
                //schedule = 10;
                Session["schedule"] = 10;

                if (Session["campusName"].ToString() == "Akron Campus")
                {
                    nbWarningMessage.Text = "Discover Group - Intimacy with God is not available at this Campus.";
                    nbWarningMessage.Visible = true;
                }
                else if (Session["campusName"].ToString() == "Canton Campus")
                {
                    Session["group"] = 37266;
                    Session["location"] = 18;
                }
                else if (Session["campusName"].ToString() == "Coshocton Campus")
                {
                    Session["group"] = 37280;
                    Session["location"] = 20;
                }
                else if (Session["campusName"].ToString() == "Dover Campus")
                {
                    Session["group"] = 58651;
                    Session["location"] = 2;
                }
                else if (Session["campusName"].ToString() == "Millersburg Campus")
                {
                    Session["group"] = 60241;
                    Session["location"] = 23;
                }
                else if (Session["campusName"].ToString() == "Wooster Campus")
                {
                    Session["group"] = 72673;
                    Session["location"] = 60;
                }

                Session["person"] = person.SelectedValue.ToString();

            }




            //Discover Group - Influence In Your World
            if (Session["eventName"].ToString() == "Discover Group - Influence In Your World")
            {
                //schedule = 10;
                Session["schedule"] = 10;

                if (Session["campusName"].ToString() == "Akron Campus")
                {
                    nbWarningMessage.Text = "Discover Group - Influence In Your World is not available at this Campus.";
                    nbWarningMessage.Visible = true;
                }
                else if (Session["campusName"].ToString() == "Canton Campus")
                {
                    Session["group"] = 37277;
                    Session["location"] = 18;
                }
                else if (Session["campusName"].ToString() == "Coshocton Campus")
                {
                    Session["group"] = 37279;
                    Session["location"] = 20;
                }
                else if (Session["campusName"].ToString() == "Dover Campus")
                {
                    Session["group"] = 58653;
                    Session["location"] = 2;
                }
                else if (Session["campusName"].ToString() == "Millersburg Campus")
                {
                    Session["group"] = 48732;
                    Session["location"] = 23;
                }
                else if (Session["campusName"].ToString() == "Wooster Campus")
                {
                    Session["group"] = 72675;
                    Session["location"] = 60;
                }

                Session["person"] = person.SelectedValue.ToString();

            }








            //New to NewPointe
            if (Session["eventName"].ToString() == "New to NewPointe")
            {
                //schedule = 10;
                Session["schedule"] = 10;

                if (Session["campusName"].ToString() == "Akron Campus")
                {
                    Session["group"] = 68292;
                    Session["location"] = 27;
                }
                else if (Session["campusName"].ToString() == "Canton Campus")
                {
                    Session["group"] = 68293;
                    Session["location"] = 18;
                }
                else if (Session["campusName"].ToString() == "Coshocton Campus")
                {
                    Session["group"] = 37294;
                    Session["location"] = 20;
                }
                else if (Session["campusName"].ToString() == "Dover Campus")
                {
                    Session["group"] = 68294;
                    Session["location"] = 2;
                }
                else if (Session["campusName"].ToString() == "Millersburg Campus")
                {
                    Session["group"] = 68295;
                    Session["location"] = 23;
                }
                else if (Session["campusName"].ToString() == "Wooster Campus")
                {
                    Session["group"] = 68296;
                    Session["location"] = 60;
                }
                
                Session["person"] = person.SelectedValue.ToString();

            }

        //KidsLife
            if (Session["eventName"].ToString() == "KidsLife")
            {
                //schedule = 10;
                Session["schedule"] = 10;

                if (Session["campusName"].ToString() == "Akron Campus")
                {
                    nbWarningMessage.Text = "KidsLife is not available at this Campus.";
                    nbWarningMessage.Visible = true;

                }
                else if (Session["campusName"].ToString() == "Canton Campus")
                {
                    Session["group"] = 71045;
                    Session["location"] = 18;
                }
                else if (Session["campusName"].ToString() == "Coshocton Campus")
                {
                    nbWarningMessage.Text = "KidsLife is not available at this Campus.";
                    nbWarningMessage.Visible = true;
                }
                else if (Session["campusName"].ToString() == "Dover Campus")
                {
                    Session["group"] = 72745;
                    Session["location"] = 2;
                }
                else if (Session["campusName"].ToString() == "Millersburg Campus")
                {
                    nbWarningMessage.Text = "KidsLife is not available at this Campus.";
                    nbWarningMessage.Visible = true;
                }
                else if (Session["campusName"].ToString() == "Wooster Campus")
                {
                    nbWarningMessage.Text = "KidsLife is not available at this Campus.";
                    nbWarningMessage.Visible = true;
                }

                Session["person"] = person.SelectedValue.ToString();

            }

        }



    protected void btnSave_Click(object sender, EventArgs e)
    {

        btnDone.Visible = true;
        lblPeople.Visible = true;


        //Create List to Save Registered People

        var peopleList = new List<string>();

        if (Session["peopleList"] != null)
        {
            peopleList = (List<string>)Session["peopleList"];
        }
        

        AttendanceCodeService attendanceCodeService = new AttendanceCodeService(rockContext);
        AttendanceService attendanceService = new AttendanceService(rockContext);
        GroupMemberService groupMemberService = new GroupMemberService(rockContext);
        PersonAliasService personAliasService = new PersonAliasService(rockContext);

            Session["person"] = ppPerson.SelectedValue.ToString();

            // Only create one attendance record per day for each person/schedule/group/location
            DateTime theTime = Convert.ToDateTime(Session["startDateTime"]);
            var attendance = attendanceService.Get(theTime, int.Parse(Session["location"].ToString()), int.Parse(Session["schedule"].ToString()), int.Parse(Session["group"].ToString()), int.Parse(ppPerson.SelectedValue.ToString()));
            var primaryAlias = personAliasService.GetPrimaryAlias(int.Parse(ppPerson.SelectedValue.ToString()));

            if (attendance == null)
            {
            
                if (primaryAlias != null)
                {
                    attendance = rockContext.Attendances.Create();
                    attendance.LocationId = int.Parse(Session["location"].ToString());
                    attendance.CampusId = int.Parse(Session["campus"].ToString());
                    attendance.ScheduleId = int.Parse(Session["schedule"].ToString());
                    attendance.GroupId = int.Parse(Session["group"].ToString());
                    attendance.PersonAlias = primaryAlias;
                    attendance.PersonAliasId = primaryAlias.Id;
                    attendance.DeviceId = null;
                    attendance.SearchTypeValueId = 1;
                    attendanceService.Add(attendance);
                }
        }
        attendance.AttendanceCodeId = null;
        attendance.StartDateTime = Convert.ToDateTime(Session["startDateTime"]);
        attendance.EndDateTime = null;
        attendance.DidAttend = true;

        //KioskLocationAttendance.AddAttendance(attendance);
        rockContext.SaveChanges();

        //Add Person to Dictionary
        peopleList.Add( ppPerson.PersonName );
        repLinks.DataSource = peopleList;
        repLinks.DataBind();
        Session["peopleList"] = peopleList;


            //Clear Person field
            ppPerson.PersonId = null;
            ppPerson.PersonName = null;


        //Update Current Participants List


    }

        protected void btnDone_Click(object sender, EventArgs e)
        {
            Session["peopleList"] = null;
            Response.Redirect(Request.RawUrl);

        }



}
}


