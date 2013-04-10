//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Routing;
using Rock;

public partial class RESTHelp : System.Web.UI.Page
{
    /// <summary>
    /// Handles the Load event of the Page control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void Page_Load(object sender, EventArgs e)
    {
        var routes = new List<ApiInfo>();

        var config = GlobalConfiguration.Configuration;
        var explorer = config.Services.GetApiExplorer();

        foreach ( ApiDescription apiDescription in explorer.ApiDescriptions )
        {
            ApiInfo apiInfo = new ApiInfo();
            apiInfo.HttpMethod = apiDescription.HttpMethod.Method;
            apiInfo.RelativePath = apiDescription.RelativePath;
            routes.Add( apiInfo );
        }

        routes.Sort();
        gvRoutes.DataSource = routes;
        gvRoutes.DataBind();
    }

    /// <summary>
    /// 
    /// </summary>
    protected class ApiInfo : IComparable
    {
        /// <summary>
        /// Gets or sets the HTTP method.
        /// </summary>
        /// <value>
        /// The HTTP method.
        /// </value>
        public string HttpMethod { get; set; }
        
        /// <summary>
        /// Gets or sets the relative path.
        /// </summary>
        /// <value>
        /// The relative path.
        /// </value>
        public string RelativePath { get; set; }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="obj" /> in the sort order. Zero This instance occurs in the same position in the sort order as <paramref name="obj" />. Greater than zero This instance follows <paramref name="obj" /> in the sort order.
        /// </returns>
        public int CompareTo( object obj )
        {
            int compare = RelativePath.CompareTo( ( (ApiInfo)obj ).RelativePath );
            if ( compare == 0 )
            {
                compare = HttpMethod.CompareTo( ( (ApiInfo)obj ).HttpMethod );
            }

            return compare;
        }
    }
}