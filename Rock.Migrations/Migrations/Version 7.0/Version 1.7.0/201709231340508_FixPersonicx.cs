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
            Sql( @" 
     UPDATE [MetaPersonicxLifestageCluster]
     SET [Summary]='Established Elite represents America’s elite couples and some singles. With no school-age children at home and some of the highest income in the country, these suburban households have substantial net worth and disposable incomes and can afford to pursue high-end luxuries and activities.'
     WHERE [LifestyleClusterCode]='02';

     UPDATE [MetaPersonicxLifestageCluster]
     SET  [Description]  = 'Corporate Connected households are exceedingly well educated (almost a third have completed graduate school) and established in their executive, technical and professional careers, with high incomes and net worth. Whether married or single, nearly all have no children under the age of 18 living with them. This group seems to be firmly tied to corporate American culture. They often hold business-related credit cards, are heavy users of air travel and pay close attention to business and financial news. They are savvy investors, frequently dine out and also love to entertain and attend sporting events. Professional responsibilities compete for time with travel and fitness-related leisure pursuits. They C2 active with sports such as tennis, biking and skiing, and many support environmental groups and causes.'
     WHERE [LifestyleClusterCode]='03';

    UPDATE [MetaPersonicxLifestageCluster]
    SET  [Description]  = 'Top Professionals are established, wealthy families with children in the home, concentrated in the more affluent suburban areas of the US. Over half of this group (55%) have completed college or graduate school. Top Professionals includes married executives and professionals who earn top-dollar incomes and indulge in a wide (and expensive) array of activities from skiing to golf. Reflecting their devotion to their children, they head to the beach or zoo and enjoy family-friendly sports. They also tend to jog, hike and bicycle, paying attention to their health and fitness. Members of this cluster often drive a new luxury minivan or SUV. They are frequent shoppers both online and offline, buying clothes for the family and furnishings for their houses. Many use the internet to read newspapers and magazines, plan travel, and manage their investments. Starbucks is a popular stop for many in this cluster; they also dine out frequently and entertain at home.',
    [Summary] = 'This entire cluster ranks high for both income and net worth, with predominantly professional, technical and management jobs. At an average age of 50, Top Professionals are homeowners with mixed-age children in the household, and three-quarters are married.'
    WHERE [LifestyleClusterCode]='04';

    UPDATE [MetaPersonicxLifestageCluster]
    SET  [Description]  = 'With top incomes (ranked #1 for average income) and climbing net worth (ranked #11), paired with a mean age of just 34, Casual Comfort members are well positioned for the future. While their jobs are likely to require long hours and supervisory responsibilities, many still find time for a variety of leisure activities. They enjoy sporting events and concerts, watching sports channels and HGTV, participating in fantasy sports, and adventurous outdoor recreation such as hiking and backpacking. They shop online and will likely pay for quality. This group also appreciates fine dining, and many make sure to go to the fitness club or day spa to keep in shape. More than half are married; none have children in the home. Many have moved into a single-family home within the last two years, adding new home mortgages and decorating expenses to their existing student and car loans.',
    [Summary] = 'Casual Comfort is made up of individuals and childless couples in their late 20s to mid 40s with high-income professional/management careers. Most own their homes. Almost half of this ethnically diverse group have a college or graduate degree.'
    WHERE [LifestyleClusterCode]='06';

    UPDATE [MetaPersonicxLifestageCluster]
    SET  [Description]  = 'Active Lifestyles includes wealthy mostly married couples in their mid-30s to early-60s with children of all ages. For those with older children, some of them are driving now, too, which means additional vehicles at home. These parents are planning for the future, investing for college expenses and taking out insurance. Before the children head off to college, though, they enjoy the here and now with casual, family-friendly activities such as cooking out, going to family restaurants, boating, watching movies and playing board games. Enjoying the third highest average income, these parents put in long hours at work, but stay active with deliberate exercise and high-energy recreation that might include bicycling, canoeing or swimming.',
    [Summary] = 'Active Lifestyles contains established couples and some singles living in more suburban areas with school-aged children and teens, multiple vehicles and mortgages. With high education levels, many are employed in professional/technical occupations, managing five or more employees and earning top-ranking incomes.'
    WHERE [LifestyleClusterCode]='07';

    UPDATE [MetaPersonicxLifestageCluster]
    SET [Summary] = 'Solid Surroundings is a mix of affluent, well-educated couples and singles between the ages of 36 and 65 who have substantial net worth and no children at home. More than 40% are employed in technical/professional occupations and earn upper-middle incomes.'
    WHERE [LifestyleClusterCode]='08';

    UPDATE [MetaPersonicxLifestageCluster]
    SET  [Description]  = 'These higher-income households are transitioning to life with babies and toddlers and preschool children. 80% are married, and 100% have children – all under the age of five. The joys of home ownership and parenthood combine to ensure that money earned is quickly spent. When not watching children’s/family movies, heading out for a picnic, or playing board games or cards, they are often busy shopping online for quality clothes, accessories and toys. Even with time at a premium, many make exercise such as running or jogging a priority. TV viewing tends to weigh toward children-oriented programs, but they also follow seasonal sports (NFL, MLB). They tend to make frequent trips to family and fast-food restaurants.'
    WHERE [LifestyleClusterCode]='12';

    UPDATE [MetaPersonicxLifestageCluster]
    SET  [Description]  = 'Work & Play households are all unmarried parents with children under 18 in the home. With an average income just above $75,000, they rank above average for income and also net worth, ranked 15th of all the clusters. Many in this group are devoted to sports that they can either pursue for their own fitness or enjoy with their children, such as playing baseball or soccer. Work & Play shopping, media and travel all reflect the mix of ages of children in the household, which can range from toddlers to teenagers. These parents like gadgets and buy electronics such as game systems, TVs and cell phones for themselves and their children. They tend to travel very little but enjoy going out to family restaurants and seeing two or three movies a month.',
    [Summary] = 'Work & Play is comprised of single parents with an average age of 46 who are raising children of various ages on upper-middle and affluent incomes. They are predominantly white-collar workers or professionals, and most are homeowners living in smaller cities and surrounding areas.'
    WHERE [LifestyleClusterCode]='13';

    UPDATE [MetaPersonicxLifestageCluster]
    SET  [Description]  = 'Career Centered contains established members of their communities, situated in cities and the surrounding areas. At a mean of 55.5, slightly above the national average, they tend to be employed in white-collar, professional and technical jobs. With their upper incomes, single status and investment activities, they are not afraid to spend money on nice clothes, entertainment, home remodeling and art. Their primary interests, outside of work and other career-oriented activities, include live music (and karaoke), fitness classes at the club, and going dancing. Sports are also an interest; they keep up with sports headlines and regularly attend sporting events, like NFL games.'
    WHERE [LifestyleClusterCode]='14';

    UPDATE [MetaPersonicxLifestageCluster]
    SET  [Description]  = 'Children First represents a busy group of young (6th youngest cluster), financially comfortable singles and couples, all of whom already have at least one child. This group of high school graduates, some still working on higher-level degrees, enjoys upper-middle wages and white-collar professional sales and technical jobs, as well as better paid blue-collar jobs. They spend time with the family fishing, swimming, going to the circus and watching pay-per-view movies. While child-rearing and parenting-focused publications and activities overshadow the adults'' own hobbies, parents still make some time for interests such as video games and electronic gadgets.'
    WHERE [LifestyleClusterCode]='21';

    UPDATE [MetaPersonicxLifestageCluster]
    SET  [Description]  = 'At a mean age of 60, Good Neighbors are the bedrock of their established neighborhoods, with almost half (49%) having lived in their homes for 15 years or longer. Nearing retirement, with upper-middle incomes and child-free homes, these married couples have time to spend on interests like gardening, collecting and home improvement. Other preferred activities include birdwatching, swimming, biking, reading and playing Sudoku and word games. They watch a variety of TV – news, sports, shopping channels, cable shows. They are heavy newspaper readers but light magazine readers. They are often found to be more conservative with their investments at this stage of their lives, with many making CDs and annuities a part of their portfolio.'
    WHERE [LifestyleClusterCode]='23';

    UPDATE [MetaPersonicxLifestageCluster]
    SET
    [Summary] = 'Career Building is made up of mostly single adults with no children. Ranging in age from 18 to 45, they are a mix of mobile renters and a few first-time homeowners. Their jobs are mostly white collar, and a high percentage (24%) have a post-graduate degree'
    WHERE [LifestyleClusterCode]='24';

    UPDATE [MetaPersonicxLifestageCluster]
    SET [Description] = 'Clubs & Causes households are maturing yet active members of the upper-middle class. At a mean age of almost 70, only 21% are retired or housewives; the remainder are still working in upper-middle income jobs. These mostly married couples (72%) own their homes, and although no children are present, a third have more than two-person household. Their investments favor stocks, bonds and money market accounts, and they make political contributions. Financially secure, many find time to participate in religious, fraternal, and veterans’ clubs. Preferring traditional channels, interest in technology, including mobile phones, is generally low. Many own RVs, and they also enjoy eating out, domestic travel, and cruises.'
    WHERE [LifestyleClusterCode]='25';

    UPDATE [MetaPersonicxLifestageCluster]
    SET [Description] = 'The third oldest of all the clusters, Community Pillars households are well-established members of their city and suburban communities. Nearing half (44%) of these consumers have lived in their homes for 15+ years. Some households now include a caregiver. This group is solidly in the upper-middle class, with above-average incomes and net worth, and steady investments. Many use a financial advisor and make contributions to social services. They generally like to eat out at family restaurants and can be found running errands at the hardware and grocery store, often make several trips each week. They own higher-end – but still practical – domestic cars that they buy new and keep for years. At home, they read books and newspapers and many like to watch PGA golf, news on CNN or Fox, and games shows such as Jeopardy.'
    WHERE [LifestyleClusterCode]='28';

    UPDATE [MetaPersonicxLifestageCluster]
    SET [Description] = 'Mid-Americana is made up of suburban, middle-income couples in with an average age of 56. Although they are three-times more likely to have a vocational/technical degree, this group is mostly high school educated (66%), with another 33% having completed college or graduate school. Squarely in the middle in terms of income (ranked 35th), this group enjoys above-average net worth (ranked 23rd) and home market value (ranked 25th). They often have modest investments and carry major and store credit cards. Many are conscientious shoppers who do not indulge in the latest fashion or trend. Mid-Americana households can be found growing houseplants, flowers and taking on DIY home projects. They also enjoy watching seasonal sports on TV, reading the newspaper, going out for breakfast or lunch, and participating in veterans clubs.'
    WHERE [LifestyleClusterCode]='31';

    UPDATE [MetaPersonicxLifestageCluster]
    SET [Description] = 'At an average age of 36, these married couples are primarily high school graduates (80%) working in a mix of technical, professional, clerical and blue-collar craftsman jobs. Perhaps working and going to school at the same time, twice as many as the national average are still in school. While less than 20% have children, they are likely to have multiple dogs and/or cats. These middle-income households spend time and money on outdoor living – hunting, fishing, horseback riding, boating, camping, and hiking. Truck and RV ownership are common. Inside, Outward Bound couples often like to play chess, board games and online games, watch TV (using a satellite dish), read outdoor-focused magazines, bake and cook. For entertainment, they like to go out on the town. Their financial lives include mortgages, auto loans and credit cards.'
    WHERE [LifestyleClusterCode]='34';

    UPDATE [MetaPersonicxLifestageCluster]
    SET [Description] = 'The fourth youngest cluster, Setting Goals ranks among the lowest clusters for income (62nd) and net worth (57th), but their average age is only 26, and some are working to further their education. Almost 90% are high school educated, but this cluster is nearly 10 times more likely to have students than the national average. Though 69% of this cluster is single, nearly all households contain at least two adults, and all have children in the home. With the majority as renters (52%), and given their young age, the length of residence for this group is short. The cluster is more ethnically diverse than the national average, with a higher percentage of Hispanic and African-American households. With limited financial resources as well as parenting responsibilities, leisure time is focused on less expensive entertainment such as renting movies, playing video games, home cooking and exercising.'
    WHERE [LifestyleClusterCode]='39';

    UPDATE [MetaPersonicxLifestageCluster]
    SET [Description] = 'Preferring the more rural areas of the country – three quarters of Rural Adventure members live in areas with fewer than 104 households per square mile – these singles (62%) and couples (38%) enjoy a more carefree life, most without children in the home. At a mean age of 36, members of this cluster are more than four times as likely to be working on degrees, perhaps to bolster their middle incomes (ranked 39th). While almost two-thirds own their homes, they are more likely than average to be renters. Seemingly not as outdoorsy as some other rural clusters, they like fishing, archery and NASCAR. They like to get a lot of exercise themselves but often are not self-described fans of professional or college sports. They read magazines of all kinds, and many like gadgets, technology and trying new things. While they may not like planning dinner, they seldom eat out, perhaps limited by options in their more rural locale.'
    WHERE [LifestyleClusterCode]='41';

    UPDATE [MetaPersonicxLifestageCluster]
    SET [Summary] = 'The households of Open Houses are community-minded, lower-to-middle income singles. They are in their mid 50s to mid 60s, some are retired, but most are still working, mainly in lower-level clerical white-collar and blue-collar jobs. All are homeowners, usually with a length of residence of six or more years.'
    WHERE [LifestyleClusterCode]='44';

    UPDATE [MetaPersonicxLifestageCluster]
    SET [Description] = 'Farm & Home is comprised mainly of Caucasian, high school-educated, rural, blue-collar families. Living in the smallest industrial second cities and mill towns of America, they are mostly in their mid-30s to mid-60s (with a mean age of 48). They are parents, and some are grandparents, with mixed-age children at home. Their blue-collar salaries are earned by both men and women and support mortgages and personal loans. They tend to be risk averse and not overly interested in investing. Most are very family oriented, and the family is likely to include more than one dog and/or cat. Activities, shopping and media habits focus on outdoor interests that include hunting, fishing, gardening and motorcycling. Aside from NASACAR, they generally are not fans of professional (or college) sports. Some own farms, and many rely on trucks, and RVs for practicality and fun'
    WHERE [LifestyleClusterCode]='48';

    UPDATE [MetaPersonicxLifestageCluster]
    SET [Summary] = 'Metro Strivers are handling single parenthood (with children who are mostly over the age of six) on low-middle income and a small budget. Ranging in age from 30 to 65, they are primarily high-school-educated homeowners who are well entrenched in their communities'
    WHERE [LifestyleClusterCode] = '53'

    UPDATE [MetaPersonicxLifestageCluster]
    SET [Description] = 'The most rural of all the clusters, Work & Outdoors households are made up of 46-65 year old singles working in blue-collar and lower echelon white-collar jobs. Despite being single with virtually no children under 18 at home, more than 40% of these households have two or more people living in them, suggesting shared living expenses for some. Many are comfortable performing some home and vehicle maintenance, shopping offline at local hardware and automotive supply stores, and watching DIY shows on TV. Woodworking is a related hobby. With room to run, several have multiple cats and/or dogs. They enjoy watching NASCAR, bull-riding, and rodeo. Heavy TV watchers, they tend not to be professional sports fans but like to watch movies at home. They also like to get outdoors for riding, hunting and fishing. The Internet and new technology are of less interest to this group.'
    WHERE [LifestyleClusterCode] = '54'

    UPDATE [MetaPersonicxLifestageCluster]
    SET [Description] = 'With an average age of 32, Mobile Mixers are almost all single, renters, and have no children living with them. Close to 90% have a high school education. The cluster has a high percentage of African Americans (29%) and a higher than average percentage of Hispanic ethnicity. They are likely to be working in sales/service or clerical jobs, but they want to get ahead, and 9% are still going to school. They enjoy playing video games, going to the movies, socializing with friends and volunteering. Other activities include going online, reading science fiction and watching weekly and syndicated comedy shows on television. Not food adventurers, they might microwave a frozen dinner, but they also view fast food as junk food.'
    WHERE [LifestyleClusterCode] = '59'

    UPDATE [MetaPersonicxLifestageCluster]
    SET [Description] = 'With an average age of 32, Mobile Mixers are almost all single, renters, and have no children living with them. Close to 90% have a high school education. The cluster has a high percentage of African Americans (29%) and a higher than average percentage of Hispanic ethnicity. They are likely to be working in sales/service or clerical jobs, but they want to get ahead, and 9% are still going to school. They enjoy playing video games, going to the movies, socializing with friends and volunteering. Other activities include going online, reading science fiction and watching weekly and syndicated comedy shows on television. Not food adventurers, they might microwave a frozen dinner, but they also view fast food as junk food.'
    WHERE [LifestyleClusterCode] = '59'

    UPDATE [MetaPersonicxLifestageCluster]
    SET [Description] = 'The second most urban of all the clusters, City Life is an ethnically mixed group with a particularly high concentration of Asians, Hispanics and African-Americans. They are a younger group of urbanites (with an average age of 30) either in school or working in entry-level white-collar jobs and beginning to make their way in the big city. With youth and tight finances, they tend to have little disposable income, not yet focused on investing in the future. Predominantly single and childless, they spend a lot of their free time following sports, using the internet and mobile apps, and watching movies, particularly crime, thriller, and action films',
    [Summary] = 'City Life is a combination of young professionals and students living in the nation’s most densely populated and expensive cities. Most live alone in multi-unit dwellings. While many are starting in white-collar professional careers, others of this highly mobile group are still finishing their degrees.'
    WHERE [LifestyleClusterCode] = '61'

    UPDATE [MetaPersonicxLifestageCluster]
    SET [Description] = 'Practical & Careful represents one of the older (fourth oldest) and more economically modest clusters. At a mean age of 79, these predominantly single men and women have modest incomes, educational attainment and net worth. They are mostly renters (79%), living in both single- and multiple-family dwellings. This group is predominantly Caucasian with 15% housewives, and (48%) retired. Given their financial situation, investing is generally not something they can focus on. Their interests and activities include indoor gardening and plants, crossword puzzles, game shows, reading and needlework. Shopping is mostly done offline. Many are not radio listeners but watch a great deal of TV'
    WHERE [LifestyleClusterCode] = '64'

    UPDATE [MetaPersonicxLifestageCluster]
    SET [Description] = 'First Steps includes a very large percentage of students – more than 13% compared to a national average of less than 1%. At a mean age of 26, those who are not students work in mostly entry-level professional or technical jobs with a few working blue-collar jobs. They have minimal household income and net worth as of yet. The cluster is ethnically diverse, with Hispanics and African Americans representing more than 45% of the population (more than twice the national average). They enjoy collecting art for their new residences and sticking close to home for entertainment, including playing video, electronic and card games and watching movies online. Cooking is often not a great interest. They enjoy engaging in sports and watching sports on TV although they are light TV watchers overall. Not generally newspaper readers, they are nonetheless interested in the ups and downs of the market and learning about financial topics.'
    WHERE [LifestyleClusterCode] = '67'" );

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
