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
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Communication.Chat.Platform.Sync;
using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Which saves the immediate sync looks at, decided from the save's own values alone.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The save hooks call into the immediate sync on every person, group and membership
    ///         save Rock makes, and almost none of them are chat's: a family edited, a check-in
    ///         group, a person's email. Those must never reach the database, so the question is
    ///         answered from the entry and from cached group types before anything else is touched.
    ///         Every read the test context serves is a call on it, so none may be made.
    ///     </para>
    ///     <para>
    ///         They may allocate a little. Asking the group type cache whether a type allows chat
    ///         costs about 192 bytes a call, for the cache key and the lookup it builds. The only
    ///         way to avoid it is a private list of chat-enabled types, which every group type save
    ///         would have to keep in step, so 512 bytes a call on average is the bound: room for
    ///         that lookup, and far short of a configuration read.
    ///     </para>
    ///     <para>
    ///         Reading the chat settings parses and decrypts a stored value and builds a new
    ///         settings object every time, so it would show both as a call on the context and in the
    ///         bytes. Each save is made inside a request, because a save outside one is turned away
    ///         for another reason and would pass here without the filter ever being asked.
    ///     </para>
    /// </remarks>
    [TestClass]
    public class ChatImmediateScopeTests
    {
        #region Constants

        private const int Calls = 1000;

        // The group type cache lookup costs about 192 bytes a call; this leaves room for it and no
        // more than a little besides.
        private const long AllocatedBytesPerCallBound = 512;

        private const int ChatGroupTypeId = 11;

        private const int OtherGroupTypeId = 12;

        private const int GroupId = 21;

        #endregion Constants

        #region Next to nothing for a save chat cannot see

        [TestMethod]
        public void AMembershipSaveInAGroupChatCannotReachReadsNoDatabaseAndAllocatesLittle()
        {
            using ( var app = TestHelper.CreateScopedRockApp() )
            {
                var rockContext = AddGroupTypes( app );
                var entry = new SaveEntry
                {
                    Entity = new GroupMember { GroupId = GroupId, GroupTypeId = OtherGroupTypeId, PersonId = 31 },
                    DataContext = rockContext,
                    State = EntityContextState.Added,
                    PreSaveState = EntityContextState.Added
                };

                AssertCheap( rockContext, "membership saves in a group chat cannot reach", () => ChatPlatformSyncHelper.RecordGroupMemberSave( entry ) );
            }
        }

        [TestMethod]
        public void AGroupSaveChatCannotReachReadsNoDatabaseAndAllocatesLittle()
        {
            using ( var app = TestHelper.CreateScopedRockApp() )
            {
                var rockContext = AddGroupTypes( app );
                var entry = new SaveEntry
                {
                    Entity = new Group { Id = GroupId, GroupTypeId = OtherGroupTypeId, Name = "A family" },
                    DataContext = rockContext,
                    State = EntityContextState.Modified,
                    PreSaveState = EntityContextState.Modified
                };

                AssertCheap( rockContext, "saves of a group chat cannot reach", () => ChatPlatformSyncHelper.RecordGroupSave( entry ) );
            }
        }

        [TestMethod]
        public void APersonSaveThatChangesNothingChatShowsReadsNoDatabaseAndAllocatesLittle()
        {
            using ( var app = TestHelper.CreateScopedRockApp() )
            {
                var rockContext = AddGroupTypes( app );
                var entry = EmailOnlyPersonSave( rockContext );

                AssertCheap( rockContext, "person saves that changed only an email", () => ChatPlatformSyncHelper.RecordPersonSave( entry ) );
            }
        }

        #endregion Next to nothing for a save chat cannot see

        #region What passes

        [TestMethod]
        public void AGroupWhoseTypeAllowsChatIsInScopeAndOneWhoseTypeDoesNotIsNot()
        {
            using ( var app = TestHelper.CreateScopedRockApp() )
            {
                AddGroupTypes( app );

                Assert.IsTrue( ChatPlatformSyncHelper.IsGroupInScope( GroupId, ChatGroupTypeId ),
                    "a membership of a group whose type allows chat can put a person in a channel or take them out" );
                Assert.IsFalse( ChatPlatformSyncHelper.IsGroupInScope( GroupId, OtherGroupTypeId ),
                    "and one whose type does not cannot" );
            }
        }

        [TestMethod]
        public void APersonSaveChangingOnlyTheEmailIsNotInScope()
        {
            using ( var app = TestHelper.CreateScopedRockApp() )
            {
                var rockContext = AddGroupTypes( app );

                Assert.IsFalse( ChatPlatformSyncHelper.IsPersonChangeInScope( EmailOnlyPersonSave( rockContext ) ),
                    "chat never shows an email, so changing one has nothing to push" );
            }
        }

        [TestMethod]
        public void APersonSaveChangingTheNickNameIsInScope()
        {
            using ( var app = TestHelper.CreateScopedRockApp() )
            {
                var rockContext = AddGroupTypes( app );
                var entry = EmailOnlyPersonSave( rockContext );
                ( ( Person ) entry.Entity ).NickName = "Teddy";

                Assert.IsTrue( ChatPlatformSyncHelper.IsPersonChangeInScope( entry ),
                    "chat shows the nick name, so a save that changes it is pushed" );
            }
        }

        #endregion What passes

        #region Support

        /// <summary>
        /// Puts one group type that allows chat and one that does not where the group type cache
        /// reads them from.
        /// </summary>
        private static RockContext AddGroupTypes( TestHelper.RockAppScope app )
        {
            var rockContext = app.App.CreateRockContext();

            rockContext.Set<GroupType>().Add( new GroupType
            {
                Id = ChatGroupTypeId,
                Guid = new Guid( "7b0f7d38-5c7c-4d2e-9d53-6f5a8b1f0c11" ),
                Name = "Chat allowed",
                IsChatAllowed = true,
                IsChatEnabledForAllGroups = true
            } );

            rockContext.Set<GroupType>().Add( new GroupType
            {
                Id = OtherGroupTypeId,
                Guid = new Guid( "2d6c8e7a-1f4b-4a39-8c2e-5b7d9e0a3f12" ),
                Name = "Family like",
                IsChatAllowed = false
            } );

            return rockContext;
        }

        /// <summary>
        /// A person's save whose only change is the email, with every value chat shows as it was.
        /// </summary>
        private static SaveEntry EmailOnlyPersonSave( RockContext rockContext )
        {
            var person = new Person
            {
                Id = 41,
                NickName = "Ted",
                LastName = "Decker",
                PhotoId = 51,
                PrimaryCampusId = 61,
                RecordStatusValueId = 71,
                IsDeceased = false,
                IsChatProfilePublic = true,
                IsChatOpenDirectMessageAllowed = false,
                Email = "ted.new@example.com"
            };

            var originalValues = new Dictionary<string, object>
            {
                [nameof( Person.NickName )] = "Ted",
                [nameof( Person.LastName )] = "Decker",
                [nameof( Person.PhotoId )] = 51,
                [nameof( Person.PrimaryCampusId )] = 61,
                [nameof( Person.RecordStatusValueId )] = 71,
                [nameof( Person.IsDeceased )] = false,
                [nameof( Person.IsChatProfilePublic )] = true,
                [nameof( Person.IsChatOpenDirectMessageAllowed )] = false,
                [nameof( Person.Email )] = "ted@example.com"
            };

            return new SaveEntry
            {
                Entity = person,
                OriginalValues = originalValues,
                ModifiedProperties = new List<string> { nameof( Person.Email ) },
                DataContext = rockContext,
                State = EntityContextState.Modified,
                PreSaveState = EntityContextState.Modified
            };
        }

        /// <summary>
        /// Makes one save many times inside a request, after one call that pays for anything a
        /// first call does once, and asserts that none of them asked the test context for anything
        /// and that together they allocated no more than the bound allows.
        /// </summary>
        private static void AssertCheap( RockContext rockContext, string what, Action save )
        {
            // The runtime has the per-thread counter; the reference assemblies this project builds
            // against do not name it.
            var method = typeof( GC ).GetMethod( "GetAllocatedBytesForCurrentThread", BindingFlags.Public | BindingFlags.Static );
            Assert.IsNotNull( method, "this runtime has no per-thread allocation counter, so nothing here can be measured" );

            var allocatedBytes = ( Func<long> ) Delegate.CreateDelegate( typeof( Func<long> ), method );
            var context = Mock.Get( rockContext );

            var (allocated, contextCalls) = InsideRequest( () =>
            {
                save();
                context.Invocations.Clear();

                var before = allocatedBytes();

                for ( var i = 0; i < Calls; i++ )
                {
                    save();
                }

                return (allocatedBytes() - before, context.Invocations.Count);
            } );

            Assert.AreEqual( 0, contextCalls, $"{Calls} {what} asked the database for something {contextCalls} times" );
            Assert.IsTrue( allocated <= AllocatedBytesPerCallBound * Calls,
                $"{Calls} {what} allocated {allocated} bytes, {allocated / Calls} a call, past the bound of {AllocatedBytesPerCallBound}" );
        }

        /// <summary>
        /// Runs code as though inside a web request, as a save from a block or the API is.
        /// </summary>
        private static T InsideRequest<T>( Func<T> action )
        {
            var accessor = new RockRequestContextAccessor { RockRequestContext = new RockRequestContext() };

            try
            {
                return action();
            }
            finally
            {
                accessor.RockRequestContext = null;
            }
        }

        /// <summary>
        /// A save entry built from values, as a save hook is handed one.
        /// </summary>
        private sealed class SaveEntry : IEntitySaveEntry
        {
            public object Entity { get; set; }

            public IReadOnlyDictionary<string, object> OriginalValues { get; set; } = new Dictionary<string, object>();

            public IReadOnlyList<string> ModifiedProperties { get; set; } = new List<string>();

            public object DataContext { get; set; }

            public EntityContextState PreSaveState { get; set; }

            public EntityContextState State { get; set; }
        }

        #endregion Support
    }
}
