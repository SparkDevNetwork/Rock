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

using System;
using System.IO;

namespace Rock.Update.Helpers
{
    /// <summary>
    /// A static class for managing the offline page used during the install process.
    /// </summary>
    public static class OfflinePageHelper
    {
        private static readonly string OFFLINE_TEMPLATE_NAME = "app_offline-template.htm";
        private static readonly string OFFLINE_FILE_NAME = "app_offline.htm";

        /// <summary>
        /// Removes the app_offline.htm file so the app can be used again.
        /// </summary>
        public static void RemoveOfflinePage()
        {
            var file = Path.Combine( FileManagementHelper.ROOT_PATH, OFFLINE_FILE_NAME );
            if ( File.Exists( file ) )
            {
                File.Delete( file );
            }
        }

        /// <summary>
        /// Copies the app_offline-template.htm file to app_offline.htm so no one else can hit the app.
        /// If the template file does not exist an app_offline.htm file will be created from scratch.
        /// </summary>
        public static void CreateOfflinePage()
        {
            var templateFile = Path.Combine( FileManagementHelper.ROOT_PATH, OFFLINE_TEMPLATE_NAME );
            var offlineFile = Path.Combine( FileManagementHelper.ROOT_PATH, OFFLINE_FILE_NAME );

            try
            {
                if ( File.Exists( templateFile ) )
                {
                    File.Copy( templateFile, offlineFile, overwrite: true );
                }
                else
                {
                    CreateOfflinePageFromScratch( offlineFile );
                }
            }
            catch ( Exception )
            {
                if ( !File.Exists( offlineFile ) )
                {
                    CreateOfflinePageFromScratch( offlineFile );
                }
            }
        }

        /// <summary>
        /// Simply creates an app_offline.htm file so no one else can hit the app.
        /// </summary>
        private static void CreateOfflinePageFromScratch( string offlineFile )
        {
            var offlinePage = @"
                <html>
                    <head>
                    <title>Application Updating...</title>
                    </head>
                    <body>
                        <h1>One Moment Please</h1>
                        This application is undergoing an essential update and is temporarily offline.  Please give me a minute or two to wrap things up.
                    </body>
                </html>
                ";
            File.WriteAllText( offlineFile, offlinePage );
        }
    }
}
