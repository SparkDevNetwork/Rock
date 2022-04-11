using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using LoxSmoke.DocXml;

using Rock;

namespace BlockGenerator.Utility
{
    public class MultiDocReader
    {
        private readonly List<DocXmlReader> _xmlDocReaders = new List<DocXmlReader>();

        public MultiDocReader()
        {
            // Read the XML documentation for the Rock.ViewModels assembly.
            var assemblyFileName = new FileInfo( new Uri( typeof( Rock.ViewModels.Utility.IViewModel ).Assembly.CodeBase ).LocalPath ).FullName;

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
            assemblyFileName = new FileInfo( new Uri( typeof( Rock.Enums.Reporting.FieldFilterSourceType ).Assembly.CodeBase ).LocalPath ).FullName;

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

        public TypeComments GetTypeComments( Type type )
        {
            return GetCommentsFromReaders( reader => reader.GetTypeComments( type ) );
        }

        public string GetMemberComment( MemberInfo memberInfo )
        {
            return GetTextFromReaders( reader => reader.GetMemberComment( memberInfo ) );
        }
    }
}
