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
using System.Web;
using System.IO;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Data;
using Rock.Web.Cache;
using Rock.Web;
using Rock.Web.UI;
using Rock.Communication;

namespace com.centralaz.GeneralJobs.Jobs
{
    /// <summary>
    /// For any people that share an email, this job sets everyone's preference to the most restrictive setting that one of them has.
    /// </summary>
    [DisallowConcurrentExecution]
    public class MatchEmailPreference : IJob
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchEmailPreference"/> class.
        /// </summary>
        public MatchEmailPreference()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Execute( IJobExecutionContext context )
        {

            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            var duplicateEmails = personService.Queryable()
                .GroupBy( p => p.Email )
                .Where( p => p.Count() >= 2 )
                .Select( p => p.Key )
                .ToList();

            var duplicateEmailPeople = personService.Queryable().Where( p => duplicateEmails.Contains( p.Email ) ).ToList();

            foreach ( var duplicateEmail in duplicateEmails )
            {
                var people = duplicateEmailPeople.Where( p => p.Email == duplicateEmail );
                EmailPreference maxPreference = people.Max( p => p.EmailPreference );

                if ( people.Any( p => p.EmailPreference != maxPreference ) )
                {
                    foreach ( var person in people.ToList() )
                    {
                        person.EmailPreference = maxPreference;
                    }
                }
            }

            rockContext.SaveChanges();
        }
    }
}