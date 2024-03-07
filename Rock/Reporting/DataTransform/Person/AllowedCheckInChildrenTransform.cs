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
using System.Linq;
using System.Linq.Expressions;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Reporting.DataTransform.Person
{
    /// <summary>
    /// Allowed CheckIn Children Transform, converts a list of people to a list of kids that these individuals are allowed to check-in.
    /// </summary>
    [Description( "Transform result to list of children they are allowed to check-in" )]
    [Export( typeof( DataTransformComponent ) )]
    [ExportMetadata( "ComponentName", "Person Allowed Check-In Children Transformation" )]
    [Rock.SystemGuid.EntityTypeGuid( "EB3513E4-5CCB-4C6D-8FB5-874A2DC77011")]
    public class AllowedCheckInChildrenTransform : DataTransformComponent<Rock.Model.Person>
    {
        private static class AttributeKey
        {
            public const string CanCheckIn = "CanCheckin";
        }

        /// <inheritdoc/>
        public override string Title
        {
            get { return "Allowed Check-In Children"; }
        }

        /// <inheritdoc/>
        public override string TransformedEntityTypeName
        {
            get { return "Rock.Model.Person"; }
        }

        /// <inheritdoc/>
        public override Expression GetExpression( IService serviceInstance, IQueryable<Model.Person> query, ParameterExpression parameterExpression )
        {
            return BuildExpression( serviceInstance, query.Select( p => p.Id ), parameterExpression );
        }

        /// <inheritdoc/>
        public override Expression GetExpression( IService service, ParameterExpression parameterExpression, Expression whereExpression )
        {
            IQueryable<int> idQuery = service.GetIds( parameterExpression, whereExpression );
            return BuildExpression( service, idQuery, parameterExpression );
        }

        /// <summary>
        /// Builds the expression.
        /// </summary>
        /// <param name="service">The service instance.</param>
        /// <param name="idQuery">The id query.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns></returns>
        private Expression BuildExpression( IService service, IQueryable<int> idQuery, ParameterExpression parameterExpression )
        {
            int adultRoleId = GroupTypeCache.GetFamilyGroupType().Roles.Where( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Select( a => a.Id ).FirstOrDefault();
            int childRoleId = GroupTypeCache.GetFamilyGroupType().Roles.Where( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ).Select( a => a.Id ).FirstOrDefault();
            var rockContext = ( RockContext ) service.Context;

            var groupTypeRoles = new GroupTypeRoleService( rockContext ).Queryable().AsEnumerable();

            groupTypeRoles.LoadAttributes( rockContext );

            var canCheckInRoleIds = groupTypeRoles.Where( r => r.GetAttributeValue( AttributeKey.CanCheckIn ).AsBoolean() ).Select( r => r.Id );

            // Get the family members of the selected people.
            var personsAndFamilyMemberIdsQry = new PersonService( rockContext ).Queryable()
                .Where( p => p.Members.Any( a => ( a.GroupRoleId == adultRoleId || a.GroupRoleId == childRoleId )
                    && !a.IsArchived
                    && a.Group.Members.Any( c => ( c.GroupRoleId == childRoleId || c.GroupRoleId == adultRoleId ) && idQuery.Contains( c.PersonId ) ) ) )
                .Select( p => p.Id );

            // Get the selected people and their family members and children in groups with them who can be checked in by them.
            var qry = new PersonService( rockContext ).Queryable()
                .Where( p => ( personsAndFamilyMemberIdsQry.Any( id => id == p.Id ) && p.Members.Any( a => a.GroupRoleId == childRoleId ) ) // is a selected person or a selected person's family member and a child.
                    || ( p.Members.Any( a => a.GroupRoleId == childRoleId  ) // OR is a child 
                        && p.Members.Any( a => canCheckInRoleIds.Contains( a.GroupRoleId ) // and a member of any of the groups that can check in others.
                        && !a.IsArchived // and is not archived.
                        && a.Group.Members.Any( b => personsAndFamilyMemberIdsQry.Any( id => id == b.PersonId ) ) ) ) ); // and is in said CanCheckIn group with any of the selected people or a member of their family.

            return FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );
        }
    }
}
