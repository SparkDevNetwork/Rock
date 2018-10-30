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



        public void ProcessRequest( HttpContext context )
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            response.ContentType = "text/plain";

            if ( request.HttpMethod != "POST" )
            {
                response.TrySkipIisCustomErrors = true;
                response.StatusCode = 405;
                response.Headers.Add( "Allow", "GET" );
                response.Write( "Invalid request method." );
                return;
            }

            if ( request.Form["FormId"].IsNullOrWhiteSpace() )
            {
                response.TrySkipIisCustomErrors = true;
                response.StatusCode = 400;
                response.Write( "A FormId is required." );
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
            if ( !heartCategories.IsNullOrWhiteSpace() )
            {
                heartCategories = heartCategories.Remove( 0, 2 );
                DefinedValueService definedValueService = new DefinedValueService( rockContext );
                heartCategories = heartCategories.Replace( "\n", " " );
                string[] heartCategoryArray = heartCategories.Split( new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries );
                foreach ( string category in heartCategoryArray )
                {
                    var firstOrDefault = definedValueService
                        .Queryable()
                        .FirstOrDefault( a => a.Value == category && a.DefinedType.Name == "SHAPE Heart" );
                    if ( firstOrDefault != null )
                    {
                        Guid categoryGuid =
                            firstOrDefault
                                .Guid;
                        guidArrayList.Add( categoryGuid );
                    }
                }
                if ( guidArrayList != null )
                {
                    HeartCategories = string.Join( ",", guidArrayList.ToArray() );
                }

            }
            else
            {
                HeartCategories = "";
            }

            HeartCauses = request.Form["HeartCauses"];
            HeartPassion = request.Form["HeartPassion"];



            // Format text boxes nicely
            People = DotLiquid.StandardFilters.NewlineToBr(People);
            Places = DotLiquid.StandardFilters.NewlineToBr(Places);
            Events = DotLiquid.StandardFilters.NewlineToBr(Events);

            string campusFromForm = request.Form["campus"];
            var campusList = CampusCache.All();
            var selectedCampus = campusList.AsQueryable().FirstOrDefault( a => a.Name == campusFromForm );
            CampusId = selectedCampus.Id;


            // Get the person based on the form (or make a new one)
            ThePerson = GetPerson( rockContext );


            // Build dictionary of <GiftId> <TotalScore>
            Dictionary<string, int> GiftDictionary = new Dictionary<string, int>();
            Dictionary<string, int> AbilityDictionary = new Dictionary<string, int>();


            // Go through Post Data and add up scores for each Gift type
            foreach ( string x in request.Params.Keys )
            {
                if ( x.Length == 8 && x.Contains( "-" ) && x.StartsWith( "S" ) )
                {
                    string gift = Int32.Parse( x.Substring( 5, 3 ) ).ToString();
                    int answer = 0;

                    switch ( request.Params[x] )
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
                    if ( GiftDictionary.TryGetValue( gift, out value ) )
                    {
                        GiftDictionary[gift] = value + answer;
                    }
                    else
                    {
                        GiftDictionary.Add( gift, answer );
                    }


                }

            }


            // Go through Post Data and add up scores for each Ability and Abilities type
            foreach ( string x in request.Params.Keys )
            {
                if ( x.Length == 8 && x.Contains( "-" ) && x.StartsWith( "A" ) )
                {
                    string ability = Int32.Parse( x.Substring( 5, 3 ) ).ToString();

                    int answer = 0;

                    switch ( request.Params[x] )
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
                    if ( AbilityDictionary.TryGetValue( ability, out value ) )
                    {
                        AbilityDictionary[ability] = value + answer;
                    }
                    else
                    {
                        AbilityDictionary.Add( ability, answer );
                    }



                }

            }


            // Make a SortedDictionary to sort highest scores descending (yay for avoiding sorting algorithm!)
            var sortedGiftDictionary = from entry in GiftDictionary orderby entry.Value descending select entry;
            var sortedAbilityDictionary = from entry in AbilityDictionary orderby entry.Value descending select entry;

            // Set highest and lowest gifts
            TopGift1 = sortedGiftDictionary.ElementAt( 0 ).Key;
            TopGift2 = sortedGiftDictionary.ElementAt( 1 ).Key;
            TopGift3 = sortedGiftDictionary.ElementAt( 2 ).Key;
            TopGift4 = sortedGiftDictionary.ElementAt( 3 ).Key;
            LowestGift = sortedGiftDictionary.Last().Key;
            TopAbility1 = sortedAbilityDictionary.ElementAt( 0 ).Key;
            TopAbility2 = sortedAbilityDictionary.ElementAt( 1 ).Key;
            LowestAbility = sortedAbilityDictionary.Last().Key;



            // Save the attributes
            SaveAttributes( Int32.Parse( TopGift1 ), Int32.Parse( TopGift2 ), Int32.Parse( TopGift3 ), Int32.Parse( TopGift4 ), Int32.Parse( TopAbility1 ), Int32.Parse( TopAbility2 ), People, Places, Events, HeartCategories, HeartCauses, HeartPassion, context );


            // Send a confirmation email describing the gifts and how to get back to them
            SendEmail( ThePerson.Email, "info@newpointe.org", "SHAPE Assessment Results", GenerateEmailBody( ThePerson, rockContext, Int32.Parse( TopGift1 ), Int32.Parse( TopGift2 ), Int32.Parse( TopAbility1 ), Int32.Parse( TopAbility2 ), FormId ), rockContext );

            // Email the campus pastor if the person has the gift of Leadership as #1
            CampusPastor( Int32.Parse( TopGift1 ), campusFromForm, ThePerson );

            // Write a 200 code in the response
            response.ContentType = "text/xml";
            response.AddHeader( "Content-Type", "text/xml" );
            response.StatusCode = 200;



        }


        /// <summary>
        /// Email the campus pastor if the person's #1 gift is leadership
        /// </summary>
        /// <param name="Gift1">Int of category of Gift1</param>
        /// <param name="campus">String of the person's campus name</param>
        /// <param name="personWithLeadership">Person object with the target person</param>
        /// <returns></returns>
        public void CampusPastor( int Gift1, string campus, Person personWithLeadership )
        {
            DefinedValueService definedValueService = new DefinedValueService( rockContext );

            string gift1Value = definedValueService.GetByIds( new List<int> { Gift1 } ).FirstOrDefault().Value;

            if ( gift1Value == "Leadership" )
            {
                // Get the Header and Footer
                string emailHeader = Rock.Web.Cache.GlobalAttributesCache.Value( "EmailHeader" );
                string emailFooter = Rock.Web.Cache.GlobalAttributesCache.Value( "EmailFooter" );

                CampusService campusService = new CampusService( rockContext );
                var campusPastor = campusService.Queryable().FirstOrDefault( a => a.Name == campus ).LeaderPersonAlias;
                string emailBody = emailHeader + String.Format( @"<h2>Hey {0},</h2>
                                     <p>{1} from your campus has taken the SHAPE Assessment and their top gift is Leadership.
                                     <a href=""https://rock.newpointe.org/Person/{2}"">Click here</a> to see their Rock profile, SHAPE Profile, and be sure to follow up!</p>
                                     <p>Thanks,<br />The RockBot</p>", campusPastor.Person.NickName, personWithLeadership.FullName, personWithLeadership.Id ) + emailFooter;

                SendEmail( campusPastor.Person.Email, "info@newpointe.org", "SHAPE Assessment Results", emailBody, rockContext );

            }
        }

        /// <summary>
        /// Write the 2 highest gift attributes on the person's record.
        /// </summary>
        public void SaveAttributes( int Gift1, int Gift2, int Gift3, int Gift4, int Ability1, int Ability2, string People, string Places, string Events, string HCategories, string HCauses, string HPassion, HttpContext context )
        {
            if ( ThePerson != null )
            {
                DefinedValueService definedValueService = new DefinedValueService( rockContext );
                ThePerson.LoadAttributes();
                ThePerson.SetAttributeValue( "SpiritualGift1", definedValueService.Get( Gift1 ).Guid );
                ThePerson.SetAttributeValue( "SpiritualGift2", definedValueService.Get( Gift2 ).Guid );
                ThePerson.SetAttributeValue( "SpiritualGift3", definedValueService.Get( Gift3 ).Guid );
                ThePerson.SetAttributeValue( "SpiritualGift4", definedValueService.Get( Gift4 ).Guid );
                ThePerson.SetAttributeValue( "Ability1", definedValueService.Get( Ability1 ).Guid );
                ThePerson.SetAttributeValue( "Ability2", definedValueService.Get( Ability2 ).Guid );
                ThePerson.SetAttributeValue( "SpiritualGiftForm", FormId );
                ThePerson.SetAttributeValue( "SHAPEPeople", People );
                ThePerson.SetAttributeValue( "SHAPEPlaces", Places );
                ThePerson.SetAttributeValue( "SHAPEEvents", Events );
                ThePerson.SetAttributeValue( "HeartCategories", HCategories );
                ThePerson.SetAttributeValue( "HeartCauses", HCauses );
                ThePerson.SetAttributeValue( "HeartPassion", HPassion );
                ThePerson.SaveAttributeValues( rockContext );
            }
        }



        /// <summary>
        /// Gets the person from form data, or creates a new person if one doesn't exist
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Person GetPerson( RockContext rockContext )
        {
            DefinedValueCache recordType_Person = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() );
            DefinedValueCache connectionStatus_WebProspect = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_WEB_PROSPECT.AsGuid() );
            DefinedValueCache recordStatus_Pending = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid() );

            var personService = new PersonService( rockContext );

            var person = personService.FindPerson( FirstName, LastName, Email, true, false, false );
            if(person != null)
            {
                return person;
            }
            else
            {
                person = new Person() {
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = Email,
                    IsEmailActive = true,
                    EmailPreference = EmailPreference.EmailAllowed,
                    RecordTypeValueId = recordType_Person.Id,
                    ConnectionStatusValueId = connectionStatus_WebProspect.Id,
                    RecordStatusValueId = recordStatus_Pending.Id,

                };
                PersonService.SaveNewPerson( person, rockContext, CampusId, false);
                return person;
            }
        }


        public bool IsReusable
        {
            get
            {
                return false;
            }
        }


        private void SendEmail( string recipient, string from, string subject, string body, RockContext rockContext )
        {
            var recipients = new List<string>();
            recipients.Add( recipient );

            var mediumData = new Dictionary<string, string>();
            mediumData.Add( "From", from );
            mediumData.Add( "Subject", subject );
            mediumData.Add( "Body", body );

            var mediumEntity = EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid(), rockContext );
            if ( mediumEntity != null )
            {
                var medium = MediumContainer.GetComponent( mediumEntity.Name );
                if ( medium != null && medium.IsActive )
                {
                    var transport = medium.Transport;
                    if ( transport != null && transport.IsActive )
                    {
                        var appRoot = GlobalAttributesCache.Value( "InternalApplicationRoot" );
                        transport.Send( mediumData, recipients, appRoot, string.Empty );
                    }
                }
            }
        }



        private static string Base64Encode( string plainText )
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes( plainText );
            return System.Convert.ToBase64String( plainTextBytes );
        }


        private static string GenerateEmailBody( Person person, RockContext rockContext, int Gift1, int Gift2, int Ability1, int Ability2, string profileValue )
        {
            // Get the Header and Footer
            string emailHeader = Rock.Web.Cache.GlobalAttributesCache.Value( "EmailHeader" );
            string emailFooter = Rock.Web.Cache.GlobalAttributesCache.Value( "EmailFooter" );


            // Get all of the data about the assiciated gifts and ability categories
            DefinedValueService definedValueService = new DefinedValueService( rockContext );

            var shapeGift1Object = definedValueService.GetListByIds( new List<int> { Gift1 } ).FirstOrDefault();
            var shapeGift2Object = definedValueService.GetListByIds( new List<int> { Gift2 } ).FirstOrDefault();
            var ability1Object = definedValueService.GetListByIds( new List<int> { Ability1 } ).FirstOrDefault();
            var ability2Object = definedValueService.GetListByIds( new List<int> { Ability2 } ).FirstOrDefault();

            shapeGift1Object.LoadAttributes();
            shapeGift2Object.LoadAttributes();
            ability1Object.LoadAttributes();
            ability2Object.LoadAttributes();

            string profileLink = "http://newpointe.org/MySHAPE/" + profileValue;

            string myNewPointeMessage;
            if ( person.Users.Count > 0 )
            {
                myNewPointeMessage = @"You can view your full SHAPE Profile any time from your 
                                    <a href=""https://newpointe.org/MyNewPointe"">MyNewPointe Account</a>";
            }
            else
            {
                myNewPointeMessage = String.Format( @"You can view your full SHAPE Profile, plus manage your personal information, 
                                     give online, and much more via a MyNewPointe account.
                                     <a href=""{0}"">Click here</a> to create a MyNewPointe Account via your SHAPE Profile now!", profileLink );
            }



            //Put the body text together
            string emailBody = emailHeader + String.Format( @"
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
            ability1Object.Value, ability1Object.Description, ability2Object.Value, ability2Object.Description, myNewPointeMessage ) + emailFooter;


            return emailBody;
        }



    }


}