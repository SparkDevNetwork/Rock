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
    public class ResponseSetService : AccountabilityService<ResponseSet>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseSetService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ResponseSetService( AccountabilityContext context ) : base( context ) { }

        public ResponseSet GetMostRecentReport( int? pId, int groupId )
        {
            var qry = Queryable()
                .Where( r => ( r.PersonId == pId ) && ( r.GroupId == groupId ) )
                .OrderBy( r => r.SubmitForDate )
                .FirstOrDefault();
            return qry;

        }
        public List<ResponseSet> GetResponseSetsForGroup( int groupId )
        {
            List<ResponseSet> responseSets = Queryable()
                .Where( r => r.GroupId == groupId )
                .ToList();
            return responseSets;
        }

        public bool DoesResponseSetExistWithSubmitDate( DateTime dateTime, int? personId, int groupId )
        {
            if ( personId == null )
            {
                return false;
            }
            var qry = Queryable()
                .Where( r => r.SubmitForDate == dateTime.Date &&
                    r.PersonId == personId &&
                    r.GroupId == groupId )
                .ToList();
            if ( qry.Count == 1 )
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public double GetOverallScore( int PersonId, int GroupId )
        {
            double overallScore = 0;
            var qry = Queryable()
                .Where( r => ( r.PersonId == PersonId ) && ( r.GroupId == GroupId ) );
            foreach ( ResponseSet r in qry )
            {
                overallScore += r.Score;
            }
            if ( qry.Count() == 0 )
            {
                overallScore = -1;
            }
            else
            {
                overallScore = overallScore / qry.Count();
            }

            return overallScore;
        }
        public double[] GetWeakScore( int PersonId, int GroupId )
        {
            double[] weakScore = new double[2];
            weakScore[0] = -1;
            QuestionService questionService = new QuestionService( new AccountabilityContext() );
            ResponseService responseService = new ResponseService( new AccountabilityContext() );

            GroupService groupService = new GroupService( new Rock.Data.RockContext() );
            int groupTypeId = groupService.Queryable()
                .Where( g => g.Id == GroupId )
                .Select( g => g.GroupTypeId )
                .FirstOrDefault();
            //get questions in responseset
            var questions = questionService.Queryable()
                .Where( q => q.GroupTypeId == groupTypeId );
            //for each question get ResponsePercentage
            foreach ( Question q in questions )
            {
                double[] score = responseService.ResponsePercentage( PersonId, GroupId, q.Id );
                if ( score[1] != null && score[1] != 0 )
                {
                    double scorePercentage = score[0] / score[1];
                    if ( weakScore[0] == -1 || weakScore[0] > scorePercentage )
                    {
                        weakScore[0] = scorePercentage;
                        weakScore[1] = q.Id;
                    }
                }
            }
            return weakScore;
        }
        public double[] GetStrongScore( int PersonId, int GroupId )
        {
            double[] strongScore = new double[2];
            strongScore[0] = -1;
            QuestionService questionService = new QuestionService( new AccountabilityContext() );
            ResponseService responseService = new ResponseService( new AccountabilityContext() );

            GroupService groupService = new GroupService( new Rock.Data.RockContext() );
            int groupTypeId = groupService.Queryable()
                .Where( g => g.Id == GroupId )
                .Select( g => g.GroupTypeId )
                .FirstOrDefault();
            //get questions in responseset
            var questions = questionService.Queryable()
                .Where( q => q.GroupTypeId == groupTypeId );
            //for each question get ResponsePercentage
            foreach ( Question q in questions )
            {
                double[] score = responseService.ResponsePercentage( PersonId, GroupId, q.Id );
                if ( score[1] != null && score[1] != 0 )
                {
                    double scorePercentage = score[0] / score[1];
                    if ( strongScore[0] == -1 || strongScore[0] < scorePercentage )
                    {
                        strongScore[0] = scorePercentage;
                        strongScore[1] = q.Id;
                    }
                }
            }
            return strongScore;
        }

    }
}
