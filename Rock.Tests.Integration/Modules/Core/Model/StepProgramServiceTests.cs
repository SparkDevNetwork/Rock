using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Core.Model
{
    /// <summary>
    /// Tests for StepProgramService that use the database
    /// </summary>
    [TestClass]
    public class StepProgramServiceTests : DatabaseTestsBase
    {
        private const string ForeignKey = "_StepProgramServiceTests_F120ABA8-6F8B-4ED1-B856-45196593AB61_";

        private const string JerryJenkinsPersonGuidString = "A8E6242D-B52E-4659-AB13-751A5F4C0BE4";
        private const string KathyKolePersonGuidString = "74BD1D38-D054-488F-86F6-38040242219E";
        private const string BarryBopPersonGuidString = "EFEFD90E-A993-493D-84D8-6903946523DB";
        private const string SimonSandsPersonGuidString = "E2D57C31-89C4-4A92-8917-894B49A42CAE";

        private static readonly Guid[] PersonGuids = new[] {
            JerryJenkinsPersonGuidString.AsGuid(),
            KathyKolePersonGuidString.AsGuid(),
            BarryBopPersonGuidString.AsGuid(),
            SimonSandsPersonGuidString.AsGuid()
        };

        #region Setup Methods

        /// <summary>
        /// Creates the test people.
        /// 3 people are included in a dataview because of their middle name.
        /// 1 person (part of the 3 in the dataview) has multiple person aliases.
        /// </summary>
        private static void CreateTestPeople()
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var personAliasService = new PersonAliasService( rockContext );

            // Create 4 people. 3 are part of a dataview because of their middle name
            var personSimonSands = new Person
            {
                FirstName = "Simon",
                LastName = "Sands",
                Guid = SimonSandsPersonGuidString.AsGuid(),
                ForeignKey = ForeignKey
            };

            var personBarryBop = new Person
            {
                FirstName = "Barry",
                LastName = "Bop",
                Guid = BarryBopPersonGuidString.AsGuid(),
                ForeignKey = ForeignKey
            };

            // Not in the dataview
            var personKathyKole = new Person
            {
                FirstName = "Kathy",
                LastName = "Kole",
                Guid = KathyKolePersonGuidString.AsGuid(),
                ForeignKey = ForeignKey
            };

            var personJerryJenkins = new Person
            {
                FirstName = "Jerry",
                LastName = "Jenkins",
                Guid = JerryJenkinsPersonGuidString.AsGuid(),
                ForeignKey = ForeignKey
            };

            personService.Add( personJerryJenkins );
            personService.Add( personSimonSands );
            personService.Add( personKathyKole );
            personService.Add( personBarryBop );
            rockContext.SaveChanges();

            // Add multiple aliases for Jerry
            personAliasService.Add( new PersonAlias
            {
                ForeignKey = ForeignKey,
                Person = personJerryJenkins,
                AliasPersonId = 5000000
            } );

            personAliasService.Add( new PersonAlias
            {
                ForeignKey = ForeignKey,
                Person = personJerryJenkins,
                AliasPersonId = 5000001
            } );

            personAliasService.Add( new PersonAlias
            {
                ForeignKey = ForeignKey,
                Person = personJerryJenkins,
                AliasPersonId = 5000002
            } );

            personAliasService.Add( new PersonAlias
            {
                ForeignKey = ForeignKey,
                Person = personJerryJenkins,
                AliasPersonId = 5000003
            } );

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Creates the test step program.
        /// There are 2 step statuses: 1 complete and 1 in-progress
        /// There are 4 step types.
        ///
        /// Jerry completed the program with two aliases
        /// Simon started the program
        /// Kathy completed the program 2x
        /// Barry started the program
        /// </summary>
        private static void CreateTestStepProgram()
        {
            var rockContext = new RockContext();
            var stepProgramService = new StepProgramService( rockContext );
            var personService = new PersonService( rockContext );

            var barryAliasId = personService.Get( BarryBopPersonGuidString.AsGuid() ).PrimaryAliasId.Value;
            var kathyAliasId = personService.Get( KathyKolePersonGuidString.AsGuid() ).PrimaryAliasId.Value;
            var simonAliasId = personService.Get( SimonSandsPersonGuidString.AsGuid() ).PrimaryAliasId.Value;

            var jerryAliases = personService.Get( JerryJenkinsPersonGuidString.AsGuid() ).Aliases.ToList();
            var jerryAliasId1 = jerryAliases[0].Id;
            var jerryAliasId2 = jerryAliases[1].Id;

            // TODO: Adding a hierarchy of StepProgram/StepType/Step objects at once doesn't correctly trigger
            // the Step EntitySaveHook that updates the StepProgramCompletion table.
            // Therefore, completed steps added in this way are not recognized as complete.
            // This is a potential issue with any hierarchy of objects, because SaveHooks are only triggered for top-level items.
            // Could fix this by adding a StepType savehook that triggers the Step savehook as needed, but that will only address this specific case.
            var stepProgram = new StepProgram
            {
                Name = "Test",
                ForeignKey = ForeignKey,
                StepStatuses = new List<StepStatus> {
                    new StepStatus
                    {
                        ForeignKey = ForeignKey,
                        Name = "Complete",
                        IsCompleteStatus = true
                    },
                    new StepStatus
                    {
                        ForeignKey = ForeignKey,
                        Name = "In-progress",
                        IsCompleteStatus = false
                    }
                },
                StepTypes = new List<StepType>
                {
                    new StepType
                    {
                        Name = "Test: Step Type 1",
                        ForeignKey = ForeignKey,
                        AllowMultiple = true,
                        Steps = new [] {
                            new Step {
                                PersonAliasId = jerryAliasId2,
                                CompletedDateTime = new DateTime(2019, 2, 1),
                                StartDateTime =  new DateTime(2019, 1, 1),
                                ForeignKey = ForeignKey
                            },
                            new Step {
                                PersonAliasId = simonAliasId,
                                CompletedDateTime = new DateTime(2019, 2, 1),
                                StartDateTime =  new DateTime(2019, 1, 1),
                                ForeignKey = ForeignKey
                            },
                            new Step {
                                PersonAliasId = barryAliasId,
                                CompletedDateTime = new DateTime(2019, 2, 1),
                                StartDateTime =  new DateTime(2019, 1, 1),
                                ForeignKey = ForeignKey
                            },
                            new Step {
                                PersonAliasId = barryAliasId,
                                CompletedDateTime = new DateTime(2019, 2, 1),
                                StartDateTime =  new DateTime(2019, 1, 1),
                                ForeignKey = ForeignKey
                            },
                            new Step {
                                PersonAliasId = kathyAliasId,
                                CompletedDateTime = new DateTime(2020, 2, 1),
                                StartDateTime =  new DateTime(2020, 1, 1),
                                ForeignKey = ForeignKey
                            },
                            new Step {
                                PersonAliasId = kathyAliasId,
                                CompletedDateTime = new DateTime(2019, 2, 1),
                                StartDateTime =  new DateTime(2019, 1, 1),
                                ForeignKey = ForeignKey
                            },
                        }
                    },
                    new StepType
                    {
                        Name = "Test: Step Type 2",
                        ForeignKey = ForeignKey,
                        AllowMultiple = true,
                        Steps = new [] {
                            new Step {
                                PersonAliasId = jerryAliasId1,
                                CompletedDateTime = new DateTime(2020, 3, 1),
                                StartDateTime =  new DateTime(2020, 1, 1),
                                ForeignKey = ForeignKey
                            },
                            new Step {
                                PersonAliasId = kathyAliasId,
                                CompletedDateTime = new DateTime(2020, 2, 1),
                                StartDateTime =  new DateTime(2020, 1, 1),
                                ForeignKey = ForeignKey
                            },
                            new Step {
                                PersonAliasId = kathyAliasId,
                                CompletedDateTime = new DateTime(2019, 2, 1),
                                StartDateTime =  new DateTime(2019, 1, 1),
                                ForeignKey = ForeignKey
                            },
                        }
                    },
                    new StepType
                    {
                        Name = "Test: Step Type 3",
                        ForeignKey = ForeignKey,
                        AllowMultiple = false,
                        Steps = new [] {
                            new Step {
                                PersonAliasId = jerryAliasId1,
                                CompletedDateTime = new DateTime(2020, 2, 1),
                                StartDateTime =  new DateTime(2020, 1, 1),
                                ForeignKey = ForeignKey
                            },
                            new Step {
                                PersonAliasId = kathyAliasId,
                                CompletedDateTime = new DateTime(2020, 2, 1),
                                StartDateTime =  new DateTime(2020, 1, 1),
                                ForeignKey = ForeignKey
                            },
                            new Step {
                                PersonAliasId = kathyAliasId,
                                CompletedDateTime = new DateTime(2019, 2, 1),
                                StartDateTime =  new DateTime(2019, 1, 1),
                                ForeignKey = ForeignKey
                            },
                        }
                    },
                    new StepType
                    {
                        Name = "Test: Step Type 4",
                        ForeignKey = ForeignKey,
                        AllowMultiple = false,
                        Steps = new [] {
                            new Step {
                                PersonAliasId = jerryAliasId1,
                                CompletedDateTime = new DateTime(2020, 2, 1),
                                StartDateTime =  new DateTime(2020, 1, 1),
                                ForeignKey = ForeignKey
                            },
                            new Step {
                                PersonAliasId = kathyAliasId,
                                CompletedDateTime = new DateTime(2020, 2, 1),
                                StartDateTime =  new DateTime(2020, 1, 1),
                                ForeignKey = ForeignKey
                            },
                            new Step {
                                PersonAliasId = kathyAliasId,
                                CompletedDateTime = new DateTime(2019, 4, 1),
                                StartDateTime =  new DateTime(2019, 1, 1),
                                ForeignKey = ForeignKey
                            },
                        }
                    }
                }
            };

            stepProgramService.Add( stepProgram );
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Create the test data
        /// </summary>
        private static void CreateTestData()
        {
            CreateTestPeople();
            CreateTestStepProgram();
        }

        /// <summary>
        /// Delete the test data
        /// </summary>
        private static void DeleteTestData()
        {
            TestDataHelper.DeletePersonByGuid( PersonGuids );

            using ( var rockContext = new RockContext() )
            {
                var stepService = new StepService( rockContext );
                var stepQuery = stepService.Queryable().Where( s => s.ForeignKey == ForeignKey );
                stepService.DeleteRange( stepQuery );
                rockContext.SaveChanges();
            }

            using ( var rockContext = new RockContext() )
            {
                var stepProgramService = new StepProgramService( rockContext );
                var stepProgramQuery = stepProgramService.Queryable().Where( sp => sp.ForeignKey == ForeignKey );
                stepProgramService.DeleteRange( stepProgramQuery );
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Test Cleanup.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            DeleteTestData();
        }

        /// <summary>
        /// Initialize Tests.
        /// </summary>
        /// <param name="testContext">The test context.</param>
        [TestInitialize]
        public void TestInitialize()
        {
            // Reset the data before each test so each test does not affect another
            DeleteTestData();
            CreateTestData();
        }

        #endregion Setup Methods

        /// <summary>
        /// Tests GetPersonCompletingProgramQuery
        /// </summary>
        [TestMethod]
        [Ignore( "Fix required. This test exposes a potential problem with the Step PreSave Hook. See comments in CreateTestStepProgram method for details." )]
        public void GetPersonCompletingProgramQuery_ReturnsCorrectData()
        {
            var startDate = new DateTime( 2019, 1, 1 );
            var endDate = new DateTime( 2019, 2, 4 );

            var rockContext = new RockContext();
            var service = new StepProgramService( rockContext );
            var stepProgram = service.Queryable().Where( sp => sp.ForeignKey == ForeignKey ).First();

            // TODO: The StepProgramCompletion table contains no records.
            // It is not correctly populated with the completed steps added by the test data.
            var result = service.GetPersonCompletingProgramQuery( stepProgram.Id ).ToList();

            Assert.IsNotNull( result );
            Assert.AreEqual( 2, result.Count );

            var personService = new PersonService( rockContext );
            var kathy = personService.Get( KathyKolePersonGuidString.AsGuid() );
            var jerry = personService.Get( JerryJenkinsPersonGuidString.AsGuid() );

            var kathyResult = result.FirstOrDefault( r => r.PersonId == kathy.Id );
            var jerryResult = result.FirstOrDefault( r => r.PersonId == jerry.Id );

            // Kathy completed once in 2019 and once in 2020 - we should only get the first
            Assert.IsNotNull( kathyResult );
            Assert.AreEqual( new DateTime( 2019, 1, 1 ), kathyResult.StartedDateTime );
            Assert.AreEqual( new DateTime( 2019, 4, 1 ), kathyResult.CompletedDateTime );

            // Jerry completed with two aliases. The first alias started in 2019. The second alias finished in 2020
            Assert.IsNotNull( jerryResult );
            Assert.AreEqual( new DateTime( 2019, 1, 1 ), jerryResult.StartedDateTime );
            Assert.AreEqual( new DateTime( 2020, 3, 1 ), jerryResult.CompletedDateTime );
        }
    }
}
