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
    /// Updates the Baptism and Group Requirements badges to use Tabler icons
    /// when their Lava has not been modified.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 343, "17.8" )]
    public class UpdateBadgeIconsToTabler : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateBaptismBadgeIconUp();
            UpdateGroupRequirementsBadgeIconUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// Changes the Baptism badge icon from fa-tint to ti-droplet.
        /// </summary>
        private void UpdateBaptismBadgeIconUp()
        {
            var originalLava = @"{% assign baptismDate = Person | Attribute:''BaptismDate'' %}
{% if baptismDate != '''' -%}
    <div class=""rockbadge rockbadge-icon rockbadge-baptism"" data-toggle=""tooltip"" data-original-title=""{{ Person.NickName }} was baptized on {{ baptismDate }}."">
<i class=""badge-icon fa fa-tint""></i>
    </div>
{% else -%}
    <div class=""rockbadge rockbadge-icon rockbadge-baptism rockbadge-disabled"" data-toggle=""tooltip"" data-original-title=""No baptism date entered for {{ Person.NickName }}."">
        <i class=""badge-icon fa fa-tint""></i>
    </div>
{% endif -%}";

            Sql( $@"
DECLARE @BadgeId INT = (SELECT TOP 1 [Id] FROM [Badge] WHERE [Guid] = '66972BFF-42CD-49AB-9A7A-E1B9DECA4EBE')
DECLARE @DisplayTextAttributeId INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '01C9BA59-D8D4-4137-90A6-B3C06C70BBC3')

UPDATE [AttributeValue]
SET [Value] = REPLACE([Value], 'fa fa-tint', 'ti ti-droplet')
    , [IsPersistedValueDirty] = 1
WHERE [AttributeId] = @DisplayTextAttributeId
    AND [EntityId] = @BadgeId
    AND {NormalizeWhitespace( "[Value]" )} = {NormalizeWhitespace( $"N'{originalLava}'" )}" );
        }

        /// <summary>
        /// Changes the Group Requirements badge icon from fa-tasks to ti-list-check
        /// and its color from --brand-success to --color-success-strong.
        /// </summary>
        private void UpdateGroupRequirementsBadgeIconUp()
        {
            var originalLava = @"{% assign groupHasRequirements = Entity.GroupType.GroupRequirements | Size | AsBoolean %}
{% assign typeHasRequirements = Entity.GroupRequirements | Size | AsBoolean %}

{% if groupHasRequirements or typeHasRequirements -%}
    <div class=""rockbadge rockbadge-icon"" data-toggle=""tooltip"" data-original-title=""Group has requirements."" style=""color:var(--brand-success);"">
        <i class=""badge-icon fa fa-tasks""></i>
    </div>
{% endif -%}";

            Sql( $@"
DECLARE @BadgeId INT = (SELECT TOP 1 [Id] FROM [Badge] WHERE [Guid] = '132F9C2A-0AF4-4AD9-87EF-7730B284E10E')
DECLARE @DisplayTextAttributeId INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '01C9BA59-D8D4-4137-90A6-B3C06C70BBC3')

UPDATE [AttributeValue]
SET [Value] = REPLACE(REPLACE([Value], 'fa fa-tasks', 'ti ti-list-check'), 'var(--brand-success)', 'var(--color-success-strong)')
    , [IsPersistedValueDirty] = 1
WHERE [AttributeId] = @DisplayTextAttributeId
    AND [EntityId] = @BadgeId
    AND {NormalizeWhitespace( "[Value]" )} = {NormalizeWhitespace( $"N'{originalLava}'" )}" );
        }

        /// <summary>
        /// Builds a SQL expression that turns line breaks and tabs into spaces,
        /// collapses runs of spaces, and trims both ends of the given SQL expression.
        /// </summary>
        /// <param name="sqlExpression">The SQL expression to normalize.</param>
        /// <returns>The normalizing SQL expression.</returns>
        private static string NormalizeWhitespace( string sqlExpression )
        {
            /*
                11/04/26 - JMH

                Comparing a normalized copy of the stored Lava against the shipped
                Lava lets an install that differs only in line breaks, tabs, or runs of spaces
                still count as unmodified, while any real edit leaves the badge alone.

                Reason: Only update badges the organization has not customized.
            */
            return $"LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE({sqlExpression}, CHAR(13), ' '), CHAR(10), ' '), CHAR(9), ' '), ' ', '*^'), '^*', ''), '*^', ' ')))";
        }
    }
}
