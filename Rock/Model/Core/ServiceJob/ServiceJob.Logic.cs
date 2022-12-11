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

using Rock.Jobs;

namespace Rock.Model
{
    public partial class ServiceJob
    {
        /// <summary>
        /// Gets the Type of the job based on Job.Assembly and Job.Class
        /// </summary>
        /// <returns>Type.</returns>
        public Type GetCompiledType()
        {
            // build the type object, will depend if the class is in an assembly or the App_Code folder
            Type type = null;
            var job = this;

            if ( string.IsNullOrWhiteSpace( job.Assembly ) )
            {
                // first, if no assembly is known, look in all the dlls for it
                type = Rock.Reflection.FindType( typeof( RockJob ), job.Class );

                if ( type == null )
                {
#pragma warning disable CS0612, CS0618 // Type or member is obsolete
                    // if this isn't a RockJob, it could be from a pre-v15 plugin that implements Quartz.IJob directly
                    type = Rock.Reflection.FindType( typeof( Quartz.IJob ), job.Class );
#pragma warning restore CS0612, CS0618 // Type or member is obsolete

                }

                if ( type == null )
                {
                    // if it can't be found in dlls, look in App_Code using BuildManager
                    type = GetCompiledTypeFromBuildManager();
                }
            }
            else
            {
                // if an assembly is specified, load the type from that
                string thetype = string.Format( "{0}, {1}", job.Class, job.Assembly );
                type = Type.GetType( thetype );
            }

            return type;
        }
    }
}
