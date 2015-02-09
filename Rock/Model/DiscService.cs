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

namespace Rock.Model
{
    /// <summary>
    /// DISC Class for administering and scoring a DISC Assessment
    /// </summary>
    public class DiscService
    {
        /// <summary>
        /// Raw question data.
        /// </summary>
        private static String[,] questionData = {
            {"Enjoys challenges","Enjoys having fun","Freely expresses feelings","Comfortable with circumstances","NNNN","NNNN"},
            {"Tries to avoid mistakes","Not easily moved","Convinces others ","Goes along with the flow","CDIS","CDIS"},
            {"Liked by others","Systematically thinks through issues","Quickly shares thoughts","Does not like change","ICDS","ICDS"},
            {"Enjoys talking","Calculates actions","Actions are predictable","Makes decisions quickly","ICSD","ICSD"},
            {"Takes risks","Sees imperfections","People person","Avoids extremes","DCIS","DCIS"},
            {"Sensitive to others","Influences others","Struggles acknowledging abilities","Produces results","SICD","SICD"},
            {"Full of energy","Tries to avoid mistakes","Takes control","Accessible to others","ICDS","ICDS"},
            {"High-spirited","Easily focuses","Does not speak up","Fully committed to the goal","ICSD","ICSD"},
            {"Pays attention to details","Consistent with others","Is entertaining","Gives direction","CSID","CSID"},
            {"Conquers challenges","Encourages others","Follows others","Minutely exact","DISC","DISC"},
            {"Plays down abilities","Listens to others","Driven by goals","An optimist","CSDI","CSDI"},
            {"Pushes others forward","Concerned for other","Concerned for correctness","Is self sufficient","ISCD","ISCD"},
            {"Pursues goals","Understands other's feelings","People enjoy them","Avoids danger","DSIC","DSIC"},
            {"Prefers precision","Follows others","Starts things","Enjoys life fully","CSDI","CSDI"},
            {"Liked by others","Thinks things through","Perseveres over obstacles","Does things the traditional way","ICDS","ICDS"},
            {"Is analytical","Takes risks","Loyal to others","Possesses charm ","CDSI","CDSI"},
            {"Enjoys others","Patient listener","Takes care of themselves","Listens to details","ISDC","ISDC"},
            {"Doesn't challenge others","Doggedly pursues","Works systematically","High energy","SDCI","SDCI"},
            {"Overcomes difficulties","Shows feelings freely","Mindful of others","Prone to worry","DISC","DISC"},
            {"Enjoys hanging out with people","Cares deeply about others","Is concerned about the facts","Secure in abilities","ISCD","ISCD"},
            {"Does things the right way","Is generous","Shares inner thoughts","Committed to the goal","CSID","CSID"},
            {"Acts without thinking","Evaluates objectively","Assumes control","Comfortable with the status quo","ICDS","ICDS"},
            {"Spends time with others","Thinks systematically through issues","Displays confidence","Gives grace to others","ICDS","ICDS"},
            {"Seeks others","Comfortable with the status quo","Resistant to opposition","Follows instructions precisely","ISDC","ISDC"},
            {"Pushes others to do their best","Thinks through problems analytically","Collaborative team player","Upbeat about life","DCSI","DCSI"},
            {"Enjoys having fun with others","Conforms exactly to a standard","Expresses opinions freely","Is consistent","ICDS","ICDS"},
            {"Forcefully pursues a goal","Welcoming to others","Considerate of others","Carefully assesses risks","DISC","DISC"},
            {"Critical of others","Pushes others","Optimistic toward others","Concerned with others","CDIS","CDIS"},
            {"Readily follows others","Tries to avoid mistakes","Does not show weakness","Enjoys others","NNNN","NNNN"},
            {"Expresses feelings","Often stands out","Monitors details","Satisfied with circumstances","IDCS","IDCS"}
        };

#pragma warning disable 1591
        /// <summary>
        /// The AssessmentResults struct used to return the final assessment scores
        /// </summary>
        public struct AssessmentResults
        {
            /// <summary>
            /// AdaptiveBehaviorS
            /// </summary>
            public int AdaptiveBehaviorS;
            public int AdaptiveBehaviorC;
            public int AdaptiveBehaviorI;
            public int AdaptiveBehaviorD;
            public int NaturalBehaviorS;
            public int NaturalBehaviorC;
            public int NaturalBehaviorI;
            public int NaturalBehaviorD;
            public string PersonalityType;
            public DateTime LastSaveDate;
        }

        /// <summary>
        /// An individual response to a question. 
        /// <para>Properties: QuestionNumber, ResponseNumber, ResponseID (QuestionNumber + ResponseNumber), ResponseText, MostScore, and LeastScore.</para>
        /// </summary>
        public struct ResponseItem
        {
            public string QuestionNumber;
            public string ResponseNumber;
            public string ResponseID;
            public string ResponseText;
            public string MostScore;
            public string LeastScore;
        }

        /// <summary>
        /// The key names for the DISC person attributes.
        /// </summary>
        public static class AttributeKeys
        {
            public const string AdaptiveD = "AdaptiveD";
            public const string AdaptiveI = "AdaptiveI";
            public const string AdaptiveS = "AdaptiveS";
            public const string AdaptiveC = "AdaptiveC";
            public const string NaturalD = "NaturalD";
            public const string NaturalI = "NaturalI";
            public const string NaturalS = "NaturalS";
            public const string NaturalC = "NaturalC";
            public const string PersonalityType = "PersonalityType";
            public const string LastSaveDate = "LastSaveDate";
        }

#pragma warning restore 1591

        /// <summary>
        /// Returns the datasource.  Each row is a question,
        /// comprised of four responses followed by a 'more' and 'less' score.
        /// </summary>
        /// <returns></returns>
        static public String[,] GetResponsesByQuestion()
        {
            return questionData;
        }

        /// <summary>
        /// Fetch a List of <see cref="ResponseItem"/> for display/processing.
        /// </summary>
        /// <returns>a List of <see cref="ResponseItem"/>.</returns>
        static public List<ResponseItem> GetResponses()
        {
            List<ResponseItem> responseList = new List<ResponseItem>();
            ResponseItem response = new ResponseItem();

            for ( int questionIndex = 0; questionIndex < questionData.GetLength( 0 ); questionIndex++ )
            {
                for ( int responseIndex = 0; responseIndex < 4; responseIndex++ )
                {
                    response.QuestionNumber = ( questionIndex + 1 ).ToString( "D2" );
                    response.ResponseNumber = ( responseIndex + 1 ).ToString();
                    response.ResponseID = response.QuestionNumber + response.ResponseNumber; ;
                    response.ResponseText = questionData[questionIndex, responseIndex];
                    response.MostScore = questionData[questionIndex, 4].Substring( responseIndex, 1 );
                    response.LeastScore = questionData[questionIndex, 5].Substring( responseIndex, 1 );

                    responseList.Add( response );
                }
            }
            return responseList;
        }

        /// <summary>
        /// Scores the test.
        /// </summary>
        /// <param name="moreN">The more n.</param>
        /// <param name="moreD">The more d.</param>
        /// <param name="moreI">The more i.</param>
        /// <param name="moreS">The more s.</param>
        /// <param name="moreC">The more c.</param>
        /// <param name="lessN">The less n.</param>
        /// <param name="lessD">The less d.</param>
        /// <param name="lessI">The less i.</param>
        /// <param name="lessS">The less s.</param>
        /// <param name="lessC">The less c.</param>
        /// <returns>returns a AssessmentResults object</returns>
        static public AssessmentResults Score( int moreN, int moreD, int moreI, int moreS, int moreC, int lessN, int lessD, int lessI, int lessS, int lessC )
        {
            // Holds the most and least totals for each Letter attribute
            Dictionary<string, int[]> results = new Dictionary<string, int[]>();
            results["S"] = new int[] { 0, 0 };
            results["C"] = new int[] { 0, 0 };
            results["I"] = new int[] { 0, 0 };
            results["N"] = new int[] { 0, 0 }; // This is intentionally not used after most/least totalling (foreach loop below). Placebo questions?
            results["D"] = new int[] { 0, 0 };

            results["S"][0] = moreS;
            results["S"][1] = lessS;
            results["C"][0] = moreC;
            results["C"][1] = lessC;
            results["I"][0] = moreI;
            results["I"][1] = lessI;
            results["N"][0] = moreN;
            results["N"][1] = lessN;
            results["D"][0] = moreD;
            results["D"][1] = lessD;

            int nbS = 27 - results["S"][1];
            int nbC = 26 - results["C"][1];
            int nbI = 26 - results["I"][1];
            int nbD = 27 - results["D"][1];

            decimal decX = results["S"][0] + results["C"][0] + results["I"][0] + results["D"][0];
            decimal decY = nbS + nbC + nbI + nbD;

            AssessmentResults testResults = new AssessmentResults();
            if ( decX > 0 && decY > 0 )
            {
                testResults.AdaptiveBehaviorS = Convert.ToInt32( ( results["S"][0] / decX * 100 ) );
                testResults.AdaptiveBehaviorC = Convert.ToInt32( ( results["C"][0] / decX * 100 ) );
                testResults.AdaptiveBehaviorI = Convert.ToInt32( ( results["I"][0] / decX * 100 ) );
                testResults.AdaptiveBehaviorD = Convert.ToInt32( ( results["D"][0] / decX * 100 ) );

                testResults.NaturalBehaviorS = Convert.ToInt32( ( nbS / decY * 100 ) );
                testResults.NaturalBehaviorC = Convert.ToInt32( ( nbC / decY * 100 ) );
                testResults.NaturalBehaviorI = Convert.ToInt32( ( nbI / decY * 100 ) );
                testResults.NaturalBehaviorD = Convert.ToInt32( ( nbD / decY * 100 ) );
                testResults.LastSaveDate = RockDateTime.Now;

                // Determine the Natural personality type
                testResults.PersonalityType = DetermineNaturalPersonalityType( testResults );
            }

            return testResults;
        }

        /// <summary>
        /// Determines the natural personality type. This is the highest score and the next highest 
        /// if the score is over the midline (we're definining midline as over 24).
        /// </summary>
        /// <param name="results">The AssessmentResults</param>
        /// <returns></returns>
        public static string DetermineNaturalPersonalityType( AssessmentResults results )
        {
            var personalityType = string.Empty;
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            dictionary["D"] = results.NaturalBehaviorD;
            dictionary["I"] = results.NaturalBehaviorI;
            dictionary["S"] = results.NaturalBehaviorS;
            dictionary["C"] = results.NaturalBehaviorC;

            List<KeyValuePair<string, int>> list = dictionary.ToList();
            list.Sort( ( x, y ) => y.Value.CompareTo( x.Value ) );
            personalityType = string.Format( "{0}{1}", list[0].Key, ( list[1].Value > 24 ) ? list[1].Key : string.Empty );
            return personalityType;
        }

        /// <summary>
        /// Fetches DISC scores.
        /// </summary>
        /// <param name="person"></param>
        /// <param name="attrib"></param>
        /// <returns>The DISC score, if one is saved. Otherwise, returns 0.</returns>
        private static int AttributeValueLookup( Person person, string attrib )
        {
            int retVal = 0;
            bool bCatch = int.TryParse( person.AttributeValues[attrib].Value, out retVal );
            return retVal;
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

            var discAttributes = person.Attributes.Values.Where( a => a.Categories.Any( c => c.Guid == new Guid( "0B187C81-2106-4875-82B6-FBF1277AE23B" ) ) ).Select( a => a.Key );

            foreach ( string attrib in discAttributes )
            {
                switch ( attrib )
                {
                    case AttributeKeys.AdaptiveD:
                        savedScores.AdaptiveBehaviorD = AttributeValueLookup( person, attrib );
                        break;
                    case AttributeKeys.AdaptiveI:
                        savedScores.AdaptiveBehaviorI = AttributeValueLookup( person, attrib );
                        break;
                    case AttributeKeys.AdaptiveS:
                        savedScores.AdaptiveBehaviorS = AttributeValueLookup( person, attrib );
                        break;
                    case AttributeKeys.AdaptiveC:
                        savedScores.AdaptiveBehaviorC = AttributeValueLookup( person, attrib );
                        break;
                    case AttributeKeys.NaturalD:
                        savedScores.NaturalBehaviorD = AttributeValueLookup( person, attrib );
                        break;
                    case AttributeKeys.NaturalI:
                        savedScores.NaturalBehaviorI = AttributeValueLookup( person, attrib );
                        break;
                    case AttributeKeys.NaturalS:
                        savedScores.NaturalBehaviorS = AttributeValueLookup( person, attrib );
                        break;
                    case AttributeKeys.NaturalC:
                        savedScores.NaturalBehaviorC = AttributeValueLookup( person, attrib );
                        break;
                    case AttributeKeys.PersonalityType:
                        savedScores.PersonalityType = person.AttributeValues[attrib].Value;
                        break;
                    case AttributeKeys.LastSaveDate:
                        DateTime lastAssessmentDate = DateTime.MinValue;
                        bool bCatch = DateTime.TryParse( person.AttributeValues[attrib].Value, out lastAssessmentDate );
                        savedScores.LastSaveDate = lastAssessmentDate;
                        break;
                }
            }
            return savedScores;
        }

        /// <summary>
        /// Saves Assessment results to a Person's PersonProperties
        /// </summary>
        /// <param name="person"></param>
        /// <param name="ABd">Adaptive Behavior D</param>
        /// <param name="ABi">Adaptive Behavior I</param>
        /// <param name="ABs">Adaptive Behavior S</param>
        /// <param name="ABc">Adaptive Behavior C</param>
        /// <param name="NBd">Natural Behavior D</param>
        /// <param name="NBi">Natural Behavior I</param>
        /// <param name="NBs">Natural Behavior S</param>
        /// <param name="NBc">Natural Behavior C</param>
        /// <param name="personalityType">One or two letters of DISC that represents the personality.</param>
        static public void SaveAssessmentResults( Person person, string ABd, string ABi, string ABs, string ABc, string NBd, string NBi, string NBs, string NBc, string personalityType )
        {
            person.LoadAttributes();

            var discAttributes = person.Attributes.Values.Where( a => a.Categories.Any( c => c.Guid == new Guid( "0B187C81-2106-4875-82B6-FBF1277AE23B" ) ) ).Select( a => a.Key );

            foreach ( string attrib in discAttributes )
            {
                switch ( attrib )
                {
                    case AttributeKeys.AdaptiveD:
                        Rock.Attribute.Helper.SaveAttributeValue( person, person.Attributes[attrib], ABd );
                        break;
                    case AttributeKeys.AdaptiveI:
                        Rock.Attribute.Helper.SaveAttributeValue( person, person.Attributes[attrib], ABi );
                        break;
                    case AttributeKeys.AdaptiveS:
                        Rock.Attribute.Helper.SaveAttributeValue( person, person.Attributes[attrib], ABs );
                        break;
                    case AttributeKeys.AdaptiveC:
                        Rock.Attribute.Helper.SaveAttributeValue( person, person.Attributes[attrib], ABc );
                        break;
                    case AttributeKeys.NaturalD:
                        Rock.Attribute.Helper.SaveAttributeValue( person, person.Attributes[attrib], NBd );
                        break;
                    case AttributeKeys.NaturalI:
                        Rock.Attribute.Helper.SaveAttributeValue( person, person.Attributes[attrib], NBi );
                        break;
                    case AttributeKeys.NaturalS:
                        Rock.Attribute.Helper.SaveAttributeValue( person, person.Attributes[attrib], NBs );
                        break;
                    case AttributeKeys.NaturalC:
                        Rock.Attribute.Helper.SaveAttributeValue( person, person.Attributes[attrib], NBc );
                        break;
                    case AttributeKeys.PersonalityType:
                        Rock.Attribute.Helper.SaveAttributeValue( person, person.Attributes[attrib], personalityType );
                        break;
                    case AttributeKeys.LastSaveDate:
                        Rock.Attribute.Helper.SaveAttributeValue( person, person.Attributes[attrib], RockDateTime.Now.ToString( "o" ) );
                        break;
                }
            }

            person.SaveAttributeValues();
        }

        #region DISC shared  UI stuff
        /// <summary>
        /// Plots the one DISC graph.
        /// </summary>
        /// <param name="barD">The D bar.</param>
        /// <param name="barI">The I bar.</param>
        /// <param name="barS">The S bar.</param>
        /// <param name="barC">The C bar.</param>
        /// <param name="scoreD">The D score.</param>
        /// <param name="scoreI">The I score.</param>
        /// <param name="scoreS">The S score.</param>
        /// <param name="scoreC">The C score.</param>
        /// <param name="maxScale">Highest score which is used for the scale of the chart.</param>
        public static void PlotOneGraph( System.Web.UI.HtmlControls.HtmlGenericControl barD, System.Web.UI.HtmlControls.HtmlGenericControl barI, 
            System.Web.UI.HtmlControls.HtmlGenericControl barS, System.Web.UI.HtmlControls.HtmlGenericControl barC,
            int scoreD, int scoreI, int scoreS, int scoreC, int maxScale )
        {
            barD.RemoveCssClass( "discbar-primary" );
            barI.RemoveCssClass( "discbar-primary" );
            barS.RemoveCssClass( "discbar-primary" );
            barC.RemoveCssClass( "discbar-primary" );

            // find the max value
            var maxScore = barD;
            var maxValue = scoreD;
            if ( scoreI > maxValue )
            {
                maxScore = barI;
                maxValue = scoreI;
            }
            if ( scoreS > maxValue )
            {
                maxScore = barS;
                maxValue = scoreS;
            }
            if ( scoreC > maxValue )
            {
                maxScore = barC;
                maxValue = scoreC;
            }
            maxScore.AddCssClass( "discbar-primary" );
            var score = Math.Floor( (double)( (double)scoreD / (double)maxScale ) * 100 ).ToString();
            barD.Style.Add( "height", score + "%" );
            barD.Attributes["title"] = scoreD.ToString();

            score = Math.Floor( (double)( (double)scoreI / (double)maxScale ) * 100 ).ToString();
            barI.Style.Add( "height", score + "%" );
            barI.Attributes["title"] = scoreI.ToString();

            score = Math.Floor( (double)( (double)scoreS / (double)maxScale ) * 100 ).ToString();
            barS.Style.Add( "height", score + "%" );
            barS.Attributes["title"] = scoreS.ToString();

            score = Math.Floor( (double)( (double)scoreC / (double)maxScale ) * 100 ).ToString();
            barC.Style.Add( "height", score + "%" );
            barC.Attributes["title"] = scoreC.ToString();
        }
        #endregion
    }
}