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
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace RockWeb
{
    /// <summary>
    /// Summary description for GetAsset
    /// </summary>
    public class GetAsset : IHttpAsyncHandler
    {
        public GetAsset()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public IAsyncResult BeginProcessRequest( HttpContext context, AsyncCallback cb, object extraData )
        {
            throw new NotImplementedException();
        }

        public void EndProcessRequest( IAsyncResult result )
        {
            throw new NotImplementedException();
        }

        public void ProcessRequest( HttpContext context )
        {
            throw new NotImplementedException();
        }
    }
}