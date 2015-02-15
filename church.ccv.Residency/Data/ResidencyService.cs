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
using System.Linq;
using com.ccvonline.Residency.Model;

namespace com.ccvonline.Residency.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResidencyService<T> : Rock.Data.Service<T> where T : Rock.Data.Entity<T>, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResidencyService{T}"/> class.
        /// </summary>
        /// <param name="residencyContext">The residency context.</param>
        public ResidencyService( ResidencyContext residencyContext )
            : base( residencyContext )
        {
            ResidencyContext = residencyContext;
        }

        /// <summary>
        /// Gets the residency context.
        /// </summary>
        /// <value>
        /// The residency context.
        /// </value>
        public ResidencyContext ResidencyContext { get; private set; }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( CompetencyPersonProjectAssessmentPointOfAssessment item, out string errorMessage )
        {
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( CompetencyPersonProjectAssessment item, out string errorMessage )
        {
            errorMessage = string.Empty;

            // NOTE: There isn't a CascadeDelete, but we will manually delete these
            /*
            if ( new ResidencyService<CompetencyPersonProjectAssessmentPointOfAssessment>().Queryable().Any( a => a.CompetencyPersonProjectAssessmentId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", CompetencyPersonProjectAssessment.FriendlyTypeName, CompetencyPersonProjectAssessmentPointOfAssessment.FriendlyTypeName );
                return false;
            }
             */

            return true;
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( CompetencyPersonProject item, out string errorMessage )
        {
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( CompetencyPerson item, out string errorMessage )
        {
            errorMessage = string.Empty;

            // NOTE: There isn't a CascadeDelete, but we will manually delete these
            /*
            if ( new ResidencyService<CompetencyPersonProject>().Queryable().Any( a => a.CompetencyPersonId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", CompetencyPerson.FriendlyTypeName, CompetencyPersonProject.FriendlyTypeName );
                return false;
            }
             */

            return true;
        }

        /// <summary>
        /// Deletes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="personId">The person unique identifier.</param>
        /// <returns></returns>
        public override bool Delete( T item )
        {
            // Manually delete child tables of CompetencyPerson, CompetencyPersonProject, and CompetencyPersonProjectAssessment due to CascadeDelete conflicts
            if ( typeof( T ) == typeof( CompetencyPerson ) )
            {
                CompetencyPerson competencyPerson = item as CompetencyPerson;
                var competencyPersonProjectService = new ResidencyService<CompetencyPersonProject>( this.ResidencyContext );
                foreach ( var rowId in competencyPerson.CompetencyPersonProjects.Select( a => a.Id ).ToList() )
                {
                    // recursively calls Delete( T item..)
                    var competencyPersonProject = competencyPersonProjectService.Get( rowId );
                    competencyPersonProjectService.Delete( competencyPersonProject);
                }
            }

            if ( typeof( T ) == typeof( CompetencyPersonProject ) )
            {
                CompetencyPersonProject competencyPersonProject = item as CompetencyPersonProject;
                var competencyPersonProjectAssessmentService = new ResidencyService<CompetencyPersonProjectAssessment>( this.ResidencyContext );

                foreach ( var rowId in competencyPersonProject.CompetencyPersonProjectAssessments.Select( a => a.Id ).ToList() )
                {
                    // recursively calls Delete( T item..)
                    var competencyPersonProjectAssessment = competencyPersonProjectAssessmentService.Get( rowId );
                    competencyPersonProjectAssessmentService.Delete( competencyPersonProjectAssessment );
                }
            }

            if ( typeof( T ) == typeof( CompetencyPersonProjectAssessment ) )
            {
                CompetencyPersonProjectAssessment competencyPersonProjectAssessment = item as CompetencyPersonProjectAssessment;
                var competencyPersonProjectAssessmentPointOfAssessmentService = new ResidencyService<CompetencyPersonProjectAssessmentPointOfAssessment>( this.ResidencyContext );

                foreach ( var rowId in competencyPersonProjectAssessment.CompetencyPersonProjectAssessmentPointOfAssessments.Select( a => a.Id ).ToList() )
                {
                    var competencyPersonProjectAssessmentPointOfAssessment = competencyPersonProjectAssessmentPointOfAssessmentService.Get( rowId );
                    competencyPersonProjectAssessmentPointOfAssessmentService.Delete( competencyPersonProjectAssessmentPointOfAssessment );
                }
            }

            return base.Delete( item );
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( Competency item, out string errorMessage )
        {
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( Period item, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( new ResidencyService<Track>(this.ResidencyContext).Queryable().Any( a => a.PeriodId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", Period.FriendlyTypeName, Track.FriendlyTypeName );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( ProjectPointOfAssessment item, out string errorMessage )
        {
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( Project item, out string errorMessage )
        {
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( Track item, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( new ResidencyService<Competency>(this.ResidencyContext).Queryable().Any( a => a.TrackId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", Track.FriendlyTypeName, Competency.FriendlyTypeName );
                return false;
            }

            return true;
        }
    }
}
