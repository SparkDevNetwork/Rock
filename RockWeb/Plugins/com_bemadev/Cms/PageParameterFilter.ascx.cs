using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_bemadev.Cms
{
    [DisplayName("Page Parameter Filter")]
    [Category("com_bemadev > Cms")]
    [Description("A collection of filters that will pass its values as page parameters.")]

    [TextField("Boolean Select", "", false, "", "CustomSetting", 1)]
    [TextField("Boolean Select 2", "", false, "", "CustomSetting", 2)]
    [TextField("Text Entry", "", false, "", "CustomSetting", 3)]

    [TextField("Date Range Label", "", false, "", "CustomSetting", 1)]
    [TextField("Date Range Label 2", "", false, "", "CustomSetting", 2)]
    [TextField("Date Label", "", false, "", "CustomSetting", 3)]

    [TextField("Multi Select Label", "", false, "", "CustomSetting", 1)]
    [KeyValueListField("Multi Select List", "", false, "", "Name", "Value", "", "", "CustomSetting", 2)]
    [TextField("Multi Select Label 2", "", false, "", "CustomSetting", 3)]
    [KeyValueListField("Multi Select List 2", "", false, "", "Name", "Value", "", "", "CustomSetting", 4)]

    [TextField("Number Range Label", "", false, "", "CustomSetting", 1)]
    [TextField("Number Range Label 2", "", false, "", "CustomSetting", 2)]
    [TextField("Number Range Label 3", "", false, "", "CustomSetting", 3)]

    [BooleanField("Enable Campuses Filter", "Enables the campus filter.", false, "CustomSetting", 1)]
    [BooleanField("Enable Account Filter", "Enables the account filter.", false, "CustomSetting", 2)]
    [BooleanField("Enable Person Filter", "Enables the person filter.", false, "CustomSetting", 3)]
    [BooleanField("Enable Groups Filter", "Enables the groups filter.", false, "CustomSetting", 4)]
    [TextField("Time Range Label", "", false, "", "CustomSetting", 2)]

    [LinkedPage("Page Redirect", "If set, the filter button will redirect to the selected page.", false, "", "CustomSetting", 1)]
    [TextField("Button Text", "Text that will be displayed on \"GO\" button", true, "Filter", "CustomSetting", 2)]
    [TextField("Block Text", "Text that will be displayed as the Block name", true, "Filter", "CustomSetting", 3)]
    public partial class PageParameterFilter : RockBlockCustomSettings
    {
        private List<string> _boolOptions = new List<string>();

        private string _booleanLabel;
        private string _booleanLabel2;
        private string _textLabel;
        private string _dateRangeLabel;
        private string _dateRangeLabel2;
        private string _dateLabel;
        private string _multiSelectLabel;
        private string _multiSelectLabel2;
        private string _numberRangeLabel;
        private string _numberRangeLabel2;
        private string _numberRangeLabel3;
        private bool _enableCampuses;
        private bool _enableAccounts;
        private bool _enablePersonPicker;
        private bool _enableGroupsPicker;
        private string _timeRangeLabel;

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // load up bool options
            _boolOptions.Add("");
            _boolOptions.Add("True");
            _boolOptions.Add("False");

            _booleanLabel = GetAttributeValue("BooleanSelect");
            _booleanLabel2 = GetAttributeValue("BooleanSelect2");
            _textLabel = GetAttributeValue("TextEntry");

            _dateRangeLabel = GetAttributeValue("DateRangeLabel");
            _dateRangeLabel2 = GetAttributeValue("DateRangeLabel2");
            _dateLabel = GetAttributeValue("DateLabel");

            _multiSelectLabel = GetAttributeValue("MultiSelectLabel");
            _multiSelectLabel2 = GetAttributeValue("MultiSelectLabel2");

            _numberRangeLabel = GetAttributeValue("NumberRangeLabel");
            _numberRangeLabel2 = GetAttributeValue("NumberRangeLabel2");
            _numberRangeLabel3 = GetAttributeValue("NumberRangeLabel3");

            _enableCampuses = GetAttributeValue("EnableCampusesFilter").AsBoolean();
            _enableAccounts = GetAttributeValue("EnableAccountFilter").AsBoolean();
            _enablePersonPicker = GetAttributeValue("EnablePersonFilter").AsBoolean();
            _enableGroupsPicker = GetAttributeValue("EnableGroupsFilter").AsBoolean();
            _timeRangeLabel = GetAttributeValue("TimeRangeLabel");

            // this event gets fired after block settings are updated.
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger(upnlContent);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                // Setting button and block text
                btnFilter.Text = GetAttributeValue("ButtonText");
                lbBlockText.InnerText = GetAttributeValue("BlockText");

                LoadFilters();
            }
        }

        #endregion

        #region Methods

        private void LoadFilters()
        {
            // Boolean 1
            if (!String.IsNullOrWhiteSpace(_booleanLabel))
            {
                ddlBoolean1Div.Visible = true;
                ddlBoolean1.Label = _booleanLabel;
                ddlBoolean1.DataSource = _boolOptions;
                ddlBoolean1.DataBind();
            }

            var selectedCustomBool = Request.QueryString[_booleanLabel.RemoveSpaces()];
            if (selectedCustomBool != null)
            {
                ddlBoolean1.SetValue(selectedCustomBool);
            }

            // Boolean 2
            if (!String.IsNullOrWhiteSpace(_booleanLabel2))
            {
                ddlBoolean2Div.Visible = true;
                ddlBoolean2.Label = _booleanLabel2;
                ddlBoolean2.DataSource = _boolOptions;
                ddlBoolean2.DataBind();
            }

            var selectedCustomBool2 = Request.QueryString[_booleanLabel2.RemoveSpaces()];
            if (selectedCustomBool2 != null)
            {
                ddlBoolean2.SetValue(selectedCustomBool2);
            }

            // Text
            if (!String.IsNullOrWhiteSpace(_textLabel))
            {
                tbTextDiv.Visible = true;
                tbText.Label = _textLabel;
            }

            var textValue = Request.QueryString[_textLabel.RemoveSpaces()];
            if (textValue != null)
            {
                tbText.Text = textValue;
            }

            // Date Range 1
            if (!String.IsNullOrWhiteSpace(_dateRangeLabel))
            {
                drpDateRangeDiv.Visible = true;
                drpDateRange.Label = _dateRangeLabel;
            }

            var dateRange = Request.QueryString[_dateRangeLabel.RemoveSpaces()];
            if (dateRange != null)
            {
                drpDateRange.DelimitedValues = dateRange;
            }

            // Date Range 2
            if (!String.IsNullOrWhiteSpace(_dateRangeLabel2))
            {
                drpDateRange2Div.Visible = true;
                drpDateRange2.Label = _dateRangeLabel2;
            }

            var dateRange2 = Request.QueryString[_dateRangeLabel2.RemoveSpaces()];
            if (dateRange2 != null)
            {
                drpDateRange2.DelimitedValues = dateRange2;
            }

            // Date
            if (!String.IsNullOrWhiteSpace(_dateLabel))
            {
                dpDateDiv.Visible = true;
                dpDate.Label = _dateLabel;
            }

            var date = Request.QueryString[_dateLabel.RemoveSpaces()];
            if (date != null)
            {
                dpDate.SelectedDate = date.AsDateTime();
            }

            //  Multi Select 1
            var selections1 = GetCustomMultiSelectListData("MultiSelectList");
            if (selections1.Any())
            {
                cbMultiSelectList.Label = _multiSelectLabel;
                cbMultiSelectListDiv.Visible = true;
                cbMultiSelectList.Items.Clear();

                foreach (var selection in selections1)
                {
                    cbMultiSelectList.Items.Add(new ListItem(selection.Key, selection.Value.ToString()));
                }
            }

            var selectedItems1 = Request.QueryString[_multiSelectLabel.RemoveSpaces()];
            if (selectedItems1 != null)
            {
                cbMultiSelectList.SetValues(selectedItems1.SplitDelimitedValues(false));
            }

            //  Multi Select 2
            var selections2 = GetCustomMultiSelectListData("MultiSelectList2");
            if (selections2.Any())
            {
                cbMultiSelectList2.Label = _multiSelectLabel2;
                cbMultiSelectList2Div.Visible = true;
                cbMultiSelectList2.Items.Clear();

                foreach (var selection in selections2)
                {
                    cbMultiSelectList2.Items.Add(new ListItem(selection.Key, selection.Value.ToString()));
                }
            }

            var selectedItems2 = Request.QueryString[_multiSelectLabel2.RemoveSpaces()];
            if (selectedItems2 != null)
            {
                cbMultiSelectList2.SetValues(selectedItems2.SplitDelimitedValues(false));
            }

            // Number Range
            if (!String.IsNullOrWhiteSpace(_numberRangeLabel))
            {
                nrNumberRange.Label = _numberRangeLabel;
                nrNumberRangeDiv.Visible = true;
            }

            var numberRangeStart = Request.QueryString[(_numberRangeLabel + "Start").RemoveSpaces()];
            if (numberRangeStart.AsDecimalOrNull() != null)
            {
                nrNumberRange.LowerValue = numberRangeStart.AsDecimalOrNull();
            }
            var numberRangeEnd = Request.QueryString[(_numberRangeLabel + "End").RemoveSpaces()];
            if (numberRangeEnd.AsDecimalOrNull() != null)
            {
                nrNumberRange.UpperValue = numberRangeEnd.AsDecimalOrNull();
            }

            // Number Range 2
            if (!String.IsNullOrWhiteSpace(_numberRangeLabel2))
            {
                nrNumberRange2.Label = _numberRangeLabel2;
                nrNumberRangeDiv2.Visible = true;
            }

            var numberRangeStart2 = Request.QueryString[(_numberRangeLabel2 + "Start").RemoveSpaces()];
            if (numberRangeStart2.AsDecimalOrNull() != null)
            {
                nrNumberRange2.LowerValue = numberRangeStart2.AsDecimalOrNull();
            }
            var numberRangeEnd2 = Request.QueryString[(_numberRangeLabel2 + "End").RemoveSpaces()];
            if (numberRangeEnd2.AsDecimalOrNull() != null)
            {
                nrNumberRange2.UpperValue = numberRangeEnd2.AsDecimalOrNull();
            }

            // Number Range 3
            if (!String.IsNullOrWhiteSpace(_numberRangeLabel3))
            {
                nrNumberRange3.Label = _numberRangeLabel3;
                nrNumberRangeDiv3.Visible = true;
            }

            var numberRangeStart3 = Request.QueryString[(_numberRangeLabel3 + "Start").RemoveSpaces()];
            if (numberRangeStart3.AsDecimalOrNull() != null)
            {
                nrNumberRange3.LowerValue = numberRangeStart3.AsDecimalOrNull();
            }
            var numberRangeEnd3 = Request.QueryString[(_numberRangeLabel3 + "End").RemoveSpaces()];
            if (numberRangeEnd3.AsDecimalOrNull() != null)
            {
                nrNumberRange3.UpperValue = numberRangeEnd3.AsDecimalOrNull();
            }

            // Campus
            if (_enableCampuses)
            {
                cpCampusesDiv.Visible = true;
                cpCampusesPicker.Campuses = CampusCache.All().Where(c => c.IsActive == true).ToList();
            }

            var selectedCampusIds = Request.QueryString["CampusIds"];
            if (selectedCampusIds != null)
            {
                cpCampusesPicker.SetValues(selectedCampusIds.SplitDelimitedValues(false));
            }

            // Account
            if (_enableAccounts)
            {
                apAcountsDiv.Visible = true;
            }

            var selectedAccountIds = Request.QueryString["AccountIds"];
            if (!string.IsNullOrEmpty(selectedAccountIds))
            {
                var accounts = new FinancialAccountService(new RockContext());

                apAccountsPicker.SetValues(accounts.GetByIds(selectedAccountIds.SplitDelimitedValues(false).AsIntegerList()));
            }

            // Person
            if (_enablePersonPicker)
            {
                ppPersonDiv.Visible = true;
            }

            var selectedPersonId = Request.QueryString["PersonId"].AsIntegerOrNull();
            if (selectedPersonId.HasValue)
            {
                ppPersonPicker.SetValue(new PersonService(new RockContext()).Queryable().Where(p => p.Id == selectedPersonId.Value).FirstOrDefault());
            }

            // Groups
            if (_enableGroupsPicker)
            {
                gpGroupsDiv.Visible = true;
            }

            var selectedGroupIds = Request.QueryString["GroupIds"];
            if (!string.IsNullOrEmpty(selectedGroupIds))
            {
                var groups = new GroupService(new RockContext());
                gpGroupsPicker.SetValues(groups.GetByIds(selectedGroupIds.SplitDelimitedValues(false).AsIntegerList()));
            }

            //Time Range        
            if (!String.IsNullOrWhiteSpace(_timeRangeLabel))
            {
                tpTimeRangeDiv.Visible = true;
                tpTimeBeg.Label = "Start " + _timeRangeLabel;
                tpTimeEnd.Label = "End " + _timeRangeLabel;

            }
        }

        private Dictionary<string, object> GetCustomMultiSelectListData(string attributeKey)
        {
            var properties = new Dictionary<string, object>();

            var selectionString = GetAttributeValue(attributeKey);

            if (!String.IsNullOrWhiteSpace(selectionString))
            {
                selectionString = selectionString.TrimEnd('|');
                var selections = selectionString.Split('|')
                                .Select(s => s.Split('^'))
                                .Select(p => new { Name = p[0], Value = p[1] });

                StringBuilder sbPageMarkup = new StringBuilder();
                foreach (var selection in selections)
                {
                    properties.Add(selection.Name, selection.Value);
                }
            }
            return properties;
        }

        /// <summary>
        /// Shows the settings.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void ShowSettings()
        {
            pnlEditModel.Visible = true;
            upnlContent.Update();
            mdEdit.Show();

            tbBooleanFilter.Text = GetAttributeValue("BooleanSelect");
            tbBooleanFilter2.Text = GetAttributeValue("BooleanSelect2");
            tbTextFilter.Text = GetAttributeValue("TextEntry");

            tbDateRange.Text = GetAttributeValue("DateRangeLabel");
            tbDateRange2.Text = GetAttributeValue("DateRangeLabel2");

            tbDate.Text = GetAttributeValue("DateLabel");

            tbMultiSelectLabel.Text = GetAttributeValue("MultiSelectLabel");
            kvMultiSelect.Value = GetAttributeValue("MultiSelectList");
            tbMultiSelectLabel2.Text = GetAttributeValue("MultiSelectLabel2");
            kvMultiSelect2.Value = GetAttributeValue("MultiSelectList2");

            tbNumberRangeLabel.Text = GetAttributeValue("NumberRangeLabel");
            tbNumberRangeLabel2.Text = GetAttributeValue("NumberRangeLabel2");
            tbNumberRangeLabel3.Text = GetAttributeValue("NumberRangeLabel3");

            ddlEnableCampusesPicker.SetValue(GetAttributeValue("EnableCampusesFilter"));
            ddlEnableAccountsPicker.SetValue(GetAttributeValue("EnableAccountFilter"));
            ddlEnablePersonPicker.SetValue(GetAttributeValue("EnablePersonFilter"));
            ddlEnableGroupsPicker.SetValue(GetAttributeValue("EnableGroupsFilter"));
            tbTimeRangeLabel.Text = GetAttributeValue("TimeRangeLabel");

            txButtonText.Text = GetAttributeValue("ButtonText");
            txtBlockText.Text = GetAttributeValue("BlockText");

            var pageId = GetAttributeValue("PageRedirect").AsIntegerOrNull();
            if (pageId > 0)
            {
                ppRedirectPage.SetValue(new PageService(new RockContext()).Queryable().Where(p => p.Id == pageId).FirstOrDefault());
            }
        }

        /// <summary>
        /// Converts time to DB time (Example 4:15 PM will be 16:15:00.0000000)
        /// </summary>
        /// <param name="sTime"></param>
        /// <returns></returns>
        private string ConvertToDbTimeFormat(string sTime)
        {
            string timeVal = "01:00:00.0000000";
            try
            {
                string[] timeValSplit = sTime.Split(':');

                switch (sTime.Right(2).ToUpper())
                {
                    case "PM":
                        switch (timeValSplit[0].ToString())
                        {
                            case "0":
                                timeVal = "13";
                                break;
                            case "1":
                                timeVal = "13";
                                break;
                            case "2":
                                timeVal = "14";
                                break;
                            case "3":
                                timeVal = "15";
                                break;
                            case "4":
                                timeVal = "16";
                                break;
                            case "5":
                                timeVal = "17";
                                break;
                            case "6":
                                timeVal = "18";
                                break;
                            case "7":
                                timeVal = "19";
                                break;
                            case "8":
                                timeVal = "20";
                                break;
                            case "9":
                                timeVal = "21";
                                break;
                            case "10":
                                timeVal = "22";
                                break;
                            case "11":
                                timeVal = "23";
                                break;
                            case "12":
                                timeVal = "12";
                                break;
                        }
                        break;
                    case "AM":
                        if (Convert.ToInt32(timeValSplit[0]) < 10)
                        {
                            timeVal = "0" + timeValSplit[0].ToString();
                        }
                        else if (Convert.ToInt32(timeValSplit[0]) == 12)
                        {
                            timeVal = "24";
                        }
                        else
                        {
                            timeVal = timeValSplit[0].ToString();
                        }
                        break;
                }
                timeVal = timeVal + ":" + timeValSplit[1].ToString() + ":00.0000000";
                timeVal = timeVal.Replace("AM", "").Replace("PM", "").RemoveSpaces();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                timeVal = "01:00:00.0000000";
            }
            return timeVal;
        }
        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated(object sender, EventArgs e)
        {
            LoadFilters();
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            var queryString = HttpUtility.ParseQueryString(String.Empty);

            // Boolean Select 1
            if (!String.IsNullOrWhiteSpace(ddlBoolean1.SelectedValue))
            {
                queryString.Set(_booleanLabel.RemoveSpaces().RemoveSpecialCharacters(), ddlBoolean1.SelectedValue.ToString());
            }

            // Boolean Select 2
            if (!String.IsNullOrWhiteSpace(ddlBoolean2.SelectedValue))
            {
                queryString.Set(_booleanLabel2.RemoveSpaces().RemoveSpecialCharacters(), ddlBoolean2.SelectedValue.ToString());
            }

            // Text Box
            if (!String.IsNullOrWhiteSpace(tbText.Text))
            {
                var textString = tbText.Text;
                textString = textString.Replace("\n", ",");
                textString = textString.Replace(" ", ",");

                queryString.Set(_textLabel.RemoveSpaces().RemoveSpecialCharacters(), textString);
            }

            // Date Range 1
            if (drpDateRange.LowerValue.HasValue && drpDateRange.UpperValue.HasValue)
            {
                queryString.Set(_dateRangeLabel.RemoveSpaces().RemoveSpecialCharacters(),
                    drpDateRange.LowerValue.Value.ToShortDateString() + "," + drpDateRange.UpperValue.Value.ToShortDateString());
            }

            // Date Range 2
            if (drpDateRange2.LowerValue.HasValue && drpDateRange2.UpperValue.HasValue)
            {
                queryString.Set(_dateRangeLabel2.RemoveSpaces().RemoveSpecialCharacters(),
                    drpDateRange2.LowerValue.Value.ToShortDateString() + "," + drpDateRange2.UpperValue.Value.ToShortDateString());
            }

            // Date            
            if (dpDate.SelectedDate.HasValue)
            {
                queryString.Set(_dateLabel.RemoveSpaces().RemoveSpecialCharacters(),
                    dpDate.SelectedDate.Value.ToShortDateString());
            }

            // Multi Select List 1
            if (cbMultiSelectList.SelectedValues.AsDelimited(",").Any())
            {
                queryString.Set(_multiSelectLabel.RemoveSpaces().RemoveSpecialCharacters(), cbMultiSelectList.SelectedValues.AsDelimited(","));
            }

            // Multi Select List 2
            if (cbMultiSelectList2.SelectedValues.AsDelimited(",").Any())
            {
                queryString.Set(_multiSelectLabel2.RemoveSpaces().RemoveSpecialCharacters(), cbMultiSelectList2.SelectedValues.AsDelimited(","));
            }

            // Number Range
            if (nrNumberRange.LowerValue != null)
            {
                queryString.Set(_numberRangeLabel.RemoveSpaces().RemoveSpecialCharacters() + "Start", nrNumberRange.LowerValue.ToString());
            }
            if (nrNumberRange.UpperValue != null)
            {
                queryString.Set(_numberRangeLabel.RemoveSpaces().RemoveSpecialCharacters() + "End", nrNumberRange.UpperValue.ToString());
            }

            // Number Range 2
            if (nrNumberRange2.LowerValue != null)
            {
                queryString.Set(_numberRangeLabel2.RemoveSpaces().RemoveSpecialCharacters() + "Start", nrNumberRange2.LowerValue.ToString());
            }
            if (nrNumberRange2.UpperValue != null)
            {
                queryString.Set(_numberRangeLabel2.RemoveSpaces().RemoveSpecialCharacters() + "End", nrNumberRange2.UpperValue.ToString());
            }

            // Number Range 3
            if (nrNumberRange3.LowerValue != null)
            {
                queryString.Set(_numberRangeLabel3.RemoveSpaces().RemoveSpecialCharacters() + "Start", nrNumberRange3.LowerValue.ToString());
            }
            if (nrNumberRange3.UpperValue != null)
            {
                queryString.Set(_numberRangeLabel3.RemoveSpaces().RemoveSpecialCharacters() + "End", nrNumberRange3.UpperValue.ToString());
            }

            // Campus 
            if (!String.IsNullOrWhiteSpace(cpCampusesPicker.SelectedValue))
            {
                queryString.Set("CampusIds", cpCampusesPicker.SelectedCampusIds.AsDelimited(","));
            }

            // Account 
            if (!String.IsNullOrWhiteSpace(apAccountsPicker.SelectedValue) && apAccountsPicker.SelectedValue != "0")
            {
                queryString.Set("AccountIds", apAccountsPicker.SelectedValues.ToList().AsDelimited(","));
            }

            // Person 
            if (ppPersonPicker.PersonId.HasValue)
            {
                queryString.Set("PersonId", ppPersonPicker.PersonId.Value.ToString());
            }

            // Group
            if (!String.IsNullOrWhiteSpace(gpGroupsPicker.SelectedValue) && gpGroupsPicker.SelectedValue != "0")
            {
                queryString.Set("GroupIds", gpGroupsPicker.SelectedValues.ToList().AsDelimited(","));
            }

            // Time Range
            if (tpTimeBeg.Text != null && tpTimeEnd.Text != null)
            {
                string begTime = ConvertToDbTimeFormat(tpTimeBeg.Text);
                string endTime = ConvertToDbTimeFormat(tpTimeEnd.Text);

                queryString.Set(_timeRangeLabel.RemoveSpaces().RemoveSpecialCharacters(),
                    begTime + "," + endTime);
            }

            // Page Redirect
            string url = Request.Url.AbsolutePath;
            if (GetAttributeValue("PageRedirect").AsIntegerOrNull() > 0)
            {
                int pageId = GetAttributeValue("PageRedirect").AsInteger();

                url = System.Web.VirtualPathUtility.ToAbsolute(string.Format("~/page/{0}", pageId));
            }


            if (queryString.AllKeys.Any())
            {
                Response.Redirect(string.Format("{0}?{1}", url, queryString), false);
            }
            else
            {
                Response.Redirect(url, false);
            }
        }

        protected void lbSave_Click(object sender, EventArgs e)
        {
            SetAttributeValue("BooleanSelect", tbBooleanFilter.Text);
            SetAttributeValue("BooleanSelect2", tbBooleanFilter2.Text);
            SetAttributeValue("TextEntry", tbTextFilter.Text);

            SetAttributeValue("DateRangeLabel", tbDateRange.Text);
            SetAttributeValue("DateRangeLabel2", tbDateRange2.Text);

            SetAttributeValue("DateLabel", tbDate.Text);

            SetAttributeValue("MultiSelectLabel", tbMultiSelectLabel.Text);
            SetAttributeValue("MultiSelectList", kvMultiSelect.Value);
            SetAttributeValue("MultiSelectLabel2", tbMultiSelectLabel2.Text);
            SetAttributeValue("MultiSelectList2", kvMultiSelect2.Value);

            SetAttributeValue("NumberRangeLabel", tbNumberRangeLabel.Text);
            SetAttributeValue("NumberRangeLabel2", tbNumberRangeLabel2.Text);
            SetAttributeValue("NumberRangeLabel3", tbNumberRangeLabel3.Text);

            SetAttributeValue("EnableCampusesFilter", ddlEnableCampusesPicker.Text);
            SetAttributeValue("EnableAccountFilter", ddlEnableAccountsPicker.Text);
            SetAttributeValue("EnablePersonFilter", ddlEnablePersonPicker.Text);
            SetAttributeValue("EnableGroupsFilter", ddlEnableGroupsPicker.Text);
            SetAttributeValue("TimeRangeLabel", tbTimeRangeLabel.Text);

            SetAttributeValue("ButtonText", txButtonText.Text);
            SetAttributeValue("BlockText", txtBlockText.Text);
            SetAttributeValue("PageRedirect", ppRedirectPage.SelectedValue);

            SaveAttributeValues();

            LoadFilters();

            mdEdit.Hide();
            pnlEditModel.Visible = false;
            upnlContent.Update();

            Response.Redirect(Request.Url.AbsolutePath);
        }       
        #endregion
    }
}