//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Net;
using System.Web.Http;

using Rock.Com.CCVOnline.Service;
using Rock.Rest;

namespace Rock.Com.CCVOnline.Rest.Service
{
	/// <summary>
	/// Recordings REST API
	/// </summary>
	/// 

	public partial class RecordingsController : Rock.Rest.ApiController<Recording, RecordingDto>, IHasCustomRoutes
	{
		public void AddRoutes( System.Web.Routing.RouteCollection routes )
		{
			routes.MapHttpRoute(
				name: "ServiceRecording",
				routeTemplate: "api/recordings/{action}/{campusId}/{label}/{app}/{stream}/{recording}",
				defaults: new
				{
					controller = "recordings"
				} );
		}

		[HttpGet]
		public RecordingDto Start( int campusId, string label, string app, string stream, string recording )
		{
			var user = this.CurrentUser();
			if ( user != null )
			{
				var RecordingService = new CCVOnline.Service.RecordingService();
				var Recording = RecordingService.StartRecording( campusId, label, app, stream, recording, user.PersonId );

				if ( Recording != null )
					return new RecordingDto( Recording );
				else
					throw new HttpResponseException( HttpStatusCode.BadRequest );
			}
			throw new HttpResponseException( HttpStatusCode.Unauthorized );
		}

		[HttpGet]
		public RecordingDto Stop( int campusId, string label, string app, string stream, string recording )
		{
			var user = this.CurrentUser();
			if ( user != null )
			{
				var RecordingService = new CCVOnline.Service.RecordingService();
				var Recording = RecordingService.StopRecording( campusId, label, app, stream, recording, user.PersonId );

				if ( Recording != null )
					return new RecordingDto( Recording );
				else
					throw new HttpResponseException( HttpStatusCode.BadRequest );
			}
			throw new HttpResponseException( HttpStatusCode.Unauthorized );
		}
	}
}
