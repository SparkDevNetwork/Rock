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
using System.Threading.Tasks;

namespace Rock.Tests.Shared.TestFramework
{
    /// <summary>
    /// Manages Rock databases for a LocalDb instance used during developer testing.
    /// </summary>
    public class LocalDatabaseContainer : ITestDatabaseContainer
    {
        public LocalDatabaseContainer( LocalDatabaseManager manager )
        {
            _manager = manager;
        }

        private LocalDatabaseManager _manager;
        private bool _hasCurrentInstance = false;

        public bool HasCurrentInstance
        {
            get
            {
                return _hasCurrentInstance;
            }
        }

        public Task DisposeAsync()
        {
            // The LocalDB container does not need to be disposed.
            return Task.CompletedTask;

            //_hasCurrentInstance = false;
            //var task = Task.Run( () =>
            //{
            //    _manager.DeleteDatabase();
            //} );
            //return task;
        }

        public Task StartAsync()
        {
            var task = Task.Run( () =>
            {
                _manager.ResetDatabase();
                _hasCurrentInstance = true;
            } );
            return task;
        }
    }
}