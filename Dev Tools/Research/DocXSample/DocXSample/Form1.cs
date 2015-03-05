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

            // Create a new document using DocX Library
            using ( DocX document = DocX.Create( path + @"\LetterOut.docx" ) )
            {

                for ( int i = 0; i < 4; i++ )
                {
                    DocX template = DocX.Load( path + @"\letter-template - insane formatting2.docx" );
                    template.ReplaceText( "<<Relation>>", relations[i], false );
                    template.ReplaceText( "<<Season>>", seasons[i], false );
                    document.InsertDocument( template );

                    // insert page break
                    if ( i != 3 )
                    {
                        Novacode.Paragraph p1 = document.InsertParagraph( "", false );
                        p1.InsertPageBreakAfterSelf();
                    }

                    template.Dispose();
                }

                document.Save();
            }
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
            string[] coolness = { "Pretty Cool", "Kinda Cool", "Exteremely Cool" };

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

            MemoryStream outputDocStream = new MemoryStream();

            using ( var letterTemplateStream = new FileStream( path + @"\letter-template - insane formatting3.docx", FileMode.Open, FileAccess.ReadWrite ) )
            {
                // Start by creating a new document with the contents of the Template (so that Styles, etc get included)
                letterTemplateStream.CopyTo( outputDocStream );
                outputDocStream.Seek( 0, SeekOrigin.Begin );

                using ( WordprocessingDocument outputDoc = WordprocessingDocument.Open( outputDocStream, true ) )
                {
                    var newDocBody = outputDoc.MainDocumentPart.Document.Body;

                    // start with a clean body
                    newDocBody.RemoveAllChildren();

                    // loop thru each merge item, using the template
                    for ( int i = 0; i < 4; i++ )
                    {
                        // create our replacement dictionary
                        Dictionary<string, string> replacements = new Dictionary<string, string>();
                        replacements.Add( "<<Relation>>", relations[i] );
                        replacements.Add( "<<Season>>", seasons[i] );
                        replacements.Add( @"<<LAVA>>.<<LAVA/>>", @"block of lava" );
                        replacements.Add( @".*\{\{.+\}\}.*", @"chunk of lava" );

                        var tempMergeDocStream = new MemoryStream();
                        letterTemplateStream.Position = 0;
                        letterTemplateStream.CopyTo( tempMergeDocStream );
                        tempMergeDocStream.Position = 0;
                        var tempMergeDoc = WordprocessingDocument.Open( tempMergeDocStream, true );

                        var xdoc = tempMergeDoc.MainDocumentPart.GetXDocument();
                        foreach ( var r in replacements )
                        {
                            OpenXmlRegex.Match( xdoc.Nodes().OfType<XElement>(), new Regex( r.Key, RegexOptions.Multiline ), ( x, m ) =>
                            {
                                string todo = "hello";
                            } );

                            OpenXmlRegex.Replace( xdoc.Nodes().OfType<XElement>(), new Regex( r.Key ), r.Value, ( x, m ) =>
                                            {
                                                return true;
                                            } );

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

                    // remove last page break
                    var lastBr = newDocBody.LastChild as DocumentFormat.OpenXml.Wordprocessing.Break;
                    if ( lastBr != null )
                    {
                        newDocBody.RemoveChild( lastBr );
                    }
                }
            }

            // Save to disk
            File.WriteAllBytes( path + @"\LetterOut_OpenXML.docx", outputDocStream.ToArray() );
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

                                XDocument xdoc = XDocument.Parse( cell.OuterXml );

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

        private void btnMakeTableOpenXML_Click( object sender, EventArgs e )
        {
            //
        }
    }
}


