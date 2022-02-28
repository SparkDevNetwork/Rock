using Karambolo.AspNetCore.Bundling;

using Microsoft.AspNetCore.Builder;

namespace Rock.Web2.Routing
{
    public static class RockBundlingBuilderExtensions
    {
        /// <summary>
        /// Registers all the Rock bundles.
        /// </summary>
        /// <param name="app">The application.</param>
        public static void UseRockBundles( this IApplicationBuilder app )
        {
            var bundlingOptions = new BundlingOptions
            {
                RequestPath = "/Scripts/Bundles"
            };

            app.UseBundling( bundlingOptions, bundles =>
            {
                bundles.AddJs( "/RockJQueryLatest.js" )
                    .Include( "/Scripts/jquery-3.3.1.js" )
                    .Include( "/Scripts/jquery-migrate-3.0.0.min.js" );

                bundles.AddJs( "/RockLibs.js" )
                .Include( "/Scripts/jquery-ui-1.10.4.custom.min.js" )
                .Include( "/Scripts/bootstrap.min.js" )
                //.Include( "/Scripts/bootstrap-timepicker.js" )
                //.Include( "/Scripts/bootstrap-datepicker.js" )
                .Include( "/Scripts/bootstrap-limit.js" )
                .Include( "/Scripts/bootstrap-modalmanager.js" )
                .Include( "/Scripts/bootstrap-modal.js" )
                .Include( "/Scripts/bootbox.min.js" )
                .Include( "/Scripts/chosen.jquery.min.js" )
                .Include( "/Scripts/typeahead.min.js" )
                //.Include( "/Scripts/jquery.fileupload.js" )
                .Include( "/Scripts/jquery.stickytableheaders.js" )
                //.Include( "/Scripts/iscroll.js" )
                .Include( "/Scripts/jcrop.min.js" )
                .Include( "/Scripts/ResizeSensor.js" )
                //.Include( "/Scripts/ion.rangeSlider/js/ion-rangeSlider/ion.rangeSlider.min.js" )
                .Include( "/Scripts/Rock/Extensions/*.js" );

                bundles.AddJs( "/RockUi.js" )
                    .Include( "/Scripts/Rock/coreListeners.js" )
                    .Include( "/Scripts/Rock/dialogs.js" )
                    .Include( "/Scripts/Rock/settings.js" )
                    .Include( "/Scripts/Rock/utility.js" )
                    .Include( "/Scripts/Rock/Controls/*.js" )
                    .Exclude( "/Scripts/Rock/Controls/bootstrap-colorpicker.min.js" )
                    .Include( "/Scripts/Rock/reportingInclude.js" );

                bundles.AddJs( "/RockValidation.js" )
                    .Include( "/Scripts/Rock/Validate/*.js" );

                // Creating a separate "Admin" bundle specifically for JS functionality that needs
                // to be included for administrative users
                bundles.AddJs( "/RockAdmin.js" )
                    .Include( "/Scripts/Rock/Admin/*.js" );
            } );
        }
    }
}
