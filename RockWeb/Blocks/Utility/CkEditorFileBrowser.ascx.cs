//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Linq;
using System.Collections.Generic;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Utility
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "CkEditor FileBrowser" )]
    [Category( "Utility" )]
    [Description( "Block to be used as part of the RockFileBrowser CKEditor Plugin" )]
    public partial class CkEditorFileBrowser : RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods
        
        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            if ( !this.Page.IsPostBack )
            {
                // we are using the Dialog layout, but don't want the Header and Footer to show
                string startupScript = @" 
var iframeHead = $('head');
$('<style> .modal-header { display: none; } .modal-footer { display: none; } </style>').appendTo(iframeHead);

$('.treeview').rockTree( {  
} );

$('.js-folder-treeview').tinyscrollbar({ size: 120, sizethumb: 20 });

$('.treeview').on('rockTree:expand rockTree:collapse rockTree:dataBound rockTree:rendered', function (evt) {

    var $container = $('.js-folder-treeview'),
        $dialog = $('#modal-scroll-container'),
        dialogTop,
        pickerTop,
        amount;

    if ($container.is(':visible')) {
        $container.tinyscrollbar_update('relative');

        if ($dialog.length > 0 && $dialog.is(':visible')) {
            dialogTop = $dialog.offset().top;
            pickerTop = $container.offset().top;
            amount = pickerTop - dialogTop;

            if (amount > 160) {
                $dialog.tinyscrollbar_update('bottom');
            }
        }
    }
});

";
                ScriptManager.RegisterStartupScript( this, this.GetType(), "script_" + this.ID, startupScript, true );
            }

            var sb = new StringBuilder();

            sb.AppendLine( "<ul id=\"treeview\">" );

            string rootFolder = PageParameter("rootFolder");

            if (string.IsNullOrWhiteSpace(rootFolder)) {
                rootFolder = "~/Content";
            }

            string physicalRootFolder = this.Request.MapPath( rootFolder );
            List<string> directoryList = Directory.GetDirectories( physicalRootFolder ).OrderBy( a => a ).ToList();

            foreach ( string directoryPath in directoryList )
            {
                sb.Append( DirectoryNode( directoryPath ) );
            }

            sb.AppendLine( "</ul>" );

            lblFolders.Text = sb.ToString();
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the PageLiquid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated(object sender, EventArgs e)
        {
            
        }

        #endregion

        #region Methods

        protected string DirectoryNode( string directoryPath )
        {
            var sb = new StringBuilder();

            DirectoryInfo directoryInfo = new DirectoryInfo( directoryPath );

            sb.AppendFormat( "<li data-expanded='false' data-id='{0}'><span><i class=\"fa fa-folder\"></i> {1}</span> \n", directoryPath, directoryInfo.Name );

            List<string> subDirectoryList = Directory.GetDirectories( directoryPath ).OrderBy( a => a ).ToList();

            if ( subDirectoryList.Any() )
            {
                sb.AppendLine( "<ul>" );

                foreach ( var subDirectoryPath in subDirectoryList )
                {
                    sb.Append( DirectoryNode( subDirectoryPath ) );
                }

                sb.AppendLine( "</ul>" );
            }

            sb.AppendLine( "</li>" );

            return sb.ToString();
        }

        #endregion
    }
}