using System;
using System.Windows.Data;

namespace Rock.CodeGeneration.Utility
{
    /// <summary>
    /// A value converter that inverts a boolean value.
    /// False => True and True => False.
    /// </summary>
    [ValueConversion( typeof( bool ), typeof( bool ) )]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns <see langword="null" />, the valid null value is used.</returns>
        /// <exception cref="System.InvalidOperationException">The target must be a boolean</exception>
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            return targetType == typeof( bool )
                ? !( bool ) value
                : throw new InvalidOperationException( "The target must be a boolean" );
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns <see langword="null" />, the valid null value is used.</returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
