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
    public class PromoService : StoreService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PromoService"/> class.
        /// </summary>
        public PromoService() : base()
        {}

        
        /// <summary>
        /// Gets a list of featured package categories from the store.
        /// </summary>
        /// <returns>a <see cref="T:IEumerable<Promos>"/> of promos.</returns>
        public List<Promo> GetPromos(int? categoryId, bool isTopFree = false, bool isFeatured = false, bool isTopPaid = false)
        {

            // setup REST call
            var client = new RestClient( _rockStoreUrl );
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
            var promos = client.Execute<List<Promo>>( request ).Data;
            return promos;          
        }
    }
}
