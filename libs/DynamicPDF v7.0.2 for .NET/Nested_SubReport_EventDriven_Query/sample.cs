using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using ceTe.DynamicPDF.ReportWriter;
using ceTe.DynamicPDF.ReportWriter.Data;
using ceTe.DynamicPDF;
using ceTe.DynamicPDF.ReportWriter.ReportElements;
using ceTe.DynamicPDF.PageElements;
using ceTe.DynamicPDF.Merger;
using System.Data;
using System.Collections;

namespace ReportWriterTest
{
    class Program
    {
        static void Main( string[] args )
        {
            SubReportParams.PassingParamsToSubReport();
        }
    }
}

public static class SubReportParams
{

    static DataTable mainTable = new DataTable();
    static DataTable childTable = new DataTable();

    public static void PassingParamsToSubReport()
    {
        //create a data table.
        mainTable.Columns.Add( "Id" );
        mainTable.Columns.Add( "Name" );

        childTable.Columns.Add( "Id" );
        childTable.Columns.Add( "Address" );

        //Add the rows to the main and child tables.
        for ( int i = 0; i < 10; i++ )
        {
            DataRow row1 = mainTable.NewRow();
            row1["Id"] = i.ToString();
            row1["Name"] = "ceTe " + i.ToString();
            mainTable.Rows.Add( row1 );

            DataRow row = childTable.NewRow();
            row["Id"] = i.ToString();
            row["Address"] = "Address " + i.ToString();
            childTable.Rows.Add( row );
        }

        //create a DocumentLayout object using the dplx
        DocumentLayout report = new DocumentLayout( @"DataTable.dplx" );
        //Get the query element so that you can set the datatable as the source without using the database
        Query query = (Query)report.GetElementById( "Query" );
        query.OpeningRecordSet += new OpeningRecordSetEventHandler( mainQuery_OpeningRecordSet );
        Document doc = report.Run();
        doc.Draw( "test.pdf" );
        System.Diagnostics.Process.Start( @"test.pdf" );

    }

    private static void mainQuery_OpeningRecordSet( object sender, OpeningRecordSetEventArgs e )
    {
        // populate the main report's data.
        e.RecordSet = new DataTableRecordSet( mainTable );

        // call the sub report event handler to populate the sub-report.
        foreach ( DataRow row in mainTable.Rows )
        {
            SubReport rp = (SubReport)e.LayoutWriter.DocumentLayout.GetElementById( "SubReport1" );
            rp.Query.OpeningRecordSet += new OpeningRecordSetEventHandler( subReport_OpeningSubRecordSet );
        }
    }


    private static void subReport_OpeningSubRecordSet( object sender, OpeningRecordSetEventArgs e )
    {
        //Get the current Id and use it to retrieve the data. 
        string x = e.LayoutWriter.RecordSets.Current["Id"].ToString();

        //Create a table off the child table structure to display in the subreport
        DataTable tableforSubReport = new DataTable();
        tableforSubReport.Columns.Add( "Id" );
        tableforSubReport.Columns.Add( "Address" );

        foreach ( DataRow row in childTable.Rows )
        {
            if ( x.Equals( row["Id"].ToString() ) )
            {
                DataRow row1 = tableforSubReport.NewRow();
                row1["Id"] = row["Id"];
                row1["Address"] = row["Address"];
                tableforSubReport.Rows.Add( row1 );
            }
        }
        e.RecordSet = new DataTableRecordSet( tableforSubReport );

        // call the child sub report event handler to populate the sub-report.
        foreach ( DataRow row in mainTable.Rows )
        {
            SubReport rp = (SubReport)e.LayoutWriter.DocumentLayout.GetElementById( "SubReport2" );
            rp.Query.OpeningRecordSet += new OpeningRecordSetEventHandler( childSubReport_OpeningSubRecordSet );
        }
    }

    private static void childSubReport_OpeningSubRecordSet( object sender, OpeningRecordSetEventArgs e )
    {
        //Get the current Id and use it to retrieve the data.
        string x = e.LayoutWriter.RecordSets.Current["Id"].ToString();

        //Create a table off the child table structure to display in the subreport
        DataTable tableforChildSubReport = new DataTable();
        tableforChildSubReport.Columns.Add( "Id" );
        tableforChildSubReport.Columns.Add( "Address" );

        foreach ( DataRow row in childTable.Rows )
        {
            if ( x.Equals( row["Id"].ToString() ) )
            {
                DataRow row1 = tableforChildSubReport.NewRow();
                row1["Id"] = row["Id"];
                row1["Address"] = row["Address"];
                tableforChildSubReport.Rows.Add( row1 );
            }
        }
        e.RecordSet = new DataTableRecordSet( tableforChildSubReport );
    }

}