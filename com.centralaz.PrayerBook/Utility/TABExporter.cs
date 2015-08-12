using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using com.centralaz.Prayerbook.SystemGuid;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;

namespace com.centralaz.Prayerbook.Utility
{
    public class TABExporter
    {
        public static void WriteToTAB( Rock.Model.Group book )
        {
            string attachment = "attachment; filename=PrayerBookEntries.txt";
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ClearHeaders();
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.AddHeader( "content-disposition", attachment );
            HttpContext.Current.Response.ContentType = "text/tab-separated-values";
            HttpContext.Current.Response.AddHeader( "Pragma", "public" );

            writeColumnNames();

            foreach ( GroupMember entry in book.Members )
                writeEntryInfo( entry );

            HttpContext.Current.Response.End();
        }

        private static void writeEntryInfo( GroupMember entry )
        {
            StringBuilder stringBuilder = new StringBuilder();
            entry.LoadAttributes();

            addTab( entry.Person.FullName, stringBuilder );
            string spouseName;
            try 
            {
                spouseName = entry.Person.GetSpouse().FirstName.ToStringSafe();
            }
            catch
            {
                spouseName = String.Empty;
            }
            addTab( spouseName, stringBuilder );  //Only include spouse's first name

            Rock.Model.DefinedValue dv;

            if ( entry.GetAttributeValue( "Ministry" ) != String.Empty )
            {
                dv = new DefinedValueService( new RockContext() ).Get( entry.GetAttributeValue( "Ministry" ).AsGuid() );
                addTab( dv.Value, stringBuilder );
            }
            else
            {
                addTab( "", stringBuilder );
            }

            if ( entry.GetAttributeValue( "Subministry" ) != String.Empty )
            {
                dv = new DefinedValueService( new RockContext() ).Get( entry.GetAttributeValue( "Subministry" ).AsGuid() );
                addTab( dv.Value, stringBuilder );
            }
            else
            {
                addTab( "", stringBuilder );
            }

            addTab( entry.GetAttributeValue( "Praise1" ), stringBuilder );
            addTab( entry.GetAttributeValue( "MinistryNeed1" ), stringBuilder );
            addTab( entry.GetAttributeValue( "MinistryNeed2" ), stringBuilder );
            addTab( entry.GetAttributeValue( "MinistryNeed3" ), stringBuilder );
            addTab( entry.GetAttributeValue( "PersonalRequest1" ), stringBuilder );
            addTab( entry.GetAttributeValue( "PersonalRequest2" ), stringBuilder, true );
            HttpContext.Current.Response.Write( stringBuilder.ToString() );
            HttpContext.Current.Response.Write( Environment.NewLine );
        }

        private static void addTab( string value, StringBuilder stringBuilder )
        {
            stringBuilder.Append( '"' );
            stringBuilder.Append( value );
            stringBuilder.Append( '"' );
            stringBuilder.Append( "\t" );
        }

        private static void addTab( string value, StringBuilder stringBuilder, bool lastItem )
        {
            stringBuilder.Append( '"' );
            stringBuilder.Append( value );
            stringBuilder.Append( '"' );
        }

        private static void writeColumnNames()
        {
            string columnNames = "FullName\tSpouseName\tMinistry\tSubministry\tPraise\tMinistryNeed1\tMinistryNeed2\tMinistryNeed3\tPersonalRequest1\tPersonalRequest2";
            HttpContext.Current.Response.Write( columnNames );
            HttpContext.Current.Response.Write( Environment.NewLine );
        }
    }
}
