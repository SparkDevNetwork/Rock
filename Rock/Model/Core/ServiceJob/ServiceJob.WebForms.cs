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
using System.Web.Compilation;

namespace Rock.Model
{
    public partial class ServiceJob
    {
        // if it can't be found in dlls, look in App_Code using BuildManager
        private Type GetCompiledTypeFromBuildManager()
        {
            try
            {
                // This is a long-shot, but it could be in the App_Code folder of RockWeb.
                return BuildManager.GetType( this.Class, false );
            }
            catch
            {
                // We told BuildManager to not throw errors, but it could throw an error if RockWeb hasn't started yet.
                // If so, ignore error.
                return null;
            }

        }
    }
}
