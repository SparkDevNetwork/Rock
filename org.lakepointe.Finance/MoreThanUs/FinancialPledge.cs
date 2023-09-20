using org.lakepointe.Finance.MoreThanUs.Jobs;
using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;

using DotLiquid;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace org.lakepointe.Finance.MoreThanUs
{
    public class FinancialPledge
    {
        public int Id { get; set; }
        public Rock.Model.FinancialPledge Pledge { get; set; }
        public string Source { get; set; }
        public decimal? OneTimeGift { get; set; }


        public static int? Import(PledgeImportItem item, PledgeFileConfiguration config, out bool pledgeAdded)
        {
            pledgeAdded = false;
            int? personPrimaryAliasId = null;
            Person person = null;

            if (item.FirstName.IsNotNullOrWhiteSpace() && item.FirstName.IsNotNullOrWhiteSpace())
            {
                person = GetPerson(item.FirstName, item.LastName, item.PhoneNumber, item.Email, out personPrimaryAliasId);
            }
            if (person == null)
            {
                pledgeAdded = false;
                return null;
            }

            var pledge = new MoreThanUs.FinancialPledge();
            pledge.Get(person.Id, item, config);

            if (pledge.Id > 0)
            {
                return pledge.Id;
            }

            var corePledge = new Rock.Model.FinancialPledge()
            {
                PersonAliasId = personPrimaryAliasId.Value,
                AccountId = config.FinancialAccountId,
                TotalAmount = item.PledgeTotal,
                StartDate = config.StartDate,
                EndDate = config.EndDate,
                CreatedDateTime = item.SubmissionDate,
                PledgeFrequencyValueId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid()).Id
            };

            pledge.SaveNewPledge(corePledge, item.OneTimeGift, config.PledgeSource);

            if (pledge.Id <= 0)
            {
                return null;
            }

            pledgeAdded = true;

            pledge.SendConfirmationCommunication();

            return pledge.Id;

        }

        public void Get(int personId, PledgeImportItem item, PledgeFileConfiguration config)
        {
            using (var pledgeContext = new RockContext())
            {
                var pledge = new FinancialPledgeService(pledgeContext).Queryable().AsNoTracking()
                    .Where(p => p.PersonAlias.PersonId == personId)
                    .Where(p => p.AccountId == config.FinancialAccountId)
                    .Where(p => p.TotalAmount == item.PledgeTotal)
                    .Where(p => p.StartDate == config.StartDate)
                    .Where(p => p.EndDate == config.EndDate)
                    .Where(p => p.CreatedDateTime == item.SubmissionDate.Value)
                    .FirstOrDefault();

                if (pledge != null)
                {
                    pledge.LoadAttributes(pledgeContext);

                    this.Id = pledge.Id;
                    this.Pledge = pledge;
                    this.Source = pledge.GetAttributeValue("Source");
                    this.OneTimeGift = pledge.GetAttributeValue("OneTimeGift").AsDecimalOrNull();

                }
            }

        }

        public void SaveNewPledge(Rock.Model.FinancialPledge pledge, decimal? oneTimeGift, string source)
        {
            using (var pledgeContext = new RockContext())
            {
                var pledgeService = new FinancialPledgeService(pledgeContext);
                pledgeService.Add(pledge);
                pledgeContext.SaveChanges();

                pledge.LoadAttributes(pledgeContext);
                if (oneTimeGift.HasValue)
                {
                    pledge.SetAttributeValue("OneTimeGift", oneTimeGift.Value);
                    pledge.SaveAttributeValue("OneTimeGift", pledgeContext);
                }

                if (source.IsNotNullOrWhiteSpace())
                {
                    pledge.SetAttributeValue("Source", source.Trim());
                    pledge.SaveAttributeValue("Source", pledgeContext);
                }

                this.Id = pledge.Id;
                this.Pledge = pledge;
                this.OneTimeGift = pledge.GetAttributeValue("OneTimeGift").AsDecimalOrNull();
                this.Source = pledge.GetAttributeValue("Source");


            }
        }

        private static Person GetPerson(string firstName, string lastName, string phoneNumber, string email, out int? primaryAliasId)
        {
            using (var personContext = new RockContext())
            {

                var personService = new PersonService(personContext);
                var personQuery = new PersonService.PersonMatchQuery(firstName, lastName, email, PhoneNumber.CleanNumber(phoneNumber));
                var person = personService.FindPerson(personQuery, true);
                primaryAliasId = null;
                if (person != null)
                {
                    primaryAliasId = person.PrimaryAliasId;
                    return person;
                }

                person = new Person();
                person.FirstName = firstName.FixCase();
                person.LastName = lastName.FixCase();
                person.IsEmailActive = true;
                person.Email = email;
                person.EmailPreference = EmailPreference.EmailAllowed;
                person.RecordTypeValueId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid()).Id;
                person.ConnectionStatusValueId = DefinedValueCache.Get("76b06690-3109-44e1-b415-1dc82a84dc0a".AsGuid()).Id;  //New From Web
                person.RecordStatusValueId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid()).Id;

                UpdatePhoneNumber(person, phoneNumber);

                var defaultCampusId = CampusCache.All(false).OrderBy(c => c.Order).Select(c => c.Id).FirstOrDefault();

                var familyGroup = PersonService.SaveNewPerson(person, personContext, defaultCampusId);

                if (familyGroup != null && familyGroup.Members.Any())
                {
                    person = familyGroup.Members.Select(m => m.Person).First();
                    primaryAliasId = person.PrimaryAliasId;
                    return person;
                }
                else
                {
                    return null;
                }
            }
        }

        private void SendConfirmationCommunication( )
        {
            var englishConfirmationEmailGuid = "9878fdd1-efc8-4077-810c-984bc846f1b2".AsGuid();
            var spanishConfirmationEmailGuid = "85d4371e-a816-466a-ac4c-61ad76c013a3".AsGuid();

            var person = new PersonAliasService(new RockContext()).GetPerson(Pledge.PersonAliasId.Value);
            var mergeFields = new Dictionary<string, object>();
            var messageRecipients = new List<RockMessageRecipient>();

            messageRecipients.Add(new RockEmailMessageRecipient(person, mergeFields));

            var campus = person.GetCampus();
            campus.LoadAttributes();
            var languageId = campus.GetAttributeValue("Language").AsInteger();

            Guid messageGuid = Guid.Empty;
            switch (languageId)
            {
                case 1:
                    messageGuid = englishConfirmationEmailGuid;
                    break;
                case 2:
                    messageGuid = spanishConfirmationEmailGuid;
                    break;
                default:
                    messageGuid = englishConfirmationEmailGuid;
                    break;
            }

            var emailMessage = new RockEmailMessage(messageGuid);
            emailMessage.SetRecipients(messageRecipients);
            emailMessage.Send();
            

        }

        private static void UpdatePhoneNumber(Person person, string mobileNumber)
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
    }
}