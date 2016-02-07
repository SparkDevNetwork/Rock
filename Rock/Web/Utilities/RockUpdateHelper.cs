// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.Utilities
{
    public static class RockUpdateHelper
    {
        /// <summary>
        /// Returns the environment data as json.
        /// </summary>
        /// <returns>a JSON formatted string</returns>
        public static string GetEnvDataAsJson( System.Web.HttpRequest request, string rockUrl )
        {
            var envData = new Dictionary<string, string>();
            envData.Add( "AppRoot", rockUrl );
            envData.Add( "Architecture", ( IntPtr.Size == 4 ) ? "32bit" : "64bit" );
            envData.Add( "AspNetVersion", Environment.Version.ToString() );
            envData.Add( "IisVersion", request.ServerVariables["SERVER_SOFTWARE"] );
            envData.Add( "ServerOs", Environment.OSVersion.ToString() );

            try { envData.Add( "SqlVersion", Rock.Data.DbService.ExecuteScaler( "SELECT SERVERPROPERTY('productversion')" ).ToString() ); } 
            catch {}

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    var entityType = EntityTypeCache.Read( "Rock.Security.BackgroundCheck.ProtectMyMinistry", false, rockContext );
                    if ( entityType != null )
                    {
                        var pmmUserName = new AttributeValueService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( v =>
                                v.Attribute.EntityTypeId.HasValue &&
                                v.Attribute.EntityTypeId.Value == entityType.Id &&
                                v.Attribute.Key == "UserName" )
                            .Select( v => v.Value )
                            .FirstOrDefault();
                        if ( !string.IsNullOrWhiteSpace( pmmUserName ) )
                        {
                            envData.Add( "PMMUserName", pmmUserName );
                        }
                    }
                }
            }
            catch { }

            return envData.ToJson();
        }
    }
}
