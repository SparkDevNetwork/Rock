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
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

using Rock.Data;
using Rock.Utility;

namespace Rock.Model
{
    public partial class LearningClassService
    {
        /// <summary>
        /// Gets the related <see cref="GroupType"/> for the <see cref="LearningClass"/> with the specified id key.
        /// </summary>
        /// <param name="idKey">The idKey of the <see cref="LearningClass"/> for which to retrieve roles.</param>
        /// <returns>A list of GroupTypeRoles for the class.</returns>
        public IQueryable<GroupTypeRole> GetClassRoles( string idKey )
        {
            var id = IdHasher.Instance.GetId( idKey ).ToIntSafe();
            return id > 0 ? GetClassRoles( id ) : Array.Empty<GroupTypeRole>().AsQueryable();
        }

        /// <summary>
        /// Gets the related <see cref="GroupType"/> for the <see cref="LearningClass"/> with the specified id.
        /// </summary>
        /// <param name="idKey">The idKey of the <see cref="LearningClass"/> for which to retrieve roles.</param>
        /// <returns>A list of GroupTypeRoles for the class.</returns>
        public IQueryable<GroupTypeRole> GetClassRoles( int id )
        {
            var groupTypeId = Queryable().Where( c => c.Id == id ).Select( c => c.GroupTypeId ).FirstOrDefault();

            return
                groupTypeId > 0 ?
                new GroupTypeRoleService( ( RockContext ) Context ).GetByGroupTypeId( groupTypeId ).OrderBy( t => t.Order ) :
                Array.Empty<GroupTypeRole>().AsQueryable();
        }

        /// <summary>
        /// Gets the Default <see cref="LearningClass"/> for the specified id key of the <see cref="LearningCourse"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="courseIdKey">The id key of the <see cref="Rock.Model.LearningCourse" /> to retrieve the default class for.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The entity containing the provided selected properties for the default class.
        /// </returns>
        public TResult GetCourseDefaultClass<TResult>( string courseIdKey, Expression<Func<LearningClass, TResult>> selector )
        {
            var courseId = IdHasher.Instance.GetId( courseIdKey ).ToIntSafe();
            if (courseId == 0)
            {
                return default;
            }

            return GetCourseDefaultClass( courseId, selector );
        }

        /// <summary>
        /// Gets the Default <see cref="LearningClass"/> for the specified identifier of the <see cref="LearningCourse"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="courseId">The identifier of the <see cref="Rock.Model.LearningCourse" /> to retrieve the default class for.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The entity containing the provided selected properties for the default class.
        /// </returns>
        public TResult GetCourseDefaultClass<TResult>( int courseId, Expression<Func<LearningClass, TResult>> selector )
        {
            var course = new LearningCourseService( ( RockContext ) Context )
                .Queryable()
                .Include( c => c.LearningClasses )
                .FirstOrDefault( c => c.Id == courseId );

            if ( course == null || course.LearningClasses == null )
            {
                return default;
            }

            return GetCourseDefaultClass(course, selector );
        }

        /// <summary>
        /// Gets the default <see cref="LearningClass"/> for the specified <see cref="LearningCourse"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="course">The <see cref="Rock.Model.LearningCourse" /> to retrieve the default class for.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The entity containing the provided selected properties for the default class.
        /// </returns>
        public TResult GetCourseDefaultClass<TResult>( LearningCourse course, Expression<Func<LearningClass, TResult>> selector )
        {

            if ( course == null || course.LearningClasses == null )
            {
                return default;
            }

            return course.LearningClasses
                .Where( c => c.IsActive )
                .OrderBy( t => t.Order )
                .AsQueryable()
                .DefaultIfEmpty(new LearningClass())
                .Select( selector )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the list of <see cref="LearningActivity"/> for the specified <see cref="LearningClass"/> id key.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="classIdKey">The id key of the <see cref="Rock.Model.LearningClass" /> to retrieve the list of <see cref="LearningActivity"/> for.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The entity containing the selected properties for the list of activities.
        /// </returns>
        public IQueryable<TResult> GetLearningPlan<TResult>( string classIdKey, Expression<Func<LearningActivity, TResult>> selector )
        {
            var classId = IdHasher.Instance.GetId( classIdKey ).ToIntSafe();
            if ( classId == 0 )
            {
                return default;
            }

            return GetLearningPlan( classId, selector );
        }

        /// <summary>
        /// Gets the list of <see cref="LearningActivity"/> for the specified <see cref="LearningClass"/> identifier.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="classId">The identifier of the <see cref="Rock.Model.LearningClass" /> to retrieve the list of <see cref="LearningActivity"/> for.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The entity containing the selected properties for the list of activities.
        /// </returns>
        public IQueryable<TResult> GetLearningPlan<TResult>( int classId, Expression<Func<LearningActivity, TResult>> selector )
        {
            var learningClass = Queryable()
                .Include( c => c.LearningActivities )
                .FirstOrDefault( c => c.Id == classId );

            if ( learningClass == null || learningClass.LearningActivities == null )
            {
                return default;
            }

            return GetLearningPlan( learningClass, selector );
        }

        /// <summary>
        /// Gets the list of <see cref="LearningActivity"/> for the specified <see cref="LearningClass"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="learningClass">The <see cref="Rock.Model.LearningClass" /> to retrieve the list of <see cref="LearningActivity"/> for.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The entity containing the selected properties for the list of activities.
        /// </returns>
        public IQueryable<TResult> GetLearningPlan<TResult>( LearningClass learningClass, Expression<Func<LearningActivity, TResult>> selector )
        {
            if ( learningClass == null || learningClass.LearningActivities == null )
            {
                return default;
            }

            return learningClass.LearningActivities
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Id )
                .AsQueryable()
                .DefaultIfEmpty( new LearningActivity() )
                .Select( selector );
        }
    }
}