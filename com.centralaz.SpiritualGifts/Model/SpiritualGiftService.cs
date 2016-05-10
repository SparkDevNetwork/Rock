// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
// Test materials (questions, scoring criteria, etc.) used by permission from http://www.gregwiens.com/scid/
// Question data as of Greg Weins' test v1.1 (2014-09-26)
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;

using Rock;
using Rock.Model;
namespace com.centralaz.SpiritualGifts.Model
{

    public class SpiritualGiftService
    {
        /// <summary>
        /// Raw question data.
        /// </summary>
        private static String[,] questionData = {
            {"I have a keen ability to discern the characteristics and motivations of others.","P"},
            {"I detect the immediate needs of others and am quick to do whatever I can to meet them.","S"},
            {"I am able to explain complicated processes or concepts and enjoy doing so.","T"},
            {"I encourage others. I get a positive response that makes me feel good.","E"},
            {"I like to give my time and money to good causes; doing so makes me feel special.","G"},
            {"It's easy for me to identify resources and delegate people, because I can see the big picture.","L"},
            {"I am easily wounded; maybe that's why it's hard to be firm.","M"},
            {"I am very direct, frank and persuasive when speaking.","P"},
            {"I can readily recall the needs, likes and dislikes of others, even casual acquaintances.","S"},
            {"I spend considerable time in research and meditation, and have attained wisdom from my effort.","T"},
            {"People often come to me for encouragement or advice.","E"},
            {"Others consider me to be an optimist.","N"},
            {"When I give to a worthy cause, I want to feel like a part of it and follow up.","G"},
            {"I don't mind criticism because I get the job done.","L"},
            {"I feel deep empathy for the  hurting; I just somehow seem to know.","M"},
            {"Important choices are black and white, right or wrong, without much gray area.","P"},
            {"I like to take charge, delegate tasks and see a project through.","S"},
            {"I enjoy analyzing texts, considering their historical and social context.","T"},
            {"I see most people in the light of their potential and offer words of encouragement.","E"},
            {"I give money and I don't want others to know.","G"},
            {"I gravitate to the leardership position. People naturally look to me.","L"},
            {"I go out of my way to visit the sick or lonely, and feel terribly sorry for them.","M"},
            {"I speak frankly; others incorrectly think me to be  judgmental.","P"},
            {"I consider myself to be optimistic.","N"},
            {"I administrate projects: I enjoy working hard and delegating responsibility.","S"},
            {"I enjoy explaining the details of a text, device or concept.","T"},
            {"I enjoy conferences, study groups, share and care opportunities. I am an encourager!","E"},
            {"I see the needs that go unnoticed by others and personally meet them.","G"},
            {"As project leader, I feel that our goals must be achieved even though feelings may be bruised.","L"},
            {"I overlook faults, even sins, if it means keeping harmony in the group.","M"}                   
        };

#pragma warning disable 1591

        /// <summary>
        /// The TestResults struct used to return the final assessment scores
        /// </summary>
        public class SpiritualGiftTestResults
        {
            public int Prophecy;
            public int Ministry;
            public int Teaching;
            public int Encouragement;
            public int Giving;
            public int Leadership;
            public int Mercy;
            public string Gifting;
            public DateTime LastSaveDate;
        }

        public class QuestionItem
        {
            public int QuestionIndex;
            public string QuestionText;
            public string QuestionGifting;
            public int? QuestionScore;
        }

        public class UiItem
        {
            public string Key { get; set; }
            public HtmlGenericControl Bar { get; set; }
            public int Score { get; set; }
        }

        /// <summary>
        /// The key names for the person's spiritual gifting attributes.
        /// </summary>
        public static class AttributeKeys
        {
            public const string Prophecy = "Prophecy";
            public const string Ministry = "Ministry";
            public const string Teaching = "Teaching";
            public const string Encouragement = "Encouragement";
            public const string Giving = "Giving";
            public const string Leadership = "Leadership";
            public const string Mercy = "Mercy";
            public const string Gifting = "Gifting";
            public const string LastSaveDate = "LastSpiritualSaveDate";
        }

#pragma warning restore 1591
        /// <summary>
        /// Fetch a List of <see cref="QuestionItem"/> for display/processing.
        /// </summary>
        /// <returns>a List of <see cref="QuestionItem"/>.</returns>
        static public List<QuestionItem> GetQuestions()
        {
            List<QuestionItem> questionList = new List<QuestionItem>();
            QuestionItem question = new QuestionItem();

            for ( int questionIndex = 0; questionIndex < questionData.GetLength( 0 ); questionIndex++ )
            {
                question = new QuestionItem();
                question.QuestionIndex = questionIndex;
                question.QuestionText = questionData[questionIndex, 0];
                question.QuestionGifting = questionData[questionIndex, 1];
                question.QuestionScore = null;
                questionList.Add( question );
            }
            return questionList;
        }

        static public SpiritualGiftTestResults Score( int prophecy, int ministry, int teaching, int encouragement, int giving, int leadership, int mercy, int placebo )
        {
            SpiritualGiftTestResults testResults = new SpiritualGiftTestResults();

            testResults.Prophecy = prophecy;
            testResults.Ministry = ministry;
            testResults.Teaching = teaching;
            testResults.Encouragement = encouragement;
            testResults.Giving = giving;
            testResults.Leadership = leadership;
            testResults.Mercy = mercy;
            testResults.LastSaveDate = RockDateTime.Now;

            // Determine the Natural gifting
            testResults.Gifting = DetermineGifting( testResults );

            return testResults;
        }

        /// <summary>
        /// Determines the natural gifting. This is the highest score and the next highest 
        /// if the score is over the midline (we're definining midline as over 24).
        /// </summary>
        /// <param name="results">The TestResults</param>
        /// <returns></returns>
        public static string DetermineGifting( SpiritualGiftTestResults results )
        {
            var gifting = string.Empty;
            List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>();
            list.Add( new KeyValuePair<string, int>( "Prophecy", results.Prophecy ) );
            list.Add( new KeyValuePair<string, int>( "Ministry", results.Ministry ) );
            list.Add( new KeyValuePair<string, int>( "Teaching", results.Teaching ) );
            list.Add( new KeyValuePair<string, int>( "Encouragement", results.Encouragement ) );
            list.Add( new KeyValuePair<string, int>( "Giving", results.Giving ) );
            list.Add( new KeyValuePair<string, int>( "Leadership", results.Leadership ) );
            list.Add( new KeyValuePair<string, int>( "Mercy", results.Mercy ) );

            list.Sort( ( x, y ) => y.Value.CompareTo( x.Value ) );
            return list[0].Key;
        }

        /// <summary>
        /// Fetches DISC scores.
        /// </summary>
        /// <param name="person"></param>
        /// <param name="attrib"></param>
        /// <returns>The DISC score, if one is saved. Otherwise, returns 0.</returns>
        private static int GetSpiritualGiftingValue( Person person, string attrib )
        {
            int giftingLevel = 0;
            bool bCatch = int.TryParse( person.AttributeValues[attrib].Value, out giftingLevel );
            return giftingLevel;
        }

        /// <summary>
        /// Loads and returns saved Test scores for the Person.
        /// </summary>
        /// <param name="person">The Person to get the scores for.</param>
        /// <returns>TestResults</returns>
        static public SpiritualGiftTestResults LoadSavedTestResults( Person person )
        {
            SpiritualGiftTestResults savedScores = new SpiritualGiftTestResults();

            person.LoadAttributes();

            var discAttributes = person.Attributes.Values.Where( a => a.Categories.Any( c => c.Guid == new Guid( "12d8e61f-ed07-41d9-be0b-43c73907896d" ) ) ).Select( a => a.Key );

            foreach ( string attrib in discAttributes )
            {
                switch ( attrib )
                {
                    case AttributeKeys.Prophecy:
                        savedScores.Prophecy = GetSpiritualGiftingValue( person, attrib );
                        break;
                    case AttributeKeys.Ministry:
                        savedScores.Ministry = GetSpiritualGiftingValue( person, attrib );
                        break;
                    case AttributeKeys.Teaching:
                        savedScores.Teaching = GetSpiritualGiftingValue( person, attrib );
                        break;
                    case AttributeKeys.Encouragement:
                        savedScores.Encouragement = GetSpiritualGiftingValue( person, attrib );
                        break;
                    case AttributeKeys.Giving:
                        savedScores.Giving = GetSpiritualGiftingValue( person, attrib );
                        break;
                    case AttributeKeys.Leadership:
                        savedScores.Leadership = GetSpiritualGiftingValue( person, attrib );
                        break;
                    case AttributeKeys.Mercy:
                        savedScores.Mercy = GetSpiritualGiftingValue( person, attrib );
                        break;
                    case AttributeKeys.Gifting:
                        savedScores.Gifting = person.AttributeValues[attrib].Value;
                        break;
                    case AttributeKeys.LastSaveDate:
                        DateTime lastTestDate = DateTime.MinValue;
                        bool bCatch = DateTime.TryParse( person.AttributeValues[attrib].Value, out lastTestDate );
                        savedScores.LastSaveDate = lastTestDate;
                        break;
                }
            }
            return savedScores;
        }

        static public void SaveTestResults( Person person, string prophecy, string ministry, string teaching, string encouragement, string giving, string leadership, string mercy, string gifting )
        {
            person.LoadAttributes();

            var discAttributes = person.Attributes.Values.Where( a => a.Categories.Any( c => c.Guid == new Guid( "12d8e61f-ed07-41d9-be0b-43c73907896d" ) ) ).Select( a => a.Key );

            foreach ( string attrib in discAttributes )
            {
                switch ( attrib )
                {
                    case AttributeKeys.Prophecy:
                        Rock.Attribute.Helper.SaveAttributeValue( person, person.Attributes[attrib], prophecy );
                        break;
                    case AttributeKeys.Ministry:
                        Rock.Attribute.Helper.SaveAttributeValue( person, person.Attributes[attrib], ministry );
                        break;
                    case AttributeKeys.Teaching:
                        Rock.Attribute.Helper.SaveAttributeValue( person, person.Attributes[attrib], teaching );
                        break;
                    case AttributeKeys.Encouragement:
                        Rock.Attribute.Helper.SaveAttributeValue( person, person.Attributes[attrib], encouragement );
                        break;
                    case AttributeKeys.Giving:
                        Rock.Attribute.Helper.SaveAttributeValue( person, person.Attributes[attrib], giving );
                        break;
                    case AttributeKeys.Leadership:
                        Rock.Attribute.Helper.SaveAttributeValue( person, person.Attributes[attrib], leadership );
                        break;
                    case AttributeKeys.Mercy:
                        Rock.Attribute.Helper.SaveAttributeValue( person, person.Attributes[attrib], mercy );
                        break;
                    case AttributeKeys.Gifting:
                        Rock.Attribute.Helper.SaveAttributeValue( person, person.Attributes[attrib], gifting );
                        break;
                    case AttributeKeys.LastSaveDate:
                        Rock.Attribute.Helper.SaveAttributeValue( person, person.Attributes[attrib], RockDateTime.Now.ToString( "o" ) );
                        break;
                }
            }

            person.SaveAttributeValues();
        }

        #region Spiritual Gifts shared  UI stuff

        public static void PlotOneGraph( HtmlGenericControl barProphecy, HtmlGenericControl barMinistry, HtmlGenericControl barTeaching, HtmlGenericControl barEncouragement, HtmlGenericControl barGiving, HtmlGenericControl barLeadership, HtmlGenericControl barMercy,
            int scoreProphecy, int scoreMinistry, int scoreTeaching, int scoreEncouragement, int scoreGiving, int scoreLeadership, int scoreMercy, int maxScale )
        {
            List<UiItem> uiList = new List<UiItem>();
            uiList.Add( new UiItem() { Key = "Prophecy", Bar = barProphecy, Score = scoreProphecy } );
            uiList.Add( new UiItem() { Key = "Ministry", Bar = barMinistry, Score = scoreMinistry } );
            uiList.Add( new UiItem() { Key = "Teaching", Bar = barTeaching, Score = scoreTeaching } );
            uiList.Add( new UiItem() { Key = "Encouragement", Bar = barEncouragement, Score = scoreEncouragement } );
            uiList.Add( new UiItem() { Key = "Giving", Bar = barGiving, Score = scoreGiving } );
            uiList.Add( new UiItem() { Key = "Leadership", Bar = barLeadership, Score = scoreLeadership } );
            uiList.Add( new UiItem() { Key = "Mercy", Bar = barMercy, Score = scoreMercy } );

            foreach ( var ui in uiList )
            {
                ui.Bar.RemoveCssClass( "discbar-primary" );
                var score = Math.Floor( (double)( (double)ui.Score / (double)maxScale ) * 100 ).ToString();
                ui.Bar.Style.Add( "height", score + "%" );
                ui.Bar.Attributes["title"] = ui.Score.ToString();
            }

            uiList = uiList
                .OrderByDescending( ui => ui.Score )
                .ThenBy( ui => ui.Key )
                .ToList();
            var maxScore = uiList.FirstOrDefault().Bar;
            maxScore.AddCssClass( "discbar-primary" );

            for ( int i = 3; i < uiList.Count; i++ )
            {
                uiList.ElementAt( i ).Bar.Visible = false;
            }
        }

        #endregion
    }
}