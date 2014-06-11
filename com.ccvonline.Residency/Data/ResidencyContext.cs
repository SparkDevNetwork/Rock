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
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.ccvonline.Residency.Model;

namespace com.ccvonline.Residency.Data
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ResidencyContext : Rock.Data.DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResidencyContext"/> class.
        /// </summary>
        public ResidencyContext()
            : base( "RockContext" )
        {
            // intentionally left blank
        }

        /// <summary>
        /// This method is called when the model for a derived context has been initialized, but
        /// before the model has been locked down and used to initialize the context.  The default
        /// implementation of this method does nothing, but it can be overridden in a derived class
        /// such that the model can be further configured before it is locked down.
        /// </summary>
        /// <param name="modelBuilder">The builder that defines the model for the context being created.</param>
        /// <remarks>
        /// Typically, this method is called only once when the first instance of a derived context
        /// is created.  The model for that context is then cached and is for all further instances of
        /// the context in the app domain.  This caching can be disabled by setting the ModelCaching
        /// property on the given ModelBuidler, but note that this can seriously degrade performance.
        /// More control over caching is provided through use of the DbModelBuilder and DbContextFactory
        /// classes directly.
        /// </remarks>
        protected override void OnModelCreating( DbModelBuilder modelBuilder )
        {
            // we don't want this context to create a database or look for EF Migrations, do set the Initializer to null
            Database.SetInitializer<ResidencyContext>( new NullDatabaseInitializer<ResidencyContext>() );

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Configurations.AddFromAssembly( typeof( ResidencyContext ).Assembly );
        }
        
        #region Models

        /// <summary>
        /// Gets or sets the residency competencies.
        /// </summary>
        /// <value>
        /// The residency competencies.
        /// </value>
        public DbSet<Competency> ResidencyCompetencies { get; set; }

        /// <summary>
        /// Gets or sets the residency competency persons.
        /// </summary>
        /// <value>
        /// The residency competency persons.
        /// </value>
        public DbSet<CompetencyPerson> CompetencyPersons { get; set; }

        /// <summary>
        /// Gets or sets the residency competency person projects.
        /// </summary>
        /// <value>
        /// The residency competency person projects.
        /// </value>
        public DbSet<CompetencyPersonProject> CompetencyPersonProjects { get; set; }

        /// <summary>
        /// Gets or sets the competency person project assessments.
        /// </summary>
        /// <value>
        /// The competency person project assessments.
        /// </value>
        public DbSet<CompetencyPersonProjectAssessment> CompetencyPersonProjectAssessments { get; set; }

        /// <summary>
        /// Gets or sets the competency person project assessment point of assessments.
        /// </summary>
        /// <value>
        /// The competency person project assessment point of assessments.
        /// </value>
        public DbSet<CompetencyPersonProjectAssessmentPointOfAssessment> CompetencyPersonProjectAssessmentPointOfAssessments { get; set; }
        
        /// <summary>
        /// Gets or sets the residency periods.
        /// </summary>
        /// <value>
        /// The residency periods.
        /// </value>
        public DbSet<Period> Periods { get; set; }

        /// <summary>
        /// Gets or sets the residency projects.
        /// </summary>
        /// <value>
        /// The residency projects.
        /// </value>
        public DbSet<Project> Projects { get; set; }

        /// <summary>
        /// Gets or sets the residency project point of assessments.
        /// </summary>
        /// <value>
        /// The residency project point of assessments.
        /// </value>
        public DbSet<ProjectPointOfAssessment> ProjectPointOfAssessments { get; set; }

        /// <summary>
        /// Gets or sets the residency tracks.
        /// </summary>
        /// <value>
        /// The residency tracks.
        /// </value>
        public DbSet<Track> Tracks { get; set; }
        
        #endregion
    }
}
