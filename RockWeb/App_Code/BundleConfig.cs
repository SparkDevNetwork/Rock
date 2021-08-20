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
//
using System.Configuration;
using System.Web.Optimization;

/// <summary>
/// Loads and concats JS bundles, fired during App_Start
/// </summary>
public class BundleConfig
{
    /// <summary>
    /// Registers the bundles.
    /// </summary>
    /// <param name="bundles">The bundles.</param>
	public static void RegisterBundles( BundleCollection bundles )
    {
        // start with a clean bundles (this seems to have fixed the javascript errors that would occur on the first time you debug after opening the solution)
        bundles.ResetAll();

        bundles.Add( new ScriptBundle( "~/Scripts/Bundles/RockJQueryLatest" ).Include(
            "~/Scripts/jquery-3.5.1.min.js",
            "~/Scripts/jquery-migrate-3.1.0.min.js" ) );

        bundles.Add( new ScriptBundle( "~/Scripts/Bundles/WebFormsJs" ).Include(
            "~/Scripts/WebForms/MsAjax/MicrosoftAjax.js",
            "~/Scripts/WebForms/MsAjax/MicrosoftAjaxWebForms.js",
            "~/Scripts/WebForms/WebForms.js",
            "~/Scripts/WebForms/WebUIValidation.js",
            "~/Scripts/WebForms/MenuStandards.js",
            "~/Scripts/WebForms/Focus.js",
            "~/Scripts/WebForms/GridView.js",
            "~/Scripts/WebForms/DetailsView.js",
            "~/Scripts/WebForms/TreeView.js",
            "~/Scripts/WebForms/WebParts.js" ) );

        bundles.Add( new ScriptBundle( "~/Scripts/Bundles/RockLibs" ).Include(
            "~/Scripts/jquery-ui-1.12.1.custom.min.js",
            "~/Scripts/bootstrap.min.js",
            "~/Scripts/bootstrap-timepicker.js",
            "~/Scripts/bootstrap-datepicker.js",
            "~/Scripts/bootstrap-limit.js",
            "~/Scripts/bootstrap-modalmanager.js",
            "~/Scripts/bootstrap-modal.js",
            "~/Scripts/bootbox.min.js",
            "~/Scripts/chosen.jquery.min.js",
            "~/Scripts/typeahead.min.js",
            "~/Scripts/jquery.fileupload.js",
            "~/Scripts/jquery.stickytableheaders.js",
            "~/Scripts/iscroll.js",
            "~/Scripts/jcrop.min.js",
            "~/Scripts/ResizeSensor.js",
            "~/Scripts/ion.rangeSlider/js/ion-rangeSlider/ion.rangeSlider.min.js",
            "~/Scripts/Rock/Extensions/*.js" ) );

        bundles.Add( new ScriptBundle( "~/Scripts/Bundles/RockUi" ).Include(
            "~/Scripts/Rock/coreListeners.js",
            "~/Scripts/Rock/dialogs.js",
            "~/Scripts/Rock/settings.js",
            "~/Scripts/Rock/utility.js",
            "~/Scripts/Rock/Controls/*.js",
            "~/Scripts/Rock/reportingInclude.js" ) );

        bundles.Add( new ScriptBundle( "~/Scripts/Bundles/RockValidation" ).Include(
            "~/Scripts/Rock/Validate/*.js" ) );

        // Creating a separate "Admin" bundle specifically for JS functionality that needs
        // to be included for administrative users
        bundles.Add( new ScriptBundle( "~/Scripts/Bundles/RockAdmin" ).Include(
            "~/Scripts/Rock/Admin/*.js" ) );

        // Creating a separate "RockHtmlEditorPlugins" bundle specifically for JS functionality that needs
        // to be included for HtmlEditor
        bundles.Add( new ScriptBundle( "~/Scripts/Bundles/RockHtmlEditorPlugins" ).Include(
            "~/Scripts/summernote/plugins/*.js" ) );

        // Creating a separate "StructureContentEditorPlugins" bundle specifically for JS functionality that needs
        // to be included for HtmlEditor
        bundles.Add( new ScriptBundle( "~/Scripts/Bundles/StructureContentEditorPlugins" ).Include(
            "~/Scripts/editor.js/*.js" ) );

        // make sure the ConcatenationToken is what we want.  This is supposed to be the default, but it occasionally was an empty string.
        foreach ( var bundle in bundles )
        {
            bundle.ConcatenationToken = ";\r\n";
        }

        var cfg = (System.Web.Configuration.CompilationSection)ConfigurationManager.GetSection( "system.web/compilation" );
        if ( cfg.Debug )
        {
            // remove the js minification if debugging
            foreach ( var bundle in bundles )
            {
                bundle.Transforms.Clear();
            }
        }
	}
}