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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Communication.Chat.Platform.Session;
using Rock.Configuration;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

using static Rock.Tests.Communication.Chat.Platform.Session.ChatSessionFixture;

namespace Rock.Tests.Communication.Chat.Platform.Session
{
    /// <summary>
    /// How a chat block's session learns whether the person may start a direct message: from
    /// the church's Direct Message Access data view, asked about this one person only.
    /// </summary>
    [TestClass]
    public class ChatSessionHelperDataViewTests
    {
        private static readonly Guid DataViewGuid = Guid.Parse( "bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb" );
        private const int DataViewId = 60;
        private const int PersonEntityTypeId = 15;

        [TestMethod]
        public void BuildSessionContext_NoDataViewConfigured_AnyoneMayStartADirectMessage()
        {
            using ( var app = TestHelper.CreateScopedRockApp() )
            {
                var configuration = ValidConfig().Configuration;
                configuration.DirectMessageAccessDataViewGuid = null;

                var context = ChatSessionHelper.BuildSessionContext( Adult(), configuration, app.App.CreateRockContext() );

                Assert.AreSame( configuration, context.Configuration );
                Assert.IsNull( context.DirectMessageAccessPersonIds, "no data view means no restriction, which the gates read as null" );
            }
        }

        [TestMethod]
        public void BuildSessionContext_NullPerson_ReadsNoDataView()
        {
            using ( var app = TestHelper.CreateScopedRockApp() )
            {
                var configuration = ValidConfig().Configuration;
                configuration.DirectMessageAccessDataViewGuid = DataViewGuid;

                var context = ChatSessionHelper.BuildSessionContext( null, configuration, app.App.CreateRockContext() );

                Assert.AreSame( configuration, context.Configuration );
                Assert.IsNull( context.DirectMessageAccessPersonIds );
            }
        }

        [TestMethod]
        public void BuildSessionContext_DeletedDataView_AdmitsNobody()
        {
            using ( var app = TestHelper.CreateScopedRockApp() )
            {
                var configuration = ValidConfig().Configuration;
                configuration.DirectMessageAccessDataViewGuid = DataViewGuid;

                var context = ChatSessionHelper.BuildSessionContext( Adult(), configuration, app.App.CreateRockContext() );

                Assert.IsNotNull( context.DirectMessageAccessPersonIds, "a deleted data view must not read as no restriction" );
                Assert.AreEqual( 0, context.DirectMessageAccessPersonIds.Count );
            }
        }

        [TestMethod]
        public void BuildSessionContext_PersonInDataView_IsAdmitted()
        {
            using ( var app = TestHelper.CreateScopedRockApp() )
            {
                var rockContext = app.App.CreateRockContext();
                SeedPersistedDataView( rockContext, PersonId, 99 );

                var configuration = ValidConfig().Configuration;
                configuration.DirectMessageAccessDataViewGuid = DataViewGuid;

                var context = ChatSessionHelper.BuildSessionContext( Adult(), configuration, rockContext );

                CollectionAssert.AreEquivalent( new[] { PersonId }, new List<int>( context.DirectMessageAccessPersonIds ), "only the person asked about is looked for, never the whole data view" );
            }
        }

        [TestMethod]
        public void BuildSessionContext_PersonNotInDataView_IsNotAdmitted()
        {
            using ( var app = TestHelper.CreateScopedRockApp() )
            {
                var rockContext = app.App.CreateRockContext();
                SeedPersistedDataView( rockContext, 99 );

                var configuration = ValidConfig().Configuration;
                configuration.DirectMessageAccessDataViewGuid = DataViewGuid;

                var context = ChatSessionHelper.BuildSessionContext( Adult(), configuration, rockContext );

                Assert.IsNotNull( context.DirectMessageAccessPersonIds );
                Assert.AreEqual( 0, context.DirectMessageAccessPersonIds.Count );
            }
        }

        /// <summary>
        /// Adds a persisted person data view holding the given people, so the data view is
        /// answered from its persisted values without running any filter.
        /// </summary>
        /// <param name="rockContext">The mocked context.</param>
        /// <param name="personIds">The people in the data view.</param>
        private static void SeedPersistedDataView( Rock.Data.RockContext rockContext, params int[] personIds )
        {
            rockContext.Set<EntityType>().Add( new EntityType
            {
                Id = PersonEntityTypeId,
                Guid = Guid.Parse( Rock.SystemGuid.EntityType.PERSON ),
                Name = typeof( Person ).FullName,
                AssemblyName = typeof( Person ).AssemblyQualifiedName
            } );

            var dataViewMock = new Mock<DataView>( MockBehavior.Loose ) { CallBase = true };
            dataViewMock.Setup( m => m.TypeId ).Returns( 0 );
            dataViewMock.Object.Id = DataViewId;
            dataViewMock.Object.Guid = DataViewGuid;
            dataViewMock.Object.Name = "Direct Message Access";
            dataViewMock.Object.EntityTypeId = PersonEntityTypeId;
            dataViewMock.Object.PersistedScheduleIntervalMinutes = 10;
            dataViewMock.Object.PersistedLastRefreshDateTime = RockDateTime.Now;
            dataViewMock.Object.Attributes = new Dictionary<string, AttributeCache>();
            dataViewMock.Object.AttributeValues = new Dictionary<string, AttributeValueCache>();
            rockContext.Set<DataView>().Add( dataViewMock.Object );

            foreach ( var personId in new[] { PersonId, 99 } )
            {
                rockContext.Set<Person>().Add( new Person { Id = personId } );
            }

            foreach ( var personId in personIds )
            {
                var valueMock = new Mock<DataViewPersistedValue>( MockBehavior.Loose ) { CallBase = true };
                valueMock.Object.DataViewId = DataViewId;
                valueMock.Object.EntityId = personId;
                rockContext.Set<DataViewPersistedValue>().Add( valueMock.Object );
            }
        }
    }
}
