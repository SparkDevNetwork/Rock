// <copyright>
// Copyright by the Central Christian Church
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
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Plugin;
using Rock.Web.Cache;

namespace com.centralaz.RoomManagement.Migrations
{
    [MigrationNumber( 3, "1.4.5" )]
    public class SampleData : Migration
    {
        public override void Up()
        {
            int entityTypeId = EntityTypeCache.Read( com.centralaz.RoomManagement.SystemGuid.EntityType.RESOURCE.AsGuid() ).Id;
            Sql( String.Format( @"INSERT [dbo].[Category] ( [IsSystem], [ParentCategoryId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Name], [IconCssClass], [Guid], [Order], [Description], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [HighlightColor], [ForeignGuid], [ForeignId]) VALUES ( 0, NULL, {0}, NULL, NULL, N'Vehicles', N'', N'ae3f4a8d-46d7-4520-934c-85d80167b22c', 0, N'', CAST(N'2016-06-08 09:03:33.357' AS DateTime), CAST(N'2016-06-08 09:03:33.357' AS DateTime), 10, 10, NULL, N'', NULL, NULL)
                    INSERT [dbo].[Category] ( [IsSystem], [ParentCategoryId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Name], [IconCssClass], [Guid], [Order], [Description], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [HighlightColor], [ForeignGuid], [ForeignId]) VALUES ( 0, NULL, {0}, NULL, NULL, N'Tables', N'', N'baf88943-64ea-4a6a-8e1e-f4efc5a6ceca', 1, N'', CAST(N'2016-06-08 09:03:37.957' AS DateTime), CAST(N'2016-06-08 09:03:37.957' AS DateTime), 10, 10, NULL, N'', NULL, NULL)
                    INSERT [dbo].[Category] ( [IsSystem], [ParentCategoryId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Name], [IconCssClass], [Guid], [Order], [Description], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [HighlightColor], [ForeignGuid], [ForeignId]) VALUES ( 0, NULL, {0}, NULL, NULL, N'Projectors', N'', N'd29a2afc-bd90-428b-9065-2ffd09fb6f6b', 2, N'', CAST(N'2016-06-08 09:03:43.350' AS DateTime), CAST(N'2016-06-08 09:03:43.350' AS DateTime), 10, 10, NULL, N'', NULL, NULL)
                    INSERT [dbo].[Category] ( [IsSystem], [ParentCategoryId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Name], [IconCssClass], [Guid], [Order], [Description], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [HighlightColor], [ForeignGuid], [ForeignId]) VALUES ( 0, NULL, {0}, NULL, NULL, N'Chairs', N'', N'355ac2fd-0831-4a11-9294-5568fdfa8fc3', 3, N'', CAST(N'2016-06-08 09:03:48.073' AS DateTime), CAST(N'2016-06-08 09:03:48.073' AS DateTime), 10, 10, NULL, N'', NULL, NULL)
                    INSERT [dbo].[Category] ( [IsSystem], [ParentCategoryId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Name], [IconCssClass], [Guid], [Order], [Description], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [HighlightColor], [ForeignGuid], [ForeignId]) VALUES ( 0, NULL, {0}, NULL, NULL, N'Other', N'', N'ddede1a7-c02b-4322-9d5b-a73cdb9224c6', 4, N'', CAST(N'2016-06-08 09:03:52.830' AS DateTime), CAST(N'2016-06-08 09:03:52.830' AS DateTime), 10, 10, NULL, N'', NULL, NULL)
                    INSERT [dbo].[_com_centralaz_RoomManagement_Resource] ( [Name], [CategoryId], [CampusId], [Quantity], [Note], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ForeignGuid], [ForeignId]) VALUES ( N'Van 14', 179, 1, 1, N'', N'18245995-0847-4a76-a86e-6bd02b4b49a3', CAST(N'2016-06-08 09:04:13.653' AS DateTime), CAST(N'2016-06-08 09:04:13.653' AS DateTime), 10, 10, NULL, NULL, NULL)
                    INSERT [dbo].[_com_centralaz_RoomManagement_Resource] ( [Name], [CategoryId], [CampusId], [Quantity], [Note], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ForeignGuid], [ForeignId]) VALUES ( N'Baptism Tub', 183, 1, 1, N'', N'483cc882-6bf0-4e0c-b773-138df685c7df', CAST(N'2016-06-08 09:04:26.420' AS DateTime), CAST(N'2016-06-08 09:04:26.420' AS DateTime), 10, 10, NULL, NULL, NULL)
                    INSERT [dbo].[_com_centralaz_RoomManagement_Resource] ( [Name], [CategoryId], [CampusId], [Quantity], [Note], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ForeignGuid], [ForeignId]) VALUES ( N'Lobby Tables', 180, 1, 12, N'', N'0180e05c-8dbb-44fc-875e-435a81bd994c', CAST(N'2016-06-08 09:04:38.640' AS DateTime), CAST(N'2016-06-08 09:04:38.640' AS DateTime), 10, 10, NULL, NULL, NULL)
                    INSERT [dbo].[_com_centralaz_RoomManagement_Resource] ( [Name], [CategoryId], [CampusId], [Quantity], [Note], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ForeignGuid], [ForeignId]) VALUES ( N'Children''s Portable Projector', 181, 1, 1, N'', N'9b82ce62-36db-4f62-8b51-24b140496a00', CAST(N'2016-06-08 09:04:56.143' AS DateTime), CAST(N'2016-06-08 09:04:56.143' AS DateTime), 10, 10, NULL, NULL, NULL)
                    INSERT [dbo].[_com_centralaz_RoomManagement_Resource] ( [Name], [CategoryId], [CampusId], [Quantity], [Note], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ForeignGuid], [ForeignId]) VALUES ( N'Gym Chairs', 182, 1, 140, N'', N'b3aff3ac-762b-4d45-800d-08195bd5b849', CAST(N'2016-06-08 09:05:09.053' AS DateTime), CAST(N'2016-06-08 09:05:09.053' AS DateTime), 10, 10, NULL, NULL, NULL)
                    INSERT [dbo].[_com_centralaz_RoomManagement_ReservationMinistry] ( [Name], [Description], [Order], [IsActive], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ForeignGuid], [ForeignId]) VALUES ( N'Children''s', NULL, 0, 1, N'd11d9ddf-7ee1-449f-a1b1-e2a281440a1f', CAST(N'2016-08-24 11:53:01.513' AS DateTime), CAST(N'2016-08-24 11:53:01.513' AS DateTime), 10, 10, NULL, NULL, NULL)
                    INSERT [dbo].[_com_centralaz_RoomManagement_ReservationMinistry] ( [Name], [Description], [Order], [IsActive], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ForeignGuid], [ForeignId]) VALUES ( N'Local Outreach', NULL, 0, 1, N'51772c5f-f359-4592-a2ea-beaa8a8cb03d', CAST(N'2016-08-24 11:53:01.513' AS DateTime), CAST(N'2016-08-24 11:53:01.513' AS DateTime), 10, 10, NULL, NULL, NULL)
                    INSERT [dbo].[_com_centralaz_RoomManagement_ReservationMinistry] ( [Name], [Description], [Order], [IsActive], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ForeignGuid], [ForeignId]) VALUES ( N'Men''s', NULL, 0, 1, N'2650ddad-514a-4360-855d-13a8c6a6ca69', CAST(N'2016-08-24 11:53:01.513' AS DateTime), CAST(N'2016-08-24 11:53:01.513' AS DateTime), 10, 10, NULL, NULL, NULL)
                    INSERT [dbo].[_com_centralaz_RoomManagement_ReservationMinistry] ( [Name], [Description], [Order], [IsActive], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ForeignGuid], [ForeignId]) VALUES ( N'Women''s', NULL, 0, 1, N'dc93a516-616b-4ed1-962f-65cc7a3cc13f', CAST(N'2016-08-24 11:53:01.513' AS DateTime), CAST(N'2016-08-24 11:53:01.513' AS DateTime), 10, 10, NULL, NULL, NULL)
                    INSERT [dbo].[_com_centralaz_RoomManagement_ReservationMinistry] ( [Name], [Description], [Order], [IsActive], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ForeignGuid], [ForeignId]) VALUES ( N'Global Outreach', NULL, 0, 1, N'b84d0e5a-806e-4058-9221-5f523fe8c468', CAST(N'2016-08-24 11:53:01.513' AS DateTime), CAST(N'2016-08-24 11:53:01.513' AS DateTime), 10, 10, NULL, NULL, NULL)
                    INSERT [dbo].[_com_centralaz_RoomManagement_ReservationMinistry] ( [Name], [Description], [Order], [IsActive], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ForeignGuid], [ForeignId]) VALUES ( N'Students', NULL, 0, 1, N'13c1f103-b84a-4f7d-9e7d-4c3ff3a090a4', CAST(N'2016-08-24 11:53:01.513' AS DateTime), CAST(N'2016-08-24 11:53:01.513' AS DateTime), 10, 10, NULL, NULL, NULL)
                    INSERT [dbo].[_com_centralaz_RoomManagement_ReservationStatus] ( [IsSystem], [Name], [Description], [IsCritical], [IsDefault], [IsActive], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ForeignGuid], [ForeignId]) VALUES ( 1, N'Unapproved', N'', 0, 1, 1, N'e739f883-8b84-4755-92c0-3db6606381f1', CAST(N'2016-08-23 21:59:31.927' AS DateTime), CAST(N'2016-08-23 21:59:31.927' AS DateTime), 10, 10, NULL, NULL, NULL)
                    INSERT [dbo].[_com_centralaz_RoomManagement_ReservationStatus] ( [IsSystem], [Name], [Description], [IsCritical], [IsDefault], [IsActive], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ForeignGuid], [ForeignId]) VALUES ( 1, N'Approved', N'', 0, 0, 1, N'd11163c8-4684-471f-9043-e976c75091e8', CAST(N'2016-08-23 21:59:31.927' AS DateTime), CAST(N'2016-08-23 21:59:31.927' AS DateTime), 10, 10, NULL, NULL, NULL)
                    INSERT [dbo].[_com_centralaz_RoomManagement_ReservationStatus] ( [IsSystem], [Name], [Description], [IsCritical], [IsDefault], [IsActive], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ForeignGuid], [ForeignId]) VALUES ( 1, N'Denied', N'', 0, 0, 1, N'79A4347E-C399-403A-9053-8FB836354D77', CAST(N'2016-08-23 21:59:31.927' AS DateTime), CAST(N'2016-08-23 21:59:31.927' AS DateTime), 10, 10, NULL, NULL, NULL)
", entityTypeId ) );

            AddSecurityAuthForReservationStatus( "E739F883-8B84-4755-92C0-3DB6606381F1", 0, "Edit", true, Rock.SystemGuid.Group.GROUP_ADMINISTRATORS, Rock.Model.SpecialRole.None, "0AD21D60-C750-4FD7-9D89-106692854BA4" );
            AddSecurityAuthForReservationStatus( "E739F883-8B84-4755-92C0-3DB6606381F1", 1, "Edit", true, "FBE0324F-F29A-4ACF-8EC3-5386C5562D70", Rock.Model.SpecialRole.None, "CCBC6C0C-EEDB-4B55-9D07-528253624FD0" );

        }
        public override void Down()
        {
        }

        public void AddSecurityAuthForReservationStatus( string statusGuid, int order, string action, bool allow, string groupGuid, Rock.Model.SpecialRole specialRole, string authGuid )
        {
            if ( string.IsNullOrWhiteSpace( groupGuid ) )
            {
                groupGuid = Guid.Empty.ToString();
            }

            string entityTypeName = "com.centralaz.RoomManagement.Model.ReservationStatus";
            EnsureEntityTypeExists( entityTypeName );

            string sql = @"
    DECLARE @EntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [name] = '{0}')
    DECLARE @ReservationStatusId int = (SELECT TOP 1 [Id] FROM [_com_centralaz_RoomManagement_ReservationStatus] WHERE [Guid] = '{1}')
    IF @EntityTypeId IS NOT NULL AND @ReservationStatusId IS NOT NULL
    BEGIN
        DECLARE @GroupId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '{2}')
        IF NOT EXISTS (
            SELECT [Id] FROM [dbo].[Auth]
            WHERE [EntityTypeId] = @EntityTypeId
            AND [EntityId] = @ReservationStatusId
            AND [Action] = '{4}'
            AND [AllowOrDeny] = '{5}'
            AND [SpecialRole] = {6}
            AND [GroupId] = @GroupId
        )
        BEGIN
            INSERT INTO [dbo].[Auth]
                   ([EntityTypeId]
                   ,[EntityId]
                   ,[Order]
                   ,[Action]
                   ,[AllowOrDeny]
                   ,[SpecialRole]
                   ,[GroupId]
                   ,[Guid])
             VALUES
                   (@EntityTypeId
                   ,@ReservationStatusId
                   ,{3}
                   ,'{4}'
                   ,'{5}'
                   ,{6}
                   ,@GroupId
                   ,'{7}')
        END
    END
";

            Sql( string.Format( sql,
                entityTypeName,                 // 0
                statusGuid,                   // 1
                groupGuid,                      // 2
                order,                          // 3
                action,                         // 4
                ( allow ? "A" : "D" ),          // 5
                specialRole.ConvertToInt(),     // 6
                authGuid ) );                   // 7

        }

        private void EnsureEntityTypeExists( string entityTypeName, bool isEntity = true, bool isSecured = true )
        {
            // NOTE: If it doesn't exist, add it assuming that IsEntity=True and IsSecured=True.  The framework will correct it if those assumptions are incorrect
            Sql( string.Format( @"
                if not exists (
                select id from EntityType where name = '{0}')
                begin
                INSERT INTO [EntityType]
                           ([Name]
                           ,[FriendlyName]
                           ,[IsEntity]
                           ,[IsSecured]
                           ,[IsCommon]
                           ,[Guid])
                     VALUES
                           ('{0}'
                           ,null
                           ,{1}
                           ,{2}
                           ,0
                           ,newid()
                           )
                end"
                , entityTypeName
                , isEntity ? 1 : 0
                , isSecured ? 1 : 0
                )
            );
        }
    }
}
