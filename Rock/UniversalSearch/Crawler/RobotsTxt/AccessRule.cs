using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.UniversalSearch.Crawler.RobotsTxt
{
    internal class AccessRule : Rule
    {
        public string Path { get; private set; }
        public bool Allowed { get; private set; }

        public AccessRule( string userAgent, Line line, int order )
            : base( userAgent, order )
        {
            Path = line.Value;
            if ( !String.IsNullOrEmpty( Path ) )
            {
                // get rid of the preceding * wild char
                while ( Path.StartsWith( "*", StringComparison.Ordinal ) )
                {
                    Path = Path.Substring( 1 );
                }
                if ( !Path.StartsWith( "/" ) )
                {
                    Path = "/" + Path;
                }
            }
            Allowed = line.Field.ToLowerInvariant().Equals( "allow" );
        }
    }
}
