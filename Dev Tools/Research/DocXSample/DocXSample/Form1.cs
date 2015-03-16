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
            string[] relations = { "son", "daughter", "uncle", "aunt" };
            string[] seasons = { "spring", "summer", "fall", "winter is a great season that lasts a long time in many areas of the world, especially in the north" };
            var path = GetOutputFolder();
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

        private static string GetOutputFolder()
        {
            var dir = new DirectoryInfo( Directory.GetCurrentDirectory() );
            var path = dir.Parent.Parent.GetDirectories( "Docs" ).FirstOrDefault().FullName;
            return path;
        }

        private void btnLabels_Click( object sender, EventArgs e )
        {
            var path = GetOutputFolder();

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
            var path = GetOutputFolder();

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

        // setting to simplify the doc so that stuff doesn't get in the way of doing Lava
        private SimplifyMarkupSettings settings = new SimplifyMarkupSettings
                    {
                        NormalizeXml = true,
                        RemoveWebHidden = true,
                        RemoveBookmarks = true,
                        RemoveGoBackBookmark = true,
                        RemoveMarkupForDocumentComparison = true,
                        RemoveComments = true,
                        RemoveContentControls = true,
                        RemoveEndAndFootNotes = true,
                        RemoveFieldCodes = false,
                        RemoveLastRenderedPageBreak = true,
                        RemovePermissions = true,
                        RemoveProof = true,
                        RemoveRsidInfo = true,
                        RemoveSmartTags = true,
                        RemoveSoftHyphens = true,
                        ReplaceTabsWithSpaces = true
                    };

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
            var path = GetOutputFolder();
            string templatePath = path + @"\letter-template - extra formatting.docx";
            string outputDocPath = path + @"\LetterOut_OpenXML.docx";
            Regex lavaRegEx = new Regex( @"\{\{.+?\}\}", RegexOptions.Multiline );

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

                    // loop thru each merge item, using the template
                    foreach ( var mergeObjects in GetSampleMergeObjectsList() )
                    {
                        var tempMergeDocStream = new MemoryStream();
                        letterTemplateStream.Position = 0;
                        letterTemplateStream.CopyTo( tempMergeDocStream );
                        tempMergeDocStream.Position = 0;
                        var tempMergeDoc = WordprocessingDocument.Open( tempMergeDocStream, true );

                        MarkupSimplifier.SimplifyMarkup( tempMergeDoc, settings );
                        var xdoc = tempMergeDoc.MainDocumentPart.GetXDocument();

                        var xml = xdoc.ToString().ReplaceWordChars();

                        DotLiquid.Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
                        DotLiquid.Template template = DotLiquid.Template.Parse( xml );

                        var mergedXml = template.Render( DotLiquid.Hash.FromDictionary( mergeObjects ) );

                        var mergedXDoc = XDocument.Parse( mergedXml );

                        //xdoc.R

                        /*OpenXmlRegex.Match( xdoc.Nodes().OfType<XElement>(), lavaRegEx, ( x, m ) =>
                        {
                            DotLiquid.Template template = DotLiquid.Template.Parse( m.Value );
                            var replacementValue = template.Render( localVariables );
                            bool didReplace = false;
                            OpenXmlRegex.Replace( new XElement[] { x }, lavaRegEx, replacementValue, ( xx, mm ) => {
                                // only replace the first occurrence
                                if ( !didReplace )
                                {
                                    didReplace = true;
                                    return true;
                                }

                                return false;
                            } );
                        } );
                         */

                        tempMergeDoc.MainDocumentPart.PutXDocument( mergedXDoc );

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
            var path = GetOutputFolder();

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
            var path = GetOutputFolder();

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


        /// <summary>
        /// Merges the Letter Template using OpenXML
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnMergeLabelsUsingNextRecord_Click( object sender, EventArgs e )
        {
            var path = GetOutputFolder();
            string templatePath = path + @"\label-template - next record.docx";
            string outputDocPath = path + @"\LabelOut_OpenXML.docx";
            var mergeObjectsList = GetSampleMergeObjectsList( 100 );

            DoMergeDoc( templatePath, outputDocPath, mergeObjectsList );
        }

        private void btnMergeLetterUsingNextRecord_Click( object sender, EventArgs e )
        {
            var path = GetOutputFolder();
            string templatePath = path + @"\letter-template - next record.docx";
            string outputDocPath = path + @"\LetterOut_OpenXML.docx";
            var mergeObjectsList = GetSampleMergeObjectsList();

            DoMergeDoc( templatePath, outputDocPath, mergeObjectsList );
        }

        /// <summary>
        /// Does the merge document.
        /// </summary>
        /// <param name="templatePath">The template path.</param>
        /// <param name="outputDocPath">The output document path.</param>
        /// <param name="mergeObjectsList">The merge objects list.</param>
        private void DoMergeDoc( string templatePath, string outputDocPath, List<Dictionary<string, object>> mergeObjectsList )
        {
            Regex nextRecordRegEx = new Regex( @"{&\s*\bnext\b\s*&}", RegexOptions.IgnoreCase );
            MemoryStream outputDocStream = new MemoryStream();

            using ( var sourceTemplateStream = new FileStream( templatePath, FileMode.Open, FileAccess.Read ) )
            {
                // Start by creating a new document with the contents of the Template (so that Styles, etc get included)
                sourceTemplateStream.CopyTo( outputDocStream );
                outputDocStream.Seek( 0, SeekOrigin.Begin );

                using ( WordprocessingDocument outputDoc = WordprocessingDocument.Open( outputDocStream, true ) )
                {
                    var xdoc = outputDoc.MainDocumentPart.GetXDocument();
                    var outputBodyNode = xdoc.DescendantNodes().OfType<XElement>().FirstOrDefault( a => a.Name.LocalName.Equals( "body" ) );
                    outputBodyNode.RemoveNodes();

                    int recordIndex = 0;
                    int recordCount = mergeObjectsList.Count();
                    while ( recordIndex < recordCount )
                    {
                        var tempMergeDocStream = new MemoryStream();
                        sourceTemplateStream.Position = 0;
                        sourceTemplateStream.CopyTo( tempMergeDocStream );
                        tempMergeDocStream.Position = 0;
                        var tempMergeWordDoc = WordprocessingDocument.Open( tempMergeDocStream, true );

                        MarkupSimplifier.SimplifyMarkup( tempMergeWordDoc, settings );
                        var tempMergeDocX = tempMergeWordDoc.MainDocumentPart.GetXDocument();
                        var tempMergeDocBodyNode = tempMergeDocX.DescendantNodes().OfType<XElement>().FirstOrDefault( a => a.Name.LocalName.Equals( "body" ) );

                        // Examples are: Body, Table Cells (Labels), Partial Page (Half Page baptism certificates), etc)

                        // find all the Nodes that have a {& next &}.  
                        List<XElement> nextIndicatorNodes = new List<XElement>();

                        OpenXmlRegex.Match( tempMergeDocX.Elements(), nextRecordRegEx, ( x, m ) =>
                        {
                            nextIndicatorNodes.Add( x );

                            // once we know the indicator node, we can clear out the "{& next &}" text
                            OpenXmlRegex.Replace( new XElement[] { x }, nextRecordRegEx, string.Empty, ( xx, mm ) => { return true; } );
                        } );

                        foreach ( var nextIndicatorNode in nextIndicatorNodes )
                        {
                            // Each of the nextIndicatorNodes will get a record until we run out of nodes or records.  
                            // If we have more records than nodes, we'll jump out to the outer "while" and append another template and keep going
                            XContainer recordContainerNode = null;
                            if ( nextIndicatorNode != null && nextIndicatorNode.Parent != null )
                            {
                                recordContainerNode = nextIndicatorNode.Parent;
                            }
                            else
                            {
                                // shouldn't happen
                                continue;
                            }

                            var xml = recordContainerNode.ToString().ReplaceWordChars();
                            XContainer mergedXRecord;

                            if ( recordIndex >= recordCount )
                            {
                                // out of records, so clear out any remaining template nodes that haven't been merged
                                mergedXRecord = XElement.Parse( xml ) as XContainer;
                                OpenXmlRegex.Replace( mergedXRecord.Nodes().OfType<XElement>(), new Regex( "." ), string.Empty, ( a, b ) => { return true; } );
                            }
                            else
                            {
                                DotLiquid.Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
                                DotLiquid.Template template = DotLiquid.Template.Parse( xml );
                                var mergedXml = template.Render( DotLiquid.Hash.FromDictionary( mergeObjectsList[recordIndex] ) );
                                mergedXRecord = XElement.Parse( mergedXml ) as XContainer;
                            }

                            // remove the orig nodes and replace with merged nodes
                            recordContainerNode.RemoveNodes();
                            foreach ( var childNode in mergedXRecord.Nodes() )
                            {
                                var xchildNode = childNode as XElement;
                                recordContainerNode.Add( xchildNode );
                            }

                            var mergedRecordContainer = XElement.Parse( recordContainerNode.ToString() );
                            var nextNode = recordContainerNode.NextNode;
                            if ( recordContainerNode.Parent != null )
                            {
                                // the recordContainerNode is some child/descendent of <body>
                                recordContainerNode.ReplaceWith( mergedRecordContainer );
                            }
                            else
                            {
                                // the recordContainerNode is the <body>
                                recordContainerNode.RemoveNodes();
                                foreach ( var node in mergedRecordContainer.Nodes() )
                                {
                                    recordContainerNode.Add( node );
                                }

                                if ( recordIndex < recordCount )
                                {
                                    // add page break
                                    var pageBreakXml = new DocumentFormat.OpenXml.Wordprocessing.Break() { Type = BreakValues.Page }.OuterXml;
                                    var pageBreak = XElement.Parse( pageBreakXml );
                                    recordContainerNode.Add( pageBreak );
                                }
                            }

                            recordIndex++;
                        }

                        foreach ( var childNode in tempMergeDocBodyNode.Nodes() )
                        {
                            outputBodyNode.Add( childNode );
                        }
                    }
                    
                    // remove the last pagebreak if there is nothing after it
                    var lastBodyElement = outputBodyNode.Nodes().OfType<XElement>().LastOrDefault();
                    if ( lastBodyElement != null && lastBodyElement.Name.LocalName == "br" )
                    {
                        if ( lastBodyElement.Parent != null )
                        {
                            lastBodyElement.Remove();
                        }
                    }
                    
                    // pop the xdoc back
                    outputDoc.MainDocumentPart.PutXDocument();
                }
            }

            // Save to disk
            File.WriteAllBytes( outputDocPath, outputDocStream.ToArray() );
            System.Diagnostics.Process.Start( outputDocPath );
        }

        /// <summary>
        /// Gets the sample merge objects list.
        /// </summary>
        /// <returns></returns>
        private static List<Dictionary<string, object>> GetSampleMergeObjectsList( int additionalRandomRecordCount = 0 )
        {
            var mergeObjectsList = new List<Dictionary<string, object>>();

            var listOfRandomColors = new List<ConsoleColor>();
            var random = new Random( 65406540 );
            for ( int c = 0; c < 5; c++ )
            {
                int randomColor = random.Next( 0, (int)ConsoleColor.White );
                listOfRandomColors.Add( (ConsoleColor)randomColor );
            }

            var mergeObjects = new Dictionary<string, object>();
            mergeObjects.Add( "Relation", "son" );
            mergeObjects.Add( "Season", "spring" );
            mergeObjects.Add( "FavoriteColors", listOfRandomColors );
            mergeObjects.Add( "DateTime", DateTime.Now );
            mergeObjects.Add( "Person", new { Name = "Ted Decker", Birthdate = new DateTime( 1960, 5, 15 ), Street1 = "100 1st St", City = "Phoenix", State = "AZ", ZipCode = "85083" } );
            mergeObjectsList.Add( mergeObjects );

            mergeObjects = new Dictionary<string, object>();
            mergeObjects.Add( "Relation", "daughter" );
            mergeObjects.Add( "Season", "summer" );
            mergeObjects.Add( "FavoriteColors", listOfRandomColors );
            mergeObjects.Add( "DateTime", DateTime.Now );
            mergeObjects.Add( "Person", new { Name = "Sally Seashell", Birthdate = new DateTime( 1970, 1, 9 ), Street1 = "200 1st St", City = "Phoenix", State = "AZ", ZipCode = "85084" } );
            mergeObjectsList.Add( mergeObjects );

            mergeObjects = new Dictionary<string, object>();
            mergeObjects.Add( "Relation", "uncle" );
            mergeObjects.Add( "Season", "fall" );
            mergeObjects.Add( "FavoriteColors", listOfRandomColors );
            mergeObjects.Add( "DateTime", DateTime.Now );
            mergeObjects.Add( "Person", new { Name = "Noah Lot", Birthdate = new DateTime( 2007, 11, 12 ), Street1 = "300 1st St", City = "Phoenix", State = "AZ", ZipCode = "85085" } );
            mergeObjectsList.Add( mergeObjects );

            mergeObjects = new Dictionary<string, object>();
            mergeObjects.Add( "Relation", "aunt" );
            mergeObjects.Add( "Season", "winter is a great season that lasts a long time in many areas of the world, especially in the north" );
            mergeObjects.Add( "FavoriteColors", listOfRandomColors );
            mergeObjects.Add( "DateTime", DateTime.Now );
            mergeObjects.Add( "Person", new { Name = "Alex Trebek", Birthdate = new DateTime( 2010, 2, 28 ), Street1 = "400 1st St", City = "Phoenix", State = "AZ", ZipCode = "85086" } );
            mergeObjectsList.Add( mergeObjects );

            for ( int randomIndex = 0; randomIndex < additionalRandomRecordCount; randomIndex++ )
            {
                string randomName = string.Format( "FirstName LastName{0}", randomIndex );
                mergeObjects = new Dictionary<string, object>();
                mergeObjects.Add( "Person", new { Name = randomName, Birthdate = new DateTime( 2010, 2, 28 ), Street1 = "400 1st St", City = "Phoenix", State = "AZ", ZipCode = "85086" } );
                mergeObjectsList.Add( mergeObjects );
            }

            return mergeObjectsList;
        }
    }
}

public static class Extensions
{
    public static string ReplaceWordChars( this string text )
    {
        var s = text;
        // smart single quotes and apostrophe
        s = Regex.Replace( s, "[\u2018\u2019\u201A]", "'" );
        // smart double quotes
        s = Regex.Replace( s, "[\u201C\u201D\u201E]", "\"" );
        // ellipsis
        s = Regex.Replace( s, "\u2026", "..." );
        // dashes
        s = Regex.Replace( s, "[\u2013\u2014]", "-" );
        // circumflex
        s = Regex.Replace( s, "\u02C6", "^" );
        // open angle bracket
        s = Regex.Replace( s, "\u2039", "<" );
        // close angle bracket
        s = Regex.Replace( s, "\u203A", ">" );
        // spaces
        s = Regex.Replace( s, "[\u02DC\u00A0]", " " );

        return s;
    }
}

