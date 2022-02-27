using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using com.intulse.PbxComponent.Services;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Web.UI;
using System.Web.UI.WebControls;
using com.intulse.PbxComponent.Models;
using System.Collections.Generic;

namespace RockWeb.Plugins.com_intulse.PbxComponent
{
    /// <summary>
    /// Intulse Communications Block
    /// </summary>
    [DisplayName("Intulse Communication Block")]
    [Category("Intulse > Communication Block")]
    [Description("Displays all Intulse communications")]
    public partial class CommunicationsBlock : RockBlock
    {
        private CallDetailRecordService _cdrService = new CallDetailRecordService(new RockContext());
        private SmsMessageService _smsService = new SmsMessageService(new RockContext());
        private CommunicationNoteService _noteService = new CommunicationNoteService(new RockContext());

        protected static class PageParameterKey
        {
            public const string personIdKey = "PersonId";
            public const string businessIdKey = "BusinessId";
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Default date range
            filterDates.LowerValue = DateTime.Today.AddMonths(-3);
            filterDates.UpperValue = DateTime.Today.AddDays(1);

            gridFilterCommunications.Show();
            gridCommunications.GridRebind += gridCommunications_GridRebind;

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
                BindGrid();
            }
        }

        /// <summary>
        /// Creates the Message/Notes column
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gridCommunications_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            var communication = e.Row.DataItem as CommunicationDisplay;

            if (communication == null)
            {
                return;
            }

            var communicationNoteLiteral = e.Row.FindControl("communicationNoteLiteral") as Literal;

            if (communicationNoteLiteral != null)
            {
                communicationNoteLiteral.Text = communication.Description;

                if (!string.IsNullOrWhiteSpace(communication.Note))
                {
                    communicationNoteLiteral.Text += "<div style=\"margin-top:5px; padding-top:5px; border-top:1px solid #ccc; font-size:80%;\"><strong>NOTE: </strong>" + communication.Note + "</div>";
                }
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gridCommunications control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gridCommunications_GridRebind(object sender, EventArgs e)
        {
            BindGrid();
        }


        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated(object sender, EventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// Handles applying filters.
        /// </summary>
        protected void gridFilterCommunications_ApplyFilterClick(object sender, EventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the button click to open the note modal
        /// </summary>
        protected void gridCommunications_Edit(object sender, RowEventArgs e)
        {
            noteModalCommunicationId.Value = e.RowKeyValue.ToString();

            var note = _noteService.GetNoteByCommunicationId(noteModalCommunicationId.Value);

            noteModalTextbox.Text = note;

            noteModal.Show();
        }

        /// <summary>
        /// Handles saving a note
        /// </summary>
        protected void noteModal_Save(object sender, EventArgs e)
        {
            noteModal.Hide();

            _noteService.CreateOrUpdateCommunicationNote(noteModalCommunicationId.Value, noteModalTextbox.Text);

            BindGrid();
        }

        /// <summary>
        /// Binds the data to the grid
        /// </summary>
        protected void BindGrid()
        {
            errorBox.Visible = false;

            var communications = new List<CommunicationDisplay>();
            int personId;

            if (!int.TryParse(PageParameter(PageParameterKey.personIdKey), out personId))
            {
                personId = int.Parse(PageParameter(PageParameterKey.businessIdKey));
            }

            var showCdr = filterShowCdr.Checked;
            var showSms = filterShowSms.Checked;
            var nameFilter = filterName.Text;
            var numberFilter = filterNumber.Text;
            var dateFilter = filterDates.DelimitedValues;

            var picker = new DateRangePicker();
            picker.DelimitedValues = dateFilter;

            var filterStartDate = picker.LowerValue;
            var filterEndDate = picker.UpperValue;

            if (filterStartDate != null)
            {
                filterStartDate = ((DateTime)filterStartDate).ToUniversalTime();
            }

            if (filterEndDate != null)
            {
                filterEndDate = ((DateTime)filterEndDate).ToUniversalTime().AddDays(1); // Have to add one day to end date as it defaults to midnight (making it excluded)
            }

            try
            {
                if (showCdr)
                {
                    communications.AddRange(_cdrService.GetPersonsCdrs(personId, nameFilter, numberFilter, filterStartDate, filterEndDate));
                }

                if (showSms)
                {
                    communications.AddRange(_smsService.GetPersonSmsMessages(personId, nameFilter, numberFilter, filterStartDate, filterEndDate));
                }

                if (communications.Count > 0)
                {
                    SortProperty sortProperty = gridCommunications.SortProperty;

                    if (sortProperty != null)
                    {
                        var property = communications
                            .First()
                            .GetType()
                            .GetProperty(sortProperty.Property);

                        if (sortProperty.Direction == SortDirection.Ascending)
                        {
                            communications = communications.OrderBy(r => property.GetValue(r, null)).ToList();
                        }
                        else
                        {
                            communications = communications.OrderByDescending(r => property.GetValue(r, null)).ToList();
                        }
                    }
                    else
                    {
                        communications = communications.OrderByDescending(r => r.CommunicationDateUtc).ToList();
                    }

                    communications.ForEach(r => r.CommunicationDateUtc = RockDateTime.ConvertLocalDateTimeToRockDateTime(r.CommunicationDateUtc.ToLocalTime()));
                }

                gridCommunications.DataSource = communications;
            }
            catch (Exception ex)
            {
                LogException(ex);
                errorBox.Visible = true;
            }

            errorBox.DataBind();
            gridCommunications.DataBind();
        }
    }
}