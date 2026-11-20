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

using System.Linq;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Encodes the person's nick name in the Baptism and eRA badge tooltips
    /// when their Lava has not been modified.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 353, "17.13" )]
    public class EncodeNameInBaptismAndEraBadges : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /*
                11/20/2026 - MSE

                The badge tooltips are shown with html enabled and sanitizing turned off,
                and the browser decodes the data-original-title attribute once before the
                tooltip inserts it as HTML. So the nick name must be encoded twice
                (Escape | Escape) to show as text, the same as the Giving badge does.
                Only the shipped Lava is changed (including the Baptism Lava with the
                Tabler icon), so customized badges are left alone and running this again
                changes nothing. Only the name tokens are replaced, so the stored
                whitespace is kept.

                Reason: Keep a person's nick name from being rendered as HTML in badge tooltips.
            */
            EncodeBaptismBadgeNameUp();
            EncodeEraBadgeNameUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// Encodes the nick name in the Baptism badge tooltips.
        /// </summary>
        private void EncodeBaptismBadgeNameUp()
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

            // The same Lava after the Tabler icon update.
            var tablerLava = originalLava.Replace( "fa fa-tint", "ti ti-droplet" );

            EncodeBadgeNickName( "66972BFF-42CD-49AB-9A7A-E1B9DECA4EBE", "{{ Person.NickName }}", originalLava, tablerLava );
        }

        /// <summary>
        /// Encodes the nick name in the eRA badge tooltips.
        /// </summary>
        private void EncodeEraBadgeNameUp()
        {
            var originalLava = @"{% assign isEra = Person | Attribute:''core_CurrentlyAnEra'' %}
{% if isEra == ''Yes'' %}

<div class=""rockbadge rockbadge-standard rockbadge-era"" data-toggle=""tooltip"" data-original-title=""{{ Person.NickName}} became an eRA on {{ Person | Attribute:''core_EraStartDate''}}"">
    <span class=""metric-value"">eRA</span>
</div>

{% else %}
    {% assign eraEndDate = Person | Attribute:''core_EraEndDate'' %}

    {% if eraEndDate != '''' %}
        {% assign todayDate = ''Now'' | Date:''M/d/yyyy'' %}
        {% assign daysSinceEnd = eraEndDate | DateDiff:todayDate,''d'' %}

        {% if daysSinceEnd <= 30 %}
            <div class=""rockbadge rockbadge-standard rockbadge-era era-loss"" data-toggle=""tooltip"" data-original-title=""{{ Person.NickName}} lost eRA status {{ daysSinceEnd }} days ago"">
                <span class=""metric-value"">eRA</span>
            </div>
        {% endif %}
    {% endif %}
{% endif %}";

            EncodeBadgeNickName( "7FC986B9-CA1E-CBB7-4E63-C79EAC34F39D", "{{ Person.NickName}}", originalLava );
        }

        /// <summary>
        /// Adds a double Escape filter to the nick name token of a Lava badge
        /// whose Display Text still matches one of the shipped versions.
        /// </summary>
        /// <param name="badgeGuid">The badge unique identifier.</param>
        /// <param name="nickNameToken">The nick name token as it appears in the shipped Lava.</param>
        /// <param name="shippedLavas">The shipped versions of the Lava, with single quotes already doubled for SQL.</param>
        private void EncodeBadgeNickName( string badgeGuid, string nickNameToken, params string[] shippedLavas )
        {
            var escapedToken = "{{ Person.NickName | Escape | Escape }}";
            var shippedLavaList = string.Join( ", ", shippedLavas.Select( lava => NormalizeWhitespace( $"N'{lava}'" ) ) );

            Sql( $@"
DECLARE @BadgeId INT = (SELECT TOP 1 [Id] FROM [Badge] WHERE [Guid] = '{badgeGuid}')
DECLARE @DisplayTextAttributeId INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '01C9BA59-D8D4-4137-90A6-B3C06C70BBC3')

UPDATE [AttributeValue]
SET [Value] = REPLACE([Value], N'{nickNameToken}', N'{escapedToken}')
    , [IsPersistedValueDirty] = 1
WHERE [AttributeId] = @DisplayTextAttributeId
    AND [EntityId] = @BadgeId
    AND {NormalizeWhitespace( "[Value]" )} IN ({shippedLavaList})" );
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
                11/20/2026 - MSE

                Comparing a normalized copy of the stored Lava against the shipped
                Lava lets an install that differs only in line breaks, tabs, or runs of spaces
                still count as unmodified, while any real edit leaves the badge alone.
                Copied from the badge icon hotfix migration, where it is private.

                Reason: Only update badges the organization has not customized.
            */
            return $"LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE({sqlExpression}, CHAR(13), ' '), CHAR(10), ' '), CHAR(9), ' '), ' ', '*^'), '^*', ''), '*^', ' ')))";
        }
    }
}
