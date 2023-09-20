using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace rocks.pillars.Jobs
{
    [DisplayName("Auto Activate Person Records")]
    [Description("This job looks through all the pending people in Rock and will automatically set their record status to active if they are either not in the \"Duplicate Finder\" list at all, or they have a confidence score less than the score specified (default is 40 which is represents a low chance of the person being a duplicate).")]

    [IntegerField("Confidence Score", "The maximum confidence score to still be auto-activate people at. When a new record is created, it is checked against other records to determine if the new record" +
        "is a potential duplicate. A higher confidence score represents a higher chance they are a duplicate. A lower number here will result in fewer people being auto-activated and more needing to be" +
        "manually reviewed. A higher number could potentially result in duplicate records being auto-activated. 40 is the reccomended number.", true, 40, "", 0, "ConfidenceScore")]

    public class AutoActivatePersonRecords : IJob
    {
        public AutoActivatePersonRecords()
        {
        }

        public virtual void Execute(IJobExecutionContext context)
        {
            RockContext rockContext = new RockContext();
            var personDuplicateService = new PersonDuplicateService(rockContext);
            var personService = new PersonService(rockContext);
            var personAliasService = new PersonAliasService(rockContext);

            DefinedValueCache pendingRecordStatus = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid());
            DefinedValueCache personRecordType = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid());

            List<Person> pendingPeople = personService.Queryable().Where(p => p.RecordStatusValueId == pendingRecordStatus.Id && p.RecordTypeValueId == personRecordType.Id).ToList();
            List<int> pendingPeopleIds = pendingPeople.Select(p => p.Id).ToList();
            List<PersonDuplicate> duplicatePeople = personDuplicateService.Queryable().ToList();


            List<string> peopleActivated = new List<string>();

            /* loop through through the list of duplicate people and determine if they are either:
             * a) not in the duplicate list at all
             * b) in the duplicate list with a confidence score less than the specified confidence score
             * if either a or b is true, change the person's record status to active.
            */
            foreach (Person person in pendingPeople)
            {
                bool inDuplicates = (duplicatePeople.Select(dp => dp.PersonAlias.PersonId).ToList().Contains(person.Id)) || (duplicatePeople.Select(dp => dp.DuplicatePersonAlias.PersonId).ToList().Contains(person.Id));
                double? confidenceScore = (duplicatePeople.Where(dp => dp.PersonAlias.PersonId == person.Id).OrderByDescending(dp => dp.ConfidenceScore).FirstOrDefault() ?? duplicatePeople.Where(dp => dp.DuplicatePersonAlias.PersonId == person.Id).OrderByDescending(dp => dp.ConfidenceScore).FirstOrDefault())?.ConfidenceScore;
                bool? lowEnoughConfidenceScore = null;

                if (confidenceScore.HasValue)
                {
                    lowEnoughConfidenceScore = confidenceScore <= Convert.ToDouble(context.JobDetail.JobDataMap.GetString("ConfidenceScore").AsIntegerOrNull() ?? 40);
                }

                if(inDuplicates)
                {
                    if(lowEnoughConfidenceScore.HasValue ? lowEnoughConfidenceScore.Value : false)
                    {
                        person.RecordStatusValueId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid()).Id; //change the record status to active
                        peopleActivated.Add(person.FullName);
                        if (peopleActivated.Count % 50 == 0) //only call this every ~50 changes
                        {
                            rockContext.SaveChanges();
                        }
                    }
                }
                else
                {
                    person.RecordStatusValueId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid()).Id; //change the record status to active
                    peopleActivated.Add(person.FullName);
                    if (peopleActivated.Count % 50 == 0) //only call this every ~50 changes
                    {
                        rockContext.SaveChanges();
                    }
                }
            }

            rockContext.SaveChanges();

            context.UpdateLastStatusMessage(String.Format("Changed {0} records from pending to active{1}", peopleActivated.Count,peopleActivated.Count > 0 ? ":\n" + String.Join("\n",peopleActivated) : "" ));

        }
    }
}
