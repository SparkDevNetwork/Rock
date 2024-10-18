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
using System.Collections.Generic;
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
        /// Determines if the <see cref="LearningClass"/> should allow updates to the <see cref="LearningGradingSystem"/>.
        /// </summary>
        /// <param name="learningClassId">The identifier of the learning class to check.</param>
        /// <returns><c>true</c> if updates should be allowed;otherwise <c>false</c>.</returns>
        public bool CanUpdateGradingSystem( int learningClassId )
        {
            return !Queryable()
                .Where( c => c.Id == learningClassId )
                .Any( c => c.LearningParticipants.Any() );
        }

        /// <summary>
        /// Creates a new <see cref="LearningClass" /> with Attributes by copying values from the specified learning class.
        /// </summary>
        /// <param name="id">The identifer to use for retreiving the <see cref="LearningClass"/> to use as a template for the copy.</param>
        /// <returns>
        ///     A new class whose properties and <see cref="AttributeValue" />s match the properties and <see cref="AttributeValue" />s
        ///     of the class whose <paramref name="id"/> was provided.
        /// </returns>
        public LearningClass Copy( int id )
        {
            var learningClass = Get( id );

            return Copy( learningClass );
        }

        /// <summary>
        /// Creates a new <see cref="LearningClass" /> with Attributes by copying values from the specified learning class.
        /// </summary>
        /// <param name="key">The identifer to use for retreiving the <see cref="LearningClass"/> to use as a template for the copy.</param>
        /// <returns>
        ///     A new class whose properties and <see cref="AttributeValue" />s match the properties and <see cref="AttributeValue" />s
        ///     of the class whose <paramref name="key"/> was provided.
        /// </returns>
        public LearningClass Copy( string key )
        {
            var learningClass = Get( key );

            return Copy( learningClass );
        }

        /// <summary>
        /// Creates a new <see cref="LearningClass" /> with Attributes by copying values from the specified learning class.
        /// </summary>
        /// <param name="learningClass">The <see cref="LearningClass"/> to use as a template for the copy.</param>
        /// <param name="includeActivities"><c>true</c> if activities should also be copied.</param>
        /// <returns>
        ///     A new class whose properties and <see cref="AttributeValue" />s match the properties and <see cref="AttributeValue" />s
        ///     of the provided class.
        /// </returns>
        public LearningClass Copy( LearningClass learningClass, bool includeActivities = true )
        {
            var newLearningClass = learningClass.CloneWithoutIdentity();
            newLearningClass.Name += " - Copy";
            this.Add( newLearningClass );
            learningClass.LoadAttributes();
            newLearningClass.LoadAttributes();
            newLearningClass.CopyAttributesFrom( learningClass );

            var rockContext = this.Context as RockContext;
            var newActivities = new List<LearningActivity>();

            var contentPages = new LearningClassContentPageService( rockContext )
                .Queryable()
                .Where( c => c.LearningClassId == learningClass.Id )
                .ToList();

            foreach ( var contentPage in contentPages )
            {
                newLearningClass.ContentPages.Add( contentPage.CloneWithoutIdentity() );
            }

            // If we're also copying activities populate a list of new activities.
            if ( includeActivities )
            {
                var activityService = new LearningActivityService( rockContext );
                var activities = activityService.Queryable().Where( c => c.LearningClassId == learningClass.Id ).ToList();

                foreach ( var activity in activities )
                {
                    var newActivity = activity.CloneWithoutIdentity();
                    newActivity.LearningClassId = newLearningClass.Id;
                    activityService.Add( newActivity );
                    activity.LoadAttributes();
                    newActivity.LoadAttributes();
                    newActivity.CopyAttributesFrom( activity );
                }
            }

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();
                newLearningClass.SaveAttributeValues( rockContext );

                // If there are new activities save the attributes for those as well.
                foreach ( var newActivity in newActivities )
                {
                    rockContext.SaveChanges();
                    newActivity.SaveAttributeValues( rockContext );
                }
            } );

            return newLearningClass;
        }

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
        /// <param name="id">The idKey of the <see cref="LearningClass"/> for which to retrieve roles.</param>
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
        /// Gets the <see cref="LearningGradingSystemScale"/> list for the specified <see cref="LearningClass"/>.
        /// </summary>
        /// <param name="id">The identifier of the Learning Class to retrieve scales for.</param>
        /// <returns>A Queryable of LearningGradingSystemScales for the specified class.</returns>
        public IQueryable<LearningGradingSystemScale> GetClassScales( int id )
        {
            return Queryable()
                .AsNoTracking()
                .Include( a => a.LearningGradingSystem )
                .Include( a => a.LearningGradingSystem.LearningGradingSystemScales )
                .Where( a => a.Id == id )
                .SelectMany( a => a.LearningGradingSystem.LearningGradingSystemScales );
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
            if ( courseId == 0 )
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

            return GetCourseDefaultClass( course, selector );
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
                .DefaultIfEmpty( new LearningClass() )
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

        /// <summary>
        /// Gets the <see cref="LearningClass"/> class records for the specified person.
        /// Where the person is in a student role.
        /// </summary>
        /// <param name="personId">The identifier of the <see cref="Person"/> for which to retrieve classes.</param>
        /// <returns>A list of <see cref="LearningClass"/> for the specified <see cref="Person"/>.</returns>
        public IQueryable<LearningClass> GetStudentClasses( int personId )
        {
            return Queryable()
                .Include( c => c.LearningCourse )
                .Where( c =>
                    c.LearningParticipants.Any( p =>
                        p.PersonId == personId &&
                        p.GroupRole.IsLeader == false )
                    );
        }

        /// <summary>
        /// Gets the active classes for the specified program.
        /// </summary>
        /// <param name="programIdKey">The identifier key of the <see cref="LearningProgram"/> to retrieve active classes for.</param>
        /// <returns>An IQueryable of LearningClasses that are considered 'Active'.</returns>
        public IQueryable<LearningClass> GetActiveClasses( string programIdKey )
        {
            if (int.TryParse(programIdKey, out var programId ) )
            {
                GetActiveClasses( programId );
            }

            var idFromHash = IdHasher.Instance.GetId( programIdKey ).ToIntSafe();

            return idFromHash > 0 ? GetActiveClasses( idFromHash ) : Queryable();
        }

        /// <summary>
        /// Gets the active classes for the specified program.
        /// </summary>
        /// <param name="programId">The identifier of the <see cref="LearningProgram"/> to retrieve active classes for.</param>
        /// <returns>An IQueryable of LearningClasses that are considered 'Active'.</returns>
        public IQueryable<LearningClass> GetActiveClasses( int programId )
        {
            var now = RockDateTime.Now;
            return Queryable()
                .Where( c =>
                    c.LearningCourse.LearningProgramId == programId
                    && c.IsActive
                    && ( !c.LearningSemester.EndDate.HasValue || c.LearningSemester.EndDate > now )
                );
        }
    }
}