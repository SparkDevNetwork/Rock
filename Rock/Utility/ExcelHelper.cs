// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;

using OfficeOpenXml;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Utility
{
    /// <summary>
    /// Methods that facilitate generating Excel files
    /// </summary>
    public static class ExcelHelper
    {
        /// <summary>
        /// The general format
        /// </summary>
        public const string GeneralFormat = "General";

        /// <summary>
        /// The currency format
        /// </summary>
        public const string CurrencyFormat = "_($* #,##0.00_);_($* (#,##0.00);_($* \"-\"??_);_(@_)";

        /// <summary>
        /// The accounting format
        /// </summary>
        public const string AccountingFormat = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";

        /// <summary>
        /// The accounting no cents format
        /// </summary>
        public const string AccountingNoCentsFormat = "_(* #,##0_);_(* (#,##0);_(* \"-\"??_);_(@_)";

        /// <summary>
        /// The date format
        /// </summary>
        public const string DateFormat = "MM/dd/yyyy";

        /// <summary>
        /// The date time format
        /// </summary>
        public const string DateTimeFormat = "MM/dd/yyyy hh:mm";

        /// <summary>
        /// The text format
        /// </summary>
        public const string TextFormat = "@";

        /// <summary>
        /// The unformatted number format
        /// </summary>
        public const string UnformattedNumberFormat = "0";

        /// <summary>
        /// The decimal format
        /// </summary>
        public const string DecimalFormat = "#,##0.00";

        /// <summary>
        /// The formatted number format
        /// </summary>
        public const string FormattedNumberFormat = "#,##0";

        /// <summary>
        /// Creates an Excel Workbook from a DataSet
        /// </summary>
        /// <param name="sqlResults">The DataSet to populate a workbook with. The TableName of each DataTable will be the header and Tab name of each sheet.</param>
        /// <param name="title">The Title of the workbook</param>
        /// <returns></returns>
        public static ExcelPackage CreateNewFile( DataSet sqlResults = null, string title = null )
        {
            var excel = new ExcelPackage();
            excel.Workbook.Properties.Title = title;
            excel.Workbook.Properties.Author = UserLoginService.GetCurrentUser()?.Person?.FullName ?? "Rock";

            // Create the worksheet(s)
            foreach ( DataTable data in sqlResults.Tables )
            {
                excel.AddWorksheet( data );
            }

            return excel;
        }

        /// <summary>
        /// Adds a worksheet from a DataTable
        /// </summary>
        /// <param name="excel">The ExcelPackage object to add a worksheet to</param>
        /// <param name="data">The DataTable to populate the worksheet with. The TableName of the DataTable will be the header and Tab name of the worksheet.</param>
        public static void AddWorksheet( this ExcelPackage excel, DataTable data )
        {
            var worksheet = excel.Workbook.Worksheets.Add( data.TableName );

            var headerRows = 3;
            var rowCounter = headerRows;
            var columnCounter = 0;

            // Set up the columns
            foreach ( DataColumn column in data.Columns )
            {
                columnCounter++;

                // Print column headings
                worksheet.Cells[rowCounter, columnCounter].Value = column.ColumnName.SplitCase();

                // Set the initial column format
                worksheet.Column( columnCounter ).Style.Numberformat.Format = DefaultColumnFormat( column.DataType );
            }

            // Populate the cells with data
            foreach ( DataRow row in data.Rows )
            {
                rowCounter++;

                for ( int i = 0; i < data.Columns.Count; i++ )
                {
                    var value = row[i];
                    SetExcelValue( worksheet.Cells[rowCounter, i + 1], value );

                    // Update column formatting based on data
                    worksheet.Column( i + 1 ).Style.Numberformat.Format = FinalColumnFormat( value, worksheet.Column( i + 1 ).Style.Numberformat.Format );
                }
            }

            worksheet.FormatWorksheet( data.TableName, headerRows, rowCounter, columnCounter );
        }

        /// <summary>
        /// Apply the default Rock worksheet formatting
        /// </summary>
        /// <param name="worksheet">The worksheet to be formatted</param>
        /// <param name="title">The title to display in the worksheet header</param>
        /// <param name="headerRows">The number of rows to use for the header</param>
        /// <param name="rows">The number of rows populated in the worksheet</param>
        /// <param name="columns">The number of columns populated in the worksheet</param>
        public static void FormatWorksheet( this ExcelWorksheet worksheet, string title, int headerRows, int rows, int columns )
        {
            var range = worksheet.Cells[headerRows, 1, rows, columns];

            // align text to the top of the cell
            range.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;

            // use conditionalFormatting to create the alternate row style
            var conditionalFormatting = range.ConditionalFormatting.AddExpression();
            conditionalFormatting.Formula = "MOD(ROW()+1,2)=0";
            conditionalFormatting.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            conditionalFormatting.Style.Fill.BackgroundColor.Color = Color.FromArgb( 240, 240, 240 );

            // Remove paces and line breaks. Replace worksheet title unallowed characters with '_'.
            var tableTitle = title
                .Replace( " ", "" )
                .Replace( Environment.NewLine, "" )
                .Replace( "\x0A", "" )
                .ReplaceSpecialCharacters( "_" )
                .TrimEnd( '_' );

            // First character cannot be a number but other ones can.
            if ( !char.IsLetter( tableTitle[0] ) )
            {
                tableTitle = $"_{tableTitle}";
            }

            var table = worksheet.Tables.Add( range, tableTitle );

            // ensure each column in the table has a unique name
            var columnNames = worksheet.Cells[headerRows, 1, headerRows, columns].Select( a => new { OrigColumnName = a.Text, Cell = a } ).ToList();
            columnNames.Reverse();
            foreach ( var col in columnNames )
            {
                int duplicateSuffix = 0;
                string uniqueName = col.OrigColumnName;

                // increment the suffix by 1 until there is only one column with that name
                while ( columnNames.Where( a => a.Cell.Text == uniqueName ).Count() > 1 )
                {
                    duplicateSuffix++;
                    uniqueName = col.OrigColumnName + duplicateSuffix.ToString();
                    col.Cell.Value = uniqueName;
                }
            }

            table.ShowHeader = true;
            table.ShowFilter = true;
            table.TableStyle = OfficeOpenXml.Table.TableStyles.None;

            // Format header range
            using ( ExcelRange r = worksheet.Cells[headerRows, 1, headerRows, columns] )
            {
                r.Style.Font.Bold = true;
                r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                r.Style.Fill.BackgroundColor.SetColor( Color.FromArgb( 223, 223, 223 ) );
                r.Style.Font.Color.SetColor( Color.Black );
                r.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            }

            // Format and set title
            worksheet.Cells[1, 1].Value = title;
            using ( ExcelRange r = worksheet.Cells[1, 1, 1, columns] )
            {
                r.Merge = true;
                r.Style.Font.SetFromFont( new Font( "Calibri", 22, FontStyle.Regular ) );
                r.Style.Font.Color.SetColor( Color.White );
                r.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                r.Style.Fill.BackgroundColor.SetColor( Color.FromArgb( 34, 41, 55 ) );

                // set border
                r.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                r.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                r.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                r.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            }

            // TODO: add image to worksheet

            worksheet.View.FreezePanes( headerRows + 1, 1 );

            // do AutoFitColumns on no more than the first 10000 rows (10000 can take 4-5 seconds, but could take several minutes if there are 100000+ rows )
            int autoFitRows = Math.Min( rows, 10000 );
            var autoFitRange = worksheet.Cells[headerRows, 1, autoFitRows, columns];

            autoFitRange.AutoFitColumns();

            // TODO: add alternating highlights

            // set some footer text
            worksheet.HeaderFooter.OddHeader.CenteredText = title;
            worksheet.HeaderFooter.OddFooter.RightAlignedText = string.Format( "Page {0} of {1}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages );
        }

        /// <summary>
        /// Sets the default formatting for an excel column based on the data type
        /// </summary>
        /// <param name="type">The data type of the column</param>
        /// <returns></returns>
        public static string DefaultColumnFormat( Type type )
        {
            if ( type == typeof( DateTime ) || type == typeof( DateTime? ) )
            {
                return DateFormat;
            }
            else if ( type == typeof( decimal ) || type == typeof( decimal? )
                || type == typeof( double ) || type == typeof( double? )
                || type == typeof( float ) || type == typeof( float? ) )
            {
                return FormattedNumberFormat;
            }
            else if ( type == typeof( int ) || type == typeof( int? )
                || type == typeof( long ) || type == typeof( long? )
                || type == typeof( short ) || type == typeof( short? ) )
            {
                return UnformattedNumberFormat;
            }

            return GeneralFormat;
        }

        /// <summary>
        /// Sets the default formatting for an excel column based on the field type and data type
        /// </summary>
        /// <param name="field">The grid field</param>
        /// <param name="exportValue">An example value for this column; you can use the data from the first row</param>
        /// <returns></returns>
        public static string DefaultColumnFormat( IRockGridField field, object exportValue )
        {
            if ( field is CurrencyField )
            {
                return DecimalFormat;
            }

            return DefaultColumnFormat( exportValue?.GetType() );
        }

        /// <summary>
        /// Updates the excel column format based on the level of detail in the data
        /// </summary>
        /// <param name="exportValue">The cell value to use for formatting</param>
        /// <param name="defaultFormat">The existing column format to use if no changes are to be made</param>
        /// <returns></returns>
        public static string FinalColumnFormat( object exportValue, string defaultFormat )
        {
            var dateTimeValue = exportValue as DateTime?;
            if ( dateTimeValue != null && dateTimeValue.Value.TimeOfDay.TotalSeconds > 0 )
            {
                return DateTimeFormat;
            }

            var dateValue = exportValue as DateTime?;
            if ( dateValue != null && dateTimeValue.Value.TimeOfDay.TotalSeconds == 0 )
            {
                return DateFormat;
            }

            var numValue = exportValue as decimal?;
            if ( numValue != null && numValue.Value - Math.Round( numValue.Value ) != 0 )
            {
                return DecimalFormat;
            }

            return defaultFormat;
        }

        /// <summary>
        /// Updates the excel column format based on the level of detail in the data
        /// </summary>
        /// <param name="worksheet">The worksheet.</param>
        /// <param name="columnCounter">The column counter.</param>
        /// <param name="exportValue">The export value.</param>
        public static void FinalizeColumnFormat( ExcelWorksheet worksheet, int columnCounter, object exportValue )
        {
            var valueFormat  = ExcelHelper.FinalColumnFormat( exportValue, null );
            if ( valueFormat == null)
            {
                return;
            }

            var currentFormat = worksheet.Column( columnCounter ).Style.Numberformat.Format;
            if ( currentFormat != valueFormat)
            {
                worksheet.Column( columnCounter ).Style.Numberformat.Format = valueFormat;
            }
        }

        /// <summary>
        /// Formats the export value.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <param name="exportValue">The export value.</param>
        public static void SetExcelValue( ExcelRange range, object exportValue )
        {
            if ( exportValue != null &&
                ( exportValue is decimal || exportValue is decimal? ||
                exportValue is int || exportValue is int? ||
                exportValue is short || exportValue is short? ||
                exportValue is long || exportValue is long? ||
                exportValue is double || exportValue is double? ||
                exportValue is float || exportValue is float? ||
                exportValue is DateTime || exportValue is DateTime? ) )
            {
                range.Value = exportValue;
            }
            else
            {
                string value = exportValue != null ? exportValue.ToString().ConvertBrToCrLf().Replace( "&nbsp;", " " ).TrimEnd( '\r', '\n' ) : string.Empty;
                range.Value = value;
                if ( value.Contains( Environment.NewLine ) )
                {
                    range.Style.WrapText = true;
                }
            }
        }

        /// <summary>
        /// Saves an ExcelPackage as a BinaryFile and stores it in the database
        /// </summary>
        /// <param name="excel">The ExcelPackage object to save</param>
        /// <param name="fileName">The filename to save the workbook as</param>
        /// <param name="rockContext">The RockContext to use</param>
        /// <param name="binaryFileType">Optionally specifies the BinaryFileType to apply to this file for security purposes</param>
        /// <returns></returns>
        public static BinaryFile Save( ExcelPackage excel, string fileName, RockContext rockContext, BinaryFileType binaryFileType = null )
        {
            if ( binaryFileType == null )
            {
                binaryFileType = new BinaryFileTypeService( rockContext ).Get( Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid() );
            }

            var ms = excel.ToMemoryStream();

            var binaryFile = new BinaryFile()
            {
                Guid = Guid.NewGuid(),
                IsTemporary = true,
                BinaryFileTypeId = binaryFileType.Id,
                MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName = fileName,
                ContentStream = ms
            };

            var binaryFileService = new BinaryFileService( rockContext );
            binaryFileService.Add( binaryFile );
            rockContext.SaveChanges();
            return binaryFile;
        }

        /// <summary>
        /// Convert an ExcelPackage to a MemoryStream
        /// </summary>
        /// <param name="excel">The ExcelPackage</param>
        /// <returns></returns>
        public static MemoryStream ToMemoryStream( this ExcelPackage excel )
        {
            MemoryStream ms = new MemoryStream();
            excel.SaveAs( ms );
            return ms;
        }

        /// <summary>
        /// Convert an ExcelPackage to a ByteArray
        /// </summary>
        /// <param name="excel">The ExcelPackage</param>
        /// <returns></returns>
        public static byte[] ToByteArray( this ExcelPackage excel )
        {
            return excel.ToMemoryStream().ToArray();
        }

        /// <summary>
        /// Sends an Excel workbook to the user
        /// </summary>
        /// <param name="excel">The ExcelPackage to send</param>
        /// <param name="page">The current Page object</param>
        /// <param name="filename">The filename to send the file as</param>
        public static void SendToBrowser( this ExcelPackage excel, System.Web.UI.Page page, string filename )
        {
            page.EnableViewState = false;
            page.Response.Clear();
            page.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            page.Response.AppendHeader( "Content-Disposition", "attachment; filename=" + filename );

            page.Response.Charset = string.Empty;
            page.Response.BinaryWrite( excel.ToByteArray() );
            page.Response.Flush();
            page.Response.End();
        }
    }
}
