// <copyright>
// Copyright by Central Christian Church
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
