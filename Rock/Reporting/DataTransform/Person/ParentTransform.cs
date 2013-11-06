//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rock.Reporting.DataTransform.Person
{
    /// <summary>
    /// Person Parent Transformation
    /// </summary>
    [Description( "Transform result to Parents" )]
    [Export( typeof( DataTransformComponent ) )]
    [ExportMetadata( "ComponentName", "Person Parent Transformation" )]
    public class ParentTransform : DataTransformComponent<Rock.Model.Person>
    {
        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public override string Title
        {
            get { return "Parents"; }
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
        public override Expression GetExpression( object serviceInstance, Expression parameterExpression, Expression whereExpression )
        {
            MethodInfo getIdsMethod = serviceInstance.GetType().GetMethod( "GetIds", new Type[] { typeof( ParameterExpression ), typeof( Expression ) } );
            if ( getIdsMethod != null )
            {
                IQueryable<int> idQuery = (IQueryable<int>)getIdsMethod.Invoke( serviceInstance, new object[] { parameterExpression, whereExpression } );

                return BuildExpression( idQuery, parameterExpression );
            }

            return null;
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="personQueryable">The person queryable.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns></returns>
        public override Expression GetExpression( IQueryable<Rock.Model.Person> personQueryable, Expression parameterExpression )
        {
            return BuildExpression( personQueryable.Select( p => p.Id ), parameterExpression );
        }

        /// <summary>
        /// Builds the expression.
        /// </summary>
        /// <param name="idQuery">The id query.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns></returns>
        private Expression BuildExpression( IQueryable<int> idQuery, Expression parameterExpression )
        {
            Guid adultGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
            Guid childGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();

            var qry = new Rock.Data.Service<Rock.Model.Person>().Queryable()
                .Where( p => p.Members.Where( a => a.GroupRole.Guid == adultGuid )
                    .Any( a => a.Group.Members
                    .Any( c => c.GroupRole.Guid == childGuid && idQuery.Contains( c.PersonId ) ) ) );

            return FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );
        }
    }
}