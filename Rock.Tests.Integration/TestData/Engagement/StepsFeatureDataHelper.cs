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
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.TestFramework;
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Crm.Steps
{
    public enum StepsFeatureTestDataSetSpecifier
    {
        StepPrograms,
        StepParticipations,
        DataViews
    }

    /// <summary>
    /// Create sample data for the Steps feature of Rock.
    /// </summary>
    public static class StepsFeatureDataHelper
    {
        private static List<string> _loadedFeatureDataSets = new List<string>();

        #region Test Settings

        // Set this value to the foreign key used to identify the test data added to the target database.
        private static readonly string _SampleDataForeignKey = "Steps Sample Data";
        // Set this value to the number of people for whom steps will be created.
        private static readonly int _MaxPersonCount = 100;
        // Set this value to the maximum number of days between consecutive step achievements.
        private static readonly int _MaxDaysBetweenSteps = 120;
        // Set this value to the percentage chance that a given person will complete their most recently attempted step.
        private static readonly int _PercentChanceOfLastStepCompletion = 70;

        #endregion

        public static class Constants
        {
            // Constants
            public static Guid CategoryAdultsGuid = new Guid( "43DC43A8-420B-4012-BAA0-0A0DDF2D4A9A" );
            public static Guid CategoryYouthGuid = new Guid( "EAB10217-F288-4B29-B56F-50BD4BA5FB08" );
            public static Guid CategoryStepsGuid = new Guid( "31A23665-1C2E-4901-9E20-A7A0C7E6DF70" );

            public static Guid ProgramSacramentsGuid = new Guid( "2CAFBB12-901F-4880-A3E4-848F25CAF1A6" );
            public static Guid StepTypeBaptismGuid = new Guid( "23E73F78-587A-483F-99EF-855FD6AD1B11" );
            public static Guid StepTypeEucharistGuid = new Guid( "5EA01E79-5D17-4E87-A94F-8C4DD22131B5" );
            public static Guid StepTypeAnnointingGuid = new Guid( "48141631-38F6-40C0-8470-5253208CEA9A" );
            public static Guid StepTypeConfessionGuid = new Guid( "5F754006-7F8C-4BED-94A8-FC61CEEC6B43" );
            public static Guid StepTypeMarriageGuid = new Guid( "D03B3C65-C128-4918-A300-509B94B90175" );
            public static Guid StepTypeHolyOrdersGuid = new Guid( "0099C701-6C1E-418E-A94F-C247A2FE4BA5" );
            public static Guid StepTypeConfirmationGuid = new Guid( "F109169F-C1F6-46ED-9091-274540E3F3E2" );
            public static Guid StatusSacramentsSuccessGuid = new Guid( "A5C2A14F-9ED9-4DF4-A1C8-8ADF75E18833" );
            public static Guid StatusSacramentsPendingGuid = new Guid( "B591240C-4D4D-49DA-82E3-F8C1738B8EC6" );
            public static Guid StatusSacramentsIncompleteGuid = new Guid( "D0CDE46C-46D6-4F8D-B48D-55D7ACE6C3BA" );
            public static Guid PrerequisiteHolyOrdersGuid = new Guid( "D96A2C67-2D76-4697-838F-3514CA11485E" );

            public static Guid ProgramAlphaGuid = new Guid( "F7C2BA07-579B-4800-BBE1-B14B73E21E12" );
            public static Guid StepTypeAttenderGuid = new Guid( "24A70E5C-9871-452E-9403-3B43C003AA87" );
            public static Guid StepTypeVolunteerGuid = new Guid( "31DB3478-840B-4541-972D-059690AD623E" );
            public static Guid StepTypeLeaderGuid = new Guid( "95DF3AC6-AFBF-4BEA-AF86-805335A1C4CB" );
            public static Guid StatusAlphaStartedGuid = new Guid( "F29CF8A1-76A9-4436-9726-810DF0BC95C7" );
            public static Guid StatusAlphaCompletedGuid = new Guid( "7BA5F14D-BB38-4AAB-AB69-3A7F49494A55" );

            // Known Step Achievements
            // These steps are specifically identified to enable verification of query and filtering operations.
            
            public static Guid TedDeckerStepBaptismGuid = new Guid( "02BB71C9-5FE9-45B8-B230-51C7A8475B6B" );
            public static Guid TedDeckerStepConfirmationGuid = new Guid( "15412166-1DE0-41C0-9F8D-F07160513CAE" );
            public static Guid TedDeckerStepMarriage1Guid = new Guid( "414EBE88-2CD2-40E1-893B-216DEA2CB25E" );
            public static Guid TedDeckerStepMarriage2Guid = new Guid( "314F5303-C803-442B-AE39-DAA7BC30CCEE" );

            public static Guid BillMarbleStepBaptismGuid = new Guid( "CBD25E24-FD14-4B69-884B-F78DA8BF8FD8" );
            public static Guid BillMarbleStepConfirmationGuid = new Guid( "FE6B8F0D-450B-4737-80EE-E2BC4FA4EAB5" );
            public static Guid BillMarbleStepMarriage1Guid = new Guid( "E6F37E9D-177F-4CEA-91EA-2C2B45C4B1FD" );
            public static Guid BillMarbleStepAlphaAttenderGuid = new Guid( "680F64F1-C579-4216-B08C-68AF3FC762EC" );
            
            public static Guid AlishaMarbleStepBaptismGuid = new Guid( "12DF97EE-52E8-49CE-B804-2568B8837B81" );
            public static Guid AlishaMarbleStepConfirmationGuid = new Guid( "6B3037D8-23AC-478C-BCC9-7483B86976AA" );
            public static Guid AlishaMarbleStepMarriage1Guid = new Guid( "FB2BA405-4233-4E7E-83AB-8A94B5E2A80A" );
            public static Guid AlishaMarbleStepAlphaAttenderGuid = new Guid( "87976C94-C9E4-4839-9EFF-1E0B3F4EF5A0" );
            
            public static Guid SarahSimmonsStepConfirmationGuid = new Guid( "E41A74DE-72C7-40B5-B05C-C1A72E09F5C0" );
            public static Guid SarahSimmonsStepMarriage1Guid = new Guid( "94342ACA-744B-41A0-9076-0BCB808514CF" );
            public static Guid SarahSimmonsStepAlphaAttenderGuid = new Guid( "B5907E4F-6197-4A9F-A1DB-046DE7BB2035" );
            
            public static Guid BrianJonesStepBaptismGuid = new Guid( "92171B1E-25C8-408B-AA93-AB7263D02B5A" );
            
            public static Guid BenJonesStepAlphaAttenderGuid = new Guid( "D5DDBBE4-9D62-4EDE-840D-E9DAB8F99430" );

            public static string ColorCodeRed = "#f00";
            public static string ColorCodeGreen = "#0f0";
            public static string ColorCodeBlue = "#00f";

            // A DataView that returns all of the people who have been baptised in 2001.
            public static Guid DataViewStepsCompleted2001Guid = new Guid( "E52D89CD-19A0-44B3-8052-D4FD206AB499" );

            public static class DataSets
            {
                public static string DataSetStepPrograms = "StepPrograms";
                public static string DataSetStepParticipations = "StepParticipations";
                public static string DataSetStepDataViews = "StepDataViews";
            }
        }

        /// <summary>
        /// Remove all Steps test data from the current database.
        /// </summary>
        public static void RemoveStepsFeatureTestData()
        {
            //var monitor = GetCurrentTaskActivity();

            //monitor.LogInformation( "Removing existing Steps Feature test data..." );

            var dataContext = new RockContext();

            // Use SQL delete for efficiency.
            int recordsAffected;

            var db = dataContext.Database;

            recordsAffected = db.ExecuteSqlCommand( $"update [Report] set [DataViewId] = null where [DataViewId] IN ( select [Id] from [DataView] where ForeignKey = '{_SampleDataForeignKey}' )" );

            //monitor.LogInformation( $"Modified {recordsAffected} Reports." );

            recordsAffected = db.ExecuteSqlCommand( $"update [DataView] set [CategoryId] = null where [CategoryId] IN ( select [Id] from [Category] where ForeignKey = '{_SampleDataForeignKey}' )" );
            recordsAffected += db.ExecuteSqlCommand( $"delete from [DataView] where ForeignKey = '{_SampleDataForeignKey}'" );
            
            //monitor.LogInformation( $"Deleted {recordsAffected} Data Views." );

            recordsAffected = db.ExecuteSqlCommand( $"delete from [StepWorkflowTrigger] where [StepTypeId] IN ( select [Id] from [StepType] where ForeignKey = '{_SampleDataForeignKey}' )" );
            recordsAffected += db.ExecuteSqlCommand( $"delete from [StepWorkflowTrigger] where [StepProgramId] IN ( select[Id] from [StepProgram] where ForeignKey = '{_SampleDataForeignKey}' )" );

            //monitor.LogInformation( $"Deleted {recordsAffected} Step Workflow Triggers." );

            recordsAffected = db.ExecuteSqlCommand( $"delete from [StepType] where ForeignKey = '{_SampleDataForeignKey}'" );

            //monitor.LogInformation( $"Deleted {recordsAffected} Step Types." );

            recordsAffected = db.ExecuteSqlCommand( $"delete from [Step] where ForeignKey = '{_SampleDataForeignKey}'" );

            //monitor.LogInformation( $"Deleted {recordsAffected} Steps." );

            recordsAffected = db.ExecuteSqlCommand( $"delete from [StepProgram] where ForeignKey = '{_SampleDataForeignKey}'" );

            //monitor.LogInformation( $"Deleted {recordsAffected} Step Programs." );

            // Remove Categories associated with Steps data.

            recordsAffected = db.ExecuteSqlCommand( $"delete from [Category] where ForeignKey = '{_SampleDataForeignKey}'" );

            //monitor.LogInformation( $"Deleted {recordsAffected} Categories." );

            //monitor.LogActionSuccess( $"Steps Feature test data removed." );

            _loadedFeatureDataSets.Clear();
        }

        public static void AddSampleTestData()
        {
            RemoveStepsFeatureTestData();

            AddTestDataSet( StepsFeatureTestDataSetSpecifier.StepPrograms );
            AddTestDataSet( StepsFeatureTestDataSetSpecifier.StepParticipations );
            AddTestDataSet( StepsFeatureTestDataSetSpecifier.DataViews );
        }

        public static void AddTestDataSet( StepsFeatureTestDataSetSpecifier dataset )
        {
            if ( dataset == StepsFeatureTestDataSetSpecifier.StepPrograms )
            {
                AddTestDataStepPrograms();
            }
            else if ( dataset == StepsFeatureTestDataSetSpecifier.StepParticipations )
            {
                AddKnownStepParticipations();
            }
            else if ( dataset == StepsFeatureTestDataSetSpecifier.DataViews )
            {
                AddStepDataViews();
            }
            else
            {
                throw new Exception( "Unknown Data Set requested." );
            }
        }


        /// <summary>
        /// Add a complete set of Steps test data to the current database.
        /// Existing Steps test data will be removed.
        /// </summary>
        public static void AddStepsFeatureTestData()
        {
            //var monitor = GetCurrentTaskActivity();

            RemoveStepsFeatureTestData();

            AddTestDataStepPrograms();

            AddKnownStepParticipations();

            
        }

        private static bool GetFeatureDataLoadState( string datasetName )
        {
            return _loadedFeatureDataSets.Contains( datasetName );
        }

        private static void SetFeatureDataLoadState( string datasetName, bool isLoaded = true )
        {
            if ( _loadedFeatureDataSets.Contains(datasetName) )
            {
                if ( !isLoaded )
                {
                    _loadedFeatureDataSets.Remove( datasetName );
                }
            }
            else
            {
                if (isLoaded)
                {
                    _loadedFeatureDataSets.Add( datasetName );
                }
            }
        }

        /// <summary>
        /// Adds a set of Step Programs to the current database that are required for integration testing.
        /// This function does not need to be executed separately - it is executed as part of the AddTestDataToCurrentDatabase() function.
        /// </summary>
        public static void AddTestDataStepPrograms()
        {
            if ( GetFeatureDataLoadState( Constants.DataSets.DataSetStepPrograms ) )
            {
                return;
            }

            //var monitor = GetCurrentTaskActivity();

            var dataContext = new RockContext();

            // Add Step Categories
            //monitor.LogInformation( "Adding Step Categories..." );

            //var categoryService = new CategoryService( dataContext );

            var categoryId = EntityTypeCache.Get( typeof( Rock.Model.StepProgram ) ).Id;

            var adultCategory = CreateCategory( "Adult", Constants.CategoryAdultsGuid, categoryId, 1 );
            var childCategory = CreateCategory( "Youth", Constants.CategoryYouthGuid, categoryId, 2 );

            AddOrUpdateCategory( dataContext, adultCategory );
            AddOrUpdateCategory( dataContext, childCategory );

            //categoryService.Add( adultCategory );
            //categoryService.Add( childCategory );

            dataContext.SaveChanges();

            CategoryCache.Clear();
            CategoryCache.All();

            var stepProgramService = new StepProgramService( dataContext );

            // Add Step Program "Sacraments"
            //monitor.LogInformation( "Adding Step Program: Sacraments..." );

            var programSacraments = CreateStepProgram( Constants.ProgramSacramentsGuid, Constants.CategoryAdultsGuid, "Sacraments", "The sacraments represent significant milestones in the Christian faith journey.", "fa fa-bible", 1 );

            stepProgramService.Add( programSacraments );

            AddStepStatusToStepProgram( programSacraments, Constants.StatusSacramentsSuccessGuid, "Success", true, Constants.ColorCodeGreen, 1 );
            AddStepStatusToStepProgram( programSacraments, Constants.StatusSacramentsPendingGuid, "Pending", false, Constants.ColorCodeBlue, 2 );
            AddStepStatusToStepProgram( programSacraments, Constants.StatusSacramentsIncompleteGuid, "Incomplete", false, Constants.ColorCodeRed, 3 );

            AddStepTypeToStepProgram( programSacraments, Constants.StepTypeBaptismGuid, "Baptism", "fa fa-tint", 1 );

            var confirmationStepType = AddStepTypeToStepProgram( programSacraments, Constants.StepTypeConfirmationGuid, "Confirmation", "fa fa-bible", 2 );

            AddStepTypeToStepProgram( programSacraments, Constants.StepTypeEucharistGuid, "Eucharist", "fa fa-cookie", 3 );
            AddStepTypeToStepProgram( programSacraments, Constants.StepTypeConfessionGuid, "Confession", "fa fa-praying-hands", 4 );
            AddStepTypeToStepProgram( programSacraments, Constants.StepTypeAnnointingGuid, "Annointing of the Sick", "fa fa-comment-medical", 5 );
            AddStepTypeToStepProgram( programSacraments, Constants.StepTypeMarriageGuid, "Marriage", "fa fa-ring", 6 );

            var holyOrdersStepType = AddStepTypeToStepProgram( programSacraments, Constants.StepTypeHolyOrdersGuid, "Holy Orders", "fa fa-cross", 7 );

            dataContext.SaveChanges();

            // Add prerequisites
            var prerequisiteService = new StepTypePrerequisiteService( dataContext );

            var stepPrerequisite = new StepTypePrerequisite();

            stepPrerequisite.Guid = Constants.PrerequisiteHolyOrdersGuid;
            stepPrerequisite.StepTypeId = holyOrdersStepType.Id;
            stepPrerequisite.PrerequisiteStepTypeId = confirmationStepType.Id;

            prerequisiteService.Add( stepPrerequisite );

            dataContext.SaveChanges();

            // Add Step Program "Alpha"
            //monitor.LogInformation( "Adding Step Program: Alpha..." );

            var programAlpha = CreateStepProgram( Constants.ProgramAlphaGuid, Constants.CategoryAdultsGuid, "Alpha", "Alpha is a series of interactive sessions that freely explore the basics of the Christian faith.", "fa fa-question", 2 );

            stepProgramService.Add( programAlpha );

            AddStepStatusToStepProgram( programAlpha, Constants.StatusAlphaCompletedGuid, "Completed", true, Constants.ColorCodeGreen, 1 );
            AddStepStatusToStepProgram( programAlpha, Constants.StatusAlphaStartedGuid, "Started", false, Constants.ColorCodeBlue, 2 );

            AddStepTypeToStepProgram( programAlpha, Constants.StepTypeAttenderGuid, "Attender", "fa fa-user", 1, hasEndDate: true );
            AddStepTypeToStepProgram( programAlpha, Constants.StepTypeVolunteerGuid, "Volunteer", "fa fa-hand-paper", 2, hasEndDate: true );
            AddStepTypeToStepProgram( programAlpha, Constants.StepTypeLeaderGuid, "Leader", "fa fa-user-graduate", 3, hasEndDate: true );

            dataContext.SaveChanges();

            SetFeatureDataLoadState( Constants.DataSets.DataSetStepPrograms );

            //monitor.LogActionSuccess( "Step Programs added." );
        }

        public static void AddKnownStepParticipations()
        {
            if ( GetFeatureDataLoadState( Constants.DataSets.DataSetStepParticipations ) )
            {
                return;
            }

            //var monitor = GetCurrentTaskActivity();

            var dataContext = new RockContext();

            var stepService = new StepService( dataContext );
            var personLookup = new PersonLookup( dataContext );

            // Baptism
            //monitor.LogInformation( "Adding Steps for Baptism..." );

            Step baptism;

            baptism = CreateWellKnownStep( Constants.TedDeckerStepBaptismGuid,
                Constants.StepTypeBaptismGuid,
                personLookup.GetPersonAliasId( PersonLookup.TedDeckerPersonGuid ),
                Constants.StatusSacramentsSuccessGuid,
                new DateTime( 2001, 4, 13 ) );
            stepService.Add( baptism );

            baptism = CreateWellKnownStep( Constants.BillMarbleStepBaptismGuid,
                Constants.StepTypeBaptismGuid,
                personLookup.GetPersonAliasId( PersonLookup.BillMarblePersonGuid ),
                Constants.StatusSacramentsSuccessGuid,
                new DateTime( 2001, 10, 2 ) );
            stepService.Add( baptism );

            baptism = CreateWellKnownStep( Constants.AlishaMarbleStepBaptismGuid,
                Constants.StepTypeBaptismGuid,
                personLookup.GetPersonAliasId( PersonLookup.AlishaMarblePersonGuid ),
                Constants.StatusSacramentsSuccessGuid,
                new DateTime( 2001, 10, 2 ) );
            stepService.Add( baptism );

            baptism = CreateWellKnownStep( Constants.BrianJonesStepBaptismGuid,
                Constants.StepTypeBaptismGuid,
                personLookup.GetPersonAliasId( PersonLookup.BrianJonesPersonGuid ),
                Constants.StatusSacramentsPendingGuid,
                new DateTime( 2012, 5, 16 ) );
            stepService.Add( baptism );

            // Confirmation
            //monitor.LogInformation( "Adding Steps for Confirmation..." );

            Step confirmation;

            confirmation = CreateWellKnownStep( Constants.TedDeckerStepConfirmationGuid,
                Constants.StepTypeConfirmationGuid,
                personLookup.GetPersonAliasId( PersonLookup.TedDeckerPersonGuid ),
                Constants.StatusSacramentsSuccessGuid,
                new DateTime( 1996, 10, 25 ) );
            stepService.Add( confirmation );

            confirmation = CreateWellKnownStep( Constants.BillMarbleStepConfirmationGuid,
                Constants.StepTypeConfirmationGuid,
                personLookup.GetPersonAliasId( PersonLookup.BillMarblePersonGuid ),
                Constants.StatusSacramentsSuccessGuid,
                new DateTime( 1999, 08, 04 ) );
            stepService.Add( confirmation );

            confirmation = CreateWellKnownStep( Constants.SarahSimmonsStepConfirmationGuid,
                Constants.StepTypeConfirmationGuid,
                personLookup.GetPersonAliasId( PersonLookup.SarahSimmonsPersonGuid ),
                Constants.StatusSacramentsSuccessGuid,
                new DateTime( 1998, 11, 30 ) );
            stepService.Add( confirmation );

            // Marriage - First
            //monitor.LogInformation( "Adding Steps for Marriage..." );

            Step marriage;

            marriage = CreateWellKnownStep( Constants.TedDeckerStepMarriage1Guid,
                Constants.StepTypeMarriageGuid,
                personLookup.GetPersonAliasId( PersonLookup.TedDeckerPersonGuid ),
                Constants.StatusSacramentsIncompleteGuid,
                new DateTime( 1998, 2, 10 ) );
            stepService.Add( marriage );

            marriage = CreateWellKnownStep( Constants.BillMarbleStepMarriage1Guid,
                Constants.StepTypeMarriageGuid,
                personLookup.GetPersonAliasId( PersonLookup.BillMarblePersonGuid ),
                Constants.StatusSacramentsSuccessGuid,
                new DateTime( 1993, 4, 12 ) );
            stepService.Add( marriage );

            // Marriage - Second
            var marriage2 = CreateWellKnownStep( Constants.TedDeckerStepMarriage2Guid,
                Constants.StepTypeMarriageGuid,
                personLookup.GetPersonAliasId( PersonLookup.TedDeckerPersonGuid ),
                Constants.StatusSacramentsSuccessGuid,
                new DateTime( 2001, 4, 1 ) );
            stepService.Add( marriage2 );

            // Alpha/Attender
            //monitor.LogInformation( "Adding Steps for Alpha..." );

            var alphaAttender = CreateWellKnownStep( Constants.BenJonesStepAlphaAttenderGuid,
                Constants.StepTypeAttenderGuid,
                personLookup.GetPersonAliasId( PersonLookup.BenJonesPersonGuid ),
                Constants.StatusAlphaStartedGuid,
                new DateTime( 2015, 10, 13 ) );
            stepService.Add( alphaAttender );

            dataContext.SaveChanges();

            //monitor.LogActionSuccess( "Steps added." );

            SetFeatureDataLoadState( Constants.DataSets.DataSetStepParticipations );
        }

        public static void AddStepDataViews()
        {
            //var monitor = GetCurrentTaskActivity();

            //monitor.LogInformation( "Adding Steps Data Views..." );

            var dataContext = new RockContext();

            // Add Data View Category "Steps".
            const string categoryDataViewStepsName = "Steps";

            Debug.Print( $"Adding Data View Category \"{ categoryDataViewStepsName }\"..." );

            var entityTypeId = EntityTypeCache.Get( typeof( Rock.Model.DataView ) ).Id;

            var stepsCategory = CreateCategory( categoryDataViewStepsName, Constants.CategoryStepsGuid, entityTypeId );

            AddOrUpdateCategory( dataContext, stepsCategory );

            dataContext.SaveChanges();

            // Create Data View: Steps Completed
            dataContext = new RockContext();

            const string dataViewStepsCompleted2001Name = "Steps Completed in 2001";

            Debug.Print( $"Adding Data View \"{ dataViewStepsCompleted2001Name }\"..." );

            DataView dataView = new DataView();

            dataView.IsSystem = false;

            var service = new DataViewService( dataContext );

            int categoryId = CategoryCache.GetId( Constants.CategoryStepsGuid ) ?? 0;

            dataView.Name = dataViewStepsCompleted2001Name;
            dataView.Description = "Steps that have a completion in the year 2001.";
            dataView.EntityTypeId = EntityTypeCache.GetId( typeof( Rock.Model.Step ) );
            dataView.CategoryId = categoryId;
            dataView.Guid = Constants.DataViewStepsCompleted2001Guid;
            dataView.ForeignKey = _SampleDataForeignKey;

            service.Add( dataView );

            dataContext.SaveChanges();

            var rootFilter = new DataViewFilter();

            rootFilter.ExpressionType = FilterExpressionType.GroupAll;

            dataView.DataViewFilter = rootFilter;

            dataContext.SaveChanges();

            // Add filter for Step Type
            var dateCompletedFilter = new DataViewFilter();

            dateCompletedFilter.ExpressionType = FilterExpressionType.Filter;
            dateCompletedFilter.EntityTypeId = EntityTypeCache.GetId( typeof( Rock.Reporting.DataFilter.PropertyFilter ) );

            var dateFilterSettings = new List<string> { "Property_CompletedDateTime", "4096", "\tDateRange|||1/01/2001 12:00:00 AM|12/31/2001 12:00:00 AM" };

            dateCompletedFilter.Selection = dateFilterSettings.ToJson();

            rootFilter.ChildFilters.Add( dateCompletedFilter );

            dataContext.SaveChanges();

            //monitor.LogActionSuccess( "Steps Data Views added." );
        }

        public static void AddStepsRandomParticipationEntries()
        {
            //var monitor = GetCurrentTaskActivity();

            //monitor.LogInformation( "Adding random Steps..." );

            // Get a complete set of active Step Types ordered by Program and structure order.
            var dataContext = new RockContext();

            var programService = new StepProgramService( dataContext );

            var programIdList = programService.Queryable().Where( x => x.StepTypes.Any() ).OrderBy( x => x.Order ).Select( x => x.Id ).ToList();

            var statusService = new StepStatusService( dataContext );

            var statuses = statusService.Queryable().ToList();

            // Get a random selection of people that are not system users or specific users for which test data already exists.
            var personService = new PersonService( dataContext );

            int tedPersonAliasId = 0;

            var testPeopleIdList = new List<int> { tedPersonAliasId };

            var personQuery = personService.Queryable().Where( x => !x.IsSystem && x.LastName != "Anonymous" && !testPeopleIdList.Contains( x.Id ) ).Select( x => x.Id );

            var personAliasService = new PersonAliasService( dataContext );

            var personAliasIdList = personAliasService.Queryable().Where( x => personQuery.Contains( x.PersonId ) ).Select( x => x.Id ).ToList();

            var personAliasIdQueue = new Queue<int>( personAliasIdList.GetRandomizedList( _MaxPersonCount ) );

            int stepCounter = 0;
            int personAliasId = 0;
            int stepProgramId = 0;
            DateTime startDateTime = RockDateTime.Now;
            DateTime newStepDateTime;
            int campusId;
            int maxStepTypeCount;
            int stepsToAddCount;
            int offsetDays;
            int personCounter = 0;
            bool isCompleted;

            // Loop through the set of people, adding at least 1 program and 1 step for each person.
            var rng = new Random();

            var typeService = new StepTypeService( dataContext );

            var stepTypesAll = typeService.Queryable().ToList();

            var campusList = CampusCache.All();

            StepService stepService = null;

            while ( personAliasIdQueue.Any() )
            {
                personAliasId = personAliasIdQueue.Dequeue();

                personCounter += 1;

                // Randomly select the Programs that this person will participate in.
                var addProgramCount = rng.Next( 1, programIdList.Count + 1 );

                var programsToAdd = new Queue<int>( programIdList.GetRandomizedList( addProgramCount ) );

                while ( programsToAdd.Any() )
                {
                    stepProgramId = programsToAdd.Dequeue();

                    newStepDateTime = startDateTime;

                    // Get a random campus at which the step was completed.
                    campusId = campusList.GetRandomElement().Id;

                    var stepStatuses = statusService.Queryable().Where( x => x.StepProgramId == stepProgramId ).ToList();

                    maxStepTypeCount = stepTypesAll.Count( x => x.StepProgramId == stepProgramId );

                    // Randomly select a number of Steps that this person will achieve in the Program, in Step order.
                    // This creates a distribution weighted toward achievement of earlier Steps, which is the likely scenario for most Programs.
                    // Steps are added from last to first in reverse chronological order, with the last step being achieved in the current year.
                    stepsToAddCount = rng.Next( 1, maxStepTypeCount );

                    //monitor.LogDebug( $"Adding Steps: PersonAliasId: {personAliasId}, ProgramId={stepProgramId}, Steps={stepsToAddCount}" );

                    var stepTypesToAdd = new Queue<StepType>( stepTypesAll.Take( stepsToAddCount ).Reverse() );

                    while ( stepTypesToAdd.Any() )
                    {
                        var stepTypeToAdd = stepTypesToAdd.Dequeue();

                        // If this is not the last step to be added for this person, make sure the status represents a completion.
                        if ( stepTypesToAdd.Any() )
                        {
                            isCompleted = true;
                        }
                        else
                        {
                            isCompleted = rng.Next( 1, 100 ) <= _PercentChanceOfLastStepCompletion;
                        }

                        var eligibleStatuses = stepStatuses.Where( x => x.IsCompleteStatus == isCompleted ).ToList();

                        // If there is no status that represents completion, allow any status.
                        if ( eligibleStatuses.Count == 0 )
                        {
                            eligibleStatuses = stepStatuses;
                        }

                        var newStatus = eligibleStatuses.GetRandomElement();

                        // Subtract a random number of days from the current step date to get a suitable date for the preceding step in the program.
                        offsetDays = rng.Next( 1, _MaxDaysBetweenSteps );

                        newStepDateTime = newStepDateTime.AddDays( -1 * offsetDays );

                        var newStep = new Step();

                        newStep.StepTypeId = stepTypeToAdd.Id;

                        if ( newStatus != null )
                        {
                            newStep.StepStatusId = newStatus.Id;
                        }

                        newStep.PersonAliasId = personAliasId;
                        newStep.CampusId = campusId;
                        newStep.StartDateTime = newStepDateTime;

                        if ( isCompleted )
                        {
                            newStep.CompletedDateTime = newStepDateTime;
                        }

                        newStep.ForeignKey = _SampleDataForeignKey;

                        if ( stepService == null )
                        {
                            var stepDataContext = new RockContext();

                            stepService = new StepService( stepDataContext );
                        }

                        stepService.Add( newStep );

                        // Save a batch of records and recycle the context to speed up processing.
                        if ( stepCounter % 100 == 0 )
                        {
                            stepService.Context.SaveChanges();

                            stepService = null;
                        }

                        stepCounter++;
                    }
                }
            }

            //monitor.LogActionSuccess( $"Created {stepCounter} Steps for {personCounter} people." );
        }

        #region Internal Methods

        private static Step CreateWellKnownStep( Guid stepGuid, Guid stepTypeGuid, int personAliasId, Guid stepStatusGuid, DateTime? completedDateTime )
        {
            var newStep = new Step();

            newStep.Guid = stepGuid;

            newStep.StepTypeId = GetIdFromWellKnownGuidOrThrow( stepTypeGuid );
            newStep.PersonAliasId = personAliasId;
            newStep.StepStatusId = GetIdFromWellKnownGuidOrThrow( stepStatusGuid );

            newStep.CompletedDateTime = completedDateTime;

            newStep.ForeignKey = _SampleDataForeignKey;

            return newStep;
        }

        private static Dictionary<Guid, int> _GuidToIdMap = null;

        private static int GetIdFromWellKnownGuidOrThrow( Guid guid )
        {
            if ( _GuidToIdMap == null )
            {
                CreateWellKnownGuidToIdMap();
            }

            if ( _GuidToIdMap.ContainsKey( guid ) )
            {
                return _GuidToIdMap[guid];
            }

            throw new Exception( $"Guid \"{guid}\" could not be resolved to a known entity Id." );
        }

        private static void CreateWellKnownGuidToIdMap()
        {
            _GuidToIdMap = new Dictionary<Guid, int>();

            Dictionary<Guid, int> guidDictionary;

            var dataContext = new RockContext();

            // Add Step Types
            var stepTypeService = new StepTypeService( dataContext );

            guidDictionary = stepTypeService.Queryable().ToDictionary( k => k.Guid, v => v.Id );

            _GuidToIdMap = _GuidToIdMap.Union( guidDictionary ).ToDictionary( k => k.Key, v => v.Value );

            // Add Step Statuses
            var stepStatusService = new StepStatusService( dataContext );

            guidDictionary = stepStatusService.Queryable().ToDictionary( k => k.Guid, v => v.Id );

            _GuidToIdMap = _GuidToIdMap.Union( guidDictionary ).ToDictionary( k => k.Key, v => v.Value );

            // Add Person Aliases - Map Person Guid to PersonAlias.Id
            //var personAliasService = new PersonAliasService( dataContext );

            //var personService = new PersonService( dataContext );

            //var personKnownGuids = new List<Guid>();

            //personKnownGuids.Add( PersonLookup.AlishaMarblePersonGuid );
            //personKnownGuids.Add( Constants.BenJonesPersonGuid );
            //personKnownGuids.Add( Constants.BillMarblePersonGuid );
            //personKnownGuids.Add( Constants.BrianJonesPersonGuid );
            //personKnownGuids.Add( Constants.SarahSimmonsPersonGuid );
            //personKnownGuids.Add( Constants.TedDeckerPersonGuid );

            //var knownPeople = personService.Queryable().Where( x => personKnownGuids.Contains( x.Guid ) );

            //foreach ( var knownPerson in knownPeople )
            //{
            //    _GuidToIdMap.Add( knownPerson.Guid, knownPerson.PrimaryAliasId ?? 0 );
            //}
        }

        private static Category CreateCategory( string name, Guid guid, int appliesToEntityTypeId, int order = 0 )
        {
            var newCategory = new Category();

            newCategory.Name = name;
            newCategory.Guid = guid;
            newCategory.IsSystem = true;
            newCategory.EntityTypeId = appliesToEntityTypeId;
            newCategory.Order = order;

            newCategory.ForeignKey = _SampleDataForeignKey;

            return newCategory;
        }

        private static void AddOrUpdateCategory( RockContext dataContext, Category category )
        {
            var categoryService = new CategoryService( dataContext );

            var existingCategory = categoryService.Queryable().FirstOrDefault( x => x.Guid == category.Guid );

            if ( existingCategory == null )
            {
                categoryService.Add( category );

                existingCategory = category;
            }
            else
            {
                var id = existingCategory.Id;

                existingCategory.CopyPropertiesFrom( category );

                existingCategory.Id = id;
            }
        }

        private static StepProgram CreateStepProgram( Guid guid, Guid categoryGuid, string name, string description, string iconCssClass, int order )
        {
            var newProgram = new StepProgram();

            var categoryId = CategoryCache.Get( categoryGuid ).Id;

            newProgram.Guid = guid;
            newProgram.Name = name;
            newProgram.Description = description;
            newProgram.CategoryId = categoryId;
            newProgram.Order = order;
            newProgram.IconCssClass = iconCssClass;

            newProgram.ForeignKey = _SampleDataForeignKey;

            return newProgram;
        }

        private static StepType AddStepTypeToStepProgram( StepProgram program, Guid guid, string name, string iconCssClass, int order, bool hasEndDate = false, string description = null )
        {
            var newType = new StepType();

            newType.Guid = guid;
            newType.Name = name;
            newType.Description = description;
            newType.IconCssClass = iconCssClass;
            newType.HasEndDate = hasEndDate;
            newType.IsActive = true;
            newType.Order = order;
            newType.ForeignKey = _SampleDataForeignKey;

            program.StepTypes.Add( newType );

            return newType;
        }

        private static StepStatus AddStepStatusToStepProgram( StepProgram program, Guid guid, string name, bool isCompleteStatus, string colorCode, int order )
        {
            var newStatus = new StepStatus();

            newStatus.Guid = guid;
            newStatus.Name = name;
            newStatus.IsActive = true;
            newStatus.Order = order;
            newStatus.IsCompleteStatus = isCompleteStatus;
            newStatus.StatusColor = colorCode;
            newStatus.ForeignKey = _SampleDataForeignKey;

            program.StepStatuses.Add( newStatus );

            return newStatus;
        }

        #endregion
    }

 //   #endregion
}