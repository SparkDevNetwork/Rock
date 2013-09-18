using System;
using System.Collections.Generic;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using Rock.Model;
using System.Linq;
using Rock.Web.Cache;

namespace ContributionStatementApp
{
    /// <summary>
    /// 
    /// </summary>
    public class ContributionForm
    {
        /// <summary>
        /// The document
        /// </summary>
        private Document document;

        /// <summary>
        /// The text frame of the MigraDoc document that contains the address.
        /// </summary>
        //private TextFrame returnAddressFrame;

        /// <summary>
        /// The table of the MigraDoc document that contains the invoice items.
        /// </summary>
        private Table table;

        // migradoc sample colors
        private readonly static Color TableBorder = new Color( 81, 125, 192 );
        private readonly static Color TableBlue = new Color( 235, 240, 249 );
        private readonly static Color TableGray = new Color( 242, 242, 242 );

        public Document CreateDocument( IQueryable<FinancialTransaction> financialTransactionQry )
        {
            this.document = new Document();

            this.document.Info.Title = "Contribution Statement - proof of concept";
            this.document.Info.Subject = "the subject";
            this.document.Info.Author = "the author";

            DefineStyles();
            CreatePage();

            FillContent( financialTransactionQry );

            return this.document;
        }

        /// <summary>
        /// Defines the styles used to format the MigraDoc document.
        /// </summary>
        private void DefineStyles()
        {
            // Get the predefined style Normal.
            Style style = this.document.Styles["Normal"];

            //// Because all styles are derived from Normal, the next line changes the 
            //// font of the whole document. Or, more exactly, it changes the font of
            //// all styles and paragraphs that do not redefine the font.
            style.Font.Name = "Verdana";

            style = this.document.Styles[StyleNames.Header];
            style.ParagraphFormat.AddTabStop( "16cm", TabAlignment.Right );

            style = this.document.Styles[StyleNames.Footer];
            style.ParagraphFormat.AddTabStop( "8cm", TabAlignment.Center );

            // Create a new style called Table based on style Normal
            style = this.document.Styles.AddStyle( "Table", "Normal" );
            style.Font.Name = "Verdana";
            style.Font.Name = "Times New Roman";
            style.Font.Size = 9;

            // Create a new style called Reference based on style Normal
            style = this.document.Styles.AddStyle( "Reference", "Normal" );
            style.ParagraphFormat.SpaceBefore = "5mm";
            style.ParagraphFormat.SpaceAfter = "5mm";
            style.ParagraphFormat.TabStops.AddTabStop( "16cm", TabAlignment.Right );
        }

        /// <summary>
        /// Creates the static parts of the invoice.
        /// </summary>
        private void CreatePage()
        {
            // Each MigraDoc document needs at least one section.
            Section section = this.document.AddSection();

            // Create header
            Paragraph headerParagraph = section.Headers.Primary.AddParagraph();
            headerParagraph.AddFormattedText( "Rock Solid Church Giving Statement", TextFormat.Bold );
            headerParagraph.AddTab();

            headerParagraph.AddFormattedText( "Aug 1, 2013 - Aug 31, 2013" );
            Paragraph headerParagraphLine2 = section.Headers.Primary.AddParagraph();
            headerParagraphLine2.AddTab();
            headerParagraphLine2.AddText( "as of " + DateTime.Now.ToShortDateString() );

            // Create the item table
            this.table = section.AddTable();
            this.table.Style = "Table";
            this.table.Borders.Visible = true;
            this.table.Borders.Color = TableBorder;
            this.table.Borders.Width = 0.25;
            this.table.Borders.Left.Width = 0.5;
            this.table.Borders.Right.Width = 0.5;
            this.table.Rows.LeftIndent = 0;
            this.table.Format.SpaceBefore = 3;

            /* Define the columns */

            // Date
            Column column = this.table.AddColumn( "2cm" );
            column.Shading.Color = TableGray;
            column.Borders.Visible = false;
            column.Format.Alignment = ParagraphAlignment.Left;

            // Payment Type
            column = this.table.AddColumn( "2.5cm" );
            column.Borders.Visible = false;
            column.Format.Alignment = ParagraphAlignment.Left;

            // Note
            column = this.table.AddColumn( "6cm" );
            column.Shading.Color = TableGray;
            column.Borders.Visible = false;
            column.Format.Alignment = ParagraphAlignment.Left;

            // Account
            column = this.table.AddColumn( "3.5cm" );
            column.Borders.Visible = false;
            column.Format.Alignment = ParagraphAlignment.Left;

            // Amount
            column = this.table.AddColumn( "2cm" );
            column.Shading.Color = TableGray;
            column.Borders.Visible = false;
            column.Format.Alignment = ParagraphAlignment.Right;

            // Create the header of the table
            Row headerRow = table.AddRow();
            headerRow.HeadingFormat = true;
            headerRow.Format.Alignment = ParagraphAlignment.Center;
            headerRow.Format.Font.Bold = true;
            headerRow.Shading.Color = new Color( 255, 255, 255 );
            headerRow.Cells[0].AddParagraph( "Date" );
            headerRow.Cells[0].Shading.Color = Color.Empty;
            headerRow.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            headerRow.Cells[1].AddParagraph( "Payment Type" );
            headerRow.Cells[1].Format.Alignment = ParagraphAlignment.Left;
            headerRow.Cells[2].AddParagraph( "Note" );
            headerRow.Cells[2].Format.Alignment = ParagraphAlignment.Left;
            headerRow.Cells[3].AddParagraph( "Account" );
            headerRow.Cells[3].Format.Alignment = ParagraphAlignment.Left;
            headerRow.Cells[4].AddParagraph( "Amount" );
            headerRow.Cells[4].Format.Alignment = ParagraphAlignment.Right;


            // Create the text frame for the address
            TextFrame returnAddressFrame = section.AddTextFrame();
            returnAddressFrame.Height = "3.0cm";
            returnAddressFrame.Width = "7.0cm";
            returnAddressFrame.Left = ShapePosition.Left;
            returnAddressFrame.RelativeHorizontal = RelativeHorizontal.Margin;
            returnAddressFrame.Top = "7.5in";
            returnAddressFrame.RelativeVertical = RelativeVertical.Line;
            
            // Populate Return Address Frame
            Paragraph addressParagraph = returnAddressFrame.AddParagraph();

            addressParagraph.Add( new Text( "Rock Solid Church" ) );
            addressParagraph.AddLineBreak();
            addressParagraph.Add( new Text( "101 Electric Ave" ) );
            addressParagraph.AddLineBreak();
            addressParagraph.Add( new Text( "Big Stone City, SD 57001" ) );
            addressParagraph.Format.Font.Name = "Times New Roman";
            addressParagraph.Format.Font.Size = 7;
            
        }

        /// <summary>
        /// Fills the content.
        /// </summary>
        /// <param name="financialTransactionList">The financial transaction list.</param>
        private void FillContent( IQueryable<FinancialTransaction> financialTransactionQry )
        {
            var selectQry = financialTransactionQry.Select( a => new
            {
                a.AuthorizedPerson,
                a.TransactionDateTime,
                a.CurrencyTypeValueId,
                a.Summary,
                AccountName = a.TransactionDetails.FirstOrDefault().Account.Name,
                a.Amount
            } );

            var personTransactionGroupBy = selectQry
                .OrderBy( t => t.TransactionDateTime )
                .GroupBy( a => a.AuthorizedPerson )
                .OrderBy( p => p.Key.FullNameLastFirst );
            

            foreach ( var personTransactionList in personTransactionGroupBy )
            {
                Person person =personTransactionList.Key;
                this.table.AddRow();
                Row personRow = this.table.AddRow();
                this.table.AddRow();
                personRow.Cells[0].AddParagraph( person.FullName );
                foreach ( var financialTransaction in personTransactionList )
                {
                    Row row = this.table.AddRow();
                    row.Cells[0].AddParagraph( financialTransaction.TransactionDateTime.Value.ToShortDateString() );
                    row.Cells[1].AddParagraph( DefinedValueCache.Read( financialTransaction.CurrencyTypeValueId ?? 0 ).Name );
                    row.Cells[2].AddParagraph( financialTransaction.Summary );

                    string accountName = string.Empty;
                    row.Cells[3].AddParagraph( financialTransaction.AccountName );

                    row.Cells[4].AddParagraph( financialTransaction.Amount.ToString() );
                }
                
                //return;
            }
        }
    }
}
