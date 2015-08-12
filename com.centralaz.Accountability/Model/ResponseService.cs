using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using com.centralaz.Accountability.Data;
using Rock.Model;

namespace com.centralaz.Accountability.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class ResponseService : AccountabilityService<Response>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ResponseService(AccountabilityContext context) : base(context) { }

        /// <summary>
        /// Returns the response the responseId is to
        /// </summary>
        public Response GetResponse(int responseId)
        {
            return Queryable("Question")
                .Where(r => (r.Id == responseId))
                .FirstOrDefault();

        }
        /// <summary>
        /// Returns a percentage of correct answers
        /// </summary>
        /// <param name="personId"> The person</param>
        /// <param name="groupId">The group</param>
        /// <param name="questionId">The question that we're getting the percentage for</param>
        /// <returns>percentage[0] is the amount correct, and percentage[1] is the amount total</returns>
        public double[] ResponsePercentage(int personId, int groupId, int questionId)
        {
            double[] percentage = new double[2];
            var qry = Queryable("ResponseSet")
                .Where(r => (r.ResponseSet.PersonId == personId) && (r.ResponseSet.GroupId == groupId) && (r.QuestionId == questionId));
            percentage[1] = qry.Count();
            qry = qry.Where(r => r.IsResponseYes == true);
            percentage[0] = qry.Count();
            return percentage;
        }

        /// <summary>
        /// Gets the responses for a response set.
        /// </summary>
        /// <param name="responseSetId">The Id of the response set</param>
        /// <returns>Returns a list of responses</returns>
        public List<Response> GetResponsesForResponseSet(int responseSetId)
        {
            List<Response> responses = Queryable("ResponseSet")
                .Where(r => r.ResponseSetId == responseSetId)
                .ToList();
            return responses;
        }

        /// <summary>
        /// Gets a particular response for a response set and question
        /// </summary>
        /// <param name="responseSetId">The response set id</param>
        /// <param name="questionId">The question id</param>
        /// <returns>Returns the response</returns>
        public Response GetResponseForResponseSetAndQuestion(int responseSetId, int questionId)
        {
            Response response = null;
            var qry = Queryable()
                .Where(r => r.ResponseSetId == responseSetId && r.QuestionId == questionId);
            if (qry.Count() != 0)
            {
                response = qry.First();
            }  
            return response;
        }
        /// <summary>
        /// Returns all the responses for a question
        /// </summary>
        /// <param name="questionId">The question Id</param>
        /// <returns>The List of responses</returns>
        public List<Response> GetResponsesForQuestion( int questionId )
        {
            List<Response> responseList = Queryable()
                .Where( r => r.QuestionId == questionId )
                .ToList();
            return responseList;               
        }

    }
}
