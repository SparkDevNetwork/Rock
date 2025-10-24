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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using System.Web;

using Microsoft.Extensions.DependencyInjection;

using Rock.Configuration;
using Rock.Lava;
using Rock.Net;

namespace Rock.Web
{
    [ExcludeFromCodeCoverage]
    internal class LavaPage : HttpTaskAsyncHandler
    {
        private readonly RockRequestContext _rockRequestContext;

        private readonly LavaPageRenderer _renderer;

        public LavaPage( string filename, RockRequestContext rockRequestContext )
        {
            _renderer = new LavaPageRenderer( File.ReadAllText( filename ), LavaService.GetCurrentEngine(), rockRequestContext );
            _rockRequestContext = rockRequestContext;
        }

        public override async Task ProcessRequestAsync( HttpContext context )
        {
            var internalAccessor = RockApp.Current.GetRequiredService<IRockRequestContextAccessor>() as RockRequestContextAccessor;

            if ( internalAccessor != null )
            {
                internalAccessor.RockRequestContext = _rockRequestContext;
            }

            context.Response.Write( await _renderer.RenderAsync() );

            if ( internalAccessor != null )
            {
                internalAccessor.RockRequestContext = null;
            }
        }
    }
}
