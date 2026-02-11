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

using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

using WinForms = System.Windows.Forms;

using Microsoft.Data.SqlClient;

namespace RockDbBackupManager
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Connection string sourced from RockWeb/web.ConnectionStrings.config
        /// </summary>
        private readonly SqlConnectionStringBuilder connectionStringBuilder;
        /// <summary>
        /// Regex used to remove non-alphanumeric characters from tags.
        /// </summary>
        private static readonly Regex AlphanumericRegex = CompiledAlphanumericRegex();

        /// <summary>
        /// Generates a compiled regex that matches non-alphanumeric characters.
        /// </summary>
        /// <returns>A compiled Regex instance.</returns>
        [GeneratedRegex( "[^a-zA-Z0-9]", RegexOptions.Compiled )]
        private static partial Regex CompiledAlphanumericRegex();

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// Loads connection string, backup directory, and database names.
        /// </summary>
        public MainWindow()
        {
            // Load connection string from RockWeb/web.ConnectionStrings.config
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "RockWeb", "web.ConnectionStrings.config");
            if (File.Exists(configPath))
            {
                var xml = System.Xml.Linq.XDocument.Load(configPath);
                var connElem = xml.Descendants("add").FirstOrDefault(e => (string?)e.Attribute("name") == "RockContext");
                if (connElem != null && connElem.Attribute("connectionString") != null)
                {
                    var connectionElementRealized = ((string?)connElem.Attribute( "connectionString" ))?.Replace( "\n", "" ).Trim();
                    connectionStringBuilder = new SqlConnectionStringBuilder(connectionElementRealized);
                }
                else
                {
                    connectionStringBuilder = [];
                }
            }
            else
            {
                connectionStringBuilder = [];
            }

            connectionStringBuilder.InitialCatalog = "master";
            connectionStringBuilder.TrustServerCertificate = true;

            InitializeComponent();
            // Load backup directory from settings or use default
            string savedDir = BackupManager.Default.BackupDirectory;
            if (!string.IsNullOrWhiteSpace(savedDir) && System.IO.Directory.Exists(savedDir))
            {
                BackupDirectoryTextBox.Text = savedDir;
            }
            else
            {
                BackupDirectoryTextBox.Text = "C:\\Program Files\\Microsoft SQL Server\\MSSQL16.MSSQLSERVER\\MSSQL\\Backup\\";
            }
            LoadDatabaseNames();
            var dbName = DatabaseNameComboBox.SelectedItem as string;
            LoadBackupFiles(dbName ?? string.Empty);
        }

        /// <summary>
        /// Loads the list of database names from the SQL Server and populates the combo box.
        /// </summary>
        private void LoadDatabaseNames()
        {
            if (string.IsNullOrWhiteSpace(connectionStringBuilder.ConnectionString))
            {
                StatusTextBlock.Text = "Error: Database connection string not found. Cannot load databases.";
                DatabaseNameComboBox.ItemsSource = null;
                return;
            }
            try
            {
                var databases = new List<string>();
                using var connection = new SqlConnection(connectionStringBuilder.ConnectionString);
                connection.Open();
                using var command = new SqlCommand("SELECT name FROM sys.databases WHERE database_id > 4", connection);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    databases.Add(reader.GetString(0));
                }
                DatabaseNameComboBox.ItemsSource = new List<string>(databases);
                if (databases.Count > 0)
                    DatabaseNameComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Error loading databases: {ex.Message}";
            }
        }

        /// <summary>
        /// Loads backup files from the backup directory, optionally filtering by database name.
        /// </summary>
        /// <param name="filterDbName">Optional database name to filter backup files.</param>
        private void LoadBackupFiles(string? filterDbName = null)
        {
            var backupDir = BackupDirectoryTextBox?.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(backupDir) && System.IO.Directory.Exists(backupDir))
            {
                var files = Directory.GetFiles(backupDir, "*.bak");
                var fileNames = files.Select(f => Path.GetFileName(f)).ToList();
                if (!string.IsNullOrWhiteSpace(filterDbName))
                {
                    fileNames = [.. fileNames.Where(f => f != null && f.Contains(filterDbName, StringComparison.OrdinalIgnoreCase))];
                }
                BackupFilesListBox.ItemsSource = fileNames;
            }
            else
            {
                BackupFilesListBox.ItemsSource = null;
            }
        }
        
        /// <summary>
        /// Sets the status message in the UI with optional error and loading indicators.
        /// </summary>
        /// <param name="message">The status message to display.</param>
        /// <param name="isError">Whether the message is an error.</param>
        /// <param name="isLoading">Whether the message indicates a loading state.</param>
        private void SetStatus(string message, bool isError = false, bool isLoading = false)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            StatusTextBlock.Text = $"[{timestamp}] {message}{(isLoading ? " ..." : "")}";
            StatusTextBlock.Foreground = isError ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.Black;
        }

        /// <summary>
        /// Enables or disables UI controls based on the provided flag.
        /// </summary>
        /// <param name="enabled">True to enable controls, false to disable.</param>
        private void SetUiEnabled(bool enabled)
        {
            DatabaseNameComboBox.IsEnabled = enabled;
            BackupDirectoryTextBox.IsEnabled = enabled;
            BrowseBackupDirectoryButton.IsEnabled = enabled;
            AdditionalTagsTextBox.IsEnabled = enabled;
            BackupButton.IsEnabled = enabled;
            RestoreButton.IsEnabled = enabled;
            BrowseRestoreFileButton.IsEnabled = enabled;
            BackupFilesListBox.IsEnabled = enabled;
        }
        /// <summary>
        /// Handles the selection change event for the database name combo box.
        /// Loads backup files for the selected database.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">Selection changed event arguments.</param>
        private void DatabaseNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dbName = DatabaseNameComboBox.SelectedItem as string;
            LoadBackupFiles(dbName ?? string.Empty);
        }

        /// <summary>
        /// Handles the selection change event for the backup files list box.
        /// Sets the restore file text box to the selected file.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">Selection changed event arguments.</param>
        private void BackupFilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BackupFilesListBox.SelectedItem is string selectedFile && RestoreFileTextBox != null)
            {
                RestoreFileTextBox.Text = selectedFile;
            }
        }

        /// <summary>
        /// Handles the click event for the browse restore file button.
        /// Opens a file dialog to select a backup file for restore.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">Routed event arguments.</param>
        private void BrowseRestoreFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Backup Files (*.bak)|*.bak|All Files (*.*)|*.*",
                Title = "Select backup file to prepare for restore",
                Multiselect = false
            };
            // If RestoreFileTextBox is empty, use BackupDirectoryTextBox as initial directory
            var restoreFilePath = RestoreFileTextBox?.Text?.Trim();
            string? initialDir = null;
            if (!string.IsNullOrWhiteSpace(restoreFilePath) && System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(restoreFilePath)))
            {
                initialDir = Path.GetDirectoryName(restoreFilePath);
            }
            else if (!string.IsNullOrWhiteSpace(BackupDirectoryTextBox.Text) && System.IO.Directory.Exists(BackupDirectoryTextBox.Text.Trim()))
            {
                initialDir = BackupDirectoryTextBox.Text.Trim();
            }
            if (!string.IsNullOrWhiteSpace(initialDir))
            {
                openFileDialog.InitialDirectory = initialDir;
            }
            if (openFileDialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                RestoreFileTextBox?.Text = openFileDialog.FileName;
            }
        }

        /// <summary>
        /// Handles the click event for the backup button.
        /// Initiates a database backup operation.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">Routed event arguments.</param>
        private async void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            string? dbName = DatabaseNameComboBox.SelectedItem as string;
            string? backupDir = BackupDirectoryTextBox?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(dbName) || string.IsNullOrWhiteSpace(backupDir))
            {
                SetStatus("Database name and backup directory are required.", isError: true);
                return;
            }

            string timestamp = DateTime.Now.ToString("yyyy_MM_dd_HHmmss");
            string tags = AdditionalTagsTextBox?.Text ?? string.Empty;
            string tagPart = string.Empty;
            if (!string.IsNullOrWhiteSpace(tags))
            {
                var tagList = tags.Split(',')
                                  .Select(t => t.Trim())
                                  .Where(t => !string.IsNullOrWhiteSpace(t))
                                  .Select(t => AlphanumericRegex.Replace(t, string.Empty))
                                  .Where(t => !string.IsNullOrWhiteSpace(t))
                                  .ToList();
                if (tagList.Count > 0)
                {
                    tagPart = "-" + string.Join("_", tagList);
                }
            }

            string backupFileName = $"{timestamp}-{dbName}{tagPart}.bak";
            string backupPath = System.IO.Path.Combine(backupDir, backupFileName);

            string sql = $@"BACKUP DATABASE [{dbName}] TO DISK = N'{backupPath}' WITH COPY_ONLY, NOFORMAT, INIT, NAME = N'{backupFileName}', SKIP, NOREWIND, NOUNLOAD, STATS = 20, CHECKSUM;";

            SetStatus("Backup started", isError: false, isLoading: true);
            SetUiEnabled(false);
            try
            {
                await Task.Run(async () =>
                {
                    using var connection = new SqlConnection(connectionStringBuilder.ConnectionString);
                    await connection.OpenAsync();
                    using var command = new SqlCommand(sql, connection);
                    command.CommandTimeout = 0;
                    await command.ExecuteNonQueryAsync();
                });
                Dispatcher.Invoke(() =>
                {
                    SetStatus($"Backup completed: {backupPath}");
                    LoadBackupFiles(dbName); // Refresh restore list after backup
                    RestoreFileTextBox.Text = string.Empty;
                });
            }
            catch (SqlException ex)
            {
                Dispatcher.Invoke(() => SetStatus($"SQL error during backup: {ex.Message}", isError: true));
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => SetStatus($"Backup failed: {ex.Message}", isError: true));
            }
            finally
            {
                Dispatcher.Invoke(() => SetUiEnabled(true));
            }
        }

        /// <summary>
        /// Handles the click event for the restore button.
        /// Initiates a database restore operation.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">Routed event arguments.</param>
        private async void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            string? dbName = DatabaseNameComboBox.SelectedItem as string;
            string? restoreFile = RestoreFileTextBox?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(dbName) || string.IsNullOrWhiteSpace(restoreFile))
            {
                SetStatus("Database name and restore file are required.", isError: true);
                return;
            }

            // Show warning if enabled
            if (BackupManager.Default.IsWarningOnRestore)
            {
                var result = System.Windows.MessageBox.Show(
                    "Restoring will completely overwrite the current contents of the selected database. Backups created or restored with this application are full, copy-only backups.\n\nHit cancel to stop the restore, but also never display this message again (not recommended, but you are the captain of your own ship.)\n\nDo you want to continue?",
                    "Restore Warning",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No);

                // Custom handling for 'Don't Warn Me Again'
                // Since MessageBox doesn't support custom buttons, treat Cancel as 'Don't Warn Me Again'
                if (result == MessageBoxResult.Cancel)
                {
                    BackupManager.Default.IsWarningOnRestore = false;
                    BackupManager.Default.Save();
                    // Ask again for confirmation
                    var confirm = System.Windows.MessageBox.Show(
                        "Do you want to continue with the restore?",
                        "Confirm Restore",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question,
                        MessageBoxResult.No);
                    if (confirm != MessageBoxResult.Yes)
                        return;
                }
                else if (result != MessageBoxResult.Yes)
                {
                    // User chose No or closed dialog
                    return;
                }
            }

            string sql = $@"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; RESTORE DATABASE [{dbName}] FROM DISK = N'{restoreFile}' WITH REPLACE; ALTER DATABASE [{dbName}] SET MULTI_USER;";

            SetStatus("Restore started", isError: false, isLoading: true);
            SetUiEnabled(false);
            try
            {
                await Task.Run(async () =>
                {
                    using var connection = new SqlConnection(connectionStringBuilder.ConnectionString);
                    await connection.OpenAsync();
                    using var command = new SqlCommand(sql, connection);
                    command.CommandTimeout = 0;
                    await command.ExecuteNonQueryAsync();
                });
                Dispatcher.Invoke(() => SetStatus($"Restore completed: {restoreFile}"));
            }
            catch (SqlException ex)
            {
                Dispatcher.Invoke(() => SetStatus($"SQL error during restore: {ex.Message}", isError: true));
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => SetStatus($"Restore failed: {ex.Message}", isError: true));
            }
            finally
            {
                Dispatcher.Invoke(() => SetUiEnabled(true));
            }
        }
        /// <summary>
        /// Handles the click event for the browse backup directory button.
        /// Opens a folder browser dialog to select the backup directory.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">Routed event arguments.</param>
        private void BrowseBackupDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = true
            };

            var initialDir = BackupDirectoryTextBox?.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(initialDir) && System.IO.Directory.Exists(initialDir))
            {
                dialog.InitialDirectory = initialDir;
            }
            if (dialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                var folder = dialog.SelectedPath;

                BackupDirectoryTextBox?.Text = folder;

                // Save to settings
                BackupManager.Default.BackupDirectory = folder;
                BackupManager.Default.Save();

                var dbName = DatabaseNameComboBox.SelectedItem as string;
                LoadBackupFiles(dbName ?? string.Empty); // Refresh backup files list filtered by selected DB
            }
        }

        /// <summary>
        /// Handles the click event for the delete backup button.
        /// Deletes the selected backup file after confirmation.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">Routed event arguments.</param>
        private void DeleteBackupButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string fileName)
            {
                var backupDir = BackupDirectoryTextBox.Text.Trim();
                var filePath = System.IO.Path.Combine(backupDir, fileName);
                if (!System.IO.File.Exists(filePath))
                {
                    System.Windows.MessageBox.Show($"File not found: {filePath}", "Delete Backup", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var result = System.Windows.MessageBox.Show($"Are you sure you want to permanently delete '{fileName}'? This action cannot be undone.", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                        var dbName = DatabaseNameComboBox.SelectedItem as string;
                        LoadBackupFiles(dbName ?? string.Empty);
                        RestoreFileTextBox.Text = string.Empty;
                        SetStatus($"Deleted backup: {fileName}");
                    }
                    catch (System.Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Failed to delete file: {ex.Message}", "Delete Backup", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the click event for the restore warning menu item.
        /// Resets the restore warning setting.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">Routed event arguments.</param>
        private void RestoreWarningMenuItem_Click(object sender, RoutedEventArgs e)
        {
            BackupManager.Default.IsWarningOnRestore = true;
            BackupManager.Default.Save();
            System.Windows.MessageBox.Show(
                "Restore warning has been reset and will be shown before future restores.",
                "Restore Warning Reset",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// <summary>
        /// Handles the click event for the exit menu item.
        /// Shuts down the application.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">Routed event arguments.</param>
        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>
        /// Handles the click event for the about help menu item.
        /// Displays information about the application.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">Routed event arguments.</param>
        private void AboutMenuHelpItem_Click(object sender, RoutedEventArgs e)
        {
            string info = "RockDB Backup Manager\n\n" +
                "Copyright © Spark Development Network\n" +
                "Licensed under the Rock Community License\n" +
                "https://www.rockrms.com/license\n\n" +
                "This core developer tool allows you to create and restore full, copy-only SQL Server backups for Rock RMS databases.";

            System.Windows.MessageBox.Show(info, "About RockDB Backup Manager", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Handles the click event for the about usage menu item.
        /// Displays usage information about the application.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">Routed event arguments.</param>
        private void AboutMenuUsageItem_Click(object sender, RoutedEventArgs e)
        {
            string info = "This tool creates and restores full, copy-only SQL Server backups for Rock RMS databases.\n\n" +
            "Connection settings are automatically sourced from the RockWeb/web.ConnectionStrings.config file. " +
            "To ensure correct database connectivity, run this application in Debug mode from its default build directory. " +
            "This allows the tool to locate the RockWeb folder and its configuration files as expected.\n\n" +
            "Backups are saved to the directory specified, which defaults to the SQL Server backup folder. " +
            "Restores will overwrite the selected database completely. Use caution and verify your backup files before restoring.\n\n" +
            "For best results, ensure both you and NT SERVICE\\MSSQLSERVER have appropriate permissions to access the SQL Server instance and file system locations.";

            System.Windows.MessageBox.Show( info, "About RockDB Backup Manager", MessageBoxButton.OK, MessageBoxImage.Information );
        }
    }
}