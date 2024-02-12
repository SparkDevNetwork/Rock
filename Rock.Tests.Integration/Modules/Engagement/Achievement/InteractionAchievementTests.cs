using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Achievement;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Engagement.Achievements
{
    /// <summary>
    /// Tests for Interaction Achievements that use the database
    /// </summary>
    [TestClass]
    [Ignore( "Broken test. Interaction component for Internal website does not exist in sample data?" )]
    public class InteractionAchievementTests : DatabaseTestsBase
    {
        private const string ComponentEntityTypeName = "Rock.Achievement.Component.InteractionSourcedAccumulativeAchievement";

        private static RockContext _rockContext { get; set; }
        private static AchievementTypeService _achievementTypeService { get; set; }
        private static InteractionService _interactionService { get; set; }

        private static List<int> _personAliasIds { get; set; }

        private static int _achievementTypeId { get; set; }
        private static InteractionComponentCache _interactionComponent { get; set; }

        private const int NUMBER_OF_ALIASES = 5;
        private const int NUMBER_TO_ACHIEVE = 350;
        private const int COUNT = 321;
        private const string KEY = "InteractioneAchievementTests Interaction";

        #region Setup Methods

        /// <summary>
        /// Creates the person alias data.
        /// </summary>
        private static void CreatePersonAliasData()
        {
            var tedDeckerGuid = TestGuids.TestPeople.TedDecker.AsGuid();

            var personAliasService = new PersonAliasService( _rockContext );
            var personService = new PersonService( _rockContext );
            var tedDecker = personService.Get( tedDeckerGuid );
            var aliasPersonId = personService.Queryable().Max( p => p.Id ) + 1;
            personAliasService.DeleteRange( personAliasService.Queryable().Where( pa => pa.AliasPersonId >= aliasPersonId ) );
            _rockContext.SaveChanges();

            for ( var i = 0; i < NUMBER_OF_ALIASES; i++ )
            {
                personAliasService.Add( new PersonAlias { Person = tedDecker, AliasPersonGuid = Guid.NewGuid(), AliasPersonId = aliasPersonId + i, ForeignKey = KEY } );
            }

            _rockContext.SaveChanges();

            var personAlias = personAliasService.Queryable().Where( pa => pa.Person.Guid == tedDeckerGuid && pa.ForeignKey == KEY ).Take( NUMBER_OF_ALIASES ).Select( p => p.Id ).ToList();
            _personAliasIds = personAlias;
        }

        /// <summary>
        /// Create interaction data
        /// </summary>
        private static void CreateInteractionData()
        {
            _interactionComponent = InteractionComponentCache.All().FirstOrDefault( i => i.InteractionChannel.Guid == SystemGuid.InteractionChannel.ROCK_RMS.AsGuid() );

            Assert.IsNotNull( _interactionComponent, "Interaction component for Internal website not found." );

            var existing = _interactionService.Queryable().Where( i =>
                _personAliasIds.Contains( i.PersonAliasId.Value) &&
                i.InteractionComponentId == _interactionComponent.Id
            );

            _interactionService.DeleteRange( existing );

            for ( var i = 0; i < COUNT; i++ )
            {
                _interactionService.Add( new Interaction
                {
                    InteractionComponentId = _interactionComponent.Id,
                    PersonAliasId = _personAliasIds[i % NUMBER_OF_ALIASES],
                    InteractionDateTime = RockDateTime.Today,
                    ForeignKey = KEY,
                    ForeignId = i
                } );
            }

            _rockContext.SaveChanges( true );
        }

        /// <summary>
        /// Delete the data created by this test class
        /// </summary>
        private static void DeleteTestData()
        {
            var interactionQuery = _interactionService.Queryable().Where( i => i.ForeignKey == KEY );
            _interactionService.DeleteRange( interactionQuery );

            var achievementTypeQuery = _achievementTypeService.Queryable().Where( at => at.Id == _achievementTypeId );
            _achievementTypeService.DeleteRange( achievementTypeQuery );

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
                ComponentConfigJson = "{ \"InteractionChannelComponent\": \"" +
                    _interactionComponent.InteractionChannel.Guid + "|" +
                    _interactionComponent.Guid + "\" }"
            };

            _achievementTypeService.Add( achievementType );
            _rockContext.SaveChanges( true );

            achievementType.LoadAttributes();
            achievementType.SetAttributeValue( Rock.Achievement.Component.InteractionSourcedAccumulativeAchievement.AttributeKey.NumberToAccumulate, NUMBER_TO_ACHIEVE.ToString() );
            achievementType.SaveAttributeValues();

            _achievementTypeId = achievementType.Id;
        }

        /// <summary>
        /// Runs before any tests in this class are executed.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
            _rockContext = new RockContext();
            _interactionService = new InteractionService( _rockContext );
            _achievementTypeService = new AchievementTypeService( _rockContext );

            DeleteTestData();
            CreatePersonAliasData();
            CreateInteractionData();
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
            _interactionService = null;
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
        /// Tests AccumulativeAchievementProcess
        /// </summary>
        [TestMethod]
        public void AccumulativeAchievementProcess()
        {
            var attemptsQuery = new AchievementAttemptService( _rockContext ).Queryable()
                .AsNoTracking()
                .Where( saa => saa.AchievementTypeId == _achievementTypeId &&  _personAliasIds.Contains( saa.AchieverEntityId) )
                .OrderBy( saa => saa.AchievementAttemptStartDateTime );

            // There should be no attempts
            Assert.That.AreEqual( 0, attemptsQuery.Count() );

            var achievementTypeCache = AchievementTypeCache.Get( _achievementTypeId );
            var interaction = new InteractionService( _rockContext ).Queryable().FirstOrDefault( i => i.ForeignKey == KEY );
            var component = AchievementContainer.GetComponent( ComponentEntityTypeName );

            component.Process( _rockContext, achievementTypeCache, interaction );
            _rockContext.SaveChanges();

            var attempts = attemptsQuery.ToList();
            Assert.That.IsNotNull( attempts );
            Assert.That.AreEqual( 1, attempts.Count );

            // The database stores progress with only 9 digits beyond the decimal
            var progress = decimal.Divide( COUNT, NUMBER_TO_ACHIEVE );
            var progressDifference = Math.Abs( progress - attempts[0].Progress );

            Assert.That.AreEqual( RockDateTime.Today, attempts[0].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( RockDateTime.Today, attempts[0].AchievementAttemptEndDateTime );
            Assert.That.IsTrue( progressDifference < .000000001m );
            Assert.That.IsFalse( attempts[0].IsClosed );
            Assert.That.IsFalse( attempts[0].IsSuccessful );
        }
    }
}
