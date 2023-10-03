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
using Rock.Logging;
using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Web.Security;
using System.Web.UI;
using System.Xml;

namespace Rock.Web.UI
{
    /// <summary>
    /// Optionally compresses the ViewState before storing it into a hidden field on the
    /// page to increase postback speed.
    /// </summary>
    public class RockHiddenFieldPageStatePersister : HiddenFieldPageStatePersister
    {
        // This  custom viewstate controller serves two purposes
        // 1. Compresses the contents of the ViewState when sending to the client
        // 2. Allows you to view the controls in the viewstate 
        //    reference: http://www.4guysfromrolla.com/articles/091510-1.aspx


        /// <summary>
        /// Gets the view state compression threshold.
        /// </summary>
        /// <value>
        /// The view state compression threshold.
        /// </value>
        public static int ViewStateCompressionThreshold { get; private set; } = 102400;

        #region Properties
        /// <summary>
        /// Gets the size of the view state.
        /// </summary>
        /// <value>
        /// The size of the view state.
        /// </value>
        public int ViewStateSize { get; private set; }

        /// <summary>
        /// Gets the view state size compressed.
        /// </summary>
        /// <value>
        /// The view state size compressed.
        /// </value>
        public int ViewStateSizeCompressed { get; private set; }

        /// <summary>
        /// Gets the view state value.
        /// </summary>
        /// <value>
        /// The view state value.
        /// </value>
        public string ViewStateValue { get; private set; }

        /// <summary>
        /// The size of the ViewState at which to compress it.
        /// </summary>
        public int SizeThreshold { get; set; }

        /// <summary>
        /// Gets a value indicating whether [view state is compressed].
        /// </summary>
        /// <value>
        /// <c>true</c> if [view state is compressed]; otherwise, <c>false</c>.
        /// </value>
        public bool ViewStateIsCompressed { get; private set; }

        #endregion

        /// <summary>
        /// Instantiate a new PageStatePersister that will store the ViewState in a hidden field
        /// and optionally compress the data if it is greater than the size threshold.
        /// </summary>
        /// <param name="page">The Page whose ViewState we need to load or save.</param>
        /// <param name="sizeThreshold">If the length of the base64 encoded ViewState is greater than or equal to this number then it will be encrypted. A value of 0 means never encrypt.</param>
        public RockHiddenFieldPageStatePersister( Page page, int sizeThreshold ) : base( page )
        {
            SizeThreshold = sizeThreshold;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockHiddenFieldPageStatePersister"/> class.
        /// </summary>
        /// <param name="page">The <see cref="T:System.Web.UI.Page" /> that the view state persistence mechanism is created for.</param>
        public RockHiddenFieldPageStatePersister( Page page ) : base( page ) {
            SizeThreshold = 102400;
        }

        /// <summary>
        /// Deserializes and loads persisted state information from an <see cref="T:System.Web.HttpRequest" /> object when a <see cref="T:System.Web.UI.Page" /> object initializes its control hierarchy.
        /// </summary>
        public override void Load()
        {
            string viewState = Page.Request.Form["__CVIEWSTATE"];
            if ( viewState == null )
            {
                return;
            }

            byte[] bytes = Convert.FromBase64String( viewState );

            // decrypt viewstate
            try
            {
                bytes = MachineKey.Unprotect( bytes );
            }
            catch ( System.Security.Cryptography.CryptographicException cryptographicException )
            {
                RockLogger.Log.Error( RockLogDomains.Core, cryptographicException, "Failed decrypting the encrypted View state Props" +
                    "" );
                return;
            }

            // uncompress viewstate
            if ( Page.Request.Form["__CVIEWSTATESIZE"] != "0" )
            {
                using ( MemoryStream output = new MemoryStream() )
                {
                    using ( MemoryStream input = new MemoryStream() )
                    {
                        input.Write( bytes, 0, bytes.Length );
                        input.Position = 0;
                        using ( GZipStream gzip = new GZipStream( input, CompressionMode.Decompress, true ) )
                        {
                            gzip.CopyTo( output );
                        }
                    }

                    bytes = output.ToArray();
                }
            }

            // deserialize the data back into ViewState and ControlState.
            Pair pair = ( Pair ) new LosFormatter().Deserialize( Convert.ToBase64String( bytes ) );
            ViewState = pair.First;
            ControlState = pair.Second;
        }

        /// <summary>
        /// Serializes any object state contained in the <see cref="P:System.Web.UI.PageStatePersister.ViewState" /> or <see cref="P:System.Web.UI.PageStatePersister.ControlState" /> property and writes the state to the response stream.
        /// </summary>
        public override void Save()
        {
            // save the viewstate contents so we can read/parse it later
            if ( ( base.ViewState != null || base.ControlState != null ) && ( ( RockPage ) this.Page ).EnableViewStateInspection )
            {
                var viewstate = new Pair( base.ViewState, base.ControlState );

                StringWriter writerViewState = new StringWriter();
                var viewStateParser = new ViewStateParser( writerViewState, base.StateFormatter, this.Page );
                viewStateParser.ParseViewStateGraph( viewstate );
                this.ViewStateValue = writerViewState.ToString();
            }

            // serialize the ViewState and ControlState data for inclusion in the hidden field.
            StringWriter writer = new StringWriter();
            new LosFormatter().Serialize( writer, new Pair( ViewState, ControlState ) );
            string viewStateString = writer.ToString();

            // get the uncompressed size and convert from Base64 to raw data.
            int uncompressedSize = viewStateString.Length;
            byte[] bytes = Convert.FromBase64String( viewStateString );

            this.ViewStateSize = bytes.Length;

            // compress if the size is past the threshhold.
            if ( SizeThreshold != 0 && uncompressedSize >= SizeThreshold )
            {
                ViewStateIsCompressed = true;

                MemoryStream output = new MemoryStream();

                using ( GZipStream gzip = new GZipStream( output, CompressionMode.Compress, true ) )
                {
                    gzip.Write( bytes, 0, bytes.Length );
                }
                bytes = output.ToArray();

                ScriptManager.RegisterHiddenField( Page, "__CVIEWSTATESIZE", uncompressedSize.ToString() );
            }
            else
            {
                ScriptManager.RegisterHiddenField( Page, "__CVIEWSTATESIZE", "0" );
            }

            // encrypt viewstate
            bytes = MachineKey.Protect( bytes );

            ScriptManager.RegisterHiddenField( Page, "__CVIEWSTATE", Convert.ToBase64String( bytes ) );

            ViewStateSizeCompressed = bytes.Length;
        }
    }

    /// <summary>
    /// Helper class for parsing viewstate information
    /// </summary>
    public class ViewStateParser
    {
        // private member variables
        private StringWriter _writer;
        private string indentString = "   ";

        private IStateFormatter formatter;
        private Page _page;
        private XmlWriter _xmlWriter;

        #region Constructor
        /// <summary>
        /// Creates a new ViewStateParser instance, specifying the TextWriter to emit the output to.
        /// </summary>
        public ViewStateParser( StringWriter writer, IStateFormatter stateFormatter, Page page )
        {
            _writer = writer;
            formatter = stateFormatter;
            _page = page;
        }
        #endregion

        #region Methods
        #region ParseViewStateGraph Methods
        /// <summary>
        /// Emits a readable version of the view state to the TextWriter passed into the object's constructor.
        /// </summary>
        /// <param name="viewState">The view state object to start parsing at.</param>
        public virtual void ParseViewStateGraph( object viewState )
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            settings.Encoding = Encoding.UTF8;

            _xmlWriter = XmlWriter.Create( _writer, settings );

            _xmlWriter.WriteStartDocument();
            _xmlWriter.WriteStartElement( "ViewState" );

            ParseViewStateGraph( viewState, 0, string.Empty );

            _xmlWriter.WriteEndElement();
            _xmlWriter.WriteEndDocument();
            _xmlWriter.Close();
        }

        /// <summary>
        /// Emits a readable version of the view state to the TextWriter passed into the object's constructor.
        /// </summary>
        /// <param name="viewStateAsString">A base-64 encoded representation of the view state to parse.</param>
        public virtual void ParseViewStateGraph( string viewStateAsString )
        {
            // First, deserialize the string into a Triplet
            LosFormatter los = new LosFormatter();
            object viewState = los.Deserialize( viewStateAsString );

            ParseViewStateGraph( viewState, 0, string.Empty );
        }

        /// <summary>
        /// Recursively parses the view state.
        /// </summary>
        /// <param name="node">The current view state node.</param>
        /// <param name="depth">The "depth" of the view state tree.</param>
        /// <param name="label">A label to display in the emitted output next to the current node.</param>
        protected virtual void ParseViewStateGraph( object node, int depth, string label )
        {

            if ( node == null )
            {
                _xmlWriter.WriteStartElement( "Empty" );
                _xmlWriter.WriteAttributeString( "Depth", depth.ToString() );
                _xmlWriter.WriteAttributeString( "Label", label );
                _xmlWriter.WriteEndElement();
            }
            else if ( node is Triplet )
            {
                _xmlWriter.WriteStartElement( "Triplet" );
                _xmlWriter.WriteAttributeString( "Depth", depth.ToString() );
                _xmlWriter.WriteAttributeString( "Label", label );
                _xmlWriter.WriteAttributeString( "Size", formatter.Serialize( ( ( Triplet ) node ) ).Length.ToString() );

                ParseViewStateGraph( ( ( Triplet ) node ).First, depth + 1, "first" );
                ParseViewStateGraph( ( ( Triplet ) node ).Second, depth + 1, "second" );
                ParseViewStateGraph( ( ( Triplet ) node ).Third, depth + 1, "third" );

                _xmlWriter.WriteEndElement();
            }
            else if ( node is Pair )
            {
                _xmlWriter.WriteStartElement( "Pair" );
                _xmlWriter.WriteAttributeString( "Depth", depth.ToString() );
                _xmlWriter.WriteAttributeString( "Label", label );
                _xmlWriter.WriteAttributeString( "Size", formatter.Serialize( ( ( Pair ) node ) ).Length.ToString() );

                ParseViewStateGraph( ( ( Pair ) node ).First, depth + 1, "first" );
                ParseViewStateGraph( ( ( Pair ) node ).Second, depth + 1, "second" );

                _xmlWriter.WriteEndElement();
            }
            else if ( node is ArrayList )
            {
                _xmlWriter.WriteStartElement( "ArrayList" );
                _xmlWriter.WriteAttributeString( "Depth", depth.ToString() );
                _xmlWriter.WriteAttributeString( "Label", label );
                _xmlWriter.WriteAttributeString( "Size", formatter.Serialize( ( ( ArrayList ) node ) ).Length.ToString() );

                // display array values
                for ( int i = 0; i < ( ( ArrayList ) node ).Count; i++ )
                {
                    ParseViewStateGraph( ( ( ArrayList ) node )[i], depth + 1, String.Format( "({0})", i ) );
                }

                _xmlWriter.WriteEndElement();
            }
            else if ( node.GetType().IsArray )
            {
                _xmlWriter.WriteStartElement( "Array" );
                _xmlWriter.WriteAttributeString( "Depth", depth.ToString() );
                _xmlWriter.WriteAttributeString( "Label", label );
                _xmlWriter.WriteAttributeString( "Size", formatter.Serialize( ( ( Array ) node ) ).Length.ToString() );
                _xmlWriter.WriteAttributeString( "Type", node.GetType().ToString() );

                IEnumerator e = ( ( Array ) node ).GetEnumerator();
                int count = 0;
                while ( e.MoveNext() )
                {
                    ParseViewStateGraph( e.Current, depth + 1, String.Format( "({0})", count++ ) );
                }

                _xmlWriter.WriteEndElement();
            }
            else if ( node.GetType().IsPrimitive || node is string )
            {
                _xmlWriter.WriteStartElement( "Item" );
                _xmlWriter.WriteAttributeString( "Depth", depth.ToString() );
                _xmlWriter.WriteAttributeString( "Label", label );
                _xmlWriter.WriteAttributeString( "Size", formatter.Serialize( node ).Length.ToString() );
                _xmlWriter.WriteAttributeString( "Type", node.GetType().ToString() );

                _xmlWriter.WriteString( node.ToString() );

                _xmlWriter.WriteEndElement();
            }
            else
            {
                _xmlWriter.WriteStartElement( "Other" );
                _xmlWriter.WriteAttributeString( "Depth", depth.ToString() );
                _xmlWriter.WriteAttributeString( "Label", label );
                _xmlWriter.WriteAttributeString( "Size", formatter.Serialize( node ).Length.ToString() );
                _xmlWriter.WriteAttributeString( "Type", node.GetType().ToString() );

                _xmlWriter.WriteString( node.ToJson() );

                _xmlWriter.WriteEndElement();
            }
        }
        #endregion

        /// <summary>
        /// Returns a string containing the <see cref="IndentString"/> property value a specified number of times.
        /// </summary>
        /// <param name="depth">The number of times to repeat the <see cref="IndentString"/> property.</param>
        /// <returns>A string containing the <see cref="IndentString"/> property value a specified number of times.</returns>
        protected virtual string Indent( int depth )
        {
            StringBuilder sb = new StringBuilder( IndentString.Length * depth );
            for ( int i = 0; i < depth; i++ )
                sb.Append( IndentString );

            return sb.ToString();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Specifies the indentation to use for each level when displaying the object graph.
        /// </summary>
        /// <value>A string value; the default is three blank spaces.</value>
        public string IndentString
        {
            get
            {
                return indentString;
            }
            set
            {
                indentString = value;
            }
        }
        #endregion
    }
}
