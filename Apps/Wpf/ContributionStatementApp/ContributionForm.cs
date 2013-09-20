using System;
using System.Collections.Generic;
using System.Linq;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using Rock.Model;
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
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// The table gray
        /// </summary>
        private readonly static Color TableGray = new Color( 242, 242, 242 );
        private readonly static Color TableBorder = new Color( 0, 0, 0 );

        /// <summary>
        /// Creates the document.
        /// </summary>
        /// <param name="financialTransactionQry">The financial transaction qry.</param>
        /// <returns></returns>
        public Document CreateDocument( IQueryable<FinancialTransaction> financialTransactionQry )
        {
            this.document = new Document();

            this.document.Info.Title = "Contribution Statement";
            this.document.Info.Subject = "";
            this.document.Info.Author = "";

            DefineStyles();

            // Each MigraDoc document needs at least one section.
            Section section = this.document.AddSection();

            // Create header
            HeaderFooter header = section.Headers.Primary;
            Paragraph headerParagraph = header.AddParagraph();
            headerParagraph.Format.Font.Bold = true;
            headerParagraph.AddText( string.Format( "Rock Solid Church Giving Statement\t{0} - {1}", this.StartDate.ToShortDateString(), this.EndDate.ToShortDateString() ) );

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

            Style headerStyle = this.document.Styles[StyleNames.Header];
            headerStyle.ParagraphFormat.AddTabStop( "16cm", TabAlignment.Right );


            Style footerStyle = this.document.Styles[StyleNames.Footer];
            footerStyle.ParagraphFormat.AddTabStop( "8cm", TabAlignment.Center );

            // Create a new style called Table based on style Normal
            Style tableStyle = this.document.Styles.AddStyle( "Table", "Normal" );
            tableStyle.Font.Name = "Verdana";
            tableStyle.Font.Name = "Times New Roman";
            tableStyle.Font.Size = 8;

            // Create a new style called Reference based on style Normal
            Style referenceStyle = this.document.Styles.AddStyle( "Reference", "Normal" );
            referenceStyle.ParagraphFormat.SpaceBefore = "5mm";
            referenceStyle.ParagraphFormat.SpaceAfter = "5mm";
            referenceStyle.ParagraphFormat.TabStops.AddTabStop( "16cm", TabAlignment.Right );
        }

        /// <summary>
        /// Creates the static parts of the invoice.
        /// </summary>
        private void CreateHeaderAndAddresses( Person person )
        {
            Image image = document.LastSection.AddImage( @"C:\Users\mpeterson\Pictures\ccv_sig_logo.png" );
            document.LastSection.AddParagraph( "" ).Format.SpaceAfter = "0.5cm";

            // Letter Size is 215.9mm x 279.4 mm (8.5 x 11 inches)

            // Create the text frame for the return address
            TextFrame returnAddressFrame = document.LastSection.AddTextFrame();
            returnAddressFrame.Height = "3.0cm";
            returnAddressFrame.Width = "7.0cm";
            returnAddressFrame.Left = ShapePosition.Left;
            returnAddressFrame.RelativeHorizontal = RelativeHorizontal.Margin;
            returnAddressFrame.Top = "22.0cm";
            returnAddressFrame.RelativeVertical = RelativeVertical.Page;

            // Populate Return Address Frame
            // TODO Get Real Address
            Paragraph addressParagraph = returnAddressFrame.AddParagraph();
            addressParagraph.Format.Font.Name = "Times New Roman";
            addressParagraph.Format.Font.Size = 7;
            addressParagraph.AddText( string.Format( "{0}\n{1}\n{2}",
                "Rock Solid Church",
                "100 Electric Ave",
                "Big Stone, SD 57101" ) );

            // Create the text frame for the return address
            TextFrame personAddressFrame = document.LastSection.AddTextFrame();
            personAddressFrame.Height = "3.0cm";
            personAddressFrame.Width = "7.0cm";
            personAddressFrame.Left = ShapePosition.Left;
            personAddressFrame.RelativeHorizontal = RelativeHorizontal.Margin;
            personAddressFrame.Top = "25.0cm";
            personAddressFrame.RelativeVertical = RelativeVertical.Page;

            // Populate Return Address Frame
            // TODO Get Real Address
            Paragraph personAddressParagraph = personAddressFrame.AddParagraph();
            personAddressParagraph.Format.Font.Name = "Times New Roman";
            personAddressParagraph.Format.Font.Size = 7;
            personAddressParagraph.AddText( string.Format( "{0}\n{1}\n{2}",
                person.FullName,
                "123 todo",
                "Todo, TD 12345" ) );
        }

        private Table AddTransactionListTable()
        {
            // Create the item table
            Table result = this.document.Sections[0].AddTable();

            result.Style = "Table";
            result.Borders.Color = TableBorder;
            result.Borders.Visible = false;
            result.Borders.Width = 0.25;
            result.Borders.Left.Width = 0.5;
            result.Borders.Right.Width = 0.5;
            result.Rows.LeftIndent = 0;

            /* Define the columns */

            // Date
            Column column = result.AddColumn( "1.4cm" );
            column.Shading.Color = TableGray;
            column.Borders.Visible = false;
            column.Borders.Left.Visible = true;
            column.Format.Alignment = ParagraphAlignment.Left;

            // Payment Type
            column = result.AddColumn( "2.1cm" );
            column.Borders.Visible = false;
            column.Format.Alignment = ParagraphAlignment.Left;

            // Note
            column = result.AddColumn( "4cm" );
            column.Shading.Color = TableGray;
            column.Borders.Visible = false;
            column.Format.Alignment = ParagraphAlignment.Left;

            // Account
            column = result.AddColumn( "3.5cm" );
            column.Borders.Visible = false;
            column.Format.Alignment = ParagraphAlignment.Left;

            // Amount
            column = result.AddColumn( "1.5cm" );
            column.Shading.Color = TableGray;
            column.Borders.Visible = false;
            column.Borders.Right.Visible = true;
            column.Format.Alignment = ParagraphAlignment.Right;

            // Create the header of the table
            Row headerRow = result.AddRow();
            headerRow.HeadingFormat = true;
            headerRow.Format.Alignment = ParagraphAlignment.Center;
            headerRow.Format.Font.Bold = true;
            headerRow.Shading.Color = new Color( 255, 255, 255 );
            headerRow.Cells[0].Borders.Left.Visible = false;
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
            headerRow.Cells[4].Borders.Right.Visible = false;

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
                AccountId = a.TransactionDetails.FirstOrDefault().Account.Id,
                AccountName = a.TransactionDetails.FirstOrDefault().Account.Name,
                a.Amount
            } );

            var personTransactionGroupBy = selectQry.GroupBy( a => a.AuthorizedPerson );

            //##DEBUG limit to x people

            bool firstPerson = true;

            foreach ( var personTransactionList in personTransactionGroupBy.OrderBy( a => a.Key.FullNameLastFirst ).Take( 2 ) )
            {
                if ( !firstPerson )
                {
                    this.document.LastSection.AddPageBreak();
                }

                Person person = personTransactionList.Key;
                CreateHeaderAndAddresses( person );

                var personTransactionTable = AddTransactionListTable();

                foreach ( var financialTransaction in personTransactionList.OrderBy( a => a.TransactionDateTime ) )
                {
                    Row row = personTransactionTable.AddRow();
                    row.Cells[0].AddParagraph( financialTransaction.TransactionDateTime.Value.ToString( "MMM d" ) );
                    row.Cells[1].AddParagraph( DefinedValueCache.Read( financialTransaction.CurrencyTypeValueId ?? 0 ).Name );
                    row.Cells[2].AddParagraph( financialTransaction.Summary );

                    string accountName = string.Empty;
                    row.Cells[3].AddParagraph( financialTransaction.AccountName );

                    row.Cells[4].AddParagraph( financialTransaction.Amount.ToString() );
                }

                personTransactionTable.SetEdge( 0, 1, 5, personTransactionTable.Rows.Count - 1, Edge.Box, BorderStyle.Single, .75 );

                var summaryFrame = this.document.LastSection.AddTextFrame();
                summaryFrame.RelativeHorizontal = RelativeHorizontal.Margin;
                summaryFrame.RelativeVertical = RelativeVertical.Page;
                summaryFrame.Left = "13cm";
                summaryFrame.Top = "5cm";
                var summaryTable = summaryFrame.AddTable();
                summaryTable.Style = "Table";
                summaryTable.AddColumn( "2.0cm" );
                summaryTable.AddColumn( "2.0cm" );
                var headerRow = summaryTable.AddRow();
                headerRow.Format.Font.Bold = true;
                headerRow.Borders.Visible = true;
                headerRow.Borders.Color = TableBorder;
                headerRow.Borders.Style = BorderStyle.Single;
                headerRow.Borders.Width = .25;
                headerRow.Cells[0].AddParagraph( "Summary" );
                headerRow.Cells[1].AddParagraph( "" );
                


                foreach ( var accountTrans in personTransactionList.GroupBy( a => new { a.AccountId, a.AccountName } ).OrderBy( a => a.Key.AccountName ) )
                {
                    var totalAmount = accountTrans.Sum( a => a.Amount );
                    var accountRow = summaryTable.AddRow();
                    accountRow.Cells[0].AddParagraph( accountTrans.Key.AccountName );
                    accountRow.Cells[1].AddParagraph( totalAmount.ToString() );
                }



                firstPerson = false;
            }
        }
    }
}
