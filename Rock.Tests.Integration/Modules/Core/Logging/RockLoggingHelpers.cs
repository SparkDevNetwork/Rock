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
using System.Threading;

namespace Rock.Tests.Integration.Modules.Core.Logging
{
    public static class RockLoggingHelpers
    {
        public static void DeleteFilesInFolder( string logFolder )
        {
            var logFolderPath = System.IO.Path.GetFullPath( logFolder );
            if ( !System.IO.Directory.Exists( logFolderPath ) )
            {
                return;
            }

            var files = System.IO.Directory.GetFiles( logFolderPath, "*.*", System.IO.SearchOption.AllDirectories );
            var retryCount = 2;
            foreach ( var file in files )
            {
                for ( int i = 0; i < retryCount; i++ )
                {
                    try
                    {
                        System.IO.File.Delete( file );
                        break;
                    }
                    catch
                    {
                        Thread.SpinWait( 1000 );
                    }
                }
            }

            System.IO.Directory.Delete( logFolder, true );
        }
    }
}
