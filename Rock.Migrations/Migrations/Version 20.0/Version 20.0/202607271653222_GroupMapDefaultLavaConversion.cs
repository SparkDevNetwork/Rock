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
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Replaces the Group Map "Info Window Contents" Lava on block instances that never customized it, so the
    /// converted Obsidian block ships with its redesigned info window while custom templates survive the chop.
    /// </summary>
    public partial class GroupMapDefaultLavaConversion : Rock.Migrations.RockMigration
    {
        #region Constants

        /// <summary>
        /// The Group Map block type Guid, carried over unchanged from the WebForms block.
        /// </summary>
        private const string GroupMapBlockTypeGuid = "967F0D2B-DB76-486A-B034-D22B9D9240D3";

        /// <summary>
        /// The attribute key for the info window Lava template.
        /// </summary>
        private const string InfoWindowContentsAttributeKey = "InfoWindowContents";

        /// <summary>
        /// The new default that ships with the converted Obsidian block; must stay in sync with
        /// GroupMap.DefaultLavaTemplate in Rock.Blocks/Group/GroupMap.cs.
        /// </summary>
        private const string NewLavaTemplate = @"
<div style='width: 300px; font-size: var(--font-size-small); line-height: var(--line-height-normal); color: var(--color-interface-strongest);'>
    {% if Campus.Name and Campus.Name != '' %}
        <span class='label label-campus'>{{ Campus.Name }}</span>
    {% endif %}
    <div style='margin: var(--spacing-xsmall) 0 var(--spacing-small); font-size: var(--font-size-h5); font-weight: var(--font-weight-bold);'>{{ GroupName }}</div>
    {% if GroupType.Guid != '790E3215-3B10-442B-AF69-616C0DCB998E' %}
        <div style='display: flex; gap: var(--spacing-xsmall);'>
            <a href='{{ DetailPageUrl }}' class='btn btn-primary' style='flex: 1;'>View Group Details</a>
            <a href='{{ MapPageUrl }}' class='btn btn-default' style='flex: 1;'>View Map</a>
        </div>
    {% endif %}
    {% if Members.size > 0 %}
        <div style='margin: var(--spacing-small) 0; border-top: 1px solid var(--color-interface-soft);'></div>
        <div style='display: flex; align-items: center; gap: var(--spacing-xsmall); margin-bottom: var(--spacing-tiny); font-size: var(--font-size-xsmall); font-weight: var(--font-weight-bold); letter-spacing: .04em; text-transform: uppercase; color: var(--color-interface-medium);'>Members <span style='display: inline-flex; align-items: center; justify-content: center; min-width: 18px; height: 18px; padding: 0 var(--spacing-tiny); border-radius: 999px; background-color: var(--color-interface-soft); color: var(--color-interface-strong); font-size: var(--font-size-xsmall); font-weight: var(--font-weight-semibold);'>{{ Members.size }}</span></div>
        {% for GroupMember in Members %}
            <div style='display: flex; gap: var(--spacing-xsmall); padding: var(--spacing-xsmall) 0;{% unless forloop.first %} border-top: 1px solid var(--color-interface-softer);{% endunless %}'>
                <img src='{{ GroupMember.PhotoUrl }}&maxheight=80&maxwidth=80' alt='' style='flex: 0 0 auto; width: 40px; height: 40px; border-radius: 50%; object-fit: cover; background-color: var(--color-interface-soft);'>
                <div style='min-width: 0;'>
                    <div>
                        <a href='{{ GroupMember.ProfilePageUrl }}' style='font-weight: var(--font-weight-bold); color: var(--color-link); text-decoration: none;'>{{ GroupMember.NickName }} {{ GroupMember.LastName }}</a>
                            <span style='display: inline-flex; align-items: center; margin-left: var(--spacing-tiny); padding: 1px var(--spacing-tiny); border: 1px solid var(--color-interface-soft); border-radius: 999px; font-size: var(--font-size-xsmall); font-weight: var(--font-weight-bold); letter-spacing: .03em; text-transform: uppercase; color: var(--color-interface-medium); vertical-align: middle;'>{{ GroupMember.Role }}</span>
                    </div>
                    {% if GroupType.Guid != '790E3215-3B10-442B-AF69-616C0DCB998E' and GroupMember.ConnectionStatus and GroupMember.ConnectionStatus != '' %}
                        <div style='color: var(--color-interface-medium); margin: 2px 0 var(--spacing-tiny);'>{{ GroupMember.ConnectionStatus }}</div>
                    {% endif %}
                    {% if GroupMember.Email and GroupMember.Email != '' %}
                        <div style='display: flex; align-items: center; gap: var(--spacing-tiny); margin-top: var(--spacing-tiny);'><i class='ti ti-mail' style='width: 14px; text-align: center; color: var(--color-interface-medium);'></i>{{ GroupMember.Email }}</div>
                    {% endif %}
                    {% for Phone in GroupMember.PhoneTypes %}
                        <div style='display: flex; align-items: center; gap: var(--spacing-tiny); margin-top: var(--spacing-tiny);'><i class='ti ti-phone' style='width: 14px; text-align: center; color: var(--color-interface-medium);'></i><span>{{ Phone.Name }} {{ Phone.Number }}</span></div>
                    {% endfor %}
                </div>
            </div>
        {% endfor %}
    {% endif %}
</div>
";

        #endregion Constants

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            var escapedNewLava = NewLavaTemplate.Replace( "'", "''" );

            // Both sides of the comparison are stripped of all whitespace so indentation, line endings, and edge whitespace drift are ignored.
            var strippedStoredValue = StripWhitespaceSql( "[Value]" );
            var strippedDefaultValue = StripWhitespaceSql( "[DefaultValue]" );

            // Migrations run before [Global.asax.cs]StartBlockTypeCompilationThread() rewrites the attribute, so
            // [DefaultValue] still holds the WebForms default rather than the new Obsidian one.
            Sql( $@"
DECLARE @BlockEntityTypeId INT = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '{Rock.SystemGuid.EntityType.BLOCK}' );
DECLARE @BlockTypeId INT = ( SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '{GroupMapBlockTypeGuid}' );
IF @BlockEntityTypeId IS NOT NULL AND @BlockTypeId IS NOT NULL
BEGIN
    DECLARE @AttributeId INT = (
        SELECT TOP 1 [Id]
        FROM [Attribute]
        WHERE [Key] = '{InfoWindowContentsAttributeKey}'
            AND [EntityTypeId] = @BlockEntityTypeId
            AND [EntityTypeQualifierColumn] = 'BlockTypeId'
            AND [EntityTypeQualifierValue] = CAST( @BlockTypeId AS NVARCHAR(20) )
    );
    IF @AttributeId IS NOT NULL
    BEGIN
        UPDATE [AttributeValue]
        SET [Value] = '{escapedNewLava}'
            , [PersistedTextValue] = NULL
            , [PersistedHtmlValue] = NULL
            , [PersistedCondensedTextValue] = NULL
            , [PersistedCondensedHtmlValue] = NULL
            , [IsPersistedValueDirty] = 1
        WHERE [AttributeId] = @AttributeId
            AND {strippedStoredValue} = (
                SELECT {strippedDefaultValue}
                FROM [Attribute]
                WHERE [Id] = @AttributeId
            );
    END
END
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// Wraps a SQL expression in nested REPLACE calls that strip tab, line feed, carriage return, and space so a comparison ignores formatting differences.
        /// </summary>
        /// <param name="expression">The SQL expression (column reference or quoted literal) to strip.</param>
        /// <returns>The SQL fragment that evaluates to the whitespace-stripped value.</returns>
        private static string StripWhitespaceSql( string expression )
        {
            return $"REPLACE(REPLACE(REPLACE(REPLACE({expression}, CHAR(9), ''), CHAR(10), ''), CHAR(13), ''), ' ', '')";
        }
    }
}
