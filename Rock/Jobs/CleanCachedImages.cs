#region License
/* 
 * All content copyright Terracotta, Inc., unless otherwise indicated. All rights reserved. 
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not 
 * use this file except in compliance with the License. You may obtain a copy 
 * of the License at 
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0 
 *   
 * Unless required by applicable law or agreed to in writing, software 
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations 
 * under the License.
 * 
 */
#endregion

using System;
using System.Web;
using System.IO;

using Quartz;

using Rock.Web.UI;

namespace Rock.Jobs
    

    /// <summary>
    /// Job to keep a heartbeat of the job process so we know when the jobs stop working
    /// </summary>
    /// <author>Jon Edmiston</author>
    /// <author>Spark Development Network</author>

    [BlockProperty( 0, "Days to Expire", "DaysToExpire", "General", "The number of days to leave an image in the cache directory.", false, "100", "Rock", "Rock.Field.Types.Integer" )]
    [BlockProperty( 0, "Starting Directory", "StartingDirectory", "General", "The starting directory to iterate through.", false, "~/Cache/" )]
    public class CleanCachedImages : IJob
        
        int DAYS_TO_EXPIRE = 100;

        /// <summary> 
        /// Empty constructor for job initilization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public CleanCachedImages()
            
        }

        /// <summary> 
        /// Job that updates the JobPulse setting with the current date/time.
        /// This will allow us to notify an admin if the jobs stop running.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
            
            // get the number of days to expire
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            DAYS_TO_EXPIRE = Int32.Parse( dataMap.GetString( "DaysToExpire" ) );
            string startingDirectory = dataMap.GetString( "StartingDirectory" );

            // get full path if an application path was given and we're running is IIS
            if ( startingDirectory.Contains( "~" ) && context.Scheduler.SchedulerName == "RockSchedulerIIS" )
                
                startingDirectory = HttpContext.Current.Server.MapPath( startingDirectory );
            }

            // start iterating through directories
            ProcessDirectory( startingDirectory );
        }

        /// <summary>
        /// Processes the directory.
        /// </summary>
        /// <param name="sourceDirectory">The source directory.</param>
        private void ProcessDirectory( string sourceDirectory )
            
            // check creation date of files
            string[] fileEntries = Directory.GetFiles( sourceDirectory );
            foreach ( string fileName in fileEntries )
                
                // delete file if it's older than the max days
                DateTime dateCreated = File.GetCreationTime( sourceDirectory + "/" + fileName );
                TimeSpan span = DateTime.Now.Subtract( dateCreated );
                if ( span.Days > DAYS_TO_EXPIRE )
                    
                    File.Delete( sourceDirectory + "/" + fileName );
                }
            }

            // Recurse into subdirectories of this directory.
            string[] subdirectories = Directory.GetDirectories( sourceDirectory );
            foreach ( string subdir in subdirectories )
                
                // Do not iterate through reparse points
                if ( ( File.GetAttributes( subdir ) &
                        FileAttributes.ReparsePoint ) !=
                        FileAttributes.ReparsePoint )
                    
                    ProcessDirectory( subdir );
                }

                // if the subdirectory is empty delete it
                int directoryCount = Directory.GetDirectories( sourceDirectory ).Length;
                int fileCount = Directory.GetFiles( sourceDirectory ).Length;

                if ( ( directoryCount + fileCount ) == 0 )
                    
                    Directory.Delete( sourceDirectory );
                }
            }

        }

    }
}