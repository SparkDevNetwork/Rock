using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Rock.CodeGeneration
{
    internal static class CodeGenHelpers
    {
        public static SqlConnection GetSqlConnection( string rootFolder )
        {
            var file = new FileInfo( Path.Combine( rootFolder, @"RockWeb\web.ConnectionStrings.config" ) );
            if ( !file.Exists )
            {
                return null;
            }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load( file.FullName );
            XmlNode root = xmlDoc.DocumentElement;
            XmlNode node = root.SelectNodes( "add[@name = \"RockContext\"]" )[0];
            SqlConnection sqlconn = new SqlConnection( node.Attributes["connectionString"].Value );
            return sqlconn;
        }

        public static List<Type> GetSystemGuidTypes( Assembly rockAssembly )
        {
            return rockAssembly.GetTypes().Where( a =>
                a.Namespace == "Rock.SystemGuid"
                && !a.IsSubclassOf( typeof( System.Attribute ) )
                && !a.IsSubclassOf( typeof( System.Exception ) )
                )
                .OrderBy( a => a.Name )
                .ToList();
        }
    }
}