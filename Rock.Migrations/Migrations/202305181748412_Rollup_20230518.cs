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
    ///
    /// </summary>
    public partial class Rollup_20230518 : Rock.Migrations.RockMigration
    {
        private static readonly string ContentTopicDomainTableName = "ContentTopicDomain";
        private static readonly string ContentTopicTableName = "ContentTopic";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ContentTopicDomainsAndContentTopicsUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            ContentTopicDomainsAndContentTopicsDown();
        }

        /// <summary>
        /// JMH: Adds the content topic domains and content topics.
        /// </summary>
        private void ContentTopicDomainsAndContentTopicsUp()
        {
            // Add/Update ContentTopicDomains.
            AddOrUpdateContentTopicDomain( "Personal Growth", "Discover how to grow in your faith and become the person God created you to be with these articles on personal growth.", 1, "5CADA157-EFD4-F55F-A2E4-1B864A867D56" );
            AddOrUpdateContentTopicDomain( "Relationships", "Discover biblical principles for building healthy relationships with family, friends, and romantic partners in this collection of Christian articles.", 2, "49D7311C-CE75-0B58-B600-543A00068230" );
            AddOrUpdateContentTopicDomain( "Beliefs", "Explore the foundational beliefs of Christianity, including the nature of God, salvation, and the role of Jesus Christ in this collection of articles.", 3, "7FE85941-B062-8BBB-E65D-8C01EF1F0297" );
            AddOrUpdateContentTopicDomain( "Disciplines", "Explore the importance of spiritual disciplines in the Christian life and how they can deepen your relationship with God.", 4, "0A657BCD-4BEE-54C6-E9C6-649ED9B25EF0" );
            AddOrUpdateContentTopicDomain( "Growing Faith", "Discover practical tips and biblical insights to help you cultivate a deeper and more vibrant faith in God through this collection of articles.", 5, "AA8577C8-54D0-0CAA-CA3C-4A1CE36AEF6B" );
            AddOrUpdateContentTopicDomain( "Explore Faith", "Discover the power of faith and how it can transform your life. Explore different aspects of faith and learn how to deepen your relationship with God.", 6, "A9680BC8-9DA2-F41C-908A-840439CC7418" );
            AddOrUpdateContentTopicDomain( "Barriers", "Overcoming Barriers: How to Break Down Obstacles in Your Spiritual Journey. Learn how to identify and overcome the obstacles that hinder your relationship with God.", 7, "BA343B3D-CC27-F1B8-648F-682B305E5FB7" );
            AddOrUpdateContentTopicDomain( "Culture", "Exploring the intersection of Christianity and culture, examining how faith can shape and be shaped by the world around us.", 8, "1DC8C0D7-9951-2BB8-E22E-E4AFDA02D63F" );
            AddOrUpdateContentTopicDomain( "Community", "Discover the importance of community in the Christian faith and how it can strengthen your relationship with God and others.", 9, "F45BD38C-3C9E-3272-F0E6-B18AE5CE9D88" );
            AddOrUpdateContentTopicDomain( "Seasons", "Explore the spiritual significance of the changing seasons and how they can deepen our faith and connection with God.", 10, "D1DBFCE0-42A3-BC86-BE16-5A36E0B526A3" );

            // Add/Update ContentTopics.
            AddOrUpdateContentTopic( "5CADA157-EFD4-F55F-A2E4-1B864A867D56", "Wisdom", "Articles focused on the biblical concept of wisdom, including its definition, importance, and practical application in daily life.", 1, "FC585C72-55A0-FBC1-5A82-F08F70809014" );
            AddOrUpdateContentTopic( "5CADA157-EFD4-F55F-A2E4-1B864A867D56", "Balancing Life", "Articles focused on finding balance in various aspects of life, including work, family, relationships, and spiritual growth, from a Christian perspective.", 1, "0F1C0EB7-17AF-6A46-7BB2-FCF5F22587D1" );
            AddOrUpdateContentTopic( "5CADA157-EFD4-F55F-A2E4-1B864A867D56", "Discover Your Passions", "Explore how to identify and pursue your God-given passions through biblical principles and practical steps in this collection of articles.", 1, "0E2DE91B-4EDE-08C8-154C-F437D139C6BE" );
            AddOrUpdateContentTopic( "5CADA157-EFD4-F55F-A2E4-1B864A867D56", "Choices", "Exploring the concept of choices from a Christian perspective, including decision-making, free will, and the role of God''s guidance in our lives.", 1, "4D062BDA-60B3-45AC-5DFD-8FC398E6A7FB" );
            AddOrUpdateContentTopic( "5CADA157-EFD4-F55F-A2E4-1B864A867D56", "Regret", "Articles discussing the Christian perspective on regret, including how to overcome it, find forgiveness, and move forward in faith.", 1, "B7389FA4-9EBC-9005-C1ED-1BE63F8ECA4C" );
            AddOrUpdateContentTopic( "5CADA157-EFD4-F55F-A2E4-1B864A867D56", "Character", "Articles focused on developing and strengthening Christian character, including topics such as integrity, humility, perseverance, and love.", 1, "9B0A4EBC-0631-C868-E31F-26E07891EF28" );
            AddOrUpdateContentTopic( "5CADA157-EFD4-F55F-A2E4-1B864A867D56", "Identity", "Articles exploring the Christian perspective on identity, including topics such as finding identity in Christ, overcoming identity struggles, and understanding our true identity as children of God.", 1, "1BC6B0CC-AD15-E79A-E95C-72F227CBEC75" );
            AddOrUpdateContentTopic( "5CADA157-EFD4-F55F-A2E4-1B864A867D56", "Influence", "Articles exploring the impact of Christian influence on individuals, communities, and society as a whole. Topics may include evangelism, discipleship, and cultural engagement.", 1, "1F944AB8-9ADC-10AB-A683-815E1560018B" );
            AddOrUpdateContentTopic( "5CADA157-EFD4-F55F-A2E4-1B864A867D56", "Purpose", "Articles discussing the purpose of life, the purpose of faith, and how to find and fulfill one''s God-given purpose.", 1, "A3108001-EBCF-6422-A8D2-64FFCCA33A05" );
            AddOrUpdateContentTopic( "5CADA157-EFD4-F55F-A2E4-1B864A867D56", "Leadership", "Articles on Christian leadership principles, strategies, and examples for individuals, churches, and organizations seeking to lead with integrity and biblical values.", 1, "ACD05A99-1356-498D-43FE-73961F4B06CB" );
            AddOrUpdateContentTopic( "5CADA157-EFD4-F55F-A2E4-1B864A867D56", "Setting Goals", "Learn how to set goals that align with God''s plan for your life and discover the power of prayer and faith in achieving them.", 1, "3231BB15-FCA2-A7EF-7C76-2B63654F241A" );
            AddOrUpdateContentTopic( "5CADA157-EFD4-F55F-A2E4-1B864A867D56", "Intentionality", "Articles focused on the importance of living with purpose and deliberate action, guided by faith and intentionality in all aspects of life.", 1, "765CEB8F-BA86-36D6-BDA0-629E583FB880" );
            AddOrUpdateContentTopic( "5CADA157-EFD4-F55F-A2E4-1B864A867D56", "Time Prioritization", "Articles on how to prioritize time as a Christian, including tips for balancing work, family, and spiritual life.", 1, "EE7D7C0C-9920-2B35-C469-835E756B0F47" );
            AddOrUpdateContentTopic( "5CADA157-EFD4-F55F-A2E4-1B864A867D56", "Men", "Articles focused on men''s issues, including biblical masculinity, fatherhood, leadership, and relationships from a Christian perspective.", 1, "1AD1B7FD-D268-F2A2-89BB-6DE903D9AF86" );
            AddOrUpdateContentTopic( "5CADA157-EFD4-F55F-A2E4-1B864A867D56", "Women", "Articles discussing the role of women in Christianity, including biblical teachings, women in leadership, and the importance of empowering and supporting women in the church.", 1, "A4CDF758-18C8-9B16-4541-B258B4609D29" );
            AddOrUpdateContentTopic( "49D7311C-CE75-0B58-B600-543A00068230", "Marriage", "Articles discussing the biblical principles and practical advice for building and maintaining a strong, God-centered marriage. Topics may include communication, intimacy, roles, and conflict resolution.", 1, "B5DDD89A-C3DD-79F1-5FEE-93CB400755C6" );
            AddOrUpdateContentTopic( "49D7311C-CE75-0B58-B600-543A00068230", "Dating", "Articles discussing Christian perspectives on dating, including tips for healthy relationships, navigating boundaries, and honoring God in romantic pursuits.", 1, "A1B55565-DE8B-8CF3-DC27-5C7E0EB136D7" );
            AddOrUpdateContentTopic( "49D7311C-CE75-0B58-B600-543A00068230", "Connection", "Articles exploring the importance of connection in the Christian faith, including connecting with God, building relationships with others, and finding community within the church.", 1, "8014DFDD-D1C2-CA27-103A-57DF2580B2DD" );
            AddOrUpdateContentTopic( "49D7311C-CE75-0B58-B600-543A00068230", "Parenting", "Articles on Christian parenting, including tips, advice, and biblical principles for raising children in a way that honors God and nurtures their faith.", 1, "EE25463B-6530-C63E-4439-9946F67570E1" );
            AddOrUpdateContentTopic( "49D7311C-CE75-0B58-B600-543A00068230", "Sex", "Articles discussing the Christian perspective on sex, including topics such as purity, marriage, and sexuality.", 1, "EA580B8E-BAD1-9E9B-D536-101981251754" );
            AddOrUpdateContentTopic( "49D7311C-CE75-0B58-B600-543A00068230", "Divorce", "Articles discussing the biblical perspective on divorce, its impact on families, and how to navigate the process with grace and forgiveness.", 1, "4752E057-474D-DA66-0333-1E176B0EAF03" );
            AddOrUpdateContentTopic( "49D7311C-CE75-0B58-B600-543A00068230", "Singleness", "Articles discussing the joys and challenges of being single, as well as biblical perspectives on singleness and practical advice for navigating this season of life.", 1, "ED996934-7C67-375C-806C-A082A2BDD29B" );
            AddOrUpdateContentTopic( "49D7311C-CE75-0B58-B600-543A00068230", "Friendship", "Exploring the importance of friendship in the Christian faith, including biblical examples and practical tips for cultivating meaningful relationships.", 1, "9C20A03F-25CA-06CD-E15D-C52CB283C9D7" );
            AddOrUpdateContentTopic( "49D7311C-CE75-0B58-B600-543A00068230", "Conflict", "Articles discussing how Christians can navigate and resolve conflicts in their personal relationships, church communities, and the world at large.", 1, "FFEAF219-473D-994D-E70D-894682A225C5" );
            AddOrUpdateContentTopic( "49D7311C-CE75-0B58-B600-543A00068230", "Communication", "Articles on effective communication skills for Christians, including tips for improving relationships, resolving conflicts, and sharing the gospel with others.", 1, "384A0AEA-35A4-A55C-806C-BE4B1F5395FF" );
            AddOrUpdateContentTopic( "49D7311C-CE75-0B58-B600-543A00068230", "Extended Family", "Articles about the importance of extended family relationships and how they can strengthen faith and provide support in times of need.", 1, "F25871F9-3DB2-6856-0286-E5A302BE8005" );
            AddOrUpdateContentTopic( "49D7311C-CE75-0B58-B600-543A00068230", "Trust", "Articles on building and maintaining trust in relationships, with a focus on trust in God and His plan for our lives.", 1, "D18E74DD-24E9-4461-78DB-CAFC3B310B92" );
            AddOrUpdateContentTopic( "7FE85941-B062-8BBB-E65D-8C01EF1F0297", "Jesus", "Articles about the life, teachings, and significance of Jesus Christ, the central figure of Christianity and the Son of God.", 1, "D0044053-C821-18BA-FE9B-DE2429769C29" );
            AddOrUpdateContentTopic( "7FE85941-B062-8BBB-E65D-8C01EF1F0297", "Holy Spirit", "Articles exploring the role and work of the Holy Spirit in the life of a Christian, including topics such as the gifts of the Spirit, the fruit of the Spirit, and the baptism of the Spirit.", 1, "63A265FF-49B5-4409-8B11-E7A5B7168374" );
            AddOrUpdateContentTopic( "7FE85941-B062-8BBB-E65D-8C01EF1F0297", "God", "Articles exploring the nature, character, and attributes of God, as well as His role in creation, salvation, and daily life.", 1, "8BC25728-8F67-63BE-9873-9704D010981E" );
            AddOrUpdateContentTopic( "7FE85941-B062-8BBB-E65D-8C01EF1F0297", "Bible", "Articles exploring the history, teachings, and relevance of the Bible for Christians today. Includes studies on individual books, themes, and practical applications.", 1, "5A32E69D-84CB-EC4F-79DF-40334AC0F15D" );
            AddOrUpdateContentTopic( "7FE85941-B062-8BBB-E65D-8C01EF1F0297", "Salvation", "Articles discussing the Christian doctrine of salvation, including topics such as grace, faith, repentance, and the role of Jesus Christ in the process.", 1, "7A963BCE-B3E0-D671-7959-55390B8289F4" );
            AddOrUpdateContentTopic( "7FE85941-B062-8BBB-E65D-8C01EF1F0297", "Sin", "Exploring the concept of sin from a Christian perspective, including its origins, effects, and the role of repentance and forgiveness in overcoming it.", 1, "A5386706-4D41-A8D9-33E6-9ED8A070A093" );
            AddOrUpdateContentTopic( "0A657BCD-4BEE-54C6-E9C6-649ED9B25EF0", "Bible Reading", "Articles focused on the importance, benefits, and methods of reading the Bible for personal growth, spiritual development, and understanding of God''s word.", 1, "FF1648D1-38C9-441B-F2D2-90425C7556D8" );
            AddOrUpdateContentTopic( "0A657BCD-4BEE-54C6-E9C6-649ED9B25EF0", "Prayer", "Articles on the importance, power, and practice of prayer in the Christian faith. Includes tips, personal experiences, and biblical insights.", 1, "5EC0B240-2FA9-4294-D9D6-4FDC852B3C61" );
            AddOrUpdateContentTopic( "0A657BCD-4BEE-54C6-E9C6-649ED9B25EF0", "Generosity", "Articles on the biblical concept of generosity, including practical tips for giving, stories of generosity in action, and reflections on the spiritual benefits of generosity.", 1, "AF7B2ACD-440C-CB65-02C5-328EDC87CA88" );
            AddOrUpdateContentTopic( "0A657BCD-4BEE-54C6-E9C6-649ED9B25EF0", "Church", "Articles related to the Christian church, including topics such as worship, community, leadership, and outreach.", 1, "7857168C-9338-5B3B-444A-1A574D8D0D57" );
            AddOrUpdateContentTopic( "0A657BCD-4BEE-54C6-E9C6-649ED9B25EF0", "Community", "Articles focused on building and strengthening Christian community, including topics such as fellowship, service, and discipleship within the church and beyond.", 1, "01720913-9261-45D7-CCF0-1CF97EFC5B92" );
            AddOrUpdateContentTopic( "0A657BCD-4BEE-54C6-E9C6-649ED9B25EF0", "Outreach", "Articles focused on reaching out to others with the love and message of Jesus Christ, including evangelism, missions, and community service.", 1, "F18960A0-8978-F578-E330-1B32F84B6A85" );
            AddOrUpdateContentTopic( "0A657BCD-4BEE-54C6-E9C6-649ED9B25EF0", "Serving", "Articles on the importance of serving others as a Christian, including practical tips and biblical examples of service.", 1, "A971527C-0A84-4DEF-9D46-BE5072A94AFC" );
            AddOrUpdateContentTopic( "0A657BCD-4BEE-54C6-E9C6-649ED9B25EF0", "Fasting", "Exploring the spiritual discipline of fasting, its biblical roots, and practical tips for incorporating it into your personal spiritual practice.", 1, "DF418BCF-0126-8721-C078-F8E9B196289C" );
            AddOrUpdateContentTopic( "AA8577C8-54D0-0CAA-CA3C-4A1CE36AEF6B", "Worship", "Articles on the importance and various forms of worship in the Christian faith, including music, prayer, and sacraments.", 1, "BED5AC0F-6F33-174E-ACF0-8975B9FD7484" );
            AddOrUpdateContentTopic( "AA8577C8-54D0-0CAA-CA3C-4A1CE36AEF6B", "Forgiveness", "Exploring the biblical concept of forgiveness and its transformative power in our lives, relationships, and communities.", 1, "FB9DC852-2659-8856-98BC-9F10E4E667B2" );
            AddOrUpdateContentTopic( "AA8577C8-54D0-0CAA-CA3C-4A1CE36AEF6B", "Baptism", "Articles discussing the significance and practice of baptism in the Christian faith, including its symbolism, history, and theological implications.", 1, "7F9423FD-9B05-AE5D-C0EF-42A2D05031CD" );
            AddOrUpdateContentTopic( "AA8577C8-54D0-0CAA-CA3C-4A1CE36AEF6B", "Communion", "Articles discussing the significance and practice of Communion in the Christian faith, including its history, symbolism, and theological implications.", 1, "23C929C8-EF93-2AD6-5BB6-A55B5E2785EF" );
            AddOrUpdateContentTopic( "AA8577C8-54D0-0CAA-CA3C-4A1CE36AEF6B", "Gratitude", "Articles on the importance of cultivating gratitude in the Christian life, including practical tips for developing a grateful heart and biblical examples of thankfulness.", 1, "9E6BAF28-FB9F-6C61-0739-E263691E37AB" );
            AddOrUpdateContentTopic( "AA8577C8-54D0-0CAA-CA3C-4A1CE36AEF6B", "Doubt", "Articles addressing the topic of doubt in the Christian faith, including how to overcome it, its role in spiritual growth, and biblical examples of doubt.", 1, "11A29EDA-38FF-B2F7-DEDC-D0170C1417B1" );
            AddOrUpdateContentTopic( "A9680BC8-9DA2-F41C-908A-840439CC7418", "Doubt", "Articles addressing the topic of doubt in the Christian faith, including how to overcome it, its role in spiritual growth, and biblical examples of doubt.", 1, "0A1E7A47-2EFC-E8EE-0CFD-D38DB8BB60BA" );
            AddOrUpdateContentTopic( "A9680BC8-9DA2-F41C-908A-840439CC7418", "Eternity", "Articles discussing the concept of eternity from a Christian perspective, including topics such as heaven, hell, and the afterlife.", 1, "6DF97DA9-043D-0475-F65B-2F4702BA3782" );
            AddOrUpdateContentTopic( "A9680BC8-9DA2-F41C-908A-840439CC7418", "Death", "Exploring the Christian perspective on death, including topics such as grief, afterlife, and the hope of resurrection.", 1, "D5D26D31-17B6-014E-622B-23E59A03D05D" );
            AddOrUpdateContentTopic( "A9680BC8-9DA2-F41C-908A-840439CC7418", "Heaven", "Articles discussing the Christian concept of Heaven, including its nature, purpose, and how to attain it. Topics may also include near-death experiences and biblical references.", 1, "4DB3DA2A-6F3B-680F-230D-2801DE0DB02C" );
            AddOrUpdateContentTopic( "A9680BC8-9DA2-F41C-908A-840439CC7418", "Hell", "Articles discussing the concept of Hell in Christianity, including its biblical origins, theological implications, and various interpretations throughout history.", 1, "A8FE0966-3A24-6894-A445-5E436B0668FB" );
            AddOrUpdateContentTopic( "A9680BC8-9DA2-F41C-908A-840439CC7418", "Sin", "Exploring the concept of sin from a Christian perspective, including its origins, effects, and the role of repentance and forgiveness in overcoming it.", 1, "A93EDB0E-5761-0055-6DF7-34646BE579C2" );
            AddOrUpdateContentTopic( "A9680BC8-9DA2-F41C-908A-840439CC7418", "Trinity", "Articles exploring the Christian doctrine of the Trinity, which teaches that God is one being in three persons: the Father, the Son, and the Holy Spirit.", 1, "F335D6C3-1D5A-6484-C38E-1675C7F13350" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Anxiety", "Articles discussing anxiety from a Christian perspective, including coping strategies, biblical teachings, and personal testimonies of overcoming anxiety through faith.", 1, "2FC160F0-378C-AB37-BB9C-C2421B0F7A50" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Depression", "Articles discussing depression from a Christian perspective, including causes, coping strategies, and the role of faith in mental health.", 1, "934D04F7-2110-5B8A-DB2A-550CE537E65C" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Addiction", "Articles discussing addiction from a Christian perspective, including personal testimonies, biblical guidance, and practical advice for those struggling with addiction.", 1, "1F505D80-0AC1-7B6F-10F4-E81A76411B83" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Grief", "Articles on coping with loss and finding comfort in God during times of grief and mourning. Biblical perspectives on death and the afterlife.", 1, "CD84CEA3-B403-5C4E-B02C-D63C70C6C8D5" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Anger", "Articles discussing the Christian perspective on anger management, forgiveness, and how to handle anger in a healthy and productive way.", 1, "9F9B02D0-44E9-5C6A-2248-BED3BE76B616" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Fear", "Articles discussing the role of fear in the Christian faith, how to overcome fear through faith, and how to use fear as a tool for growth.", 1, "276C58D2-3FDC-B6FD-2333-1F6271DB5BB6" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Stress", "Articles on how Christians can manage stress and find peace through prayer, faith, and biblical principles. Tips for reducing anxiety and improving mental health.", 1, "2BDC5448-0EE1-3A6D-965C-962992D43264" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Pornography", "Articles discussing the harmful effects of pornography on individuals, relationships, and society, as well as strategies for overcoming addiction and finding freedom in Christ.", 1, "A4BB5E6C-E9FC-2EB0-4319-F5B75B90D335" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Shame", "Exploring the concept of shame from a Christian perspective, including its causes, effects, and how to overcome it through the power of God''s grace.", 1, "29F7CF4C-BEF8-27CD-8B79-90E91E9F8D69" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Health", "Articles related to physical, mental, and spiritual health from a Christian perspective, including tips for healthy living and biblical principles for wellness.", 1, "C399D058-7AB5-44AE-5EC8-C2E2ABB8E8AE" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Pride", "Exploring the dangers of pride and how to cultivate humility in our daily lives as Christians. Biblical insights and practical tips for overcoming pride.", 1, "56C9AE1A-DD5B-5539-98F5-84B1128986C9" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Gambling", "Articles discussing the Christian perspective on gambling, including its potential dangers and how to approach it in a responsible and ethical manner.", 1, "A6301EBE-93E1-F3E6-5033-1B4148A8B1AD" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Unforgiving", "Articles on the topic of forgiveness and the dangers of holding onto grudges and bitterness in the Christian faith.", 1, "0940E1AA-A55A-C5C4-A717-A3E24DEC3EA2" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Judgmental", "Articles discussing the dangers of being judgmental and how to avoid it, with a focus on biblical teachings and examples.", 1, "0EF19715-F5B8-068F-4908-5B7A6B1FC456" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Inappropriate Sexual Relationship", "Articles discussing the dangers and consequences of engaging in sexual relationships outside of marriage, from a Christian perspective.", 1, "12034C9D-5E39-B147-FCB1-737E0A4A18BD" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Social Media", "Articles discussing the impact of social media on Christian living, relationships, and evangelism. Tips for using social media in a positive and effective way.", 1, "3B81031D-A5D2-3757-8CC0-9C12BDE3752D" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Hiding Emotions", "Exploring the importance of acknowledging and processing emotions as a Christian, and how hiding them can impact mental and spiritual health.", 1, "AC1CDC45-67A6-8F8E-6DA7-B504227956EE" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Emotional Abuse", "Articles discussing the effects of emotional abuse on individuals and relationships, and how to recognize and heal from this form of abuse from a Christian perspective.", 1, "4A1BCA35-4BF6-8F01-2CDA-39A73F396C96" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Physical Abuse", "Articles discussing the issue of physical abuse, its impact on individuals and families, and how to seek help and healing from a Christian perspective.", 1, "A954BA0C-B35A-C098-EF6D-03EDEB75DD59" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Shopping", "Articles related to shopping from a Christian perspective, including tips for ethical consumerism, budgeting, and avoiding materialism.", 1, "17D10478-AFC2-18BA-1EE5-7852ED3B3CC1" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Drugs", "Articles discussing the Christian perspective on drug use, addiction, and recovery. Includes personal testimonies, biblical teachings, and practical advice for those struggling with substance abuse.", 1, "B36275F7-CD01-44E0-D2C6-F8AFA1A8AFA8" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Alcohol", "Articles discussing the Christian perspective on alcohol consumption, including biblical teachings, personal testimonies, and practical advice for navigating social situations.", 1, "06ABDA89-43ED-7A1F-E38F-F2403AEF2F72" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Eating", "Articles related to the Christian perspective on food, including topics such as fasting, feasting, and the spiritual significance of eating.", 1, "941644E8-1D31-13A2-9B69-370F6E717C24" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Gambling", "Articles discussing the Christian perspective on gambling, including its potential dangers and how to approach it in a responsible and ethical manner.", 1, "3DE6DECF-D222-A207-BB6E-42FA4BB89202" );
            AddOrUpdateContentTopic( "BA343B3D-CC27-F1B8-648F-682B305E5FB7", "Finances", "Articles on managing money, budgeting, debt reduction, and biblical principles for financial stewardship. Practical advice for Christians seeking financial freedom and generosity.", 1, "36AE364E-5E0A-88E7-7C2D-328ADCC375BC" );
            AddOrUpdateContentTopic( "1DC8C0D7-9951-2BB8-E22E-E4AFDA02D63F", "Technology", "Articles discussing the impact of technology on Christian life, including tips for using technology in a positive way and avoiding its negative effects.", 1, "F2103646-94D1-3121-E496-C08FDCD9D961" );
            AddOrUpdateContentTopic( "1DC8C0D7-9951-2BB8-E22E-E4AFDA02D63F", "Politics", "Articles discussing the intersection of politics and Christianity, including topics such as voting, government policies, and social justice issues.", 1, "DCE50A50-D31D-9E25-D936-E396043EC6ED" );
            AddOrUpdateContentTopic( "1DC8C0D7-9951-2BB8-E22E-E4AFDA02D63F", "Social Justice", "Exploring the biblical call to seek justice and mercy for the oppressed and marginalized in society, and how Christians can actively engage in social justice issues.", 1, "28A86A98-E2AA-9EFB-8655-EBE1797CEF85" );
            AddOrUpdateContentTopic( "1DC8C0D7-9951-2BB8-E22E-E4AFDA02D63F", "Racial Unity", "Articles discussing the importance of racial unity in the Christian faith, and how to promote understanding and reconciliation among different races.", 1, "1217E0E2-7427-F581-37AC-707375117DAA" );
            AddOrUpdateContentTopic( "1DC8C0D7-9951-2BB8-E22E-E4AFDA02D63F", "Gender", "Exploring the biblical perspective on gender roles and identity, including discussions on masculinity, femininity, and the importance of God''s design for our lives.", 1, "2401A217-8555-8C6A-6A13-F7F686CD3741" );
            AddOrUpdateContentTopic( "F45BD38C-3C9E-3272-F0E6-B18AE5CE9D88", "Local Impact", "Articles focused on how Christians can make a positive impact in their local communities through service, evangelism, and outreach.", 1, "0BBF6E43-5EB3-0A19-F08D-5858ACD70576" );
            AddOrUpdateContentTopic( "F45BD38C-3C9E-3272-F0E6-B18AE5CE9D88", "Global Impact", "Articles discussing the impact of Christianity on a global scale, including missions, humanitarian efforts, and cultural influence.", 1, "8DA88925-07FF-2CD2-F1BF-DB221156DE32" );
            AddOrUpdateContentTopic( "F45BD38C-3C9E-3272-F0E6-B18AE5CE9D88", "Life Change Story", "Articles sharing personal stories of how individuals'' lives were transformed through their faith in Jesus Christ.", 1, "00095E1A-2E93-8348-4C04-3A7F58D6AF2C" );
            AddOrUpdateContentTopic( "D1DBFCE0-42A3-BC86-BE16-5A36E0B526A3", "Christmas", "Articles related to the celebration of Christmas, including its history, traditions, and significance in the Christian faith.", 1, "C1EB9549-ED66-2E5C-A321-8C7DE784C24A" );
            AddOrUpdateContentTopic( "D1DBFCE0-42A3-BC86-BE16-5A36E0B526A3", "Easter", "Articles related to the Christian holiday of Easter, including its history, significance, and traditions such as the Easter bunny and egg hunts.", 1, "BC8ACD22-B6D1-9BDE-2B47-84A51082B7DC" );
        }

        /// <summary>
        /// Adds or updates a content topic domain.
        /// </summary>
        /// <param name="name">The domain name.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="guid">The unique identifier.</param>
        private void AddOrUpdateContentTopicDomain( string name, string description, int order, string guid )
        {
            Sql( $@"
IF NOT EXISTS (SELECT * FROM [{ContentTopicDomainTableName}] WHERE [Guid] = '{guid}')
BEGIN
INSERT INTO [{ContentTopicDomainTableName}]
([Name], [Description], [Order], [IsSystem], [IsActive], [Guid])
VALUES
('{name}', '{description}', {order}, 1, 1,'{guid}') 
END
ELSE
BEGIN
UPDATE [{ContentTopicDomainTableName}]
SET [Name] = '{name}', [Order] = {order}, [Description] = '{description}'
WHERE [Guid] = '{guid}'
END" );
        }

        /// <summary>
        /// Adds or updates a content topic.
        /// </summary>
        /// <param name="contentTopicDomainGuid">The content topic domain unique identifier.</param>
        /// <param name="name">The topic name.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="guid">The unique identifier.</param>
        private void AddOrUpdateContentTopic( string contentTopicDomainGuid, string name, string description, int order, string guid )
        {
            Sql( $@"
DECLARE @TopicDomainId int
SET @TopicDomainId = (SELECT TOP 1 [Id] FROM [{ContentTopicDomainTableName}] WHERE [Guid] = '{contentTopicDomainGuid}')
IF NOT EXISTS (SELECT * FROM [{ContentTopicTableName}] WHERE [Guid]= '{guid}')
BEGIN
INSERT INTO [{ContentTopicTableName}]
([Name], [Description], [Order], [IsSystem], [IsActive], [Guid], [ContentTopicDomainId])
VALUES
('{name}', '{description}', {order}, 1, 1, '{guid}', @TopicDomainId) 
END
ELSE
BEGIN
UPDATE [{ContentTopicTableName}]
SET [Name] = '{name}', [Order] = {order}, [ContentTopicDomainId] = @TopicDomainId, [Description] = '{description}'
WHERE [Guid] = '{guid}'
END
" );
        }

        /// <summary>
        /// JMH: Deletes the content topics and content topic domains.
        /// </summary>
        private void ContentTopicDomainsAndContentTopicsDown()
        {
            // Delete ContentTopics.
            DeleteContentTopic( "FC585C72-55A0-FBC1-5A82-F08F70809014" );
            DeleteContentTopic( "0F1C0EB7-17AF-6A46-7BB2-FCF5F22587D1" );
            DeleteContentTopic( "0E2DE91B-4EDE-08C8-154C-F437D139C6BE" );
            DeleteContentTopic( "4D062BDA-60B3-45AC-5DFD-8FC398E6A7FB" );
            DeleteContentTopic( "B7389FA4-9EBC-9005-C1ED-1BE63F8ECA4C" );
            DeleteContentTopic( "9B0A4EBC-0631-C868-E31F-26E07891EF28" );
            DeleteContentTopic( "1BC6B0CC-AD15-E79A-E95C-72F227CBEC75" );
            DeleteContentTopic( "1F944AB8-9ADC-10AB-A683-815E1560018B" );
            DeleteContentTopic( "A3108001-EBCF-6422-A8D2-64FFCCA33A05" );
            DeleteContentTopic( "ACD05A99-1356-498D-43FE-73961F4B06CB" );
            DeleteContentTopic( "3231BB15-FCA2-A7EF-7C76-2B63654F241A" );
            DeleteContentTopic( "765CEB8F-BA86-36D6-BDA0-629E583FB880" );
            DeleteContentTopic( "EE7D7C0C-9920-2B35-C469-835E756B0F47" );
            DeleteContentTopic( "1AD1B7FD-D268-F2A2-89BB-6DE903D9AF86" );
            DeleteContentTopic( "A4CDF758-18C8-9B16-4541-B258B4609D29" );
            DeleteContentTopic( "B5DDD89A-C3DD-79F1-5FEE-93CB400755C6" );
            DeleteContentTopic( "A1B55565-DE8B-8CF3-DC27-5C7E0EB136D7" );
            DeleteContentTopic( "8014DFDD-D1C2-CA27-103A-57DF2580B2DD" );
            DeleteContentTopic( "EE25463B-6530-C63E-4439-9946F67570E1" );
            DeleteContentTopic( "EA580B8E-BAD1-9E9B-D536-101981251754" );
            DeleteContentTopic( "4752E057-474D-DA66-0333-1E176B0EAF03" );
            DeleteContentTopic( "ED996934-7C67-375C-806C-A082A2BDD29B" );
            DeleteContentTopic( "9C20A03F-25CA-06CD-E15D-C52CB283C9D7" );
            DeleteContentTopic( "FFEAF219-473D-994D-E70D-894682A225C5" );
            DeleteContentTopic( "384A0AEA-35A4-A55C-806C-BE4B1F5395FF" );
            DeleteContentTopic( "F25871F9-3DB2-6856-0286-E5A302BE8005" );
            DeleteContentTopic( "D18E74DD-24E9-4461-78DB-CAFC3B310B92" );
            DeleteContentTopic( "D0044053-C821-18BA-FE9B-DE2429769C29" );
            DeleteContentTopic( "63A265FF-49B5-4409-8B11-E7A5B7168374" );
            DeleteContentTopic( "8BC25728-8F67-63BE-9873-9704D010981E" );
            DeleteContentTopic( "5A32E69D-84CB-EC4F-79DF-40334AC0F15D" );
            DeleteContentTopic( "7A963BCE-B3E0-D671-7959-55390B8289F4" );
            DeleteContentTopic( "A5386706-4D41-A8D9-33E6-9ED8A070A093" );
            DeleteContentTopic( "FF1648D1-38C9-441B-F2D2-90425C7556D8" );
            DeleteContentTopic( "5EC0B240-2FA9-4294-D9D6-4FDC852B3C61" );
            DeleteContentTopic( "AF7B2ACD-440C-CB65-02C5-328EDC87CA88" );
            DeleteContentTopic( "7857168C-9338-5B3B-444A-1A574D8D0D57" );
            DeleteContentTopic( "01720913-9261-45D7-CCF0-1CF97EFC5B92" );
            DeleteContentTopic( "F18960A0-8978-F578-E330-1B32F84B6A85" );
            DeleteContentTopic( "A971527C-0A84-4DEF-9D46-BE5072A94AFC" );
            DeleteContentTopic( "DF418BCF-0126-8721-C078-F8E9B196289C" );
            DeleteContentTopic( "BED5AC0F-6F33-174E-ACF0-8975B9FD7484" );
            DeleteContentTopic( "FB9DC852-2659-8856-98BC-9F10E4E667B2" );
            DeleteContentTopic( "7F9423FD-9B05-AE5D-C0EF-42A2D05031CD" );
            DeleteContentTopic( "23C929C8-EF93-2AD6-5BB6-A55B5E2785EF" );
            DeleteContentTopic( "9E6BAF28-FB9F-6C61-0739-E263691E37AB" );
            DeleteContentTopic( "11A29EDA-38FF-B2F7-DEDC-D0170C1417B1" );
            DeleteContentTopic( "0A1E7A47-2EFC-E8EE-0CFD-D38DB8BB60BA" );
            DeleteContentTopic( "6DF97DA9-043D-0475-F65B-2F4702BA3782" );
            DeleteContentTopic( "D5D26D31-17B6-014E-622B-23E59A03D05D" );
            DeleteContentTopic( "4DB3DA2A-6F3B-680F-230D-2801DE0DB02C" );
            DeleteContentTopic( "A8FE0966-3A24-6894-A445-5E436B0668FB" );
            DeleteContentTopic( "A93EDB0E-5761-0055-6DF7-34646BE579C2" );
            DeleteContentTopic( "F335D6C3-1D5A-6484-C38E-1675C7F13350" );
            DeleteContentTopic( "2FC160F0-378C-AB37-BB9C-C2421B0F7A50" );
            DeleteContentTopic( "934D04F7-2110-5B8A-DB2A-550CE537E65C" );
            DeleteContentTopic( "1F505D80-0AC1-7B6F-10F4-E81A76411B83" );
            DeleteContentTopic( "CD84CEA3-B403-5C4E-B02C-D63C70C6C8D5" );
            DeleteContentTopic( "9F9B02D0-44E9-5C6A-2248-BED3BE76B616" );
            DeleteContentTopic( "276C58D2-3FDC-B6FD-2333-1F6271DB5BB6" );
            DeleteContentTopic( "2BDC5448-0EE1-3A6D-965C-962992D43264" );
            DeleteContentTopic( "A4BB5E6C-E9FC-2EB0-4319-F5B75B90D335" );
            DeleteContentTopic( "29F7CF4C-BEF8-27CD-8B79-90E91E9F8D69" );
            DeleteContentTopic( "C399D058-7AB5-44AE-5EC8-C2E2ABB8E8AE" );
            DeleteContentTopic( "56C9AE1A-DD5B-5539-98F5-84B1128986C9" );
            DeleteContentTopic( "A6301EBE-93E1-F3E6-5033-1B4148A8B1AD" );
            DeleteContentTopic( "0940E1AA-A55A-C5C4-A717-A3E24DEC3EA2" );
            DeleteContentTopic( "0EF19715-F5B8-068F-4908-5B7A6B1FC456" );
            DeleteContentTopic( "12034C9D-5E39-B147-FCB1-737E0A4A18BD" );
            DeleteContentTopic( "3B81031D-A5D2-3757-8CC0-9C12BDE3752D" );
            DeleteContentTopic( "AC1CDC45-67A6-8F8E-6DA7-B504227956EE" );
            DeleteContentTopic( "4A1BCA35-4BF6-8F01-2CDA-39A73F396C96" );
            DeleteContentTopic( "A954BA0C-B35A-C098-EF6D-03EDEB75DD59" );
            DeleteContentTopic( "17D10478-AFC2-18BA-1EE5-7852ED3B3CC1" );
            DeleteContentTopic( "B36275F7-CD01-44E0-D2C6-F8AFA1A8AFA8" );
            DeleteContentTopic( "06ABDA89-43ED-7A1F-E38F-F2403AEF2F72" );
            DeleteContentTopic( "941644E8-1D31-13A2-9B69-370F6E717C24" );
            DeleteContentTopic( "3DE6DECF-D222-A207-BB6E-42FA4BB89202" );
            DeleteContentTopic( "36AE364E-5E0A-88E7-7C2D-328ADCC375BC" );
            DeleteContentTopic( "F2103646-94D1-3121-E496-C08FDCD9D961" );
            DeleteContentTopic( "DCE50A50-D31D-9E25-D936-E396043EC6ED" );
            DeleteContentTopic( "28A86A98-E2AA-9EFB-8655-EBE1797CEF85" );
            DeleteContentTopic( "1217E0E2-7427-F581-37AC-707375117DAA" );
            DeleteContentTopic( "2401A217-8555-8C6A-6A13-F7F686CD3741" );
            DeleteContentTopic( "0BBF6E43-5EB3-0A19-F08D-5858ACD70576" );
            DeleteContentTopic( "8DA88925-07FF-2CD2-F1BF-DB221156DE32" );
            DeleteContentTopic( "00095E1A-2E93-8348-4C04-3A7F58D6AF2C" );
            DeleteContentTopic( "C1EB9549-ED66-2E5C-A321-8C7DE784C24A" );
            DeleteContentTopic( "BC8ACD22-B6D1-9BDE-2B47-84A51082B7DC" );

            // Delete ContentTopicDomains.
            DeleteContentTopicDomain( "5CADA157-EFD4-F55F-A2E4-1B864A867D56" );
            DeleteContentTopicDomain( "49D7311C-CE75-0B58-B600-543A00068230" );
            DeleteContentTopicDomain( "7FE85941-B062-8BBB-E65D-8C01EF1F0297" );
            DeleteContentTopicDomain( "0A657BCD-4BEE-54C6-E9C6-649ED9B25EF0" );
            DeleteContentTopicDomain( "AA8577C8-54D0-0CAA-CA3C-4A1CE36AEF6B" );
            DeleteContentTopicDomain( "A9680BC8-9DA2-F41C-908A-840439CC7418" );
            DeleteContentTopicDomain( "BA343B3D-CC27-F1B8-648F-682B305E5FB7" );
            DeleteContentTopicDomain( "1DC8C0D7-9951-2BB8-E22E-E4AFDA02D63F" );
            DeleteContentTopicDomain( "F45BD38C-3C9E-3272-F0E6-B18AE5CE9D88" );
            DeleteContentTopicDomain( "D1DBFCE0-42A3-BC86-BE16-5A36E0B526A3" );
        }

        /// <summary>
        /// Deletes a content topic domain by unique identifier.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        private void DeleteContentTopicDomain( string guid )
        {
            RockMigrationHelper.DeleteByGuid( guid, ContentTopicDomainTableName );
        }

        /// <summary>
        /// Deletes a content topic by unique identifier.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        private void DeleteContentTopic( string guid )
        {
            RockMigrationHelper.DeleteByGuid( guid, ContentTopicTableName );
        }
    }
}
