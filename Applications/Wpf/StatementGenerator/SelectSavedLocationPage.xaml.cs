﻿// <copyright>
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
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// Interaction logic for SelectSavedLocationPage.xaml
    /// </summary>
    public partial class SelectSaveLocationPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectSaveLocationPage"/> class.
        /// </summary>
        public SelectSaveLocationPage()
        {
            InitializeComponent();

            txtFolderLocation.Text = Environment.GetFolderPath( Environment.SpecialFolder.Desktop );
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSelectFolder_Click( object sender, RoutedEventArgs e )
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if ( result == System.Windows.Forms.DialogResult.OK )
            {
                txtFolderLocation.Text = dialog.SelectedPath;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnNext_Click( object sender, RoutedEventArgs e )
        {
            if (txtFolderLocation.Text.Trim() == string.Empty )
            {
                MessageBoxResult result = MessageBox.Show( "Please select a folder to save contribution statements to.", "Folder Location Required", MessageBoxButton.OK, MessageBoxImage.Warning );
                return;
            }

            if ( !Directory.Exists( txtFolderLocation.Text ) )
            {
                try {
                    System.IO.Directory.CreateDirectory( txtFolderLocation.Text );
                }
                catch(Exception ex )
                {
                    MessageBoxResult result = MessageBox.Show( "Could create the directory provided. Please double-check that it is a valid path.", "Path Not Valid", MessageBoxButton.OK, MessageBoxImage.Exclamation );
                    return;
                }
            }

            if ( txtFileName.Text == string.Empty )
            {
                MessageBoxResult result = MessageBox.Show( "Please provide a base file name.", "Filename Required", MessageBoxButton.OK, MessageBoxImage.Warning );
                return;
            }

            ReportOptions.Current.BaseFileName = txtFileName.Text;
            ReportOptions.Current.SaveDirectory = txtFolderLocation.Text;

            int chapterSize = 0;

            if(txtChapterSize.Text.Trim() != string.Empty && !int.TryParse(txtChapterSize.Text.Trim(), out chapterSize ) )
            {
                MessageBoxResult result = MessageBox.Show( "Please provide a number for the chapter size or leave blank.", "Invalid Chapter Size", MessageBoxButton.OK, MessageBoxImage.Warning );
                return;
            }

            if (chapterSize > 0 )
            {
                ReportOptions.Current.ChapterSize = chapterSize;
            }

            ProgressPage nextPage = new ProgressPage();
            this.NavigationService.Navigate( nextPage );
        }

        /// <summary>
        /// Handles the Click event of the btnPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnPrev_Click( object sender, RoutedEventArgs e )
        {
            this.NavigationService.GoBack();
        }
    }
}
