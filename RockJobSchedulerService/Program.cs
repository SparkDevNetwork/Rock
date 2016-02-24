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
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;

namespace RockJobSchedulerService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {

            string serviceFolder = Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly().Location );
            SqlServerTypes.Utilities.LoadNativeAssemblies( serviceFolder );

            // set the current directory to the same as the current exe so that we can find the web.connectionstrings.config
            Directory.SetCurrentDirectory( serviceFolder );
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new JobScheduler() 
            };

            //// NOTE: To run and debug this service in Visual Studio uncomment out the debug code below
            //// Make sure you have a web.connectionstring.config in your debug/bin directory!
            JobScheduler debug = new JobScheduler();
            debug.StartJobScheduler();

            //ServiceBase.Run( ServicesToRun );
            
        }
    }
}
