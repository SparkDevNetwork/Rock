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

        bundles.Add( new ScriptBundle( "~/bundles/WebFormsJs" ).Include(
            "~/Scripts/WebForms/WebForms.js",
            "~/Scripts/WebForms/WebUIValidation.js",
            "~/Scripts/WebForms/MenuStandards.js",
            "~/Scripts/WebForms/Focus.js",
            "~/Scripts/WebForms/GridView.js",
            "~/Scripts/WebForms/DetailsView.js",
            "~/Scripts/WebForms/TreeView.js",
            "~/Scripts/WebForms/WebParts.js" ) );

        bundles.Add( new ScriptBundle( "~/Scripts/Bundles/RockLibs" ).Include(
            "~/Scripts/jquery-ui-1.10.0.custom.min.js",
            "~/Scripts/bootstrap.min.js",
            "~/Scripts/bootstrap-timepicker.js",
            "~/Scripts/bootstrap-datepicker.js",
            "~/Scripts/bootstrap-modalmanager.js",
            "~/Scripts/bootstrap-modal.js",
            "~/Scripts/bootbox.min.js",
            "~/Scripts/typeahead.min.js",
            "~/Scripts/jquery.fileupload.js",
            "~/Scripts/jquery.tinyscrollbar.js",
            "~/Scripts/jcrop.min.js",
            "~/Scripts/ResizeSensor.js",       
            "~/Scripts/Rock/Extensions/*.js" ) );

        bundles.Add( new ScriptBundle( "~/Scripts/Bundles/RockUi" ).Include(
            "~/Scripts/Rock/dialogs.js",
            "~/Scripts/Rock/settings.js",
            "~/Scripts/Rock/utility.js",
            "~/Scripts/Rock/Controls/*.js" ) );

        bundles.Add( new ScriptBundle( "~/Scripts/Bundles/RockValidation" ).Include(
            "~/Scripts/Rock/Validate/*.js" ) );

        // Creating a separate "Admin" bundle specifically for JS functionality that needs
        // to be included for administrative users
        bundles.Add( new ScriptBundle( "~/Scripts/Bundles/RockAdmin" ).Include( 
            "~/Scripts/Rock/Admin/*.js" ) );

        // make sure the ConcatenationToken is what we want.  This is supposed to be the default, but it occassionally was an empty string.
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