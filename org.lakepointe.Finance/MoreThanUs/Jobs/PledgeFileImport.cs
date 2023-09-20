using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using CsvHelper;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using CsvHelper.Configuration;

namespace org.lakepointe.Finance.MoreThanUs.Jobs
{

    [AccountField("Pledge Financial Account",
        Description = "The Financial Account that incoming pledges should be assigned to.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.FinancialAccount)]

    [DateField("Pledge Start Date",
        Description = "The start date for the pledge.",
        IsRequired = true,
        Order = 1,
        Key = AttributeKey.StartDate)]

    [DateField("Pledge End Date",
        Description = "The end date for the pledge.",
        IsRequired = true,
        Order = 2,
        Key = AttributeKey.EndDate)]

    [TextField("Pledge Source",
        Description = "Where the pledge entry originated from.",
        IsRequired = false,
        DefaultValue = "Online",
        Order = 3,
        Key = AttributeKey.PledgeSource)]

    [WorkflowTypeField("Pledge Import File Worflow Type",
        Description = "The Workflow Type that is triggered from a pledge file being imported.",
        IsRequired =  true,
        Order = 4,
        Key = AttributeKey.ImportFileWorkflow)]

    [DisplayName("Process More Than Us Pledge File")]
    [Description("Processes Staged More Than Us .csv Online Pledge Forms ")]

    [DisallowConcurrentExecution]
    public class PledgeFileImport : IJob
    {
        public static class AttributeKey
        {
            public const string FinancialAccount = "FinancialAccount";
            public const string StartDate = "StartDate";
            public const string EndDate = "EndDate";
            public const string PledgeSource = "PledgeSource";
            public const string ImportFileWorkflow = "ImportfileWorkflow";
        }
        public void Execute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;
            var pledgeConfiguration = new PledgeFileConfiguration();


            var financialAccountGuid = dataMap.GetString(AttributeKey.FinancialAccount).AsGuid();
            var importFileWorkflowGuid = dataMap.GetString(AttributeKey.ImportFileWorkflow).AsGuid();
            pledgeConfiguration.StartDate = dataMap.GetString(AttributeKey.StartDate).AsDateTime().Value;
            pledgeConfiguration.EndDate = dataMap.GetString(AttributeKey.EndDate).AsDateTime().Value;
            pledgeConfiguration.PledgeSource = dataMap.GetString(AttributeKey.PledgeSource);

            var account = new FinancialAccountService(new RockContext()).Get(financialAccountGuid);
            var workflowType = WorkflowTypeCache.Get(importFileWorkflowGuid);

            if (workflowType == null)
            {
                throw new Exception("Workflow Type Not Found.");
            }


            if (account == null)
            {
                throw new Exception("Financial Account Not Found.");
            }

            pledgeConfiguration.FinancialAccountId = account.Id;

            var fileWorkflows = new WorkflowService(new RockContext()).Queryable().AsNoTracking()
                .Where(w => w.WorkflowTypeId == workflowType.Id)
                .Where(w => !w.CompletedDateTime.HasValue)
                .Where(w => w.Status.Equals("Pending", StringComparison.InvariantCultureIgnoreCase))
                .ToList();


            foreach (var file in fileWorkflows)
            {
                ProcessFile(file, pledgeConfiguration);
            }

        }

        private void CompleteWorkflow(int workflowId, bool isSuccess, string message, int? processedRows = null, int? importedPledges = null, int? totalRows = null)
        {
            var successfulImportActivityGuid = "6c67ab20-c2c9-40be-9987-1655962fa81c".AsGuid();
            var errorImportActivityGuid = "584957cd-f867-4494-bbfd-19e20c9ea973".AsGuid();
            using (var workflowContext = new RockContext())
            {
                var workflowService = new WorkflowService(workflowContext);
                var workflow = workflowService.Get(workflowId);

                workflow.LoadAttributes(workflowContext);
                workflow.SetAttributeValue("TotalRows", totalRows);
                workflow.SetAttributeValue("RowsProcessed", processedRows);
                workflow.SetAttributeValue("PledgesImported", importedPledges);
                workflow.SetAttributeValue("StatusMessage", message.Trim());

                workflow.SaveAttributeValues(workflowContext);

                WorkflowActivityTypeCache activityType;
                if (isSuccess)
                {
                    activityType = WorkflowActivityTypeCache.Get(successfulImportActivityGuid, workflowContext);
                }
                else
                {
                    activityType = WorkflowActivityTypeCache.Get(errorImportActivityGuid, workflowContext);
                }

                WorkflowActivity.Activate(activityType, workflow, workflowContext);

                workflowContext.SaveChanges();
                var errorMesssages = new List<string>();
                workflowService.Process(workflow, out errorMesssages);
            }
        }

        private void ProcessFile(Workflow w, PledgeFileConfiguration config)
        {
            int? totalRows = 0;
            int? processedRows = 0;
            int? importedPledges = 0;
            try
            {
                w.LoadAttributes();
                var binaryFileGuid = w.GetAttributeValue("DataFile").AsGuid();

                var dataFile = new BinaryFileService(new RockContext()).Get(binaryFileGuid);

                if (dataFile == null)
                {
                    CompleteWorkflow(w.Id, false, "Data File not found.", processedRows, importedPledges, totalRows);
                    return;
                }

                var rawPledges = new List<PledgeImportItem>();
                using (var reader = new StreamReader(dataFile.ContentStream))
                {
                    using (var csv = new CsvHelper.CsvReader(reader))
                    {
                        csv.Configuration.Delimiter = ",";
                        csv.Configuration.HasHeaderRecord = true;
                        csv.Configuration.Quote = '\"';
                        csv.Configuration.TrimFields = true;
                        csv.Configuration.WillThrowOnMissingField = false;
                        csv.Configuration.RegisterClassMap(new PledgeImportMap());

                        rawPledges = csv.GetRecords<PledgeImportItem>().ToList();
                    }

                }

                foreach (var item in rawPledges)
                {
                    bool newPledge = false;
                    var pledgeId = MoreThanUs.FinancialPledge.Import(item, config, out newPledge);

                    if (newPledge)
                    {
                        importedPledges++;
                    }


                    processedRows++;
                }

                CompleteWorkflow(w.Id, true, string.Empty, processedRows, importedPledges, totalRows);
            }
            catch (Exception ex)
            {
                CompleteWorkflow(w.Id, false, ex.Message, processedRows, importedPledges, totalRows);
            }

        }

        protected class PledgeImportMap : CsvClassMap<PledgeImportItem>
        {
            public PledgeImportMap()
            {
                AutoMap();
                Map(m => m.RowId).Ignore();
                Map(m => m.SubmissionDate).Name("Submission Date").ConvertUsing(row => row.GetField("Submission Date").AsDateTime());
                Map(m => m.OneTimeGift).Name("My one-time gift today").ConvertUsing(row => row.GetField("My one-time gift today").AsDecimal());
                Map(m => m.PledgeTotal).Name("In addition to the above gift, over the next 3 years I will commit to give").ConvertUsing(row => row.GetField("In addition to the above gift, over the next 3 years I will commit to give").AsDecimal());
                Map(m => m.FirstName).Name("First Name");
                Map(m => m.LastName).Name("Last Name");
                Map(m => m.Email).Name("Email Address");
                Map(m => m.PhoneNumber).Name("PhoneNumber");
            }
        }
    }
}
