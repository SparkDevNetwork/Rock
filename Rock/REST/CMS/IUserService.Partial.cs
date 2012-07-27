//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.ServiceModel;

namespace Rock.REST.CMS
{
    public partial interface IUserService
    {
		/// <summary>
		/// Determines if the given username is available
		/// </summary>
		[OperationContract]
        bool IsAvailable( string username );
    }
}
