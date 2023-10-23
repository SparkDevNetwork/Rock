// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Diagnostics;
using System.Linq;
using Rock.Data;
using Rock.Model;
using System.Data.Entity;
using Rock;
using Rock.Web.Cache;
using Rock.Tests.Shared;
using Rock.Tests.Integration.TestData;

namespace Rock.Tests.Integration.Crm.Prayer
{
    /// <summary>
    /// Create and manage test data for the Prayer Requests feature.
    /// </summary>
    public static class PrayerFeatureDataHelper
    {
        private static PrayerFeatureDataFactory _factory = new PrayerFeatureDataFactory();

        public static void RemoveSampleData()
        {
            _factory.RemovePrayerRequestTestData();
        }
        public static void AddSampleData( bool removeExistingData = false )
        {
            if ( removeExistingData )
            {
                RemoveSampleData();
            }

            _factory.AddPrayerCategories();
            _factory.AddPrayerRequestKnownData();
            _factory.AddPrayerRequestCommentsEmailTemplate();
        }
    }

    public class PrayerFeatureDataFactory
    {
        private const int _PrayerRequestsMaxCandidates = 20;
        private const int _PrayerCommentsMaxCandidates = 500;
        private const int _PrayerCommentsMaxPerRequest = 10;

        private readonly DateTime _PrayerRequestTestPeriodStartDate = new DateTime( 2019, 1, 1 );
        private readonly DateTime _PrayerRequestTestPeriodEndDate = new DateTime( 2019, 3, 31 );
        private const int _PrayerRequestExpiryDays = 90;

        private int _CreatedByPersonAliasId;
        private int _PrayerNoteTypeId;

        private Person GetWellKnownTestPerson( string personGuidString, RockContext dataContext )
        {
            var personService = new PersonService( dataContext );

            var person = personService.Get( personGuidString.AsGuid() );

            return person;
        }

        private void Initialize()
        {
            var dataContext = new RockContext();

            var noteTypeService = new NoteTypeService( dataContext );

            _PrayerNoteTypeId = noteTypeService.GetId( Rock.SystemGuid.NoteType.PRAYER_COMMENT.AsGuid() ).GetValueOrDefault();

            var personService = new PersonService( dataContext );

            var adminPerson = this.GetAdminPersonOrThrow( personService );

            _CreatedByPersonAliasId = adminPerson.PrimaryAliasId.GetValueOrDefault();
        }

        public void RemovePrayerRequestTestData()
        {
            var dataContext = new RockContext();

            // Delete Prayer Requests
            var requestService = new PrayerRequestService( dataContext );

            var requests = requestService.Queryable().Where( x => x.ForeignKey == RecordTag.PrayerRequestFeature ).ToList();

            var requestCount = requests.Count;

            requestService.DeleteRange( requests );

            dataContext.SaveChanges();

            // Delete Prayer Comments
            var noteService = new NoteService( dataContext );

            var notes = noteService.Queryable().Where( x => x.ForeignKey == RecordTag.PrayerRequestFeature ).ToList();

            var noteCount = notes.Count;

            noteService.DeleteRange( notes );

            dataContext.SaveChanges();

            // Delete Email Templates
            var systemEmailService = new SystemCommunicationService( dataContext );

            var systemEmails = systemEmailService.Queryable().Where( x => x.ForeignKey == RecordTag.PrayerRequestFeature ).ToList();

            systemEmailService.DeleteRange( systemEmails );

            dataContext.SaveChanges();

            // Delete Categories
            var categoriesService = new CategoryService( dataContext );

            var helper = new CoreModuleDataFactory( RecordTag.PrayerRequestFeature );

            helper.DeleteCategoriesByRecordTag( dataContext );

            dataContext.SaveChanges();

            Debug.Print( $"Delete Prayer Requests: {requestCount} requests deleted, {noteCount} notes deleted." );
        }

        public void AddPrayerRequestTestData()
        {
            RemovePrayerRequestTestData();

            AddPrayerCategories();

            AddPrayerRequestKnownData();

            AddPrayerRequestCommentsEmailTemplate();
        }

        internal void AddPrayerRequestCommentsEmailTemplate()
        {
            var dataContext = new RockContext();
            var systemEmailService = new SystemCommunicationService( dataContext );

            // Add Email Template
            var systemCategory = CategoryCache.Get( TestGuids.Category.SystemEmailSystem.AsGuid() );

            var template = systemEmailService.Get( TestGuids.SystemEmailGuid.PrayerCommentsNotification.AsGuid() );
            if ( template == null )
            {
                template = new SystemCommunication();
                template.Guid = TestGuids.SystemEmailGuid.PrayerCommentsNotification.AsGuid();

                systemEmailService.Add( template );
            }

            template.ForeignKey = RecordTag.PrayerRequestFeature;
            template.Title = "Prayer Request Comments Digest";
            template.Subject = "Prayer Request Update - {{ PrayerRequest.Text | Truncate:50,'...' }}";
            template.CategoryId = systemCategory.Id;

            template.Body = @"
{{ 'Global' | Attribute:'EmailHeader' }}
{% assign firstName = FirstName  %}
{% if PrayerRequest.RequestedByPersonAlias.Person.NickName %}
   {% assign firstName = PrayerRequest.RequestedByPersonAlias.Person.NickName %}
{% endif %}

<p>
{{ firstName }}, below are recent comments from the prayer request you submitted on {{ PrayerRequest.EnteredDateTime | Date:'dddd, MMMM dd' }}.
</p>
<p>
<strong>Request</strong>
<br/>
{{ PrayerRequest.Text }}
</p>
<p>
<strong>Comments</strong>
<br/>
{% for comment in Comments %}
<i>{{ comment.CreatedByPersonName }} ({{ comment.CreatedDateTime | Date:'dd-MMM-yyyy h:mmtt' }})</i><br/>
{{ comment.Text }}<br/><br/>
{% endfor %}
</p>

{{ 'Global' | Attribute:'EmailFooter' }}";

            dataContext.SaveChanges();
        }

        /* Prayer Request Categories:
         * All Church
         * All Church|Comfort/Grief
         * All Church|Finances/Job.
         * All Church|Finances/Job|Finances
         * All Church|Finances/Job|Job
         * All Church|Finances/Job|Job
         */
        internal void AddPrayerCategories()
        {
            var helper = new CoreModuleDataFactory( RecordTag.PrayerRequestFeature );

            var dataContext = new RockContext();

            var prayerRequestEntityTypeId = EntityTypeCache.GetId( typeof( Rock.Model.PrayerRequest ) ).GetValueOrDefault();

            // Add Category: All Church
            var allChurchCategory = helper.CreateCategory( "All Church",
                TestGuids.Category.PrayerRequestAllChurch.AsGuid(),
                prayerRequestEntityTypeId );
            helper.AddOrUpdateCategory( dataContext, allChurchCategory );

            dataContext.SaveChanges();

            // Add Category: All Church|Comfort/Grief
            var healthCategory = helper.CreateCategory( "Comfort/Grief", TestGuids.Category.PrayerRequestComfortAndGrief.AsGuid(), prayerRequestEntityTypeId );
            healthCategory.ParentCategoryId = allChurchCategory.Id;
            helper.AddOrUpdateCategory( dataContext, healthCategory );

            // Add Category: All Church|Provision.
            var needsProvisionCategory = helper.CreateCategory( "Provision", TestGuids.Category.PrayerRequestFinancesAndJob.AsGuid(), prayerRequestEntityTypeId );
            needsProvisionCategory.ParentCategoryId = allChurchCategory.Id;
            helper.AddOrUpdateCategory( dataContext, needsProvisionCategory );

            dataContext.SaveChanges();

            // Add some non-standard child Categories to test nested child categories.

            // Add Category: All Church|Provision|Finances
            var needsFinancesCategory = helper.CreateCategory( "Finances", TestGuids.Category.PrayerRequestFinancesOnly.AsGuid(), prayerRequestEntityTypeId );
            needsFinancesCategory.ParentCategoryId = needsProvisionCategory.Id;
            helper.AddOrUpdateCategory( dataContext, needsFinancesCategory );

            // Add Category: All Church|Provision|Job
            var needsJobCategory = helper.CreateCategory( "Job", TestGuids.Category.PrayerRequestJobOnly.AsGuid(), prayerRequestEntityTypeId );
            needsJobCategory.ParentCategoryId = needsProvisionCategory.Id;
            helper.AddOrUpdateCategory( dataContext, needsJobCategory );

            // Add Category: All Church|Provision|Other
            var otherFinancesCategory = helper.CreateCategory( "Other", TestGuids.Category.PrayerRequestFinancesOther.AsGuid(), prayerRequestEntityTypeId );
            needsJobCategory.ParentCategoryId = needsProvisionCategory.Id;
            helper.AddOrUpdateCategory( dataContext, otherFinancesCategory );

            dataContext.SaveChanges();
        }

        /// <summary>
        /// Adds a predictable set of Prayer Requests to the test database that are used for integration testing.
        /// </summary>
        public void AddPrayerRequestKnownData()
        {
            Initialize();

            // Category: All Church|FinanceAndJobs
            AddPrayerRequestAllChurchJobsAndFinances();

            // Category: All Church|FinanceAndJobs|Jobs
            AddPrayerRequestTedDeckerForJob();

            // Category: All Church|FinanceAndJobs|Finance
            AddPrayerRequestBenJonesForFinance();

            // Category: All Church|Health
            AddPrayerRequestMariahJacksonForHealth();

            // Category: All Church
            AddPrayerRequestSarahSimmonsForWisdom();
        }

        /// <summary>
        /// Add a prayer request with the following properties:
        /// - Requested in week 1 of the test period.
        /// - Comments added over a 3 week period.
        /// - In Category "All Church|Finances/Job".
        /// - Requested by a guest.
        /// </summary>
        private void AddPrayerRequestAllChurchJobsAndFinances()
        {
            Debug.Print( $"Adding Prayer Request: Jobs and Finances (All Church)..." );

            var dataContext = new RockContext();

            var noteService = new NoteService( dataContext );
            var prayerRequestService = new PrayerRequestService( dataContext );

            var personAlishaMarble = GetWellKnownTestPerson( TestGuids.TestPeople.AlishaMarble, dataContext );
            var personMariahJackson = GetWellKnownTestPerson( TestGuids.TestPeople.MariahJackson, dataContext );
            var personBenJones = GetWellKnownTestPerson( TestGuids.TestPeople.BenJones, dataContext );
            var personSarahSimmons = GetWellKnownTestPerson( TestGuids.TestPeople.SarahSimmons, dataContext );

            var requestDate = _PrayerRequestTestPeriodStartDate;

            var prayerRequest = CreateNewPrayerRequest( _PrayerRequestTestPeriodStartDate.AddHours( 19 ).AddMinutes( 30 ),
                null,
                null,
                "Please pray for all of the people in our town who have been affected by the recent closing of local factories, that they have sufficient finances to cover their needs and will find new employment opportunities.",
                TestGuids.PrayerRequestGuid.AllChurchForEmployment );
            prayerRequest.CategoryId = CategoryCache.GetId( TestGuids.Category.PrayerRequestFinancesAndJob.AsGuid() );
            prayerRequest.Email = "concernedcitizen@ourtown.org";
            prayerRequestService.Add( prayerRequest );

            dataContext.SaveChanges();

            var requestId = prayerRequest.Id;

            // Add comments for prayer request: Week 1.
            var comment1 = CreateNewPrayerRequestComment( requestId,
                requestDate.AddDays( 2 ).AddHours( 11 ).AddMinutes( 15 ),
                "At times like these, we may find comfort in the words of Jesus in Matthew 6:25 - if we seek His kingdom first, He has promised that all these things will be added to us.",
                personMariahJackson.PrimaryAliasId.Value,
                TestGuids.PrayerRequestCommentGuid.FinancesAndJobW1C1 );

            noteService.Add( comment1 );

            var comment2 = CreateNewPrayerRequestComment( requestId,
                requestDate.AddDays( 4 ).AddHours( 7 ),
                "I feel that God is using this as an opportunity for us as a church to demonstrate His love and genorosity. Let's all open our hearts and give whatever we can to help those who are affected by these difficult times.",
                personBenJones.PrimaryAliasId.Value,
                TestGuids.PrayerRequestCommentGuid.FinancesAndJobW1C2 );

            noteService.Add( comment2 );

            // Add comments for prayer request: Week 2.
            var comment3 = CreateNewPrayerRequestComment( requestId,
                requestDate.AddDays( 8 ),
                "While praying through this, I felt God reassuring me with His promise: \"My God shall supply all your needs according to his riches in glory...\"",
                personSarahSimmons.PrimaryAliasId.Value,
                TestGuids.PrayerRequestCommentGuid.FinancesAndJobW2C1 );

            noteService.Add( comment3 );

            dataContext.SaveChanges();
        }

        /// <summary>
        /// Add a prayer request with the following properties:
        /// - Requested in week 1 of the test period.
        /// - Comments added over a 3 week period.
        /// - Category: Job
        /// </summary>
        /* Ted Decker
         * 01-Jan-2019: Request Created
         * 03-Jan-2019: Comment 1 (Sarah)
         * 04-Jan-2019: Comment 2 (Ben)
         * 08-Jan-2019: Comment 3
         * 11-Jan-2019: Comment 4
         */
        private void AddPrayerRequestTedDeckerForJob()
        {
            Debug.Print( $"Adding Prayer Request: Ted Decker (Job)..." );

            var dataContext = new RockContext();

            var noteService = new NoteService( dataContext );
            var prayerRequestService = new PrayerRequestService( dataContext );

            var personTedDecker = GetWellKnownTestPerson( TestGuids.TestPeople.TedDecker, dataContext );
            var personAlishaMarble = GetWellKnownTestPerson( TestGuids.TestPeople.AlishaMarble, dataContext );
            var personMariahJackson = GetWellKnownTestPerson( TestGuids.TestPeople.MariahJackson, dataContext );
            var personBenJones = GetWellKnownTestPerson( TestGuids.TestPeople.BenJones, dataContext );
            var personSarahSimmons = GetWellKnownTestPerson( TestGuids.TestPeople.SarahSimmons, dataContext );

            var requestDate = _PrayerRequestTestPeriodStartDate;

            var prayerRequest = CreateNewPrayerRequest( _PrayerRequestTestPeriodStartDate,
                personTedDecker,
                personTedDecker,
                "Requested prayer for full-time employment.",
                TestGuids.PrayerRequestGuid.TedDeckerForEmployment );
            prayerRequest.CategoryId = CategoryCache.GetId( TestGuids.Category.PrayerRequestJobOnly.AsGuid() );
            prayerRequestService.Add( prayerRequest );

            dataContext.SaveChanges();

            var requestId = prayerRequest.Id;

            // Add comments for prayer request: Week 1.
            var comment1 = CreateNewPrayerRequestComment( requestId,
                requestDate.AddDays( 2 ),
                "I am upholding you in prayer, claiming the promise that God will provide for your needs according to His riches in glory!",
                personSarahSimmons.PrimaryAliasId.Value,
                TestGuids.PrayerRequestCommentGuid.TedDecker11 );

            noteService.Add( comment1 );

            var comment2 = CreateNewPrayerRequestComment( requestId,
                requestDate.AddDays( 4 ),
                "Hang in there Ted, we are praying for you!",
                personBenJones.PrimaryAliasId.Value,
                TestGuids.PrayerRequestCommentGuid.TedDecker12 );

            noteService.Add( comment2 );

            // Add comments for prayer request: Week 2.
            var comment3 = CreateNewPrayerRequestComment( requestId,
                requestDate.AddDays( 8 ),
                "Ted, I hope you find this scripture encouraging: [Psalm 90:17] \"May the favor of the Lord our God rest on us; establish the work of our hands for us — yes, establish the work of our hands.\"",
                personSarahSimmons.PrimaryAliasId.Value,
                TestGuids.PrayerRequestCommentGuid.TedDecker21 );

            noteService.Add( comment3 );

            var comment4 = CreateNewPrayerRequestComment( requestId,
                 requestDate.AddDays( 10 ),
                 "We are praying for you - if there is anything we can do to help you during this time please let us know.",
                 personMariahJackson.PrimaryAliasId.Value,
                 TestGuids.PrayerRequestCommentGuid.TedDecker22 );

            noteService.Add( comment4 );

            dataContext.SaveChanges();
        }

        /// <summary>
        /// Add a prayer request with the following properties:
        /// - Requested in week 2 of the test period.
        /// - Has associated comments, but "Allow Comments" flag is set to False.
        /// - Category: Health
        /// </summary>
        /* Prayer Request: Mariah Jackson
         * 03-Jan: Request Created
         * 03-Jan: Comment 1 (Week 1)
         * 04-Jan: Comment 2 (Week 1)
         * 08-Jan: Comment 3 (Week 2)
         *//// 
        private void AddPrayerRequestMariahJacksonForHealth()
        {
            Debug.Print( $"Adding Prayer Request: Mariah Jackson..." );

            var dataContext = new RockContext();

            var noteService = new NoteService( dataContext );
            var prayerRequestService = new PrayerRequestService( dataContext );

            var personAlishaMarble = GetWellKnownTestPerson( TestGuids.TestPeople.AlishaMarble, dataContext );
            var personMariahJackson = GetWellKnownTestPerson( TestGuids.TestPeople.MariahJackson, dataContext );
            var personBenJones = GetWellKnownTestPerson( TestGuids.TestPeople.BenJones, dataContext );
            var personSarahSimmons = GetWellKnownTestPerson( TestGuids.TestPeople.SarahSimmons, dataContext );

            // Add new Prayer Request
            var requestDate = _PrayerRequestTestPeriodStartDate.AddDays( 4 );

            var prayerRequest = CreateNewPrayerRequest( requestDate,
                personMariahJackson,
                personMariahJackson,
                "Please pray for my mother. She is in hospital, waiting for a kidney to become available for a transplant.",
                TestGuids.PrayerRequestGuid.MariahJacksonForMother );
            prayerRequest.CategoryId = CategoryCache.GetId( TestGuids.Category.PrayerRequestHealthIssues.AsGuid() );

            prayerRequest.AllowComments = false;

            prayerRequestService.Add( prayerRequest );

            dataContext.SaveChanges();

            var requestId = prayerRequest.Id;

            // Add comments for prayer request: Week 1
            var comment1 = CreateNewPrayerRequestComment( requestId,
                requestDate.AddDays( 2 ),
                "I am upholding you in prayer, claiming the promise that God will provide for your needs according to His riches in glory!",
                personSarahSimmons.PrimaryAliasId.Value,
                TestGuids.PrayerRequestCommentGuid.MariahJacksonComment1 );

            noteService.Add( comment1 );

            var comment2 = CreateNewPrayerRequestComment( requestId,
                requestDate.AddDays( 4 ),
                "Remember that your mother is being cared for by Jehovah-Rapha, our God of healing.",
                personBenJones.PrimaryAliasId.Value,
                TestGuids.PrayerRequestCommentGuid.MariahJacksonComment2 );

            noteService.Add( comment2 );

            // Add comments for prayer request: Week 2.
            var comment3 = CreateNewPrayerRequestComment( requestId,
                requestDate.AddDays( 8 ),
                "I hope you find this scripture encouraging: [Psalm 90:17] \"May the favor of the Lord our God rest on us; establish the work of our hands for us— yes, establish the work of our hands.\"",
                personSarahSimmons.PrimaryAliasId.Value,
                TestGuids.PrayerRequestCommentGuid.MariahJacksonComment3 );

            noteService.Add( comment3 );

            dataContext.SaveChanges();
        }

        /// <summary>
        /// Add a prayer request with the following properties:
        /// - Requested in week 2 of the test period.
        /// - Includes private comments.
        /// - Exists in sub-category "All Church|Finance"
        /// </summary>
        private void AddPrayerRequestBenJonesForFinance()
        {
            Debug.Print( $"Adding Prayer Request: Ben Jones..." );

            var dataContext = new RockContext();

            var noteService = new NoteService( dataContext );
            var prayerRequestService = new PrayerRequestService( dataContext );

            var personAlishaMarble = GetWellKnownTestPerson( TestGuids.TestPeople.AlishaMarble, dataContext );
            //var personMariahJackson = GetWellKnownTestPerson( TestGuids.TestPeople.MariahJackson, dataContext );
            var personBenJones = GetWellKnownTestPerson( TestGuids.TestPeople.BenJones, dataContext );
            var personSarahSimmons = GetWellKnownTestPerson( TestGuids.TestPeople.SarahSimmons, dataContext );

            // Add new Prayer Request
            var requestDate = _PrayerRequestTestPeriodStartDate.AddDays( 9 );

            var prayerRequest = CreateNewPrayerRequest( requestDate.AddHours( 14 ).AddMinutes( 30 ),
                personBenJones,
                personAlishaMarble,
                "Requested prayer for finances to buy a new car so he can travel to and from his new job in a neighbouring town.",
                TestGuids.PrayerRequestGuid.BenJonesForCarFinance );
            prayerRequest.CategoryId = CategoryCache.GetId( TestGuids.Category.PrayerRequestFinancesOnly.AsGuid() );

            prayerRequestService.Add( prayerRequest );

            dataContext.SaveChanges();

            var requestId = prayerRequest.Id;

            // Add comments for prayer request: Week 1
            var comment1 = CreateNewPrayerRequestComment( requestId,
                requestDate.AddDays( 2 ).AddHours( 6 ).AddMinutes( 30 ),
                "Ben, I've sent a little something in the mail for you that should help toward that car - let's both praise God for his blessings!",
                personAlishaMarble.PrimaryAliasId.Value,
                TestGuids.PrayerRequestCommentGuid.BenJonesComment1 );

            noteService.Add( comment1 );

            var comment2 = CreateNewPrayerRequestComment( requestId,
                requestDate.AddDays( 4 ).AddHours( 15 ).AddMinutes( 30 ),
                "[Private] I was praying about your need and a friend of mine mentioned that they are getting a company vehicle soon and would like to donate their current family car to you!",
                personSarahSimmons.PrimaryAliasId.Value,
                TestGuids.PrayerRequestCommentGuid.BenJonesComment2 );

            comment2.IsPrivateNote = true;

            noteService.Add( comment2 );

            dataContext.SaveChanges();
        }

        /// <summary>
        /// Add a prayer request with the following properties:
        /// - Requested on Day 1 (9:00am) of the test period.
        /// - Has 1 comment on Day 1.
        /// - Exists in sub-category "All Church|Finance"
        /// </summary>
        private void AddPrayerRequestSarahSimmonsForWisdom()
        {
            Debug.Print( $"Adding Prayer Request: Sarah Simmons..." );

            var dataContext = new RockContext();

            var noteService = new NoteService( dataContext );
            var prayerRequestService = new PrayerRequestService( dataContext );

            var personAlishaMarble = GetWellKnownTestPerson( TestGuids.TestPeople.AlishaMarble, dataContext );
            var personMariahJackson = GetWellKnownTestPerson( TestGuids.TestPeople.MariahJackson, dataContext );
            var personBenJones = GetWellKnownTestPerson( TestGuids.TestPeople.BenJones, dataContext );
            var personSarahSimmons = GetWellKnownTestPerson( TestGuids.TestPeople.SarahSimmons, dataContext );

            // Add new Prayer Request
            var requestDate = _PrayerRequestTestPeriodStartDate;

            var prayerRequest = CreateNewPrayerRequest( requestDate.AddHours( 9 ),
                personSarahSimmons,
                personMariahJackson,
                "Needs prayer for wisdom in deciding how to best distribute a recent lottery win.",
                TestGuids.PrayerRequestGuid.SarahSimmonsForFinancialWisdom );
            prayerRequest.CategoryId = CategoryCache.GetId( TestGuids.Category.PrayerRequestFinancesOther.AsGuid() );

            prayerRequestService.Add( prayerRequest );

            dataContext.SaveChanges();

            var requestId = prayerRequest.Id;

            // Add comments for prayer request: Week 1
            var comment1 = CreateNewPrayerRequestComment( requestId,
                requestDate.AddHours( 6 ).AddMinutes( 30 ),
                "I pray that I would have similar 'afflictions'!",
                personBenJones.PrimaryAliasId.Value,
                TestGuids.PrayerRequestCommentGuid.SarahSimmonsComment1 );

            noteService.Add( comment1 );

            dataContext.SaveChanges();
        }

        /// <summary>
        /// Adds a random set of Prayer Requests and comments to the test database that are used for integration testing.
        /// </summary>
        public void AddPrayerRequestBulkTestData()
        {
            var dataContext = new RockContext();

            var noteTypeService = new NoteTypeService( dataContext );

            _PrayerNoteTypeId = noteTypeService.GetId( Rock.SystemGuid.NoteType.PRAYER_COMMENT.AsGuid() ).GetValueOrDefault();

            var personService = new PersonService( dataContext );

            // Get the list of people who are candidates for prayer.
            var personList = personService.Queryable()
                .AsNoTracking()
                .Where( x => !x.IsSystem )
                .Take( _PrayerRequestsMaxCandidates + _PrayerCommentsMaxCandidates )
                .ToList();

            var requestCandidateList = personList.Take( _PrayerRequestsMaxCandidates ).ToList();
            var commentCandidateList = personList.Skip( _PrayerRequestsMaxCandidates ).ToList();

            var currentDate = _PrayerRequestTestPeriodEndDate;

            var rng = new Random();

            var adminPerson = this.GetAdminPersonOrThrow( personService );

            _CreatedByPersonAliasId = adminPerson.PrimaryAliasId.GetValueOrDefault();

            // Create new prayer requests
            int entriesAdded = 0;

            var startDate = _PrayerRequestTestPeriodStartDate;
            var endDate = _PrayerRequestTestPeriodEndDate;

            var requestDates = RandomizedDataHelper.GetRandomDatesInPeriod( startDate, endDate, requestCandidateList.Count );

            foreach ( var person in requestCandidateList )
            {
                var requestDate = requestDates[entriesAdded];

                // Add new Prayer Request
                var prayerRequestService = new PrayerRequestService( dataContext );

                var prayerRequest = CreateNewPrayerRequest( requestDate, person, adminPerson, "Here are the details of the prayer request..." );

                prayerRequestService.Add( prayerRequest );

                dataContext.SaveChanges();

                var requestId = prayerRequest.Id;

                // Add comments for prayer request.
                var noteService = new NoteService( dataContext );

                var commentCount = RandomizedDataHelper.GetRandomNumber( 0, _PrayerCommentsMaxPerRequest );

                var commentDates = RandomizedDataHelper.GetRandomDatesInPeriod( startDate, endDate, commentCount );

                foreach ( var commentDate in commentDates )
                {
                    var commentByPerson = commentCandidateList.GetRandomElement();

                    var comment = CreateNewPrayerRequestComment( requestId, commentDate, "I am upholding you in prayer!", commentByPerson.PrimaryAliasId.Value );

                    noteService.Add( comment );
                }

                dataContext.SaveChanges();

                entriesAdded++;

                Debug.Print( $"Added Prayer Request #{entriesAdded}... [PersonId={person.Id}, Date={requestDate}]" );
            }

            Debug.Print( $"Create Data completed: { entriesAdded } requests created." );

            if ( entriesAdded == 0 )
            {
                throw new Exception( "No requests created." );
            }
        }

        private PrayerRequest CreateNewPrayerRequest( DateTime requestDateTime, Person requestedForPerson, Person requestedByPerson, string text, string requestGuidText = null )
        {
            // Add new Prayer Request
            var prayerRequest = new PrayerRequest();

            if ( requestGuidText.IsNotNullOrWhiteSpace() )
            {
                prayerRequest.Guid = requestGuidText.AsGuid();
            }

            if ( requestedByPerson != null )
            {
                prayerRequest.RequestedByPersonAliasId = requestedByPerson.PrimaryAliasId;
                prayerRequest.Email = requestedByPerson.Email;
            }

            if ( requestedForPerson != null )
            {
                prayerRequest.FirstName = requestedForPerson.FirstName;
                prayerRequest.LastName = requestedForPerson.LastName;
                prayerRequest.CampusId = requestedForPerson.PrimaryCampusId;
            }
            else
            {
                prayerRequest.FirstName = "(All Unemployed)";
            }

            prayerRequest.CreatedDateTime = requestDateTime;
            prayerRequest.FlagCount = RandomizedDataHelper.GetRandomNumber( 0, 99 );
            prayerRequest.EnteredDateTime = requestDateTime;
            prayerRequest.CreatedByPersonAliasId = _CreatedByPersonAliasId;
            prayerRequest.ExpirationDate = requestDateTime.AddDays( _PrayerRequestExpiryDays );
            prayerRequest.IsActive = true;
            prayerRequest.IsApproved = true;
            prayerRequest.IsPublic = true;
            prayerRequest.PrayerCount = RandomizedDataHelper.GetRandomNumber( 0, 99 );
            prayerRequest.AllowComments = true;

            prayerRequest.Text = text;

            prayerRequest.ForeignKey = RecordTag.PrayerRequestFeature;

            return prayerRequest;
        }

        private Note CreateNewPrayerRequestComment( int prayerRequestId, DateTime commentDateTime, string text, int createdByPersonAliasId, string guidText = null )
        {
            var newNote = new Note();

            newNote.NoteTypeId = _PrayerNoteTypeId;

            if ( guidText.IsNotNullOrWhiteSpace() )
            {
                newNote.Guid = guidText.AsGuid();
            }

            newNote.EntityId = prayerRequestId;
            newNote.Caption = "Prayer Comment";
            newNote.IsAlert = false;
            newNote.IsPrivateNote = false;
            newNote.Text = text;
            newNote.CreatedByPersonAliasId = createdByPersonAliasId;
            newNote.CreatedDateTime = commentDateTime;

            newNote.ForeignKey = RecordTag.PrayerRequestFeature;

            return newNote;
        }

        #region Support Methods

        /// <summary>
        /// Get a known Person who has been assigned a security role of Administrator.
        /// </summary>
        /// <param name="personService"></param>
        /// <returns></returns>
        private Person GetAdminPersonOrThrow( PersonService personService )
        {
            var adminPerson = personService.Queryable().FirstOrDefault( x => x.FirstName == "Alisha" && x.LastName == "Marble" );

            if ( adminPerson == null )
            {
                throw new Exception( "Admin Person not found in test data set." );
            }

            return adminPerson;
        }

        #endregion
    }
}