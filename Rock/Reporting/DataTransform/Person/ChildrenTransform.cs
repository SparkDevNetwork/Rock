// <copyright>
// Copyright 2013 by the Spark Development Network
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
using Rock.Data;
using Rock.Model;

namespace Rock.Reporting.DataTransform.Person
{
    /// <summary>
    /// Person Children Transformation
    /// </summary>
    [Description( "Transform result to Children" )]
    [Export( typeof( DataTransformComponent ) )]
    [ExportMetadata( "ComponentName", "Person Children Transformation" )]
    public class ChildrenTransform : DataTransformComponent<Rock.Model.Person>
    {
        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public override string Title
        {
            get { return "Children"; }
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
        /// Builds the expression.
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="idQuery">The id query.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns></returns>
        private Expression BuildExpression( IService serviceInstance, IQueryable<int> idQuery, ParameterExpression parameterExpression )
        {
            Guid adultGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
            Guid childGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();

            var qry = new PersonService( (RockContext)serviceInstance.Context ).Queryable()
                .Where( p => p.Members.Where( a => a.GroupRole.Guid == childGuid )
                    .Any( a => a.Group.Members
                    .Any( c => c.GroupRole.Guid == adultGuid && idQuery.Contains( c.PersonId ) ) ) );

            return FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );
        }
    }
}