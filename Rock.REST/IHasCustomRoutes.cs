//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

namespace Rock.Rest
    
	/// <summary>
	/// Interface for controllers that need to add additional routes beyond the default 
	/// api/    controller}/    id} route.
	/// </summary>
	public interface IHasCustomRoutes
	    
		void AddRoutes( System.Web.Routing.RouteCollection routes );
	}
}
