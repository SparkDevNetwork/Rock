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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;

namespace Westwind.Web.WebApi
{
    // From: https://weblog.west-wind.com/posts/2013/dec/13/accepting-raw-request-body-content-with-aspnet-web-api

    /// <summary>
    /// Reads the Request body into a string/byte[] and
    /// assigns it to the parameter bound.
    /// 
    /// Should only be used with a single parameter on
    /// a Web API method using the [NakedBody] attribute
    /// </summary>
    public class NakedBodyParameterBinding : HttpParameterBinding
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NakedBodyParameterBinding"/> class.
        /// </summary>
        /// <param name="descriptor">An <see cref="T:System.Web.Http.Controllers.HttpParameterDescriptor" /> that describes the parameters.</param>
        public NakedBodyParameterBinding( HttpParameterDescriptor descriptor )
            : base( descriptor )
        {

        }

        /// <summary>
        /// Check for simple
        /// </summary>
        /// <param name="metadataProvider"></param>
        /// <param name="actionContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task ExecuteBindingAsync( ModelMetadataProvider metadataProvider,
                                                    HttpActionContext actionContext,
                                                    CancellationToken cancellationToken )
        {
            if (  actionContext.Request.Method == HttpMethod.Get )
                return EmptyTask.Start();

            var type = this.Descriptor.ParameterType;

            if ( type == typeof( string ) )
            {
                return actionContext.Request.Content
                        .ReadAsStringAsync()
                        .ContinueWith( ( task ) =>
                        {
                            var stringResult = task.Result;
                            SetValue( actionContext, stringResult );
                        } );
            }
            else if ( type == typeof( byte[] ) )
            {
                return actionContext.Request.Content
                    .ReadAsByteArrayAsync()
                    .ContinueWith( ( task ) =>
                    {
                        byte[] result = task.Result;
                        SetValue( actionContext, result );
                    } );
            }

            throw new InvalidOperationException( "Only string and byte[] are supported for [NakedBody] parameters" );
        }

        /// <summary>
        /// Returns a value indicating whether this <see cref="T:System.Web.Http.Controllers.HttpParameterBinding" /> instance will read the entity body of the HTTP message.
        /// </summary>
        public override bool WillReadBody
        {
            get
            {
                return true;
            }
        }
    }

    /// <summary>
    /// A do nothing task that can be returned
    /// from functions that require task results
    /// when there's nothing to do.
    /// 
    /// This essentially returns a completed task
    /// with an empty value structure result.
    /// </summary>
    public class EmptyTask
    {
        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// <returns></returns>
        public static Task Start()
        {
            var taskSource = new TaskCompletionSource<AsyncVoid>();
            taskSource.SetResult( default( AsyncVoid ) );
            return taskSource.Task as Task;
        }

        private struct AsyncVoid
        {
        }
    }
}