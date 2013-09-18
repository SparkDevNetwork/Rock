using System;
using System.Collections.Generic;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using Rock.Model;
using System.Linq;
using Rock.Web.Cache;
using MigraDoc.DocumentObjectModel.Fields;

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

        private SectionField headerPersonSectionField;

        /// <summary>
        /// The text frame of the MigraDoc document that contains the address.
        /// </summary>
        //private TextFrame returnAddressFrame;

        /// <summary>
        /// The table of the MigraDoc document that contains the invoice items.
        /// </summary>
        //private Table table;

        // migradoc sample colors
        private readonly static Color TableBorder = new Color( 81, 125, 192 );
        private readonly static Color TableBlue = new Color( 235, 240, 249 );
        private readonly static Color TableGray = new Color( 242, 242, 242 );

        public Document CreateDocument( IQueryable<FinancialTransaction> financialTransactionQry )
        {
            this.document = new Document();

            this.document.Info.Title = "Contribution Statement";
            this.document.Info.Subject = "";
            this.document.Info.Author = "";

            DefineStyles();

            // 

            // Each MigraDoc document needs at least one section.
            Section section = this.document.AddSection();

            // Create header
            Paragraph headerParagraph = section.Headers.Primary.AddParagraph();
            headerParagraph.Format.Font.Bold = true;
            headerParagraph.AddText( string.Format( "Rock Solid Church Giving Statement\t{0}", "TODO: Aug 1, 2013 - Aug 31, 2013" ) );
            headerPersonSectionField = headerParagraph.AddSectionField();

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
        private void CreateHeaderAndAddresses( Person person )
        {
            //headerPersonSectionField.A

            // Create the text frame for the address
            TextFrame returnAddressFrame = document.LastSection.AddTextFrame();
            returnAddressFrame.Height = "3.0cm";
            returnAddressFrame.Width = "7.0cm";
            returnAddressFrame.Left = ShapePosition.Left;
            returnAddressFrame.RelativeHorizontal = RelativeHorizontal.Margin;
            returnAddressFrame.Top = "7.5in";
            returnAddressFrame.RelativeVertical = RelativeVertical.Line;

            // Populate Return Address Frame
            Paragraph addressParagraph = returnAddressFrame.AddParagraph();
            addressParagraph.Format.Font.Name = "Times New Roman";
            addressParagraph.Format.Font.Size = 7;
            addressParagraph.AddText( string.Format("{0}\n{1}\n{2}", 
                person.FullName, 
                "ToDo Address", 
                "ToDo City State Zip") );
            
        }

        private Table AddTransactionListTable()
        {
            // Create the item table
            Table result = this.document.Sections[0].AddTable();

            result.Style = "Table";
            result.Borders.Visible = true;
            result.Borders.Color = TableBorder;
            result.Borders.Width = 0.25;
            result.Borders.Left.Width = 0.5;
            result.Borders.Right.Width = 0.5;
            result.Rows.LeftIndent = 0;
            result.Format.SpaceBefore = 3;

            /* Define the columns */

            // Date
            Column column = result.AddColumn( "2cm" );
            column.Shading.Color = TableGray;
            column.Borders.Visible = false;
            column.Format.Alignment = ParagraphAlignment.Left;

            // Payment Type
            column = result.AddColumn( "2.5cm" );
            column.Borders.Visible = false;
            column.Format.Alignment = ParagraphAlignment.Left;

            // Note
            column = result.AddColumn( "6cm" );
            column.Shading.Color = TableGray;
            column.Borders.Visible = false;
            column.Format.Alignment = ParagraphAlignment.Left;

            // Account
            column = result.AddColumn( "3.5cm" );
            column.Borders.Visible = false;
            column.Format.Alignment = ParagraphAlignment.Left;

            // Amount
            column = result.AddColumn( "2cm" );
            column.Shading.Color = TableGray;
            column.Borders.Visible = false;
            column.Format.Alignment = ParagraphAlignment.Right;

            // Create the header of the table
            Row headerRow = result.AddRow();
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

            return result;
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
                Person person = personTransactionList.Key;
                CreateHeaderAndAddresses( person );

                var personTransactionTable = AddTransactionListTable();
                
                foreach ( var financialTransaction in personTransactionList )
                {
                    Row row = personTransactionTable.AddRow();
                    row.Cells[0].AddParagraph( financialTransaction.TransactionDateTime.Value.ToShortDateString() );
                    row.Cells[1].AddParagraph( DefinedValueCache.Read( financialTransaction.CurrencyTypeValueId ?? 0 ).Name );
                    row.Cells[2].AddParagraph( financialTransaction.Summary + person.FullName);

                    string accountName = string.Empty;
                    row.Cells[3].AddParagraph( financialTransaction.AccountName );

                    row.Cells[4].AddParagraph( financialTransaction.Amount.ToString() );
                }

                this.document.Sections[0].AddPageBreak();
            }
        }
    }
}
