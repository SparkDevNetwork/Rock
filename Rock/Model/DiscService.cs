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
    /// DISC Class for administering and scoring a DISC Assessment
    /// </summary>
    public class DiscService
    {
        /// <summary>
        /// Raw question data.
        /// </summary>
        private static String[,] questionData = {
            {"Follows rules","Enjoys challenges","Enjoys having fun","Uncomfortable with change","CDIS","CDIS"},
            {"Tries to avoid mistakes","Not easily moved","Convinces others ","Goes with the flow","CDIS","CDIS"},
            {"Thinks through issues","Quickly shares thoughts","Liked by others","Prefers not to change","CDIS","CDIS"},
            {"Calculates actions","Makes decisions quickly","Enjoys talking","Actions are predictable","CDIS","CDIS"},
            {"Sees imperfections","Takes risks","People person","Avoids extremes","CDIS","CDIS"},
            {"Struggles acknowledging abilities","Produces results","Influences others","Sensitive to others","CDIS","CDIS"},
            {"Avoids mistakes","Takes control","Full of energy","Available to others","CDIS","CDIS"},
            {"Easily focuses","Fully committed to the goal","High-spirited","Does not speak up","CDIS","CDIS"},
            {"Pays attention to details","Gives direction","Is entertaining","Consistent with others","CDIS","CDIS"},
            {"Very exact","Conquers challenges","Encourages others","Follows others","CDIS","CDIS"},
            {"Plays down abilities","Driven by goals","Is an optimist","Concerned for others","CDIS","CDIS"},
            {"Concerned for correctness","Is self sufficient","Persuades others","Concerned for other","CDIS","CDIS"},
            {"Avoids danger","Pursues goals","Are enjoyed by People","Empathizes with others","CDIS","CDIS"},
            {"Prefers precision","Starts things","Enjoys life fully","Follows others","CDIS","CDIS"},
            {"Thinks things through","Perseveres obstacles","Liked by others","Sticks with the traditional way","CDIS","CDIS"},
            {"Is analytical","Takes risks","Possesses charm ","Loyal to others","CDIS","CDIS"},
            {"Share details","Unmovable in opposition","Enjoys others","Patient listener","CDIS","CDIS"},
            {"Works systematically","Pursues doggedly","Full of energy","Doesn't challenge others","CDIS","CDIS"},
            {"Prone to worrying","Overcomes difficulties","Freely shows feelings","Mindful of others","CDIS","CDIS"},
            {"Is concerned about the facts","Secure in abilities","Enjoys people","Cares deeply about others","CDIS","CDIS"},
            {"Does things the right way","Committed to the goal","Shares inner thoughts","Is generous","CDIS","CDIS"},
            {"Evaluates objectively","Assumes control","Acts without thinking","Like things the way they are","CDIS","CDIS"},
            {"Systematically thinks through issues","Displays confidence","Spends time with others","Gives grace to others","CDIS","CDIS"},
            {"Follows instructions precisely","Resistant to opposition","Seeks out others","Comfortable with the status quo","CDIS","CDIS"},
            {"Thinks through problems","Pushes others","Upbeat about life","Collaborates with other","CDIS","CDIS"},
            {"Conforms exactly to a standard","Freely expresses opinions","Enjoys having fun with others","Is consistent","CDIS","CDIS"},
            {"Carefully assesses risks","Forcefully pursues goals","Welcomes others","Considerate of others","CDIS","CDIS"},
            {"Critical of others","Drives to complete the goal","Optimistic toward others","Concerned for others","CDIS","CDIS"},
            {"Tries to avoid mistakes","Does not show weakness","Enjoys others","Readily follows others","CDIS","CDIS"},
            {"Follows rules","Stands out","Expresses feelings","Satisfied with circumstances","CDIS","CDIS"}
        };


        /// <summary>
        /// Raw question data with code as key.
        /// </summary>
        private static Dictionary<string, DiscConstant> constructData = new Dictionary<string, DiscConstant>()
        {
            { AttributeKeys.AdaptiveD,  new DiscConstant(11.65673864,4.418053037) },
            { AttributeKeys.AdaptiveI,  new DiscConstant(13.16455696,5.194907006) },
            { AttributeKeys.AdaptiveS,  new DiscConstant(11.93373045,3.956836979) },
            { AttributeKeys.AdaptiveC,  new DiscConstant(13.24497394,5.183690143) },
            { AttributeKeys.NaturalD,  new DiscConstant(11.76396128,4.344015851) },
            { AttributeKeys.NaturalI,  new DiscConstant(13.3655994,4.156180707) },
            { AttributeKeys.NaturalS,  new DiscConstant(12.38644825,3.79901483) },
            { AttributeKeys.NaturalC,  new DiscConstant(12.48399106,4.431075553) }
        };

        /// <summary>
        /// Z score to percentage
        /// </summary>
        private static Dictionary<double, double> zScoreToPercentage = new Dictionary<double, double>()
        {
            { 3,99.9},
            { 2.9,99.8},
            { 2.8,99.7},
            { 2.7,99.6},
            { 2.6,99.5},
            { 2.5,99.4},
            { 2.4,99.2},
            { 2.3,98.9},
            {2.2,98.6},
            {2.1,98.2},
            {2,97.8},
            {1.9,97.1},
            {1.8,96.4},
            {1.7,95.5},
            {1.6,94.5},
            {1.5,93.3},
            {1.4,91.9},
            {1.3,90.3},
            {1.2,88.5},
            {1.1,86.4},
            {1,84.1},
            {0.9,78.8},
            {0.8,78.8},
            {0.7,75.8},
            {0.6,72.6},
            {0.5,69.2},
            {0.4,65.5},
            {0.3,61.8},
            {0.2,57.9},
            {0.1,54},
            {0,50},
            {-0.1,46},
            {-0.2,42.1},
            {-0.3,38.2},
            {-0.4,34.5},
            {-0.5,30.9},
            {-0.6,27.4},
            {-0.7,24.2},
            {-0.8,21.2},
            {-0.9,18.4},
            {-1,15.9},
            {-1.1,13.6},
            {-1.2,11.5},
            {-1.3,9.7},
            {-1.4,8.1},
            {-1.5,6.7},
            {-1.6,5.5},
            {-1.7,4.5},
            {-1.8,3.6},
            {-1.9,2.9},
            {-2,2.3},
            {-2.1,1.8},
            {-2.2,1.4},
            {-2.3,1.1},
            {-2.4,0.8},
            {-2.5,0.6},
            {-2.6,0.5},
            {-2.7,0.4},
            {-2.8,0.3},
            {-2.9,0.2},
            {-3,0.1}
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
                    response.QuestionNumber = ( questionIndex + 1 ).ToString();
                    response.ResponseNumber = ( responseIndex + 1 ).ToString();
                    response.ResponseID = response.QuestionNumber + response.ResponseNumber;
                    ;
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
        /// <param name="moreD">The more d.</param>
        /// <param name="moreI">The more i.</param>
        /// <param name="moreS">The more s.</param>
        /// <param name="moreC">The more c.</param>
        /// <param name="lessD">The less d.</param>
        /// <param name="lessI">The less i.</param>
        /// <param name="lessS">The less s.</param>
        /// <param name="lessC">The less c.</param>
        /// <returns>
        /// returns a AssessmentResults object
        /// </returns>
        static public AssessmentResults Score( int moreD, int moreI, int moreS, int moreC, int lessD, int lessI, int lessS, int lessC )
        {
            AssessmentResults testResults = new AssessmentResults();
            testResults.AdaptiveBehaviorS = GetAdaptiveScoreValue( AttributeKeys.AdaptiveS, moreS );
            testResults.AdaptiveBehaviorC = GetAdaptiveScoreValue( AttributeKeys.AdaptiveC, moreC );
            testResults.AdaptiveBehaviorI = GetAdaptiveScoreValue( AttributeKeys.AdaptiveI, moreI );
            testResults.AdaptiveBehaviorD = GetAdaptiveScoreValue( AttributeKeys.AdaptiveD, moreD );

            testResults.NaturalBehaviorS = GetNaturalScoreValue( AttributeKeys.NaturalS, lessS );
            testResults.NaturalBehaviorC = GetNaturalScoreValue( AttributeKeys.NaturalC, lessC );
            testResults.NaturalBehaviorI = GetNaturalScoreValue( AttributeKeys.NaturalI, lessI );
            testResults.NaturalBehaviorD = GetNaturalScoreValue( AttributeKeys.NaturalD, lessD );
            testResults.LastSaveDate = RockDateTime.Now;

            // Determine the Natural personality type
            testResults.PersonalityType = DetermineNaturalPersonalityType( testResults );

            return testResults;
        }

        /// <summary>
        /// Evaluate the adaptive score for the given Key and count.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="count">The count.</param>
        /// <returns>returns a adaptive score value</returns>
        public static int GetAdaptiveScoreValue( string key, int count )
        {
            var discConst = constructData[key];
            var scoreValue = Math.Round( ( count + 5 - discConst.Mean ) / constructData[key].StandardDeviation, 1 );
            return Convert.ToInt32( GetPercentFromScore( scoreValue ) );
        }

        private static decimal GetPercentFromScore( double scoreValue )
        {
            decimal percent;
            if ( scoreValue > 3.0 )
            {
                percent = 100;
            }
            else if ( scoreValue < -3.0 )
            {
                percent = 0;
            }
            else
            {
                percent = Convert.ToDecimal( zScoreToPercentage[scoreValue] );
            }
            return percent;
        }

        /// <summary>
        /// Evaluate the natural score for the given Key and count.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="count">The count.</param>
        /// <returns>returns a natural score value</returns>
        public static int GetNaturalScoreValue( string key, int count )
        {
            int nb = 30 - count;
            var discConst = constructData[key];
            var scoreValue = Math.Round( ( nb - 10 - discConst.Mean ) / constructData[key].StandardDeviation, 1 );
            return Convert.ToInt32( GetPercentFromScore( scoreValue ) );
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
            personalityType = string.Format( "{0}{1}", list[0].Key, ( list[1].Value > 50 ) ? list[1].Key : string.Empty );
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
            var score = Math.Floor( ( double ) ( ( double ) scoreD / ( double ) maxScale ) * 100 ).ToString();
            barD.Style.Add( "height", score + "%" );
            barD.Attributes["title"] = scoreD.ToString();

            score = Math.Floor( ( double ) ( ( double ) scoreI / ( double ) maxScale ) * 100 ).ToString();
            barI.Style.Add( "height", score + "%" );
            barI.Attributes["title"] = scoreI.ToString();

            score = Math.Floor( ( double ) ( ( double ) scoreS / ( double ) maxScale ) * 100 ).ToString();
            barS.Style.Add( "height", score + "%" );
            barS.Attributes["title"] = scoreS.ToString();

            score = Math.Floor( ( double ) ( ( double ) scoreC / ( double ) maxScale ) * 100 ).ToString();
            barC.Style.Add( "height", score + "%" );
            barC.Attributes["title"] = scoreC.ToString();
        }
        #endregion


        /// <summary>
        /// 
        /// </summary>
        public class DiscConstant
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DiscConstant"/> class.
            /// </summary>
            /// <param name="mean">The mean.</param>
            /// <param name="standardDeviation">The standard deviation.</param>
            public DiscConstant( double mean, double standardDeviation )
            {
                Mean = mean;
                StandardDeviation = standardDeviation;
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
        }
    }
}