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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Reporting.DataTransform.Person
{
    /// <summary>
    /// Person Spouse Transformation
    /// </summary>
    [Description( "Transform result to Spouse" )]
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
            get { return "Spouse"; }
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
            var personService = serviceInstance as Service<Rock.Model.Person>;

            IQueryable<Model.Person> personQuery = personService.Get( parameterExpression, whereExpression, null, null );

            return BuildExpression( serviceInstance, personQuery, parameterExpression );
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
            return BuildExpression( serviceInstance, personQueryable, parameterExpression );
        }

        /// <summary>
        /// Builds the expression.
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="personQuery">The person query.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns></returns>
        private Expression BuildExpression( IService serviceInstance, IQueryable<Rock.Model.Person> personQuery, ParameterExpression parameterExpression )
        {
            int adultRoleId = GroupTypeCache.GetFamilyGroupType().Roles.Where( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Select( a => a.Id ).FirstOrDefault();
            int marriedDefinedValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid() ).Id;

            IQueryable<Model.Person> qry = new PersonService( ( RockContext ) serviceInstance.Context ).Queryable()
                .Where( p =>
                        // spouse of person is Married
                        p.MaritalStatusValueId == marriedDefinedValueId

                        // spouse of person is Adult
                        && p.Members.Where( sgm =>
                            sgm.GroupRoleId == adultRoleId
                        )
                    .Any( a => a.Group.Members
                    .Any( pgm =>
                        // person is adult
                        pgm.GroupRoleId == adultRoleId

                        && personQuery.Any( origPerson =>
                            // person is in the original personQuery 
                            origPerson.Id == pgm.PersonId

                            // person is married
                            && origPerson.MaritalStatusValueId == marriedDefinedValueId

                            // person is not the same gender as spouse (if gender is known)
                            && ( ( origPerson.Gender != p.Gender ) || origPerson.Gender == Gender.Unknown || p.Gender == Gender.Unknown )

                            // person isn't one of the spouses we are returning
                            && origPerson.Id != p.Id )
                        )
                    )
                    );



            return FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );
        }
    }
}