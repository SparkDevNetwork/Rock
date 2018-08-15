<%@ WebHandler Language="C#" Class="RockWeb.Webhooks.Shape" %>
// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System;
using System.Web;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Communication;

namespace RockWeb.Webhooks
{

    public class Shape : IHttpHandler
    {

        private int transactionCount = 0;
        private RockContext rockContext = new RockContext();

        private string TopGift1;
        private string TopGift2;
        private string TopGift3;
        private string TopGift4;
        private string LowestGift;

        private string TopAbility1;
        private string TopAbility2;
        private string LowestAbility;

        private string HeartCategories;
        private string HeartCauses;
        private string HeartPassion;

        private string Email;
        private string FirstName;
        private string LastName;

        private Person ThePerson;

        private string FormId;

        private string People;
        private string Places;
        private string Events;

        private int? CampusId;



        public void ProcessRequest(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            response.ContentType = "text/plain";

            if (request.HttpMethod != "POST")
            {
                response.Write("Invalid request type.");
                return;
            }

            if (request.Form["FormId"].IsNullOrWhiteSpace())
            {
                response.Write("Invalid request type.");
                return;
            }

            // Get personal fields out of POST data
            Email = request.Form["Email"];
            FirstName = request.Form["FirstName"];
            LastName = request.Form["LastName"];
            FormId = request.Form["EntryNumber"] + "-" + request.Form["FormId"] + "-" + request.Form["Email"];
            People = request.Form["People"];
            Places = request.Form["Places"];
            Events = request.Form["Events"];

            // Heart fields
            string heartCategories = request.Form["HeartCategories"];
            IList<Guid> guidArrayList = new List<Guid>();
            if (!heartCategories.IsNullOrWhiteSpace())
            {
                heartCategories = heartCategories.Remove(0, 2);
                DefinedValueService definedValueService = new DefinedValueService(rockContext);
                heartCategories = heartCategories.Replace("\n", " ");
                string[] heartCategoryArray = heartCategories.Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string category in heartCategoryArray)
                {
                    var firstOrDefault = definedValueService
                        .Queryable()
                        .FirstOrDefault(a => a.Value == category && a.DefinedType.Name == "SHAPE Heart");
                    if (firstOrDefault != null)
                    {
                        Guid categoryGuid =
                            firstOrDefault
                                .Guid;
                        guidArrayList.Add(categoryGuid);
                    }
                }
                if (guidArrayList != null)
                {
                    HeartCategories = string.Join(",", guidArrayList.ToArray());
                }

            }
            else
            {
                HeartCategories = "";
            }

            HeartCauses = request.Form["HeartCauses"];
            HeartPassion = request.Form["HeartPassion"];



            // Format text boxes nicely
            People = People.Replace(System.Environment.NewLine, "<br />");
            Places = Places.Replace(System.Environment.NewLine, "<br />");
            Events = Events.Replace(System.Environment.NewLine, "<br />");

            string campusFromForm = request.Form["campus"];
            var campusList = CampusCache.All();
            var selectedCampus = campusList.AsQueryable().FirstOrDefault(a => a.Name == campusFromForm);
            CampusId = selectedCampus.Id;


            // Get the person based on the form (or make a new one)
            var person = GetPerson(rockContext);
            ThePerson = person;


            // Build dictionary of <GiftId> <TotalScore>
            Dictionary<string, int> GiftDictionary = new Dictionary<string, int>();
            Dictionary<string, int> AbilityDictionary = new Dictionary<string, int>();


            // Go through Post Data and add up scores for each Gift type
            foreach (string x in request.Params.Keys)
            {
                if (x.Length == 8 && x.Contains("-") && x.StartsWith("S"))
                {
                    string gift = Int32.Parse(x.Substring(5, 3)).ToString();
                    int answer = 0;

                    switch (request.Params[x])
                    {
                        case "Never":
                            answer = 1;
                            break;

                        case "Almost Never":
                            answer = 2;
                            break;

                        case "Sometimes":
                            answer = 3;
                            break;

                        case "Almost Always":
                            answer = 4;
                            break;

                        case "Always":
                            answer = 5;
                            break;
                    }

                    int value;
                    if (GiftDictionary.TryGetValue(gift, out value))
                    {
                        GiftDictionary[gift] = value + answer;
                    }
                    else
                    {
                        GiftDictionary.Add(gift, answer);
                    }


                }

            }


            // Go through Post Data and add up scores for each Ability and Abilities type
            foreach (string x in request.Params.Keys)
            {
                if (x.Length == 8 && x.Contains("-") && x.StartsWith("A"))
                {
                    string ability = Int32.Parse(x.Substring(5, 3)).ToString();

                    int answer = 0;

                    switch (request.Params[x])
                    {
                        case "Never":
                            answer = 1;
                            break;

                        case "Almost Never":
                            answer = 2;
                            break;

                        case "Sometimes":
                            answer = 3;
                            break;

                        case "Almost Always":
                            answer = 4;
                            break;

                        case "Always":
                            answer = 5;
                            break;
                    }
                    int value;
                    if (AbilityDictionary.TryGetValue(ability, out value))
                    {
                        AbilityDictionary[ability] = value + answer;
                    }
                    else
                    {
                        AbilityDictionary.Add(ability, answer);
                    }



                }

            }


            // Make a SortedDictionary to sort highest scores descending (yay for avoiding sorting algorithm!)
            var sortedGiftDictionary = from entry in GiftDictionary orderby entry.Value descending select entry;
            var sortedAbilityDictionary = from entry in AbilityDictionary orderby entry.Value descending select entry;

            // Set highest and lowest gifts
            TopGift1 = sortedGiftDictionary.ElementAt(0).Key;
            TopGift2 = sortedGiftDictionary.ElementAt(1).Key;
            TopGift3 = sortedGiftDictionary.ElementAt(2).Key;
            TopGift4 = sortedGiftDictionary.ElementAt(3).Key;
            LowestGift = sortedGiftDictionary.Last().Key;
            TopAbility1 = sortedAbilityDictionary.ElementAt(0).Key;
            TopAbility2 = sortedAbilityDictionary.ElementAt(1).Key;
            LowestAbility = sortedAbilityDictionary.Last().Key;



            // Save the attributes
            SaveAttributes(Int32.Parse(TopGift1), Int32.Parse(TopGift2), Int32.Parse(TopGift3), Int32.Parse(TopGift4), Int32.Parse(TopAbility1), Int32.Parse(TopAbility2), People, Places, Events, HeartCategories, HeartCauses, HeartPassion, context);


            // Send a confirmation email describing the gifts and how to get back to them
            SendEmail(person.Email, "info@newpointe.org", "SHAPE Assessment Results", GenerateEmailBody(ThePerson, rockContext, Int32.Parse(TopGift1), Int32.Parse(TopGift2), Int32.Parse(TopAbility1), Int32.Parse(TopAbility2), FormId), rockContext);

            // Email the campus pastor if the person has the gift of Leadership as #1
            CampusPastor(Int32.Parse(TopGift1), campusFromForm, person);

            // Write a 200 code in the response
            response.ContentType = "text/xml";
            response.AddHeader("Content-Type", "text/xml");
            response.StatusCode = 200;



        }


        /// <summary>
        /// Email the campus pastor if the person's #1 gift is leadership
        /// </summary>
        /// <param name="Gift1">Int of category of Gift1</param>
        /// <param name="campus">String of the person's campus name</param>
        /// <param name="personWithLeadership">Person object with the target person</param>
        /// <returns></returns>
        public void CampusPastor(int Gift1, string campus, Person personWithLeadership)
        {
            DefinedValueService definedValueService = new DefinedValueService(rockContext);

            string gift1Value = definedValueService.GetByIds(new List<int> {Gift1}).FirstOrDefault().Value;

            if (gift1Value == "Leadership")
            {
                // Get the Header and Footer
                string emailHeader = Rock.Web.Cache.GlobalAttributesCache.Value("EmailHeader");
                string emailFooter = Rock.Web.Cache.GlobalAttributesCache.Value("EmailFooter");

                CampusService campusService = new CampusService(rockContext);
                var campusPastor = campusService.Queryable().FirstOrDefault(a => a.Name == campus).LeaderPersonAlias;
                string emailBody = emailHeader + String.Format(@"<h2>Hey {0},</h2>
                                     <p>{1} from your campus has taken the SHAPE Assessment and their top gift is Leadership.
                                     <a href=""https://rock.newpointe.org/Person/{2}"">Click here</a> to see their Rock profile, SHAPE Profile, and be sure to follow up!</p>
                                     <p>Thanks,<br />The RockBot</p>", campusPastor.Person.NickName,personWithLeadership.FullName,personWithLeadership.Id) + emailFooter;

                SendEmail(campusPastor.Person.Email, "info@newpointe.org", "SHAPE Assessment Results", emailBody, rockContext);

            }
        }

        /// <summary>
        /// Write the 2 highest gift attributes on the person's record.
        /// </summary>
        /// <param name="Gift1">Int of category of Gift1</param>
        /// <param name="Gift2">Int of category of Gift2</param>
        /// <param name="Ability1">Int of category of Ability1</param>
        /// <param name="Ability2">Int of category of Ability2</param>
        /// <returns></returns>
        public void SaveAttributes(int Gift1, int Gift2, int Gift3, int Gift4, int Ability1, int Ability2, string People, string Places, string Events, string HCategories, string HCauses, string HPassion, HttpContext context)
        {

            AttributeService attributeService = new AttributeService(rockContext);
            AttributeValueService attributeValueService = new AttributeValueService(rockContext);
            DefinedValueService definedValueService = new DefinedValueService(rockContext);

            // Get the Ids of the needed attributes from their Key
            var spiritualGift1Attribute = attributeService.Queryable().FirstOrDefault(a => a.Key == "SpiritualGift1");
            var spiritualGift2Attribute = attributeService.Queryable().FirstOrDefault(a => a.Key == "SpiritualGift2");
            var spiritualGift3Attribute = attributeService.Queryable().FirstOrDefault(a => a.Key == "SpiritualGift3");
            var spiritualGift4Attribute = attributeService.Queryable().FirstOrDefault(a => a.Key == "SpiritualGift4");
            var ability1Attribute = attributeService.Queryable().FirstOrDefault(a => a.Key == "Ability1");
            var ability2Attribute = attributeService.Queryable().FirstOrDefault(a => a.Key == "Ability2");
            var spiritualGiftFormAttribute = attributeService.Queryable().FirstOrDefault(a => a.Key == "SpiritualGiftForm");
            var peopleAttribute = attributeService.Queryable().FirstOrDefault(a => a.Key == "SHAPEPeople");
            var placesAttribute = attributeService.Queryable().FirstOrDefault(a => a.Key == "SHAPEPlaces");
            var eventsAttribute = attributeService.Queryable().FirstOrDefault(a => a.Key == "SHAPEEvents");
            var formAttribute = attributeService.Queryable().FirstOrDefault(a => a.Key == "SpiritualGiftForm");
            var heartCategoriesAttribute = attributeService.Queryable().FirstOrDefault(a => a.Key == "HeartCategories");
            var heartCausesAttribute = attributeService.Queryable().FirstOrDefault(a => a.Key == "HeartCauses");
            var heartPassionAttribute = attributeService.Queryable().FirstOrDefault(a => a.Key == "HeartPassion");

            // Get an AttributeValue object from the attribute Id and person Id (to see if it is already saved)
            AttributeValue spiritualGiftAttributeValue1 = attributeValueService.GetByAttributeIdAndEntityId(spiritualGift1Attribute.Id, ThePerson.Id);
            AttributeValue spiritualGiftAttributeValue2 = attributeValueService.GetByAttributeIdAndEntityId(spiritualGift2Attribute.Id, ThePerson.Id);
            AttributeValue spiritualGiftAttributeValue3 = attributeValueService.GetByAttributeIdAndEntityId(spiritualGift3Attribute.Id, ThePerson.Id);
            AttributeValue spiritualGiftAttributeValue4 = attributeValueService.GetByAttributeIdAndEntityId(spiritualGift4Attribute.Id, ThePerson.Id);
            AttributeValue abilityAttributeValue1 = attributeValueService.GetByAttributeIdAndEntityId(ability1Attribute.Id, ThePerson.Id);
            AttributeValue abilityAttributeValue2 = attributeValueService.GetByAttributeIdAndEntityId(ability2Attribute.Id, ThePerson.Id);
            AttributeValue peopleAttributeValue =  attributeValueService.GetByAttributeIdAndEntityId(peopleAttribute.Id, ThePerson.Id);
            AttributeValue placesAttributeValue =  attributeValueService.GetByAttributeIdAndEntityId(placesAttribute.Id, ThePerson.Id);
            AttributeValue eventsAttributeValue =  attributeValueService.GetByAttributeIdAndEntityId(eventsAttribute.Id, ThePerson.Id);
            AttributeValue formAttributeValue = attributeValueService.GetByAttributeIdAndEntityId(formAttribute.Id, ThePerson.Id);
            AttributeValue heartCategoriesAttributeValue = attributeValueService.GetByAttributeIdAndEntityId(heartCategoriesAttribute.Id, ThePerson.Id);
            AttributeValue heartCausesAttributeValue = attributeValueService.GetByAttributeIdAndEntityId(heartCausesAttribute.Id, ThePerson.Id);
            AttributeValue heartPassionAttributeValue = attributeValueService.GetByAttributeIdAndEntityId(heartPassionAttribute.Id, ThePerson.Id);

            // Convert the Defined Value Id to a Guid for each attribute to save
            Guid gift1Guid = definedValueService.GetByIds(new List<int> {Gift1}).FirstOrDefault().Guid;
            Guid gift2Guid = definedValueService.GetByIds(new List<int> {Gift2}).FirstOrDefault().Guid;
            Guid gift3Guid = definedValueService.GetByIds(new List<int> {Gift3}).FirstOrDefault().Guid;
            Guid gift4Guid = definedValueService.GetByIds(new List<int> {Gift4}).FirstOrDefault().Guid;
            Guid ability1Guid = definedValueService.GetByIds(new List<int> {Ability1}).FirstOrDefault().Guid;
            Guid ability2Guid = definedValueService.GetByIds(new List<int> {Ability2}).FirstOrDefault().Guid;


            if (spiritualGiftAttributeValue1 == null)
            {
                spiritualGiftAttributeValue1 = new AttributeValue();
                spiritualGiftAttributeValue1.AttributeId = spiritualGift1Attribute.Id;
                spiritualGiftAttributeValue1.EntityId = ThePerson.Id;
                spiritualGiftAttributeValue1.Value = gift1Guid.ToString();
                attributeValueService.Add(spiritualGiftAttributeValue1);
            }
            else
            {
                spiritualGiftAttributeValue1.AttributeId = spiritualGift1Attribute.Id;
                spiritualGiftAttributeValue1.EntityId = ThePerson.Id;
                spiritualGiftAttributeValue1.Value = gift1Guid.ToString();
            }



            if (spiritualGiftAttributeValue2 == null)
            {
                spiritualGiftAttributeValue2 = new AttributeValue();
                spiritualGiftAttributeValue2.AttributeId = spiritualGift2Attribute.Id;
                spiritualGiftAttributeValue2.EntityId = ThePerson.Id;
                spiritualGiftAttributeValue2.Value = gift2Guid.ToString();
                attributeValueService.Add(spiritualGiftAttributeValue2);
            }
            else
            {
                spiritualGiftAttributeValue2.AttributeId = spiritualGift2Attribute.Id;
                spiritualGiftAttributeValue2.EntityId = ThePerson.Id;
                spiritualGiftAttributeValue2.Value = gift2Guid.ToString();
            }

            if (spiritualGiftAttributeValue3 == null)
            {
                spiritualGiftAttributeValue3 = new AttributeValue();
                spiritualGiftAttributeValue3.AttributeId = spiritualGift3Attribute.Id;
                spiritualGiftAttributeValue3.EntityId = ThePerson.Id;
                spiritualGiftAttributeValue3.Value = gift3Guid.ToString();
                attributeValueService.Add(spiritualGiftAttributeValue3);
            }
            else
            {
                spiritualGiftAttributeValue3.AttributeId = spiritualGift3Attribute.Id;
                spiritualGiftAttributeValue3.EntityId = ThePerson.Id;
                spiritualGiftAttributeValue3.Value = gift3Guid.ToString();
            }


            if (spiritualGiftAttributeValue4 == null)
            {
                spiritualGiftAttributeValue4 = new AttributeValue();
                spiritualGiftAttributeValue4.AttributeId = spiritualGift4Attribute.Id;
                spiritualGiftAttributeValue4.EntityId = ThePerson.Id;
                spiritualGiftAttributeValue4.Value = gift4Guid.ToString();
                attributeValueService.Add(spiritualGiftAttributeValue4);
            }
            else
            {
                spiritualGiftAttributeValue4.AttributeId = spiritualGift4Attribute.Id;
                spiritualGiftAttributeValue4.EntityId = ThePerson.Id;
                spiritualGiftAttributeValue4.Value = gift4Guid.ToString();
            }


            if (abilityAttributeValue1 == null)
            {
                abilityAttributeValue1 = new AttributeValue();
                abilityAttributeValue1.AttributeId = ability1Attribute.Id;
                abilityAttributeValue1.EntityId = ThePerson.Id;
                abilityAttributeValue1.Value = ability1Guid.ToString();
                attributeValueService.Add(abilityAttributeValue1);
            }
            else
            {
                abilityAttributeValue1.AttributeId = ability1Attribute.Id;
                abilityAttributeValue1.EntityId = ThePerson.Id;
                abilityAttributeValue1.Value = ability1Guid.ToString();
            }

            if (abilityAttributeValue2 == null)
            {
                abilityAttributeValue2 = new AttributeValue();
                abilityAttributeValue2.AttributeId = ability2Attribute.Id;
                abilityAttributeValue2.EntityId = ThePerson.Id;
                abilityAttributeValue2.Value = ability2Guid.ToString();
                attributeValueService.Add(abilityAttributeValue2);
            }
            else
            {
                abilityAttributeValue2.AttributeId = ability2Attribute.Id;
                abilityAttributeValue2.EntityId = ThePerson.Id;
                abilityAttributeValue2.Value = ability2Guid.ToString();
            }


            if (peopleAttributeValue == null)
            {
                peopleAttributeValue = new AttributeValue();
                peopleAttributeValue.AttributeId = peopleAttribute.Id;
                peopleAttributeValue.EntityId = ThePerson.Id;
                peopleAttributeValue.Value = People;
                attributeValueService.Add(peopleAttributeValue);
            }
            else
            {
                peopleAttributeValue.AttributeId = peopleAttribute.Id;
                peopleAttributeValue.EntityId = ThePerson.Id;
                peopleAttributeValue.Value = People;
            }


            if (placesAttributeValue == null)
            {
                placesAttributeValue = new AttributeValue();
                placesAttributeValue.AttributeId = placesAttribute.Id;
                placesAttributeValue.EntityId = ThePerson.Id;
                placesAttributeValue.Value = Places;
                attributeValueService.Add(placesAttributeValue);
            }
            else
            {
                placesAttributeValue.AttributeId = placesAttribute.Id;
                placesAttributeValue.EntityId = ThePerson.Id;
                placesAttributeValue.Value = Places;
            }



            if (eventsAttributeValue == null)
            {
                eventsAttributeValue = new AttributeValue();
                eventsAttributeValue.AttributeId = eventsAttribute.Id;
                eventsAttributeValue.EntityId = ThePerson.Id;
                eventsAttributeValue.Value = Events;
                attributeValueService.Add(eventsAttributeValue);
            }
            else
            {
                eventsAttributeValue.AttributeId = eventsAttribute.Id;
                eventsAttributeValue.EntityId = ThePerson.Id;
                eventsAttributeValue.Value = Events;
            }



            if (heartCategoriesAttributeValue == null)
            {
                heartCategoriesAttributeValue = new AttributeValue();
                heartCategoriesAttributeValue.AttributeId = heartCategoriesAttribute.Id;
                heartCategoriesAttributeValue.EntityId = ThePerson.Id;
                heartCategoriesAttributeValue.Value = HCategories;
                attributeValueService.Add(heartCategoriesAttributeValue);
            }
            else
            {
                heartCategoriesAttributeValue.AttributeId = heartCategoriesAttribute.Id;
                heartCategoriesAttributeValue.EntityId = ThePerson.Id;
                heartCategoriesAttributeValue.Value = HCategories;
            }


            if (heartCausesAttributeValue == null)
            {
                heartCausesAttributeValue = new AttributeValue();
                heartCausesAttributeValue.AttributeId = heartCausesAttribute.Id;
                heartCausesAttributeValue.EntityId = ThePerson.Id;
                heartCausesAttributeValue.Value = HCauses;
                attributeValueService.Add(heartCausesAttributeValue);
            }
            else
            {
                heartCausesAttributeValue.AttributeId = heartCausesAttribute.Id;
                heartCausesAttributeValue.EntityId = ThePerson.Id;
                heartCausesAttributeValue.Value = HCauses;
            }



            if (heartPassionAttributeValue == null)
            {
                heartPassionAttributeValue = new AttributeValue();
                heartPassionAttributeValue.AttributeId = heartPassionAttribute.Id;
                heartPassionAttributeValue.EntityId = ThePerson.Id;
                heartPassionAttributeValue.Value = HPassion;
                attributeValueService.Add(heartPassionAttributeValue);
            }
            else
            {
                heartPassionAttributeValue.AttributeId = heartPassionAttribute.Id;
                heartPassionAttributeValue.EntityId = ThePerson.Id;
                heartPassionAttributeValue.Value = HPassion;
            }




            if (formAttributeValue == null)
            {
                formAttributeValue = new AttributeValue();
                formAttributeValue.AttributeId = spiritualGiftFormAttribute.Id;
                formAttributeValue.EntityId = ThePerson.Id;
                formAttributeValue.Value = FormId;
                attributeValueService.Add(formAttributeValue);
            }
            else
            {
                formAttributeValue.AttributeId = formAttribute.Id;
                formAttributeValue.EntityId = ThePerson.Id;
                formAttributeValue.Value = FormId;
            }



            try
            {
                rockContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                ExceptionLogService.LogException( ex, context );
            }


        }



        /// <summary>
        /// Gets the person from form data, or creates a new person if one doesn't exist
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Person GetPerson(RockContext rockContext)
        {
            var personService = new PersonService(rockContext);

            var personMatches = personService.GetByEmail(Email)
                .Where(p =>
                   p.LastName.Equals(LastName, StringComparison.OrdinalIgnoreCase) &&
                   ((p.FirstName != null && p.FirstName.Equals(FirstName, StringComparison.OrdinalIgnoreCase)) ||
                       (p.NickName != null && p.NickName.Equals(FirstName, StringComparison.OrdinalIgnoreCase))))
                .ToList();
            if (personMatches.Count() >= 1)
            {
                return personMatches.FirstOrDefault();
            }
            else
            {
                DefinedValueCache dvcConnectionStatus = DefinedValueCache.Read("368DD475-242C-49C4-A42C-7278BE690CC2");
                DefinedValueCache dvcRecordStatus = DefinedValueCache.Read("283999EC-7346-42E3-B807-BCE9B2BABB49");

                Person person = new Person();
                person.FirstName = FirstName;
                person.LastName = LastName;
                person.Email = Email;
                person.IsEmailActive = true;
                person.EmailPreference = EmailPreference.EmailAllowed;
                person.RecordTypeValueId = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid()).Id;
                if (dvcConnectionStatus != null)
                {
                    person.ConnectionStatusValueId = dvcConnectionStatus.Id;
                }

                if (dvcRecordStatus != null)
                {
                    person.RecordStatusValueId = dvcRecordStatus.Id;
                }

                PersonService.SaveNewPerson(person, rockContext, CampusId, false);

                return personService.Get(person.Id);
            }
        }


        public bool IsReusable
        {
            get
            {
                return false;
            }
        }


        private void SendEmail(string recipient, string from, string subject, string body, RockContext rockContext)
        {
            var recipients = new List<string>();
            recipients.Add(recipient);

            var mediumData = new Dictionary<string, string>();
            mediumData.Add("From", from);
            mediumData.Add("Subject", subject);
            mediumData.Add("Body", body);

            var mediumEntity = EntityTypeCache.Read(Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid(), rockContext);
            if (mediumEntity != null)
            {
                var medium = MediumContainer.GetComponent(mediumEntity.Name);
                if (medium != null && medium.IsActive)
                {
                    var transport = medium.Transport;
                    if (transport != null && transport.IsActive)
                    {
                        var appRoot = GlobalAttributesCache.Read(rockContext).GetValue("InternalApplicationRoot");
                        transport.Send(mediumData, recipients, appRoot, string.Empty);
                    }
                }
            }
        }



        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }


        private static string GenerateEmailBody(Person person, RockContext rockContext, int Gift1, int Gift2, int Ability1, int Ability2, string profileValue)
        {
            // Get the Header and Footer
            string emailHeader = Rock.Web.Cache.GlobalAttributesCache.Value("EmailHeader");
            string emailFooter = Rock.Web.Cache.GlobalAttributesCache.Value("EmailFooter");


            // Get all of the data about the assiciated gifts and ability categories
            DefinedValueService definedValueService = new DefinedValueService(rockContext);

            var shapeGift1Object = definedValueService.GetListByIds(new List<int> { Gift1 }).FirstOrDefault();
            var shapeGift2Object = definedValueService.GetListByIds(new List<int> { Gift2 }).FirstOrDefault();
            var ability1Object = definedValueService.GetListByIds(new List<int> { Ability1 }).FirstOrDefault();
            var ability2Object = definedValueService.GetListByIds(new List<int> { Ability2 }).FirstOrDefault();

            shapeGift1Object.LoadAttributes();
            shapeGift2Object.LoadAttributes();
            ability1Object.LoadAttributes();
            ability2Object.LoadAttributes();

            string profileLink = "http://newpointe.org/SHAPEProfile/" + profileValue;

            string myNewPointeMessage;
            if (person.Users.Count > 0)
            {
                myNewPointeMessage = @"You can view your full SHAPE Profile any time from your 
                                    <a href=""https://newpointe.org/MyAccount"">MyNewPointe Account</a>";
            }
            else
            {
                myNewPointeMessage = String.Format(@"You can view your full SHAPE Profile, plus manage your personal information, 
                                     give online, and much more via a MyNewPointe account.
                                     <a href=""{0}"">Click here</a> to create a MyNewPointe Account via your SHAPE Profile now!", profileLink);
            }



            //Put the body text together
            string emailBody = emailHeader + String.Format(@"
            <h2><span style=""color:#8bc540"">{0}, Here Are Your SHAPE Results!</span></h2>
            <p>Your SHAPE Assessment is complete and a sneak peek of the details are below.  <strong><a href=""{1}"">Click here</a> to view your full SHAPE Profile</strong></p>
            <h3><u>Spiritual Gifts</u></h3>
            <h4>{2}</h4>
            {3}<br /><br />
            <h4>{4}</h4>
            {5}<br /><br /><br />
            <h3><u>Abilities</u></h3>
            <h4>{6}</h4>
            {7}<br /><br />
            <h4>{8}</h4>
            {9}<br /><br /><br />
            <p>{10}</p>
            ", person.NickName, profileLink, shapeGift1Object.Value, shapeGift1Object.Description, shapeGift2Object.Value, shapeGift2Object.Description,
            ability1Object.Value, ability1Object.Description, ability2Object.Value, ability2Object.Description, myNewPointeMessage) + emailFooter;


            return emailBody;
        }



    }


}