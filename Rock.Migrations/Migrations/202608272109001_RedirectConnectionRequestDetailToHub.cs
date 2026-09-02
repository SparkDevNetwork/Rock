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
namespace Rock.Migrations
{

    /// <summary>
    /// Points the shipped Connection Request links at the Connections Hub.
    /// </summary>
    public partial class RedirectConnectionRequestDetailToHub : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// The Connections Hub's "people/connections/hub" system route.
        /// </summary>
        private const string ConnectionsHubPageRouteGuid = "565DFC73-E223-4C52-9174-11BB65700B7B";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            MoveDetailPageAttributeValues( Rock.SystemGuid.Page.CONNECTION_REQUEST_DETAIL, Rock.SystemGuid.Page.CONNECTIONS_HUB, ConnectionsHubPageRouteGuid );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // The Connection Request Detail page has no route, so the value moves back to a bare page reference.
            MoveDetailPageAttributeValues( Rock.SystemGuid.Page.CONNECTIONS_HUB, Rock.SystemGuid.Page.CONNECTION_REQUEST_DETAIL, null );
        }

        /// <summary>
        /// Repoints the detail page settings on My Connection Opportunities, My Connection
        /// Opportunities Lava, Person Profile Connection Requests, and Celebrations Report from one
        /// page to another.
        /// </summary>
        /// <param name="fromPageGuid">The page the setting must currently hold to be changed.</param>
        /// <param name="toPageGuid">The page to point the setting at.</param>
        /// <param name="toPageRouteGuid">The route to pair with the page in configured values, or <c>null</c> when the page has no route.</param>
        private void MoveDetailPageAttributeValues( string fromPageGuid, string toPageGuid, string toPageRouteGuid )
        {
            // A page reference is stored as "Page.Guid" or "Page.Guid,PageRoute.Guid". Only the
            // configured value takes the route; [DefaultValue] is rewritten from the block's code
            // declaration, which holds the page on its own.
            var toValue = toPageRouteGuid.IsNullOrWhiteSpace() ? toPageGuid : $"{toPageGuid},{toPageRouteGuid}";

            /*
                8/27/26 - JPH

                Only a value still holding the page we shipped is moved. An organization may have
                pointed any of these blocks at a page of their own, and that choice has to survive
                the upgrade. The two blocks whose code default also changed are included because
                saving block settings writes a value even when it matches the default, so an
                explicit row can still be pinning the old page.

                Reason: Repointing a partner's configured detail page would discard their setting.
            */
            Sql( $@"
DECLARE @FromPageGuid NVARCHAR(50) = '{fromPageGuid}';
DECLARE @ToPageGuid NVARCHAR(50) = '{toPageGuid}';
DECLARE @ToValue NVARCHAR(100) = '{toValue}';

UPDATE a
SET a.[DefaultValue] = @ToPageGuid
    , a.[DefaultPersistedTextValue] = NULL
    , a.[DefaultPersistedHtmlValue] = NULL
    , a.[DefaultPersistedCondensedTextValue] = NULL
    , a.[DefaultPersistedCondensedHtmlValue] = NULL
    , a.[IsDefaultPersistedValueDirty] = 1
    , a.[ModifiedDateTime] = GETDATE()
FROM [Attribute] a
WHERE EXISTS (
        SELECT 1
        FROM [BlockType] bt
        WHERE a.[EntityTypeQualifierColumn] = 'BlockTypeId'
            AND a.[EntityTypeQualifierValue] = CAST(bt.[Id] AS NVARCHAR(20))
            AND (
                (bt.[Guid] = '1B8E50A0-7AC4-475F-857C-50D0809A3F04' AND a.[Key] = 'DetailPage')                         -- My Connection Opportunities Lava
                OR (bt.[Guid] = '8D3E5A9C-7B2F-4E1D-A6C0-3F9B8D2E5A7C' AND a.[Key] = 'ConnectionRequestDetailPage')     -- Connection Celebrations Report
            )
    )
    AND (a.[DefaultValue] = @FromPageGuid OR a.[DefaultValue] LIKE @FromPageGuid + ',%');

UPDATE av
SET av.[Value] = @ToValue
    , av.[PersistedTextValue] = NULL
    , av.[PersistedHtmlValue] = NULL
    , av.[PersistedCondensedTextValue] = NULL
    , av.[PersistedCondensedHtmlValue] = NULL
    , av.[IsPersistedValueDirty] = 1
    , av.[ModifiedDateTime] = GETDATE()
FROM [AttributeValue] av
WHERE EXISTS (
        SELECT 1
        FROM [Attribute] a
        INNER JOIN [BlockType] bt
            ON a.[EntityTypeQualifierColumn] = 'BlockTypeId'
            AND a.[EntityTypeQualifierValue] = CAST(bt.[Id] AS NVARCHAR(20))
        WHERE a.[Id] = av.[AttributeId]
            AND (
                (bt.[Guid] = '3F69E04F-F966-4CAE-B89D-F97DFEF6407A' AND a.[Key] = 'DetailPage')                         -- My Connection Opportunities
                OR (bt.[Guid] = '1B8E50A0-7AC4-475F-857C-50D0809A3F04' AND a.[Key] = 'DetailPage')                      -- My Connection Opportunities Lava
                OR (bt.[Guid] = '39C53B93-C75A-45DE-B9E7-DFA4EE6B7027' AND a.[Key] = 'ConnectionRequestDetail')         -- Connection Requests
                OR (bt.[Guid] = '8D3E5A9C-7B2F-4E1D-A6C0-3F9B8D2E5A7C' AND a.[Key] = 'ConnectionRequestDetailPage')     -- Connection Celebrations Report
            )
    )
    AND (av.[Value] = @FromPageGuid OR av.[Value] LIKE @FromPageGuid + ',%');" );
        }
    }
}
