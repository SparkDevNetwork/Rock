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
            //p.Members.Where(a => a.GroupRole.Guid == new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) )
            //    .Any( a => a.Group.Members
            //        .Any( c => c.GroupRole.Guid == new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD ) && idList.Contains( c.Person.Id ) ) ) );

            MethodInfo whereMethod = (MethodInfo)GetGenericMethod( "Where" );
            MethodInfo anyMethod = (MethodInfo)GetGenericMethod( "Any" );

            // p.Members
            MemberExpression adultMembers = Expression.Property( parameterExpression, "Members" );

            // a =>
            ParameterExpression adultMemberParameter = Expression.Parameter( typeof( Rock.Model.GroupMember ), "a" );

            // a.GroupRole
            MemberExpression adultGroupRole = Expression.Property( adultMemberParameter, "GroupRole" );

            // a.GroupRole.Guid
            MemberExpression adultGroupRoleGuid = Expression.Property( adultGroupRole, "Guid" );

            // [AdultRoleGuid]
            Expression adultRole = Expression.Constant( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) );

            // a.GroupRole.Guid == [AdultRoleGuid]
            Expression isAdult = Expression.Equal( adultGroupRoleGuid, adultRole );

            // a => a.GroupRole.Guid == [AdultRoleGuid]
            LambdaExpression adultLambda = Expression.Lambda<Func<Rock.Model.GroupMember, bool>>( isAdult, adultMemberParameter );

            // p.Members.where(a => a.GroupRole.Guid == [AdultRoleGuid])
            Expression whereAdult = Expression.Call( whereMethod, adultMembers, adultLambda );

            // c =>
            ParameterExpression childMemberParameter = Expression.Parameter( typeof( Rock.Model.GroupMember ), "c" );

            // c.GroupRole
            MemberExpression childGroupRole = Expression.Property( childMemberParameter, "GroupRole" );

            // c.GroupRole.Guid
            MemberExpression childGroupRoleGuid = Expression.Property( childGroupRole, "Guid" );

            // [ChildRoleGuid]
            Expression childRole = Expression.Constant( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD ) );

            // c.GroupRole.Guid == [ChildRoleGuid]
            Expression isChild = Expression.Equal( childGroupRoleGuid, childRole );

            // c.Person
            MemberExpression childPerson = Expression.Property( childMemberParameter, "Person" );

            // c.Person.Id
            MemberExpression childPersonId = Expression.Property( childPerson, "Id" );

            // [IdList]
            ConstantExpression ids = Expression.Constant( idQuery, typeof( IQueryable<int> ) );

            // [IdList].Contains(c.Person.Id)
            MethodCallExpression containsExpression = Expression.Call( typeof( Queryable ), "Contains", new Type[] { typeof( int ) }, ids, childPersonId );

            // c.GroupRole.Guid == [ChildRoleGuid] && [IdList].Contains(c.Person.Id)
            Expression andExpression = Expression.AndAlso( isChild, containsExpression );
            
            // a.Group
            Expression groupProperty = Expression.Property( adultMemberParameter, "Group" );

            // a.Group.Members
            Expression membersProperty = Expression.Property( groupProperty, "Members" );

            // c => c.GroupRole.Guid == [ChildRoleGuid] && [IdList].Contains(c.Person.Id)
            LambdaExpression anyChildLambda = Expression.Lambda<Func<Rock.Model.GroupMember, bool>>( andExpression, childMemberParameter );

            // a.Group.Members.Any(c => c.GroupRole.Guid == [ChildRoleGuid] && [IdList].Contains(c.Person.Id)
            Expression whereAnyChild = Expression.Call( anyMethod, membersProperty, anyChildLambda );

            // a => a.Group.Members.Any(c => c.GroupRole.Guid == [ChildRoleGuid] && [IdList].Contains(c.Person.Id)
            LambdaExpression anyAdultLambda = Expression.Lambda<Func<Rock.Model.GroupMember, bool>>( whereAnyChild, adultMemberParameter );

            // p.Members.where(a => a.GroupRole.Guid == [AdultRoleGuid]).Any(a => a.Group.Members.Any(c => c.GroupRole.Guid == [ChildRoleGuid] && [IdList].Contains(c.Person.Id))
            return Expression.Call( anyMethod, whereAdult, anyAdultLambda );
        }

        /// <summary>
        /// Gets the generic method.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
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