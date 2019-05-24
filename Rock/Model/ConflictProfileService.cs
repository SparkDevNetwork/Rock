using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Model
{
    /// <summary>
    /// Conflict Profile Class for administering and scoring a Conflict Profile test
    /// </summary>
    public class ConflictProfileService
    {
        private const string ATTRIBUTE_MODE_WINNING = "core_ConflictModeWinning";

        private const string ATTRIBUTE_MODE_RESOLVING = "core_ConflictModeResolving";

        private const string ATTRIBUTE_MODE_COMPROMISING = "core_ConflictModeCompromising";

        private const string ATTRIBUTE_MODE_AVOIDING = "core_ConflictModeAvoiding";

        private const string ATTRIBUTE_MODE_YEILDING = "core_ConflictModeYielding";

        private const string ATTRIBUTE_THEME_ACCOMMODATING = "core_ConflictThemeAccommodating";

        private const string ATTRIBUTE_THEME_WINNING = "core_ConflictThemeWinning";

        private const string ATTRIBUTE_THEME_SOLVING = "core_ConflictThemeSolving";

        /// <summary>
        /// Raw question data with code as key.
        /// </summary>
        private static Dictionary<string, string> questionData = new Dictionary<string, string>(){
            { "a01","I do what is necessary to avoid tension."},
            { "a03","I postpone the issue until I have had some time to think it over."},
            { "a04","I don't take a position that would create controversy."},
            { "a06","I don't like conflict."},
            { "a07","I avoid creating unpleasantness for myself."},
            { "a08","I will do whatever I can to sidestep the issue causing the problem."},
            { "a09","I prevent the conflict from escalating by ignoring it."},
            { "a10","I can circumvent potential conflict by steering clear of the issue."},
            { "c01","I find a compromise solution."},
            { "c02","I give up some points in exchange for others."},
            { "c04","I find a middle ground."},
            { "c05","I look for a fair combination of wins and losses for both of us."},
            { "c08","I look for a solution in the middle."},
            { "c09","I meet the person halfway."},
            { "c10","I see how we both can give and take."},
            { "c11","I bargain for a solution which all can agree on."},
            { "r02","I attempt to immediately work through our differences."},
            { "r03","I tell the other person my ideas and ask for theirs."},
            { "r05","I consistently seek the other person's help in working out a solution."},
            { "r06","I attempt to get all concerns and issues immediately out in the open."},
            { "r07","I strive for a solution we can all agree is best."},
            { "r08","I do whatever it takes for everyone to express their position."},
            { "r09","I work very hard to ensure everyone voices their concerns."},
            { "r10","I know the best decision depends on everyone's input."},
            { "w01","I seek to prevail because I believe in my position."},
            { "w03","I convince the other person of the benefits of my position."},
            { "w04","I am clear to let others know of my desires."},
            { "w05","I press to get my points made."},
            { "w06","I convince the other person of the merits of my position."},
            { "w07","I am firm in pursuing my goals."},
            { "w09","I am able to justify my perspective."},
            { "w10","I use logic to sway others to agree with me."},
            { "y01","I defer to the needs of the other person."},
            { "y02","I often give in to keep peace."},
            { "y03","I smooth others' feelings and preserve our relationship."},
            { "y05","I make other people happy by letting them keep their position."},
            { "y06","I accommodate the other person's wishes."},
            { "y07","I meet others' desires if they are important to them."},
            { "y08","I give in to others' pressure."},
            { "y11","I find myself not getting what I would like."}
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
        private static Dictionary<Guid, ConflictProfile> constructData = new Dictionary<Guid, ConflictProfile>()
        {
            { SystemGuid.DefinedValue.CONFLICT_PROFILE_AVOIDING.AsGuid(),  new ConflictProfile(21.459,2.90, new List<string>(){ "a01", "a03", "a04", "a06", "a07","a08","a09","a10" }) },
            { SystemGuid.DefinedValue.CONFLICT_PROFILE_COMPROMISING.AsGuid(), new ConflictProfile(22.193, 2.40,  new List<string>(){ "c01", "c02", "c04", "c05","c08","c09","c10","c11"}) },
            { SystemGuid.DefinedValue.CONFLICT_PROFILE_RESOLVING.AsGuid(), new ConflictProfile(21.061,1.59, new List<string>(){ "r02", "r03", "r05", "r06", "r07","r08","r09","r10"}) },
            { SystemGuid.DefinedValue.CONFLICT_PROFILE_WINNING.AsGuid(), new ConflictProfile( 17.009, 2.69, new List<string>(){ "w01", "w03", "w04", "w06", "w07", "w09", "w10" } )},
            { SystemGuid.DefinedValue.CONFLICT_PROFILE_YEILDING.AsGuid(), new ConflictProfile(18.276,2.22, new List<string>(){ "y01", "y02", "y03", "y05", "y06","y07","y08","y11" } )}
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

                var relativeProportion = ( totalResponse / grandTotal ) * 100;
                var zScore = Math.Round( ( relativeProportion - conflictProfile.Mean ) / conflictProfile.StandardDeviation, 1 );
                zScores.AddOrReplace( conflictProfileDefinedValue, zScore );
                assessmentDatas.AddOrReplace( conflictProfileDefinedValue, assessmentData );
            }

            testResults.AssessmentData = new AssessmentData();
            testResults.AssessmentData.Winning = assessmentDatas[SystemGuid.DefinedValue.CONFLICT_PROFILE_WINNING.AsGuid()];
            testResults.AssessmentData.Resolving = assessmentDatas[SystemGuid.DefinedValue.CONFLICT_PROFILE_RESOLVING.AsGuid()];
            testResults.AssessmentData.Compromising = assessmentDatas[SystemGuid.DefinedValue.CONFLICT_PROFILE_COMPROMISING.AsGuid()];
            testResults.AssessmentData.Avoiding = assessmentDatas[SystemGuid.DefinedValue.CONFLICT_PROFILE_AVOIDING.AsGuid()];
            testResults.AssessmentData.Yielding = assessmentDatas[SystemGuid.DefinedValue.CONFLICT_PROFILE_YEILDING.AsGuid()];

            testResults.ModeWinningScore = GetPercentFromScore( zScores[SystemGuid.DefinedValue.CONFLICT_PROFILE_WINNING.AsGuid()] );
            testResults.ModeResolvingScore = GetPercentFromScore( zScores[SystemGuid.DefinedValue.CONFLICT_PROFILE_RESOLVING.AsGuid()] );
            testResults.ModeCompromisingScore = GetPercentFromScore( zScores[SystemGuid.DefinedValue.CONFLICT_PROFILE_COMPROMISING.AsGuid()] );
            testResults.ModeAvoidingScore = GetPercentFromScore( zScores[SystemGuid.DefinedValue.CONFLICT_PROFILE_AVOIDING.AsGuid()] );
            testResults.ModeYieldingScore = GetPercentFromScore( zScores[SystemGuid.DefinedValue.CONFLICT_PROFILE_YEILDING.AsGuid()] );

            var totalPerc = testResults.ModeWinningScore + testResults.ModeResolvingScore
                                + testResults.ModeCompromisingScore + testResults.ModeAvoidingScore
                                + testResults.ModeYieldingScore;
            // Compute the optional "Conflict Engagement Profile" scores
            testResults.EngagementSolvingScore = Math.Round( ( testResults.ModeResolvingScore + testResults.ModeCompromisingScore ) * 100 / totalPerc, 1 );
            testResults.EngagementAccommodatingScore = Math.Round( ( testResults.ModeAvoidingScore + testResults.ModeYieldingScore ) * 100 / totalPerc, 1 );
            testResults.EngagementWinningScore = Math.Round( testResults.ModeWinningScore * 100 / totalPerc, 1 );
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

            person.SetAttributeValue( ATTRIBUTE_MODE_WINNING, assessmentResults.ModeWinningScore );
            person.SetAttributeValue( ATTRIBUTE_MODE_RESOLVING, assessmentResults.ModeResolvingScore );
            person.SetAttributeValue( ATTRIBUTE_MODE_COMPROMISING, assessmentResults.ModeCompromisingScore );
            person.SetAttributeValue( ATTRIBUTE_MODE_AVOIDING, assessmentResults.ModeAvoidingScore );
            person.SetAttributeValue( ATTRIBUTE_MODE_YEILDING, assessmentResults.ModeYieldingScore );
            person.SetAttributeValue( ATTRIBUTE_THEME_ACCOMMODATING, assessmentResults.EngagementAccommodatingScore );
            person.SetAttributeValue( ATTRIBUTE_THEME_WINNING, assessmentResults.EngagementWinningScore );
            person.SetAttributeValue( ATTRIBUTE_THEME_SOLVING, assessmentResults.EngagementSolvingScore );
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
            savedScores.ModeWinningScore = person.GetAttributeValue( ATTRIBUTE_MODE_WINNING ).AsDecimal();
            savedScores.ModeResolvingScore = person.GetAttributeValue( ATTRIBUTE_MODE_RESOLVING ).AsDecimal();
            savedScores.ModeCompromisingScore = person.GetAttributeValue( ATTRIBUTE_MODE_COMPROMISING ).AsDecimal();
            savedScores.ModeAvoidingScore = person.GetAttributeValue( ATTRIBUTE_MODE_AVOIDING ).AsDecimal();
            savedScores.ModeYieldingScore = person.GetAttributeValue( ATTRIBUTE_MODE_YEILDING ).AsDecimal();
            savedScores.EngagementAccommodatingScore = person.GetAttributeValue( ATTRIBUTE_THEME_ACCOMMODATING ).AsDecimal();
            savedScores.EngagementWinningScore = person.GetAttributeValue( ATTRIBUTE_THEME_WINNING ).AsDecimal();
            savedScores.EngagementSolvingScore = person.GetAttributeValue( ATTRIBUTE_THEME_SOLVING ).AsDecimal();

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
            public decimal ModeWinningScore { get; set; }

            /// <summary>
            /// Gets or sets the conflict mode resolving score.
            /// </summary>
            /// <value>
            /// The resolving score.
            /// </value>
            public decimal ModeResolvingScore { get; set; }

            /// <summary>
            /// Gets or sets the conflict mode compromising score.
            /// </summary>
            /// <value>
            /// The compromising score.
            /// </value>
            public decimal ModeCompromisingScore { get; set; }

            /// <summary>
            /// Gets or sets the conflict mode avoiding score.
            /// </summary>
            /// <value>
            /// The avoiding score.
            /// </value>
            public decimal ModeAvoidingScore { get; set; }

            /// <summary>
            /// Gets or sets the conflict mode yielding score.
            /// </summary>
            /// <value>
            /// The yielding score.
            /// </value>
            public decimal ModeYieldingScore { get; set; }

            /// <summary>
            /// Gets or sets the conflict engagement "solving" (aka engaged) score.
            /// </summary>
            /// <value>
            /// The conflict engagement "solving" score.
            /// </value>
            public decimal EngagementSolvingScore { get; set; }

            /// <summary>
            /// Gets or sets the conflict engagement "accommodating" (aka passive) score.
            /// </summary>
            /// <value>
            /// The conflict engagement "accommodating" score.
            /// </value>
            public decimal EngagementAccommodatingScore { get; set; }

            /// <summary>
            /// Gets or sets the conflict engagement "winning" (aka aggressive) score.
            /// </summary>
            /// <value>
            /// The conflict engagement "winning" score.
            /// </value>
            public decimal EngagementWinningScore { get; set; }

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
            /// Gets or sets the winning data.
            /// </summary>
            /// <value>
            /// The winning data.
            /// </value>
            public Dictionary<string, string> Winning { get; set; }

            /// <summary>
            /// Gets or sets the avoiding data.
            /// </summary>
            /// <value>
            /// The avoiding data.
            /// </value>
            public Dictionary<string, string> Avoiding { get; set; }

            /// <summary>
            /// Gets or sets the compromising data.
            /// </summary>
            /// <value>
            /// The compromising data.
            /// </value>
            public Dictionary<string, string> Compromising { get; set; }

            /// <summary>
            /// Gets or sets the resolving data.
            /// </summary>
            /// <value>
            /// The resolving data.
            /// </value>
            public Dictionary<string, string> Resolving { get; set; }

            /// <summary>
            /// Gets or sets the yielding data.
            /// </summary>
            /// <value>
            /// The yielding data.
            /// </value>
            public Dictionary<string, string> Yielding { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class ConflictProfile
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ConflictProfile"/> class.
            /// </summary>
            /// <param name="mean">The mean.</param>
            /// <param name="standardDeviation">The standard deviation.</param>
            /// <param name="constructs">The constructs.</param>
            public ConflictProfile( double mean, double standardDeviation, List<string> constructs )
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