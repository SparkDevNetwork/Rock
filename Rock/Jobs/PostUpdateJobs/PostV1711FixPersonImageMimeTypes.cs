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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Utility;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v17.11 to set the mime type of person photos uploaded
    /// through the profile photo REST API. Those photos were saved with the
    /// mime type "octet/stream", which GetImage.ashx refuses to serve. Fix for
    /// issue #7089.
    /// </summary>
    [DisplayName( "Rock Update Helper v17.11 - Fix Person Image Mime Types" )]
    [Description( "This job will set the mime type of person photos uploaded from mobile devices so that they can be displayed." )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete.",
        IsRequired = false,
        DefaultIntegerValue = 14400 )]
    public class PostV1711FixPersonImageMimeTypes : PostUpdateJobs.PostUpdateJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary>
        /// The mime type that PersonService.UpdatePersonProfilePhoto used to
        /// save. This is not a real mime type and nothing else in Rock writes
        /// it, so together with the Person Image file type it identifies
        /// exactly the photos that need to be fixed.
        /// </summary>
        private const string InvalidMimeType = "octet/stream";

        /// <summary>
        /// The number of binary files to load and save at one time.
        /// </summary>
        private const int BatchSize = 100;

        /// <inheritdoc />
        public override void Execute()
        {
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;
            var updatedCount = 0;
            var skippedCount = 0;
            List<int> binaryFileIds;

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                var personImageFileTypeGuid = SystemGuid.BinaryFiletype.PERSON_IMAGE.AsGuid();

                // Photos that were replaced still exist and can be shown
                // elsewhere, so don't limit this to current Person.PhotoId values.
                binaryFileIds = new BinaryFileService( rockContext )
                    .Queryable()
                    .Where( bf => bf.MimeType == InvalidMimeType
                        && bf.BinaryFileType.Guid == personImageFileTypeGuid )
                    .OrderBy( bf => bf.Id )
                    .Select( bf => bf.Id )
                    .ToList();
            }

            for ( var i = 0; i < binaryFileIds.Count; i += BatchSize )
            {
                var batchIds = binaryFileIds.Skip( i ).Take( BatchSize ).ToList();

                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.CommandTimeout = commandTimeout;

                    var binaryFiles = new BinaryFileService( rockContext )
                        .Queryable()
                        .Where( bf => batchIds.Contains( bf.Id ) )
                        .ToList();

                    var mimeTypes = new Dictionary<int, string>();

                    foreach ( var binaryFile in binaryFiles )
                    {
                        var mimeType = GetImageMimeType( binaryFile );

                        // Leave anything we can't read as an image alone.
                        if ( mimeType == null )
                        {
                            skippedCount++;
                            continue;
                        }

                        binaryFile.MimeType = mimeType;
                        binaryFile.ModifiedDateTime = RockDateTime.Now;
                        mimeTypes.Add( binaryFile.Id, mimeType );
                    }

                    // Try to save the changes in bulk. If something fails the
                    // job would hit the same error every time it runs, so if
                    // that happens try one at a time skipping the failures.
                    try
                    {
                        // Only the mime type changed, so skip the save hooks
                        // that would otherwise process the file content.
                        rockContext.SaveChanges( true );
                        updatedCount += mimeTypes.Count;
                    }
                    catch
                    {
                        var savedCount = UpdateMimeTypesOneByOne( mimeTypes, commandTimeout );

                        updatedCount += savedCount;
                        skippedCount += mimeTypes.Count - savedCount;
                    }
                }
            }

            Result = $"Updated {updatedCount:N0} and skipped {skippedCount:N0} person photos.";

            DeleteJob();
        }

        /// <summary>
        /// Gets the image mime type of the file from its contents.
        /// </summary>
        /// <param name="binaryFile">The binary file.</param>
        /// <returns>The image mime type, or <c>null</c> if the content could not be read or is not a supported image.</returns>
        private static string GetImageMimeType( BinaryFile binaryFile )
        {
            try
            {
                using ( var stream = binaryFile.ContentStream )
                {
                    return FileUtilities.GetImageMimeType( stream );
                }
            }
            catch ( Exception ex )
            {
                // Storage providers (file system, Azure, S3 and other plugins)
                // throw their own exception types. Log it and skip this file
                // so one bad file doesn't stop the job from finishing.
                ExceptionLogService.LogException( new Exception( $"Unable to read person photo BinaryFile {binaryFile.Id} to fix its mime type.", ex ) );

                return null;
            }
        }

        /// <summary>
        /// Updates the mime types of the binary files specified in the
        /// parameters. These are saved one at a time so that any errors
        /// during the save operation can be logged and skipped.
        /// </summary>
        /// <param name="mimeTypes">The mime types to save, keyed by binary file identifier.</param>
        /// <param name="commandTimeout">The database command timeout.</param>
        /// <returns>The number of binary files that were saved.</returns>
        private static int UpdateMimeTypesOneByOne( Dictionary<int, string> mimeTypes, int commandTimeout )
        {
            var savedCount = 0;

            foreach ( var mimeType in mimeTypes )
            {
                try
                {
                    using ( var rockContext = new RockContext() )
                    {
                        rockContext.Database.CommandTimeout = commandTimeout;

                        var binaryFile = new BinaryFileService( rockContext ).Get( mimeType.Key );

                        if ( binaryFile == null )
                        {
                            continue;
                        }

                        binaryFile.MimeType = mimeType.Value;
                        binaryFile.ModifiedDateTime = RockDateTime.Now;

                        rockContext.SaveChanges( true );
                        savedCount++;
                    }
                }
                catch ( Exception ex )
                {
                    // Log and skip this file so one bad file doesn't stop
                    // the job from finishing.
                    ExceptionLogService.LogException( new Exception( $"Unable to save the mime type of person photo BinaryFile {mimeType.Key}.", ex ) );
                }
            }

            return savedCount;
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
    }
}
