using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rock.Tests.Shared
{
    public static partial class AssertExtensions
    {
        public static void FileContains( this Assert assert, string filePath, string expectedString )
        {
            if ( string.IsNullOrWhiteSpace( filePath ) )
            {
                throw new ArgumentException( "No file path provided.", "filePath" );
            }

            if ( string.IsNullOrWhiteSpace( expectedString ) )
            {
                throw new ArgumentException( "No expected string provided.", "expectedString" );
            }

            var file = System.IO.File.ReadAllText( filePath );
            var fileContainsExpectedString = file.Contains( expectedString );

            if ( !fileContainsExpectedString )
            {
                throw new AssertFailedException( $"File {filePath} did not contain '{expectedString}'." );
            }

        }

        public static void FileDoesNotContains( this Assert assert, string filePath, string excludedString )
        {
            if ( string.IsNullOrWhiteSpace( filePath ) )
            {
                throw new ArgumentException( "No file path provided.", "filePath" );
            }

            if ( string.IsNullOrWhiteSpace( excludedString ) )
            {
                throw new ArgumentException( "No exclude string provided.", "excludedString" );
            }

            var file = System.IO.File.ReadAllText( filePath );
            var fileContainsExpectedString = file.Contains( excludedString );
            if ( fileContainsExpectedString )
            {
                throw new AssertFailedException( $"File {filePath} contained '{excludedString}'." );
            }
        }

        public static void FolderHasCorrectNumberOfFiles( this Assert assert, string folderPath, int fileCount )
        {
            if ( string.IsNullOrWhiteSpace( folderPath ) )
            {
                throw new ArgumentException( "No folder path provided.", "folderPath" );
            }

            var folder = System.IO.Directory.GetFiles( folderPath );
            var folderHasCorrectNumberOfFiles = folder.Length == fileCount;
            if ( !folderHasCorrectNumberOfFiles )
            {
                throw new AssertFailedException( $"The folder {folderPath} contain {folder.Length} files but expected {fileCount} files." );
            }
        }

        public static void FileNotFound( this Assert assert, string filePath )
        {
            if ( string.IsNullOrWhiteSpace( filePath ) )
            {
                throw new ArgumentException( "No file path provided.", "filePath" );
            }

            if ( System.IO.File.Exists( filePath ) )
            {
                throw new AssertFailedException( $"File {filePath} was found." );
            }

        }

        public static void FolderFileSizeIsWithinRange( this Assert assert, string folderPath, long minFileSize, long maxFileSize, double allowedVariation )
        {
            if ( string.IsNullOrWhiteSpace( folderPath ) )
            {
                throw new ArgumentException( "No folder path provided.", "folderPath" );
            }

            var folder = System.IO.Directory.GetFiles( folderPath );
            var allowedMinFileSize = minFileSize - ( minFileSize * allowedVariation );
            var allowedMaxFileSize = maxFileSize + ( maxFileSize * allowedVariation );

            foreach ( var filePath in folder )
            {
                var file = System.IO.File.OpenRead( filePath );
                var fileSize = file.Length;
                if ( fileSize > allowedMaxFileSize || fileSize < allowedMinFileSize )
                {
                    throw new AssertFailedException( $"The {filePath} file size is {fileSize}, but was expected to be between {minFileSize} and {maxFileSize}." );
                }
                file.Close();
            }
        }
    }
}
