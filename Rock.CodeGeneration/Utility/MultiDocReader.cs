using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using LoxSmoke.DocXml;

namespace Rock.CodeGeneration.Utility
{
    /// <summary>
    /// Reads from multiple XML documentation documents.
    /// </summary>
    public class MultiDocReader
    {
        #region Fields

        /// <summary>
        /// The XML document readers to read from.
        /// </summary>
        private readonly List<DocXmlReader> _xmlDocReaders = new List<DocXmlReader>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiDocReader"/> class.
        /// </summary>
        public MultiDocReader()
        {
            // Read the XML documentation for the Rock assembly.
            var assemblyFileName = new FileInfo( new Uri( typeof( Data.IEntity ).Assembly.CodeBase ).LocalPath ).FullName;

            try
            {
                var docXmlReader = new DocXmlReader( assemblyFileName.Substring( 0, assemblyFileName.Length - 4 ) + ".xml" );
                _xmlDocReaders.Add( docXmlReader );
            }
            catch
            {
                /* Intentionally left blank. */
            }

            // Read the XML documentation for the Rock.ViewModels assembly.
            assemblyFileName = new FileInfo( new Uri( typeof( Rock.ViewModels.Utility.IViewModel ).Assembly.CodeBase ).LocalPath ).FullName;

            try
            {
                var docXmlReader = new DocXmlReader( assemblyFileName.Substring( 0, assemblyFileName.Length - 4 ) + ".xml" );
                _xmlDocReaders.Add( docXmlReader );
            }
            catch
            {
                /* Intentionally left blank. */
            }

            // Read the XML documentation for the Rock.Enums assembly.
            assemblyFileName = new FileInfo( new Uri( typeof( Enums.Reporting.FieldFilterSourceType ).Assembly.CodeBase ).LocalPath ).FullName;

            try
            {
                var docXmlReader = new DocXmlReader( assemblyFileName.Substring( 0, assemblyFileName.Length - 4 ) + ".xml" );
                _xmlDocReaders.Add( docXmlReader );
            }
            catch
            {
                /* Intentionally left blank. */
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the comments from the first reader that finds it.
        /// </summary>
        /// <typeparam name="TResult">The type of the result expected.</typeparam>
        /// <param name="selector">The selector to get the data from the reader.</param>
        /// <returns>A <typeparamref name="TResult"/> that represents the comment; or the default value if it wasn't found.</returns>
        private TResult GetCommentsFromReaders<TResult>( Func<DocXmlReader, TResult> selector )
            where TResult : CommonComments
        {
            foreach ( var reader in _xmlDocReaders )
            {
                var result = selector( reader );

                if ( result != null && result.Summary.IsNotNullOrWhiteSpace() )
                {
                    return result;
                }
            }

            return default;
        }

        /// <summary>
        /// Gets the plain text string from the first reader that finds it.
        /// </summary>
        /// <param name="selector">The selector to get the data from the reader.</param>
        /// <returns>A string that contains the comment; or <c>null</c> if it wasn't found.</returns>
        private string GetTextFromReaders( Func<DocXmlReader, string> selector )
        {
            foreach ( var reader in _xmlDocReaders )
            {
                var result = selector( reader );

                if ( result.IsNotNullOrWhiteSpace() )
                {
                    return result;
                }
            }

            return default;
        }

        /// <summary>
        /// Gets the comment associated with the type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A <see cref="TypeComments"/> instance that represents the XML documentation; or <c>null</c> if it wasn't found.</returns>
        public TypeComments GetTypeComments( Type type )
        {
            return GetCommentsFromReaders( reader => reader.GetTypeComments( type ) );
        }

        /// <summary>
        /// Gets the comment associated with the member.
        /// </summary>
        /// <param name="memberInfo">The member information.</param>
        /// <returns>A string that represents the text from the XML documentation; or <c>null</c> if it wasn't found.</returns>
        public string GetMemberComment( MemberInfo memberInfo )
        {
            return GetTextFromReaders( reader => reader.GetMemberComment( memberInfo ) );
        }

        #endregion
    }
}
