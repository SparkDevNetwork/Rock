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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.Reporting.DataFilter;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core.Jobs
{
    /// <summary>
    /// Tests for Steps Automation that use the database
    /// </summary>
    [TestClass]
    public class StepsAutomationJobTests : DatabaseTestsBase
    {
        private const string ForeignKey = "_StepsAutomationJobTests_654FDDBF-6758-49B2-938F-F0856683E201_";
        private const string DataViewMiddleName = "_InTheDataViewForTests_";

        private const string JerryJenkinsPersonGuidString = "B8E6242D-B52E-4659-AB13-751A5F4C0BE4";
        private const string KathyKolePersonGuidString = "64BD1D38-D054-488F-86F6-38040242219E";
        private const string BarryBopPersonGuidString = "DFEFD90E-A993-493D-84D8-6903946523DB";
        private const string SimonSandsPersonGuidString = "D2D57C31-89C4-4A92-8917-894B49A42CAE";

        #region Setup Methods

        /// <summary>
        /// Test Cleanup.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            DeleteTestData();
        }

        ///// <summary>
        ///// Initialize Tests.
        ///// </summary>
        ///// <param name="testContext">The test context.</param>
        [TestInitialize]
        public void TestInitialize()
        {
            CreateTestData();
        }

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

            // Not in the dataview
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
        /// Creates the test step program.
        /// There are 2 step statuses: 1 complete and 1 in-progress
        /// There are 4 step types:
        ///     1) Allow multiple with an auto-complete dataview
        ///     2) Allow multiple without an autocomplete dataview
        ///     3) No multiple with an auto-complete dataview
        ///     4) No multiple with no auto-complete dataview
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
            DeleteTestData();

            CreateTestPeople();
            CreateTestDataView();
            CreateTestStepProgram();
        }

        /// <summary>
        /// Delete the test data
        /// </summary>
        private static void DeleteTestData()
        {
            var personGuidList = new List<Guid>
            {
                BarryBopPersonGuidString.AsGuid(),
                JerryJenkinsPersonGuidString.AsGuid(),
                KathyKolePersonGuidString.AsGuid(),
                SimonSandsPersonGuidString.AsGuid()
            };

            TestDataHelper.DeletePersonByGuid( personGuidList );

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
        }

        #endregion Setup Methods

        /// <summary>
        /// Tests Execute. Adds new steps for each person where steps don't exist.
        /// GitHub Issue #4390: Jerry Jenkins would have more than one step because he has multiple aliases.
        /// </summary>
        [TestMethod]
        public void Execute_AddsNewStep()
        {
            var testAttributeValues = new Dictionary<string, string>();
            testAttributeValues.AddOrReplace( StepsAutomation.AttributeKey.DuplicatePreventionDayRange, 7.ToString() );

            var job = new StepsAutomation();
            job.ExecuteInternal( testAttributeValues );

            var rockContext = new RockContext();
            var stepProgramService = new StepProgramService( rockContext );
            var stepProgram = stepProgramService.Queryable( "StepTypes.Steps.StepStatus" ).FirstOrDefault( sp => sp.ForeignKey == ForeignKey );

            Assert.AreEqual( 4, stepProgram.StepTypes.Count );

            foreach ( var stepType in stepProgram.StepTypes )
            {
                if ( stepType.AutoCompleteDataViewId.HasValue )
                {
                    // The three people in the dataview should have completed steps
                    Assert.AreEqual( 3, stepType.Steps.Count );

                    foreach ( var step in stepType.Steps )
                    {
                        Assert.IsTrue( step.IsComplete );
                        Assert.IsTrue( step.StepStatus.IsCompleteStatus );
                        Assert.IsNotNull( step.CompletedDateTime );
                    }
                }
                else
                {
                    // No steps should exist for a step type with no auto-complete dataview
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
            // Add an in-progress step that should be made complete by the job
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
            var testAttributeValues = new Dictionary<string, string>();
            testAttributeValues.AddOrReplace( StepsAutomation.AttributeKey.DuplicatePreventionDayRange, 7.ToString() );

            var job = new StepsAutomation();
            job.ExecuteInternal( testAttributeValues );

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
                        // The 3 people of the dataview should have a completed step
                        Assert.AreEqual( 3, stepType.Steps.Count );

                        foreach ( var step in stepType.Steps )
                        {
                            if ( step.Guid == stepGuid )
                            {
                                // We need to ensure that the original step (was in-progress) still exists
                                foundOriginalStep = true;
                            }

                            Assert.IsTrue( step.IsComplete );
                            Assert.IsTrue( step.StepStatus.IsCompleteStatus );
                            Assert.IsNotNull( step.CompletedDateTime );
                        }
                    }
                    else
                    {
                        // No steps should exist for a type with no auto-complete dataview
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
            // Create a complete step for a step type that does not allow multiple for a person in the dataview
            var stepGuid = Guid.NewGuid();
            var stepCompletedDateTime = new DateTime( 2000, 1, 1 );

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
                    CompletedDateTime = stepCompletedDateTime,
                    PersonAlias = personAliasService.Queryable().FirstOrDefault( pa =>
                        pa.ForeignKey == ForeignKey &&
                        pa.Person.MiddleName == DataViewMiddleName ),
                    Guid = stepGuid
                } );

                rockContext.SaveChanges();
            }

            var testAttributeValues = new Dictionary<string, string>();
            testAttributeValues.AddOrReplace( StepsAutomation.AttributeKey.DuplicatePreventionDayRange, 7.ToString() );

            var job = new StepsAutomation();

            // Run the job
            job.ExecuteInternal( testAttributeValues );

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
                        // 3 people in the dataview should have a completed step
                        Assert.AreEqual( 3, stepType.Steps.Count );

                        foreach ( var step in stepType.Steps )
                        {
                            // One of the 3 steps should be the original with an unmodified completed date
                            if ( step.Guid == stepGuid )
                            {
                                foundOriginalStep = true;
                                Assert.IsNotNull( step.CompletedDateTime );
                                Assert.AreEqual( stepCompletedDateTime, step.CompletedDateTime.Value );
                            }

                            Assert.IsTrue( step.IsComplete );
                            Assert.IsTrue( step.StepStatus.IsCompleteStatus );
                            Assert.IsNotNull( step.CompletedDateTime );
                        }
                    }
                    else
                    {
                        // No steps should exist for a type with no auto-complete dataview
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
            // Create a complete step for a step type that does allow multiple for a person in the dataview
            var stepGuid = Guid.NewGuid();
            var stepCompletedDateTime = new DateTime( 2000, 1, 1 );
            int stepTypeId;

            using ( var rockContext = new RockContext() )
            {
                var stepProgramService = new StepProgramService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );
                var stepService = new StepService( rockContext );

                var stepProgram = stepProgramService.Queryable( "StepTypes.Steps.StepStatus" ).FirstOrDefault( sp => sp.ForeignKey == ForeignKey );
                stepTypeId = stepProgram.StepTypes.FirstOrDefault( st => st.AutoCompleteDataViewId.HasValue && st.AllowMultiple ).Id;

                stepService.Add( new Step
                {
                    ForeignKey = ForeignKey,
                    StepTypeId = stepTypeId,
                    StepStatus = stepProgram.StepStatuses.FirstOrDefault( ss => ss.IsCompleteStatus ),
                    CompletedDateTime = stepCompletedDateTime,
                    PersonAlias = personAliasService.Queryable().FirstOrDefault( pa =>
                        pa.ForeignKey == ForeignKey &&
                        pa.Person.MiddleName == DataViewMiddleName ),
                    Guid = stepGuid
                } );

                rockContext.SaveChanges();
            }

            var testAttributeValues = new Dictionary<string, string>();
            testAttributeValues.AddOrReplace( StepsAutomation.AttributeKey.DuplicatePreventionDayRange, 7.ToString() );

            var job = new StepsAutomation();

            // Run the job
            job.ExecuteInternal( testAttributeValues );

            // Refresh the data from the database
            using ( var rockContext = new RockContext() )
            {
                var stepProgramService = new StepProgramService( rockContext );
                var stepProgram = stepProgramService.Queryable( "StepTypes.Steps.StepStatus" ).FirstOrDefault( sp => sp.ForeignKey == ForeignKey );
                var foundOriginalStep = false;

                Assert.AreEqual( 4, stepProgram.StepTypes.Count );

                foreach ( var stepType in stepProgram.StepTypes )
                {
                    if ( stepType.Id == stepTypeId )
                    {
                        // This is the allow multiple type with an autocomplete dataview
                        // There should be the original step and now also 3 new ones.
                        Assert.AreEqual( 4, stepType.Steps.Count );

                        foreach ( var step in stepType.Steps )
                        {
                            // The original step should be found and the completed datetime should not have changed
                            if ( step.Guid == stepGuid )
                            {
                                foundOriginalStep = true;
                                Assert.IsTrue( step.CompletedDateTime.HasValue );
                                Assert.AreEqual( stepCompletedDateTime, step.CompletedDateTime.Value );
                            }

                            Assert.IsTrue( step.IsComplete );
                            Assert.IsTrue( step.StepStatus.IsCompleteStatus );
                            Assert.IsNotNull( step.CompletedDateTime );
                        }
                    }
                    else if ( stepType.AutoCompleteDataViewId.HasValue )
                    {
                        // There should be a completed step for each person in the dataview
                        Assert.AreEqual( 3, stepType.Steps.Count );

                        foreach ( var step in stepType.Steps )
                        {
                            Assert.IsTrue( step.IsComplete );
                            Assert.IsTrue( step.StepStatus.IsCompleteStatus );
                            Assert.IsNotNull( step.CompletedDateTime );
                        }
                    }
                    else
                    {
                        // No steps for types with no dataview
                        Assert.AreEqual( 0, stepType.Steps.Count );
                    }
                }

                Assert.IsTrue( foundOriginalStep );
            }
        }
    }
}
