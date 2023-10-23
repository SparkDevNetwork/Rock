using System.Text;

namespace InteractiveExperienceLoadTest
{
    /// <summary>
    /// Provides helper methods for working with the console.
    /// </summary>
    internal static class ConsoleHelper
    {
        /// <summary>
        /// Prompt the individual for input.
        /// </summary>
        /// <param name="message">The name of the input.</param>
        /// <param name="defaultValue">The default value if a blank string is entered.</param>
        /// <param name="secure"><c>true</c> if this is a secure input, no feedback will be provided.</param>
        /// <param name="required"><c>true</c> if this input is required.</param>
        /// <returns>The string that was entered by the individual or the default value.</returns>
        public static string Prompt( string message, string defaultValue = "", bool secure = false, bool required = false )
        {
            string input;

            do
            {
                if ( string.IsNullOrWhiteSpace( defaultValue ) )
                {
                    Console.Write( $"{message}: " );
                }
                else
                {
                    Console.Write( $"{message} [{defaultValue}]: " );
                }

                if ( secure )
                {
                    input = ReadPassword();
                    Console.WriteLine();
                }
                else
                {
                    input = Console.ReadLine() ?? string.Empty;
                }

                if ( string.IsNullOrEmpty( input ) )
                {
                    input = defaultValue;
                }
            } while ( required && input == string.Empty );

            return input;
        }

        /// <summary>
        /// Reads a secure password from the console. The typed values are not
        /// echoed back to the screen.
        /// </summary>
        /// <returns>The string that was entered.</returns>
        public static string ReadPassword()
        {
            var password = new StringBuilder();

            while ( true )
            {
                var key = Console.ReadKey( true );

                if ( key.Key == ConsoleKey.Enter )
                {
                    return password.ToString();
                }
                else if ( key.Key == ConsoleKey.Backspace )
                {
                    if ( password.Length > 0 )
                    {
                        password.Remove( password.Length - 1, 1 );
                    }
                }
                else
                {
                    password.Append( key.KeyChar );
                }
            }
        }

    }
}