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
using Rock.Web;
using Rock.Web.Cache;

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

        private readonly List<Guid> _badgeDataViewGuids = new List<Guid>();

        private readonly List<Action<RockContext>> _restores = new List<Action<RockContext>>();

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
        /// Adds another role to one of this fixture's group types.
        /// </summary>
        /// <param name="groupTypeId">The type, which must be one this fixture made.</param>
        /// <param name="name">The role's name.</param>
        /// <param name="isLeader">Whether holding the role makes a person a leader of the group.</param>
        /// <returns>The role's id.</returns>
        public int AddRole( int groupTypeId, string name, bool isLeader )
        {
            using ( var rockContext = new RockContext() )
            {
                var role = new GroupTypeRole
                {
                    Guid = Guid.NewGuid(),
                    GroupTypeId = groupTypeId,
                    Name = name,
                    IsLeader = isLeader,
                    ForeignKey = ForeignKey
                };

                rockContext.Set<GroupTypeRole>().Add( role );
                rockContext.SaveChanges();

                return role.Id;
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
        /// Gives a group an icon, as a file of a binary file type made for it.
        /// </summary>
        /// <param name="channelGuid">The group.</param>
        /// <param name="requiresViewSecurity">Whether the file's type requires view security.</param>
        /// <returns>The file's guid.</returns>
        /// <remarks>
        /// Written as rows rather than saved through Rock, because the projection reads only the
        /// file's guid and its type, and a save would ask a storage provider for content nothing
        /// here reads.
        /// </remarks>
        public Guid SetChannelIcon( Guid channelGuid, bool requiresViewSecurity )
        {
            var fileGuid = Guid.NewGuid();

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand(
                    "INSERT INTO [BinaryFileType] ( [IsSystem], [Name], [CacheToServerFileSystem], [RequiresViewSecurity], [Guid], [ForeignKey] ) "
                    + "VALUES ( 0, @p0, 0, @p1, NEWID(), @p2 );"
                    + "INSERT INTO [BinaryFile] ( [IsTemporary], [IsSystem], [BinaryFileTypeId], [FileName], [MimeType], [Guid], [ForeignKey] ) "
                    + "VALUES ( 0, 0, SCOPE_IDENTITY(), N'icon.png', N'image/png', @p3, @p2 );"
                    + "UPDATE [Group] SET [ChatChannelAvatarBinaryFileId] = ( SELECT [Id] FROM [BinaryFile] WHERE [Guid] = @p3 ) WHERE [Guid] = @p4;",
                    "Chat icon fixture " + fileGuid.ToString( "N" ).Substring( 0, 8 ),
                    requiresViewSecurity,
                    ForeignKey,
                    fileGuid,
                    channelGuid );
            }

            return fileGuid;
        }

        /// <summary>
        /// Writes a person's nick name past Rock's own save path, for the states a row can only
        /// reach from outside Rock.
        /// </summary>
        /// <param name="personId">The person.</param>
        /// <param name="nickName">The nick name to write.</param>
        public void SetNickNameDirectly( int personId, string nickName )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( "UPDATE [Person] SET [NickName] = @p1 WHERE [Id] = @p0", personId, nickName );
            }
        }

        /// <summary>
        /// Writes a person's last name past Rock's own save path, for the states a row can only
        /// reach from outside Rock.
        /// </summary>
        /// <param name="personId">The person.</param>
        /// <param name="lastName">The last name to write.</param>
        public void SetLastNameDirectly( int personId, string lastName )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( "UPDATE [Person] SET [LastName] = @p1 WHERE [Id] = @p0", personId, lastName );
            }
        }

        /// <summary>
        /// Puts many more people in a group, each as a copy of one person already in it, written as
        /// rows past Rock's save path so that no save hook sees them.
        /// </summary>
        /// <param name="channelGuid">The group.</param>
        /// <param name="templatePersonId">A person this fixture made who is already a member of the group.</param>
        /// <param name="count">How many people to add.</param>
        /// <remarks>
        /// Each copy gets its own guid and its own primary alias, and carries this fixture's foreign
        /// key, so disposing takes them away with everything else. Columns are copied by name from
        /// the catalog, so a column Rock adds later is copied too.
        /// </remarks>
        public void SeedMembersDirectly( Guid channelGuid, int templatePersonId, int count )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = 120;
                rockContext.Database.ExecuteSqlCommand(
                    @"
DECLARE @TemplateMemberId INT = (
    SELECT TOP 1 [GM].[Id]
    FROM [GroupMember] AS [GM]
    INNER JOIN [Group] AS [G] ON [G].[Id] = [GM].[GroupId]
    WHERE [G].[Guid] = @p0 AND [GM].[PersonId] = @p1 );

DECLARE @PersonColumns NVARCHAR(MAX) = (
    SELECT STRING_AGG( QUOTENAME( [name] ), ',' )
    FROM sys.columns
    WHERE [object_id] = OBJECT_ID( 'Person' )
        AND [is_identity] = 0 AND [is_computed] = 0
        AND [name] NOT IN ( 'Guid', 'PrimaryAliasId', 'PrimaryFamilyId', 'GivingGroupId' ) );

DECLARE @MemberColumns NVARCHAR(MAX) = (
    SELECT STRING_AGG( QUOTENAME( [name] ), ',' )
    FROM sys.columns
    WHERE [object_id] = OBJECT_ID( 'GroupMember' )
        AND [is_identity] = 0 AND [is_computed] = 0
        AND [name] NOT IN ( 'Guid', 'PersonId' ) );

CREATE TABLE #Seeded ( [Id] INT NOT NULL );

DECLARE @Sql NVARCHAR(MAX) = N'INSERT INTO [Person] ( ' + @PersonColumns + N', [Guid] ) OUTPUT inserted.[Id] INTO #Seeded '
    + N'SELECT ' + @PersonColumns + N', NEWID() FROM [Person] '
    + N'CROSS JOIN ( SELECT TOP ( @Count ) 1 AS [N] FROM sys.all_objects AS [A] CROSS JOIN sys.all_objects AS [B] ) AS [Numbers] '
    + N'WHERE [Person].[Id] = @TemplatePersonId;';
EXEC sp_executesql @Sql, N'@Count INT, @TemplatePersonId INT', @p2, @p1;

INSERT INTO [PersonAlias] ( [PersonId], [AliasPersonId], [AliasPersonGuid], [Guid], [ForeignKey] )
SELECT [P].[Id], [P].[Id], [P].[Guid], NEWID(), [P].[ForeignKey]
FROM [Person] AS [P]
INNER JOIN #Seeded AS [S] ON [S].[Id] = [P].[Id];

UPDATE [P]
SET [P].[PrimaryAliasId] = [PA].[Id]
FROM [Person] AS [P]
INNER JOIN #Seeded AS [S] ON [S].[Id] = [P].[Id]
INNER JOIN [PersonAlias] AS [PA] ON [PA].[AliasPersonId] = [P].[Id];

SET @Sql = N'INSERT INTO [GroupMember] ( ' + @MemberColumns + N', [PersonId], [Guid] ) '
    + N'SELECT ' + @MemberColumns + N', [S].[Id], NEWID() FROM [GroupMember] CROSS JOIN #Seeded AS [S] '
    + N'WHERE [GroupMember].[Id] = @TemplateMemberId;';
EXEC sp_executesql @Sql, N'@TemplateMemberId INT', @TemplateMemberId;

DROP TABLE #Seeded;",
                    channelGuid,
                    templatePersonId,
                    count );
            }
        }

        /// <summary>
        /// Stores chat settings as enabling chat and the settings screen would, so that code which
        /// reads the church's own settings finds them. Put back when disposed.
        /// </summary>
        /// <param name="configuration">The settings, with the signing key in the clear.</param>
        public void StoreConfiguration( ChatPlatformConfiguration configuration )
        {
            var stored = SystemSettings.GetValue( Rock.SystemKey.SystemSetting.CHAT_PLATFORM_CONFIGURATION );
            _restores.Add( context => SystemSettings.SetValue( Rock.SystemKey.SystemSetting.CHAT_PLATFORM_CONFIGURATION, stored ) );

            ChatPlatformConfigurationService.SaveChurchSettings( configuration );
            ChatPlatformConfigurationService.SavePlatformCredentials( new ConnectedServicesChatEntry
            {
                TenantId = configuration.TenantId.Value,
                ProjectUrl = configuration.ProjectUrl,
                PublishableKey = configuration.PublishableKey,
                Kid = configuration.Kid,
                PrivateKey = configuration.PrivateKey
            } );
        }

        /// <summary>
        /// Makes a new signing key pair, as enabling chat gives a church.
        /// </summary>
        /// <param name="kid">The key's id.</param>
        /// <returns>The private key as the settings store it, and the public key as the platform registers it.</returns>
        public static (string PrivateJwk, JObject PublicJwk) CreateSigningKey( string kid )
        {
            using ( var ecdsa = System.Security.Cryptography.ECDsa.Create( System.Security.Cryptography.ECCurve.NamedCurves.nistP256 ) )
            {
                var key = new Microsoft.IdentityModel.Tokens.ECDsaSecurityKey( ecdsa ) { KeyId = kid };
                var jwk = Microsoft.IdentityModel.Tokens.JsonWebKeyConverter.ConvertFromECDsaSecurityKey( key );
                jwk.Kid = kid;
                jwk.Use = "sig";
                jwk.Alg = "ES256";

                var publicJwk = new JObject
                {
                    ["kty"] = "EC",
                    ["crv"] = "P-256",
                    ["kid"] = kid,
                    ["x"] = jwk.X,
                    ["y"] = jwk.Y
                };

                return (JsonConvert.SerializeObject( jwk ), publicJwk);
            }
        }

        /// <summary>
        /// Makes the code that follows run as though inside a web request, as a save from a block
        /// or the API does, until the result is disposed.
        /// </summary>
        /// <returns>The request, ended when disposed.</returns>
        /// <remarks>
        /// Rock marks a request with an ambient value that flows to the code the request calls, so
        /// setting it here is exactly what a real request does, minus the request.
        /// </remarks>
        public static IDisposable InsideRequest()
        {
            var accessor = new Rock.Net.RockRequestContextAccessor
            {
                RockRequestContext = new Rock.Net.RockRequestContext()
            };

            return new RequestScope( accessor );
        }

        /// <summary>
        /// Adds a persisted person Data View and puts it on the church's badge list.
        /// </summary>
        /// <param name="name">The Data View's name.</param>
        /// <returns>The Data View's guid, which is the badge's key.</returns>
        public Guid AddBadge( string name )
        {
            return AddBadge( name, dataView => dataView.PersistedScheduleIntervalMinutes = 60 );
        }

        /// <summary>
        /// Adds a person Data View persisted on a named schedule rather than an interval, and puts
        /// it on the church's badge list.
        /// </summary>
        /// <param name="name">The Data View's name.</param>
        /// <returns>The Data View's guid, which is the badge's key.</returns>
        public Guid AddBadgeOnSchedule( string name )
        {
            int scheduleId;

            using ( var rockContext = new RockContext() )
            {
                var schedule = new Schedule
                {
                    Guid = Guid.NewGuid(),
                    Name = name + " schedule",
                    ForeignKey = ForeignKey
                };

                new ScheduleService( rockContext ).Add( schedule );
                rockContext.SaveChanges();

                scheduleId = schedule.Id;
            }

            return AddBadge( name, dataView => dataView.PersistedScheduleId = scheduleId );
        }

        /// <summary>
        /// Adds a persisted group Data View and puts it on the church's badge list, as a settings
        /// call that skipped the picker could.
        /// </summary>
        /// <param name="name">The Data View's name.</param>
        /// <returns>The Data View's guid.</returns>
        public Guid AddGroupDataViewBadge( string name )
        {
            return AddBadge( name, dataView =>
            {
                dataView.EntityTypeId = EntityTypeCache.GetId<Group>().Value;
                dataView.PersistedScheduleIntervalMinutes = 60;
            } );
        }

        /// <summary>
        /// Adds a person Data View that is not persisted and puts it on the church's badge list.
        /// </summary>
        /// <param name="name">The Data View's name.</param>
        /// <returns>The Data View's guid.</returns>
        public Guid AddUnpersistedBadge( string name )
        {
            return AddBadge( name, dataView => { } );
        }

        /// <summary>
        /// Puts a Data View that does not exist on the church's badge list, as one deleted after it
        /// was chosen would leave it.
        /// </summary>
        /// <returns>The guid on the list.</returns>
        public Guid AddMissingBadge()
        {
            var badgeGuid = Guid.NewGuid();
            _badgeDataViewGuids.Add( badgeGuid );

            return badgeGuid;
        }

        /// <summary>
        /// Puts a badge on the church's badge list a second time, after everything already on it.
        /// </summary>
        /// <param name="badgeGuid">The Data View.</param>
        public void RepeatBadge( Guid badgeGuid )
        {
            _badgeDataViewGuids.Add( badgeGuid );
        }

        /// <summary>
        /// Puts a person in a badge Data View's persisted values, as a refresh of it would.
        /// </summary>
        /// <param name="badgeGuid">The Data View.</param>
        /// <param name="personId">The person.</param>
        public void GiveBadge( Guid badgeGuid, int personId )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand(
                    "INSERT INTO [DataViewPersistedValue] ( [DataViewId], [EntityId] ) SELECT [Id], @p1 FROM [DataView] WHERE [Guid] = @p0",
                    badgeGuid,
                    personId );
            }
        }

        /// <summary>
        /// Writes a badge Data View's name past Rock's own validation, for the states a row can
        /// only reach from outside Rock.
        /// </summary>
        /// <param name="badgeGuid">The Data View.</param>
        /// <param name="name">The name to write.</param>
        public void SetBadgeNameDirectly( Guid badgeGuid, string name )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( "UPDATE [DataView] SET [Name] = @p1 WHERE [Guid] = @p0", badgeGuid, name );
            }
        }

        /// <summary>
        /// Turns chat on for every group of an existing group's type, and clears that group's own
        /// chat setting, as an administrator could. Put back, marks included, when disposed.
        /// </summary>
        /// <param name="groupGuid">The existing group, such as one Rock ships.</param>
        /// <returns>The group type's id.</returns>
        public int EnableChatForGroupTypeOf( Guid groupGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var database = rockContext.Database;
                var groupTypeId = database.SqlQuery<int>( "SELECT [GroupTypeId] FROM [Group] WHERE [Guid] = @p0", groupGuid ).First();
                var groupOverride = database.SqlQuery<bool?>( "SELECT [IsChatEnabledOverride] FROM [Group] WHERE [Guid] = @p0", groupGuid ).First();
                var isChatAllowed = database.SqlQuery<bool>( "SELECT [IsChatAllowed] FROM [GroupType] WHERE [Id] = @p0", groupTypeId ).First();
                var isChatEnabledForAllGroups = database.SqlQuery<bool>( "SELECT [IsChatEnabledForAllGroups] FROM [GroupType] WHERE [Id] = @p0", groupTypeId ).First();
                var unmarkedGroupIds = string.Join( ",", database.SqlQuery<int>(
                    "SELECT [Id] FROM [Group] WHERE [GroupTypeId] = @p0 AND [ChatChannelFirstEnabledDateTime] IS NULL", groupTypeId ).ToList() );

                // A stamp run while this is on marks every group of the type, so those marks go too.
                _restores.Add( context => context.Database.ExecuteSqlCommand(
                    "UPDATE [GroupType] SET [IsChatAllowed] = @p1, [IsChatEnabledForAllGroups] = @p2 WHERE [Id] = @p0;"
                    + "UPDATE [Group] SET [IsChatEnabledOverride] = @p4 WHERE [Guid] = @p3;"
                    + "UPDATE [Group] SET [ChatChannelFirstEnabledDateTime] = NULL WHERE [Id] IN ( SELECT CAST( [value] AS INT ) FROM STRING_SPLIT( @p5, ',' ) );",
                    groupTypeId,
                    isChatAllowed,
                    isChatEnabledForAllGroups,
                    groupGuid,
                    ( object ) groupOverride ?? DBNull.Value,
                    unmarkedGroupIds ) );

                database.ExecuteSqlCommand(
                    "UPDATE [GroupType] SET [IsChatAllowed] = 1, [IsChatEnabledForAllGroups] = 1 WHERE [Id] = @p0;"
                    + "UPDATE [Group] SET [IsChatEnabledOverride] = NULL WHERE [Guid] = @p1;",
                    groupTypeId,
                    groupGuid );

                return groupTypeId;
            }
        }

        /// <summary>
        /// Writes a channel mark on an existing group, as a run before a rule changed could have
        /// left it. Put back when disposed.
        /// </summary>
        /// <param name="groupGuid">The group.</param>
        public void MarkChannelDirectly( Guid groupGuid )
        {
            var mark = ChannelMark( groupGuid );

            _restores.Add( context => context.Database.ExecuteSqlCommand(
                "UPDATE [Group] SET [ChatChannelFirstEnabledDateTime] = @p1 WHERE [Guid] = @p0",
                groupGuid,
                ( object ) mark ?? DBNull.Value ) );

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand(
                    "UPDATE [Group] SET [ChatChannelFirstEnabledDateTime] = @p1 WHERE [Guid] = @p0",
                    groupGuid,
                    RockDateTime.Now );
            }
        }

        /// <summary>
        /// Marks the groups that are chat channels right now and reads the whole church, exactly as
        /// a run would.
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
        /// Reads back only what one save touched, as the immediate sync does after the save commits.
        /// </summary>
        /// <param name="changes">The keys the save touched.</param>
        /// <returns>The push body.</returns>
        public ChatPlatformSyncHelper.PushBody ProjectChanges( ChatPlatformSyncHelper.ImmediateChanges changes )
        {
            using ( var rockContext = new RockContext() )
            {
                return ChatPlatformSyncHelper.ProjectChanges( rockContext, Configuration(), changes );
            }
        }

        /// <summary>
        /// The guid of a person's primary alias, which is the key a membership row carries.
        /// </summary>
        /// <param name="personId">The person.</param>
        /// <returns>The alias guid.</returns>
        public Guid PrimaryAliasGuid( int personId )
        {
            using ( var rockContext = new RockContext() )
            {
                return new PersonService( rockContext ).Get( personId ).PrimaryAlias.Guid;
            }
        }

        /// <summary>
        /// Takes a person out of a group by deleting every membership they hold in it.
        /// </summary>
        /// <param name="channelGuid">The group.</param>
        /// <param name="personId">The person.</param>
        public void DeleteMember( Guid channelGuid, int personId )
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new GroupMemberService( rockContext );
                var members = service.Queryable().Where( m => m.Group.Guid == channelGuid && m.PersonId == personId ).ToList();

                service.DeleteRange( members );
                rockContext.SaveChanges();
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
                // Newest first, so a group changed twice ends where it began.
                for ( var i = _restores.Count - 1; i >= 0; i-- )
                {
                    _restores[i]( rockContext );
                }

                DeletePeopleAndChannels( rockContext, ForeignKey );

                // A Data View's persisted values go with it; its schedule is let go of after it. A
                // channel icon goes once the group that pointed at it has, and its type after it.
                rockContext.Database.ExecuteSqlCommand(
                    "DELETE FROM [DataView] WHERE [ForeignKey] = @p0;"
                    + "DELETE FROM [Schedule] WHERE [ForeignKey] = @p0;"
                    + "DELETE FROM [BinaryFile] WHERE [ForeignKey] = @p0;"
                    + "DELETE FROM [BinaryFileType] WHERE [ForeignKey] = @p0;",
                    ForeignKey );

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
                // A merge of two people with different last names records the one that went.
                + "DELETE FROM [PersonPreviousName] WHERE [PersonAliasId] IN "
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
        /// defaults the projection reads and the badge list matter. The badge list is the badges
        /// this fixture made, in the order it made them.
        /// </summary>
        private ChatPlatformConfiguration Configuration()
        {
            return new ChatPlatformConfiguration
            {
                AreChatProfilesVisible = true,
                IsOpenDirectMessagingAllowed = true,
                ChatBadgeDataViewGuids = _badgeDataViewGuids.ToList()
            };
        }

        private Guid AddBadge( string name, Action<DataView> persist )
        {
            using ( var rockContext = new RockContext() )
            {
                var dataView = new DataView
                {
                    Guid = Guid.NewGuid(),
                    Name = name,
                    EntityTypeId = EntityTypeCache.GetId<Person>().Value,
                    ForeignKey = ForeignKey
                };

                persist( dataView );

                rockContext.Set<DataView>().Add( dataView );
                rockContext.SaveChanges();

                _badgeDataViewGuids.Add( dataView.Guid );

                return dataView.Guid;
            }
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

        #region Support Classes

        /// <summary>
        /// Ends a simulated request when disposed.
        /// </summary>
        private sealed class RequestScope : IDisposable
        {
            private readonly Rock.Net.RockRequestContextAccessor _accessor;

            public RequestScope( Rock.Net.RockRequestContextAccessor accessor )
            {
                _accessor = accessor;
            }

            public void Dispose()
            {
                _accessor.RockRequestContext = null;
            }
        }

        #endregion Support Classes
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

        public ProjectedPayload( ChatPlatformSync.ProjectionResult result )
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
        public ChatPlatformSync.ProjectionResult Result { get; }

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
