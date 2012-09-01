//********************************************************************************************
// DISC Personality Profile Assessment - This is basically a javascript to C# port 
//  with a few changes to make it more readable.
//
// Source Material from http://www.gregwiens.com/scid/
// Used By Permission
//
// Written By: Derek Mangrum
//       Date: 2012-04-25
//
//********************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Crm
{
    /// <summary>
    /// DISC Class for administering and scoring a DISC Assessment
    /// </summary>
    public class DiscService
    {
        /// <summary>
        /// Raw question data. This data format comes from source disc.js file from Greg Wiens.
        /// </summary>
        private static string[,] questionData = {
			{"Fearless in conquering a challenge", "Always enjoys having fun", "Free to express true feelings", "Usually at rest with circumstances", "NNNN", "NNNN"},
			{"Tries to avoid mistakes","Securely fixed in place","Convinces others ","Goes along with the flow","CDIS","CDIN"},
			{"Liked by others","Systematically thinks through issues","Clearly conveys thoughts","Untroubled by stress","ICDN","NCDS"},
			{"Enjoys talking","Able to control his/her impulses","Actions can be foreseen by others","Quickly makes decisions","ICSD","ICSD"},
			{"Willing to take risks","Easily distinguishes differences","People person","Avoids extremes","DCIS","DCIS"},
			{"Sensitive to the needs of others","Guides the thinking or behavior of others","Humbly acknowledges his/her abilities","Capable of producing a desired result","SINN","SNCD"},
			{"Full of energy","Tries to avoid mistakes","Takes control of situations","Readily accessible to others","ICDN","ICDS"},
			{"Has high-spirited energy","Easily focuses his/her attention","Tends not to speak frequently","Fully committed to achieving a goal","ICSD","INSD"},
			{"Has an extreme care for details","People person","Captivates peoples' attention","Able to give direction","CSID","CSID"},
			{"Fearless in conquering a challenge","Has the ability to encourage others","Readily follows someone else's lead","Minutely exact","DISN","DISC"},
			{"Humbly acknowledges abilities","Pays attention to the needs of others","Driven by goals","Sees the glass as half-full","CSDI","CSDI"},
			{"Drives others forward","Concerned about others' needs above own","Concerned with correct information","Able to take care of yourself","ISCD","ISCD"},
			{"Forcefully pursues a goal","Shares the feelings of others","Other people love being around him/her","Tries to avoid danger","DSIC","DSIC"},
			{"Likes to have things a certain way","Readily follows someone else's lead","Establishes new endeavors","Joyfully enjoys life","CSDI","CSDI"},
			{"Liked by others","Full of thought","Perseveres despite opposition","Does things the traditional way","ICDS","INDS"},
			{"Thinks through problems analytically","Willing to take risks","Loyal to others","Possesses a magnetic charm ","CDSI","CDSI"},
			{"Enjoys the company of others","Unmovable in opposition","Able to take care of oneself","Listens more than he/she talks","ISDC","ISDC"},
			{"Goes along with the flow","Has a one-track mind","Works systematically","Full of energy","SDCI","SNCI"},
			{"Continues despite difficulties","Shows love freely","Mindful of the needs of others","Prone to worrying","DISN","DISC"},
			{"Seeks the company of others","Cares deeply about the needs of others","Free from bias","Secure in his/her own abilities","ISND","ISCD"},
			{"Capable of producing a desired result","Gives freely to others","Clearly conveys meaningful thoughts","Fully committed to achieving a goal","CSID","CSID"},
			{"Instantly acts without thought","Sees things as they are","Readily accepts leadership roles","Usually at rest with circumstances","ICDS","ICDS"},
			{"Enjoys time spent with others","Systematically thinks through issues","Does not show weakness or uncertainty","Gives grace when it is undeserved","ICDS","ICDS"},
			{"Enjoys the company of others","Satisfied with life","Securely fixed in place","Follows directions","ISDC","ISCD"},
			{"Pushes others to do their best","Thinks through problems analytically","Collaborative team player","Upbeat about life","DCSI","DCSI"},
			{"Always enjoys having fun","Conforms exactly to a standard","Free to express true feelings","Can be depended on consistently","ICDS","ICDS"},
			{"Forcefully pursues a goal","Welcoming to others","Holds the attention of others","Carefully assesses risks","DISC","DISC"},
			{"Courteous of others","Requires others to accomplish tasks correctly","Sees the glass half-full","Concerned with the feelings of others","CDIS","CDIS"},
			{"Readily follows someone else's lead","Tries to avoid mistakes","Does not show weakness or uncertainty","Minutely exact","NNNN","NNNN"},
			{"Capable of expressing strong feelings","Prominently stands out","Minutely exact","Satisfied with life","IDCS","IDCS"}
		};

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
        /// Fetch a List of <see cref="ResponseItem"/> for display/processing.
        /// </summary>
        /// <returns>a List of <see cref="ResponseItem"/>.</returns>
        static public List<ResponseItem> GetResponses()
        {
            List<ResponseItem> responseList = new List<ResponseItem>();
            ResponseItem response = new ResponseItem();

            for (int questionIndex = 0; questionIndex < questionData.GetLength(0); questionIndex++)
            {
                for (int responseIndex = 0; responseIndex < 4; responseIndex++)
                {
                    response.QuestionNumber = (questionIndex + 1).ToString("D2");
                    response.ResponseNumber = (responseIndex + 1).ToString();
                    response.ResponseID = response.QuestionNumber + response.ResponseNumber; ;
                    response.ResponseText = questionData[questionIndex, responseIndex];
                    response.MostScore = questionData[questionIndex, 4].Substring(responseIndex, 1);
                    response.LeastScore = questionData[questionIndex, 5].Substring(responseIndex, 1);

                    responseList.Add(response);
                }
            }
            return responseList;
        }

        /// <summary>
        /// Scores the test.
        /// </summary>
        /// <param name="selectedResponseIDs">a List of ResponseIDs to be scored.</param>
        /// <returns>a struct TestResults object with final scores.</returns>
        //static public AssessmentResults Score(List<string> selectedResponseIDs)
        static public String Score(List<string> selectedResponseIDs)
        {
            List<DiscService.ResponseItem> responseList = DiscService.GetResponses();

            // Holds the most and least totals for each Letter attribute
            Dictionary<string, int[]> results = new Dictionary<string, int[]>();
            results["S"] = new int[] { 0, 0 };
            results["C"] = new int[] { 0, 0 };
            results["I"] = new int[] { 0, 0 };
            results["N"] = new int[] { 0, 0 }; // This is intentionally not used after most/least totalling (foreach loop below). Placebo questions?
            results["D"] = new int[] { 0, 0 };

            foreach (string selectedResponseID in selectedResponseIDs)
            {
                string responseID = selectedResponseID.Substring(0, 3);
                string MorL = selectedResponseID.Substring(3, 1);

                DiscService.ResponseItem selectedResponse = responseList.Find(
                    delegate(DiscService.ResponseItem responseItem)
                    {
                        return responseItem.ResponseID == responseID;
                    }
                );

                if (MorL == "m")
                    results[selectedResponse.MostScore][0]++;
                else
                    results[selectedResponse.LeastScore][1]++;
            }

            int nbS = 27 - results["S"][1];
            int nbC = 26 - results["C"][1];
            int nbI = 26 - results["I"][1];
            int nbD = 27 - results["D"][1];

            decimal decX = results["S"][0] + results["C"][0] + results["I"][0] + results["D"][0];
            decimal decY = nbS + nbC + nbI + nbD;

            string testResults = String.Empty;
            if (decX > 0 && decY > 0)
            {
                testResults += Convert.ToInt32((results["D"][0] / decX * 100)) + ":";
                testResults += Convert.ToInt32((results["I"][0] / decX * 100)) + ":";
                testResults += Convert.ToInt32((results["S"][0] / decX * 100)) + ":";
                testResults += Convert.ToInt32((results["C"][0] / decX * 100)) + ":";

                testResults += Convert.ToInt32((nbD / decY * 100)) + ":";
                testResults += Convert.ToInt32((nbI / decY * 100)) + ":";
                testResults += Convert.ToInt32((nbS / decY * 100)) + ":";
                testResults += Convert.ToInt32((nbC / decY * 100));
            }

            return testResults;
        }
    }
}