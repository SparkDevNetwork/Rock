using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using com.blueboxmoon.DatabaseThinner;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_blueboxmoon.DatabaseThinner
{
    [DisplayName( "Automation Job Configuration" )]
    [Category( "Blue Box Moon > Database Thinner" )]
    [Description( "Configuration the settings for the Database Thinner Automation job." )]
    public partial class AutomationJobConfiguration : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                ShowDetails();
                UpdateInterfaceState();
            }
        }

        #region Methods

        /// <summary>
        /// Shows the details.
        /// </summary>
        protected void ShowDetails()
        {
            var configuration = Rock.Web.SystemSettings.GetValue( SystemSettings.CONFIGURATION ).FromJsonOrNull<Configuration>() ?? new Configuration();

            cbCompressCommunicationsEnabled.Checked = configuration.CompressCommunications.IsEnabled;
            nbCompressCommunicationsOlderThan.Text = configuration.CompressCommunications.CompressContentOlderThan.ToString();
            nbCompressCommunicationsBatchSize.Text = configuration.CompressCommunications.BatchSize.ToString();
            cbCompressCommunicationsShouldDecompress.Checked = configuration.CompressCommunications.ShouldDecompress;

            cbDeleteSystemCommunicationsEnabled.Checked = configuration.DeleteSystemCommunications.IsEnabled;
            nbDeleteSystemCommunicationsOlderThan.Text = configuration.DeleteSystemCommunications.OlderThan.ToString();
            nbDeleteSystemCommunicationsMaximumDaysBack.Text = configuration.DeleteSystemCommunications.MaximumDaysBack.ToString();
            vlDeleteSystemCommunicationsSubjects.Value = string.Join( "|", configuration.DeleteSystemCommunications.Subjects );

            cbTransactionImagesEnabled.Checked = configuration.TransactionImages.IsEnabled;
            nbTransactionImagesOlderThan.Text = configuration.TransactionImages.DeleteImagesOlderThan.ToString();

            cbUnusedFilesEnabled.Checked = configuration.UnusedBinaryFiles.IsEnabled;
            nbUnusedFilesQuarantineDays.Text = configuration.UnusedBinaryFiles.QuarantineDays.ToString();
        }

        /// <summary>
        /// Updates the state of the interface.
        /// </summary>
        protected void UpdateInterfaceState()
        {
            //
            // Update the Compress Communications panel.
            //
            if ( cbCompressCommunicationsEnabled.Checked )
            {
                pnlCompressCommunications.RemoveCssClass( "panel-block" ).AddCssClass( "panel-success" );
            }
            else
            {
                pnlCompressCommunications.RemoveCssClass( "panel-success" ).AddCssClass( "panel-block" );
            }
            nbCompressCommunicationsOlderThan.Enabled = cbCompressCommunicationsEnabled.Checked;
            nbCompressCommunicationsBatchSize.Enabled = cbCompressCommunicationsEnabled.Checked;
            cbCompressCommunicationsShouldDecompress.Enabled = cbCompressCommunicationsEnabled.Checked;

            //
            // Update the Delete Communications panel.
            //
            if ( cbDeleteSystemCommunicationsEnabled.Checked )
            {
                pnlDeleteSystemCommunications.RemoveCssClass( "panel-block" ).AddCssClass( "panel-success" );
            }
            else
            {
                pnlDeleteSystemCommunications.RemoveCssClass( "panel-success" ).AddCssClass( "panel-block" );
            }
            nbDeleteSystemCommunicationsOlderThan.Enabled = cbDeleteSystemCommunicationsEnabled.Checked;
            nbDeleteSystemCommunicationsMaximumDaysBack.Enabled = cbDeleteSystemCommunicationsEnabled.Checked;
            vlDeleteSystemCommunicationsSubjects.Enabled = cbDeleteSystemCommunicationsEnabled.Checked;
            lbDeleteSystemCommunicationsSuggestSubjects.Enabled = cbDeleteSystemCommunicationsEnabled.Checked;

            //
            // Update the Transaction Images panel.
            //
            if ( cbTransactionImagesEnabled.Checked )
            {
                pnlTransactionImages.RemoveCssClass( "panel-block" ).AddCssClass( "panel-success" );
            }
            else
            {
                pnlTransactionImages.RemoveCssClass( "panel-success" ).AddCssClass( "panel-block" );
            }
            nbTransactionImagesOlderThan.Enabled = cbTransactionImagesEnabled.Checked;

            //
            // Update the Unused Files panel.
            //
            if ( cbUnusedFilesEnabled.Checked )
            {
                pnlUnusedFiles.RemoveCssClass( "panel-block" ).AddCssClass( "panel-success" );
            }
            else
            {
                pnlUnusedFiles.RemoveCssClass( "panel-success" ).AddCssClass( "panel-block" );
            }
            nbUnusedFilesQuarantineDays.Enabled = cbUnusedFilesEnabled.Checked;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the CheckedChanged event of the bgCompressCommunicationsEnabled control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bgCompressCommunicationsEnabled_CheckedChanged( object sender, EventArgs e )
        {
            UpdateInterfaceState();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbTransactionImagesEnabled control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbTransactionImagesEnabled_CheckedChanged( object sender, EventArgs e )
        {
            UpdateInterfaceState();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbUnusedFilesEnabled control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbUnusedFilesEnabled_CheckedChanged( object sender, EventArgs e )
        {
            UpdateInterfaceState();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbDeleteSystemCommunicationsEnabled control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbDeleteSystemCommunicationsEnabled_CheckedChanged( object sender, EventArgs e )
        {
            UpdateInterfaceState();
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var configuration = new Configuration();

            configuration.CompressCommunications.IsEnabled = cbCompressCommunicationsEnabled.Checked;
            configuration.CompressCommunications.CompressContentOlderThan = nbCompressCommunicationsOlderThan.Text.AsInteger();
            configuration.CompressCommunications.BatchSize = nbCompressCommunicationsBatchSize.Text.AsInteger();
            configuration.CompressCommunications.ShouldDecompress = cbCompressCommunicationsShouldDecompress.Checked;

            configuration.DeleteSystemCommunications.IsEnabled = cbDeleteSystemCommunicationsEnabled.Checked;
            configuration.DeleteSystemCommunications.OlderThan = nbDeleteSystemCommunicationsOlderThan.Text.AsInteger();
            configuration.DeleteSystemCommunications.MaximumDaysBack = nbDeleteSystemCommunicationsMaximumDaysBack.Text.AsInteger();
            configuration.DeleteSystemCommunications.Subjects = vlDeleteSystemCommunicationsSubjects.Value.Split( new[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

            configuration.TransactionImages.IsEnabled = cbTransactionImagesEnabled.Checked;
            configuration.TransactionImages.DeleteImagesOlderThan = nbTransactionImagesOlderThan.Text.AsInteger();

            configuration.UnusedBinaryFiles.IsEnabled = cbUnusedFilesEnabled.Checked;
            configuration.UnusedBinaryFiles.QuarantineDays = nbUnusedFilesQuarantineDays.Text.AsInteger();

            Rock.Web.SystemSettings.SetValue( SystemSettings.CONFIGURATION, configuration.ToJson() );

            nbSaved.Visible = true;
        }

        #endregion

        #region Delete System Communications Suggestions

        /// <summary>
        /// Handles the Click event of the lbDeleteSystemCommunicationsSuggestSubjects control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDeleteSystemCommunicationsSuggestSubjects_Click( object sender, EventArgs e )
        {
            var olderThan = nbDeleteSystemCommunicationsOlderThan.Text.AsIntegerOrNull() ?? 365;
            var maximumDaysBack = nbDeleteSystemCommunicationsMaximumDaysBack.Text.AsIntegerOrNull() ?? 1095;
            var attributeId = AttributeCache.Get( com.blueboxmoon.DatabaseThinner.SystemGuid.Attribute.COMPRESSED_FILE ).Id;
            var rockContext = new RockContext();

            //
            // Custom query. Takes 2 seconds on my system. The LINQ version takes over 30.
            //
            var sql = @"SELECT
C.[Subject], COUNT(*) AS [Count], SUM(DATALENGTH(ISNULL(C.[Message], '')) + ISNULL(BF.[FileSize], 0)) AS [Size]
FROM [Communication] AS C
LEFT JOIN [AttributeValue] AS AV ON AV.[EntityId] = C.[Id] AND AV.[AttributeId] = {0} AND AV.[Value] IS NOT NULL AND AV.[Value] != ''
LEFT JOIN [BinaryFile] AS BF ON BF.[Guid] = AV.[Value]
WHERE C.[Subject] IS NOT NULL AND C.[Subject] != ''
  AND C.[CommunicationType] = 1
  AND C.[CreatedDateTime] IS NOT NULL AND C.[CreatedDateTime] < DATEADD(DAY, -{1}, GETDATE()) AND C.[CreatedDateTime] >= DATEADD(DAY, -{2}, GETDATE())
GROUP BY C.[Subject]
HAVING COUNT(*) > 250";
            var suggestions = rockContext.Database.SqlQuery<DeleteCommunicationSuggestion>( sql, attributeId, olderThan, maximumDaysBack );

            gSuggestedSubjects.SelectedKeys.Clear();
            gSuggestedSubjects.DataKeyNames = new[] { "Subject" };
            gSuggestedSubjects.DataSource = suggestions.OrderByDescending( s => s.Size ).ToList();
            gSuggestedSubjects.DataBind();

            hfSuggestedSubjects.Value = suggestions.ToJson();

            mdSuggestSubjects.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdSuggestSubjects control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdSuggestSubjects_SaveClick( object sender, EventArgs e )
        {
            var keys = gSuggestedSubjects.SelectedKeys;

            //
            // Get existing subjects.
            //
            var subjects = vlDeleteSystemCommunicationsSubjects.Value.Split( new[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

            //
            // Add in only newly selected subjects.
            //
            gSuggestedSubjects.SelectedKeys
                .Cast<string>()
                .Where( s => !subjects.Contains( s ) )
                .ToList()
                .ForEach( s => subjects.Add( s ) );

            vlDeleteSystemCommunicationsSubjects.Value = subjects.AsDelimited( "|" );

            mdSuggestSubjects.Hide();
        }

        /// <summary>
        /// Handles the GridRebind event of the gSuggestedSubjects control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gSuggestedSubjects_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            var suggestions = hfSuggestedSubjects.Value.FromJsonOrNull<List<DeleteCommunicationSuggestion>>();

            if ( gSuggestedSubjects.SortProperty != null )
            {
                suggestions = suggestions.AsQueryable().Sort( gSuggestedSubjects.SortProperty ).ToList();
            }
            else
            {
                suggestions = suggestions.OrderByDescending( s => s.Size ).ToList();
            }

            gSuggestedSubjects.DataSource = suggestions;
            gSuggestedSubjects.DataBind();
        }

        #endregion

        #region Support Classes

        protected class DeleteCommunicationSuggestion
        {
            public string Subject { get; set; }

            public int Count { get; set; }

            public long Size { get; set; }

            public double SizeMB { get { return Size / 1024.0 / 1024.0; } }
        }

        #endregion
    }
}
