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
            {"enjoys challenges","enjoys having fun","freely expresses feelings","comfortable with circumstances","NNNN","NNNN"},
            {"tries to avoid mistakes","not easily moved","convinces others ","goes along with the flow","CDIS","CDIS"},
            {"liked by others","systematically thinks through issues","quickly shares thoughts","does not like change","ICDS","ICDS"},
            {"enjoys talking","calculates actions","actions are predictable","makes decisions quickly","ICSD","ICSD"},
            {"takes risks","sees imperfections","people person","avoids extremes","DCIS","DCIS"},
            {"sensitive to others","influences others","struggles acknowledging abilities","produces results","SICD","SICD"},
            {"full of energy","tries to avoid mistakes","takes control","accessible to others","ICDS","ICDS"},
            {"high-spirited","easily focuses","does not speak up","fully committed to the goal","ICSD","ICSD"},
            {"pays attention to details","consistent with others","is entertaining","gives direction","CSID","CSID"},
            {"conquers challenges","encourages others","follows others","minutely exact","DISC","DISC"},
            {"plays down abilities","listens to others","driven by goals","an optimist","CSDI","CSDI"},
            {"pushes others forward","concerned for other","concerned for correctness","is self sufficient","ISCD","ISCD"},
            {"pursues goals","understands other's feelings","people enjoy them","avoids danger","DSIC","DSIC"},
            {"prefers precision","follows others","starts things","enjoys life fully","CSDI","CSDI"},
            {"liked by others","thinks things through","perseveres over obstacles","does things the traditional way","ICDS","ICDS"},
            {"is analytical","takes risks","loyal to others","possesses charm ","CDSI","CDSI"},
            {"enjoys others","patient listener","takes care of themselves","listens to details","ISDC","ISDC"},
            {"doesn't challenge others","doggedly pursues","works systematically","high energy","SDCI","SDCI"},
            {"overcomes difficulties","shows feelings freely","mindful of others","prone to worry","DISC","DISC"},
            {"enjoys hanging out with people","cares deeply about others","is concerned about the facts","secure in abilities","ISCD","ISCD"},
            {"does things the right way","is generous","shares inner thoughts","committed to the goal","CSID","CSID"},
            {"acts without thinking","evaluates objectively","assumes control","comfortable with the status quo","ICDS","ICDS"},
            {"spends time with others","thinks systematically through issues","displays confidence","gives grace to others","ICDS","ICDS"},
            {"seeks others","comfortable with the status quo","resistant to opposition","follows instructions precisely","ISDC","ISDC"},
            {"pushes others to do their best","thinks through problems analytically","collaborative team player","upbeat about life","DCSI","DCSI"},
            {"enjoys having fun with others","conforms exactly to a standard","expresses opinions freely","is consistent","ICDS","ICDS"},
            {"forcefully pursues a goal","welcoming to others","considerate of others","carefully assesses risks","DISC","DISC"},
            {"critical of others","pushes others","optimistic toward others","concerned with others","CDIS","CDIS"},
            {"readily follows others","tries to avoid mistakes","does not show weakness","enjoys others","NNNN","NNNN"},
            {"expresses feelings","often stands out","monitors details","satisfied with circumstances","IDCS","IDCS"}
        };

        //#pragma warning disable 1591
        /// <summary>
        /// The AssessmentResults struct used to return the final assessment scores
        /// </summary>
        public struct AssessmentResults
        {
            /// <summary>
            /// AdaptiveBehaviorS
            /// </summary>
            public int AdaptiveBehaviorS;
            /// <summary>
            /// AdaptiveBehaviorC
            /// </summary>
            public int AdaptiveBehaviorC;
            /// <summary>
            /// AdaptiveBehaviorI
            /// </summary>
            public int AdaptiveBehaviorI;
            /// <summary>
            /// AdaptiveBehaviorD
            /// </summary>
            public int AdaptiveBehaviorD;
            /// <summary>
            /// NaturalBehaviorS
            /// </summary>
            public int NaturalBehaviorS;
            /// <summary>
            /// NaturalBehaviorC
            /// </summary>
            public int NaturalBehaviorC;
            /// <summary>
            /// NaturalBehaviorI
            /// </summary>
            public int NaturalBehaviorI;
            /// <summary>
            /// NaturalBehaviorD
            /// </summary>
            public int NaturalBehaviorD;
            /// <summary>
            /// LastSaveDate
            /// </summary>
            public DateTime LastSaveDate;
        }

        /// <summary>
        /// An individual response to a question. 
        /// <para>Properties: QuestionNumber, ResponseNumber, ResponseID (QuestionNumber + ResponseNumber), ResponseText, MostScore, and LeastScore.</para>
        /// </summary>
        public struct ResponseItem
        {
            /// <summary>
            /// QuestionNumber
            /// </summary>
            public string QuestionNumber;
            /// <summary>
            /// ResponseNumber
            /// </summary>
            public string ResponseNumber;
            /// <summary>
            /// ResponseID
            /// </summary>
            public string ResponseID;
            /// <summary>
            /// ResponseText
            /// </summary>
            public string ResponseText;
            /// <summary>
            /// MostScore
            /// </summary>
            public string MostScore;
            /// <summary>
            /// LeastScore
            /// </summary>
            public string LeastScore;
        }

        public static class AttributeKeys
        {
            /// <summary>
            /// AdaptiveD
            /// </summary>
            public const string AdaptiveD = "AdaptiveD";
            /// <summary>
            /// AdaptiveI
            /// </summary>
            public const string AdaptiveI = "AdaptiveI";
            /// <summary>
            /// AdaptiveS
            /// </summary>
            public const string AdaptiveS = "AdaptiveS";
            /// <summary>
            /// AdaptiveC
            /// </summary>
            public const string AdaptiveC = "AdaptiveC";
            /// <summary>
            /// NaturalD
            /// </summary>
            public const string NaturalD = "NaturalD";
            /// <summary>
            /// NaturalI
            /// </summary>
            public const string NaturalI = "NaturalI";
            /// <summary>
            /// NaturalS
            /// </summary>
            public const string NaturalS = "NaturalS";
            /// <summary>
            /// NaturalC
            /// </summary>
            public const string NaturalC = "NaturalC";
            /// <summary>
            /// LastSaveDate
            /// </summary>
            public const string LastSaveDate = "LastSaveDate";
        }

        //#pragma warning restore 1591

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
            }
            return testResults;
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

            var discAttributes = person.Attributes.Values.Where( a => a.Categories.Any( c => c.Name == "DISC" ) ).Select( a => a.Key );

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
        /// <param name="person">The person taking the test</param>
        /// <param name="ABd">Adaptive Behavior D</param>
        /// <param name="ABi">Adaptive Behavior I</param>
        /// <param name="ABs">Adaptive Behavior S</param>
        /// <param name="ABc">Adaptive Behavior C</param>
        /// <param name="NBd">Natural Behavior D</param>
        /// <param name="NBi">Natural Behavior I</param>
        /// <param name="NBs">Natural Behavior S</param>
        /// <param name="NBc">Natural Behavior C</param>
        static public void SaveAssessmentResults( Person person, String ABd, String ABi, String ABs, String ABc, String NBd, String NBi, String NBs, String NBc )
        {
            person.LoadAttributes();

            var discAttributes = person.Attributes.Values.Where( a => a.Categories.Any( c => c.Name == "DISC" ) ).Select( a => a.Key );

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
                    case AttributeKeys.LastSaveDate:
                        Rock.Attribute.Helper.SaveAttributeValue( person, person.Attributes[attrib], RockDateTime.Now.ToString() );
                        break;
                }
            }

            person.SaveAttributeValues();
        }
    }
}