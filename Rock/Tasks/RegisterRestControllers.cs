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

namespace Rock.Tasks
{
    /*
     05-02-2022 MDP

     We changed this back to use Rock.Transactions.RegisterControllersTransaction instead.
     This is because RestControllerService.RegisterControllers has a dependancy on having
     all the Routes configured. The Bus message approach would sometimes result
     in RegisterControllers happening before Routes would be configured, which would cause
     RestController and RestAction records to get deleted.
     */


    /// <summary>
    /// Calls <see cref="RestControllerService.RegisterControllers"/>
    /// </summary>
    [RockObsolete("1.13")]
    [Obsolete( "Use Rock.Transactions.RegisterControllersTransaction instead" )]
    public sealed class RegisterRestControllers : BusStartedTask<RegisterRestControllers.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            RestControllerService.RegisterControllers();
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
        }
    }
}