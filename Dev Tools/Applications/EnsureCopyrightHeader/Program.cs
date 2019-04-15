using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EnsureCopyrightHeader
{
    class Program
    {
        /// <summary>
        /// The ignore files
        /// </summary>
        static string[] IgnoreFiles = new string[] { "\\DoubleMetaphone.cs", "\\Rock.Version\\AssemblySharedInfo.cs" };

        /// <summary>
        /// The ignore folders
        /// </summary>
        static string[] IgnoreFolders = new string[] { "\\CodeGenerated", "\\obj" };

        /// <summary>
        /// Mains the specified args.
        /// </summary>
        /// <param name="args">The args.</param>
        static void Main( string[] args )
        {
            //C:\Projects\Rock-ChMS\Dev Tools\Apps\EnsureCopyrightHeader\Program.cs
            string currentDirectory = Directory.GetCurrentDirectory();
            string rockDirectory = currentDirectory.Replace( "Dev Tools\\Applications\\EnsureCopyrightHeader\\bin\\Debug", string.Empty );

            int updatedFileCount = 0;
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "RockWeb\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Checkr\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Mailgun\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Mandrill\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Migrations\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.NMI\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.PayFlowPro\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Rest\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Security.Authentication.Auth0\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.SignNow\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Slingshot\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.StatementGenerator\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Slingshot.Model\\" );
            //updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Tests\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.Version\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Rock.WebStartup\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "RockJobSchedulerService\\" );
            updatedFileCount += FixupCopyrightHeaders( rockDirectory + "Applications\\" );

            Console.WriteLine( "\n\nDone!  Files Updated: {0}\n\nPress any key to continue.", updatedFileCount );
            Console.ReadLine();
        }

        /// <summary>
        /// Fixups the copyright headers.
        /// </summary>
        /// <param name="searchDirectory">The search directory.</param>
        private static int FixupCopyrightHeaders( string searchDirectory )
        {
            int result = 0;

            List<string> sourceFilenames = Directory.GetFiles( searchDirectory, "*.cs", SearchOption.AllDirectories ).ToList();

            // exclude files that come from the localhistory VS extension
            sourceFilenames = sourceFilenames.Where( a => !a.Contains( ".localhistory" ) ).ToList();

            // this was was our standard copyright badge up until 1/17/2014. Look for it in case it sneaks back in
            const string oldCopyrightBadge1 = @"// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the ""License"");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an ""AS IS"" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//";

            // standard copyright badge 4/1/2016 to 5/22/2016
            const string oldCopyrightBadge2 = @"// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the ""License"");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an ""AS IS"" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
";

            // standard copyright badge starting 5/23/2016
            const string newCopyrightBadgeStart = @"// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the ""License"");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an ""AS IS"" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>";

            const string newCopyrightBadge = newCopyrightBadgeStart + @"
//
";
            foreach ( string fileName in sourceFilenames )
            {
                bool skipFile = false;
                foreach ( var f in IgnoreFolders )
                {
                    if ( fileName.Contains( f ) )
                    {
                        skipFile = true;
                    }

                }

                foreach ( var f in IgnoreFiles )
                {
                    if ( Path.GetFullPath( fileName ).EndsWith( f, StringComparison.OrdinalIgnoreCase ) )
                    {
                        skipFile = true;
                    }
                }

                if ( skipFile )
                {
                    continue;
                }

                string origFileContents = File.ReadAllText( fileName );

                if ( origFileContents.Contains( "<auto-generated" ) )
                {
                    continue;
                }

                if ( origFileContents.StartsWith( newCopyrightBadgeStart ) )
                {
                    continue;
                }

                // get rid of any incorrect header by finding keyword using or namespace
                int positionNamespace = origFileContents.IndexOf( "namespace ", 0 );
                int positionUsing = origFileContents.IndexOf( "using ", 0 );
                int codeStart = positionNamespace > positionUsing ? positionUsing : positionNamespace;
                codeStart = codeStart < 0 ? 0 : codeStart;

                string newFileContents = origFileContents.Substring( codeStart );

                // try to clean up cases where the badge is after some of the using statements
                newFileContents = newFileContents.Replace( oldCopyrightBadge1, string.Empty ).Replace( newCopyrightBadge, string.Empty );
                newFileContents = newFileContents.Replace( oldCopyrightBadge2, string.Empty ).Replace( newCopyrightBadge, string.Empty );

                newFileContents = newCopyrightBadge + newFileContents.TrimStart();

                if ( !origFileContents.Equals( newFileContents ) )
                {
                    Console.WriteLine( "Updating header in {0}", fileName );
                    result++;

                    System.Text.Encoding encoding;
                    using ( var r = new StreamReader( fileName, detectEncodingFromByteOrderMarks: true ) )
                    {
                        encoding = r.CurrentEncoding;
                    }

                    File.WriteAllText( fileName, newFileContents, encoding );
                }
            }
            return result;
        }
    }
}
