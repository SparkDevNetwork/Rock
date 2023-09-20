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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;

namespace org.lakepointe.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Select the Age of the Person on Nearest Birthday" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select Person's Age on Nearest Birthday" )]
    public class NearestAgeSelect : Rock.Reporting.DataSelect.Person.AgeSelect
    {
        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( RockContext context, MemberExpression entityIdProperty, string selection )
        {
            DateTime conversionDate = RockDateTime.Today.AddMonths( 6 );  // Calculate age at nearest birthdate, not last birthdate
            
            //// have SQL Server do the following math (DateDiff only returns the integers):
            //// If the person hasn't had their birthday this year, their age is the DateDiff in Years - 1, otherwise, it is DateDiff in Years (without adjustment)
            var personAgeQuery = new PersonService( context ).Queryable()
                .Select( p => p.BirthDate > SqlFunctions.DateAdd( "year", -SqlFunctions.DateDiff( "year", p.BirthDate, conversionDate ), conversionDate )
                    ? SqlFunctions.DateDiff( "year", p.BirthDate, conversionDate ) - 1
                    : SqlFunctions.DateDiff( "year", p.BirthDate, conversionDate ));

            var selectAgeExpression = SelectExpressionExtractor.Extract( personAgeQuery, entityIdProperty, "p" );

            return selectAgeExpression;
        }

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        /// </value>
        public override string GetTitle( Type entityType )
        {
            return "Age on Nearest Birthday";
        }
    }
}
