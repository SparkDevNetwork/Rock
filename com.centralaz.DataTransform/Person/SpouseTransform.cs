// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;

namespace com.centralaz.DataTransform.Person
{
    /// <summary>
    /// Person Spouse Transformation
    /// </summary>
    [Description( "Transform result to spouse" )]
    [Export( typeof( DataTransformComponent ) )]
    [ExportMetadata( "ComponentName", "Person Spouse Transformation" )]
    public class SpouseTransform : DataTransformComponent<Rock.Model.Person>
    {
        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public override string Title
        {
            get { return "com_centralaz: Spouse"; }
        }

        /// <summary>
        /// Gets the name of the transformed entity type.
        /// </summary>
        /// <value>
        /// The name of the transformed entity type.
        /// </value>
        public override string TransformedEntityTypeName
        {
            get { return "Rock.Model.Person"; }
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="whereExpression">The where expression.</param>
        /// <returns></returns>
        public override Expression GetExpression( IService serviceInstance, ParameterExpression parameterExpression, Expression whereExpression )
        {
            IQueryable<int> idQuery = serviceInstance.GetIds( parameterExpression, whereExpression );
            return BuildExpression( serviceInstance, idQuery, parameterExpression );
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="personQueryable">The person queryable.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns></returns>
        public override Expression GetExpression( IService serviceInstance, IQueryable<Rock.Model.Person> personQueryable, ParameterExpression parameterExpression )
        {
            return BuildExpression( serviceInstance, personQueryable.Select( p => p.Id ), parameterExpression );
        }

        /// <summary>
        /// Builds the expression to get spouses of the given id.  This will be all adult members of the id in the idQuery except himself/herself.
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="idQuery">The id query.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns></returns>
        private Expression BuildExpression( IService serviceInstance, IQueryable<int> idQuery, ParameterExpression parameterExpression )
        {
            Guid adultGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
            Guid marriedGuid = Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid();
            int marriedDefinedValueId = Rock.Web.Cache.DefinedValueCache.Read( marriedGuid ).Id;
            Guid familyGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();

            //// Spouse is determined if all these conditions are met
            //// 1) Adult in the same family as Person (GroupType = Family, GroupRole = Adult, and in same Group)
            //// 2) Opposite Gender as Person
            //// 3) Both Persons are Married

            var familyGroupMembers = new GroupMemberService( (RockContext)serviceInstance.Context ).Queryable()
                .Where( m => m.Group.GroupType.Guid == familyGuid                   // get all family members    
                    && m.GroupRole.Guid == adultGuid                                // who are adults
                    && idQuery.Contains( m.PersonId ) );                            // for all people given in the query

            var personSpouseQuery = new PersonService( (RockContext)serviceInstance.Context ).Queryable()
                .Where( p => familyGroupMembers                                     // now, go through all those family members
                    .Where( s => s.PersonId == p.Id                                 // where the person
                        && s.Person.MaritalStatusValueId == marriedDefinedValueId   // is married
                        && s.GroupRole.Guid == adultGuid )                          // and an adult (redundant?)
                    .Any( c => c.Group.Members                                      // and look at their family's members
                        .Any( m =>                                                  // and for any member
                            m.PersonId != p.Id                                      // that is not the same person
                            && m.GroupRole.Guid == adultGuid                        // who is also an adult
                            && m.Person.Gender != p.Gender                          // of the opposite gender
                            && m.Person.MaritalStatusValueId == marriedDefinedValueId // who is married
                            && !m.Person.IsDeceased )                               // and not deceased... that is the spouse
                        )
                    );

            return FilterExpressionExtractor.Extract<Rock.Model.Person>( personSpouseQuery, parameterExpression, "p" );
        }
    }
}