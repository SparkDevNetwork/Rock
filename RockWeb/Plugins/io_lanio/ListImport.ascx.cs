using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OfficeOpenXml;
using System.IO;
using System.Data;
using System.ComponentModel;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock;

using Rock.Attribute;
using Rock.Web.UI.Controls;
using System.Data.Entity;



using CsvHelper;

namespace RockWeb.Plugins.io_Lanio
{



    [DisplayName("CSV List Import")]
    [Category("Lanio")]
    [Description("Plugin that allows you to import lists of people into your Rock RMS site from CSV or Excel documents.")]
    
    public partial class ListImport : Rock.Web.UI.RockBlock
    {

        public List<string> colHeadings = new List<string>();
        public List<string> schema = new List<string> { "FirstName", "LastName", "Email", "Phone", "DOB", "Gender", "ForeignKey", "Id", "Graduation" };
        public List<string> groups = new List<string>();
        

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                displayPnl();
            }

            using (RockContext rockContext = new RockContext())
            {

                var set = new GroupService(rockContext).Queryable()
                                        .Where(g => g.IsPublic && g.IsSystem == false && g.GroupTypeId != 10 && g.GroupTypeId != 11 && g.GroupTypeId != 12)
                                        .GroupBy(g => new { g.Id, g.Name })
                                        .Select(g => g.FirstOrDefault())
                                        .ToList();

                
                groups = set.Select(g => g.Name).ToList();
                if (!IsPostBack)
                {
                    var groupTypes = new GroupTypeService(rockContext).Queryable().Where(a => a.Roles.Any()).OrderBy(a => a.Name).ToList();
                    gtpType.GroupTypes = groupTypes;

                }


            }
        }

        public void displayPnl(string pnlName = "Upload")
        {
            
            upnlUpload.Visible = false;
            upnlSync.Visible = false;
            upnlGroup.Visible = false;
            upnlSuccess.Visible = false;
            upnlFailure.Visible = false;

            if (pnlName == "Upload")
                upnlUpload.Visible = true;
            else if (pnlName == "Sync")
                upnlSync.Visible = true;
            else if (pnlName == "Group")
                upnlGroup.Visible = true;
            else if (pnlName == "Success")
                upnlSuccess.Visible = true;
            else if (pnlName == "Failure")
                upnlFailure.Visible = true;
        }

        protected void btnUpload_OnClick(object sender, EventArgs e)
        {
            bool needSync = false;
            DataTable dt = null;

            if (fuFile.HasFile)
            {
                MemoryStream ms = new MemoryStream(fuFile.FileBytes);

                if (fuFile.FileName.EndsWith("xlsx"))
                {
                    using (ExcelPackage pack = new ExcelPackage(ms))
                    {
                        dt = pack.ToDataTable();
                        Session["dt"] = dt;
                        foreach (DataColumn c in dt.Columns)
                        {
                            colHeadings.Add(c.ColumnName);
                            if (!schema.Contains(c.ColumnName))  //If the file contains a field that is not standared
                            {
                                needSync = true;
                                string guid = Guid.NewGuid().ToString();
                                string path = System.IO.Path.GetTempPath();

                                fuFile.SaveAs(path + guid + ".xlsx");
                                hdnFile.Value = guid + ".xlsx";

                                displayPnl("Sync");
                            }

                        }

                        if (!needSync)
                            displayPnl("Group");
                        //import(dt);

                    }
                }

                if (fuFile.FileName.EndsWith("csv"))
                {
                    using (var pack = new CsvReader(new StreamReader(ms)))
                    {
                        pack.Configuration.TrimHeaders = true;
                        dt = pack.ToDataTable();
                        Session["dt"] = dt;
                        foreach (DataColumn c in dt.Columns)
                        {
                            colHeadings.Add(c.ColumnName);
                            if (!schema.Contains(c.ColumnName))  //If the file contains a field that is not standared
                            {
                                needSync = true;
                                string guid = Guid.NewGuid().ToString();
                                string path = System.IO.Path.GetTempPath();

                                fuFile.SaveAs(path + guid + ".csv");
                                hdnFile.Value = guid + ".csv";

                                displayPnl("Sync");
                            }

                        }

                        if (!needSync)
                            displayPnl("Group");
                            //import(dt);

                    }

                }
                
            }
        }

        protected void btnSync_OnClick(object sender, EventArgs e)
        {
            DataTable dt = Session["dt"] as DataTable;
            
            if (dt.Columns.Contains(txtFirstName.Text))
            {
                dt.Columns[dt.Columns.IndexOf(txtFirstName.Text)].ColumnName = "FirstName";
            }
            if (dt.Columns.Contains(txtLastName.Text))
            {
                dt.Columns[dt.Columns.IndexOf(txtLastName.Text)].ColumnName = "LastName";
            }
            if (dt.Columns.Contains(txtEmail.Text))
            {
                dt.Columns[dt.Columns.IndexOf(txtEmail.Text)].ColumnName = "Email";
            }
            if (dt.Columns.Contains(txtPhone.Text))
            {
                dt.Columns[dt.Columns.IndexOf(txtPhone.Text)].ColumnName = "Phone";
            }
            if (dt.Columns.Contains(txtDOB.Text))
            {
                dt.Columns[dt.Columns.IndexOf(txtDOB.Text)].ColumnName = "DOB";
            }
            if (dt.Columns.Contains(txtForeignKey.Text))
            {
                dt.Columns[dt.Columns.IndexOf(txtForeignKey.Text)].ColumnName = "ForeignKey";
            }
            if (dt.Columns.Contains(txtGender.Text))
            {
                dt.Columns[dt.Columns.IndexOf(txtGender.Text)].ColumnName = "Gender";
            }
            if (dt.Columns.Contains(txtId.Text))
            {
                dt.Columns[dt.Columns.IndexOf(txtId.Text)].ColumnName = "Id";
                
            }
            if (dt.Columns.Contains(txtGraduation.Text))
            {
                dt.Columns[dt.Columns.IndexOf(txtGraduation.Text)].ColumnName = "Graduation";

            }

            Session["dt"] = dt;
            displayPnl("Group");

        }

        protected void btnSkip_OnClick(object sender, EventArgs e)
        {
            DataTable dt = Session["dt"] as DataTable;
            newGroup.Attributes.Add("class", "collapse");
            chooseGroup.Attributes.Add("class", "collapse");

            using (RockContext rc = new RockContext())
            {
                import(dt, null, 0, 0, rc);
            }
        }

        protected void btnGroup_OnClick(object sender, EventArgs e)
        {
            int? groupId = gpGroup.SelectedValueAsInt();
            int? groupTypeId = grpTypeRole.GroupTypeId;
            int? groupRoleId = grpTypeRole.GroupRoleId;
            string newGroupName = txtGroupName.Text;
            DataTable dt = Session["dt"] as DataTable;
            
            using (RockContext rc = new RockContext())
            {
                Group group = null;
                if (!chkNew.Checked)
                    group = new GroupService(rc).Get(groupId.Value);
                else
                {
                    group = new Rock.Model.Group();
                    group.GroupTypeId = groupTypeId.Value;
                    group.Name = newGroupName;
                    group.ParentGroupId = groupId.Value;
                }

                import(dt, group, groupRoleId.Value, groupTypeId.Value, rc);
            }
        }

        protected void gtpType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var typeId = gtpType.SelectedGroupTypeId;
            grpTypeRole.GroupTypeId = typeId.HasValue? typeId: 0 as int?;

            chooseGroup.Attributes.Add("class", "collapse in");
        }

        protected void chkNew_CheckedChanged(object sender, EventArgs e)
        {
            grpTypeRole.GroupRoleId = default(int?);
            grpTypeRole.GroupTypeId = 0 as int?;
            gtpType.GroupTypes = new GroupTypeService(new RockContext()).Queryable().Where(a => a.Roles.Any()).OrderBy(a => a.Name).ToList();

            var newClass = chkNew.Checked ? "collapse in" : "collapse";
            newGroup.Attributes.Add("class", newClass);
            chooseGroup.Attributes.Add("class", "collapse in");
        }

        protected void gpGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            int? groupId = gpGroup.SelectedValueAsInt();

            using (RockContext rockContext = new RockContext())
            {

                var group = new GroupService(rockContext).Queryable()
                            .Where(g => g.Id == groupId)
                            .FirstOrDefault();

                if (group == null)
                    gtpType.GroupTypes = new GroupTypeService(rockContext).Queryable().Where(a => a.Roles.Any()).OrderBy(a => a.Name).ToList();
                else
                    gtpType.SelectedGroupTypeId = group.GroupTypeId as int?;
                grpTypeRole.GroupTypeId = group == null ? 0 as int? : group.GroupTypeId as int?; 
                chooseGroup.Attributes.Add("class", "collapse in");
            }
        }

        public void import(DataTable dt, Group list, int role, int type, RockContext rc)
        {
            
            var personService = new PersonService(rc);
            var groupService = new GroupService(rc);
            
            if (chkNew.Checked)
                groupService.Add(list);
               
            foreach (DataRow r in dt.Rows)
            {
                Rock.Model.Person person = null;
                string fname;
                string lname;
                string email = null;
                try
                {
                    fname = r["FirstName"].ToString().Trim();
                    lname = r["LastName"].ToString().Trim();

                    if (dt.Columns.Contains("Email"))
                        email = r["Email"].ToString().Trim();
                    else if (dt.Columns.Contains("ForeignKey") == false && dt.Columns.Contains("Id") == false)
                        throw new System.Exception("not enough identifying info");
                    
                }
                catch
                {
                    displayPnl("Failure");
                    lbError.Text = "Error with FirstName, LastName, or Email";
                    return;
                }

                
                int pId = -1;
                if (dt.Columns.Contains("Id"))
                {
                    pId = int.Parse(r["Id"].ToString().Trim());
                }

                string foreignKey = null;
                if (dt.Columns.Contains("ForeignKey"))
                    foreignKey = r["ForeignKey"].ToString().Trim();

                try
                {
                    if (pId != -1)
                        person = personService.Queryable().Where(p => p.Id == pId).FirstOrDefault();
                    else if (!String.IsNullOrEmpty(foreignKey))
                        person = personService.Queryable().Where(p => p.ForeignKey == foreignKey).FirstOrDefault();
                    if (person == null)
                    {
                        person = personService.Queryable().Where(p => p.FirstName == fname &&
                                                            p.LastName == lname &&
                                                            p.Email == email).FirstOrDefault();
                    }
                }
                catch
                {
                    person = personService.Queryable().Where(p => p.FirstName == fname &&
                                                            p.LastName == lname &&
                                                            p.Email == email).FirstOrDefault();
                }
                bool newPerson = false;
                if (person == null)
                {
                    newPerson = true;
                    person = new Rock.Model.Person();
                    person.IsEmailActive = true;
                    person.EmailPreference = EmailPreference.EmailAllowed;
                    person.RecordTypeValueId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid()).Id;

                    person.Guid = Guid.NewGuid();
                  
                }

                try
                {
                    person.FirstName = fname;
                    person.LastName = lname;
                    person.Email = email;
                }
                catch
                {
                    displayPnl("Failure");
                    lbError.Text = "Error with FirstName, LastName, or Email";
                    return;
                }

                try
                {
                    string gender = r["Gender"].ToString().Trim();
                    if (gender.ToLower() == "m" || gender.ToLower() == "male")
                    {
                        person.Gender = Rock.Model.Gender.Male;
                    }
                    else if (gender.ToLower() == "f" || gender.ToLower() == "female")
                    {
                        person.Gender = Rock.Model.Gender.Female;
                    }
                    else
                    {
                        person.Gender = Rock.Model.Gender.Unknown;
                    }

                }
                catch
                {
                    person.Gender = Rock.Model.Gender.Unknown;
                }

                try
                {

                    SavePhone(rc, DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid()), person, r["Phone"].ToString().Trim());
                    person.SetBirthDate(DateTime.Parse(r["DOB"].ToString().Trim()));

                }
                catch { }

                try
                {
                    person.ForeignKey = foreignKey;
                }
                catch { }

                try
                {
                    int gradYear = int.Parse(r["Graduation"].ToString().Trim());
                    person.GraduationYear = gradYear;
                }
                catch(System.Exception) { }


                //group member stuff
                if (list != null)
                {
                    if (list.Members.Any(m => m.PersonId == person.Id) == false)
                    {
                        GroupMember gm = new GroupMember();
                        gm.Person = person;
                        gm.GroupRoleId = role;

                        list.Members.Add(gm);
                    }
                }

                if (newPerson)
                {
                    PersonService.SaveNewPerson(person, rc, null, false);
                }

            }

            try
            {
                rc.SaveChanges();
            }
            catch(Exception) { }
            displayPnl("Success");
           
        }

        private void SavePhone(RockContext rockContext, DefinedValueCache numberType, Rock.Model.Person person, string phoneNumber, string ext = "", bool isMobile = false)
        {
            if (numberType != null)
            {
                string cleanNumber = PhoneNumber.CleanNumber(phoneNumber).Left(10);
                var phone = person.PhoneNumbers.FirstOrDefault(p => p.NumberTypeValueId == numberType.Id);
                if (!string.IsNullOrWhiteSpace(cleanNumber))
                {
                    if (phone == null)
                    {
                        phone = new PhoneNumber();
                        person.PhoneNumbers.Add(phone);
                        phone.NumberTypeValueId = numberType.Id;

                        // per requirements they want all mobile numbers to be allowed to use SMS
                        if (isMobile)
                        {
                            phone.IsMessagingEnabled = true;
                        }
                    }
                    phone.Number = cleanNumber;
                    phone.Extension = PhoneNumber.CleanNumber(ext);
                }
                else
                {
                    if (phone != null)
                    {
                        PhoneNumberService phoneService = new PhoneNumberService(rockContext);
                        var currentPhone = phoneService.Get(phone.Id);
                        phoneService.Delete(currentPhone);
                        rockContext.SaveChanges();
                    }
                }
            }
        }

    }


    public static class ExcelPackageExtensions
    {
        public static DataTable ToDataTable(this ExcelPackage package)
        {
            ExcelWorksheet workSheet = package.Workbook.Worksheets.First();
            DataTable table = new DataTable();
            foreach (var firstRowCell in workSheet.Cells[1, 1, 1, workSheet.Dimension.End.Column])
            {
                table.Columns.Add(firstRowCell.Text);
            }

            for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
            {
                var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];
                var newRow = table.NewRow();
                foreach (var cell in row)
                {
                    newRow[cell.Start.Column - 1] = cell.Text;
                }
                table.Rows.Add(newRow);
            }
            return table;
        }
    }

    public static class CsvReaderExtensions
    {
        public static DataTable ToDataTable(this CsvReader csv)
        {   
            DataTable dt = new DataTable();
            csv.Read(); //Do a read so we can get the headers
            foreach (var header in csv.FieldHeaders)
            {
                
                dt.Columns.Add(header.Trim());
            }

            do //Do-while instead of a while loop because we already did the first Read()
            {
                var row = dt.NewRow();
                foreach (DataColumn col in dt.Columns)
                {
                    row[col.ColumnName] = csv.GetField(col.DataType, col.ColumnName);
                }
                dt.Rows.Add(row);
            }
            while (csv.Read());
            return dt;

        }

    }

}