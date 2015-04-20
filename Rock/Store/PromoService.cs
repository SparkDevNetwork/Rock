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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestSharp;
using System.Configuration;
using System.IO;


namespace Rock.Store
{
    /// <summary>
    /// Service class for the store promotions model.
    /// </summary>
    public class PromoService : StoreServiceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PromoService"/> class.
        /// </summary>
        public PromoService() : base()
        {}


        /// <summary>
        /// Gets the promos.
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <param name="isTopFree">if set to <c>true</c> [is top free].</param>
        /// <param name="isFeatured">if set to <c>true</c> [is featured].</param>
        /// <param name="isTopPaid">if set to <c>true</c> [is top paid].</param>
        /// <returns></returns>
        public List<Promo> GetPromos( int? categoryId, bool isTopFree = false, bool isFeatured = false, bool isTopPaid = false )
        {
            string error = null;
            return GetPromos(categoryId, out error, isTopFree, isFeatured, isTopPaid);
        }

        /// <summary>
        /// Gets the promos.
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <param name="errorResponse">The error response.</param>
        /// <param name="isTopFree">if set to <c>true</c> [is top free].</param>
        /// <param name="isFeatured">if set to <c>true</c> [is featured].</param>
        /// <param name="isTopPaid">if set to <c>true</c> [is top paid].</param>
        /// <returns></returns>
        public List<Promo> GetPromos(int? categoryId, out string errorResponse, bool isTopFree = false, bool isFeatured = false, bool isTopPaid = false)
        {
            errorResponse = string.Empty;

            // setup REST call
            var client = new RestClient( _rockStoreUrl );
            client.Timeout = _clientTimeout;
            var request = new RestRequest();
            request.Method = Method.GET;
            
            if (categoryId.HasValue) {
                request.Resource = string.Format( "Api/Promos/GetByCategory/{0}", categoryId.Value.ToString().ToString() );
            }
            else
            {
                request.Resource = "Api/Promos/GetNonCategorized";
            }

            if ( isTopFree )
            {
                Parameter parm = new Parameter();
                parm.Name = "$filter";
                parm.Value = "IsTopFree eq true";
                parm.Type = ParameterType.QueryString;
                request.AddParameter( parm );
            }

            if ( isTopPaid )
            {
                Parameter parm = new Parameter();
                parm.Name = "$filter";
                parm.Value = "IsTopPaid eq true";
                parm.Type = ParameterType.QueryString;
                request.AddParameter( parm );
            }

            if ( isFeatured )
            {
                Parameter parm = new Parameter();
                parm.Name = "$filter";
                parm.Value = "IsFeatured eq true";
                parm.Type = ParameterType.QueryString;
                request.AddParameter( parm );
            }

            // deserialize to list of packages
            var response = client.Execute<List<Promo>>( request );

            if ( response.ResponseStatus == ResponseStatus.Completed )
            {
                return response.Data;
            }
            else
            {
                errorResponse = response.ErrorMessage;
                return new List<Promo>();
            }
        }
    }
}
