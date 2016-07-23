﻿// <copyright>
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
using System.Web;

namespace Rock.Web
{
    /// <summary>
    /// Provides application start, and module initialization and disposal events to the implementing class.
    /// </summary>
    public abstract class HttpModule : IHttpModule
    {
        #region Static privates

        private static bool _applicationStarted = false;
        private static object _applicationStartLock = new object();

        #endregion

        #region IHttpModule implementation

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application</param>
        public virtual void Init( HttpApplication context )
        {
            if ( !_applicationStarted )
            {
                lock ( _applicationStartLock )
                {
                    if ( !_applicationStarted )
                    {
                        this.Application_Start( context );
                        _applicationStarted = true;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Method that will be called once on application start.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Application_Start( HttpApplication context )
        {
        }
    }
}
