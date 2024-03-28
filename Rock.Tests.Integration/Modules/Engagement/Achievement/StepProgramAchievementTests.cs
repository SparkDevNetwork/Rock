using System;
using System.Data.Entity;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Achievement;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Engagement.Steps
{
    /// <summary>
    /// Tests for Step Program Achievements that use the database
    /// </summary>
    [TestClass]
    public class StepProgramAchievementTests : DatabaseTestsBase
    {
        private const string ComponentEntityTypeName = "Rock.Achievement.Component.StepProgramAchievement";

        private static RockContext _rockContext { get; set; }
        private static AchievementTypeService _achievementTypeService { get; set; }
        private static StepProgramService _stepProgramService { get; set; }

        private static int _personAliasId { get; set; }
        private static int _achievementTypeId { get; set; }

        private const int STEP_TYPE_COUNT = 350;
        private const int COMPLETE_COUNT = 321;
        private const string KEY = "StepProgramAchievementTests";
        private const string StepProgramGuidString = "82A41CAA-82FC-4EBE-9AB8-8BB8BADFB2F0";

        #region Setup Methods

        /// <summary>
        /// Create step program data
        /// </summary>
        private static void CreateStepProgramData()
        {
            var tedDeckerGuid = TestGuids.TestPeople.TedDecker.AsGuid();
            var personAlias = new PersonAliasService( _rockContext ).Queryable().First( pa => pa.Person.Guid == tedDeckerGuid );
            _personAliasId = personAlias.Id;

            var stepProgram = new StepProgram
            {
                Guid = new Guid( StepProgramGuidString ),
                Name = "Testing Program",
                ForeignKey = KEY
            };

            for ( var i = 0; i < STEP_TYPE_COUNT; i++ )
            {
                var stepType = new StepType
                {
                    Name = $"Step Type {i}",
                    ForeignKey = KEY
                };

                if ( i < COMPLETE_COUNT )
                {
                    stepType.Steps.Add( new Step
                    {
                        StartDateTime = RockDateTime.Today.AddDays( -1 ),
                        CompletedDateTime = RockDateTime.Today,
                        PersonAliasId = _personAliasId,
                        ForeignKey = KEY
                    } );
                }

                stepProgram.StepTypes.Add( stepType );
            }

            _stepProgramService.Add( stepProgram );
            _rockContext.SaveChanges( true );
        }

        /// <summary>
        /// Delete the data created by this test class
        /// </summary>
        private static void DeleteTestData()
        {
            var stepProgramGuid = StepProgramGuidString.AsGuid();
            var query = _stepProgramService.Queryable().Where( sp => sp.Guid == stepProgramGuid );
            _stepProgramService.DeleteRange( query );
            _rockContext.SaveChanges();
        }

        /// <summary>
        /// Creates the achievement type data.
        /// </summary>
        private static void CreateAchievementTypeData()
        {
            // Create the component so it sets up the attributes.
            AchievementContainer.Instance.Refresh();
            _ = AchievementContainer.GetComponent( ComponentEntityTypeName );

            var achievementType = new AchievementType
            {
                Name = "Test Achievement",
                IsActive = true,
                ComponentEntityTypeId = EntityTypeCache.GetId( ComponentEntityTypeName ) ?? 0,
                MaxAccomplishmentsAllowed = 1,
                AllowOverAchievement = false,
                ComponentConfigJson = "{ \"StepProgram\": \"" +
                    StepProgramGuidString + "\" }"
            };

            _achievementTypeService.Add( achievementType );
            _rockContext.SaveChanges( true );

            _achievementTypeId = achievementType.Id;
        }

        /// <summary>
        /// Runs before any tests in this class are executed.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
            _rockContext = new RockContext();
            _stepProgramService = new StepProgramService( _rockContext );
            _achievementTypeService = new AchievementTypeService( _rockContext );

            DeleteTestData();
            CreateStepProgramData();
            CreateAchievementTypeData();
        }

        /// <summary>
        /// Runs after all tests in this class is executed.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            DeleteTestData();
            _rockContext = null;
            _stepProgramService = null;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            var service = new AchievementAttemptService( _rockContext );
            service.DeleteRange( service.Queryable().Where( saa => saa.AchievementTypeId == _achievementTypeId ) );
            _rockContext.SaveChanges();
        }

        #endregion Setup Methods

        /// <summary>
        /// Tests Step Program
        /// </summary>
        [TestMethod]
        public void StepProgramProcess()
        {
            var attemptsQuery = new AchievementAttemptService( _rockContext ).Queryable()
                .AsNoTracking()
                .Where( saa => saa.AchievementTypeId == _achievementTypeId && saa.AchieverEntityId == _personAliasId )
                .OrderBy( saa => saa.AchievementAttemptStartDateTime );

            // There should be no attempts
            Assert.That.AreEqual( 0, attemptsQuery.Count() );

            var achievementTypeCache = AchievementTypeCache.Get( _achievementTypeId );
            var step = new StepService( _rockContext ).Queryable().FirstOrDefault( i => i.ForeignKey == KEY );
            var component = AchievementContainer.GetComponent( ComponentEntityTypeName );

            component.Process( _rockContext, achievementTypeCache, step );
            _rockContext.SaveChanges();

            var attempts = attemptsQuery.ToList();
            Assert.That.IsNotNull( attempts );
            Assert.That.AreEqual( 1, attempts.Count );

            // The database stores progress with only 2 digits beyond the decimal
            var progress = decimal.Divide( COMPLETE_COUNT, STEP_TYPE_COUNT );
            var progressDifference = Math.Abs( progress - attempts[0].Progress );

            Assert.That.AreEqual( RockDateTime.Today, attempts[0].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( RockDateTime.Today, attempts[0].AchievementAttemptEndDateTime );
            Assert.That.IsTrue( progressDifference < .01m );
            Assert.That.IsFalse( attempts[0].IsClosed );
            Assert.That.IsFalse( attempts[0].IsSuccessful );
        }
    }
}
