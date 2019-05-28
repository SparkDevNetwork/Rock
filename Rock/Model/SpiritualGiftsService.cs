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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// Spiritual Gift Class for administering and scoring a Spiritual Gift Assessment
    /// </summary>
    public class SpiritualGiftsService
    {
        private static readonly double minDominantGiftScore = 0.5;

        private static readonly double maxSupportiveGiftScore = 0.4;

        private static readonly double minSupportiveGiftScore = -0.4;

        private static readonly double maxOtherGiftScore = -0.5;

        private const string ATTRIBUTE_DOMINANT_GIFTS = "core_DominantGifts";

        private const string ATTRIBUTE_SUPPORTIVE_GIFTS = "core_SupportiveGifts";

        private const string ATTRIBUTE_OTHER_GIFTS = "core_OtherGifts";

        private const string ATTRIBUTE_LAST_SAVE_DATE = "core_SpiritualGiftsLastSaveDate";

        /// <summary>
        /// Raw question data with code as key.
        /// </summary>
        private static Dictionary<string, string> questionData = new Dictionary<string, string>(){
            { "C1","You notice 'body language' that is not consistent with what the person is saying."},
            { "C3","You have an analytical mind that tries to figure out how to apply what you learn."},
            { "C4","You experience sadness when you hear of people who are hurting."},
            { "C5","You feel at ease with others who are culturally very different from you."},
            { "C6","You feel comfortable working with details requiring concentration."},
            { "C7","You have a passion to confront issues with truths God has given you."},
            { "C8","You have the ability to share Christ with people in terms they can understand."},
            { "C9","In the church, you have a willingness to do jobs which allows others to use their more public gifts."},
            { "C12","You enjoy entertaining people in your home who you do not know well."},
            { "C13","You pray for the deep hurts or needs of those for whom you are concerned."},
            { "C14","You lean toward fewer deep relationships rather than many superficial ones."},
            { "C15","You can get frustrated with tasks that take you away from serving those close to you."},
            { "C16","You enjoy inviting neighbors over to your home."},
            { "C17","You enjoy studying so you can communicate truth with others."},
            { "C18","You are a logical thinker which allows you to keep projects on target."},
            { "C19","When you share with individuals, they react in one extreme or another, but few are neutral."},
            { "C20","You have an understanding of what is happening beneath the surface which cannot be explained logically."},
            { "C21","In prayer, meditation, or scripture study, solutions come to mind."},
            { "C22","You enjoy being able to focus on a few individuals rather than relating to many."},
            { "C25","You tend to take over in a situation where no one is designated to lead."},
            { "C26","You have been through many situations that were not safe."},
            { "C29","People naturally look to you for direction."},
            { "C30","You are quick to invite people you have recently met to your home."},
            { "C31","Your reaction to seeing someone in difficulty is to reach out and help with their immediate need."},
            { "C33","You enjoy spending time studying so you can share with others."},
            { "C35","You have a good grasp of Biblical principles."},
            { "C37","When you learn something new, you want to teach it to others."},
            { "C39","You enjoy doing the little things that help other people."},
            { "C41","You are able to stand alone in spite of others not agreeing with you."},
            { "C42","You are not afraid to step out and trust God for something you sense He desires."},
            { "C45","You pray for the understanding of the needs of those whom you are teaching."},
            { "C47","You desire to move from accomplishing one challenge to tackling another."},
            { "C50","You feel uneasy when you see someone in financial or material need."},
            { "C51","Your intuitions are correct."},
            { "C56","You adapt easily to other cultures."},
            { "C57","You are seen as a courageous person because of your confidence that God is with you."},
            { "C58","You see solutions to problems before others see them."},
            { "C59","You feel free to give advice for others to pursue specific courses of action."},
            { "C60","You have the ability to see under-the-surface issues to the real problem."},
            { "C61","In your conversations with others, you find yourself talking about relational issues."},
            { "C62","You enjoy coming up with solutions to problems more than actually executing the solutions."},
            { "C63","People like to be around you because you cheer them up."},
            { "C64","When listening to others, you listen for the deep truths which underlie their message."},
            { "C65","Others see you exemplifying a Godly lifestyle."},
            { "C66","People naturally feel they are welcome in your home."},
            { "C67","People come to you with problems, because you understand what they are going through."},
            { "C68","You articulate your thoughts well."},
            { "C69","You attempt to reconcile various truths from scripture in order to understand how they relate."},
            { "C70","You do not have to ask what needs to be done; you already see it."},
            { "C71","You think about others' needs before you think of your own."},
            { "C72","You enjoy learning complex truths as you have a long attention span."},
            { "C74","Others follow you as they see you are confident in knowing the next step."},
            { "C75","You find yourself spontaneously affirming others."},
            { "C76","You pray for God to give you wisdom on what to do with your intuitions."},
            { "C77","You are concerned that your words communicate exactly what you want to say."},
            { "C78","You share experiences from your life, because you see God using this to help others."},
            { "C80","You are able to help people cross the line of faith in their relationship with Christ."},
            { "C81","You do the type of work that few others know about."},
            { "C83","You sacrifice so that others may have their basic needs met."},
            { "C85","You ask God for insight with deep theological truths."},
            { "C86","You give freely to needs when you become aware of them."},
            { "C87","You find it easy to decide on a course of action that needs to be taken."},
            { "C88","You are concerned that those whom you personally lead grow into maturity."},
            { "C89","You are able to offer guidance to those for whom you are responsible."},
            { "C90","People would see you as an empathetic person."},
            { "C92","You train others well."},
            { "C94","You enjoy working with the details to ensure everything flows together well."},
            { "C96","You find yourself in extremely challenging situations."},
            { "C99","You enjoy working with people in various roles to reach a common objective."},
            { "C100","You enjoy visiting people who are sick or needy."},
            { "C101","You sense a deep joy when giving to meet someone's need."},
            { "C102","You focus on accomplishing the tasks before you."},
            { "C103","You have a desire to help others."},
            { "C105","People do not feel as if they are imposing when they stay in your home."},
            { "C106","You are frustrated by negative people, because you are a positive person."},
            { "C108","You are not defeated by significant obstacles."},
            { "C109","You notice when others' body language does not match what they are saying."},
            { "C111","You see how God uses difficult situations to produce maturity in others' lives."},
            { "C112","You naturally move conversations in the direction of a person's relationship with Christ."},
            { "C113","Trusting God comes easily for you."},
            { "C114","When talking with others, their relationship with God comes up naturally."},
            { "C116","You remain calm under pressure."},
            { "C117","You have a passion to share the gospel in everything you do."},
            { "C118","You articulate your faith in a way that people respond positively."},
            { "C119","You grasp deep spiritual truths quickly."}
        };

        /// <summary>
        /// Z score to percentage
        /// </summary>
        private static Dictionary<double, double> zScoreToPercentage = new Dictionary<double, double>()
        {
            {3,99.9},
            { 2.9,99.8},
            { 2.8,99.7},
            { 2.7,99.6},
            { 2.6,99.5},
            { 2.5,99.4},
            { 2.4,99.2},
            { 2.3,98.9},
            { 2.2,98.6},
            { 2.1,98.2},
            { 2,97.8},
            { 1.9,97.1},
            { 1.8,96.4},
            { 1.7,95.5},
            { 1.6,94.5},
            { 1.5,93.3},
            { 1.4,91.9},
            { 1.3,90.3},
            { 1.2,88.5},
            { 1.1,86.4},
            { 1,84.1},
            { 0.9,78.8},
            { 0.8,78.8},
            { 0.7,75.8},
            { 0.6,72.6},
            { 0.5,69.2},
            { 0.4,65.5},
            { 0.3,61.8},
            { 0.2,57.9},
            { 0.1,54},
            { 0,50},
            { -0.1,46},
            { -0.2,42.1},
            { -0.3,38.2},
            { -0.4,34.5},
            { -0.5,30.9},
            { -0.6,27.4},
            { -0.7,24.2},
            { -0.8,21.2},
            { -0.9,18.4},
            { -1,15.9},
            { -1.1,13.6},
            { -1.2,11.5},
            { -1.3,9.7},
            { -1.4,8.1},
            { -1.5,6.7},
            { -1.6,5.5},
            { -1.7,4.5},
            { -1.8,3.6},
            { -1.9,2.9},
            { -2,2.3},
            { -2.1,1.8},
            { -2.2,1.4},
            { -2.3,1.1},
            { -2.4,0.8},
            { -2.5,0.6},
            { -2.6,0.5},
            { -2.7,0.4},
            { -2.8,0.3},
            { -2.9,0.2},
            { -3,0.1}
        };

        /// <summary>
        /// Raw question data with code as key.
        /// </summary>
        private static Dictionary<Guid, SpiritualGift> constructData = new Dictionary<Guid, SpiritualGift>()
        {
            { SystemGuid.DefinedValue.SPIRITUAL_GIFTS_ADMINISTRATION.AsGuid(),  new SpiritualGift(6.722,3.647, new List<string>(){"C6","C18","C94","C99","C102" }) },
            { SystemGuid.DefinedValue.SPIRITUAL_GIFTS_APOSTLESHIP.AsGuid(), new SpiritualGift(8.377, 3.947,  new List<string>(){"C5","C8","C26","C56","C96"}) },
            { SystemGuid.DefinedValue.SPIRITUAL_GIFTS_DISCERNMENT.AsGuid(), new SpiritualGift(6.586,3.853, new List<string>(){"C1","C20","C51","C76","C109"}) },
            { SystemGuid.DefinedValue.SPIRITUAL_GIFTS_ENCOURAGEMENT.AsGuid(), new SpiritualGift(5.960, 3.653, new List<string>(){"C59","C63","C75","C78","C111"} )},
            { SystemGuid.DefinedValue.SPIRITUAL_GIFTS_EVANGELISM.AsGuid(), new SpiritualGift(11.173,4.484, new List<string>(){"C80","C112","C114","C117","C118"} )},
            { SystemGuid.DefinedValue.SPIRITUAL_GIFTS_FAITH.AsGuid(), new SpiritualGift(7.130,3.677, new List<string>(){"C42","C57","C106","C108","C113"}) },
            { SystemGuid.DefinedValue.SPIRITUAL_GIFTS_GIVING.AsGuid(), new SpiritualGift(6.210,3.548, new List<string>(){"C50","C71","C83","C86","C101"} )},
            { SystemGuid.DefinedValue.SPIRITUAL_GIFTS_HELPS.AsGuid(), new SpiritualGift(5.377,3.028, new List<string>(){"C9","C39","C70","C81","C103"} )},
            { SystemGuid.DefinedValue.SPIRITUAL_GIFTS_HOSPITALITY.AsGuid(),new SpiritualGift(11.211, 4.800, new List<string>(){"C12","C16","C30","C66","C105"}) },
            { SystemGuid.DefinedValue.SPIRITUAL_GIFTS_KNOWLEDGE.AsGuid(),new SpiritualGift(8.753,4.095, new List<string>(){"C64","C69","C72","C85","C119"} )},
            { SystemGuid.DefinedValue.SPIRITUAL_GIFTS_LEADERSHIP.AsGuid(),new SpiritualGift(7.483,4.081, new List<string>(){"C25","C29","C47","C74","C116"} )},
            { SystemGuid.DefinedValue.SPIRITUAL_GIFTS_MERCY.AsGuid(),new SpiritualGift(6.774,3.664, new List<string>(){"C4","C31","C67","C90","C100"} )},
            { SystemGuid.DefinedValue.SPIRITUAL_GIFTS_PASTOR_SHEPHERD.AsGuid(),new SpiritualGift(7.227,3.248, new List<string>(){"C13","C14","C15","C22","C61"} )},
            { SystemGuid.DefinedValue.SPIRITUAL_GIFTS_PASTOR_TEACHER.AsGuid(), new SpiritualGift(8.249,3.612, new List<string>(){"C65","C68","C88","C89","C92"}) },
            { SystemGuid.DefinedValue.SPIRITUAL_GIFTS_PROPHECY.AsGuid(), new SpiritualGift(8.184,3.682, new List<string>(){"C7","C19","C35","C41","C60"} )},
            { SystemGuid.DefinedValue.SPIRITUAL_GIFTS_TEACHING.AsGuid(),new SpiritualGift(8.757,4.227, new List<string>(){"C17","C33","C37","C45","C77"} )},
            { SystemGuid.DefinedValue.SPIRITUAL_GIFTS_WISDOM.AsGuid(), new SpiritualGift(7.619,3.712, new List<string>(){"C3","C21","C58","C62","C87"} )}
        };

        /// <summary>
        /// Gets the questions.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetQuestions()
        {
            Random r = new Random();
            var questions = questionData.OrderBy( x => r.Next( 0, questionData.Count ) ).ToDictionary( item => item.Key, item => item.Value );
            return questions;
        }

        /// <summary>
        /// Scores the test.
        /// </summary>
        /// <param name="assessmentResponse">The assessment response.</param>
        /// <returns>returns a AssessmentResults object</returns>
        static public AssessmentResults GetResult( Dictionary<string, int> assessmentResponse )
        {
            AssessmentResults testResults = new AssessmentResults();
            testResults.LastSaveDate = RockDateTime.Now;
            var zScores = new Dictionary<Guid, double>();

            foreach ( var spiritualGiftDefinedValue in constructData.Keys )
            {
                var spiritualGift = constructData[spiritualGiftDefinedValue];
                double totalResponse = 0;
                foreach ( var construct in spiritualGift.Constructs )
                {
                    if ( assessmentResponse.ContainsKey( construct ) )
                    {
                        totalResponse += assessmentResponse[construct];
                    }
                }

                var zScore = Math.Round( ( totalResponse - spiritualGift.Mean ) / spiritualGift.StandardDeviation, 1 );
                zScores.AddOrReplace( spiritualGiftDefinedValue, zScore );
            }

            testResults.DominantGifts = zScores.Where( a => a.Value >= minDominantGiftScore ).OrderByDescending(a=>a.Value).Select( a => a.Key ).ToList();
            testResults.SupportiveGifts = zScores.Where( a => a.Value >= minSupportiveGiftScore && a.Value <= maxSupportiveGiftScore ).OrderByDescending( a => a.Value ).Select( a => a.Key ).ToList();
            testResults.OtherGifts = zScores.Where( a => a.Value <= maxOtherGiftScore ).OrderByDescending( a => a.Value ).Select( a => a.Key ).ToList();
            return testResults;
        }

        /// <summary>
        /// Saves Assessment results to a Person's PersonProperties
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="assessmentResults">The assessment results.</param>
        static public void SaveAssessmentResults( Person person, AssessmentResults assessmentResults )
        {
            person.LoadAttributes();

            person.SetAttributeValue( ATTRIBUTE_DOMINANT_GIFTS, assessmentResults.DominantGifts.AsDelimited( "," ) );
            person.SetAttributeValue( ATTRIBUTE_SUPPORTIVE_GIFTS, assessmentResults.SupportiveGifts.AsDelimited( "," ) );
            person.SetAttributeValue( ATTRIBUTE_OTHER_GIFTS, assessmentResults.OtherGifts.AsDelimited( "," ) );
            person.SetAttributeValue( ATTRIBUTE_LAST_SAVE_DATE, assessmentResults.LastSaveDate );
            person.SaveAttributeValues();
        }

        /// <summary>
        /// Loads and returns saved Assessment scores for the Person.
        /// </summary>
        /// <param name="person">The Person to get the scores for.</param>
        /// <returns>AssessmentResults</returns>
        static public AssessmentResults LoadSavedAssessmentResults( Person person )
        {
            AssessmentResults savedScores = new AssessmentResults();

            person.LoadAttributes();
            savedScores.DominantGifts = person.GetAttributeValues( ATTRIBUTE_DOMINANT_GIFTS ).AsGuidList();
            savedScores.SupportiveGifts = person.GetAttributeValues( ATTRIBUTE_SUPPORTIVE_GIFTS ).AsGuidList();
            savedScores.OtherGifts = person.GetAttributeValues( ATTRIBUTE_OTHER_GIFTS ).AsGuidList();
            savedScores.LastSaveDate = person.GetAttributeValue( ATTRIBUTE_LAST_SAVE_DATE ).AsDateTime();

            return savedScores;
        }

        /// <summary>
        /// The AssessmentResults struct used to return the final assessment scores
        /// </summary>
        public class AssessmentResults
        {
            /// <summary>
            /// Gets or sets the dominant gifts.
            /// </summary>
            /// <value>
            /// The dominant gifts.
            /// </value>
            public List<Guid> DominantGifts { get; set; }

            /// <summary>
            /// Gets or sets the supportive gifts.
            /// </summary>
            /// <value>
            /// The supportive gifts.
            /// </value>
            public List<Guid> SupportiveGifts { get; set; }

            /// <summary>
            /// Gets or sets the other gifts.
            /// </summary>
            /// <value>
            /// The other gifts.
            /// </value>
            public List<Guid> OtherGifts { get; set; }

            /// <summary>
            /// The last save date
            /// </summary>
            public DateTime? LastSaveDate;
        }

        /// <summary>
        /// 
        /// </summary>
        public class SpiritualGift
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SpiritualGift"/> class.
            /// </summary>
            /// <param name="mean">The mean.</param>
            /// <param name="standardDeviation">The standard deviation.</param>
            /// <param name="constructs">The constructs.</param>
            public SpiritualGift( double mean, double standardDeviation, List<string> constructs )
            {
                Mean = mean;
                StandardDeviation = standardDeviation;
                Constructs = constructs;
            }

            /// <summary>
            /// Gets or sets the mean.
            /// </summary>
            /// <value>
            /// The mean.
            /// </value>
            public double Mean { get; set; }

            /// <summary>
            /// Gets or sets the standard deviation.
            /// </summary>
            /// <value>
            /// The standard deviation.
            /// </value>
            public double StandardDeviation { get; set; }

            /// <summary>
            /// Gets or sets the constructs.
            /// </summary>
            /// <value>
            /// The constructs.
            /// </value>
            public List<string> Constructs { get; set; }
        }
    }
}