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
using Rock.Model;

namespace Rock.Transactions
{
    /*
         5/2/2022 - MDP

         We changed this back to use Rock.Transactions.RegisterControllersTransaction instead.
         This is because RestControllerService.RegisterControllers has a dependency on having
         all the Routes configured. The Bus message approach (Rock.Tasks.RegisterRestControllers)
         would sometimes result in RegisterControllers happening before Routes would be configured,
         which would cause RestController and RestAction records to get deleted.

         Reason: RestControllerService.RegisterControllers requires Routes to be configured first.
    */

    /// <summary>
    /// Registers controllers
    /// </summary>
    public class RegisterControllersTransaction : ITransaction
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            RestControllerService.RegisterControllers();
        }
    }
}