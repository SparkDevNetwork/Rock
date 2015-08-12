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
    public class QuestionService : AccountabilityService<Question>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public QuestionService(AccountabilityContext context) : base(context) { }
        public string GetShortForm(double questionId)
        {
            Question question = Queryable()
                .Where(q => q.Id == questionId)
                .FirstOrDefault();
            return question.ShortForm;
        }
        public List<Question> GetQuestionsFromGroupTypeID(int groupTypeId)
        {
            List<Question> questions = Queryable("GroupType")
                .Where(q => q.GroupTypeId == groupTypeId)
                .ToList();
            return questions;
        }
    }
}
