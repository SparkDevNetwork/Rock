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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    [MigrationNumber( 324, "17.9" )]
    public class AddAllowedRedirectDomainsToSite : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JPH_AddAllowedRedirectDomainsToSite_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JPH_AddAllowedRedirectDomainsToSite_Down();
        }

        /// <summary>
        /// JPH: Add an Allowed Redirect Domains attribute to Site - up.
        /// </summary>
        private void JPH_AddAllowedRedirectDomainsToSite_Up()
        {
            RockMigrationHelper.AddOrUpdateEntityAttribute(
                entityTypeName: "Rock.Model.Site",
                fieldTypeGuid: Rock.SystemGuid.FieldType.VALUE_LIST,
                entityTypeQualifierColumn: string.Empty,
                entityTypeQualifierValue: string.Empty,
                name: "Allowed Redirect Domain(s)",
                abbreviatedName: "Allowed Redirect Domain(s)",
                description: "The external domains that individuals may be sent to after actions like logging in. The current domain, the organization's website and the public application root are always allowed. Add each domain as its own value (e.g. example.com); use *.example.com to allow any subdomain.",
                order: 0,
                defaultValue: string.Empty,
                guid: Rock.SystemGuid.Attribute.SITE_ALLOWED_REDIRECT_DOMAINS,
                key: "AllowedRedirectDomains"
            );

            /*
                Seed from Allowed Frame Domains so sites that already redirect to those domains keep working.

                1. Replace carriage returns and line feeds with spaces.
                2. Replace commas with spaces.
                3. Trim the result.
                4. Replace spaces with pipes.
                5. Replace double pipes with single pipes (twice to catch excessive delimiters).

                If we're left over with multiple pipes, that's okay;
                the ValueList field type will handle that when it parses the values.
             */
            Sql( $@"
DECLARE @AttributeId INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '{Rock.SystemGuid.Attribute.SITE_ALLOWED_REDIRECT_DOMAINS}');

INSERT INTO [AttributeValue]
(
    [IsSystem]
    , [AttributeId]
    , [EntityId]
    , [Value]
    , [Guid]
    , [IsPersistedValueDirty]
)
SELECT
    0
    , @AttributeId
    , [s].[Id]
    , REPLACE(
        REPLACE(
            REPLACE(
                LTRIM(RTRIM(
                    REPLACE(
                        REPLACE(
                            REPLACE([s].[AllowedFrameDomains]
                            , CHAR(13), ' ')
                        , CHAR(10), ' ')
                    , ',', ' ')
                ))
            , ' ', '|')
        , '||', '|')
      , '||', '|')
    , NEWID()
    , 1
FROM [Site] AS [s]
WHERE LTRIM(RTRIM(ISNULL([s].[AllowedFrameDomains], ''))) <> ''
    AND NOT EXISTS (
        SELECT 1
        FROM [AttributeValue] AS [av]
        WHERE [av].[AttributeId] = @AttributeId
            AND [av].[EntityId] = [s].[Id]
    );" );
        }

        /// <summary>
        /// JPH: Add an Allowed Redirect Domains attribute to Site - down.
        /// </summary>
        private void JPH_AddAllowedRedirectDomainsToSite_Down()
        {
            RockMigrationHelper.DeleteAttribute( Rock.SystemGuid.Attribute.SITE_ALLOWED_REDIRECT_DOMAINS );
        }
    }
}
