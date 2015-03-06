using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Novacode;
using OpenXmlPowerTools;

namespace DocXSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // docs
        private string[] relations = { "son", "daughter", "uncle", "aunt" };
        private string[] seasons = { "spring", "summer", "fall", "winter is a great season that lasts a long time in many areas of the world, especially in the north" };
        private object[] letterMergeObjects = {
            new { Name = "Ted", Birthdate = new DateTime(1960, 5, 15) }, 
            new { Name = "Sally", Birthdate = new DateTime(1970, 1, 9) }, 
            new { Name = "Noah", Birthdate = new DateTime(2007, 11, 12) }, 
            new { Name = "Alex", Birthdate = new DateTime(2010, 2, 28) }, 
        };

        // labels
        string[] companies = { "Time Warner", "Apple", "IBM", "CCV", "Honeywell", "Amex" };
        string[] addresses = { "123 W Elm St", "352 Monroe Blvd", "2321 W Washington Ave", "1231 24th St", "3426 E Warner Rd", "211 Peterson St" };
        string[] cities = { "Phoenix", "Glendale", "Mesa", "Scottsdale", "Peoria", "Chandler" };
        string[] states = { "AZ", "AZ", "AZ", "AZ", "AZ", "AZ" };
        string[] zips = { "85383", "85697", "85452", "85452", "85741", "85732" };

        /// <summary>
        /// Merges the Letter Template using DOCX
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnGo_Click( object sender, EventArgs e )
        {
            string path = Directory.GetCurrentDirectory();
            string templatePath = path + @"\letter-template - extra formatting.docx";
            MemoryStream templateStream = new MemoryStream();
            File.OpenRead( templatePath ).CopyTo( templateStream );
            string outputDocPath = path + @"\LetterOut.docx";
            var docStream = new MemoryStream();



            // Create a new document using DocX Library
            using ( DocX document = DocX.Create( docStream ) )
            {
                //                for ( int j = 0; j < 100; j++ )
                {
                    for ( int i = 0; i < 4; i++ )
                    {
                        var itemTemplateStream = new MemoryStream();
                        templateStream.Position = 0;
                        templateStream.CopyTo( itemTemplateStream );
                        using ( DocX template = DocX.Load( itemTemplateStream ) )
                        {
                            template.ReplaceText( "<<Relation>>", relations[i], false );
                            template.ReplaceText( @".*\{\{.+\}\}.*", @"chunk of lava", false );
                            template.ReplaceText( "<<Season>>", seasons[i], false );

                            document.InsertDocument( template );

                            // insert page break
                            if ( i != 3 )
                            {
                                Novacode.Paragraph p1 = document.InsertParagraph( "", false );
                                p1.InsertPageBreakAfterSelf();
                            }
                        }
                    }
                }
                document.Save();
                document.SaveAs( outputDocPath );
            }

            System.Diagnostics.Process.Start( outputDocPath );
        }



        private void btnLabels_Click( object sender, EventArgs e )
        {
            string path = Directory.GetCurrentDirectory();

            int companyCount = 0;

            // Create a new document.
            using ( DocX document = DocX.Create( path + @"\LabelsOut.docx" ) )
            {

                DocX template = DocX.Load( path + @"\label-template.docx" );

                foreach ( Novacode.Table t in template.Tables )
                {
                    foreach ( Row r in t.Rows )
                    {
                        foreach ( Cell c in r.Cells )
                        {
                            if ( companyCount == ( companies.Length - 1 ) )
                            {
                                // out of data so hide cell contents
                                foreach ( Novacode.Paragraph p in c.Paragraphs )
                                {
                                    p.Hide();
                                }
                            }
                            else
                            {
                                // make sure cell has merge fields
                                List<int> fields = c.FindAll( "{{" );
                                if ( fields.Count > 0 )
                                {
                                    c.ReplaceText( "{{CompanyName}}", companies[companyCount] );
                                    c.ReplaceText( "{{Address}}", addresses[companyCount] );
                                    c.ReplaceText( "{{City}}", cities[companyCount] );
                                    c.ReplaceText( "{{State}}", states[companyCount] );
                                    c.ReplaceText( "{{ZipCode}}", zips[companyCount] );

                                    companyCount++;
                                }
                            }
                        }
                    }
                }

                document.InsertDocument( template );
                template.Dispose();
                document.Save();
            }
        }

        private void btnTable_Click( object sender, EventArgs e )
        {
            string path = Directory.GetCurrentDirectory();

            string[] names = { "Tom Selleck", "Tom Cruise", "Jon Edmiston" };
            string[] emails = { "ts@higgins.com", "tom@topgun.com", "jonathan.edmiston@gmail.com" };
            string[] coolness = { "Pretty Cool", "Kinda Cool", "Extremely Cool" };

            // Create a new document.
            using ( DocX document = DocX.Create( path + @"\TableOut.docx" ) )
            {
                DocX template = DocX.Load( path + @"\table-template.docx" );

                /*foreach (Table t in template.Tables)
                {
                    
                    for (int r = 0; r < 3; r++)
                    {
                        // check if this is a template row
                        List<int> commands = t.Rows[r].Cells[0].FindAll("{%tr%}", System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace);
                        if (commands.Count > 0)
                        {
                            // we have a command row

                            // remove command
                            t.Rows[r].ReplaceText("{%tr%}", "");

                            for (int i = 0; i < emails.Count(); i++)
                            {
                                
                                
                                //Row newRow =  t.InsertRow(r + 1);
                                //newRow.Xml = t.Rows[r].Xml;
                                //newRow.ReplaceText("{{Name}}", names[i]);
                                //t.InsertRow(newRow, r);
                            }

                            
                            
                        }
                    }
                }*/

                document.InsertDocument( template );


                template.Dispose();
                document.Save();
            }
        }

        /*
         
         Using OPEN XML 
         
         */

        /// <summary>
        /// Merges the Letter Template using OpenXML
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnMakeDocumentOpenXML_Click( object sender, EventArgs e )
        {
            string path = Directory.GetCurrentDirectory();
            string templatePath = path + @"\letter-template - extra formatting.docx";
            string outputDocPath = path + @"\LetterOut_OpenXML.docx";
            Regex lavaChunk = new Regex( @".*\{\{.+\}\}.*", RegexOptions.Multiline );

            MemoryStream outputDocStream = new MemoryStream();

            using ( var letterTemplateStream = new FileStream( templatePath, FileMode.Open, FileAccess.Read ) )
            {
                // Start by creating a new document with the contents of the Template (so that Styles, etc get included)
                letterTemplateStream.CopyTo( outputDocStream );
                outputDocStream.Seek( 0, SeekOrigin.Begin );

                using ( WordprocessingDocument outputDoc = WordprocessingDocument.Open( outputDocStream, true ) )
                {
                    var newDocBody = outputDoc.MainDocumentPart.Document.Body;

                    // start with a clean body
                    newDocBody.RemoveAllChildren();

                    for ( int j = 0; j < 100; j++ )
                    {
                        // loop thru each merge item, using the template
                        for ( int i = 0; i < 4; i++ )
                        {
                            // create our replacement dictionary
                            Dictionary<string, string> replacements = new Dictionary<string, string>();
                            replacements.Add( "<<Relation>>", relations[i] );
                            replacements.Add( "<<Season>>", seasons[i] );

                            var tempMergeDocStream = new MemoryStream();
                            letterTemplateStream.Position = 0;
                            letterTemplateStream.CopyTo( tempMergeDocStream );
                            tempMergeDocStream.Position = 0;
                            var tempMergeDoc = WordprocessingDocument.Open( tempMergeDocStream, true );

                            var xdoc = tempMergeDoc.MainDocumentPart.GetXDocument();

                            // first look for DotLiquid/Lava stuff
                            OpenXmlRegex.Match( xdoc.Nodes().OfType<XElement>(), lavaChunk, ( x, m ) =>
                            {
                                DotLiquid.Template template = DotLiquid.Template.Parse( m.Value );
                                var localVariables = new DotLiquid.Hash();
                                localVariables.Add( "Person", letterMergeObjects[i] );
                                var replacementValue = template.Render( localVariables );
                                OpenXmlRegex.Replace( new XElement[] { x }, lavaChunk, replacementValue, ( xx, mm ) => { return true; } );
                            } );

                            foreach ( var r in replacements )
                            {
                                string replacementValue = r.Value;
                                OpenXmlRegex.Replace( xdoc.Nodes().OfType<XElement>(), new Regex( r.Key ), r.Value, ( x, m ) => { return true; } );
                            }

                            tempMergeDoc.MainDocumentPart.PutXDocument();

                            foreach ( var childBodyItem in tempMergeDoc.MainDocumentPart.Document.Body )
                            {
                                var clonedChild = childBodyItem.CloneNode( true );
                                newDocBody.AppendChild( clonedChild );
                            }

                            // add page break
                            newDocBody.AppendChild( new DocumentFormat.OpenXml.Wordprocessing.Break() { Type = BreakValues.Page } );
                        }
                    }

                    // remove last page break
                    var lastBr = newDocBody.LastChild as DocumentFormat.OpenXml.Wordprocessing.Break;
                    if ( lastBr != null )
                    {
                        newDocBody.RemoveChild( lastBr );
                    }
                }
            }

            // Save to disk
            File.WriteAllBytes( outputDocPath, outputDocStream.ToArray() );
            System.Diagnostics.Process.Start( outputDocPath );
        }

        /// <summary>
        /// Make Labels using OpenXML
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnLabelsOpenXML_Click( object sender, EventArgs e )
        {
            string path = Directory.GetCurrentDirectory();

            // see HowTos at https://msdn.microsoft.com/EN-US/library/office/cc850849.aspx

            // Start by creating a new document with the contents of the Template (so that Styles, etc get included)
            MemoryStream outputDocStream = new MemoryStream();
            var letterTemplateStream = File.OpenRead( path + @"\label-template.docx" );
            letterTemplateStream.CopyTo( outputDocStream );
            outputDocStream.Seek( 0, SeekOrigin.Begin );

            int companyCount = 0;

            using ( WordprocessingDocument outputDoc = WordprocessingDocument.Open( outputDocStream, true ) )
            {
                var newDocBody = outputDoc.MainDocumentPart.Document.Body;
                var tables = newDocBody.ChildElements.OfType<DocumentFormat.OpenXml.Wordprocessing.Table>();

                foreach ( var table in tables )
                {
                    foreach ( var row in table.ChildElements.OfType<TableRow>() )
                    {
                        foreach ( var cell in row.ChildElements.OfType<TableCell>().Where( a => a.InnerText.Contains( "{{" ) ).ToList() )
                        {
                            if ( companyCount >= ( companies.Length - 1 ) )
                            {
                                // out of data so hide cell contents
                                var emptyCell = new TableCell( new DocumentFormat.OpenXml.Wordprocessing.Paragraph[] { new DocumentFormat.OpenXml.Wordprocessing.Paragraph() } );
                                cell.Parent.ReplaceChild( emptyCell, cell );
                            }
                            else
                            {
                                XElement[] xe = new XElement[] { XElement.Parse( cell.OuterXml ) };

                                Dictionary<string, string> replacements = new Dictionary<string, string>();

                                replacements.Add( "{{CompanyName}}", companies[companyCount] );
                                replacements.Add( "{{Address}}", addresses[companyCount] );
                                replacements.Add( "{{City}}", cities[companyCount] );
                                replacements.Add( "{{State}}", states[companyCount] );
                                replacements.Add( "{{ZipCode}}", zips[companyCount] );

                                foreach ( var r in replacements )
                                {

                                    OpenXmlRegex.Replace( xe, new Regex( r.Key ), r.Value, ( x, m ) =>
                                    {
                                        return true;
                                    } );
                                }

                                var newCell = new TableCell( xe[0].ToString() );

                                cell.Parent.ReplaceChild( newCell, cell );

                                companyCount++;
                            }


                        }
                    }
                }
            }

            // Save to disk
            File.WriteAllBytes( path + @"\LabelOut_OpenXML.docx", outputDocStream.ToArray() );
        }

        /// <summary>
        /// Handles the Click event of the btnMakeTableOpenXML control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnMakeTableOpenXML_Click( object sender, EventArgs e )
        {
            string path = Directory.GetCurrentDirectory();

            string[] names = { "Tom Selleck", "Tom Cruise", "Jon Edmiston" };
            string[] emails = { "ts@higgins.com", "tom@topgun.com", "jonathan.edmiston@gmail.com" };
            string[] coolness = { "Pretty Cool", "Kinda Cool", "Extremely Cool" };
            string[] colors = { "blue", "red", "c4c4c4" };

            var mergeObjectsList = new List<Dictionary<string, object>>();
            for ( int i = 0; i < names.Length; i++ )
            {
                var mergeObjects = new Dictionary<string, object>();
                mergeObjects.Add( "Name", names[i] );
                mergeObjects.Add( "Email", emails[i] );
                mergeObjects.Add( "Coolness", coolness[i] );
                mergeObjects.Add( "FavoriteColor", colors[i] );
                mergeObjects.Add( "DateTime", DateTime.Now );
                mergeObjectsList.Add( mergeObjects );
            }

            // Start by creating a new document with the contents of the Template (so that Styles, etc get included)
            MemoryStream outputDocStream = new MemoryStream();
            using ( var letterTemplateStream = File.OpenRead( path + @"\table-template.docx" ) )
            {
                letterTemplateStream.CopyTo( outputDocStream );
                outputDocStream.Seek( 0, SeekOrigin.Begin );
            }

            using ( WordprocessingDocument outputDoc = WordprocessingDocument.Open( outputDocStream, true ) )
            {
                var newDocBody = outputDoc.MainDocumentPart.Document.Body;
                var tables = newDocBody.ChildElements.OfType<DocumentFormat.OpenXml.Wordprocessing.Table>();

                foreach ( var table in tables )
                {
                    var templateRows = table.ChildElements.OfType<TableRow>().ToList();
                    var firstFooterRow = templateRows.Where( a => a.ChildElements.Any( c => c.InnerText.Contains( "{{" ) ) ).LastOrDefault().NextSibling();

                    // get the templateRows that have merge fields in it
                    var templateMergeFieldsRows = templateRows.Where( a => a.ChildElements.Any( c => c.InnerText.Contains( "{{" ) ) ).ToList();

                    foreach ( var mergeObjects in mergeObjectsList )
                    {
                        foreach ( var templateMergeFieldsRow in templateMergeFieldsRows )
                        {
                            XElement[] xe = new XElement[] { XElement.Parse( templateMergeFieldsRow.OuterXml ) };
                            foreach ( var mergeObject in mergeObjects )
                            {
                                OpenXmlRegex.Replace( xe, new Regex( "{{" + mergeObject.Key + "}}" ), mergeObject.Value.ToString(), ( x, m ) =>
                                {
                                    return true;
                                } );
                            }

                            if ( firstFooterRow != null )
                            {
                                table.InsertBefore( new TableRow( xe[0].ToString() ), firstFooterRow );
                            }
                            else
                            {
                                table.AppendChild( new TableRow( xe[0].ToString() ) );
                            }
                        }

                    }

                    // now that we are done with the template rows, remove them
                    foreach ( var templateMergeFieldsRow in templateMergeFieldsRows )
                    {
                        templateMergeFieldsRow.Remove();
                    }
                }
            }

            // Save to disk
            File.WriteAllBytes( path + @"\TableOut_OpenXML.docx", outputDocStream.ToArray() );
        }
    }
}


