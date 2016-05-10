using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Xml;

namespace Rock.Web.UI
{

    public class RockHiddenFieldPageStatePersister : HiddenFieldPageStatePersister
    {
        // reference: http://www.4guysfromrolla.com/articles/091510-1.aspx

        public RockHiddenFieldPageStatePersister( Page page ) : base( page ) { }

        public override void Save()
        {
            if ( (base.ViewState != null || base.ControlState != null) && ((RockPage)this.Page).EnableViewStateInspection )
            {

                var viewstate = new Pair( base.ViewState, base.ControlState );

                this.ViewStateSize = base.StateFormatter.Serialize( viewstate ).Length;
                
                StringWriter writer = new StringWriter();
                var viewStateParser = new ViewStateParser( writer, base.StateFormatter, this.Page );
                viewStateParser.ParseViewStateGraph( viewstate );
                this.ViewStateValue = writer.ToString();
            }

            base.Save();
        }

        public int ViewStateSize
        {
            get;
            private set;
        }

        public string ViewStateValue
        {
            get;
            private set;
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
        public ViewStateParser( StringWriter writer, IStateFormatter stateFormatter, Page page  )
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

            _xmlWriter = XmlWriter.Create( _writer,  settings);

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
                _xmlWriter.WriteAttributeString( "Size", formatter.Serialize( ((Triplet)node) ).Length.ToString() );

                ParseViewStateGraph( ((Triplet)node).First, depth + 1, "first" );
                ParseViewStateGraph( ((Triplet)node).Second, depth + 1, "second" );
                ParseViewStateGraph( ((Triplet)node).Third, depth + 1, "third" );

                _xmlWriter.WriteEndElement();
            }
            else if ( node is Pair )
            {
                _xmlWriter.WriteStartElement( "Pair" );
                _xmlWriter.WriteAttributeString( "Depth", depth.ToString() );
                _xmlWriter.WriteAttributeString( "Label", label );
                _xmlWriter.WriteAttributeString( "Size", formatter.Serialize( ((Pair)node) ).Length.ToString() );

                ParseViewStateGraph( ((Pair)node).First, depth + 1, "first" );
                ParseViewStateGraph( ((Pair)node).Second, depth + 1, "second" );

                _xmlWriter.WriteEndElement();
            }
            else if ( node is ArrayList )
            {
                _xmlWriter.WriteStartElement( "ArrayList" );
                _xmlWriter.WriteAttributeString( "Depth", depth.ToString() );
                _xmlWriter.WriteAttributeString( "Label", label );
                _xmlWriter.WriteAttributeString( "Size", formatter.Serialize( ((ArrayList)node) ).Length.ToString() );

                // display array values
                for ( int i = 0; i < ((ArrayList)node).Count; i++ )
                {
                    ParseViewStateGraph( ((ArrayList)node)[i], depth + 1, String.Format( "({0})", i ) );
                }

                _xmlWriter.WriteEndElement();
            }
            else if ( node.GetType().IsArray )
            {
                _xmlWriter.WriteStartElement( "Array" );
                _xmlWriter.WriteAttributeString( "Depth", depth.ToString() );
                _xmlWriter.WriteAttributeString( "Label", label );
                _xmlWriter.WriteAttributeString( "Size", formatter.Serialize( ((Array)node) ).Length.ToString() );
                _xmlWriter.WriteAttributeString( "Type", node.GetType().ToString() );

                IEnumerator e = ((Array)node).GetEnumerator();
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
