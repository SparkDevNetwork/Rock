using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.Reporting.DataFilter;
using Rock.Tests.Integration.Jobs;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.RockTests.Model
{
    /// <summary>
    /// Tests for Steps Automation that use the database
    /// </summary>
    [TestClass]
    public class StepsAutomationJobTests
    {
        private const string ForeignKey = "_StepsAutomationJobTests_654FDDBF-6758-49B2-938F-F0856683E201_";
        private const string DataViewMiddleName = "_InTheDataViewForTests_";

        private const string JerryJenkinsPersonGuidString = "B8E6242D-B52E-4659-AB13-751A5F4C0BE4";
        private const string KathyKolePersonGuidString = "64BD1D38-D054-488F-86F6-38040242219E";
        private const string BarryBopPersonGuidString = "DFEFD90E-A993-493D-84D8-6903946523DB";
        private const string SimonSandsPersonGuidString = "D2D57C31-89C4-4A92-8917-894B49A42CAE";

        #region Setup Methods

        /// <summary>
        /// Creates the test people.
        /// </summary>
        private static void CreateTestPeople()
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var personAliasService = new PersonAliasService( rockContext );

            var personSimonSands = new Person
            {
                FirstName = "Simon",
                LastName = "Sands",
                Guid = SimonSandsPersonGuidString.AsGuid(),
                ForeignKey = ForeignKey,
                MiddleName = DataViewMiddleName
            };

            var personBarryBop = new Person
            {
                FirstName = "Barry",
                LastName = "Bop",
                Guid = BarryBopPersonGuidString.AsGuid(),
                ForeignKey = ForeignKey,
                MiddleName = DataViewMiddleName
            };

            var personKathyKole = new Person
            {
                FirstName = "Kathy",
                LastName = "Kole",
                Guid = KathyKolePersonGuidString.AsGuid(),
                ForeignKey = ForeignKey,
                MiddleName = $"NOT{DataViewMiddleName}"
            };

            var personJerryJenkins = new Person
            {
                FirstName = "Jerry",
                LastName = "Jenkins",
                Guid = JerryJenkinsPersonGuidString.AsGuid(),
                ForeignKey = ForeignKey,
                MiddleName = DataViewMiddleName
            };

            personService.Add( personJerryJenkins );
            personService.Add( personSimonSands );
            personService.Add( personKathyKole );
            personService.Add( personBarryBop );
            rockContext.SaveChanges();

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
        /// Creates the test data view.
        /// </summary>
        private static void CreateTestDataView()
        {
            var rockContext = new RockContext();
            var dataViewService = new DataViewService( rockContext );
            var dataViewFilterService = new DataViewFilterService( rockContext );

            var subDataViewFilter = new DataViewFilter
            {
                ForeignKey = ForeignKey,
                ExpressionType = FilterExpressionType.Filter,
                EntityTypeId = EntityTypeCache.Get<PropertyFilter>().Id,
                Guid = Guid.NewGuid(),
                Selection = $@"[""Property_MiddleName"",""1"",""{DataViewMiddleName}""]"
            };

            dataViewFilterService.Add( subDataViewFilter );
            rockContext.SaveChanges();

            var rootDataViewFilter = new DataViewFilter
            {
                ForeignKey = ForeignKey,
                ExpressionType = FilterExpressionType.GroupAll,
                Guid = Guid.NewGuid(),
                ChildFilters = new List<DataViewFilter>
                {
                    subDataViewFilter
                }
            };

            dataViewFilterService.Add( rootDataViewFilter );
            rockContext.SaveChanges();

            var dataView = new DataView
            {
                ForeignKey = ForeignKey,
                IsSystem = false,
                Name = "TEST " + ForeignKey,
                Description = "TEST " + ForeignKey,
                EntityTypeId = EntityTypeCache.Get<Person>().Id,
                Guid = Guid.NewGuid(),
                DataViewFilter = rootDataViewFilter
            };

            dataViewService.Add( dataView );
            rockContext.SaveChanges();

            subDataViewFilter.DataView = dataView;
            rootDataViewFilter.DataView = dataView;
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Creates the test steps.
        /// </summary>
        private static void CreateTestStepProgram()
        {
            var rockContext = new RockContext();
            var stepProgramService = new StepProgramService( rockContext );

            var dataViewService = new DataViewService( rockContext );
            var dataView = dataViewService.Queryable().FirstOrDefault( dv => dv.ForeignKey == ForeignKey );

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
                        Name = "Test: AllowMultiple with DataView",
                        ForeignKey = ForeignKey,
                        AllowMultiple = true,
                        AutoCompleteDataView = dataView
                    },
                    new StepType
                    {
                        Name = "Test: AllowMultiple without DataView",
                        ForeignKey = ForeignKey,
                        AllowMultiple = true
                    },
                    new StepType
                    {
                        Name = "Test: No multiple with DataView",
                        ForeignKey = ForeignKey,
                        AllowMultiple = false,
                        AutoCompleteDataView = dataView
                    },
                    new StepType
                    {
                        Name = "Test: No multiple and no dataview",
                        ForeignKey = ForeignKey,
                        AllowMultiple = false
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
            CreateTestDataView();
            CreateTestStepProgram();
        }

        /// <summary>
        /// Delete the test data
        /// </summary>
        private static void DeleteTestData()
        {
            var rockContext = new RockContext();

            var stepProgramService = new StepProgramService( rockContext );
            var stepProgramQuery = stepProgramService.Queryable().Where( sp => sp.ForeignKey == ForeignKey );
            stepProgramService.DeleteRange( stepProgramQuery );
            rockContext.SaveChanges();

            var dataViewFilterService = new DataViewFilterService( rockContext );
            var dvfQuery = dataViewFilterService.Queryable().Where( dvf => dvf.DataView.ForeignKey == ForeignKey || dvf.ForeignKey == ForeignKey );
            dataViewFilterService.DeleteRange( dvfQuery );
            rockContext.SaveChanges();

            var dataViewService = new DataViewService( rockContext );
            var dvQuery = dataViewService.Queryable().Where( dv => dv.ForeignKey == ForeignKey );
            dataViewService.DeleteRange( dvQuery );
            rockContext.SaveChanges();

            var personSearchKeyService = new PersonSearchKeyService( rockContext );
            var personSearchKeyQuery = personSearchKeyService.Queryable().Where( psk => psk.PersonAlias.Person.ForeignKey == ForeignKey );
            personSearchKeyService.DeleteRange( personSearchKeyQuery );

            var personAliasService = new PersonAliasService( rockContext );
            var personAliasQuery = personAliasService.Queryable().Where( pa => pa.Person.ForeignKey == ForeignKey || pa.ForeignKey == ForeignKey );
            personAliasService.DeleteRange( personAliasQuery );
            rockContext.SaveChanges();

            var personService = new PersonService( rockContext );
            var personQuery = personService.Queryable().Where( p => p.ForeignKey == ForeignKey );
            personService.DeleteRange( personQuery );
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Test Cleanup
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
            DeleteTestData();
            CreateTestData();
        }

        #endregion Setup Methods

        /// <summary>
        /// Tests Execute. Adds new steps for each person where steps don't exist.
        /// GitHub Issue #4390: Jerry Jenkins would have more than one step because he has multiple aliases.
        /// </summary>
        [TestMethod]
        public void Execute_AddsNewStep()
        {
            var jobContext = new TestJobContext();
            jobContext.JobDetail.JobDataMap[StepsAutomation.AttributeKey.DuplicatePreventionDayRange] = 7.ToString();

            var job = new StepsAutomation();
            job.Execute( jobContext );

            var rockContext = new RockContext();
            var stepProgramService = new StepProgramService( rockContext );
            var stepProgram = stepProgramService.Queryable( "StepTypes.Steps.StepStatus" ).FirstOrDefault( sp => sp.ForeignKey == ForeignKey );

            Assert.AreEqual( 4, stepProgram.StepTypes.Count );

            foreach ( var stepType in stepProgram.StepTypes )
            {
                if ( stepType.AutoCompleteDataViewId.HasValue )
                {
                    Assert.AreEqual( 3, stepType.Steps.Count );

                    foreach ( var step in stepType.Steps )
                    {
                        Assert.IsTrue( step.IsComplete );
                        Assert.IsTrue( step.StepStatus.IsCompleteStatus );
                    }
                }
                else
                {
                    Assert.AreEqual( 0, stepType.Steps.Count );
                }
            }
        }

        /// <summary>
        /// Tests Execute. Completes existing steps.
        /// GitHub Issue #4394: We changed the logic so in-progress steps would be completed.
        /// </summary>
        [TestMethod]
        public void Execute_CompletesExistingStep()
        {
            var stepGuid = Guid.NewGuid();

            using ( var rockContext = new RockContext() )
            {
                var stepProgramService = new StepProgramService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );
                var stepService = new StepService( rockContext );

                var stepProgram = stepProgramService.Queryable( "StepTypes.Steps.StepStatus" ).FirstOrDefault( sp => sp.ForeignKey == ForeignKey );

                stepService.Add( new Step
                {
                    ForeignKey = ForeignKey,
                    StepTypeId = stepProgram.StepTypes.FirstOrDefault( st => st.AutoCompleteDataViewId.HasValue ).Id,
                    StepStatus = stepProgram.StepStatuses.FirstOrDefault( ss => !ss.IsCompleteStatus ),
                    CompletedDateTime = null,
                    PersonAlias = personAliasService.Queryable().FirstOrDefault( pa =>
                        pa.ForeignKey == ForeignKey &&
                        pa.Person.MiddleName == DataViewMiddleName ),
                    Guid = stepGuid
                } );

                rockContext.SaveChanges();
            }

            // Run the job
            var jobContext = new TestJobContext();
            jobContext.JobDetail.JobDataMap[StepsAutomation.AttributeKey.DuplicatePreventionDayRange] = 7.ToString();

            var job = new StepsAutomation();
            job.Execute( jobContext );

            // Refresh the data from the database
            using ( var rockContext = new RockContext() )
            {
                var stepProgramService = new StepProgramService( rockContext );
                var stepProgram = stepProgramService.Queryable( "StepTypes.Steps.StepStatus" ).FirstOrDefault( sp => sp.ForeignKey == ForeignKey );
                var foundOriginalStep = false;

                Assert.AreEqual( 4, stepProgram.StepTypes.Count );

                foreach ( var stepType in stepProgram.StepTypes )
                {
                    if ( stepType.AutoCompleteDataViewId.HasValue )
                    {
                        Assert.AreEqual( 3, stepType.Steps.Count );

                        foreach ( var step in stepType.Steps )
                        {
                            if ( step.Guid == stepGuid )
                            {
                                foundOriginalStep = true;
                            }

                            Assert.IsTrue( step.IsComplete );
                            Assert.IsTrue( step.StepStatus.IsCompleteStatus );
                            Assert.IsNotNull( step.CompletedDateTime );
                        }
                    }
                    else
                    {
                        Assert.AreEqual( 0, stepType.Steps.Count );
                    }
                }

                Assert.IsTrue( foundOriginalStep );
            }
        }

        /// <summary>
        /// Tests Execute. Respects allow multiple and does not create another step.
        /// </summary>
        [TestMethod]
        public void Execute_RespectsAllowMultipleFalse()
        {
            var stepGuid = Guid.NewGuid();

            using ( var rockContext = new RockContext() )
            {
                var stepProgramService = new StepProgramService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );
                var stepService = new StepService( rockContext );

                var stepProgram = stepProgramService.Queryable( "StepTypes.Steps.StepStatus" ).FirstOrDefault( sp => sp.ForeignKey == ForeignKey );

                stepService.Add( new Step
                {
                    ForeignKey = ForeignKey,
                    StepTypeId = stepProgram.StepTypes.FirstOrDefault( st => st.AutoCompleteDataViewId.HasValue && !st.AllowMultiple ).Id,
                    StepStatus = stepProgram.StepStatuses.FirstOrDefault( ss => ss.IsCompleteStatus ),
                    CompletedDateTime = new DateTime( 2000, 1, 1 ),
                    PersonAlias = personAliasService.Queryable().FirstOrDefault( pa =>
                        pa.ForeignKey == ForeignKey &&
                        pa.Person.MiddleName == DataViewMiddleName ),
                    Guid = stepGuid
                } );

                rockContext.SaveChanges();
            }

            // Run the job
            var jobContext = new TestJobContext();
            jobContext.JobDetail.JobDataMap[StepsAutomation.AttributeKey.DuplicatePreventionDayRange] = 7.ToString();

            var job = new StepsAutomation();
            job.Execute( jobContext );

            // Refresh the data from the database
            using ( var rockContext = new RockContext() )
            {
                var stepProgramService = new StepProgramService( rockContext );
                var stepProgram = stepProgramService.Queryable( "StepTypes.Steps.StepStatus" ).FirstOrDefault( sp => sp.ForeignKey == ForeignKey );
                var foundOriginalStep = false;

                Assert.AreEqual( 4, stepProgram.StepTypes.Count );

                foreach ( var stepType in stepProgram.StepTypes )
                {
                    if ( stepType.AutoCompleteDataViewId.HasValue )
                    {
                        Assert.AreEqual( 3, stepType.Steps.Count );

                        foreach ( var step in stepType.Steps )
                        {
                            if ( step.Guid == stepGuid )
                            {
                                foundOriginalStep = true;
                            }

                            Assert.IsTrue( step.IsComplete );
                            Assert.IsTrue( step.StepStatus.IsCompleteStatus );
                            Assert.IsNotNull( step.CompletedDateTime );
                        }
                    }
                    else
                    {
                        Assert.AreEqual( 0, stepType.Steps.Count );
                    }
                }

                Assert.IsTrue( foundOriginalStep );
            }
        }

        /// <summary>
        /// Tests Execute. Respects allow multiple and creates another step.
        /// </summary>
        [TestMethod]
        public void Execute_RespectsAllowMultipleTrue()
        {
            var stepGuid = Guid.NewGuid();

            using ( var rockContext = new RockContext() )
            {
                var stepProgramService = new StepProgramService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );
                var stepService = new StepService( rockContext );

                var stepProgram = stepProgramService.Queryable( "StepTypes.Steps.StepStatus" ).FirstOrDefault( sp => sp.ForeignKey == ForeignKey );

                stepService.Add( new Step
                {
                    ForeignKey = ForeignKey,
                    StepTypeId = stepProgram.StepTypes.FirstOrDefault( st => st.AutoCompleteDataViewId.HasValue && st.AllowMultiple ).Id,
                    StepStatus = stepProgram.StepStatuses.FirstOrDefault( ss => ss.IsCompleteStatus ),
                    CompletedDateTime = new DateTime( 2000, 1, 1 ),
                    PersonAlias = personAliasService.Queryable().FirstOrDefault( pa =>
                        pa.ForeignKey == ForeignKey &&
                        pa.Person.MiddleName == DataViewMiddleName ),
                    Guid = stepGuid
                } );

                rockContext.SaveChanges();
            }

            // Run the job
            var jobContext = new TestJobContext();
            jobContext.JobDetail.JobDataMap[StepsAutomation.AttributeKey.DuplicatePreventionDayRange] = 7.ToString();

            var job = new StepsAutomation();
            job.Execute( jobContext );

            // Refresh the data from the database
            using ( var rockContext = new RockContext() )
            {
                var stepProgramService = new StepProgramService( rockContext );
                var stepProgram = stepProgramService.Queryable( "StepTypes.Steps.StepStatus" ).FirstOrDefault( sp => sp.ForeignKey == ForeignKey );
                var foundOriginalStep = false;

                Assert.AreEqual( 4, stepProgram.StepTypes.Count );

                foreach ( var stepType in stepProgram.StepTypes )
                {
                    if ( stepType.AutoCompleteDataViewId.HasValue )
                    {
                        Assert.AreEqual( stepType.AllowMultiple ? 4 : 3, stepType.Steps.Count );

                        foreach ( var step in stepType.Steps )
                        {
                            if ( step.Guid == stepGuid )
                            {
                                foundOriginalStep = true;
                                Assert.IsTrue( step.CompletedDateTime.HasValue );
                                Assert.AreEqual( 2000, step.CompletedDateTime.Value.Year );
                            }

                            Assert.IsTrue( step.IsComplete );
                            Assert.IsTrue( step.StepStatus.IsCompleteStatus );
                            Assert.IsNotNull( step.CompletedDateTime );
                        }
                    }
                    else
                    {
                        Assert.AreEqual( 0, stepType.Steps.Count );
                    }
                }

                Assert.IsTrue( foundOriginalStep );
            }
        }
    }
}
