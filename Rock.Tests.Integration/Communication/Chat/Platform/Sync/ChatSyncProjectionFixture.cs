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
using System.IO;
using System.Linq;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Contract;
using Rock.Communication.Chat.Platform.Sync;
using Rock.Configuration;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// A small church made on the spot, and the reading the projection takes of it.
    /// </summary>
    /// <remarks>
    /// Everything it makes carries one foreign key, and everything with that foreign key is deleted
    /// when it is disposed. The projection reads the whole database, so these tests find their own
    /// rows by the identifiers they made rather than by position or by count.
    /// </remarks>
    internal sealed class ChatSyncProjectionFixture : IDisposable
    {
        #region Fields

        private readonly List<int> _createdGroupTypeIds = new List<int>();

        private readonly List<int> _createdFamilyGroupIds = new List<int>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Makes the group types the tests hang their channels off.
        /// </summary>
        public ChatSyncProjectionFixture()
        {
            ForeignKey = "chat-sync-projection " + Guid.NewGuid();

            using ( var rockContext = new RockContext() )
            {
                SharedGroupTypeId = AddChatGroupType( rockContext, "Chat Shared Channel Fixture", null );

                var directMessageGuid = Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_DIRECT_MESSAGE.AsGuid();
                var directMessage = new GroupTypeService( rockContext ).Queryable().FirstOrDefault( t => t.Guid == directMessageGuid );

                if ( directMessage == null )
                {
                    // The projection asks whether a group type is the direct message one by its
                    // guid, so a stand-in with a different guid would not be a direct message at
                    // all and every invariant below would pass by accident.
                    DirectMessageGroupTypeId = AddChatGroupType( rockContext, "Chat Direct Message Fixture", directMessageGuid );
                }
                else
                {
                    DirectMessageGroupTypeId = directMessage.Id;
                }
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// The mark every row this fixture made carries.
        /// </summary>
        public string ForeignKey { get; }

        /// <summary>
        /// A group type that allows chat on every one of its groups.
        /// </summary>
        public int SharedGroupTypeId { get; }

        /// <summary>
        /// The direct message group type, as the projection recognises it.
        /// </summary>
        public int DirectMessageGroupTypeId { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Adds a group of one of this fixture's types.
        /// </summary>
        /// <param name="groupTypeId">The type.</param>
        /// <param name="name">The name, which may be blank.</param>
        /// <param name="edit">Anything else to set before it is saved.</param>
        /// <returns>The group's guid.</returns>
        public Guid AddChannel( int groupTypeId, string name, Action<Group> edit = null )
        {
            using ( var rockContext = new RockContext() )
            {
                var group = new Group
                {
                    Guid = Guid.NewGuid(),
                    GroupTypeId = groupTypeId,
                    Name = name,
                    IsActive = true,
                    IsArchived = false,
                    IsSystem = false,
                    ForeignKey = ForeignKey
                };

                edit?.Invoke( group );

                new GroupService( rockContext ).Add( group );
                rockContext.SaveChanges();

                return group.Guid;
            }
        }

        /// <summary>
        /// Adds a person, with the family and primary alias Rock gives every new person.
        /// </summary>
        /// <param name="lastName">A name to tell them apart by.</param>
        /// <returns>The person's id.</returns>
        public int AddPerson( string lastName )
        {
            using ( var rockContext = new RockContext() )
            {
                var person = new Person
                {
                    Guid = Guid.NewGuid(),
                    FirstName = "Projection",
                    LastName = lastName,
                    Gender = Gender.Unknown,
                    ForeignKey = ForeignKey
                };

                // Adding a person makes that person a family, and the family is a group this
                // fixture is responsible for taking away again.
                var family = PersonService.SaveNewPerson( person, rockContext, null, false );
                if ( family != null )
                {
                    _createdFamilyGroupIds.Add( family.Id );
                }

                return person.Id;
            }
        }

        /// <summary>
        /// Adds a second, non-primary alias to a person.
        /// </summary>
        /// <param name="personId">The person.</param>
        /// <returns>The new alias's guid.</returns>
        public Guid AddExtraAlias( int personId )
        {
            using ( var rockContext = new RockContext() )
            {
                // No alias person id, because the one that equals the person id is the primary
                // alias and there is a unique index behind that.
                var alias = new PersonAlias
                {
                    Guid = Guid.NewGuid(),
                    PersonId = personId,
                    ForeignKey = ForeignKey
                };

                new PersonAliasService( rockContext ).Add( alias );
                rockContext.SaveChanges();

                return alias.Guid;
            }
        }

        /// <summary>
        /// Puts a person in a group.
        /// </summary>
        /// <param name="channelGuid">The group.</param>
        /// <param name="personId">The person.</param>
        /// <param name="edit">Anything else to set before it is saved.</param>
        public void AddMember( Guid channelGuid, int personId, Action<GroupMember> edit = null )
        {
            using ( var rockContext = new RockContext() )
            {
                var group = new GroupService( rockContext ).Queryable( "GroupType" ).First( g => g.Guid == channelGuid );

                var member = new GroupMember
                {
                    Guid = Guid.NewGuid(),
                    GroupId = group.Id,
                    GroupTypeId = group.GroupTypeId,
                    PersonId = personId,
                    GroupRoleId = group.GroupType.DefaultGroupRoleId.Value,
                    GroupMemberStatus = GroupMemberStatus.Active,
                    IsArchived = false,
                    ForeignKey = ForeignKey
                };

                edit?.Invoke( member );

                new GroupMemberService( rockContext ).Add( member );
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Changes a group this fixture made.
        /// </summary>
        /// <param name="channelGuid">The group.</param>
        /// <param name="edit">What to change.</param>
        public void EditChannel( Guid channelGuid, Action<Group> edit )
        {
            using ( var rockContext = new RockContext() )
            {
                var group = new GroupService( rockContext ).Queryable().First( g => g.Guid == channelGuid );
                edit( group );
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Writes a group's name past Rock's own validation, for the states a row can only reach
        /// from outside Rock.
        /// </summary>
        /// <param name="channelGuid">The group.</param>
        /// <param name="name">The name to write.</param>
        public void SetChannelNameDirectly( Guid channelGuid, string name )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( "UPDATE [Group] SET [Name] = @p1 WHERE [Guid] = @p0", channelGuid, name );
            }
        }

        /// <summary>
        /// Reads the whole church, exactly as a run would.
        /// </summary>
        /// <returns>The reading.</returns>
        public ProjectedPayload Project()
        {
            using ( var rockContext = new RockContext() )
            {
                var result = ChatPlatformSync.Project( rockContext, Configuration() );

                return new ProjectedPayload( result );
            }
        }

        /// <summary>
        /// Marks the groups that are chat channels right now, exactly as a run would.
        /// </summary>
        public void StampChannels()
        {
            using ( var rockContext = new RockContext() )
            {
                ChatPlatformSync.StampChannels( rockContext );
            }
        }

        /// <summary>
        /// The mark a group is carrying, if any.
        /// </summary>
        /// <param name="channelGuid">The group.</param>
        /// <returns>The mark, or null.</returns>
        public DateTime? ChannelMark( Guid channelGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                return new GroupService( rockContext ).Queryable()
                    .Where( g => g.Guid == channelGuid )
                    .Select( g => g.ChatChannelFirstEnabledDateTime )
                    .First();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            using ( var rockContext = new RockContext() )
            {
                DeletePeopleAndChannels( rockContext, ForeignKey );

                foreach ( var familyGroupId in _createdFamilyGroupIds )
                {
                    rockContext.Database.ExecuteSqlCommand(
                        "DELETE FROM [GroupMember] WHERE [GroupId] = @p0;"
                        + "DELETE FROM [GroupLocation] WHERE [GroupId] = @p0;"
                        + "UPDATE [Person] SET [PrimaryFamilyId] = NULL WHERE [PrimaryFamilyId] = @p0;"
                        + "DELETE FROM [Group] WHERE [Id] = @p0;",
                        familyGroupId );
                }

                foreach ( var groupTypeId in _createdGroupTypeIds )
                {
                    // The type points at its own default role, so that has to be let go of before
                    // the role can be taken away.
                    rockContext.Database.ExecuteSqlCommand(
                        "UPDATE [GroupType] SET [DefaultGroupRoleId] = NULL WHERE [Id] = @p0;"
                        + "DELETE FROM [GroupTypeRole] WHERE [GroupTypeId] = @p0;"
                        + "DELETE FROM [GroupTypeAssociation] WHERE [GroupTypeId] = @p0 OR [ChildGroupTypeId] = @p0;"
                        + "DELETE FROM [GroupType] WHERE [Id] = @p0;",
                        groupTypeId );
                }
            }
        }

        /// <summary>
        /// Takes away everything carrying one foreign key, in the order the foreign keys allow.
        /// </summary>
        /// <remarks>
        /// A person is not a row on their own. Rock hangs a search key off every alias, so the
        /// aliases cannot go until those have, and the person cannot go until the aliases have.
        /// </remarks>
        internal static void DeletePeopleAndChannels( RockContext rockContext, string foreignKey )
        {
            rockContext.Database.ExecuteSqlCommand(
                "DELETE FROM [GroupMember] WHERE [ForeignKey] = @p0;"
                + "DELETE FROM [GroupMember] WHERE [PersonId] IN ( SELECT [Id] FROM [Person] WHERE [ForeignKey] = @p0 );"
                + "DELETE FROM [Group] WHERE [ForeignKey] = @p0;"
                + "DELETE FROM [PersonSearchKey] WHERE [PersonAliasId] IN "
                + "( SELECT [Id] FROM [PersonAlias] WHERE [PersonId] IN ( SELECT [Id] FROM [Person] WHERE [ForeignKey] = @p0 ) );"
                + "UPDATE [Person] SET [PrimaryAliasId] = NULL WHERE [ForeignKey] = @p0;"
                + "DELETE FROM [PersonAlias] WHERE [PersonId] IN ( SELECT [Id] FROM [Person] WHERE [ForeignKey] = @p0 );"
                + "DELETE FROM [Person] WHERE [ForeignKey] = @p0;",
                foreignKey );
        }

        #endregion Methods

        #region Private Methods

        /// <summary>
        /// The settings a reading needs. Nothing here reaches the chat platform, so only the two
        /// defaults the projection reads and the badge list matter.
        /// </summary>
        private static ChatPlatformConfiguration Configuration()
        {
            return new ChatPlatformConfiguration
            {
                AreChatProfilesVisible = true,
                IsOpenDirectMessagingAllowed = true,
                ChatBadgeDataViewGuids = new List<Guid>()
            };
        }

        private int AddChatGroupType( RockContext rockContext, string name, Guid? guid )
        {
            var groupType = new GroupType
            {
                Guid = guid ?? Guid.NewGuid(),
                Name = name + " " + Guid.NewGuid().ToString( "N" ).Substring( 0, 8 ),
                IsChatAllowed = true,
                IsChatEnabledForAllGroups = true,
                IsChatChannelPublic = true,
                IsChatChannelAlwaysShown = true,
                IsLeavingChatChannelAllowed = true,
                CanViewMembers = true,
                IsChatSearchIndexed = true,
                IsSystem = false,
                ShowInGroupList = false,
                ShowInNavigation = false,
                ForeignKey = ForeignKey
            };

            var service = new GroupTypeService( rockContext );
            service.Add( groupType );
            rockContext.SaveChanges();

            var role = new GroupTypeRole
            {
                Guid = Guid.NewGuid(),
                GroupTypeId = groupType.Id,
                Name = "Member",
                ForeignKey = ForeignKey
            };

            rockContext.Set<GroupTypeRole>().Add( role );
            rockContext.SaveChanges();

            groupType.DefaultGroupRoleId = role.Id;
            rockContext.SaveChanges();

            _createdGroupTypeIds.Add( groupType.Id );

            return groupType.Id;
        }

        #endregion Private Methods
    }

    /// <summary>
    /// One reading, with its positional rows addressable by the names the wire contract gives them.
    /// </summary>
    internal sealed class ProjectedPayload
    {
        #region Fields

        private readonly JObject _payload;

        private readonly JObject _contract;

        #endregion Fields

        #region Constructors

        public ProjectedPayload( ChatSyncProjectionResult result )
        {
            Result = result;

            // Dates are deliberately not parsed. A reader that recognised them would hand back a
            // local DateTime, and the whole question about a time on this wire is what was written,
            // not what a reader could make of it. The body is read as the UTF-8 bytes the runner
            // hands to the transport, which is the form the platform receives.
            var body = result.Payload;

            using ( var stream = new MemoryStream( body.Array ?? new byte[0], body.Offset, body.Count, false ) )
            using ( var text = new StreamReader( stream, new UTF8Encoding( false ) ) )
            using ( var reader = new JsonTextReader( text ) { DateParseHandling = DateParseHandling.None } )
            {
                _payload = JObject.Load( reader );
            }

            _contract = JObject.Parse( ChatWireContract.Json );
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// The reading this was parsed from.
        /// </summary>
        public ChatSyncProjectionResult Result { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Every row of a section.
        /// </summary>
        public IList<JArray> Rows( string section )
        {
            return ( ( JArray ) _payload[section] ).Cast<JArray>().ToList();
        }

        /// <summary>
        /// The one row of a section whose named column holds a value, or null.
        /// </summary>
        public JArray Row( string section, string column, object value )
        {
            var index = ColumnIndex( section, column );
            var wanted = value.ToStringSafe();

            return Rows( section ).FirstOrDefault( r => string.Equals( r[index].ToStringSafe(), wanted, StringComparison.OrdinalIgnoreCase ) );
        }

        /// <summary>
        /// One value out of a row, by the name the contract gives its position.
        /// </summary>
        public JToken Value( string section, JArray row, string column )
        {
            return row[ColumnIndex( section, column )];
        }

        /// <summary>
        /// Where a section's column sits in its rows.
        /// </summary>
        public int ColumnIndex( string section, string column )
        {
            // The contract pairs a section with a table by position rather than by name, so the
            // section's place in its own list is what finds the column list.
            var sections = _contract["payload"]["sections"].Select( v => ( string ) v ).ToList();
            var position = sections.IndexOf( section );

            if ( position < 0 )
            {
                throw new InvalidOperationException( string.Format( "the payload carries no {0} section", section ) );
            }

            var table = ( JObject ) _contract["tables"][position];
            var columns = table["columns"].Select( c => ( string ) c ).ToList();
            var index = columns.IndexOf( column );

            if ( index < 0 )
            {
                throw new InvalidOperationException( string.Format( "the {0} section carries no {1} column", section, column ) );
            }

            return index;
        }

        #endregion Methods
    }
}
