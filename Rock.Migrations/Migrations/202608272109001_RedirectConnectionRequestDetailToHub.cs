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
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            MoveDetailPageAttributeValues( Rock.SystemGuid.Page.CONNECTION_REQUEST_DETAIL, Rock.SystemGuid.Page.CONNECTIONS_HUB );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            MoveDetailPageAttributeValues( Rock.SystemGuid.Page.CONNECTIONS_HUB, Rock.SystemGuid.Page.CONNECTION_REQUEST_DETAIL );
        }

        /// <summary>
        /// Repoints the detail page settings on My Connection Opportunities, My Connection
        /// Opportunities Lava, Person Profile Connection Requests, and Celebrations Report from one
        /// page to another.
        /// </summary>
        /// <param name="fromPageGuid">The page the setting must currently hold to be changed.</param>
        /// <param name="toPageGuid">The page to point the setting at.</param>
        private void MoveDetailPageAttributeValues( string fromPageGuid, string toPageGuid )
        {
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
DECLARE @FromPageGuid UNIQUEIDENTIFIER = '{fromPageGuid}';
DECLARE @ToPageGuid UNIQUEIDENTIFIER = '{toPageGuid}';

UPDATE a
SET a.[DefaultValue] = CAST(@ToPageGuid AS NVARCHAR(50))
    , a.[DefaultPersistedTextValue] = NULL
    , a.[DefaultPersistedHtmlValue] = NULL
    , a.[DefaultPersistedCondensedTextValue] = NULL
    , a.[DefaultPersistedCondensedHtmlValue] = NULL
    , a.[IsDefaultPersistedValueDirty] = 1
    , a.[ModifiedDateTime] = GETDATE()
FROM [Attribute] a
INNER JOIN [BlockType] bt
    ON a.[EntityTypeQualifierColumn] = 'BlockTypeId'
    AND a.[EntityTypeQualifierValue] = CAST(bt.[Id] AS NVARCHAR(20))
WHERE TRY_CAST(a.[DefaultValue] AS UNIQUEIDENTIFIER) = @FromPageGuid
    AND (
        (bt.[Guid] = '1B8E50A0-7AC4-475F-857C-50D0809A3F04' AND a.[Key] = 'DetailPage')                         -- My Connection Opportunities Lava
        OR (bt.[Guid] = '8D3E5A9C-7B2F-4E1D-A6C0-3F9B8D2E5A7C' AND a.[Key] = 'ConnectionRequestDetailPage')     -- Connection Celebrations Report
    );

UPDATE av
SET av.[Value] = CAST(@ToPageGuid AS NVARCHAR(50))
    , av.[PersistedTextValue] = NULL
    , av.[PersistedHtmlValue] = NULL
    , av.[PersistedCondensedTextValue] = NULL
    , av.[PersistedCondensedHtmlValue] = NULL
    , av.[IsPersistedValueDirty] = 1
    , av.[ModifiedDateTime] = GETDATE()
FROM [AttributeValue] av
INNER JOIN [Attribute] a
    ON a.[Id] = av.[AttributeId]
INNER JOIN [BlockType] bt
    ON a.[EntityTypeQualifierColumn] = 'BlockTypeId'
    AND a.[EntityTypeQualifierValue] = CAST(bt.[Id] AS NVARCHAR(20))
WHERE TRY_CAST(av.[Value] AS UNIQUEIDENTIFIER) = @FromPageGuid
    AND (
        (bt.[Guid] = '3F69E04F-F966-4CAE-B89D-F97DFEF6407A' AND a.[Key] = 'DetailPage')                         -- My Connection Opportunities
        OR (bt.[Guid] = '1B8E50A0-7AC4-475F-857C-50D0809A3F04' AND a.[Key] = 'DetailPage')                      -- My Connection Opportunities Lava
        OR (bt.[Guid] = '39C53B93-C75A-45DE-B9E7-DFA4EE6B7027' AND a.[Key] = 'ConnectionRequestDetail')         -- Connection Requests
        OR (bt.[Guid] = '8D3E5A9C-7B2F-4E1D-A6C0-3F9B8D2E5A7C' AND a.[Key] = 'ConnectionRequestDetailPage')     -- Connection Celebrations Report
    );" );
        }
    }
}
