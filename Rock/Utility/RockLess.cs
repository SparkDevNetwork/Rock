using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using dotless.Core;
using dotless.Core.configuration;

namespace Rock.Utility
{
    public static class RockLess
    {
        public static bool CompileTheme(string themeName, out string messages )
        {
            messages = string.Empty;
            bool result = true;

            try {
                string fullThemePath = HttpRuntime.AppDomainAppPath + @"Themes\" + themeName;

                DirectoryInfo themeDirectory = new DirectoryInfo( fullThemePath + @"\Styles" );
                if ( themeDirectory.Exists )
                {
                    FileInfo[] files = themeDirectory.GetFiles();

                    DotlessConfiguration dotLessConfiguration = new DotlessConfiguration();
                    dotLessConfiguration.MinifyOutput = true;
                    dotLessConfiguration.RootPath = themeDirectory.FullName;

                    Directory.SetCurrentDirectory( themeDirectory.FullName );

                    if ( files != null )
                    {
                        // if a directory contains a file with .nocompile don't compile the less
                        if ( files.Where( f => f.Name == ".nocompile" ).Count() == 0 )
                        {
                            // don't compile files that start with an underscore
                            foreach ( var file in files.Where( f => f.Name.EndsWith( ".less" ) && !f.Name.StartsWith( "_" ) ) )
                            {
                                string cssSource = Less.Parse( File.ReadAllText( file.FullName ), dotLessConfiguration );
                                File.WriteAllText( file.DirectoryName + @"\" + file.Name.Replace( ".less", ".css" ), cssSource );
                            }
                        }
                    }
                }
            }
            catch(Exception ex )
            {
                result = false;
                messages = ex.Message;
            }

            return result;
        }
    }
}
