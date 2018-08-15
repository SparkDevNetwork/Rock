using System;
using System.Collections.Generic;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace org.newpointe.ExpiredCards.Data
{

    /// <summary>
    /// Summary description for CustomJob
    /// </summary>
    /// 
    [WorkflowTypeField("Workflow", "The Workflow to launch", false, true, "", "General", 2, "WGuid")]
    public class ExpiredCardsJob : IJob
    {

        public void Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            string wGuidStr = dataMap.GetString("WGuid");
            Guid wGuid = new Guid(wGuidStr);
            var rockContext = new RockContext();


            // Get all financial payment detail rows in the database where there is a CC expiration date
            var q =
                rockContext.Database.SqlQuery<CardEntity>(
                    @"SELECT ExpirationMonthEncrypted, ExpirationYearEncrypted, AccountNumberMasked, pa.Guid as PersonGuid
                    FROM [FinancialScheduledTransaction] fst
                    JOIN [FinancialPaymentDetail] fpd ON fst.FinancialPaymentDetailId = fpd.Id
                    JOIN PersonAlias pa ON fst.AuthorizedPersonAliasId = pa.Id
                    WHERE ExpirationMonthEncrypted IS NOT NULL AND fst.IsActive = 1
                    ORDER BY fpd.Id DESC");

            // Get the current month and year 
            DateTime now = DateTime.Now;

            int month = now.Month;
            int year = now.Year;

            int i = 0;


            foreach (CardEntity fpd in q) // Loop through all the payment details
            {

                int expirationMonthDecrypted = Int32.Parse(Encryption.DecryptString(fpd.ExpirationMonthEncrypted));
                int expirationYearDecrypted = Int32.Parse(Encryption.DecryptString(fpd.ExpirationYearEncrypted));
                Guid personId = fpd.PersonGuid;
                string personIdString = personId.ToString();
                string acctNum = fpd.AccountNumberMasked.Substring(fpd.AccountNumberMasked.Length - 4);

                int warningYear = expirationYearDecrypted;
                int warningMonth = expirationMonthDecrypted - 1;
                if (warningMonth == 0)
                {
                    warningYear -= 1;
                    warningMonth = 12;
                }

                string warningDate = warningMonth.ToString() + warningYear.ToString();
                string currentMonthString = month.ToString() + year.ToString();

                if (warningDate == currentMonthString)
                {
                    // Start workflow for this person
                    Dictionary<string, string> attr = new Dictionary<string, string>();
                    attr.Add("Person", personIdString);
                    attr.Add("Card", acctNum);
                    LaunchWorkflow(wGuid, attr);
                }

                i++;
            }

            context.Result = i + " Expiring Card workflows were processed.";
        }



        protected void LaunchWorkflow(Guid workflowGuid, Dictionary<string, string> attributes)
        {

            RockContext _rockContext = new RockContext();
            WorkflowService _workflowService = new WorkflowService(_rockContext);
            WorkflowTypeService _workflowTypeService = new WorkflowTypeService(_rockContext);
            WorkflowType _workflowType = _workflowTypeService.Get(workflowGuid);

            Workflow _workflow = Rock.Model.Workflow.Activate(_workflowType, "New Test" + _workflowType.WorkTerm);


            foreach (KeyValuePair<string, string> attribute in attributes)
            {
                _workflow.SetAttributeValue(attribute.Key, attribute.Value);
            }


            List<string> errorMessages;
            
            if ( _workflowService.Process( _workflow, out errorMessages ) )
            {
                // If the workflow type is persisted, save the workflow
                if (_workflow.IsPersisted || _workflowType.IsPersisted)
                {
                    if (_workflow.Id == 0)
                    {
                        _workflowService.Add(_workflow);
                    }

                    _rockContext.WrapTransaction(() =>
                    {
                        _rockContext.SaveChanges();
                        _workflow.SaveAttributeValues(_rockContext);
                        foreach (var activity in _workflow.Activities)
                        {
                            activity.SaveAttributeValues(_rockContext);
                        }

                    });

                }
            }
        }


    }

}

class CardEntity
{
    public string ExpirationMonthEncrypted { get; set; }
    public string ExpirationYearEncrypted { get; set; }
    public string AccountNumberMasked { get; set; }
    public int AuthorizedPersonAliasId { get; set; }
    public Guid PersonGuid { get; set; }
}