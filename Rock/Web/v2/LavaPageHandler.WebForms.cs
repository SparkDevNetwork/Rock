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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using System.Web;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

using Rock.Configuration;
using Rock.Lava;
using Rock.Net;
using Rock.Observability;

namespace Rock.Web.v2
{
    [ExcludeFromCodeCoverage]
    internal class LavaPageHandler : HttpTaskAsyncHandler
    {
        private readonly RockRequestContext _rockRequestContext;

        private readonly LavaPageRenderer _renderer;

        public LavaPageHandler( string filename, RockRequestContext rockRequestContext )
        {
            var factory = new LavaPageLayoutFactory( new StaticFileProvider() );
            var layout = factory.GetLayout( filename, "RockNextGen", LavaService.GetCurrentEngine() );
            _renderer = new LavaPageRenderer( layout, LavaService.GetCurrentEngine(), rockRequestContext );
            _rockRequestContext = rockRequestContext;
        }

        public override async Task ProcessRequestAsync( HttpContext context )
        {
            // Validate the trace if it is for this page.
            if ( context.Items.Contains( "Rock:DebugTraceEnabled" ) && Activity.Current != null )
            {
                var tracePageId = context.Items["Rock:DebugTraceEnabled"] as int?;

                if ( tracePageId == _rockRequestContext.Page.Id )
                {
                    RockApp.Current.GetRequiredService<DebugTraceObserver>()
                        .ValidateTrace( Activity.Current.TraceId.ToString() );
                }
            }

            // Store the page, layout and site information on the context. This
            // is used in a few rare places, such as application error handling.
            if ( _rockRequestContext.Page != null )
            {
                context.Items["Rock:PageId"] = _rockRequestContext.Page.Id;
                context.Items["Rock:LayoutId"] = _rockRequestContext.Page.Layout.Id;
                context.Items["Rock:SiteId"] = _rockRequestContext.Page.Layout.Site.Id;
            }

            var internalAccessor = RockApp.Current.GetRequiredService<IRockRequestContextAccessor>() as RockRequestContextAccessor;

            if ( internalAccessor != null )
            {
                internalAccessor.RockRequestContext = _rockRequestContext;
            }

            context.Response.Write( await _renderer.RenderAsync() );

            if ( _rockRequestContext.Response is RockResponseBase responseBase )
            {
                foreach ( var header in responseBase.Headers )
                {
                    context.Response.Headers[header.Key] = header.Value;
                }

                if ( responseBase.RedirectInfo != null )
                {
                    context.Response.Clear();
                    context.Response.Redirect( responseBase.RedirectInfo.Value.Url, responseBase.RedirectInfo.Value.Permanent );
                }
            }

            if ( internalAccessor != null )
            {
                internalAccessor.RockRequestContext = null;
            }
        }

        private class StaticFileProvider : IFileProvider
        {
            public IDirectoryContents GetDirectoryContents( string subpath )
            {
                throw new System.NotImplementedException();
            }

            public IFileInfo GetFileInfo( string subpath )
            {
                return new StaticFileInfo( subpath );
            }

            public IChangeToken Watch( string filter )
            {
                throw new System.NotImplementedException();
            }
        }

        private class StaticFileInfo : IFileInfo
        {
            private readonly string _path;

            public bool Exists => File.Exists( _path );

            public long Length => throw new NotImplementedException();

            public string PhysicalPath => throw new NotImplementedException();

            public string Name => throw new NotImplementedException();

            public DateTimeOffset LastModified => throw new NotImplementedException();

            public bool IsDirectory => throw new NotImplementedException();

            public StaticFileInfo( string path )
            {
                _path = path;
            }

            public Stream CreateReadStream()
            {
                return File.OpenRead( _path );
            }
        }
    }
}
