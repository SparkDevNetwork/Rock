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
    using System.Linq;

    /// <summary>
    ///
    /// </summary>
    public partial class FixPersonicx : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    UPDATE [MetaPersonicxLifestageGroup] SET
        [Summary] = 'Although not yet past their mid-30s, members of Taking Hold have made it into middle and upper-middle income brackets and are building their net worth. They are a diverse group. Some have a college education, many with high-school degrees, and they work in a variety of occupations. Most are single, but some are married, and about a quarter of their households have children. Some have purchased their first homes, though most are renters.',
        [Description] = 'The consumers in Taking Hold – in their 20’s and early 30’s – include many single professionals purchasing homes, buying home furnishings and getting ready to marry and start a family. Estimated income varies significantly in this group, but well over a third (44%) earn $150,000+ per year, with a job change not too likely in the year ahead. At this point, a compact car or two-door sports coupe is usually big enough to meet their needs. Early adopters, they are comfortable exploring online and mobile resources, and enjoy the conveniences of mobile Internet. They often download and listen to/watch music and movies, watch live television, read digital magazines and newspapers, and play games. Their smartphones are typically loaded with apps for entertainment, sports, banking, books, social networking and more.' 
    WHERE [LifestyleGroupCode] = '02Y';

    UPDATE [MetaPersonicxLifestageGroup]
    SET [Summary] = 'After spending their 20s working in entry-level jobs and focusing on the social aspects of their young lives, this group is transitioning to a more settled lifestyle, with most getting married and buying homes and a few starting families. Members have a mix of white- and blue-collar jobs, with low-middle to mid-level household incomes and modest net worth.'
    WHERE [LifestyleGroupCode]='03X';

    UPDATE [MetaPersonicxLifestageGroup]
    SET [Summary] = 'Concentrated in more urban areas, most members of this group have very low incomes and very little net worth. Usually unmarried and renting, very few have children. With lower-echelon white- and blue-collar jobs, they want to do well but struggle to stay in control.'
    WHERE [LifestyleGroupCode]='06X';

    UPDATE [MetaPersonicxLifestageGroup]
    SET [Summary] = 'Well off enough to consider retirement, most members of this group continue to work, often in white-collar management or technical roles. With high levels of education and an interest in continuous learning, this group has their lives under control. They are enjoying the fruits of their labor in the company of their spouses or significant others.'
    WHERE [LifestyleGroupCode]='19M';

    UPDATE [MetaPersonicxLifestageGroup]
    SET [Summary] = 'With an average age of 78, many members of this group are limited in their retirement by fixed incomes and modest net worth. More than half are single or widowed. A mix of renters and homeowners, their homes are central to their lives, with a longer-than-average length of residence. Not generally interested in going online or using a cellphone, Leisure Seekers fill their substantial free time with home-based activities such as gardening, doing crosswords, and watching daytime television.'
    WHERE [LifestyleGroupCode]='21S';
" );
            Sql( "DELETE [MetaPersonicxLifestageCluster]" );
            ExecuteSqlInsert(
                "INSERT INTO [MetaPersonicxLifestageCluster] ([LifestyleClusterCode],[LifestyleClusterName],[Description],[Summary],[PercentUS],[LifeStage],[MaritalStatus],[HomeOwnership],[Children],[Income],[IncomeRank],[Urbanicity], [UrbanicityRank], [NetWorth], [NetworthRank],[MetaPersonicxLifestyleGroupId],[DetailsUrl],[Guid])",
                MigrationSQL._201709231340508_PersonicxCluster );

            // DH: Add Fundraising Button Text Attribute
            RockMigrationHelper.AddDefinedTypeAttribute( "53C8FFF6-3022-4A2D-9BAE-FD3435BEA43D",
                SystemGuid.FieldType.TEXT, "Donate Button Text", "core_DonateButtonText",
                "If set this text will be used on the Donate button when viewing a fundraising opportunity. If not set then the default 'Donate to a Participant' will be used instead.",
                0, string.Empty, SystemGuid.Attribute.DEFINED_VALUE_FUNDRAISING_DONATE_BUTTON_TEXT );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        private void ExecuteSqlInsert( string insertIntoStatement, string columnValues )
        {
            // Convert line-feed delimited values into string list
            var valueLines = columnValues.Split( new[] { "\n" }, StringSplitOptions.None ).ToList();

            // Build and execute an insert statement that inserts a group of values.
            int pos = 0;
            int take = 500;
            var subset = valueLines.Skip( pos ).Take( take ).ToList();

            while ( subset.Any() )
            {
                string values = subset.Select( v => $"({v},NEWID())" ).ToList().AsDelimited( "," );
                string sqlStatement = $"{insertIntoStatement} VALUES {values}";

                Sql( sqlStatement );

                pos += take;
                subset = valueLines.Skip( pos ).Take( take ).ToList();
            }
        }
    }
}
