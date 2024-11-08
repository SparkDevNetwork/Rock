using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Filters;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;
using Rock.ViewModels.CheckIn;
using Rock.Web.Cache;

namespace Rock.Tests.UnitTests.Rock.CheckIn.v2.Filters
{
    /// <summary>
    /// This suite checks the various combinations of filter settings related to
    /// a person's membership in a dataview for the check-in process.
    /// </summary>
    /// <seealso cref="DataViewOpportunityFilter"/>
    [TestClass]
    public class DataViewOpportunityFilterTests : CheckInMockDatabase
    {
        #region IsGroupValid Tests

        [TestMethod]
        public void DataViewFilter_WithEmptyDataView_ExcludesGroup()
        {
            var dataViewGuid = new Guid( "f2bc089d-7754-4b9d-ba94-328b5279e0b4" );
            var dataViewMock = CreatePersistedDataViewMock( 1, dataViewGuid, "Test" );
            var rockContextMock = GetRockContextMock();

            rockContextMock.SetupDbSet( dataViewMock.Object );
            rockContextMock.SetupDbSet<DataViewPersistedValue>();

            var filter = CreateDataViewFilter( 3, rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( new[] { dataViewGuid } );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void DataViewFilter_WithoutDataView_IncludesGroup()
        {
            var rockContextMock = GetRockContextMock();

            var filter = CreateDataViewFilter( 3, rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( Array.Empty<Guid>() );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void DataViewFilter_WithSingleDataViewMatchingPerson_IncludesGroup()
        {
            var personId = 3;
            var dataViewId = 2;
            var dataViewGuid = new Guid( "f2bc089d-7754-4b9d-ba94-328b5279e0b4" );

            var dataViewMock = CreatePersistedDataViewMock( dataViewId, dataViewGuid, "Test" );
            var persistedValueMock = GetPersistedDataViewValueMock( dataViewId, personId );
            var rockContextMock = GetRockContextMock();

            rockContextMock.SetupDbSet( dataViewMock.Object );
            rockContextMock.SetupDbSet( persistedValueMock.Object );

            var filter = CreateDataViewFilter( personId, rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( new[] { dataViewGuid } );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void DataViewFilter_WithOneDataViewMatchingPersonAndOneNotMatchingPerson_ExcludesGroup()
        {
            var personId = 3;
            var includesPersonDataViewId = 2;
            var includesPersonDataViewGuid = new Guid( "f2bc089d-7754-4b9d-ba94-328b5279e0b4" );
            var excludesPersonDataViewId = 4;
            var excludesPersonDataViewGuid = new Guid( "acf878ae-f386-4f7d-bb1a-f30ca480eade" );

            var includesPersonDataViewMock = CreatePersistedDataViewMock( includesPersonDataViewId, includesPersonDataViewGuid, "Test" );
            var excludesPersonDataViewMock = CreatePersistedDataViewMock( excludesPersonDataViewId, excludesPersonDataViewGuid, "Test" );
            var persistedValueMock = GetPersistedDataViewValueMock( includesPersonDataViewId, personId );
            var rockContextMock = GetRockContextMock();

            rockContextMock.SetupDbSet( includesPersonDataViewMock.Object, excludesPersonDataViewMock.Object );
            rockContextMock.SetupDbSet( persistedValueMock.Object );

            var filter = CreateDataViewFilter( personId, rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( new[] { includesPersonDataViewGuid, excludesPersonDataViewGuid } );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void DataViewFilter_WithDeletedDataView_IncludesGroup()
        {
            var personId = 3;
            var deletedDataViewGuid = new Guid( "b1f5fe41-d546-47e6-b4d0-09f9b2a3f676" );
            var rockContextMock = GetRockContextMock();

            rockContextMock.SetupDbSet<DataView>();

            var filter = CreateDataViewFilter( personId, rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( new[] { deletedDataViewGuid } );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Creates the <see cref="DataViewOpportunityFilter"/> along with a
        /// person to be filtered.
        /// </summary>
        /// <param name="personId">The identifier of the person to be used for membership check.</param>
        /// <param name="rockContext">The context to use when accessing database objects.</param>
        /// <returns>An instance of <see cref="DataViewOpportunityFilter"/>.</returns>
        private DataViewOpportunityFilter CreateDataViewFilter( int personId, RockContext rockContext )
        {
            // Create the template configuration.
            var templateConfigurationMock = new Mock<TemplateConfigurationData>( MockBehavior.Strict );

            var director = new CheckInDirector( rockContext );

            // Create the filter.
            var filter = new DataViewOpportunityFilter
            {
                Person = new Attendee
                {
                    Person = new PersonBag()
                },
                Session = new CheckInSession( director, templateConfigurationMock.Object ),
                TemplateConfiguration = templateConfigurationMock.Object
            };

            filter.Person.Person.Id = IdHasher.Instance.GetHash( personId );

            return filter;
        }

        /// <summary>
        /// Creates a group opportunity with the specified data view unique
        /// identifier values.
        /// </summary>
        /// <param name="dataViewGuids">The data view unique identifiers.</param>
        /// <returns>A new instance of <see cref="GroupOpportunity"/>.</returns>
        private GroupOpportunity CreateGroupOpportunity( IReadOnlyCollection<Guid> dataViewGuids )
        {
            var groupConfigurationMock = new Mock<GroupConfigurationData>( MockBehavior.Strict );

            groupConfigurationMock.Setup( m => m.DataViewGuids ).Returns( dataViewGuids );

            return new GroupOpportunity
            {
                CheckInData = groupConfigurationMock.Object
            };
        }

        /// <summary>
        /// Creates a mock <see cref="DataView"/> object that is defined as
        /// persisted.
        /// </summary>
        /// <param name="id">The data view identifier.</param>
        /// <param name="guid">The data view unique identifier.</param>
        /// <param name="name">The data view name.</param>
        /// <returns>A mocking instance for <see cref="DataView"/>.</returns>
        private Mock<DataView> CreatePersistedDataViewMock( int id, Guid guid, string name )
        {
            var dataViewMock = new Mock<DataView>( MockBehavior.Loose )
            {
                CallBase = true
            };

            dataViewMock.Setup( m => m.TypeId ).Returns( 0 );

            dataViewMock.Object.Id = id;
            dataViewMock.Object.Guid = guid;
            dataViewMock.Object.Name = name;
            dataViewMock.Object.PersistedScheduleIntervalMinutes = 10;
            dataViewMock.Object.PersistedLastRefreshDateTime = RockDateTime.Now;
            dataViewMock.Object.Attributes = new Dictionary<string, AttributeCache>();
            dataViewMock.Object.AttributeValues = new Dictionary<string, AttributeValueCache>();

            return dataViewMock;
        }

        /// <summary>
        /// Creates a mock <see cref="DataViewPersistedValue"/> object that
        /// identifies a specified entity for the data view.
        /// </summary>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns>A mocking instance for <see cref="DataViewPersistedValue"/>.</returns>
        private Mock<DataViewPersistedValue> GetPersistedDataViewValueMock( int dataViewId, int entityId )
        {
            var valueMock = new Mock<DataViewPersistedValue>( MockBehavior.Loose )
            {
                CallBase = true
            };

            valueMock.Object.DataViewId = dataViewId;
            valueMock.Object.EntityId = entityId;

            return valueMock;
        }

        #endregion
    }
}
