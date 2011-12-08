//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

namespace Rock.REST
{
    /// <summary>
    /// Interface used for the MEF import/export signature of all WCF REST Api services
    /// </summary>
    public interface IService
    {
    }

    /// <summary>
    /// Interface used for the MEF metadata of all WCF REST Api services
    /// </summary>
    public interface IServiceData
    {
        /// <summary>
        /// The name of the route
        /// </summary>
        string RouteName { get; }
    }

}
