using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Workflow;
using Rock.Web.Cache;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Text;
using System.Net;
using System.IO;
using System.Linq;

namespace org.newpointe.Giving.Workflow
{
    [ActionCategory( "Extra Actions" )]
    [Description( "Gets a Short URL using a YOURLS service. NOTE: Rock v7 should have a built-in way of doing this! You should replace this once you upgrade!" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Get Short URL (YOURLS)" )]

    [WorkflowAttribute( "Attribute", "The workflow attribute to store the short URL in.", true, "", "", 0, null, new[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.UrlLinkFieldType", "Rock.Field.Types.VideoUrlFieldType", "Rock.Field.Types.VideoUrlFieldType" } )]
    [WorkflowTextOrAttribute( "URL", "URL", "Workflow attribute that contains the url to shorten.", true, "", "", 1, "URL", new[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.UrlLinkFieldType", "Rock.Field.Types.VideoUrlFieldType", "Rock.Field.Types.VideoUrlFieldType" } )]
    [WorkflowTextOrAttribute( "URL Shortener", "URL Shortener", "Workflow attribute that contains the url of the YOURLS url shortener to use. {0} is the url, and {1} is the shortened keyword.", true, "https://example.com/yourls-api.php?signature=05e2685fc7&action=shorturl&url={0}&keyword={1}&format=simple", "", 1, "URL", new[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.UrlLinkFieldType", "Rock.Field.Types.VideoUrlFieldType", "Rock.Field.Types.VideoUrlFieldType" } )]
    public class GetShortUrl : ActionComponent
    {

        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {

            errorMessages = new List<string>();

            AttributeCache attribute = AttributeCache.Read( GetAttributeValue( action, "Attribute" ).AsGuid(), rockContext );
            string urlToShorten = GetAttributeValue( action, "URL", true );
            string shortenerURL = GetAttributeValue( action, "URLShortener", true );

            if ( attribute == null )
                errorMessages.Add( "Invalid Attribute." );

            if ( string.IsNullOrWhiteSpace( urlToShorten ) || !IsUrl( urlToShorten ) )
                errorMessages.Add( "Invalid Url." );

            if ( string.IsNullOrWhiteSpace( shortenerURL ) || !IsUrl( shortenerURL ) )
                errorMessages.Add( "Invalid Url Shortener." );

            if ( attribute == null || errorMessages.Any() )
            {
                errorMessages.ForEach( m => action.AddLogEntry( m, true ) );
                return false;
            }


            string url = GetShortUrlFromString( shortenerURL, urlToShorten );

            if ( attribute.EntityTypeId == new Rock.Model.Workflow().TypeId )
            {
                action.Activity.Workflow.SetAttributeValue( attribute.Key, url );
                action.AddLogEntry( $"Set '{attribute.Name}' attribute to '{url}'." );
            }
            else if ( attribute.EntityTypeId == new Rock.Model.WorkflowActivity().TypeId )
            {
                action.Activity.SetAttributeValue( attribute.Key, url );
                action.AddLogEntry( $"Set '{attribute.Name}' attribute to '{url}'." );
            }


            return true;
        }

        public static bool IsUrl( string str )
        {
            return Uri.TryCreate( str, UriKind.Absolute, out Uri uriResult );
        }

        // Shorten the give URL
        public static string GetShortUrlFromString( string shortener, string url, int rec = 0 )
        {
            // Generate a random Short URL from a GUID
            string randomShortUrl = Guid.NewGuid().ToString( "N" ).Substring( 0, 8 ).ToLower();

            // Construct the URL for the request
            string theUrl = string.Format( shortener, Uri.EscapeDataString( url ), randomShortUrl );

            // GET request to API
            using ( Stream responseStream = WebRequest.Create( theUrl ).GetResponse().GetResponseStream() )
            {
                string responseStr = new StreamReader( responseStream, Encoding.UTF8 ).ReadToEnd();

                if ( !string.IsNullOrWhiteSpace( responseStr ) )
                {
                    return responseStr;
                }
                else if ( rec < 4 )
                {
                    return GetShortUrlFromString( shortener, url, rec + 1 );
                }
                else
                {
                    return url;
                }
            }
        }


    }
}