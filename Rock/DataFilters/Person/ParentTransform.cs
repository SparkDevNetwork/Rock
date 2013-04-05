//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rock.DataFilters.Person
{
    /// <summary>
    /// Person Parent Transformation
    /// </summary>
    [Description( "Transform result to Parents" )]
    [Export( typeof( DataTransformComponent ) )]
    [ExportMetadata( "ComponentName", "Person Parent Transformation" )]
    public class ParentTransform : DataTransformComponent
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

                MethodInfo whereMethod = (MethodInfo)GetGenericMethod( "Where" );
                MethodInfo anyMethod = (MethodInfo)GetGenericMethod( "Any" );

                MemberExpression adultMembers = Expression.Property( parameterExpression, "Members" );

                ParameterExpression adultMemberParameter = Expression.Parameter( typeof( Rock.Model.GroupMember ), "a" );
                MemberExpression adultGroupRole = Expression.Property( adultMemberParameter, "GroupRole" );
                MemberExpression adultGroupRoleGuid = Expression.Property( adultGroupRole, "Guid" );
                Expression adultRole = Expression.Constant( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) );
                Expression isAdult = Expression.Equal( adultGroupRoleGuid, adultRole );
                LambdaExpression adultLambda = Expression.Lambda<Func<Rock.Model.GroupMember, bool>>( isAdult, adultMemberParameter );
                Expression whereAdult = Expression.Call( whereMethod, adultMembers, adultLambda );

                ParameterExpression childMemberParameter = Expression.Parameter( typeof( Rock.Model.GroupMember ), "c" );
                MemberExpression childGroupRole = Expression.Property( childMemberParameter, "GroupRole" );
                MemberExpression childGroupRoleGuid = Expression.Property( childGroupRole, "Guid" );
                Expression childRole = Expression.Constant( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD ) );
                Expression isChild = Expression.Equal( childGroupRoleGuid, childRole );

                MemberExpression childPerson = Expression.Property( childMemberParameter, "Person" );
                MemberExpression childPersonId = Expression.Property( childPerson, "Id" );

                ConstantExpression ids = Expression.Constant( idQuery, typeof( IQueryable<int> ) );
                MethodCallExpression containsExpression = Expression.Call( typeof( Queryable ), "Contains", new Type[] { typeof( int ) }, ids, childPersonId );

                Expression andExpression = Expression.AndAlso( isChild, containsExpression );

                Expression groupProperty = Expression.Property( adultMemberParameter, "Group" );
                Expression membersProperty = Expression.Property( groupProperty, "Members" );

                LambdaExpression anyChildLambda = Expression.Lambda<Func<Rock.Model.GroupMember, bool>>( andExpression, childMemberParameter );
                Expression whereAnyChild = Expression.Call( anyMethod, membersProperty, anyChildLambda );

                LambdaExpression anyAdultLambda = Expression.Lambda<Func<Rock.Model.GroupMember, bool>>( whereAnyChild, adultMemberParameter );
                return  Expression.Call( anyMethod, whereAdult, anyAdultLambda );
            }

            return null;

        }

        private MethodBase GetGenericMethod( string name )
        {
            var methods = typeof( Enumerable ).GetMethods()
                .Where( m => m.Name == name )
                .Where( m => m.GetGenericArguments().Length == 1 )
                .Select( m => m.MakeGenericMethod( new Type[] { typeof( Rock.Model.GroupMember ) } ) );

            return Type.DefaultBinder.SelectMethod( BindingFlags.Static, methods.ToArray(), new Type[] { typeof( IEnumerable<Rock.Model.GroupMember> ), typeof( Func<Rock.Model.GroupMember, bool> ) }, null );
        }



    }
}