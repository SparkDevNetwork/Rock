using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using OfficeOpenXml;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;
using System.Data.Entity;

namespace rocks.pillars.Jobs.Jobs
{
    /// <summary>
    /// Job that executes CSharp code.
    /// </summary>
    [TextField("Batch Name", "The name that should be used for the batches created", true, "SecureGive Import", order: 0)]
    [IntegerField("Anonymous Giver PersonAliasID", "PersonAliasId to use in case of anonymous giver", true, order: 1)]
    [DefinedValueField(Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE, "Default Transaction Source", "The default transaction source to use if a match is not found (Website, Kiosk, etc.).", true, order: 2)]
    [DefinedValueField(Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE, "Default Tender Type Value", "The default tender type if a match is not found (Cash, Credit Card, etc.).", true, order: 3)]
    [DefinedValueField(Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Default Connection Status", "The default connection status to set for new people", true, false, Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_MEMBER, "", 4)]
    [TextField("Fund Code Mapping",
        "Held in the Subsplash's fund_name field, these correspond to Rock's Account IDs (integer). Each FundCode should be mapped to a matching AccountId otherwise Rock will just use the same value. Delimit them with commas or semicolons, and write them in the format 'SecureGive_value=Rock_value'.",
        false,
        "General Tithes/Offerings=5,Building=7,Global Outreach=6,Benevolent=8",
        "Data Mapping",
        3)]
    [TextField("Campus Code Mapping",
        "Held in the Subsplash's Cause_Name field, these correspond to Rock's Campus IDs (integer). Each Cause Name should be mapped to a matching CampusId otherwise Rock will just use the first Campus. Delimit them with commas or semicolons, and write them in the format 'SecureGive_value=Rock_value'.",
        false,
        "Mesa Campus=1,Gilbert Campus=2,Queen Creek=3,Glendale Campus=4,Tempe Campus=6,Online Campus=7",
        "Data Mapping",
        4)]
    [TextField("Batch Name", "The name of the batch we are importing", true, "", "", 5)]
    [FileField(Rock.SystemGuid.BinaryFiletype.DEFAULT, "Transaction File", "This is the file of transactions that we can import", true, "", "", 6)]
    [DataViewField("No Email Update", 
        "This is a data view of the people you want to not have their email address updated. Like staff members who would use their personal email for giving and would want their organization email as their email on rock",
        false,
        "",
        "Rock.Model.Person",
        "",
        7)]

    [DisallowConcurrentExecution]
    public class SubplashTransactionImport : IJob
    {
        #region Fields

        private int _anonymousPersonAliasId = 0;
        private BinaryFile _txnFile;
        private FinancialBatch _financialBatch;
        private JobDataMap _dataMap;

        public List<string> _errors = new List<string>();

        private Dictionary<int, FinancialAccount> _financialAccountCache = new Dictionary<int, FinancialAccount>();

        private Dictionary<string, int> _accountMappings;
        private Dictionary<string, int> _campusCodeMappings;
        private Dictionary<string, FinancialAccount> _accounts;
        private List<int> _noEmailUpdatePeople;

        #endregion

        public SubplashTransactionImport()
        {
            _noEmailUpdatePeople = new List<int>();
        }

        public void Execute(IJobExecutionContext context)
        {
            _dataMap = context.JobDetail.JobDataMap;
            _anonymousPersonAliasId = _dataMap.GetIntegerFromString("AnonymousGiverPersonAliasID");
            var rockContext = new RockContext();

            if (_anonymousPersonAliasId == 0)
            {
                context.UpdateLastStatusMessage("There is no Anonymous Giver Person Alias Id designated.");
                return;
            }

            var file = _dataMap.GetString("TransactionFile").AsGuidOrNull();

            if (file.HasValue)
            {
                var fileService = new BinaryFileService(rockContext);
                _txnFile = fileService.Get(file.Value);

                //Set Dictionaries and lists for transaction processing
                _accountMappings = _dataMap.GetString("FundCodeMapping").Split(',')
                    .Select(f => f.Split('='))
                    .ToDictionary(m => m[0], m => m[1].AsInteger());

                _campusCodeMappings = _dataMap.GetString("CampusCodeMapping").Split(',')
                    .Select(f => f.Split('='))
                    .ToDictionary(m => m[0], m => m[1].AsInteger());

                _accounts = LoadAccounts();

                if (_txnFile.FileName.EndsWith("xlsx"))
                {
                    ProcessFile(rockContext);

                    if(_errors.Count > 0)
                    {
                        context.UpdateLastStatusMessage(string.Join("\n", _errors));
                    }
                    else
                    {
                        context.UpdateLastStatusMessage("All transactions have been imported");
                    }
                }
                else
                {
                    context.UpdateLastStatusMessage("The Transaction file must be a xlsx file.");
                    return;
                }
            }
            else
            {
                context.UpdateLastStatusMessage("There is no Transaction File uploaded to import.");
                return;
            }
        }


        private Dictionary<string, FinancialAccount> LoadAccounts()
        {
            var accountDict = new Dictionary<string, FinancialAccount>();

            if (_accountMappings != null && _campusCodeMappings != null)
            {
                var rockContext = new RockContext();
                var accountService = new FinancialAccountService(rockContext);
                var accountIds = _accountMappings.Values.ToList();

                var accounts = accountService.Queryable()
                    .Where(fa => fa.ParentAccountId.HasValue && accountIds.Contains(fa.ParentAccountId.Value))
                    .ToList();

                foreach (var item in accounts)
                {
                    var ackey = _accountMappings.FirstOrDefault(m => m.Value == item.ParentAccountId).Key;
                    var cmKey = _campusCodeMappings.FirstOrDefault(c => c.Value == item.CampusId).Key;

                    var newKey = string.Format("{0}.{1}", ackey, cmKey);

                    if (accountDict.ContainsKey(newKey) == false)
                    {
                        accountDict.Add(newKey, item);
                    }
                }
            }

            return accountDict;
        }


        private void ProcessFile(RockContext rockContext)
        {
            FinancialBatchService financialBatchService = new FinancialBatchService(rockContext);
            DataViewService dataViewService = new DataViewService(rockContext);
            FinancialTransactionService ftService;

            var transactions = GetTransactionInfo();

            if (transactions != null)
            {
                _financialBatch = new FinancialBatch();
                _financialBatch.Name = _dataMap.GetString("BatchName");
                _financialBatch.BatchStartDateTime = Rock.RockDateTime.Now;

                financialBatchService.Add(_financialBatch);
                rockContext.SaveChanges();

                var dataViewGuid = _dataMap.GetString("NoEmailUpdate").AsGuidOrNull();
                if (dataViewGuid.HasValue)
                {
                    var dataView = dataViewService.Get(dataViewGuid.Value);
                    if(dataView != null)
                    {
                        var dvArgs = new DataViewGetQueryArgs
                        {
                            DbContext = rockContext,
                            DatabaseTimeoutSeconds = 35
                        };

                        _noEmailUpdatePeople = dataView.GetQuery(dvArgs).AsNoTracking()
                            .OfType<Person>()
                            .Select(p => p.Id)
                            .ToList();
                    }
                }

                int batchId = _financialBatch.Id;

                List<FinancialTransaction> rockTransactions = new List<FinancialTransaction>();
                int count = 2;
                int saveCount = 1;
                foreach (var t in transactions)
                {
                    try
                    {
                        var txn = CreateTransaction(t);
                        if(txn == null)
                        {
                            _errors.Add(string.Format("Error On Row Number {0}: Could not find matching account {1} for {2} Campus", count, t.fund, t.cause));
                            count++;

                            continue;
                        }

                        txn.BatchId = batchId;

                        rockTransactions.Add(txn);

                        if(saveCount >= 100)
                        {
                            saveCount = 0;
                            ftService = new FinancialTransactionService(rockContext);
                            ftService.AddRange(rockTransactions);
                            rockContext.SaveChanges(true);

                            rockContext = new RockContext();
                            rockTransactions = new List<FinancialTransaction>();
                        }
                    }
                    catch (Exception ex)
                    {
                        _errors.Add(string.Format("Error On Row# {0}: {1}", count.ToString(), ex.Message));
                    }
                    count++;
                    saveCount++;
                }

                ftService = new FinancialTransactionService(rockContext);
                ftService.AddRange(rockTransactions);
                rockContext.SaveChanges(true);
            }
        }

        private List<SubsplashInfo> GetTransactionInfo()
        {
            DataTable dt = null;
            List<SubsplashInfo> transactions = new List<SubsplashInfo>();

            if (_txnFile.FileName.EndsWith("xlsx"))
            {
                using (MemoryStream ms = new MemoryStream(_txnFile.DatabaseData.Content))
                using (ExcelPackage pack = new ExcelPackage(ms))
                {
                    ExcelWorksheet workSheet = pack.Workbook.Worksheets.First();
                    dt = new DataTable();
                    foreach (var firstRowCell in workSheet.Cells[1, 1, 1, workSheet.Dimension.End.Column])
                    {
                        dt.Columns.Add(firstRowCell.Text);
                    }

                    for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
                    {
                        var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];
                        var newRow = dt.NewRow();
                        foreach (var cell in row)
                        {
                            newRow[cell.Start.Column - 1] = cell.Text;
                        }
                        dt.Rows.Add(newRow);
                    }

                    foreach (DataRow r in dt.Rows)
                    {
                        if (r.ItemArray.All(o => o.ToString().Trim().IsNotNullOrWhiteSpace()) == false)
                        {
                            transactions.Add(new SubsplashInfo(r));
                        }
                    }
                }
            }

            return transactions;
        }

        private FinancialTransaction CreateTransaction(SubsplashInfo info)
        {
            var transaction = new FinancialTransaction();

            var contributionTypeId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid()).Id;
            var transSourceId = DefinedValueCache.Get(_dataMap.GetString("DefaultTransactionSource").AsGuid()).Id;
            var tranTypeId = DefinedValueCache.Get(_dataMap.GetString("DefaultTenderTypeValue").AsGuid()).Id;
            var achTypeId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH.AsGuid()).Id;

            transaction.ProcessedDateTime = info.date.AsDateTime() ?? RockDateTime.Today;
            transaction.TransactionDateTime = info.date.AsDateTime() ?? RockDateTime.Today;
            transaction.TransactionTypeValueId = contributionTypeId;
            transaction.SourceTypeValueId = transSourceId;
            transaction.TransactionCode = info.transaction_id;

            var account = ReturnAccount(string.Format("{0}.{1}", info.fund, info.cause));

            if(account == null)
            {
                return null;
            }

            var personAliasId = GetPersonAliasId(info, account.CampusId);

            transaction.FinancialPaymentDetail = new FinancialPaymentDetail
            {
                CurrencyTypeValueId = info.status.Contains("ach") ? achTypeId : tranTypeId
            };

            transaction.AuthorizedPersonAliasId = personAliasId != 0 ? personAliasId : _anonymousPersonAliasId;

            var financialTransactionDetail = new FinancialTransactionDetail
            {
                AccountId = account.Id,
                Amount = info.give_amount.AsDecimal(),
                Summary = info.donor_memo
            };

            transaction.TransactionDetails = new List<FinancialTransactionDetail> { financialTransactionDetail };

            return transaction;
        }

        private FinancialAccount ReturnAccount(string key)
        {
            FinancialAccount account;

            _accounts.TryGetValue(key, out account);

            return account;
        }

        private int GetPersonAliasId(SubsplashInfo info, int? campusId)
        {
            var rockContext = new RockContext();
            int personAliasId = 0;

            var homeLocationTypeId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME).Id;
            var homeLocation = CreateHomeLocation(info);

            Person person = null;
            var personService = new PersonService(rockContext);

            var personQuery = new PersonService.PersonMatchQuery(info.first_name, info.last_name, info.email, info.phone, null, null, null, null);
            person = personService.FindPerson(personQuery, false);

            if (person != null)
            {
                personAliasId = person.PrimaryAlias.Id;

                if(_noEmailUpdatePeople.Contains(person.Id) == false)
                {
                    person.Email = info.email; //Only update if they are not in the no update email people
                }

                //var family = person.GetFamily();
                //var prevHomeLoc = family.GroupLocations.Where(gl => gl.GroupLocationTypeValueId == homeLocationTypeId).FirstOrDefault();

                //if(prevHomeLoc == null)
                //{
                //    family.GroupLocations.Add(homeLocation);
                //}
                //else
                //{
                //    var groupLocationService = new GroupLocationService(rockContext);
                //    groupLocationService.Delete(prevHomeLoc);

                //    family.GroupLocations.Add(homeLocation);
                //}
            }
            else
            {
                // Add New Person
                person = new Person();
                person.FirstName = info.first_name.FixCase();
                person.LastName = info.last_name.FixCase();
                person.IsEmailActive = true;
                person.Email = info.email;
                person.EmailPreference = EmailPreference.EmailAllowed;
                person.RecordTypeValueId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid()).Id;

                UpdatePhoneNumber(person, info.phone);

                var defaultConnectionStatus = DefinedValueCache.Get(_dataMap.GetString("DefaultConnectionStatus").AsGuid());
                if (defaultConnectionStatus != null)
                {
                    person.ConnectionStatusValueId = defaultConnectionStatus.Id;
                }

                var defaultRecordStatus = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE);
                if (defaultRecordStatus != null)
                {
                    person.RecordStatusValueId = defaultRecordStatus.Id;
                }

                var familyGroup = PersonService.SaveNewPerson(person, rockContext, campusId, false);
                if (familyGroup != null && familyGroup.Members.Any())
                {
                    familyGroup.GroupLocations.Add(homeLocation);
                    person = familyGroup.Members.Select(m => m.Person).First();
                    personAliasId = person.PrimaryAlias.Id;
                }
            }

            rockContext.SaveChanges();
            return personAliasId;
        }

        private GroupLocation CreateHomeLocation(SubsplashInfo info)
        {
            var location = new Location
            {
                Street1 = info.address_line_1,
                Street2 = info.address_line_2,
                City = info.city,
                State = info.state,
                PostalCode = info.postal_code
            };

            GroupLocation homeLocation = new GroupLocation
            {
                GroupLocationTypeValueId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid()).Id,
                Location = location
            };

            return homeLocation;
        }

        void UpdatePhoneNumber(Person person, string mobileNumber)
        {
            if (!string.IsNullOrWhiteSpace(PhoneNumber.CleanNumber(mobileNumber)))
            {
                var phoneNumberType = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid());
                if (phoneNumberType == null)
                {
                    return;
                }

                var phoneNumber = person.PhoneNumbers.FirstOrDefault(n => n.NumberTypeValueId == phoneNumberType.Id);
                string oldPhoneNumber = string.Empty;
                if (phoneNumber == null)
                {
                    phoneNumber = new PhoneNumber { NumberTypeValueId = phoneNumberType.Id };
                    person.PhoneNumbers.Add(phoneNumber);
                }
                else
                {
                    oldPhoneNumber = phoneNumber.NumberFormattedWithCountryCode;
                }

                // TODO handle country code here
                phoneNumber.Number = PhoneNumber.CleanNumber(mobileNumber);
            }
        }

        #region Helper Classes

        public class SubsplashInfo
        {
            public string date { get; set; }
            public string time { get; set; }
            public string time_zone { get; set; }
            public string transaction_id { get; set; }
            public string transfer_id { get; set; }
            public string transfer_date { get; set; }
            public string transfer_net { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string email { get; set; }
            public string address_line_1 { get; set; }
            public string address_line_2 { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public string postal_code { get; set; }
            public string phone { get; set; }
            public string give_amount { get; set; }
            public string net_amount { get; set; }
            public string fee_amount { get; set; }
            public string covered_fee { get; set; }
            public string status { get; set; }
            public string member_id { get; set; }
            public string campus_id { get; set; }
            public string fund { get; set; }
            public string fund_id { get; set; }
            public string cause { get; set; }
            public string cause_id { get; set; }
            public string refund_amount { get; set; }
            public string donor_memo { get; set; }

            //public string transaction_id { get; set; }
            //public string transaction_date { get; set; }
            //public string transfer_id { get; set; }
            //public string transfer_date { get; set; }
            //public string transfer_amount { get; set; }
            //public string donor_id { get; set; }
            //public string donor_first_name { get; set; }
            //public string donor_last_name { get; set; }
            //public string donor_email { get; set; }
            //public string gross_amount { get; set; }
            //public string net_amount { get; set; }
            //public string fee_amount { get; set; }
            //public string covered_fee { get; set; }
            //public string fund_id { get; set; }
            //public string fund { get; set; }
            //public string cause_id { get; set; }
            //public string cause { get; set; }
            //public string type { get; set; }
            //public string campus { get; set; }
            //public string donor_memo { get; set; }

            public SubsplashInfo()
            {
            }

            public SubsplashInfo(DataRow dataRow)
            {
                if (dataRow["date"].ToString().Trim() != null)
                {
                    date = dataRow["date"].ToString().Trim();
                }

                //if (dataRow["time"].ToString().Trim() != null)
                //{
                //    time = dataRow["time"].ToString().Trim();
                //}

                //if (dataRow["time_zone"].ToString().Trim() != null)
                //{
                //    time_zone = dataRow["time_zone"].ToString().Trim();
                //}

                if (dataRow["transaction_id"].ToString().Trim() != null)
                {
                    transaction_id = dataRow["transaction_id"].ToString().Trim();
                }

                //if (dataRow["transaction_date"].ToString().Trim() != null)
                //{
                //    transaction_date = dataRow["transaction_date"].ToString().Trim();
                //}

                if (dataRow["transfer_id"].ToString().Trim() != null)
                {
                    transfer_id = dataRow["transfer_id"].ToString().Trim();
                }

                if (dataRow["transfer_date"].ToString().Trim() != null)
                {
                    transfer_date = dataRow["transfer_date"].ToString().Trim();
                }

                if (dataRow["transfer_net"].ToString().Trim() != null)
                {
                    transfer_net = dataRow["transfer_net"].ToString().Trim();
                }

                if (dataRow["first_name"].ToString().Trim() != null)
                {
                    first_name = dataRow["first_name"].ToString().Trim();
                }

                if (dataRow["last_name"].ToString().Trim() != null)
                {
                    last_name = dataRow["last_name"].ToString().Trim();
                }

                if (dataRow["email"].ToString().Trim() != null)
                {
                    email = dataRow["email"].ToString().Trim();
                }

                if (dataRow["address_line_1"].ToString().Trim() != null)
                {
                    address_line_1 = dataRow["address_line_1"].ToString().Trim();
                }

                if (dataRow["address_line_2"].ToString().Trim() != null)
                {
                    address_line_2 = dataRow["address_line_2"].ToString().Trim();
                }

                if (dataRow["city"].ToString().Trim() != null)
                {
                    city = dataRow["city"].ToString().Trim();
                }

                if (dataRow["state"].ToString().Trim() != null)
                {
                    state = dataRow["state"].ToString().Trim();
                }

                if (dataRow["postal_code"].ToString().Trim() != null)
                {
                    postal_code = dataRow["postal_code"].ToString().Trim();
                }

                if (dataRow["phone"].ToString().Trim() != null)
                {
                    phone = dataRow["phone"].ToString().Trim();
                }

                if (dataRow["give_amount"].ToString().Trim() != null)
                {
                    give_amount = dataRow["give_amount"].ToString().Trim();
                }

                //if (dataRow["transfer_amount"].ToString().Trim() != null)
                //{
                //    transfer_amount = dataRow["transfer_amount"].ToString().Trim();
                //}

                //if (dataRow["donor_id"].ToString().Trim() != null)
                //{
                //    donor_id = dataRow["donor_id"].ToString().Trim();
                //}

                //if (dataRow["donor_first_name"].ToString().Trim() != null)
                //{
                //    donor_first_name = dataRow["donor_first_name"].ToString().Trim();
                //}

                //if (dataRow["donor_last_name"].ToString().Trim() != null)
                //{
                //    donor_last_name = dataRow["donor_last_name"].ToString().Trim();
                //}

                //if (dataRow["donor_email"].ToString().Trim() != null)
                //{
                //    donor_email = dataRow["donor_email"].ToString().Trim();
                //}

                //if (dataRow["gross_amount"].ToString().Trim() != null)
                //{
                //    gross_amount = dataRow["gross_amount"].ToString().Trim();
                //}

                if (dataRow["net_amount"].ToString().Trim() != null)
                {
                    net_amount = dataRow["net_amount"].ToString().Trim();
                }

                if (dataRow["fee_amount"].ToString().Trim() != null)
                {
                    fee_amount = dataRow["fee_amount"].ToString().Trim();
                }

                if (dataRow["covered_fee"].ToString().Trim() != null)
                {
                    covered_fee = dataRow["covered_fee"].ToString().Trim();
                }

                if (dataRow["status"].ToString().Trim() != null)
                {
                    status = dataRow["status"].ToString().Trim();
                }

                if (dataRow["member_id"].ToString().Trim() != null)
                {
                    member_id = dataRow["member_id"].ToString().Trim();
                }

                if (dataRow["campus_id"].ToString().Trim() != null)
                {
                    campus_id = dataRow["campus_id"].ToString().Trim();
                }

                if (dataRow["fund"].ToString().Trim() != null)
                {
                    fund = dataRow["fund"].ToString().Trim();
                }

                if (dataRow["fund_id"].ToString().Trim() != null)
                {
                    fund_id = dataRow["fund_id"].ToString().Trim();
                }

                if (dataRow["cause"].ToString().Trim() != null)
                {
                    cause = dataRow["cause"].ToString().Trim();
                }

                if (dataRow["cause_id"].ToString().Trim() != null)
                {
                    cause_id = dataRow["cause_id"].ToString().Trim();
                }

                if (dataRow["refund_amount"].ToString().Trim() != null)
                {
                    refund_amount = dataRow["refund_amount"].ToString().Trim();
                }

                if (dataRow["donor_memo"].ToString().Trim() != null)
                {
                    donor_memo = dataRow["donor_memo"].ToString().Trim();
                }

                //if (dataRow["campus"].ToString().Trim() != null)
                //{
                //    campus = dataRow["campus"].ToString().Trim();
                //}

                //if (dataRow["type"].ToString().Trim() != null)
                //{
                //    type = dataRow["type"].ToString().Trim();
                //}

                //if (dataRow["donor_email"].ToString().Trim() != null)
                //{
                //    donor_email = dataRow["donor_email"].ToString().Trim();
                //}
            }
        }

        #endregion
    }
}
