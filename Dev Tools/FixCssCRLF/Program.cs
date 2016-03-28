using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication5
{
    class Program
    {
        static void Main( string[] args )
        {
            var recentCssFiles = Directory.GetFiles( @"C:\Projects\CCVRockit\RockWeb\Themes\", "*.css", SearchOption.AllDirectories ).Select( a => new FileInfo( a ) ).Where( a => ( DateTime.Now - a.LastWriteTime ).Hours < 24 ).ToList();
            foreach (var cssFile in recentCssFiles)
            {
                var contents = File.ReadAllText( cssFile.FullName );

                // from
                var crlfContents = contents.Replace( "\r\n", "\n" ).Replace( "\r", "\n" ).Replace( "\n", "\r\n" );

                if (crlfContents != contents)
                {
                    File.WriteAllText( cssFile.FullName, crlfContents );
                }
            }

        }
    }
}
