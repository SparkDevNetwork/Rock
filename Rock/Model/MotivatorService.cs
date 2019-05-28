using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Motivator Class for administering and scoring a Motivator test
    /// </summary>
    public class MotivatorService
    {
        private const string ATTRIBUTE_MOTIVATOR_BELIEVING = "core_MotivatorBelieving";

        private const string ATTRIBUTE_MOTIVATOR_CARING = "core_MotivatorCaring";

        private const string ATTRIBUTE_MOTIVATOR_EXPRESSING = "core_MotivatorExpressing";

        private const string ATTRIBUTE_MOTIVATOR_EMPOWERING = "core_MotivatorEmpowering";

        private const string ATTRIBUTE_MOTIVATOR_ENGAGING = "core_MotivatorEngaging";

        private const string ATTRIBUTE_MOTIVATOR_ADAPTING = "core_MotivatorAdapting";

        private const string ATTRIBUTE_MOTIVATOR_GATHERING = "core_MotivatorGathering";

        private const string ATTRIBUTE_MOTIVATOR_INNOVATING = "core_MotivatorInnovating";

        private const string ATTRIBUTE_MOTIVATOR_LEADING = "core_MotivatorLeading";

        private const string ATTRIBUTE_MOTIVATOR_LEARNING = "core_MotivatorLearning";

        private const string ATTRIBUTE_MOTIVATOR_MAXIMIZING = "core_MotivatorMaximizing";

        private const string ATTRIBUTE_MOTIVATOR_ORGANIZING = "core_MotivatorOrganizing";

        private const string ATTRIBUTE_MOTIVATOR_PACING = "core_MotivatorPacing";

        private const string ATTRIBUTE_MOTIVATOR_PERCEIVING = "core_MotivatorPerceiving";

        private const string ATTRIBUTE_MOTIVATOR_RELATING = "core_MotivatorRelating";

        private const string ATTRIBUTE_MOTIVATOR_SERVING = "core_MotivatorServing";

        private const string ATTRIBUTE_MOTIVATOR_THINKING = "core_MotivatorThinking";

        private const string ATTRIBUTE_MOTIVATOR_TRANSFORMING = "core_MotivatorTransforming";

        private const string ATTRIBUTE_MOTIVATOR_UNITING = "core_MotivatorUniting";

        private const string ATTRIBUTE_MOTIVATOR_PERSERVERING = "core_MotivatorPersevering";

        private const string ATTRIBUTE_MOTIVATOR_RISKING = "core_MotivatorRisking";

        private const string ATTRIBUTE_MOTIVATOR_VISIONING = "core_MotivatorVisioning";

        private const string ATTRIBUTE_MOTIVATOR_GROWTHPROPENSITY = "core_MotivatorGrowthPropensity";

        private const string ATTRIBUTE_MOTIVATOR_CLUSTER_INFLUENTIAL = "core_MotivatorClusterInfluential";

        private const string ATTRIBUTE_MOTIVATOR_CLUSTER_ORGANIZATIONAL = "core_MotivatorClusterOrganizational";

        private const string ATTRIBUTE_MOTIVATOR_CLUSTER_INTELLECTUAL = "core_MotivatorClusterIntellectual";

        private const string ATTRIBUTE_MOTIVATOR_CLUSTER_OPERATIONAL = "core_MotivatorClusterOperational";

        /// <summary>
        /// Raw question data with code as key.
        /// </summary>
        private static List<MotivatorQuestion> questionData = new List<MotivatorQuestion>(){
            new MotivatorQuestion ("f0101","I have well founded thoughts regarding what I believe.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_BELIEVING),
            new MotivatorQuestion ("f0102","I’m not afraid to tell others what I believe.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_BELIEVING),
            new MotivatorQuestion ("f0103","I have clear expectations of myself and others.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_BELIEVING),
            new MotivatorQuestion ("f0104","I have done the work necessary to support my convictions.",OptionType.Agreement,SystemGuid.DefinedValue.MOTIVATOR_BELIEVING),
            new MotivatorQuestion ("f0105","I find it easy to share my opinion with others who may disagree.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_BELIEVING),
            new MotivatorQuestion ("f0106","I clearly know what I believe on an issue.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_BELIEVING),
            new MotivatorQuestion ("f0201","I care deeply about other’s needs.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_CARING),
            new MotivatorQuestion ("f0202","I hurt when others hurt.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_CARING),
            new MotivatorQuestion ("f0203","I easily feel the pain of others.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_CARING),
            new MotivatorQuestion ("f0204","I have empathy for those in difficult situations.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_CARING),
            new MotivatorQuestion ("f0301","People describe me as an engaging communicator.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_EXPRESSING),
            new MotivatorQuestion ("f0302","Others describe my speaking as challenging them to change.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_EXPRESSING),
            new MotivatorQuestion ("f0303","I hold the attention of others when I speak.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_EXPRESSING),
            new MotivatorQuestion ("f0304","I enjoy speaking in public.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_EXPRESSING),
            new MotivatorQuestion ("f0305","Being in front of people doesn't bother me.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_EXPRESSING),
            new MotivatorQuestion ("f0401","I invest in others who see positive life change.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_EMPOWERING),
            new MotivatorQuestion ("f0402","I invest in others who lead well.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_EMPOWERING),
            new MotivatorQuestion ("f0403","I match others' strengths to needs.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_EMPOWERING),
            new MotivatorQuestion ("f0404","I equip others to do what they do best.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_EMPOWERING),
            new MotivatorQuestion ("f0405","I match others' abilities with appropriate opportunities.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_EMPOWERING),
            new MotivatorQuestion ("f0501","I'm involved with various community organizations.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_ENGAGING),
            new MotivatorQuestion ("f0502","I identify community needs well.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_ENGAGING),
            new MotivatorQuestion ("f0503","I see successes or failures of community service organizations.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_ENGAGING),
            new MotivatorQuestion ("f0504","I know the background of my community.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_ENGAGING),
            new MotivatorQuestion ("f0505","I have a specific approach to helping in my community.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_ENGAGING),
            new MotivatorQuestion ("f0506","I demonstrate a commitment to helping others in my community.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_ENGAGING),
            new MotivatorQuestion ("f0601","I quickly change a plan that is not working.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_ADAPTING),
            new MotivatorQuestion ("f0602","I create new ways of doing things.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_ADAPTING),
            new MotivatorQuestion ("f0603","Others see me as open to change.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_ADAPTING),
            new MotivatorQuestion ("f0604","I cope well with ambiguity.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_ADAPTING),
            new MotivatorQuestion ("f0605","I enjoy constant change.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_ADAPTING),
            new MotivatorQuestion ("f0606","I plan my projects knowing that it will change as issues arise.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_ADAPTING),
            new MotivatorQuestion ("f0701","Others want to work with me.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_GATHERING),
            new MotivatorQuestion ("f0702","Others change their lives through my influence.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_GATHERING),
            new MotivatorQuestion ("f0703","People emulate me.",OptionType.Frequency,SystemGuid.DefinedValue.MOTIVATOR_GATHERING),
            new MotivatorQuestion ("f0704","People enjoy hanging around me.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_GATHERING),
            new MotivatorQuestion ("f0705","People are drawn to me.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_GATHERING),
            new MotivatorQuestion ("f0801","I have a high energy level when starting a new project.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_INNOVATING),
            new MotivatorQuestion ("f0802","I enjoy starting things.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_INNOVATING),
            new MotivatorQuestion ("f0803","I enjoy creating something from nothing.",OptionType.Agreement,SystemGuid.DefinedValue.MOTIVATOR_INNOVATING),
            new MotivatorQuestion ("f0804","I enjoy finding new ways of doing something.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_INNOVATING),
            new MotivatorQuestion ("f0805","I enjoy the challenge of the impossible.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_INNOVATING),
            new MotivatorQuestion ("f0901","I naturally take the lead when there is no given leader.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_LEADING),
            new MotivatorQuestion ("f0902","I am often asked to lead a team after joining it as a member.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_LEADING),
            new MotivatorQuestion ("f0903","Individuals follow my lead.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_LEADING),
            new MotivatorQuestion ("f0904","I prefer to have responsibility for a team.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_LEADING),
            new MotivatorQuestion ("f0905","I know how to inspire others toward an objective.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_LEADING),
            new MotivatorQuestion ("f1001","I prefer learning new things.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_LEARNING),
            new MotivatorQuestion ("f1002","I enjoy reading or listening to things that stretch my thinking.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_LEARNING),
            new MotivatorQuestion ("f1003","I find time to listen or read things that challenge me.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_LEARNING),
            new MotivatorQuestion ("f1004","I feel stagnant if I am not learning something new.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_LEARNING),
            new MotivatorQuestion ("f1005","The more I learn, the more I want to learn.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_LEARNING),
            new MotivatorQuestion ("f1101","I build relationships with others who are making significant impact.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_MAXIMIZING),
            new MotivatorQuestion ("f1102","I strive to gain the greatest impact with my time.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_MAXIMIZING),
            new MotivatorQuestion ("f1103","I put my efforts where it will produce the greatest effect.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_MAXIMIZING),
            new MotivatorQuestion ("f1104","I seek the highest rate of return for my investments.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_MAXIMIZING),
            new MotivatorQuestion ("f1105","I’m alert to ways that increase the impact of my resources.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_MAXIMIZING),
            new MotivatorQuestion ("f1201","I pay close attention to details because they are important.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_ORGANIZING),
            new MotivatorQuestion ("f1202","I’m persistent in following up on details.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_ORGANIZING),
            new MotivatorQuestion ("f1203","My grasp of details keeps everything working well.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_ORGANIZING),
            new MotivatorQuestion ("f1204","I enjoy organizing many different pieces.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_ORGANIZING),
            new MotivatorQuestion ("f1301","I keep time in my schedule for personally restorative activities.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_PACING),
            new MotivatorQuestion ("f1302","I find time to do the important things in my life.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_PACING),
            new MotivatorQuestion ("f1303","I set healthy boundaries in my job and life.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_PACING),
            new MotivatorQuestion ("f1304","I have a good work-home balance.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_PACING),
            new MotivatorQuestion ("f1305","I know how much I can handle so I say no before my limit is reached.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_PACING),
            new MotivatorQuestion ("f1401","I sense things about individuals that I find out later are true.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_PERCEIVING),
            new MotivatorQuestion ("f1402","I notice body language that is not consistent with what is being said.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_PERCEIVING),
            new MotivatorQuestion ("f1403","I know something is going on beneath the surface which otherwise can't be explained.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_PERCEIVING),
            new MotivatorQuestion ("f1404","My intuitions are usually correct.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_PERCEIVING),
            new MotivatorQuestion ("f1405","I pay attention to my gut instinct.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_PERCEIVING),
            new MotivatorQuestion ("f1406","Others seek out my perspective on situations that are unclear.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_PERCEIVING),
            new MotivatorQuestion ("f1501","Others describe me as friendly.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_RELATING),
            new MotivatorQuestion ("f1502","People seek me out to listen to their problems.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_RELATING),
            new MotivatorQuestion ("f1503","Others feel accepted by me.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_RELATING),
            new MotivatorQuestion ("f1504","I value many different types of people.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_RELATING),
            new MotivatorQuestion ("f1505","People feel encouraged when I individually spend time with them.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_RELATING),
            new MotivatorQuestion ("f1506","People enjoy spending time with me.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_RELATING),
            new MotivatorQuestion ("f1601","I want to know what is clearly expected.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_SERVING),
            new MotivatorQuestion ("f1602","I prefer knowing what is expected of me.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_SERVING),
            new MotivatorQuestion ("f1603","I enjoy serving others who lead.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_SERVING),
            new MotivatorQuestion ("f1604","I do the work that is behind the scenes.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_SERVING),
            new MotivatorQuestion ("f1605","I don’t have to be in front of people.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_SERVING),
            new MotivatorQuestion ("f1606","I like doing the little things that most people miss.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_SERVING),
            new MotivatorQuestion ("F1701","I evaluate what I’m thinking in my conversations with others.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_THINKING),
            new MotivatorQuestion ("f1702","I reflect on why I react like I do.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_THINKING),
            new MotivatorQuestion ("f1703","I'm able to see patterns in the way I think.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_THINKING),
            new MotivatorQuestion ("f1704","I’m aware when my thinking is impaired for one reason or another.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_THINKING),
            new MotivatorQuestion ("f1705","I find myself investigating why something disturbs me.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_THINKING),
            new MotivatorQuestion ("f1706","I regularly examine my own perspective when learning from others.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_THINKING),
            new MotivatorQuestion ("f1801","I help people know how to embrace crucial changes.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_TRANSFORMING),
            new MotivatorQuestion ("f1802","I enjoy walking with people through the fear of important changes.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_TRANSFORMING),
            new MotivatorQuestion ("f1803","I know how to make people feel comfortable with necessary changes.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_TRANSFORMING),
            new MotivatorQuestion ("f1804","I help people process essential changes.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_TRANSFORMING),
            new MotivatorQuestion ("f1805","I can help people see the good coming out of confusion.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_TRANSFORMING),
            new MotivatorQuestion ("f1901","I build a group of individuals into a committed team.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_UNITING),
            new MotivatorQuestion ("f1902","I enjoy helping individuals gain ownership of a vision.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_UNITING),
            new MotivatorQuestion ("f1903","When I lead a team, the members are committed to each other.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_UNITING),
            new MotivatorQuestion ("f1904","I have the ability to get disjointed members to work as a team.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_UNITING),
            new MotivatorQuestion ("f1905","I deal with conflict in a way that brings the team together.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_UNITING),
            new MotivatorQuestion ("f1906","I help people feel responsible for the success of the team.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_UNITING),
            new MotivatorQuestion ("f2001","I am motivated in the face of discouragement.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_PERSEVERING),
            new MotivatorQuestion ("f2002","I experience setbacks without defeat.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_PERSEVERING),
            new MotivatorQuestion ("f2003","I ride the ups and downs well.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_PERSEVERING),
            new MotivatorQuestion ("f2004","I rebound from disappointments with renewed focus and commitment.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_PERSEVERING),
            new MotivatorQuestion ("f2005","I'm not easily discouraged.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_PERSEVERING),
            new MotivatorQuestion ("f2101","I like to be challenged.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_RISKTAKING),
            new MotivatorQuestion ("f2102","I enjoy taking on assignments which I don't know if I can accomplish.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_RISKTAKING),
            new MotivatorQuestion ("f2103","I see risk as a necessary part of the decisions I make.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_RISKTAKING),
            new MotivatorQuestion ("f2104","I become more engaged when there is some risk involved.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_RISKTAKING),
            new MotivatorQuestion ("f2105","Risk does not discourage me from attempting something.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_RISKTAKING),
            new MotivatorQuestion ("f2106","I am not threatened by risk.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_RISKTAKING),
            new MotivatorQuestion ("f2201","I can picture what I would like to see become reality.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_VISIONING),
            new MotivatorQuestion ("f2202","I dream of things that don't exist yet.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_VISIONING),
            new MotivatorQuestion ("f2203","I focus on a preferred future when challenging others.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_VISIONING),
            new MotivatorQuestion ("f2204","I persuasively convince others of what could be.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_VISIONING),
            new MotivatorQuestion ("f2205","I approach challenges as opportunities to bring about a different future.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_VISIONING),
            new MotivatorQuestion ("f2206","Others are attracted to follow my vision.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_VISIONING),
            new MotivatorQuestion ("f2207","I see the unrealized potential in my organization.",OptionType.Agreement, SystemGuid.DefinedValue.MOTIVATOR_VISIONING),
            new MotivatorQuestion ("ps0101","I embrace challenges.",OptionType.Frequency, SystemGuid.DefinedValue.MOTIVATOR_GROWTH_PROPENSITY),
            new MotivatorQuestion ("ps0102N","I avoid challenges.",OptionType.Frequency,  SystemGuid.DefinedValue.MOTIVATOR_GROWTH_PROPENSITY),
            new MotivatorQuestion ("ps0103","I persist in the face of setbacks.",OptionType.Frequency,  SystemGuid.DefinedValue.MOTIVATOR_GROWTH_PROPENSITY),
            new MotivatorQuestion ("ps0104N","I give up easily with setbacks.",OptionType.Frequency,  SystemGuid.DefinedValue.MOTIVATOR_GROWTH_PROPENSITY),
            new MotivatorQuestion ("ps0105","I see effort as the path to mastery.",OptionType.Frequency,  SystemGuid.DefinedValue.MOTIVATOR_GROWTH_PROPENSITY),
            new MotivatorQuestion ("ps0109","I learn lessons from the successes of others.",OptionType.Frequency,  SystemGuid.DefinedValue.MOTIVATOR_GROWTH_PROPENSITY),
            new MotivatorQuestion ("ps0111","I am inspired to learn from the accomplishments of others.",OptionType.Frequency,  SystemGuid.DefinedValue.MOTIVATOR_GROWTH_PROPENSITY)
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
        private static Dictionary<Guid, MotivatorData> constructData = new Dictionary<Guid, MotivatorData>()
        {
            { SystemGuid.DefinedValue.MOTIVATOR_BELIEVING.AsGuid(),  new MotivatorData(27.56, 4.441 ) },
            { SystemGuid.DefinedValue.MOTIVATOR_CARING.AsGuid(),  new MotivatorData(17.06, 5.391 ) },
            { SystemGuid.DefinedValue.MOTIVATOR_EXPRESSING.AsGuid(), new MotivatorData(21.82, 6.247) },
            { SystemGuid.DefinedValue.MOTIVATOR_EMPOWERING.AsGuid(), new MotivatorData(21.87, 4.126) },
            { SystemGuid.DefinedValue.MOTIVATOR_ENGAGING.AsGuid(), new MotivatorData(23.25, 6.58) },
            { SystemGuid.DefinedValue.MOTIVATOR_ADAPTING.AsGuid(), new MotivatorData(23.26, 5.144) },
            { SystemGuid.DefinedValue.MOTIVATOR_GATHERING.AsGuid(), new MotivatorData(21.77, 3.475) },
            { SystemGuid.DefinedValue.MOTIVATOR_INNOVATING.AsGuid(), new MotivatorData(22.66, 5.157) },
            { SystemGuid.DefinedValue.MOTIVATOR_LEADING.AsGuid(), new MotivatorData(22.52, 4.657) },
            { SystemGuid.DefinedValue.MOTIVATOR_LEARNING.AsGuid(), new MotivatorData(24.37, 5.192) },
            { SystemGuid.DefinedValue.MOTIVATOR_MAXIMIZING.AsGuid(), new MotivatorData(21.59, 4.161) },
            { SystemGuid.DefinedValue.MOTIVATOR_ORGANIZING.AsGuid(), new MotivatorData(16.84, 4.802) },
            { SystemGuid.DefinedValue.MOTIVATOR_PACING.AsGuid(), new MotivatorData(20.09, 5.636) },
            { SystemGuid.DefinedValue.MOTIVATOR_PERCEIVING.AsGuid(), new MotivatorData(27.75, 4.365) },
            { SystemGuid.DefinedValue.MOTIVATOR_RELATING.AsGuid(), new MotivatorData(28.92, 4.302) },
            { SystemGuid.DefinedValue.MOTIVATOR_SERVING.AsGuid(), new MotivatorData(25.97, 5.187) },
            { SystemGuid.DefinedValue.MOTIVATOR_THINKING.AsGuid(), new MotivatorData(27.93, 4.418) },
            { SystemGuid.DefinedValue.MOTIVATOR_TRANSFORMING.AsGuid(), new MotivatorData(21.5, 4.556) },
            { SystemGuid.DefinedValue.MOTIVATOR_UNITING.AsGuid(), new MotivatorData(26.37, 5.22) },
            { SystemGuid.DefinedValue.MOTIVATOR_PERSEVERING.AsGuid(), new MotivatorData(20.53, 4.715) },
            { SystemGuid.DefinedValue.MOTIVATOR_RISKTAKING.AsGuid(), new MotivatorData(24.83, 6.619) },
            { SystemGuid.DefinedValue.MOTIVATOR_VISIONING.AsGuid(), new MotivatorData(30.03, 5.883) },
            { SystemGuid.DefinedValue.MOTIVATOR_GROWTH_PROPENSITY.AsGuid(), new MotivatorData(32.63, 4.728) },
        };

        /// <summary>
        /// Agreement Response Option
        /// </summary>
        public static List<ResponseOption> Agreement_Option = new List<ResponseOption>()
        {
            new ResponseOption ("Strongly Disagree",0,1 ),
            new ResponseOption ("Disagree",1,5 ),
            new ResponseOption ("Somewhat Disagree",2,4 ),
            new ResponseOption ("Undecided",3,3 ),
            new ResponseOption ("Somewhat agree",4,2 ),
            new ResponseOption ("Agree",5,1 ),
            new ResponseOption ("Strongly agree",6,0 ),
        };

        /// <summary>
        /// Frequency Response Option
        /// </summary>
        public static List<ResponseOption> Frequency_Option = new List<ResponseOption>()
        {
            new ResponseOption ("Never",0,1 ),
            new ResponseOption ("Rarely",1,5 ),
            new ResponseOption ("Occasionally",2,4 ),
            new ResponseOption ("Sometimes",3,3 ),
            new ResponseOption ("Frequently",4,2 ),
            new ResponseOption ("Usually",5,1 ),
            new ResponseOption ("Always",6,0 ),
        };

        /// <summary>
        /// Gets the questions.
        /// </summary>
        /// <returns></returns>
        public static List<MotivatorQuestion> GetQuestions()
        {
            Random r = new Random();
            var questions = questionData.OrderBy( x => r.Next( 0, questionData.Count ) ).ToList();
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

            var motivatorClusterScore = new Dictionary<Guid, List<decimal>>();
            var motivatorScore = new Dictionary<DefinedValueCache, decimal>();
            var grandTotal = assessmentResponse.Sum( a => Convert.ToDouble( a.Value ) );
            foreach ( var motivatorDefinedValueGuid in constructData.Keys )
            {
                var conflictProfile = constructData[( Guid ) motivatorDefinedValueGuid];
                double totalResponse = 0;
                var assessmentData = new Dictionary<string, string>();
                foreach ( var construct in questionData.Where( ( Func<MotivatorQuestion, bool> ) ( a => a.MotivatorId.AsGuid() == motivatorDefinedValueGuid ) ) )
                {
                    if ( assessmentResponse.ContainsKey( construct.Id ) )
                    {
                        totalResponse += assessmentResponse[construct.Id];
                        assessmentData.AddOrReplace( construct.Id, assessmentResponse[construct.Id].ToString() );
                    }
                }

                var zScore = Math.Round( ( totalResponse - conflictProfile.Mean ) / conflictProfile.StandardDeviation, 1 );
                var score = GetPercentFromScore( zScore );
                var motivatorDefinedValue = DefinedValueCache.Get( ( Guid ) motivatorDefinedValueGuid );
                motivatorScore.AddOrReplace( motivatorDefinedValue, score );

                testResults.AssessmentData.AddOrReplace( motivatorDefinedValue.Value, assessmentData );

                var clusterGuid = motivatorDefinedValue.GetAttributeValue("Cluster").AsGuidOrNull();
                if ( clusterGuid.HasValue )
                {
                    if ( !motivatorClusterScore.ContainsKey( clusterGuid.Value ) )
                    {
                        motivatorClusterScore[clusterGuid.Value] = new List<decimal>();
                    }
                    motivatorClusterScore[clusterGuid.Value].Add( score );
                }
            }

            foreach ( var clusterDefinedValueGuid in motivatorClusterScore.Keys )
            {
                var clusterScore = motivatorClusterScore[clusterDefinedValueGuid].Sum() / motivatorClusterScore[clusterDefinedValueGuid].Count();
                var clusterDefinedValue = DefinedValueCache.Get( ( Guid ) clusterDefinedValueGuid );
                testResults.MotivatorClusterScores.Add( new MotivatorScore()
                                                        {
                                                            DefinedValue = clusterDefinedValue,
                                                            Value = clusterScore
                                                        } );
            }

            testResults.MotivatorScores = motivatorScore
                                            .OrderByDescending( a => a.Value )
                                            .Select( a => new MotivatorScore()
                                            {
                                                DefinedValue = a.Key,
                                                Value = a.Value
                                            } )
                                            .ToList();
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

            var motivatorTypes = DefinedTypeCache.Get( SystemGuid.DefinedType.MOTIVATOR_TYPE.AsGuid() );
            foreach ( var motivatorScore in assessmentResults.MotivatorScores )
            {
                var scoreKey = motivatorScore.DefinedValue.GetAttributeValue( "AttributeScoreKey" );
                person.SetAttributeValue( scoreKey, motivatorScore.Value );
            }

            var motivatorClusterTypes = DefinedTypeCache.Get( SystemGuid.DefinedType.MOTIVATOR_CLUSTER_TYPE.AsGuid() );
            foreach ( var motivatorClusterScore in assessmentResults.MotivatorClusterScores )
            {
                var scoreKey = motivatorClusterScore.DefinedValue.GetAttributeValue( "AttributeScoreKey" );
                person.SetAttributeValue( scoreKey, motivatorClusterScore.Value );
            }

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

            var motivatorClusterScores = new Dictionary<DefinedValueCache, decimal>();
            var motivatorScores = new Dictionary<DefinedValueCache, decimal>();

            var motivatorClusterTypes = DefinedTypeCache.Get( SystemGuid.DefinedType.MOTIVATOR_CLUSTER_TYPE.AsGuid() );
            foreach ( var motivatorClusterType in motivatorClusterTypes.DefinedValues )
            {
                var scoreKey = motivatorClusterType.GetAttributeValue( "AttributeScoreKey" );
                motivatorClusterScores.Add( motivatorClusterType, person.GetAttributeValue( scoreKey ).AsDecimal() );
            }

            savedScores.MotivatorClusterScores = motivatorClusterScores
                                           .OrderByDescending( a => a.Value )
                                           .Select( a => new MotivatorScore()
                                           {
                                               DefinedValue = a.Key,
                                               Value = a.Value
                                           } )
                                           .ToList();

            var motivatorTypes = DefinedTypeCache.Get( SystemGuid.DefinedType.MOTIVATOR_TYPE.AsGuid() );
            foreach ( var motivatorType in motivatorTypes.DefinedValues )
            {
                var scoreKey = motivatorType.GetAttributeValue( "AttributeScoreKey" );
                motivatorScores.Add( motivatorType, person.GetAttributeValue( scoreKey ).AsDecimal() );
            }


            savedScores.MotivatorScores = motivatorScores
                                            .OrderByDescending( a => a.Value )
                                            .Select( a => new MotivatorScore()
                                            {
                                                DefinedValue = a.Key,
                                                Value = a.Value
                                            } )
                                            .ToList();

            return savedScores;
        }

        /// <summary>
        /// The MotivatorQuestion class used to return the question detail
        /// </summary>
        public class MotivatorQuestion
        {
            public MotivatorQuestion( string id, string question, OptionType optionType, string motivatorId )
            {
                this.Id = id;
                this.Question = question;
                this.OptionType = optionType;
                this.MotivatorId = motivatorId;
            }

            /// <summary>
            /// Gets or sets the question identifier.
            /// </summary>
            /// <value>
            /// The question identifier.
            /// </value>
            public string Id { get; set; }

            /// <summary>
            /// Gets or sets the question.
            /// </summary>
            /// <value>
            /// The question.
            /// </value>
            public string Question { get; set; }

            /// <summary>
            /// Gets or sets the option type for question.
            /// </summary>
            /// <value>
            /// The option type for question.
            /// </value>
            public OptionType OptionType { get; set; }

            /// <summary>
            /// Gets or sets the motivator identifier.
            /// </summary>
            /// <value>
            /// The motivator identifier.
            /// </value>
            public string MotivatorId { get; set; }
        }

        /// <summary>
        /// The AssessmentResults struct used to return the final assessment scores
        /// </summary>
        public class AssessmentResults
        {
            public AssessmentResults()
            {
                MotivatorScores = new List<MotivatorScore>();
                MotivatorClusterScores = new List<MotivatorScore>();
                AssessmentData = new Dictionary<string, Dictionary<string, string>>();
            }

            /// <summary>
            /// Gets or sets the Motivator Score data.
            /// </summary>
            /// <value>
            /// The Motivator Score data.
            /// </value>
            public List<MotivatorScore> MotivatorScores { get; set; }

            /// <summary>
            /// Gets or sets the Motivator Cluster Score data.
            /// </summary>
            /// <value>
            /// The Motivator Cluster Score data.
            /// </value>
            public List<MotivatorScore> MotivatorClusterScores { get; set; }

            /// <summary>
            /// Gets or sets the assessment data.
            /// </summary>
            /// <value>
            /// The assessment data.
            /// </value>
            public Dictionary<string, Dictionary<string, string>> AssessmentData { get; set; }
        }

        /// <summary>
        /// The MotivatorScore struct used to return the motivator score
        /// </summary>
        public class MotivatorScore : DotLiquid.Drop
        {
            /// <summary>
            /// Gets or sets the Name.
            /// </summary>
            /// <value>
            /// The Name.
            /// </value>
            public DefinedValueCache DefinedValue { get; set; }

            /// <summary>
            /// Gets or sets the score Value.
            /// </summary>
            /// <value>
            /// The score Value.
            /// </value>
            public decimal Value { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class MotivatorData
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="EQInventory"/> class.
            /// </summary>
            /// <param name="mean">The mean.</param>
            /// <param name="standardDeviation">The standard deviation.</param>
            public MotivatorData( double mean, double standardDeviation )
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

        /// <summary>
        /// The ResponseOption class used to return the response option
        /// </summary>
        public class ResponseOption
        {
            public ResponseOption( string name, int positive, int negative )
            {
                this.Name = name;
                this.Positive = positive;
                this.Negative = negative;
            }
            public string Name { get; set; }
            public int Positive { get; set; }
            public int Negative { get; set; }
        }

        public enum OptionType
        {
            Agreement,
            Frequency
        }
    }
}