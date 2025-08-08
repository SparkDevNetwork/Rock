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
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;

using DocumentFormat.OpenXml.Spreadsheet;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v18.0 to delete the deprecated GroupLocationHistoricalSchedule table from the database.
    /// </summary>
    [DisplayName( "Rock Update Helper v18.0 - Delete Self-Service Kiosk Site and Blocks" )]
    [Description( "This job will delete the deprecated Self-Service Kiosk (Preview) website, the corresponding 'Kiosk' blocks, and KioskStark theme from the database and file system. See https://www.rockrms.com/tech-bulletin/removal-of-obsoleted-kiosk-blocks for details." )]

    [IntegerField( "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete.",
        IsRequired = false,
        DefaultIntegerValue = 300 )]

    public class PostV18DeleteSelfServiceKioskSiteAndBlocks : RockJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary>
        /// The command timeout for database operations.
        /// </summary>
        private int _commandTimeout = 300; // Default to 5 minutes (300 seconds)

        /// <summary>
        /// A custom message to include in the Email notification that is sent out.
        /// See SendNotificationMessage() below for usage.
        /// </summary>
        private string _customMessage = "";

        /// <inheritdoc />
        public override void Execute()
        {
            try
            {
                _commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 300;

                this.UpdateLastStatusMessage( "Checking for 'Kiosk' blocks used outside of the Self-Service Kiosk (Preview) site..." );
                CheckForBlockTypeUseOutsidPreviewSite();

                this.UpdateLastStatusMessage( "Checking for other sites that are using pages from the Self-Service Kiosk (Preview) site..." );
                CheckForPreviewSitePagesUsedByOtherSites();

                this.UpdateLastStatusMessage( "Deleting 'Kiosk' blocks and the Self-Service Kiosk (Preview) site..." );
                DeleteBlocksPagesAndBlockTypesAndPreviewSite();

                this.UpdateLastStatusMessage( "Deleting the KioskStark theme folder and 'Kiosk' block type files from the file system..." );
                DeleteThemeAndBlockTypeFilesFromFileSystem();

                DeleteJob();
            }
            catch ( System.Exception ex )
            {
                SendNotificationMessage( ex, this.ServiceJob );
                var exceptionList = new AggregateException( "One or more exceptions occurred while trying to remove the Self-Service Kiosk website and blocks.", ex );
                throw new RockJobWarningException( "PostV18DeleteSelfServiceKioskSiteAndBlocks job completed with warnings", exceptionList );
            }
        }

        /// <summary>
        /// Checks for 'Kiosk' blocks that are used outside of the Self-Service Kiosk (Preview) site.
        /// </summary>
        private void CheckForBlockTypeUseOutsidPreviewSite()
        {

            // Count of Pages that are using the blocks in question that are outside the Self-Service Kiosk (Preview) site.
            string checkForUseOutsidePreviewSiteQuery = @"
DECLARE @SiteId INT = (SELECT TOP 1 [Id] FROM [Site] WHERE [Guid] = '05e96f7b-b75e-4987-825a-b6f51f8d9caa'); -- Self-Service Kiosk (Preview) Site

SELECT COUNT(p.Id) FROM [Page] p
INNER JOIN [Block] b ON b.[PageId] = p.[Id]
INNER JOIN [BlockType] bt ON bt.[Id] = b.[BlockTypeId]
WHERE bt.[Guid] IN (
	'61C5C8F2-6F76-4583-AB97-228878A6AB65', -- PersonUpdate.Kiosk.ascx 
	'D10900A8-C2C1-4414-A443-3781A5CF371C', -- TransactionEntry.Kiosk.ascx 
	'9D8ED334-F1F5-4377-9E27-B8C0852CF34D' -- PrayerRequestEntry.Kiosk.ascx 
)
AND p.[SiteId] != @SiteId
";
            int count = Convert.ToInt32( DbService.ExecuteScalar( checkForUseOutsidePreviewSiteQuery, CommandType.Text, null, _commandTimeout ) );
            if ( count > 0 )
            {
                var statusMessage = $"There {( count == 1 ? "is" : "are" )} {count} page{( count == 1 ? "" : "s" )} using the 'Kiosk' blocks outside of the Self-Service Kiosk (Preview) site. Please remove {( count == 1 ? "that block from that page" : "those blocks from those pages" )} before running this job.";
                this.UpdateLastStatusMessage( statusMessage );
                _customMessage = "is using the blocks listed below outside of the Self-Service Kiosk (Preview) site";
                throw new RockJobWarningException( statusMessage );
            }
        }

        /// <summary>
        /// Checks for pages in the Self-Service Kiosk (Preview) site that are using the 'Kiosk' blocks outside of that site.
        /// </summary>
        /// <exception cref="RockJobWarningException"></exception>
        private void CheckForPreviewSitePagesUsedByOtherSites()
        {
            string checkForPreviewSitePagesUsedByOtherSites = @"
DECLARE @SiteId INT = (SELECT TOP 1 [Id] FROM [Site] WHERE [Guid] = '05e96f7b-b75e-4987-825a-b6f51f8d9caa'); -- Self-Service Kiosk (Preview) Site
DECLARE @SitePageIds TABLE (PageId INT);

INSERT INTO @SitePageIds (PageId)
SELECT [Id]
FROM [Page]
WHERE [SiteId] = @SiteId;

-- Find any OTHER [Site] records referencing those Pages
SELECT s.[Name]
FROM [Site] s
WHERE
(
    s.[DefaultPageId] IN (SELECT PageId FROM @SitePageIds) OR
    s.[LoginPageId] IN (SELECT PageId FROM @SitePageIds) OR
    s.[RegistrationPageId] IN (SELECT PageId FROM @SitePageIds) OR
    s.[PageNotFoundPageId] IN (SELECT PageId FROM @SitePageIds) OR
    s.[CommunicationPageId] IN (SELECT PageId FROM @SitePageIds) OR
    s.[MobilePageId] IN (SELECT PageId FROM @SitePageIds) OR
    s.[ChangePasswordPageId] IN (SELECT PageId FROM @SitePageIds)
)
AND [Id] != @SiteId;
";
            var siteNamesTable = DbService.GetDataTable( checkForPreviewSitePagesUsedByOtherSites, CommandType.Text, null, _commandTimeout );
            if ( siteNamesTable.Rows.Count > 0 )
            {
                var siteNames = siteNamesTable.Rows.Cast<DataRow>()
                    .Select( r => r["Name"].ToString() )
                    .ToList();

                var siteList = string.Join( ", ", siteNames );
                var statusMessage = $"The following site{( siteNames.Count == 1 ? "" : "s" )} reference{( siteNames.Count == 1 ? "s" : "" )} page settings (such as LoginPage, RegistrationPage, MobilePage, etc.) from the Self-Service Kiosk (Preview) site: {siteList}. Please unlink these references before running this job.";
                this.UpdateLastStatusMessage( statusMessage );
                _customMessage = "has one or more sites that are using a page that belongs the Self-Service Kiosk (Preview) site which is going to be deleted";
                throw new RockJobWarningException( statusMessage );
            }
        }

        /// <summary>
        /// Deletes the 'Kiosk' blocks, pages, block types, and the Self-Service Kiosk (Preview) site from the database.
        /// </summary>
        private void DeleteBlocksPagesAndBlockTypesAndPreviewSite()
        {
            string queryToDeleteBlocksPagesAndBlockTypes = @"
DECLARE @SiteId INT = (SELECT TOP 1 [Id] FROM [Site] WHERE [Guid] = '05e96f7b-b75e-4987-825a-b6f51f8d9caa'); -- Self-Service Kiosk (Preview) Site

-- Step 1: Delete from Block
DELETE FROM [Block]
WHERE [BlockTypeId] IN (
	SELECT [Id]
	FROM [BlockType]
	WHERE [Guid] IN (
		'61C5C8F2-6F76-4583-AB97-228878A6AB65', -- PersonUpdate.Kiosk.ascx 
		'D10900A8-C2C1-4414-A443-3781A5CF371C', -- TransactionEntry.Kiosk.ascx 
		'9D8ED334-F1F5-4377-9E27-B8C0852CF34D'  -- PrayerRequestEntry.Kiosk.ascx 
	)
);

-- Step 2: Delete Page Route for the Self-Service Kiosk (Preview) site
DELETE FROM [PageRoute]
WHERE [Route] = 'kiosk'
  AND [PageId] = (
    SELECT [Id]
    FROM [Page]
    WHERE [Guid] = 'AB045324-60A4-4972-8936-7B319FF5D2CE' -- Self-Service Kiosk Homepage
  );

-- Step 3.0: Clear out the 'Default' Pages for the Self-Service Kiosk (Preview) site
UPDATE [Site]
SET [DefaultPageId] = NULL
    ,[DefaultPageRouteId] = NULL
    ,[LoginPageId] = NULL
    ,[LoginPageRouteId] = NULL
    ,[RegistrationPageId] = NULL
    ,[RegistrationPageRouteId] = NULL
    ,[PageNotFoundPageId] = NULL
    ,[PageNotFoundPageRouteId] = NULL
    ,[CommunicationPageId] = NULL
    ,[CommunicationPageRouteId] = NULL
    ,[MobilePageId] = NULL
    ,[ChangePasswordPageId] = NULL
    ,[ChangePasswordPageRouteId] = NULL
WHERE [Id] = @SiteId

-- Step 3.1: Delete orphaned Pages (with no Blocks remaining)
DELETE FROM [Page]
WHERE [Id] IN (
	SELECT p.[Id]
	FROM [Page] p
	LEFT JOIN [Block] b ON b.[PageId] = p.[Id]
	WHERE b.[Id] IS NULL
	AND EXISTS (
		SELECT 1
		FROM [BlockType] bt
		JOIN [Block] b2 ON b2.[BlockTypeId] = bt.[Id]
		WHERE b2.[PageId] = p.[Id]
		AND bt.[Guid] IN (
			'61C5C8F2-6F76-4583-AB97-228878A6AB65', -- PersonUpdate.Kiosk.ascx 
			'D10900A8-C2C1-4414-A443-3781A5CF371C', -- TransactionEntry.Kiosk.ascx 
			'9D8ED334-F1F5-4377-9E27-B8C0852CF34D'  -- PrayerRequestEntry.Kiosk.ascx 
		)
	)
);

-- Step 3.2: Delete Pages tied to the Site
DELETE FROM [Page]
WHERE [SiteId] = @SiteId

-- Step 4: Delete the preview 'Kiosk' BlockTypes
DELETE FROM [BlockType]
WHERE [Guid] IN (
    '61C5C8F2-6F76-4583-AB97-228878A6AB65', -- PersonUpdate.Kiosk.ascx 
    'D10900A8-C2C1-4414-A443-3781A5CF371C', -- TransactionEntry.Kiosk.ascx 
    '9D8ED334-F1F5-4377-9E27-B8C0852CF34D'  -- PrayerRequestEntry.Kiosk.ascx 
)

-- Step 5: Delete any Layouts tied to the  the Self-Service Kiosk (Preview) site
DELETE FROM [Layout]
WHERE [SiteId] = @SiteId

-- Step 6: Delete the Self-Service Kiosk (Preview) site
DELETE FROM [Site]
WHERE [Id] = @SiteId

-- Step 7: Delete the Theme 'KioskStark' from the database
DELETE FROM [Theme]
WHERE [RootPath] = '/Themes/KioskStark' AND IsSystem = 1
";
            DbService.ExecuteCommand( queryToDeleteBlocksPagesAndBlockTypes, System.Data.CommandType.Text, null, _commandTimeout );
        }

        /// <summary>
        /// Deletes the KioskStark theme folder/files and specific 'Kiosk' block type files from the file system.
        /// </summary>
        /// <returns>Number of files and folders deleted.</returns>
        private int DeleteThemeAndBlockTypeFilesFromFileSystem()
        {
            int deletedCount = 0;

            var virtualPathsToDelete = new List<string>
    {
        "~/Blocks/CRM/PersonUpdate.Kiosk.ascx",
        "~/Blocks/CRM/PersonUpdate.Kiosk.ascx.cs",
        "~/Blocks/Finance/TransactionEntry.Kiosk.ascx",
        "~/Blocks/Finance/TransactionEntry.Kiosk.ascx.cs",
        "~/Blocks/Prayer/PrayerRequestEntry.Kiosk.ascx",
        "~/Blocks/Prayer/PrayerRequestEntry.Kiosk.ascx.cs"
    };

            // Map and delete block files.
            foreach ( var virtualPath in virtualPathsToDelete )
            {
                var fullPath = System.Web.Hosting.HostingEnvironment.MapPath( virtualPath );
                if ( File.Exists( fullPath ) )
                {
                    File.Delete( fullPath );
                    deletedCount++;
                }
            }

            // Delete the ~/Themes/KioskStark folder.
            var themeFolderPath = System.Web.Hosting.HostingEnvironment.MapPath( "~/Themes/KioskStark" );
            if ( Directory.Exists( themeFolderPath ) )
            {
                Directory.Delete( themeFolderPath, recursive: true );
                deletedCount++;
            }

            return deletedCount;
        }

        /// <summary>
        /// Deletes the job.
        /// </summary>
        private void DeleteJob()
        {
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( GetJobId() );

                if ( job != null )
                {
                    jobService.Delete( job );
                    rockContext.SaveChanges();
                }
            }
        }

        private void SendNotificationMessage( Exception jobException, ServiceJob job )
        {
            var groupGuid = Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid();

            var rockContext = new RockContext();
            var group = new GroupService( rockContext ).Get( groupGuid );

            if ( group == null )
            {
                return;
            }

            if ( _customMessage.IsNullOrWhiteSpace() )
            {
                _customMessage = "still has the Self-Service Kiosk (Preview) site and 'Kiosk' blocks on your Rock system";
            }

            var internalApplicationRoot = GlobalAttributesCache.Get().GetValue( "InternalApplicationRoot" );
            var url = internalApplicationRoot.EnsureTrailingForwardslash() + "admin/system/jobs";

            job.LastStatusMessage = $@"
<p>
Hello Rock Admin,
</p>

<p>
You're receiving this message because it appears your Rock instance {_customMessage}, and we couldn't determine if you are still using them. This 'preview' site was introduced in an earlier version of Rock (v4), but we've decided it's time to retire and remove it. 😢
</p>

<p>
The blocks that are being removed are:
</p>
<ul>
    <li> <b>Person Update - Kiosk (deprecated)</b> or CRM/PersonUpdate.Kiosk.ascx</li>
    <li> <b>Transaction Entry - Kiosk (deprecated)</b> or Finance/TransactionEntry.Kiosk.ascx</li>
    <li> <b>Prayer Request Entry - Kiosk (deprecated)</b> or Prayer/PrayerRequestEntry.Kiosk.ascx</li>
</ul>
<p>
One of the blocks in that preview, specifically the ""Give"" page using the ""Transaction Entry - Kiosk (deprecated)"" block, is not PCI compliant. Therefore, we recommend discontinuing its use along with the other blocks in that preview site.
</p>

<p>
While there’s no direct replacement for these preview blocks, if needed, we suggest you find a Rock partner who can assist in creating and maintaining copies of those pages and block types if needed. Otherwise, we recommend removing them from your Rock system.
</p>

<p>
—The Rock Core Team<br>
[This email was generated by the <a href=""{url}"">{this.ServiceJobName}</a> job.]
</p>

<br>
<br>
<hr>";

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null, new Lava.CommonMergeFieldsOptions() );
            mergeFields.Add( "Job", job );
            try
            {
                if ( jobException != null )
                {
                    mergeFields.Add( "Exception", LavaDataObject.FromAnonymousObject( jobException ) );
                }
            }
            catch
            {
                // ignore
            }

            var emailMessage = new RockEmailMessage( Rock.SystemGuid.SystemCommunication.CONFIG_JOB_NOTIFICATION.AsGuid() );
            emailMessage.AdditionalMergeFields = mergeFields;
            emailMessage.CreateCommunicationRecord = false;
            emailMessage.Subject = "Action Required: Regarding the \"Self-Service Kiosk (Preview)\" Site and Blocks";
            emailMessage.SetRecipients( group.Id );

            emailMessage.Send();
        }
    }
}
