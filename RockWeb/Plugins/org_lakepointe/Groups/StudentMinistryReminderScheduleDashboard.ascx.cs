using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_lakepointe.Groups
{
    [DisplayName( "SM Reminder Schedule DB" )]
    [Category( "LPC > Groups" )]
    [Description( "Presents status of reminder jobs and allows schedules to be enabled and disabled. Jobs should be named \"SM Event Reminders [campus short name] [event token]\"" )]

    [CampusesField(
        name: "Campuses",
        description: "Campuses to include in the dashboard",
        required: true,
        order: 1,
        includeInactive: false)]

    public partial class StudentMinistryReminderScheduleDashboard: RockBlock
    {
        #region Fields

        RockContext _rockContext = null;
        private static string[] EventTokens = { "Connect", "Groups", "United" };
        private static string[] ToggleKeys = { "tConnect", "tGroups", "tUnited" };
        private static string[] LiteralKeys = { "lConnect", "lGroups", "lUnited" };

        #endregion

        #region Properties

        #endregion

        #region Control Methods

        /// <summary>
        /// Executes on every user control initization (an early event on page load)
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _rockContext = new RockContext();

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upnlContent );

            rDashboard.ItemCreated += RDashboard_ItemCreated;
            rDashboard.DataSource = BuildMatrix();
            rDashboard.DataBind();
        }

        /// <summary>
        /// Executes every time that the user control loads. Happens later in the lifecycle than init
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            // check to make sure settings are valid
            if ( !CheckSettings() )
            {
                // if not show warning and hide view panel
                nbNotice.Visible = true;
                pnlView.Visible = false;
            }
            else
            {
                //if valid hide notice and load block
                nbNotice.Visible = false;
                pnlView.Visible = true;

                if ( !Page.IsPostBack )
                {
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Executes the block updated event (i.e. when the block settings have been adjusted)
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        private void RDashboard_ItemCreated(object sender, RepeaterItemEventArgs e)
        {
            CampusData campusData = e.Item.DataItem as CampusData;

            if (campusData != null)
            {
                Literal indicator = e.Item.FindControl("lCampus") as Literal;
                if (indicator != null)
                {
                    indicator.Text = campusData.CampusName;
                }

                for (int i = 0; i < EventTokens.Length; i++)
                {
                    if (campusData.Events.ContainsKey(EventTokens[i]))
                    {
                        ConfigureToggle(e.Item.FindControl(ToggleKeys[i]) as Toggle, campusData.Events[EventTokens[i]]);
                        ConfigureDescription(e.Item.FindControl(LiteralKeys[i]) as Literal, campusData.Events[EventTokens[i]].CronDescription);
                    }
                    else
                    {
                        ((Toggle)e.Item.FindControl(ToggleKeys[i])).Visible = false;
                        ((Literal)e.Item.FindControl(LiteralKeys[i])).Visible = false;
                    }
                }
            }
        }

        private void ConfigureDescription(Literal literal, string description)
        {
            if (literal != null)
            {
                literal.Visible = true;
                literal.Text = description;
            }
        }

        private void ConfigureToggle(Toggle toggle, EventData eventData)
        {
            if (toggle != null)
            {
                toggle.Visible = true;
                toggle.Checked = eventData.JobState;
                toggle.Attributes["JobId"] = eventData.JobId.ToString();
                toggle.CheckedChanged += ToggleCheckedChanged;
                ScriptManager scriptMan = ScriptManager.GetCurrent(Page);
                scriptMan.RegisterAsyncPostBackControl(toggle);
            }
        }

        private void ToggleCheckedChanged(object sender, EventArgs e)
        {
            var toggle = sender as Toggle;
            if (toggle != null)
            {
                var jobId = toggle.Attributes["JobId"].AsIntegerOrNull();
                if (jobId.HasValue)
                {
                    var job = new ServiceJobService(_rockContext).Get(jobId.Value);
                    job.IsActive = !job.IsActive;
                    _rockContext.SaveChanges();
                }
            }
        }

        #endregion

        #region Internal Methods        

        /// <summary>
        /// Verifies and initializes block settings
        /// </summary>
        /// <returns>a true/false flag indicating if settings are valid</returns>
        private bool CheckSettings()
        {
            _rockContext = _rockContext ?? new RockContext();

            return true;
        }

        private List<CampusData> BuildMatrix()
        {
            var enabledCampuses = GetAttributeValue("Campuses").SplitDelimitedValues()
                .Select(c => c.AsGuidOrNull())
                .ToList();

            var campusQuery = new CampusService(_rockContext).Queryable().AsNoTracking()
                .Where(c => c.IsActive.HasValue)
                .Where(c => c.IsActive.Value);

            if (enabledCampuses.Count > 0)
            {
                campusQuery = campusQuery.Where(c => enabledCampuses.Contains(c.Guid));
            }

            var campuses = campusQuery.OrderBy("Order");

            var jobQuery = new ServiceJobService(_rockContext).Queryable();

            var values = new List<CampusData>();

            foreach (var campus in campuses)
            {
                var item = new CampusData();
                item.CampusName = campus.Name;

                var campusJobQuery = jobQuery.Where(j => j.Name.StartsWith("SM Event Reminders " + campus.ShortCode));
                foreach (var token in EventTokens)
                {
                    var job = campusJobQuery.Where(j => j.Name.Contains(token)).FirstOrDefault();
                    if (job != null)
                    {
                        item.Events.Add(token, new EventData(token, job));
                    }
                }

                values.Add(item);
            }

            return values;
        }

        #endregion

        public class CampusData
        {
            public string CampusName;
            public Dictionary<string, EventData> Events = new Dictionary<string, EventData>();
        }

        public class EventData
        {
            public EventData(string eventName, ServiceJob job)
            {
                EventName = eventName;
                JobId = job.Id;
                JobState = job.IsActive ?? false;
                CronDescription = job.CronDescription;
            }

            public string EventName;
            public int JobId;
            public bool JobState;
            public string CronDescription;
        }
    }
}