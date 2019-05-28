using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Model
{
    /// <summary>
    /// EQ Inventory Class for administering and scoring a EQ Inventory test
    /// </summary>
    public class EQInventoryService
    {
        private const string ATTRIBUTE_EQ_CONSTRUCTS_SELF_AWARE = "core_EQSelfAware";

        private const string ATTRIBUTE_EQ_CONSTRUCTS_SELF_REGULATE = "core_EQSelfRegulate";

        private const string ATTRIBUTE_EQ_CONSTRUCTS_OTHERS_AWARE = "core_EQOthersAware";

        private const string ATTRIBUTE_EQ_CONSTRUCTS_OTHERS_REGULATE = "core_EQOthersRegulate";

        private const string ATTRIBUTE_EQ_SCALES_PROBLEM_SOLVING = "core_EQProblemSolving";

        private const string ATTRIBUTE_EQ_SCALES_UNDER_STRESS = "core_EQUnderStress";

        /// <summary>
        /// Raw question data with code as key.
        /// </summary>
        private static Dictionary<string, string> questionData = new Dictionary<string, string>(){
            { "fsa02N","I'm surprised by the emotions I feel."},
            { "fsa03","When I get upset, I know what is bothering me."},
            { "fsa04N","I struggle to know what I'm really feeling."},
            { "fsa05","I understand why I feel the way I do."},
            { "fsa06","I understand my emotions in the midst of conflict."},
            { "fsa07","I know why something bothers me."},
            { "fsa08","I describe my emotions accurately."},
            { "fsa09","When I'm moody, I know why."},
            { "fsa10N","I'm not able to identify my emotions at any given moment."},
            { "fsr01","I express my emotions in such a manner that others are comfortable."},
            { "fsr02","I express my emotions appropriately even when others struggle to do so."},
            { "fsr03N","When I share my feelings, I feel awkward."},
            { "fsr04","I listen to criticism without becoming defensive."},
            { "fsr06N","I am defensive when others criticize me."},
            { "fsr07","I show my emotions appropriately in how I respond to others."},
            { "fsr08","I set appropriate boundaries with others."},
            { "fsr09N","I struggle to express my emotions with others when I am upset."},
            { "fsr10N","I resist sharing my emotions because it doesn't make a difference."},
            { "foa02","I know what emotions others are experiencing."},
            { "foa03","I understand why someone responds emotionally."},
            { "foa04","When I disagree with someone, I understand the emotions they are feeling."},
            { "foa05","I can tell when another person is feeling down."},
            { "foa06","I read the body language of others easily."},
            { "foa07","I know when a friend is struggling emotionally."},
            { "foa08N","I'm confused with what another person is feeling."},
            { "foa09","I sense what a person is feeling before they tell me."},
            { "foa10","I connect with people because I understand their emotions."},
            { "for02","I enjoy helping others better understand what they are feeling."},
            { "for03","Others come and freely share their hurt with me."},
            { "for04","I encourage people when others may not even know they need it."},
            { "for05","I try to help others express their emotions."},
            { "for06","Others quickly share their feelings with me without probing."},
            { "for07","I help others process their emotions."},
            { "for08","Others feel safe sharing their feelings with me."},
            { "for09","Others connect with me because they know I understand their feelings."},
            { "for10","I respond to other's emotional reactions with helping them better understand what they are feeling."},
            { "fps01N","Problems can be solved using straight forward logic."},
            { "fps02","Problems seldom have a simple logical solution."},
            { "fps03N","I can solve difficulties with facts."},
            { "fps04N","When challenged, I respond with logical reasons for my position."},
            { "fps05N","Most challenges just need someone with a good command of the facts to make a decision."},
            { "fps07N","I solve problems without using emotions."},
            { "fps08N","Only facts are relevant to solving problems."},
            { "fes01","I understand my own emotions in stressful situations."},
            { "fes02","I understand the emotions of those with whom I disagree."},
            { "fes03N","I have trouble understanding my emotions when I am under stress."},
            { "fes04","In disputes with others I have a deep understanding of how they are feeling."},
            { "fes05","I identify what I am feeling in the midst of a dispute."},
            { "fes06N","In the midst of a disagreement with someone I am not able to identify what they are feeling."},
            { "fes07","I express my feelings in a dispute in a way that everyone appreciates."},
            { "fes08","When others are panicking, I am able to understand my emotions to make good decisions."}
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
        private static Dictionary<Guid, EQInventory> constructData = new Dictionary<Guid, EQInventory>()
        {
            { SystemGuid.Attribute.PERSON_EQ_CONSTRUCTS_SELF_AWARE.AsGuid(),  new EQInventory(34.36,4.209, new List<string>(){ "fsa02N", "fsa03", "fsa04N", "fsa05", "fsa06", "fsa07", "fsa08", "fsa09","fsa10N" }) },
            { SystemGuid.Attribute.PERSON_EQ_CONSTRUCTS_SELF_REGULATE.AsGuid(), new EQInventory(31.43, 4.305,  new List<string>(){ "fsr01", "fsr02", "fsr03N", "fsr04", "fsr06N", "fsr07", "fsr08", "fsr09N","fsr10N"}) },
            { SystemGuid.Attribute.PERSON_EQ_CONSTRUCTS_OTHERS_AWARE.AsGuid(), new EQInventory(34.17,4.160, new List<string>(){ "foa02", "foa03", "foa04", "foa05", "foa06", "foa07", "foa08N", "foa09","foa10"}) },
            { SystemGuid.Attribute.PERSON_EQ_CONSTRUCTS_OTHERS_REGULATE.AsGuid(), new EQInventory( 33.66, 5.352, new List<string>(){ "for02", "for03", "for04", "for05", "for06", "for07", "for08", "for09", "for10" } )},
            { SystemGuid.Attribute.PERSON_EQ_SCALES_IN_PROBLEM_SOLVING.AsGuid(), new EQInventory(20.18,3.59, new List<string>(){ "fps01N", "fps02", "fps03N", "fps04N", "fps05N", "fps07N", "fps08N" } )},
            { SystemGuid.Attribute.PERSON_EQ_SCALES_UNDER_STRESS.AsGuid(), new EQInventory(29.07,3.478, new List<string>(){ "fes01", "fes02", "fes03N", "fes04", "fes05", "fes06N", "fes07", "fes08" } )},
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
            var zScores = new Dictionary<Guid, double>();
            var assessmentDatas = new Dictionary<Guid, Dictionary<string, string>>();
            var grandTotal = assessmentResponse.Sum( a => Convert.ToDouble( a.Value ) );
            foreach ( var conflictProfileDefinedValue in constructData.Keys )
            {
                var conflictProfile = constructData[conflictProfileDefinedValue];
                double totalResponse = 0;
                var assessmentData = new Dictionary<string, string>();
                foreach ( var construct in conflictProfile.Constructs )
                {
                    if ( assessmentResponse.ContainsKey( construct ) )
                    {
                        totalResponse += assessmentResponse[construct];
                        assessmentData.AddOrReplace( construct, assessmentResponse[construct].ToString() );
                    }
                }

                var zScore = Math.Round( ( totalResponse - conflictProfile.Mean ) / conflictProfile.StandardDeviation, 1 );
                zScores.AddOrReplace( conflictProfileDefinedValue, zScore );
                assessmentDatas.AddOrReplace( conflictProfileDefinedValue, assessmentData );
            }

            testResults.AssessmentData = new AssessmentData();
            testResults.AssessmentData.SelfAware = assessmentDatas[SystemGuid.Attribute.PERSON_EQ_CONSTRUCTS_SELF_AWARE.AsGuid()];
            testResults.AssessmentData.SelfRegulate = assessmentDatas[SystemGuid.Attribute.PERSON_EQ_CONSTRUCTS_SELF_REGULATE.AsGuid()];
            testResults.AssessmentData.OthersAware = assessmentDatas[SystemGuid.Attribute.PERSON_EQ_CONSTRUCTS_OTHERS_AWARE.AsGuid()];
            testResults.AssessmentData.OthersRegulate = assessmentDatas[SystemGuid.Attribute.PERSON_EQ_CONSTRUCTS_OTHERS_REGULATE.AsGuid()];
            testResults.AssessmentData.EQ_InProblemSolving = assessmentDatas[SystemGuid.Attribute.PERSON_EQ_SCALES_IN_PROBLEM_SOLVING.AsGuid()];
            testResults.AssessmentData.EQ_UnderStress = assessmentDatas[SystemGuid.Attribute.PERSON_EQ_SCALES_UNDER_STRESS.AsGuid()];

            testResults.SelfAwareConstruct = GetPercentFromScore( zScores[SystemGuid.Attribute.PERSON_EQ_CONSTRUCTS_SELF_AWARE.AsGuid()] );
            testResults.SelfRegulatingConstruct = GetPercentFromScore( zScores[SystemGuid.Attribute.PERSON_EQ_CONSTRUCTS_SELF_REGULATE.AsGuid()] );
            testResults.OtherAwarenessContruct = GetPercentFromScore( zScores[SystemGuid.Attribute.PERSON_EQ_CONSTRUCTS_OTHERS_AWARE.AsGuid()] );
            testResults.OthersRegulatingConstruct = GetPercentFromScore( zScores[SystemGuid.Attribute.PERSON_EQ_CONSTRUCTS_OTHERS_REGULATE.AsGuid()] );
            testResults.EQ_ProblemSolvingScale = GetPercentFromScore( zScores[SystemGuid.Attribute.PERSON_EQ_SCALES_IN_PROBLEM_SOLVING.AsGuid()] );
            testResults.EQ_UnderStressScale = GetPercentFromScore( zScores[SystemGuid.Attribute.PERSON_EQ_SCALES_UNDER_STRESS.AsGuid()] );
            return testResults;
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
        /// Saves Assessment results to a Person's PersonProperties
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="assessmentResults">The assessment results.</param>
        static public void SaveAssessmentResults( Person person, AssessmentResults assessmentResults )
        {
            person.LoadAttributes();

            person.SetAttributeValue( ATTRIBUTE_EQ_CONSTRUCTS_SELF_AWARE, assessmentResults.SelfAwareConstruct );
            person.SetAttributeValue( ATTRIBUTE_EQ_CONSTRUCTS_SELF_REGULATE, assessmentResults.SelfRegulatingConstruct );
            person.SetAttributeValue( ATTRIBUTE_EQ_CONSTRUCTS_OTHERS_AWARE, assessmentResults.OtherAwarenessContruct );
            person.SetAttributeValue( ATTRIBUTE_EQ_CONSTRUCTS_OTHERS_REGULATE, assessmentResults.OthersRegulatingConstruct );
            person.SetAttributeValue( ATTRIBUTE_EQ_SCALES_PROBLEM_SOLVING, assessmentResults.EQ_ProblemSolvingScale );
            person.SetAttributeValue( ATTRIBUTE_EQ_SCALES_UNDER_STRESS, assessmentResults.EQ_UnderStressScale );
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
            savedScores.SelfAwareConstruct = person.GetAttributeValue( ATTRIBUTE_EQ_CONSTRUCTS_SELF_AWARE ).AsDecimal();
            savedScores.SelfRegulatingConstruct = person.GetAttributeValue( ATTRIBUTE_EQ_CONSTRUCTS_SELF_REGULATE ).AsDecimal();
            savedScores.OtherAwarenessContruct = person.GetAttributeValue( ATTRIBUTE_EQ_CONSTRUCTS_OTHERS_AWARE ).AsDecimal();
            savedScores.OthersRegulatingConstruct = person.GetAttributeValue( ATTRIBUTE_EQ_CONSTRUCTS_OTHERS_REGULATE ).AsDecimal();
            savedScores.EQ_ProblemSolvingScale = person.GetAttributeValue( ATTRIBUTE_EQ_SCALES_PROBLEM_SOLVING ).AsDecimal();
            savedScores.EQ_UnderStressScale = person.GetAttributeValue( ATTRIBUTE_EQ_SCALES_UNDER_STRESS ).AsDecimal();

            return savedScores;
        }

        /// <summary>
        /// The AssessmentResults struct used to return the final assessment scores
        /// </summary>
        public class AssessmentResults
        {
            /// <summary>
            /// Gets or sets the conflict mode winning score.
            /// </summary>
            /// <value>
            /// The conflict mode winning score.
            /// </value>
            public decimal SelfAwareConstruct { get; set; }

            /// <summary>
            /// Gets or sets the conflict mode resolving score.
            /// </summary>
            /// <value>
            /// The resolving score.
            /// </value>
            public decimal SelfRegulatingConstruct { get; set; }

            /// <summary>
            /// Gets or sets the conflict mode compromising score.
            /// </summary>
            /// <value>
            /// The compromising score.
            /// </value>
            public decimal OtherAwarenessContruct { get; set; }

            /// <summary>
            /// Gets or sets the conflict mode avoiding score.
            /// </summary>
            /// <value>
            /// The avoiding score.
            /// </value>
            public decimal OthersRegulatingConstruct { get; set; }

            /// <summary>
            /// Gets or sets the conflict mode yielding score.
            /// </summary>
            /// <value>
            /// The yielding score.
            /// </value>
            public decimal EQ_ProblemSolvingScale { get; set; }

            /// <summary>
            /// Gets or sets the conflict engagement "solving" (aka engaged) score.
            /// </summary>
            /// <value>
            /// The conflict engagement "solving" score.
            /// </value>
            public decimal EQ_UnderStressScale { get; set; }

            /// <summary>
            /// Gets or sets the assessment data.
            /// </summary>
            /// <value>
            /// The assessment data.
            /// </value>
            public AssessmentData AssessmentData { get; set; }
        }

        public class AssessmentData
        {
            /// <summary>
            /// Gets or sets the self aware data.
            /// </summary>
            /// <value>
            /// The self aware data.
            /// </value>
            public Dictionary<string, string> SelfAware { get; set; }

            /// <summary>
            /// Gets or sets the others regulate data.
            /// </summary>
            /// <value>
            /// The  others regulate data.
            /// </value>
            public Dictionary<string, string> OthersRegulate { get; set; }

            /// <summary>
            /// Gets or sets the others aware data.
            /// </summary>
            /// <value>
            /// The others aware data.
            /// </value>
            public Dictionary<string, string> OthersAware { get; set; }

            /// <summary>
            /// Gets or sets the self regulate data.
            /// </summary>
            /// <value>
            /// The self regulate data.
            /// </value>
            public Dictionary<string, string> SelfRegulate { get; set; }

            /// <summary>
            /// Gets or sets the problem solving data.
            /// </summary>
            /// <value>
            /// The problem solving data.
            /// </value>
            public Dictionary<string, string> EQ_InProblemSolving { get; set; }

            /// <summary>
            /// Gets or sets the under stress data.
            /// </summary>
            /// <value>
            /// The yielding data.
            /// </value>
            public Dictionary<string, string> EQ_UnderStress { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class EQInventory
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="EQInventory"/> class.
            /// </summary>
            /// <param name="mean">The mean.</param>
            /// <param name="standardDeviation">The standard deviation.</param>
            /// <param name="constructs">The constructs.</param>
            public EQInventory( double mean, double standardDeviation, List<string> constructs )
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