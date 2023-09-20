using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;


namespace RockWeb.Plugins.org_lakepointe.Finance
{
    [AccountField("Primary Account",
        Description = "The primary pledge account.",
        IsRequired = true,
        DefaultValue = "ff9499e5-e5dc-45d2-bf22-c740a6ea6902",
        Key = AttributeKey.PrimaryAccount,
        Order = 1)]
    [BooleanField("Send Confirmations",
        "A true/false flag indicating if the pledge confirmation emails should be sent.",
        TrueText = "Yes",
        FalseText = "No",
        DefaultBooleanValue = true,
        Key = AttributeKey.SendConfirmation,
        Order = 2)]

    [TextField("Pledge Source",
        Description = "The source of the pledge. Default is 'Card'",
        IsRequired = false,
        DefaultValue = "Card",
        Key = AttributeKey.PledgeSource,
        Order = 3)]

    [DateField("Default Start Date",
        Description = "The default start date for the pledge",
        IsRequired = false,
        Key = AttributeKey.StartDate,
        Order = 4)]

    [DateField("Default End Date",
        Description = "The default end date for the pledge.",
        IsRequired = false,
        Key = AttributeKey.EndDate,
        Order = 5)]

    [SystemCommunicationField("English Confirmation",
        Description = "The system communciation for the English Confirmation",
        IsRequired = false,
        Key = AttributeKey.EnglishConfirmation,
        Order = 6)]

    [SystemCommunicationField("Spanish Confirmation",
        Description = "The system communicaiton for the Spanish Confirmation",
        IsRequired = false,
        Key = AttributeKey.SpanishConfirmation,
        Order = 7)]

    [DisplayName("More Than Us Pledge")]
    [Category("LPC > Finance")]
    [Description("More Than Us Pledge Data Entry Page")]
    public partial class MoreThanUsPledgeEntry : RockBlock
    {
        public static class AttributeKey
        {
            public const string PrimaryAccount = "PrimaryAccount";
            public const string SendConfirmation = "SendConfirmation";
            public const string PledgeSource = "PledgeSource";
            public const string StartDate = "StartDate";
            public const string EndDate = "EndDate";
            public const string EnglishConfirmation = "EnglishConfirmation";
            public const string SpanishConfirmation = "SpanishConfirmation";
        }

        #region Base Control Methods
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            lbSave.Click += lbSave_Click;
            lbCancel.Click += lbCancel_Click;
            lbSaveAndAdd.Click += lbSaveAndAdd_Click;
            lbEdit.Click += lbEdit_Click;
        }



        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SetNotificationBox(string.Empty, string.Empty);

            if (!IsPostBack)
            {
                var pledgeId = PageParameter("PledgeId").AsIntegerOrNull();

                if (pledgeId.HasValue && pledgeId > 0)
                {
                    hfPledgeId.Value = pledgeId.ToString();
                    LoadPledgeDetail(pledgeId.Value);
                }
                else
                {
                    LoadPledgeEdit();
                }
            }
        }
        #endregion

        #region Events
        private void lbCancel_Click(object sender, EventArgs e)
        {
            var pledgeId = hfPledgeId.Value.AsIntegerOrNull();

            if (pledgeId.HasValue && pledgeId > 0)
            {
                LoadPledgeDetail(pledgeId.Value);
            }
            else
            {
                NavigateToParentPage();
            }
        }

        private void lbEdit_Click(object sender, EventArgs e)
        {
            LoadPledgeEdit();
        }

        private void lbSaveAndAdd_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            var pledgeId = SavePledge();

            if (pledgeId > 0)
            {
                SetNotificationBox("Pledge Saved", "The pledge has been successfully saved.", NotificationBoxType.Success);
                ClearFields();
                SetDefaults();
            }
        }



        private void lbSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }
            var pledgeId = SavePledge();
            hfPledgeId.Value = pledgeId.ToString();
            SetNotificationBox("Pledge Saved", "The pledge has been successfully saved.", NotificationBoxType.Success);
            LoadPledgeDetail(pledgeId);
        }

        #endregion


        #region Methods

        private void ClearFields()
        {

            pPerson.SetValue(null);
            pPledgeAccount.SetValue(null);
            dpStartDate.SelectedDate = null;
            dpEndDate.SelectedDate = null;
            tbPledgeAmount.Text = string.Empty;
            tbOneTimeGiftAmount.Text = string.Empty;
            hfSource.Value = string.Empty;
        }

        private void LoadPledgeDetail(int id)
        {
            pnlEdit.Visible = false;
            pnlView.Visible = false;
            using (var pledgeContext = new RockContext())
            {
                var pledgeService = new FinancialPledgeService(pledgeContext);
                var pledge = pledgeService.Queryable().AsNoTracking()
                    .Include(p => p.PersonAlias.Person)
                    .Include(p => p.Account)
                    .Where(p => p.Id == id)
                    .SingleOrDefault();

                if (pledge == null)
                {
                    SetNotificationBox("Pledge Not Found", "The pledge that you requested was not found.", NotificationBoxType.Warning);
                    return;
                }

                pledge.LoadAttributes(pledgeContext);
                var personUrl = ResolveRockUrl(string.Format("~/Person/{0}", pledge.PersonAlias.Person.Guid));
                lPerson.Text = string.Format("<a href='{0}'>{1}</a>", personUrl, pledge.PersonAlias.Person.FullName);

                lAccount.Text = pledge.Account.Name;
                lStartDate.Text = pledge.StartDate.ToShortDateString();
                lEndDate.Text = pledge.EndDate.ToShortDateString();
                lPledgeAmount.Text = pledge.TotalAmount.FormatAsCurrency();

                var oneTimeGiftAmount = pledge.GetAttributeValue("OneTimeGift").AsDecimal();
                lOneTimeGiftAmount.Text = oneTimeGiftAmount > 0 ? oneTimeGiftAmount.FormatAsCurrency() : "&nbsp;";

                pnlView.Visible = true;
            }
        }

        private void LoadPledgeEdit()
        {
            pnlEdit.Visible = false;
            pnlView.Visible = false;

            ClearFields();

            int pledgeId = hfPledgeId.Value.AsInteger();

            using (var pledgeContext = new RockContext())
            {
                var pledgeService = new FinancialPledgeService(pledgeContext);
                var pledge = pledgeService.Queryable().AsNoTracking()
                    .Include(p => p.PersonAlias.Person)
                    .Include(p => p.Account)
                    .Where(p => p.Id == pledgeId)
                    .SingleOrDefault();

                if (pledge == null)
                {
                    SetDefaults();
                }
                else
                {
                    pledge.LoadAttributes(pledgeContext);
                    pPerson.SetValue(pledge.PersonAlias.Person);
                    pPledgeAccount.SetValue(pledge.AccountId);
                    dpStartDate.SelectedDate = pledge.StartDate;
                    dpEndDate.SelectedDate = pledge.EndDate;
                    tbPledgeAmount.Text = string.Format("{0:0.00}", pledge.TotalAmount);

                    var oneTimeGift = pledge.GetAttributeValue("OneTimeGift").AsDecimal();

                    if (oneTimeGift > 0)
                    {
                        tbOneTimeGiftAmount.Text = string.Format("{0:0.00}", oneTimeGift);
                    }

                    hfSource.Value = pledge.GetAttributeValue("Source");
                }
                lbSaveAndAdd.Visible = pledgeId <= 0;
                pnlEdit.Visible = true;
            }
        }

        private void SetDefaults()
        {
            var accountGuid = GetAttributeValue(AttributeKey.PrimaryAccount).AsGuid();
            var account = new FinancialAccountService(new RockContext()).Get(accountGuid);

            if (account != null)
            {
                pPledgeAccount.SetValue(account);
            }

            var startDate = GetAttributeValue(AttributeKey.StartDate).AsDateTime();
            dpStartDate.SelectedDate = startDate;

            var endDate = GetAttributeValue(AttributeKey.EndDate).AsDateTime();
            dpEndDate.SelectedDate = endDate;

            hfSource.Value = GetAttributeValue(AttributeKey.PledgeSource);

        }

        private int SavePledge()
        {
            var pledgeId = hfPledgeId.Value.AsIntegerOrNull();

            using (var pledgeContext = new RockContext())
            {
                FinancialPledge pledge = null;
                var pledgeService = new FinancialPledgeService(pledgeContext);
                if (pledgeId.HasValue)
                {
                    pledge = pledgeService.Get(pledgeId.Value);
                }
                else
                {
                    pledge = new FinancialPledge();
                    pledgeService.Add(pledge);
                }

                pledge.PersonAliasId = pPerson.PersonAliasId;
                pledge.AccountId = pPledgeAccount.SelectedValuesAsInt().First();
                pledge.StartDate = dpStartDate.SelectedDate.Value;
                pledge.EndDate = dpEndDate.SelectedDate.Value;
                pledge.TotalAmount = tbPledgeAmount.Value.Value;
                pledge.PledgeFrequencyValueId = DefinedValueCache.GetId(Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid());

                pledgeContext.SaveChanges();

                pledge.LoadAttributes(pledgeContext);
                pledge.SetAttributeValue("OneTimeGift", tbOneTimeGiftAmount.Value);
                pledge.SetAttributeValue("Source", hfSource.Value);
                pledge.SaveAttributeValues(pledgeContext);

                SendCommunication(pledge.Id);

                return pledge.Id;
            }
        }

        private void SendCommunication(int pledgeId)
        {
            if (!GetAttributeValue(AttributeKey.SendConfirmation).AsBoolean())
            {
                // do not send the confirmation email;
                return;
            }
            if (pledgeId <= 0)
            {
                return;
            }

            var englishConfirmationGuid = GetAttributeValue(AttributeKey.EnglishConfirmation).AsGuid();
            var spanishConfirmationGuid = GetAttributeValue(AttributeKey.SpanishConfirmation).AsGuid();



            using (var pledgeContext = new RockContext())
            {
                var pledgeService = new FinancialPledgeService(pledgeContext);

                var person = pledgeService.Queryable().AsNoTracking()
                    .Include(p => p.PersonAlias.Person)
                    .Where(p => p.Id == pledgeId)
                    .Select(p => p.PersonAlias.Person)
                    .FirstOrDefault();

                if (person == null)
                {
                    return;
                }

                var campusLanguage = CampusCache.Get(person.PrimaryCampusId ?? 3).GetAttributeValue("Language") == "1" ? "English" : "Spanish";

                var commService = new SystemCommunicationService(pledgeContext);
                SystemCommunication communication = null;
                if (campusLanguage.Equals("Spanish", StringComparison.InvariantCultureIgnoreCase))
                {
                    communication = commService.Get(spanishConfirmationGuid);
                }
                else
                {
                    communication = commService.Get(englishConfirmationGuid);
                }

                if (communication != null)
                {
                    var emailMessage = new RockEmailMessage(communication.Guid);
                    emailMessage.AddRecipient(new RockEmailMessageRecipient(person, new Dictionary<string, object>()));
                    emailMessage.AppRoot = ResolveRockUrl("~/");
                    emailMessage.ThemeRoot = ResolveRockUrl("~~/");
                    emailMessage.Send();
                }
            }

        }

        private void SetNotificationBox(string title, string message, NotificationBoxType nbType = NotificationBoxType.Info)
        {
            nbMain.Visible = message.IsNotNullOrWhiteSpace();
            nbMain.Title = title;
            nbMain.Text = message;
        }


        #endregion
    }
}