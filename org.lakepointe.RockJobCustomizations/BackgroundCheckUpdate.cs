using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace org.lakepointe.RockJobCustomizations
{
    [DataViewField("Background Check Data View",
        description: "The dataview that contains the list of people who has an active backgroudund check.",
        required: true,
        order: 0,
        key: "BackgroundCheckDataview")]

    [DisallowConcurrentExecution]
    public class BackgroundCheckUpdate : IJob
    {
        #region Fields
        private const string BACKGROUND_CHECKED_ATTRIBUTE_KEY = "BackgroundChecked";
        private const string BACKGROUND_CHECK_DATE_ATTRIBUTE_KEY = "BackgroundCheckDate";
        private const string BACKGROUND_CHECK_RESULT_ATTRIBUTE_KEY = "BackgroundCheckResult";
        private const string BACKGROUND_CHECK_EXPIRE_ATTRIBUTE_KEY = "BackgroundCheckExpireDate";
        private const string BACKGROUND_CHECK_PASS_VALUE = "Pass";
        private const string BACKGROUND_CHECK_FAIL_VALUE = "Fail";

        private const string SCREEN_FORM_CLEAR_ATTRIBUTE_KEY = "Arena-29-265";
        private const string SCREEN_FORM_EXPIRE_ATTRIBUTE_KEY = "Arena-29-279";
        #endregion

        public void Execute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;
            var dataViewGuid = dataMap.GetString("BackgroundCheckDataview").AsGuid();
            var recordsProcessed = 0;

            var rockContext = new RockContext();

            var dataview = new DataViewService(rockContext).Get(dataViewGuid);

            var errorMessages = new List<string>();
            var personService = new PersonService(rockContext);
            var paramExpression = personService.ParameterExpression;
            var whereExpression = dataview.GetExpression(personService, paramExpression);

            var bgcPersonIds = personService.Queryable(false, false).AsNoTracking()
                .Where(paramExpression, whereExpression, null)
                .Select(p => p.Id)
                .ToList();

            foreach (var personId in bgcPersonIds)
            {
                using (var personContext = new RockContext())
                {
                    var person = new PersonService(personContext).Get(personId);
                    person.LoadAttributes(personContext);

                    var clearDate = person.GetAttributeValue(SCREEN_FORM_CLEAR_ATTRIBUTE_KEY).AsDateTime();
                    var expireDate = person.GetAttributeValue(SCREEN_FORM_EXPIRE_ATTRIBUTE_KEY).AsDateTime();

                    if (expireDate > RockDateTime.Now)
                    {

                        person.SetAttributeValue(BACKGROUND_CHECK_DATE_ATTRIBUTE_KEY, clearDate);
                        person.SaveAttributeValue(BACKGROUND_CHECK_DATE_ATTRIBUTE_KEY, personContext);

                        person.SetAttributeValue(BACKGROUND_CHECK_EXPIRE_ATTRIBUTE_KEY, expireDate);
                        person.SaveAttributeValue(BACKGROUND_CHECK_EXPIRE_ATTRIBUTE_KEY, personContext);

                        person.SetAttributeValue(BACKGROUND_CHECK_RESULT_ATTRIBUTE_KEY, BACKGROUND_CHECK_PASS_VALUE);
                        person.SaveAttributeValue(BACKGROUND_CHECK_RESULT_ATTRIBUTE_KEY, personContext);

                        person.SetAttributeValue(BACKGROUND_CHECKED_ATTRIBUTE_KEY, bool.TrueString);
                        person.SaveAttributeValue(BACKGROUND_CHECKED_ATTRIBUTE_KEY, personContext);

                    }
                    recordsProcessed += 1;

                    if (recordsProcessed % 100 == 0)
                    {
                        UpdateStatusMessage(recordsProcessed, context);
                    }

                }
            }

            UpdateStatusMessage(recordsProcessed, context);
        }


        private void UpdateStatusMessage(int count, IJobExecutionContext context)
        {
            var message = string.Format("{0} records processed.", count);
            context.UpdateLastStatusMessage(message);
            
        }
    }
}
