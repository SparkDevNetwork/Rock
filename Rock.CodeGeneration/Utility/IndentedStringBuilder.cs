using System;
using System.Text;

namespace Rock.CodeGeneration.Utility
{
    /// <summary>
    /// Special use builder for strings when they need to have indentation
    /// awareness. Used when generating source code.
    /// </summary>
    internal class IndentedStringBuilder
    {
        #region Fields

        /// <summary>
        /// The underlying builder that performs the real work.
        /// </summary>
        private readonly StringBuilder _builder = new StringBuilder();

        /// <summary>
        /// The number of spaces for each indentation level.
        /// </summary>
        private readonly int _indentSize;

        /// <summary>
        /// The current indentation level.
        /// </summary>
        private int _indentLevel = 0;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="IndentedStringBuilder"/>.
        /// </summary>
        /// <param name="initialIndentLevel">The indentation level to start with.</param>
        /// <param name="indentSize">The number of spaces for each indentation level.</param>
        public IndentedStringBuilder( int initialIndentLevel = 0, int indentSize = 4 )
        {
            _indentLevel = initialIndentLevel;
            _indentSize = indentSize;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Increase the indentation by one level.
        /// </summary>
        public void Indent()
        {
            _indentLevel++;
        }

        /// <summary>
        /// Increase the indentation by one level, call the action, and then
        /// decrease indentation.
        /// </summary>
        /// <param name="action">The action to call while indented.</param>
        public void Indent( Action action )
        {
            _indentLevel++;
            action();
            _indentLevel--;
        }

        /// <summary>
        /// Decreased the indentation by one level.
        /// </summary>
        public void Unindent()
        {
            _indentLevel--;
        }

        /// <summary>
        /// Appends a blank line without any indentation.
        /// </summary>
        public void AppendLine()
        {
            _builder.AppendLine();
        }

        /// <summary>
        /// Appends the text as a line of text with the proper indentation
        /// applied to the start of the line.
        /// </summary>
        /// <param name="value">The text to be written.</param>
        public void AppendLine( string value )
        {
            _builder.AppendLine( new string( ' ', _indentLevel * _indentSize ) + value );
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _builder.ToString();
        }

        #endregion
    }
}
