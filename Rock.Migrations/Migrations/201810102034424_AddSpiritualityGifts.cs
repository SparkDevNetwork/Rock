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
    public partial class AddSpiritualityGifts : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddSpiritualGiftDefinedType();
            RockMigrationHelper.UpdatePersonAttributeCategory( "TrueWiring", "fas fa-file-user", string.Empty, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969" );
            AddDominantGiftPersonAttribute();
            AddSupportiveGiftPersonAttribute();
            AddOtherGiftPersonAttribute();
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DATE_TIME, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Spiritual Gifts Last Save Date", "core_SpiritualGiftsLastSaveDate", "", "", 18, "", SystemGuid.Attribute.PERSON_SPIRITUAL_GIFTS_LAST_SAVE_DATE );
            AddSecurityToAttributes( Rock.SystemGuid.Attribute.PERSON_SPIRITUAL_GIFTS_LAST_SAVE_DATE );
            AddExtendedAttributesInProfilePage();
            AddPageWithRoutsForGiftAssessment();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute("44272FB2-27DC-452D-8BBB-2F76266FA92E");
            RockMigrationHelper.DeleteAttribute("861F4601-82B7-46E3-967F-2E03D769E2D2");
            RockMigrationHelper.DeleteAttribute("86C9E794-B678-4453-A831-FE348A440646");
            RockMigrationHelper.DeleteAttribute("9DC69746-7AD4-4BC9-B6EE-27E24774CE5B");
            RockMigrationHelper.DeleteAttribute("85107259-0A30-4F1A-A651-CBED5243B922");
            RockMigrationHelper.DeleteAttribute("DA7752F5-9F21-4391-97F3-BB7D35F885CE");
            RockMigrationHelper.DeleteAttribute("85256610-56EB-4E6F-B62B-A5517B54B39E");
            RockMigrationHelper.DeleteBlock("B76F0F54-E03A-4835-A69D-B6D6F6499D4A");
            RockMigrationHelper.DeleteBlockType( "A7E86792-F0ED-46F2-988D-25EBFCD1DC96" );
            RockMigrationHelper.DeletePage( "06410598-3DA4-4710-A047-A518157753AB" );

            RockMigrationHelper.DeleteSecurityAuthForBlock( "0C244AA1-2473-4749-8D7E-81CAA415C886" );
            RockMigrationHelper.DeleteBlockAttribute( "0C244AA1-2473-4749-8D7E-81CAA415C886" );
            RockMigrationHelper.DeleteBlock( "0C244AA1-2473-4749-8D7E-81CAA415C886" );

            RockMigrationHelper.DeleteSecurityAuthForAttribute( SystemGuid.Attribute.PERSON_DOMINANT_GIFTS );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.PERSON_DOMINANT_GIFTS );

            RockMigrationHelper.DeleteSecurityAuthForAttribute( SystemGuid.Attribute.PERSON_SUPPORTIVE_GIFTS );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.PERSON_SUPPORTIVE_GIFTS );

            RockMigrationHelper.DeleteSecurityAuthForAttribute( SystemGuid.Attribute.PERSON_OTHER_GIFTS );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.PERSON_OTHER_GIFTS );

            DeleteSpiritualGiftDefinedType();
        }

        /// <summary>
        /// Add spiritual gifts defined type and values
        /// </summary>
        private void AddSpiritualGiftDefinedType()
        {
            RockMigrationHelper.AddDefinedType( "Global", "Spiritual Gifts", "Used by the TrueWiring spiritual gifts assessment in Rock.", Rock.SystemGuid.DefinedType.SPIRITUAL_GIFTS, @"" );
            RockMigrationHelper.AddDefinedTypeAttribute( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Scripture", "Scripture", "", 1017, "", "C1DBD74B-5B31-4E5D-B800-FF0B4C0DC583" );
            RockMigrationHelper.AddDefinedTypeAttribute( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", "Details", "Details", "", 1018, "", "EADE1EC5-E05B-4956-973E-BBBA2A02F0C0" );
            RockMigrationHelper.AddAttributeQualifier( "C1DBD74B-5B31-4E5D-B800-FF0B4C0DC583", "ispassword", "False", "D68A35B3-8957-45B8-B150-696C0DCB34B6" );
            RockMigrationHelper.AddAttributeQualifier( "C1DBD74B-5B31-4E5D-B800-FF0B4C0DC583", "maxcharacters", "", "817CA542-2B6B-477C-A27C-06B2AB1A0C11" );
            RockMigrationHelper.AddAttributeQualifier( "C1DBD74B-5B31-4E5D-B800-FF0B4C0DC583", "showcountdown", "False", "26FD8970-F79C-49C8-9E3F-7117CE65FCEA" );
            RockMigrationHelper.AddAttributeQualifier( "EADE1EC5-E05B-4956-973E-BBBA2A02F0C0", "documentfolderroot", "", "8AECB3ED-F5C3-4F6F-B9D0-887239DE9D77" );
            RockMigrationHelper.AddAttributeQualifier( "EADE1EC5-E05B-4956-973E-BBBA2A02F0C0", "imagefolderroot", "", "9CD944A3-3FE8-4F59-81BE-A5ADF9FEF45A" );
            RockMigrationHelper.AddAttributeQualifier( "EADE1EC5-E05B-4956-973E-BBBA2A02F0C0", "toolbar", "Light", "576AE059-3D93-446B-BE63-79E20C426D1F" );
            RockMigrationHelper.AddAttributeQualifier( "EADE1EC5-E05B-4956-973E-BBBA2A02F0C0", "userspecificroot", "False", "4BBE2B34-5BBC-4D21-B653-5EA185EDD82D" );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Administration", "This  gift  is  the  ability  to  develop,  articulate,  and  execute  effective  and  specific  plans  in  the  accomplishment  of  the  long  and  short  range  goals  of  the  Body  of  Christ.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_ADMINISTRATION, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Apostleship", "This gift is the ability to perceive the motives of others and the sincerity within relationships concerning the Body of Christ. Or in other words, it is the ability to know if something is from God, a demonic source, or if it simply reflects human opinion.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_APOSTLESHIP, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Discernment", "This gift is the ability to perceive the motives of others and the sincerity within relationships concerning the Body of Christ. Or in other words, it is the ability to know if something is from God, a demonic source, or if it simply reflects human opinion.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_DISCERNMENT, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Encouragement", "This gift is the ability to counsel, model, and encourage people through your personal testimony, life, and scripture so that people are comforted and encouraged to action.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_ENCOURAGEMENT, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Evangelism", "This gift is the ability to share the gospel of Christ publicly or privately with unbelievers in such a way that men and women respond to become disciples of Jesus Christ.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_EVANGELISM, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Faith", "This gift is the ability to envision with clarity and confidence God’s future direction and goals for the Body of Christ.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_FAITH, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Giving", "This gift is the ability to give liberally to meet the needs of others and support God’s ministry with that which God has entrusted to you.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_GIVING, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Helps/Service", "This gift is the ability to unselfishly meet the needs of other people through practical service. It may be either ministering to the person himself or doing things to enable him to be free to do other ministries.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_HELPS, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Hospitality", "This gift is the ability to reach out to others and welcome them into your home and life in a loving, warm manner, while providing food and lodging, such that the guests feel ''at home'' in your presence.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_HOSPITALITY, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Knowledge", "This gift is the ability to discover, analyze, accumulate, systematize, and articulate ideas that are essential for growth and edification of the Body of Christ. This especially involves the ability to concretely expound upon Old and New Testament history as well as the history of the Church.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_KNOWLEDGE, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Leadership", "This gift is the ability to set Godly goals, make decisions, and then communicate them to the Body of Christ, such that others will voluntarily follow and joyously work to accomplish these goals.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_LEADERSHIP, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Mercy", "This gift is the ability to empathize with those in need (especially those suffering and miserable) and to manifest this empathy in such a way as to encourage those that are in need.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_MERCY, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Pastor-Shepherd", "This gift is the ability to minister to an established group of people (believers) by caring for their spiritual welfare holistically on a long term basis.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_PASTOR_SHEPHERD, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Pastor-Teacher", "This gift is the ability to instruct, guide, and care for a local expression of the Body of Christ, such that the members are prepared to reach out in ministry to others, both inside and outside the Body.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_PASTOR_TEACHER, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Prophecy", "This gift is the ability to publicly or privately proclaim God’s word such that people are convicted, consoled, encouraged, challenged or strengthened. Another angle also involves knowing and speaking God’s mind intuitively rather than, as with the teacher, deductively from scripture alone. Scripture is to always check this intuition.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_PROPHECY, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Teaching", "This gift is the ability to communicate information in such a way that members of the Body of Christ understand how to apply spiritual principles to their own lives and ministries.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_TEACHING, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Wisdom", "This gift is the ability to gain insight on how knowledge may best be applied to specific needs in the Body of Christ. This involves applying Biblical truths to everyday situations. It also involves special insight into the profundity and implications of how God is working in the world, especially to further the gospel. Compare Romans 11:33-35 with 1 Corinthians 2:6-10.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_WISDOM, false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0894EDBA-8FC8-4433-877C-53351A06A8B7", "C1DBD74B-5B31-4E5D-B800-FF0B4C0DC583", @"Romans 12:8" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0894EDBA-8FC8-4433-877C-53351A06A8B7", "EADE1EC5-E05B-4956-973E-BBBA2A02F0C0", @"<p>&lt;p&gt;The gift is to be used with cheerfulness; that is, joyousness which is ready to do anything: &lt;br /&gt;Romans 12:8. We see this gift in Dorcas as well as Helps/Services: Acts 9:36. Mercy will be &lt;br /&gt;manifested in good deeds: James 2:15,16. All believers are commanded to have mercy: &lt;br /&gt;Matthew 9:13; Luke 10:37.&lt;br /&gt;Probable characteristics (often latent):&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;You find it easy to identify with the feelings of someone in pain.&lt;/li&gt; </p><p>&lt;li&gt;You cry easily or show emotions when you see or hear things that sadden you.&lt;/li&gt; </p><p>&lt;li&gt;When someone is sharing something deep with you, you find yourself crying when they do.&lt;/li&gt; </p><p>&lt;li&gt;Your immediate reactions when you see someone in difficulty is to want to reach out and help.&lt;/li&gt; </p><p>&lt;li&gt;You sometimes become bitter because of the injustices someone else suffers.&lt;/li&gt; </p><p>&lt;li&gt;Most people would see you as an empathetic person.&lt;/li&gt; </p><p>&lt;li&gt;People come to you with problems not because of some practical things you do, but because you have the ability to understand what they are going through and can console them.&lt;/li&gt; </p><p>&lt;li&gt;You have the ability to &amp;ldquo;cheer up&amp;rdquo; people who are going through deep personal struggles.&lt;/li&gt; </p><p>&lt;li&gt;Your prayer life includes the needs of the many personal struggles you know people are experiencing.&lt;/li&gt; </p><p>&lt;li&gt;When you are finished having someone share with you, many times you find yourself emotionally drained.&lt;/li&gt; </p><p>&lt;li&gt;You enjoy calling on people in the hospital or who are sick or going through some difficulty.&lt;/li&gt; </p><p>&lt;/ul&gt;</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0F8D41AA-7236-40BF-AA37-980BCCF4A881", "C1DBD74B-5B31-4E5D-B800-FF0B4C0DC583", @"Ephesians 4:11" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0F8D41AA-7236-40BF-AA37-980BCCF4A881", "EADE1EC5-E05B-4956-973E-BBBA2A02F0C0", @"<p>&lt;p&gt;All Christians are called to evangelize (Matthew 28:19,20; Acts 5:42; 2 Timothy 4:5). It is an &lt;br /&gt;office of the New Testament church (Acts 21:8). This gift not only evangelizes but also equips &lt;br /&gt;others to do it as well (Ephesians 4:11). There are two aspects of this gift:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;Public: </p><p>&lt;ul&gt; </p><p>&lt;li&gt;Philip (Acts 8:5-13)&lt;/li&gt; </p><p>&lt;li&gt;Paul (Acts 17:19-34)&lt;/li&gt; </p><p>&lt;/ul&gt; </p><p>&lt;/li&gt; </p><p>&lt;li&gt;Private: </p><p>&lt;ul&gt; </p><p>&lt;li&gt;Philip (Acts 8:26-39)&lt;/li&gt; </p><p>&lt;li&gt;Paul (Acts 16:25-33)&lt;/li&gt; </p><p>&lt;/ul&gt; </p><p>&lt;/li&gt; </p><p>&lt;/ul&gt; </p><p>&lt;p&gt;It provides growth for existing churches and helps establish newly-planted churches (2 Timothy &lt;br /&gt;4:5; Acts 17:10-15). It normally does not include cross-cultural evangelism. &lt;br /&gt;Biblical examples:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;Philip: Acts 8:5-13; 26-39&lt;/li&gt; </p><p>&lt;li&gt;Paul: Acts 16: 25-33; 17:19-34&lt;/li&gt; </p><p>&lt;/ul&gt; </p><p>&lt;p&gt;Probable characteristics of this gift:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;Your prayer life reveals a burden for those who do not know Christ.&lt;/li&gt; </p><p>&lt;li&gt;You enjoy meeting people and make new friends easily.&lt;/li&gt; </p><p>&lt;li&gt;You feel comfortable with strangers and talk about many different subjects.&lt;/li&gt; </p><p>&lt;li&gt;You are able to articulate your faith easily and people respond positively.&lt;/li&gt; </p><p>&lt;li&gt;You enjoy being around people.&lt;/li&gt; </p><p>&lt;li&gt;You have a burden to share the gospel of Jesus Christ publicly or with individuals.&lt;/li&gt; </p><p>&lt;li&gt;You are able to persuade or help people reach decisions to commit their lives to Christ.&lt;/li&gt; </p><p>&lt;li&gt;You are not satisfied with &amp;ldquo;superficial commitments;&amp;rdquo; you want people to become full &lt;br /&gt;disciples of Christ.&lt;/li&gt; </p><p>&lt;li&gt;You have an inner unrest at the thought of people dying and going to hell.&lt;/li&gt; </p><p>&lt;li&gt;You have the ability to move conversations in a direction of the other person&amp;rsquo;s relationship with Christ.&lt;/li&gt; </p><p>&lt;li&gt;When with non-Christians, their position with Christ inevitably comes up.&lt;/li&gt; </p><p>&lt;li&gt;Your prayers contain the names of many people who are not Christians or those you are training to share Christ.&lt;/li&gt; </p><p>&lt;li&gt;People enjoy being trained by you to share their faith.&lt;/li&gt; </p><p>&lt;li&gt;You have the ability to motivate and train others to share their faith effectively&lt;/li&gt; </p><p>&lt;/ul&gt;</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "13C40209-F41D-4C1D-83D3-2EC530588245", "C1DBD74B-5B31-4E5D-B800-FF0B4C0DC583", @"1 Corinthians 12:28, Romans 12:7" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "13C40209-F41D-4C1D-83D3-2EC530588245", "EADE1EC5-E05B-4956-973E-BBBA2A02F0C0", @"<p>&lt;p&gt;This is the behind the scenes worker. These people enjoy such tasks as:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;Washing dishes&lt;/li&gt; </p><p>&lt;li&gt;Cooking meals&lt;/li&gt; </p><p>&lt;li&gt;Setting up meeting areas&lt;/li&gt; </p><p>&lt;li&gt;Operating sound equipment&lt;/li&gt; </p><p>&lt;/ul&gt; </p><p>&lt;p&gt;This is one of the more important gifts and often the least practiced. Without it, all other gifts &lt;br /&gt;must perform functions out of their own gift mix, inhibiting their effectiveness. Therefore, the &lt;br /&gt;people with the gift of helps/service free others up to use their own gifts. We all should &lt;br /&gt;exemplify this gift as characterized by Christ. But a person who truly has this gift personifies it &lt;br /&gt;to the extreme and is recharged by exercising it.&lt;br /&gt;Biblical examples:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;Acts 6: -- Men relieved Apostles of work&lt;/li&gt; </p><p>&lt;li&gt;Acts 13:5 -- John Mark helped Paul and Barnabas&lt;/li&gt; </p><p>&lt;li&gt;2 Timothy 4:11&lt;/li&gt; </p><p>&lt;li&gt;1 Peter 5:13&lt;/li&gt; </p><p>&lt;li&gt;Acts 19:22 -- Paul had two other helpers&lt;/li&gt; </p><p>&lt;li&gt;Romans 16:1-4 -- Priscilla and Aquilla&lt;/li&gt; </p><p>&lt;li&gt;Colossians 4:12, 13 -- Epaphras&lt;/li&gt; </p><p>&lt;/ul&gt; </p><p>&lt;p&gt;Probable characteristics of this gift:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;You have an uncanny ability to see needs.&lt;/li&gt; </p><p>&lt;li&gt;You work by yourself or in small groups.&lt;/li&gt; </p><p>&lt;li&gt;You have a desire to help others.&lt;/li&gt; </p><p>&lt;li&gt;You do not need to receive public recognition for what you do.&lt;/li&gt; </p><p>&lt;li&gt;You enjoy doing &amp;ldquo;little things&amp;rdquo; that can help other people.&lt;/li&gt; </p><p>&lt;li&gt;You do not enjoy handling major leadership or overseeing responsibilities.&lt;/li&gt; </p><p>&lt;li&gt;You enjoy doing things for someone else that enables them to use their gifts more fully.&lt;/li&gt; </p><p>&lt;li&gt;You do not have to ask what needs to be done; you can see needs.&lt;/li&gt; </p><p>&lt;li&gt;You have the ability to sense the temporal needs of others.&lt;/li&gt; </p><p>&lt;li&gt;In the church, you have a willingness to do jobs which will allow leadership gifts to be used in a fruitful way.&lt;/li&gt; </p><p>&lt;li&gt;Sometimes you feel guilty or envious because you do not have one of the more &amp;ldquo;obvious&amp;rdquo; gifts that others seem to possess.&lt;/li&gt; </p><p>&lt;/ul&gt;</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3EB352F3-F624-4ED6-A9EE-7951B71B1952", "C1DBD74B-5B31-4E5D-B800-FF0B4C0DC583", @"1 Corinthians 12:10" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3EB352F3-F624-4ED6-A9EE-7951B71B1952", "EADE1EC5-E05B-4956-973E-BBBA2A02F0C0", @"<p>&lt;p&gt;All Christians should be able to distinguish between right and wrong (Hebrews 5:14; 1 John 4:1). &lt;br /&gt;This gift involves insight into circumstances which ordinarily could not be known, so that &lt;br /&gt;motives may be ascertained (Acts 5:1-10). At times, this gift will reveal a person&amp;rsquo;s sincerity in &lt;br /&gt;the course of a conversation (Acts 8:23). This gift is easily counterfeited by Satan (Luke 4:33, 34; &lt;br /&gt;Acts 16:17). Biblical examples include Matthew 9:4.&lt;br /&gt;Probable characteristics of this gift:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;You are very observant to what people say.&lt;/li&gt; </p><p>&lt;li&gt;You tend to notice &amp;ldquo;body language&amp;rdquo; that is not consistent with what the person is &lt;br /&gt;saying.&lt;/li&gt; </p><p>&lt;li&gt;You almost have a sixth sense about what is being said or is happening which cannot &lt;br /&gt;always be explained.&lt;/li&gt; </p><p>&lt;li&gt;Your intuitions are often correct.&lt;/li&gt; </p><p>&lt;li&gt;You hesitate to speak to others of their motives because they may not be receptive.&lt;/li&gt; </p><p>&lt;li&gt;You sometimes gossip about a &amp;ldquo;feeling&amp;rdquo; you have with regard to a person rather than &lt;br /&gt;confront the person involved.&lt;/li&gt; </p><p>&lt;li&gt;You do not view yourself as a &amp;ldquo;witch hunter&amp;rdquo;.&lt;/li&gt; </p><p>&lt;li&gt;Certain events often confirm a premonition you have had.&lt;/li&gt; </p><p>&lt;li&gt;You have an analytical mind.&lt;/li&gt; </p><p>&lt;li&gt;People see you as a very perceptive person.&lt;/li&gt; </p><p>&lt;li&gt;Your prayers often consist of asking God what you should do with your intuitions.&lt;/li&gt; </p><p>&lt;/ul&gt;</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "462A5D10-6DEA-43D7-96EF-8F82FF1E2E14", "C1DBD74B-5B31-4E5D-B800-FF0B4C0DC583", @"1 Corinthians 12:8" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "462A5D10-6DEA-43D7-96EF-8F82FF1E2E14", "EADE1EC5-E05B-4956-973E-BBBA2A02F0C0", @"<p>&lt;p&gt;This gift is intended to be communicated. This is why it is called the &amp;ldquo;message of knowledge&amp;rdquo; &lt;br /&gt;(1 Corinthians 12:8). This gift sometimes &amp;ldquo;puffs up&amp;rdquo; if kept to yourself (1 Corinthians 8:1). All &lt;br /&gt;believers should exemplify some knowledge (Colossians 1:9-12). Sometimes this gift requires a &lt;br /&gt;person to set aside human knowledge (1 Corinthians 1:19- 20; 2:14-16). Biblical examples &lt;br /&gt;include much of Jesus&amp;rsquo; teachings. &lt;br /&gt;Probable characteristics of this gift:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;Your prayers typically involve deep theological truths and often seek God&amp;rsquo;s aid in understanding more.&lt;/li&gt; </p><p>&lt;li&gt;You are able to grasp spiritual truths quickly.&lt;/li&gt; </p><p>&lt;li&gt;You enjoy reading the Bible and studying it in depth.&lt;/li&gt; </p><p>&lt;li&gt;You enjoy reading books and articles.&lt;/li&gt; </p><p>&lt;li&gt;You are eager to learn and have a long attention span.&lt;/li&gt; </p><p>&lt;li&gt;You can absorb and remember large quantities of information.&lt;/li&gt; </p><p>&lt;li&gt;You tend to neglect other types of Christian activities and family responsibilities because of your desire to learn and study.&lt;/li&gt; </p><p>&lt;li&gt;When listening to a message, you listen for deep truths which are being communicated.&lt;/li&gt; </p><p>&lt;li&gt;When you are involved in a discussion about spiritual issues, you find yourself with your &lt;br /&gt;Bible in hand.&lt;/li&gt; </p><p>&lt;li&gt;You are more comfortable with ideas than people.&lt;/li&gt; </p><p>&lt;li&gt;You like to take time by yourself to think.&lt;/li&gt; </p><p>&lt;li&gt;When faced with a decision, you ask yourself what Biblical principles are involved.&lt;/li&gt; </p><p>&lt;li&gt;You are greatly disturbed by people thinking &amp;ldquo;the end justifies the means&amp;rdquo;.&lt;/li&gt; </p><p>&lt;li&gt;People see you as more ideological than practical.&lt;/li&gt; </p><p>&lt;li&gt;You are aware of times when people say things that contradict spiritual principles.&lt;/li&gt; </p><p>&lt;li&gt;You continually try to reconcile different truths from scripture so you can understand how they relate.&lt;/li&gt; </p><p>&lt;/ul&gt;</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4ADAEED1-D0E6-4DA4-A0BA-8E7D058075C4", "C1DBD74B-5B31-4E5D-B800-FF0B4C0DC583", @"Romans 2:6; 1 Corinthians 12:10, 28-29; Ephesians 4:11" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4ADAEED1-D0E6-4DA4-A0BA-8E7D058075C4", "EADE1EC5-E05B-4956-973E-BBBA2A02F0C0", @"<p>&lt;p&gt;The primary function of this gift in the Old and New Testament is not to foretell the future but &lt;br /&gt;to tell forth the Word of God.&lt;br /&gt;&amp;ldquo;Less than two percent of Old Testament prophecy is Messianic. Less than five &lt;br /&gt;percent specifically describe the New Covenant Age (from New Testament on). &lt;br /&gt;Less than one percent concerns events yet to come.&amp;rdquo;&lt;br /&gt;Page 150, Fee &amp;amp; Stuart&amp;rsquo;s How to Read the Bible for All Its Worth.&lt;br /&gt;As a result of the use of this gift, people are often strengthened and encouraged (Acts 15:32).&lt;br /&gt;They are able to speak accurately with regard to the future (Acts 1:27-28); although they are &lt;br /&gt;not always correct (Acts 21:10). What they say should be weighed by other prophets and the &lt;br /&gt;Word of God (1 Corinthians 14:29-32). The New Testament church had an office for this gift. &lt;br /&gt;Biblical examples:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;Agabus: Acts 11:27-28; 21:10-11&lt;/li&gt; </p><p>&lt;li&gt;Judas and Silas: Acts 15:32&lt;/li&gt; </p><p>&lt;/ul&gt; </p><p>&lt;p&gt;Probable characteristics of this gift:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;Your prayers are often for courage to speak out against injustices and to help others understand God&amp;rsquo;s Word.&lt;/li&gt; </p><p>&lt;li&gt;You have a good understanding of many social and political trends of our time.&lt;/li&gt; </p><p>&lt;li&gt;You have an excellent grasp of God&amp;rsquo;s Word; frequently quoting the Bible to others.&lt;/li&gt; </p><p>&lt;li&gt;You do not mind someone challenging you.&lt;/li&gt; </p><p>&lt;li&gt;You are able to stand alone against the tide.&lt;/li&gt; </p><p>&lt;li&gt;You have courage to confront issues with insights God has given you.&lt;/li&gt; </p><p>&lt;li&gt;Sometimes people see you as a negative person.&lt;/li&gt; </p><p>&lt;li&gt;You have the ability to see through surface issues to the true spiritual problem.&lt;/li&gt; </p><p>&lt;li&gt;People have different responses to your messages; some are convicted, others are encouraged, but few are neutral.&lt;/li&gt; </p><p>&lt;li&gt;Your &amp;ldquo;&amp;hellip; words carry God&amp;rsquo;s authority and have the power to build by stimulating and encouraging. Often this gift reflects special insight into the truth and calls men back to the obedience of faith.&amp;rdquo; (A. W. Tozer)&lt;/li&gt; </p><p>&lt;li&gt;You see events which will occur if people do not act now on God&amp;rsquo;s Word.&lt;/li&gt; </p><p>&lt;/ul&gt;</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5F1F5A92-D981-4027-A4BC-C3642E784D0B", "C1DBD74B-5B31-4E5D-B800-FF0B4C0DC583", @"1 Corinthians 12:8" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5F1F5A92-D981-4027-A4BC-C3642E784D0B", "EADE1EC5-E05B-4956-973E-BBBA2A02F0C0", @"<p>&lt;p&gt;This wisdom is intended to be communicated. This is why it is called the &amp;ldquo;message of wisdom&amp;rdquo; &lt;br /&gt;(1 Corinthians 12:8). All believers should exemplify some wisdom. (Colossians 1: 9-12)&lt;br /&gt;This wisdom involves revelation from God. It may be direct or unwritten revelation &lt;br /&gt;(1 Corinthians 2:6-16).&lt;br /&gt;Use of this gift will be accompanied by: (James 3:13-18)&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;Consideration&lt;/li&gt; </p><p>&lt;li&gt;Fullness of mercy&lt;/li&gt; </p><p>&lt;li&gt;Fullness of the Fruit of the Spirit&lt;/li&gt; </p><p>&lt;li&gt;Impartiality&lt;/li&gt; </p><p>&lt;li&gt;Peace-lovingness&lt;/li&gt; </p><p>&lt;li&gt;Purity&lt;/li&gt; </p><p>&lt;li&gt;Sincerity&lt;/li&gt; </p><p>&lt;li&gt;Submission&lt;/li&gt; </p><p>&lt;/ul&gt; </p><p>&lt;p&gt;This gift can be used to: Answer an unbeliever&amp;rsquo;s arguments. (1 Peter 3:15), solve problems &lt;br /&gt;facing the Body of Christ, apply to believer&amp;rsquo;s conduct (see Paul&amp;rsquo;s epistles)&lt;br /&gt;Probable characteristics of this gift:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;You find yourself praying for insight on how to apply truths to needs around you.&lt;/li&gt; </p><p>&lt;li&gt;You enjoy the application passages of scripture rather than the doctrinal.&lt;/li&gt; </p><p>&lt;li&gt;Sometimes you see solutions to problems that no one else seems to see.&lt;/li&gt; </p><p>&lt;li&gt;You become frustrated when people do not listen to your solution because you know it will work.&lt;/li&gt; </p><p>&lt;li&gt;You have little difficulty communicating your thoughts and usually others listen to you.&lt;/li&gt; </p><p>&lt;li&gt;You enjoy working on solutions to problems more than you do applying all the details of the solutions to the problems.&lt;/li&gt; </p><p>&lt;li&gt;Many times in prayer, meditation, or scripture study, solutions will come to mind.&lt;/li&gt; </p><p>&lt;li&gt;People come to you with their personal problems for insight. They see you as a problem solver.&lt;/li&gt; </p><p>&lt;li&gt;You feel uncomfortable with discussions which remain at the theoretical level for too long a period of time.&lt;/li&gt; </p><p>&lt;li&gt;You have an analytical mind which is always trying to figure out how and why.&lt;/li&gt; </p><p>&lt;li&gt;You usually get to the heart of an issue quickly.&lt;/li&gt; </p><p>&lt;li&gt;You find it easy to decide on a course of action which needs to be taken.&lt;/li&gt; </p><p>&lt;li&gt;You observe many problems which have solutions.&lt;/li&gt; </p><p>&lt;li&gt;When you listen to a message, you are disappointed if it does not include some specific applications.&lt;/li&gt; </p><p>&lt;/ul&gt;</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7B30E2BA-9461-4688-9B43-D2B774E33A18", "C1DBD74B-5B31-4E5D-B800-FF0B4C0DC583", @"1 Corinthians 12:9" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7B30E2BA-9461-4688-9B43-D2B774E33A18", "EADE1EC5-E05B-4956-973E-BBBA2A02F0C0", @"<p>&lt;p&gt;Everyone should manifest faith in their daily living (Colossians 2:6; 2 Corinthians 5:7). This gift &lt;br /&gt;involves seeing both short and long term goals which will enable the Body of Christ to &lt;br /&gt;accomplish its divine purpose (Philippians 3:12-17).&lt;/p&gt; </p><p>&lt;p&gt;The visionary nature of this gift requires a significant number of supporting gifts (helps/service, &lt;br /&gt;administration, giving, encouragement, etc.) to actually accomplish the envisioned goals. &lt;br /&gt;(2 Timothy 4:9-13, 21-22)&lt;/p&gt; </p><p>&lt;p&gt;&lt;br /&gt;The few with this gift face the danger of expecting others to see with similar clarity and &lt;br /&gt;confidence what God wants the Body to accomplish and having little empathy when they don&amp;rsquo;t &lt;br /&gt;(Galatians 3:1; Acts 15:38). Biblical examples include Acts 27: 13-44 -- Paul and the shipwreck. &lt;br /&gt;This also is one of the primary gifts needed by the leadership in large growing churches. &lt;br /&gt;Probable characteristics of this gift:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;Your prayers include many future goals and projects for the work of God.&lt;/li&gt; </p><p>&lt;li&gt;You find yourself much more interested in the future than the past.&lt;/li&gt; </p><p>&lt;li&gt;You are a positive thinker and are frustrated by negative people.&lt;/li&gt; </p><p>&lt;li&gt;You are goal-oriented and enjoy challenges involving new horizons.&lt;/li&gt; </p><p>&lt;li&gt;You are not daunted by obstacles or problems.&lt;/li&gt; </p><p>&lt;li&gt;You are non-traditional in your approach and find significant difficulty in working within a &amp;ldquo;traditional system&amp;rdquo;.&lt;/li&gt; </p><p>&lt;li&gt;You find yourself dreaming of what God can do in the future but lack the desire to attend to all the details to bring His plan into existence.&lt;/li&gt; </p><p>&lt;li&gt;You become irritated by those who lack your vision and criticize you.&lt;/li&gt; </p><p>&lt;li&gt;You are seen as a courageous person because of your confidence that God is with you.&lt;/li&gt; </p><p>&lt;li&gt;&amp;nbsp;Sometimes people see you as too self-assured.&lt;/li&gt; </p><p>&lt;li&gt;You find others often are &amp;ldquo;infected&amp;rdquo; with your vision if they spend time with you.&lt;/li&gt; </p><p>&lt;li&gt;You take risks and often are willing to pursue a vision at great personal sacrifice.&lt;/li&gt; </p><p>&lt;li&gt;You are not afraid to stand alone for God&amp;rsquo;s vision.&lt;/li&gt; </p><p>&lt;li&gt;You sometimes expect too much of others and can&amp;rsquo;t understand why they just don&amp;rsquo;t &amp;ldquo;live by faith&amp;rdquo; more.&lt;/li&gt; </p><p>&lt;li&gt;Trusting God comes easily for you.&lt;/li&gt; </p><p>&lt;/ul&gt;</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "809F65A6-1759-472A-8B8B-F37009F476BF", "C1DBD74B-5B31-4E5D-B800-FF0B4C0DC583", @"Romans 12:8" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "809F65A6-1759-472A-8B8B-F37009F476BF", "EADE1EC5-E05B-4956-973E-BBBA2A02F0C0", @"<p>&lt;p&gt;If you have the gift of encouragement, you:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;Admonish people to pursue some specific course in the future (1 Thessalonians 4:1).&lt;/li&gt; </p><p>&lt;li&gt;Comfort someone who is experiencing some trial or difficulty, often in areas of your life &lt;br /&gt;where you have been through a lot and have already been helped by God.&lt;/li&gt; </p><p>&lt;li&gt;Are able to encourage someone for something they are going to face in the future.&lt;/li&gt; </p><p>&lt;li&gt;The gift of encouragement is the major way through which God works in the Body to encourage each of us to live practical Christian lives.&lt;/li&gt; </p><p>&lt;/ul&gt; </p><p>&lt;p&gt;Biblical example: Barnabas - &amp;ldquo;Son of Encouragement:&amp;rdquo;&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;Acts 4:36, 37 &amp;ndash; Helped needy saints&lt;/li&gt; </p><p>&lt;li&gt;Acts 9:27 &amp;ndash; Endorsed an unwelcome convert&lt;/li&gt; </p><p>&lt;li&gt;Acts 11:19-24 &amp;ndash; Accepted alien behavior&lt;/li&gt; </p><p>&lt;li&gt;Acts 11:25, 26 &amp;ndash; Enlisted a promising teacher&lt;/li&gt; </p><p>&lt;li&gt;Acts 13:2, 13 &amp;ndash; Developed gifted assistant&lt;/li&gt; </p><p>&lt;li&gt;Acts 15:39 &amp;ndash; Restored a youthful deserter&lt;/li&gt; </p><p>&lt;/ul&gt; </p><p>&lt;p&gt;Probable characteristics of this gift:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;You feel free to counsel others into certain courses of action.&lt;/li&gt; </p><p>&lt;li&gt;You enjoy sharing a verse of scripture with someone that has changed some specific &lt;br /&gt;area of your life.&lt;/li&gt; </p><p>&lt;li&gt;You enjoy people.&lt;/li&gt; </p><p>&lt;li&gt;You have personally faced many different situations in your life that God has used to help you grow in specific areas.&lt;/li&gt; </p><p>&lt;li&gt;You are able to identify with the feelings and emotions of another.&lt;/li&gt; </p><p>&lt;li&gt;You accompany biblical truth with practical steps of action.&lt;/li&gt; </p><p>&lt;li&gt;You can see how God is using difficulties in people&amp;rsquo;s lives to produce new levels of maturity.&lt;/li&gt; </p><p>&lt;li&gt;You find yourself emotionally drained after ministering to someone because of your &lt;br /&gt;ability to identify with another&amp;rsquo;s difficulties.&lt;/li&gt; </p><p>&lt;li&gt;You have people come to you on many occasions to receive help from you with problems with which they are wrestling.&lt;/li&gt; </p><p>&lt;li&gt;You find people like to be around you because you have a tendency to cheer up people.&lt;/li&gt; </p><p>&lt;li&gt;You find yourself just spontaneously encouraging or affirming people.&lt;/li&gt; </p><p>&lt;li&gt;You enjoy sharing particular aspects of your life with others because you know God will &lt;br /&gt;use it to help them in areas of their life.&lt;/li&gt; </p><p>&lt;/ul&gt;</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "98D5EE08-633D-4635-80CD-169449604D18", "C1DBD74B-5B31-4E5D-B800-FF0B4C0DC583", @"1 Peter 4:9" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "98D5EE08-633D-4635-80CD-169449604D18", "EADE1EC5-E05B-4956-973E-BBBA2A02F0C0", @"<p>&lt;p&gt;This gift is not clearly delineated with other gifts and may be a specific application of the &lt;br /&gt;Helps/Service (1 Peter 4:9-11). All believers are to exhibit hospitality (Hebrews 13:2). &lt;br /&gt;Hospitality in general is a qualification for spiritual leadership (1 Timothy 3:2; Titus 1:8) and for &lt;br /&gt;widows who are supported by the church (1 Timothy 5:10). This gift is to be used to provide for &lt;br /&gt;itinerant ministers (missionaries, etc.) (3 John 5-8). The people with this gift need to see &lt;br /&gt;themselves as an extension of their guest&amp;rsquo;s ministry and, therefore, as accountable for their &lt;br /&gt;guest&amp;rsquo;s teachings.&lt;br /&gt;Biblical examples:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;Lydia: Acts 16:15&lt;/li&gt; </p><p>&lt;li&gt;Accommodating Paul: Acts 21:4,7,8,16&lt;/li&gt; </p><p>&lt;li&gt;Gaius: Romans 16:23; 3 John 5-8&lt;/li&gt; </p><p>&lt;li&gt;Philemon: Philemon 2:22&lt;/li&gt; </p><p>&lt;/ul&gt; </p><p>&lt;p&gt;Probable characteristics of this gift are:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;You often pray that God will use your home to accommodate visitors.&lt;/li&gt; </p><p>&lt;li&gt;You enjoy meeting new people.&lt;/li&gt; </p><p>&lt;li&gt;You enjoy having visitors in your home, and often, your guest registry looks like a hotel registry (i.e. you often have people staying with you).&lt;/li&gt; </p><p>&lt;li&gt;You enjoy preparing meals for guests.&lt;/li&gt; </p><p>&lt;li&gt;You are relaxed entertaining people in your home and enjoy having people over who you do not know well.&lt;/li&gt; </p><p>&lt;li&gt;You are quick to invite people you have just met to your residence.&lt;/li&gt; </p><p>&lt;li&gt;People feel very comfortable in your home and do not feel as if they are imposing. They often &amp;ldquo;kick off their shoes&amp;rdquo; and find it easy to relax.&lt;/li&gt; </p><p>&lt;li&gt;You are not upset if your home is not in perfect order when someone drops in.&lt;/li&gt; </p><p>&lt;li&gt;Many people just stop by.&lt;/li&gt; </p><p>&lt;li&gt;You sometimes think others may be too uptight concerning having guests visit.&lt;/li&gt; </p><p>&lt;li&gt;You often open your home for bible studies and social functions.&lt;/li&gt; </p><p>&lt;li&gt;You enjoy asking neighbors over.&lt;/li&gt; </p><p>&lt;li&gt;Others see you as a friendly and open person.&lt;/li&gt; </p><p>&lt;/ul&gt;</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A1CB038C-AAFC-4745-A7D2-7C8BA5028F05", "C1DBD74B-5B31-4E5D-B800-FF0B4C0DC583", @"Romans 12:8" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A1CB038C-AAFC-4745-A7D2-7C8BA5028F05", "EADE1EC5-E05B-4956-973E-BBBA2A02F0C0", @"<p>&lt;p&gt;This gift is to be used diligently (Romans 12:8). This gift is in part exercised by the Elders Board &lt;br /&gt;(1 Timothy 5:17). Those using this gift must first exemplify spiritual maturity and ministry fruit &lt;br /&gt;(1 Timothy 3; Titus 1). Age is not a determining factor in using this gift (1 Timothy 4:12). One &lt;br /&gt;should be tested before assuming such a position (1 Timothy 3:10) &lt;br /&gt;Biblical examples:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;Paul: 2 Corinthians&lt;/li&gt; </p><p>&lt;li&gt;Timothy: 1 &amp;amp; 2 Timothy&lt;/li&gt; </p><p>&lt;li&gt;Titus: Titus&lt;/li&gt; </p><p>&lt;/ul&gt; </p><p>&lt;p&gt;&lt;br /&gt;Probable characteristics of this gift:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;You are often setting an example in behavior for others.&lt;/li&gt; </p><p>&lt;li&gt;Your prayers consist of tasks which need accomplishing.&lt;/li&gt; </p><p>&lt;li&gt;Others readily follow you, and they see you confidently knowing what to do next.&lt;/li&gt; </p><p>&lt;li&gt;If a male, your family also responds well to your leadership.&lt;/li&gt; </p><p>&lt;li&gt;You are a well-disciplined person.&lt;/li&gt; </p><p>&lt;li&gt;You will not ask others to do what you will not do yourself.&lt;/li&gt; </p><p>&lt;li&gt;You are relaxed when in leadership positions.&lt;/li&gt; </p><p>&lt;li&gt;You delegate well.&lt;/li&gt; </p><p>&lt;li&gt;You do not enjoy maintaining a program or administering details.&lt;/li&gt; </p><p>&lt;li&gt;You tend to &amp;ldquo;take over&amp;rdquo; in a situation where there is not a designated leader.&lt;/li&gt; </p><p>&lt;li&gt;People look to you for leadership and direction.&lt;/li&gt; </p><p>&lt;li&gt;You remain calm under pressure.&lt;/li&gt; </p><p>&lt;li&gt;You don&amp;rsquo;t allow criticism to distract from the task at hand.&lt;/li&gt; </p><p>&lt;li&gt;You desire to see things move quickly to a conclusion; then you want to move on to a &lt;br /&gt;new challenge.&lt;/li&gt; </p><p>&lt;li&gt;You can organize and motivate people to accomplish a desired task.&lt;/li&gt; </p><p>&lt;/ul&gt;</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A276421D-F662-4723-99DA-6FDF3E9CFF7C", "C1DBD74B-5B31-4E5D-B800-FF0B4C0DC583", @"1  Corinthians  12:28" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A276421D-F662-4723-99DA-6FDF3E9CFF7C", "EADE1EC5-E05B-4956-973E-BBBA2A02F0C0", @"<p>&lt;p&gt;The word for this gift is derived from &amp;ldquo;captain&amp;rdquo; or &amp;ldquo;pilot&amp;rdquo; of a ship (Acts 27:11, Revelation &lt;br /&gt;18:17). It implies keeping the ship on a predetermined course, not determining the course).&lt;br /&gt;This gift is implementing plans and programs consistent with the purpose and goal of the Body.&lt;br /&gt;This gift is not necessary for leaders, but they must have someone with this gift close by to &lt;br /&gt;execute plans (Acts 15:40). This gift if often used to continue ministries started by others &lt;br /&gt;(Acts 17:15) &lt;br /&gt;Biblical examples:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;Silas: Acts 15:22-35; 17:14-15&lt;/li&gt; </p><p>&lt;li&gt;Timothy: Acts 17:14-15, 1 Thessalonians 3:1&lt;/li&gt; </p><p>&lt;/ul&gt; </p><p>&lt;p&gt;Probable characteristics of this gift:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;You enjoy working with intricate details to make everything work together.&lt;/li&gt; </p><p>&lt;li&gt;Your prayers often consist of asking for God to bring many different factors together to &lt;br /&gt;accomplish a given task for His kingdom.&lt;/li&gt; </p><p>&lt;li&gt;You enjoy long hours in the office handling business affairs, such as staff problems, &lt;br /&gt;phone calls, closing deals, writing or dictating.&lt;/li&gt; </p><p>&lt;li&gt;You have the ability to make a committee or organization run like a &amp;ldquo;finely-oiled &lt;br /&gt;machine.&amp;rdquo;&lt;/li&gt; </p><p>&lt;li&gt;You are a very organized person and use outlines or flow charts to develop a plan.&lt;/li&gt; </p><p>&lt;li&gt;You often work from a list.&lt;/li&gt; </p><p>&lt;li&gt;You are a logical thinker and able to keep several projects on target.&lt;/li&gt; </p><p>&lt;li&gt;You enjoy working with people to enable them to work together smoothly toward a &lt;br /&gt;common objective.&lt;/li&gt; </p><p>&lt;li&gt;You become frustrated with brainstorming sessions which never get around to &lt;br /&gt;implementation.&lt;/li&gt; </p><p>&lt;li&gt;Others see you as a very pragmatic person.&lt;/li&gt; </p><p>&lt;li&gt;You enjoy working on and within systems.&lt;/li&gt; </p><p>&lt;li&gt;You feel comfortable working with or around computers.&lt;/li&gt; </p><p>&lt;li&gt;You are very loyal to your goals.&lt;/li&gt; </p><p>&lt;li&gt;You tend to be a perfectionist and often expect the same of others.&lt;/li&gt; </p><p>&lt;/ul&gt;</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A2C7074E-AC97-4D89-9240-47A552CDC4C0", "C1DBD74B-5B31-4E5D-B800-FF0B4C0DC583", @"Ephesians 4:11" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A2C7074E-AC97-4D89-9240-47A552CDC4C0", "EADE1EC5-E05B-4956-973E-BBBA2A02F0C0", @"<p>&lt;p&gt;Jesus was the ultimate apostle (Hebrews 3:1). &lt;br /&gt;&amp;ldquo;Apostles&amp;rdquo; included the original 12 disciples (minus Judas). They had been with Jesus from the &lt;br /&gt;beginning. They had been called by Christ (Mark 3:14) and reported back to Him (Mark 6:30). &lt;br /&gt;They had witnessed the resurrection (Acts 1:22). They were the spiritual, structural, and &lt;br /&gt;doctrinal foundation of the church (Matthew 16:18,19; Acts 2:38-42, 15:1-35; Ephesians 2:20). &lt;br /&gt;They worked miracles as a sign of their apostleship (2 Corinthians 12:12; Acts 2:43, 5:12, 8:18). &lt;br /&gt;They will hold special positions in the events of the end of our world (Luke 22:29-30; &lt;br /&gt;Revelation 21:14).&lt;br /&gt;Other Apostles:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;Paul: Romans 1:1; 1 Corinthians 1:1&lt;/li&gt; </p><p>&lt;li&gt;Barnabas: Acts 14:4, 14&lt;/li&gt; </p><p>&lt;li&gt;James (Jesus&amp;rsquo; half-brother): Galatians 1:19; 1 Corinthians 15:7&lt;/li&gt; </p><p>&lt;li&gt;Silas and Timothy: 1 Thessalonians 1:1; 2:6&lt;/li&gt; </p><p>&lt;li&gt;Andronicus and Junia: Romans 16:7&lt;/li&gt; </p><p>&lt;li&gt;Many others: 1 Corinthians 15:7 (above apostles mentioned in 1 Corinthians 15:5,6).&lt;/li&gt; </p><p>&lt;li&gt;Apostles were impersonated. (2 Corinthians 11:13; Revelations 2:2)&lt;/li&gt; </p><p>&lt;/ul&gt; </p><p>&lt;p&gt;Probable characteristics of this gift are:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;Your prayers often include requests for people groups who have little or no exposure to &lt;br /&gt;the gospel.&lt;/li&gt; </p><p>&lt;li&gt;You have an unusual ability to survive difficult situations.&lt;/li&gt; </p><p>&lt;li&gt;You adjust well to adversity, almost thrive on it.&lt;/li&gt; </p><p>&lt;li&gt;You have a real burden for sharing the Gospel in places that it has not permeated.&lt;/li&gt; </p><p>&lt;li&gt;You have the ability to learn other languages easily.&lt;/li&gt; </p><p>&lt;li&gt;You feel at ease with others in spite of vast cultural differences with them.&lt;/li&gt; </p><p>&lt;li&gt;You share Christ with people in terms they can understand and relate to.&lt;/li&gt; </p><p>&lt;li&gt;You can live with situations which are uncertain.&lt;/li&gt; </p><p>&lt;li&gt;You are a jack-of-all-trades.&lt;/li&gt; </p><p>&lt;li&gt;You enjoy learning from people vastly different from you.&lt;/li&gt; </p><p>&lt;li&gt;You and your family easily adapt to other cultures.&lt;/li&gt; </p><p>&lt;li&gt;You lack roots which would inhibit your mobility.&lt;/li&gt; </p><p>&lt;li&gt;You sometimes become frustrated with others&amp;rsquo; apparent lack of burden for lost souls all &lt;br /&gt;around you and the world.&lt;/li&gt; </p><p>&lt;li&gt;Being separated from extended family and friends does not inhibit your ministry.&lt;/li&gt; </p><p>&lt;/ul&gt;</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C4259D6E-675C-417B-9175-6D599C86A204", "C1DBD74B-5B31-4E5D-B800-FF0B4C0DC583", @"Romans 12:8" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C4259D6E-675C-417B-9175-6D599C86A204", "EADE1EC5-E05B-4956-973E-BBBA2A02F0C0", @"<p>&lt;p&gt;This gift is not to be associated only with wealthy people (2 Corinthians 8:1-2). This gift is to be &lt;br /&gt;exercised with &amp;ldquo;liberality.&amp;rdquo; The work means &amp;ldquo;simplicity&amp;rdquo; or &amp;ldquo;sincerity&amp;rdquo; or &amp;ldquo;purity&amp;rdquo; (Romans &lt;br /&gt;12:8).&lt;br /&gt;The purposes for which this gift is to be used are:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;To meet the needs of believers within your local assembly. (Ephesians 4:28; Galatians &lt;br /&gt;6:10; 1 John 3:17)&lt;/li&gt; </p><p>&lt;li&gt;To meet the needs of believers in other assemblies. (Romans 15: 25,26)&lt;/li&gt; </p><p>&lt;li&gt;To meet the needs of those who labor in preaching and teaching. (1 Timothy 5:17, 18; &lt;br /&gt;Philippians 4:10)&lt;/li&gt; </p><p>&lt;li&gt;To meet the needs of non-believers.&lt;/li&gt; </p><p>&lt;li&gt;The use of this gift will many times be used at considerable cost to the giver. (Luke 21:1-&lt;br /&gt;4)&lt;/li&gt; </p><p>&lt;li&gt;The gift should be used in secret as much as possible. (Matthew 6:2-4)&lt;/li&gt; </p><p>&lt;/ul&gt; </p><p>&lt;p&gt;Probable characteristics of this gift:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;You have an inner sense of unrest when you see someone in financial or material need.&lt;/li&gt; </p><p>&lt;li&gt;You have a reluctance to let the person helped know where the help came from.&lt;/li&gt; </p><p>&lt;li&gt;You sense real joy when giving to meet someone&amp;rsquo;s need.&lt;/li&gt; </p><p>&lt;li&gt;You have the ability to make wise purchases and investments.&lt;/li&gt; </p><p>&lt;li&gt;You are burdened because you don&amp;rsquo;t see others sacrificing financially to meet needs you see to be important.&lt;/li&gt; </p><p>&lt;li&gt;You want to be a part of the work or project to which you give.&lt;/li&gt; </p><p>&lt;li&gt;You don&amp;rsquo;t need to be pressured to give.&lt;/li&gt; </p><p>&lt;li&gt;You avoid the spotlight that sometimes comes to those who give significant amounts of money.&lt;/li&gt; </p><p>&lt;li&gt;You willingly live a life of personal sacrifice so that others may have their basic needs met.&lt;/li&gt; </p><p>&lt;li&gt;You are sometimes taken advantage of by people in need.&lt;/li&gt; </p><p>&lt;li&gt;You have a tendency to think about others before you think about yourself.&lt;/li&gt; </p><p>&lt;li&gt;You realize that all of what you have belongs to God and that you are only a steward of what you &amp;ldquo;possess&amp;rdquo;.&lt;/li&gt; </p><p>&lt;li&gt;When giving, you don&amp;rsquo;t think about tax advantages before deciding to give to something&lt;/li&gt; </p><p>&lt;/ul&gt;</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C7291F22-05F0-4EF9-A7C2-2CFEBFEBCB45", "C1DBD74B-5B31-4E5D-B800-FF0B4C0DC583", @"Ephesians 4:11" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C7291F22-05F0-4EF9-A7C2-2CFEBFEBCB45", "EADE1EC5-E05B-4956-973E-BBBA2A02F0C0", @"<p>&lt;p&gt;This gift is a combination of Pastoring: Shepherding and Teaching (Ephesians 4:11). The primary &lt;br /&gt;responsibility of this gift is preparing the members of the Body for ministry (Ephesians 4:12). &lt;br /&gt;Only as others use their gift is the Body built up (Ephesians 4:12,13). The offices of Apostles, &lt;br /&gt;Prophets, and Evangelists also include preparing others for works of ministry. The teaching &lt;br /&gt;should correct, rebuke, encourage, and contain careful instruction, and considerable effort and &lt;br /&gt;time will be spent accordingly. The gift should not neglect evangelism (2 Timothy 4:5). This &lt;br /&gt;position may be supported financially to facilitate preaching and teaching (1 Timothy 5:17-18).&lt;br /&gt;Biblical example: 1 &amp;amp; 2 Timothy&lt;br /&gt;Probable characteristics of this gift are:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;Your prayers are for your leadership of the flock with which you have been entrusted.&lt;/li&gt; </p><p>&lt;li&gt;People look to you for leadership.&lt;/li&gt; </p><p>&lt;li&gt;You are a disciplined person.&lt;/li&gt; </p><p>&lt;li&gt;You often feel internal tension between the task at hand and the people problems which impede progress.&lt;/li&gt; </p><p>&lt;li&gt;You are goal-oriented but people focused.&lt;/li&gt; </p><p>&lt;li&gt;You are successful at preparing and motivating others to become involved in using their gifts in ministry.&lt;/li&gt; </p><p>&lt;li&gt;You train leaders well.&lt;/li&gt; </p><p>&lt;li&gt;You articulate your thoughts well.&lt;/li&gt; </p><p>&lt;li&gt;Your desire is to please God first, people second.&lt;/li&gt; </p><p>&lt;li&gt;You are an example of a godly lifestyle.&lt;/li&gt; </p><p>&lt;li&gt;You are able to offer decisive leadership to the Body you are shepherding.&lt;/li&gt; </p><p>&lt;li&gt;You are frustrated when you do not see others growing into maturity or using their gifts for the building of the Body.&lt;/li&gt; </p><p>&lt;li&gt;You are concerned that all of the needs of the Body are met, though not necessarily by yourself.&lt;/li&gt; </p><p>&lt;/ul&gt;</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "E8278791-2400-4DDA-AEAA-C6F11E0AC9D0", "C1DBD74B-5B31-4E5D-B800-FF0B4C0DC583", @"Romans 12:7, 1 Corinthians 12:28" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "E8278791-2400-4DDA-AEAA-C6F11E0AC9D0", "EADE1EC5-E05B-4956-973E-BBBA2A02F0C0", @"<p>&lt;p&gt;There were positions in the New Testament church for some who had this gift (Acts 13:1). &lt;br /&gt;As a result of the use of this gift, people will not just know more, but their behavior will actually &lt;br /&gt;change (Acts 11:25, 26). Everyone is called to teach (Matthew 19, 20). Exercising this gift &lt;br /&gt;carries with it a greater accountability for the teacher (James 3:1). The goal of this gift is &lt;br /&gt;spiritual maturity. (Colossians 1:28-29; 2 Timothy 3:16-17).&lt;br /&gt;Some may use this gift in a:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;Large group - Acts 5:42; 19:11; 20:20&lt;/li&gt; </p><p>&lt;li&gt;Small group - Acts 2:42; 5:42; 20:20&lt;/li&gt; </p><p>&lt;li&gt;One-on-one - Acts 18:26; 2 Timothy 2:2&lt;/li&gt; </p><p>&lt;/ul&gt; </p><p>&lt;p&gt;Biblical examples:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;Apollos: Acts 18:24-28&lt;/li&gt; </p><p>&lt;li&gt;Paul: Acts 18:11; 20:20&lt;/li&gt; </p><p>&lt;li&gt;Timothy: 2 Timothy 2:2; 4:11&lt;/li&gt; </p><p>&lt;/ul&gt; </p><p>&lt;p&gt;Probable characteristics of this gift are:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;Your prayers are often for understanding with those you are teaching.&lt;/li&gt; </p><p>&lt;li&gt;You have a great deal of patience for those who are learning from you.&lt;/li&gt; </p><p>&lt;li&gt;You have the ability to communicate scripture in an understandable way.&lt;/li&gt; </p><p>&lt;li&gt;You have seen others apply your teachings to their lives and grow as a result.&lt;/li&gt; </p><p>&lt;li&gt;You are not easily threatened by criticism when teaching.&lt;/li&gt; </p><p>&lt;li&gt;You enjoy spending time in study for your teaching.&lt;/li&gt; </p><p>&lt;li&gt;Your study is intended to help you communicate with others.&lt;/li&gt; </p><p>&lt;li&gt;As a lay person, you find little time for other ministries in addition to teaching.&lt;/li&gt; </p><p>&lt;li&gt;You may communicate through the use of various media.&lt;/li&gt; </p><p>&lt;li&gt;You enjoy people.&lt;/li&gt; </p><p>&lt;li&gt;You have a genuine concern for those you are teaching.&lt;/li&gt; </p><p>&lt;li&gt;You are concerned that precise words and illustrations you use communicate what you want to say.&lt;/li&gt; </p><p>&lt;li&gt;You are always learning and then wanting to teach it to others.&lt;/li&gt; </p><p>&lt;li&gt;You are able to apply truths to the lives of others.&lt;/li&gt; </p><p>&lt;li&gt;You sometimes feel you could communicate a point better than someone else who is teaching.&lt;/li&gt; </p><p>&lt;li&gt;You are a disciplined person.&lt;/li&gt; </p><p>&lt;/ul&gt;</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FC4F1B46-F0C3-45B0-9FD9-D15F4FD05A31", "C1DBD74B-5B31-4E5D-B800-FF0B4C0DC583", @"1 Peter 5:1-4; Ephesians 4:11" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FC4F1B46-F0C3-45B0-9FD9-D15F4FD05A31", "EADE1EC5-E05B-4956-973E-BBBA2A02F0C0", @"<p>&lt;p&gt;The word &amp;ldquo;pastor&amp;rdquo; is based on the caring for animals (sheep), and its function is not well &lt;br /&gt;understood today. Jesus is the Good and Ultimate Shepherd of all believers (Psalms 23:1; John &lt;br /&gt;10:11, 14; 1 Peter 2:25). Jesus has gifted some as under-shepherds to care for His followers &lt;br /&gt;(1 Peter 5:2). This gift is concerned with serving and caring for the needs of a small group of &lt;br /&gt;people (1 Peter 5:2-4). This gift is concerned with protecting the believers from attacks, both &lt;br /&gt;from within and from the outside (Acts 20:29-31). &lt;br /&gt;This is not a passive or docile gift, but must always be alert (Acts 20:29-31). This gift is often &lt;br /&gt;combined with other gifts (Ephesians 4:11). This is not the dominant gift of the Senior Pastor for &lt;br /&gt;a large growing church. &lt;br /&gt;Biblical examples: Acts 20:28-35 -- Ephesians elders &lt;br /&gt;Probable characteristics of this gift are:&lt;/p&gt; </p><p>&lt;ul&gt; </p><p>&lt;li&gt;You are a people person and enjoy being around others.&lt;/li&gt; </p><p>&lt;li&gt;You tend to build deep relationships with others and do not enjoy superficial or casual friendships for very long.&lt;/li&gt; </p><p>&lt;li&gt;Your prayers are often comprised of requests for the deep hurts and needs of those whom you are shepherding.&lt;/li&gt; </p><p>&lt;li&gt;Others are attracted to you because they sense that you truly care for them.&lt;/li&gt; </p><p>&lt;li&gt;People see you as a warm person with a real concern for the welfare of others.&lt;/li&gt; </p><p>&lt;li&gt;You are frustrated with tasks that take you away from ministering to those around you for very long.&lt;/li&gt; </p><p>&lt;li&gt;You often see others too involved in their own goals to care for those around themselves.&lt;/li&gt; </p><p>&lt;li&gt;You do not feel comfortable in a tightly regimented time schedule.&lt;/li&gt; </p><p>&lt;li&gt;When you share with others, you often focus on the relational aspects of the Christian life.&lt;/li&gt; </p><p>&lt;li&gt;You feel inadequate to meet all the needs of those to whom you minister.&lt;/li&gt; </p><p>&lt;li&gt;You have a great burden to protect those around you from non-Biblical influences.&lt;/li&gt; </p><p>&lt;li&gt;You feel frustrated when relationships with others do not last long-term.&lt;/li&gt; </p><p>&lt;li&gt;You enjoy being able to focus on a few rather than being &amp;ldquo;spread too thin.&amp;rdquo;&lt;/li&gt; </p><p>&lt;/ul&gt;</p>" );
        }
        /// <summary>
        /// Add spiritual gift
        /// </summary>
        private void DeleteSpiritualGiftDefinedType()
        {
            RockMigrationHelper.DeleteAttribute( "C1DBD74B-5B31-4E5D-B800-FF0B4C0DC583" ); // Scripture
            RockMigrationHelper.DeleteAttribute( "EADE1EC5-E05B-4956-973E-BBBA2A02F0C0" ); // Details
            RockMigrationHelper.DeleteDefinedValue( "0894EDBA-8FC8-4433-877C-53351A06A8B7" ); // Mercy
            RockMigrationHelper.DeleteDefinedValue( "0F8D41AA-7236-40BF-AA37-980BCCF4A881" ); // Evangelism
            RockMigrationHelper.DeleteDefinedValue( "13C40209-F41D-4C1D-83D3-2EC530588245" ); // Helps/Service
            RockMigrationHelper.DeleteDefinedValue( "3EB352F3-F624-4ED6-A9EE-7951B71B1952" ); // Discernment
            RockMigrationHelper.DeleteDefinedValue( "462A5D10-6DEA-43D7-96EF-8F82FF1E2E14" ); // Knowledge
            RockMigrationHelper.DeleteDefinedValue( "4ADAEED1-D0E6-4DA4-A0BA-8E7D058075C4" ); // Prophecy
            RockMigrationHelper.DeleteDefinedValue( "5F1F5A92-D981-4027-A4BC-C3642E784D0B" ); // Wisdom
            RockMigrationHelper.DeleteDefinedValue( "7B30E2BA-9461-4688-9B43-D2B774E33A18" ); // Faith
            RockMigrationHelper.DeleteDefinedValue( "809F65A6-1759-472A-8B8B-F37009F476BF" ); // Encouragement
            RockMigrationHelper.DeleteDefinedValue( "98D5EE08-633D-4635-80CD-169449604D18" ); // Hospitality
            RockMigrationHelper.DeleteDefinedValue( "A1CB038C-AAFC-4745-A7D2-7C8BA5028F05" ); // Leadership
            RockMigrationHelper.DeleteDefinedValue( "A276421D-F662-4723-99DA-6FDF3E9CFF7C" ); // Administration
            RockMigrationHelper.DeleteDefinedValue( "A2C7074E-AC97-4D89-9240-47A552CDC4C0" ); // Apostleship
            RockMigrationHelper.DeleteDefinedValue( "C4259D6E-675C-417B-9175-6D599C86A204" ); // Giving
            RockMigrationHelper.DeleteDefinedValue( "C7291F22-05F0-4EF9-A7C2-2CFEBFEBCB45" ); // Pastor-Teacher
            RockMigrationHelper.DeleteDefinedValue( "E8278791-2400-4DDA-AEAA-C6F11E0AC9D0" ); // Teaching
            RockMigrationHelper.DeleteDefinedValue( "FC4F1B46-F0C3-45B0-9FD9-D15F4FD05A31" ); // Pastor-Shepherd
            RockMigrationHelper.DeleteDefinedType( "9D9628F0-7FC5-411E-B9DF-740AA17689A0" ); // Spiritual Gifts
        }

        /// <summary>
        /// Add dominant gift
        /// </summary>
        private void AddDominantGiftPersonAttribute()
        {
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DEFINED_VALUE, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Dominant Gifts", "core_DominantGifts", "", "", 15, "", SystemGuid.Attribute.PERSON_DOMINANT_GIFTS );
            RockMigrationHelper.AddAttributeQualifier( SystemGuid.Attribute.PERSON_DOMINANT_GIFTS, "allowmultiple", "True", "3E4DC7F0-11CF-42A1-8FB9-FDF278291D69" );
            // add attribute qualifiers
            Sql( string.Format( @"  DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '{0}')
  DECLARE @DefinedTypeId int = (SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = '{1}')


                  IF NOT EXISTS (
		                SELECT *
		                FROM AttributeQualifier
		                WHERE [AttributeId] = @AttributeId
                        AND [Key] = 'definedtype'
		                )
                  BEGIN                  
  INSERT INTO [AttributeQualifier] ([IsSystem], [AttributeId], [Key], [Value], [Guid])
  VALUES
  (0, @AttributeId, 'definedtype', @DefinedTypeId, '243D4409-4A8F-4C24-AADF-C8D6EA19750D')

END ", SystemGuid.Attribute.PERSON_DOMINANT_GIFTS, SystemGuid.DefinedType.SPIRITUAL_GIFTS ) );

            RockMigrationHelper.AddAttributeQualifier( SystemGuid.Attribute.PERSON_DOMINANT_GIFTS, "displaydescription", "False", "EB2563EE-4FEB-47DC-A7B1-AB3E555E05BC" );

            AddSecurityToAttributes( Rock.SystemGuid.Attribute.PERSON_DOMINANT_GIFTS );
        }

        /// <summary>
        /// Add supportive gift
        /// </summary>
        private void AddSupportiveGiftPersonAttribute()
        {
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DEFINED_VALUE, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Supportive Gifts", "core_SupportiveGifts", "", "", 16, "", SystemGuid.Attribute.PERSON_SUPPORTIVE_GIFTS );
            RockMigrationHelper.AddAttributeQualifier( SystemGuid.Attribute.PERSON_SUPPORTIVE_GIFTS, "allowmultiple", "True", "C8053078-B97D-4221-84EE-142010FE5478" );
            // add attribute qualifiers
            Sql( string.Format( @"  DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '{0}')
  DECLARE @DefinedTypeId int = (SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = '{1}')


                  IF NOT EXISTS (
		                SELECT *
		                FROM AttributeQualifier
		                WHERE [AttributeId] = @AttributeId
                        AND [Key] = 'definedtype'
		                )
                  BEGIN                  
  INSERT INTO [AttributeQualifier] ([IsSystem], [AttributeId], [Key], [Value], [Guid])
  VALUES
  (0, @AttributeId, 'definedtype', @DefinedTypeId, '42C078A6-B4A5-4B80-AB70-E1743D9568CF')

END ", SystemGuid.Attribute.PERSON_SUPPORTIVE_GIFTS, SystemGuid.DefinedType.SPIRITUAL_GIFTS ) );

            RockMigrationHelper.AddAttributeQualifier( SystemGuid.Attribute.PERSON_SUPPORTIVE_GIFTS, "displaydescription", "False", "7485DCFC-8ACC-4F20-8DAC-C190A5D7E489" );

            AddSecurityToAttributes( Rock.SystemGuid.Attribute.PERSON_SUPPORTIVE_GIFTS );
        }

        /// <summary>
        /// Add other gift
        /// </summary>
        private void AddOtherGiftPersonAttribute()
        {
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DEFINED_VALUE, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Other Gifts", "core_OtherGifts", "", "", 17, "", SystemGuid.Attribute.PERSON_OTHER_GIFTS );
            RockMigrationHelper.AddAttributeQualifier( SystemGuid.Attribute.PERSON_OTHER_GIFTS, "allowmultiple", "True", "4B307B6B-2EEC-476D-9ADF-1A2C1A9C5DC8" );
            // add attribute qualifiers
            Sql( string.Format( @"  DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '{0}')
  DECLARE @DefinedTypeId int = (SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = '{1}')


                  IF NOT EXISTS (
		                SELECT *
		                FROM AttributeQualifier
		                WHERE [AttributeId] = @AttributeId
                        AND [Key] = 'definedtype'
		                )
                  BEGIN                  
  INSERT INTO [AttributeQualifier] ([IsSystem], [AttributeId], [Key], [Value], [Guid])
  VALUES
  (0, @AttributeId, 'definedtype', @DefinedTypeId, 'D342A5C2-0F65-495A-887F-B1E16F29655A')

END ", SystemGuid.Attribute.PERSON_OTHER_GIFTS, SystemGuid.DefinedType.SPIRITUAL_GIFTS ) );

            RockMigrationHelper.AddAttributeQualifier( SystemGuid.Attribute.PERSON_OTHER_GIFTS, "displaydescription", "False", "26E1E7B9-C324-43E0-8819-763C00721B8A" );

            AddSecurityToAttributes( Rock.SystemGuid.Attribute.PERSON_OTHER_GIFTS );
        }

        /// <summary>
        /// Add security to attributes
        /// </summary>
        private void AddSecurityToAttributes( string attributeGuid )
        {
            RockMigrationHelper.AddSecurityAuthForAttribute(
                            attributeGuid,
                            0,
                            Rock.Security.Authorization.VIEW,
                            true,
                            Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                            ( int ) Rock.Model.SpecialRole.None,
                            Guid.NewGuid().ToString()
                            );
            RockMigrationHelper.AddSecurityAuthForAttribute(
               attributeGuid,
               1,
               Rock.Security.Authorization.VIEW,
               true,
               Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS,
               ( int ) Rock.Model.SpecialRole.None,
               Guid.NewGuid().ToString()
               );
            RockMigrationHelper.AddSecurityAuthForAttribute(
               attributeGuid,
               2,
               Rock.Security.Authorization.VIEW,
               true,
               Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
               ( int ) Rock.Model.SpecialRole.None,
               Guid.NewGuid().ToString()
               );
            RockMigrationHelper.AddSecurityAuthForAttribute(
                attributeGuid,
                3,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                ( int ) Rock.Model.SpecialRole.AllUsers,
                Guid.NewGuid().ToString()
             );

            RockMigrationHelper.AddSecurityAuthForAttribute(
                attributeGuid,
                0,
                Rock.Security.Authorization.EDIT,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) Rock.Model.SpecialRole.None,
                Guid.NewGuid().ToString()
                );
            RockMigrationHelper.AddSecurityAuthForAttribute(
               attributeGuid,
               1,
               Rock.Security.Authorization.EDIT,
               true,
               Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS,
               ( int ) Rock.Model.SpecialRole.None,
               Guid.NewGuid().ToString()
               );
            RockMigrationHelper.AddSecurityAuthForAttribute(
               attributeGuid,
               2,
               Rock.Security.Authorization.EDIT,
               true,
               Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
               ( int ) Rock.Model.SpecialRole.None,
               Guid.NewGuid().ToString()
               );
            RockMigrationHelper.AddSecurityAuthForAttribute(
                attributeGuid,
                3,
                Rock.Security.Authorization.EDIT,
                false,
                null,
                ( int ) Rock.Model.SpecialRole.AllUsers,
                Guid.NewGuid().ToString()
             );
        }

        /// <summary>
        /// Add extended attributes to profile page
        /// </summary>
        private void AddExtendedAttributesInProfilePage()
        {
            RockMigrationHelper.AddBlock( true, "1C737278-4CBA-404B-B6B3-E3F0E05AB5FE", "", "D70A59DC-16BE-43BE-9880-59598FA7A94C", "TrueWiring", "SectionB1", @"", @"", 2, "0C244AA1-2473-4749-8D7E-81CAA415C886" );
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '0C244AA1-2473-4749-8D7E-81CAA415C886'" );  // Page: Extended Attributes,  Zone: SectionB1,  Block: TrueWiring
            RockMigrationHelper.AddBlockAttributeValue( "0C244AA1-2473-4749-8D7E-81CAA415C886", "235C6D48-E1D1-410C-8006-1EA412BC12EF", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "0C244AA1-2473-4749-8D7E-81CAA415C886", "EC43CF32-3BDF-4544-8B6A-CE9208DD7C81", @"b08a3096-fcfa-4da0-b95d-1f3f11cc9969" );
            RockMigrationHelper.AddSecurityAuthForBlock(
                "0C244AA1-2473-4749-8D7E-81CAA415C886",
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                Model.SpecialRole.None,
                "2D22D182-1C57-42C6-B14F-D6A1A4EB9AC1" );
            RockMigrationHelper.AddSecurityAuthForBlock(
                "0C244AA1-2473-4749-8D7E-81CAA415C886",
                0,
                Rock.Security.Authorization.EDIT,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                Model.SpecialRole.None,
                "6BB3D285-11C2-49B4-B3E2-22203FEF42C9" );
            RockMigrationHelper.AddSecurityAuthForBlock(
                "0C244AA1-2473-4749-8D7E-81CAA415C886",
                1,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS,
                Model.SpecialRole.None,
                "0521568F-299F-412F-B815-3276EC073AF8"
                );
            RockMigrationHelper.AddSecurityAuthForBlock(
                "0C244AA1-2473-4749-8D7E-81CAA415C886",
                1,
                Rock.Security.Authorization.EDIT,
                true,
                Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS,
                Model.SpecialRole.None,
                "C6E6BFCB-C264-4771-806A-DB17459864FB" );
            RockMigrationHelper.AddSecurityAuthForBlock(
                "0C244AA1-2473-4749-8D7E-81CAA415C886",
                2,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
                Model.SpecialRole.None,
                "4EF4A430-C2A6-433E-B5E6-F2BE3C39F812" );
            RockMigrationHelper.AddSecurityAuthForBlock(
                "0C244AA1-2473-4749-8D7E-81CAA415C886",
                2,
                Rock.Security.Authorization.EDIT,
                true,
                Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
                Model.SpecialRole.None,
                "710DEC27-E2C5-4CE5-9334-5D7BBA9A4502" );
            RockMigrationHelper.AddSecurityAuthForBlock(
                "0C244AA1-2473-4749-8D7E-81CAA415C886",
                3,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                Model.SpecialRole.AllUsers,
                "0E2A4949-895B-4499-A099-051707EC4051" );
            RockMigrationHelper.AddSecurityAuthForBlock(
                "0C244AA1-2473-4749-8D7E-81CAA415C886",
                3,
                Rock.Security.Authorization.EDIT,
                false,
                null,
                Model.SpecialRole.AllUsers,
                "510A9463-BF30-489E-A3AE-497830907D34" );
        }

        private void AddPageWithRoutsForGiftAssessment()
        {
            RockMigrationHelper.AddPage( true, "EBAA5140-4B8F-44B8-B1E8-C73B654E4B22", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Gifts Assessment", "", "06410598-3DA4-4710-A047-A518157753AB", "" ); // Site:External Website
            RockMigrationHelper.AddPageRoute( "06410598-3DA4-4710-A047-A518157753AB", "GiftsAssessment", "1B580CA3-F1DB-443F-ABA4-F9C7EC6A8A1B" );// for Page:Gifts Assessment
            RockMigrationHelper.AddPageRoute( "06410598-3DA4-4710-A047-A518157753AB", "GiftsAssessment/{rckipid}", "B991B18C-9B71-4BA9-8149-760CF15F37F3" );// for Page:Gifts Assessment
            RockMigrationHelper.UpdateBlockType( "Gifts Assessment", "Allows you to take a spiritual gifts test and saves your spiritual gifts score.", "~/Blocks/Crm/GiftsAssessment.ascx", "CRM", "A7E86792-F0ED-46F2-988D-25EBFCD1DC96" );
            RockMigrationHelper.AddBlock( true, "06410598-3DA4-4710-A047-A518157753AB".AsGuid(), null, "F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(), "A7E86792-F0ED-46F2-988D-25EBFCD1DC96".AsGuid(), "Gifts Assessment", "Main", @"", @"", 0, "B76F0F54-E03A-4835-A69D-B6D6F6499D4A" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "A7E86792-F0ED-46F2-988D-25EBFCD1DC96", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Results Message", "ResultsMessage", "", @"The text (HTML) to display at the top of the results section.<span class='tip tip-lava'></span><span class='tip tip-html'></span>", 0, @"  <div class='row'>      <div class='col-md-12'>      <h2 class='h2'> Dominant Gifts</h2>      </div>      <div class='col-md-9'>      <table class='table table-bordered table-responsive'>      <thead>          <tr>              <th>                  Spiritual Gift              </th>              <th>                  You are uniquely wired to:              </th>          </tr>      </thead>      <tbody>          {% for dominantGift in DominantGifts %}          <tr>          <td>          {{ dominantGift.Value }}          </td>          <td>          {{ dominantGift.Description }}              </td>          </tr>          {% endfor %}      </tbody>      </table>      </div>      </div>        <div class='row'>      <div class='col-md-12'>          <h2 class='h2'> Supportive Gifts</h2>      </div>      <div class='col-md-9'>          <table class='table table-bordered table-responsive'>              <thead>                  <tr>                     <th>                      Spiritual Gift                      </th>                      <th>                      You are uniquely wired to:                      </th>                  </tr>              </thead>              <tbody>              {% for supportiveGift in SupportiveGifts %}              <tr>                  <td>                  {{ supportiveGift.Value }}                  </td>                  <td>                  {{ supportiveGift.Description }}                  </td>              </tr>                  {% endfor %}              </tbody>          </table>      </div>  </div?  <div class='row'>      <div class='col-md-12'>          <h2 class='h2'> Other Gifts</h2>      </div>      <div class='col-md-9'>          <table class='table table-bordered table-responsive'>              <thead>                  <tr>                     <th>                      Spiritual Gift                      </th>                      <th>                      You are uniquely wired to:                      </th>                  </tr>              </thead>              <tbody>                  {% for otherGift in OtherGifts %}              <tr>                  <td>                  {{ otherGift.Value }}                  </td>                  <td>                      {{ otherGift.Description }}                  </td>              </tr>                  {% endfor %}             </tbody>          </table>      </div>  </div>  ", "85256610-56EB-4E6F-B62B-A5517B54B39E" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "A7E86792-F0ED-46F2-988D-25EBFCD1DC96", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Set Page Icon", "SetPageIcon", "", @"The css class name to use for the heading icon.", 1, @"fa fa-gift", "DA7752F5-9F21-4391-97F3-BB7D35F885CE" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "A7E86792-F0ED-46F2-988D-25EBFCD1DC96", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Set Page Title", "SetPageTitle", "", @"The text to display as the heading.", 0, @"Spiritual Gifts Assessment", "85107259-0A30-4F1A-A651-CBED5243B922" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "A7E86792-F0ED-46F2-988D-25EBFCD1DC96", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Retakes", "AllowRetakes", "", @"If enabled, the person can retake the test after the minimum days passes.", 3, @"True", "9DC69746-7AD4-4BC9-B6EE-27E24774CE5B" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "A7E86792-F0ED-46F2-988D-25EBFCD1DC96", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Instructions", "Instructions", "", @"The text (HTML) to display at the top of the instructions section.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 0, @"              <h2>Welcome to Your Spiritual Gifts Assessment</h2>              <p>{{ Person.NickName }}, the purpose of this assessment is to help you identify spiritual gifts that are most naturally used in the life of the local church. This survey does not include all spiritual gifts, just those that are often seen in action for most churches and most people.</p>              <p>In churches it’s not uncommon to see 90% of the work being done by a weary 10%. Why does this happen? Partially due to ignorance and partially due to avoidance of spiritual gifts. Here’s the process:</p>              <ol><li>Discover the primary gifts given to us at our Spiritual birth.</li>              <li>Learn what these gifts are and what they are not.</li>              <li>See where these gifts fit into the functioning of the body. </li>              </ol>              <p>When you are working within your Spirit-given gifts, you will be most effective for the body of Christ in your local setting. </p> <p>     Before you begin, please take a moment and pray that the Holy Spirit would guide your thoughts, calm your mind, and help you respond to each item as honestly as you can. Don't spend much time on each item. Your first instinct is probably your best response.</p>", "86C9E794-B678-4453-A831-FE348A440646" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "A7E86792-F0ED-46F2-988D-25EBFCD1DC96", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Questions", "NumberofQuestions", "", @"The number of questions to show per page while taking the test", 2, @"17", "861F4601-82B7-46E3-967F-2E03D769E2D2" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "A7E86792-F0ED-46F2-988D-25EBFCD1DC96", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Min Days To Retake", "MinDaysToRetake", "", @"The number of days that must pass before the test can be taken again.", 4, @"360", "44272FB2-27DC-452D-8BBB-2F76266FA92E" );
        }

    }
}
