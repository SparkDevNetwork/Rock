// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Windows;

namespace Rock.Wpf
{
    /// <summary>
    /// Interaction logic for ErrorMessageWindow.xaml
    /// </summary>
    public partial class ErrorMessageWindow : Window
    {
        public ErrorMessageWindow( Exception ex )
        {
            if (ex is AggregateException)
            {
                ex = ( ex as AggregateException ).Flatten();
            }


            System.Diagnostics.Debug.Write( ex.StackTrace );
            
            string message = ex.Message;
            var innerEx = ex.InnerException;
            while (innerEx != null)
            {
                message += "\n" + innerEx.Message;
                innerEx = innerEx.InnerException;
            }

            InitializeComponent();
            lblErrorMessage.Content = message;
        }

        /// <summary>
        /// Handles the Click event of the OK control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OK_Click( object sender, RoutedEventArgs e )
        {
            this.Close();
        }
    }
}
