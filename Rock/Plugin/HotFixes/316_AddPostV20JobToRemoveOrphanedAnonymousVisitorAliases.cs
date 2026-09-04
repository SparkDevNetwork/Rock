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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Registers the run-once job that removes orphaned Anonymous Visitor person
    /// alias records left behind by bot page views.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 316, "20.0" )]
    public class AddPostV20JobToRemoveOrphanedAnonymousVisitorAliases : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            NA_AddPostUpdateJobToRemoveOrphanedAnonymousVisitorAliases();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /*
            8/24/2026 - CLAUDE

            The deletion itself is deliberately not done here. Plugin migrations
            run during the startup migration phase, so removing a backlog that
            reaches six figures on a large site, with a per-record retry for any
            foreign key violation, would directly extend the restart after an
            install.

            Registering the work as a post-update job costs one ServiceJob insert
            at migration time. DataMigrationsStartup then starts the job inside a
            Task.Run, so the deleting happens in the background.

            Reason: Keep a potentially long cleanup off the startup path.
        */

        /// <summary>
        /// Add the run-once, post update job that removes Anonymous Visitor person alias
        /// records with no interactions. These accumulated because the crawler check ran
        /// after the alias had already been committed. This job will be added to the
        /// ServiceJob table during the post update process of the v20.0 update, and its
        /// guid is also added to startup so that it runs shortly after start.
        /// See: Rock.Migrations.RockStartup.DataMigrationsStartup />
        /// </summary>
        private void NA_AddPostUpdateJobToRemoveOrphanedAnonymousVisitorAliases()
        {
            // Note: This cronExpression was chosen at random. It is provided as it is mandatory in the Service Job. Feel free to change it if needed.
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v20.0 - Remove Orphaned Anonymous Visitor Aliases",
                description: "This job removes Anonymous Visitor PersonAlias records that were created for bot traffic whose page view interaction was subsequently discarded as a crawler. After all the operations are done, this job will delete itself.",
                jobType: typeof( Rock.Jobs.PostV20RemoveOrphanedAnonymousVisitorAliases ).FullName,
                cronExpression: "0 0 20 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_200_REMOVE_ORPHANED_ANONYMOUS_VISITOR_ALIASES );
        }
    }
}
