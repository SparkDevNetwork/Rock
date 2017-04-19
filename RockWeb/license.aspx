<%@ Page Language="C#" AutoEventWireup="true" CodeFile="license.aspx.cs" Inherits="license" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Rock - License</title>

    <link rel="stylesheet" href="<%= Page.ResolveUrl("~/Themes/Rock/Styles/bootstrap.css") %>"/>
    <link rel="stylesheet" href="<%= Page.ResolveUrl("~/Themes/Rock/Styles/theme.css") %>" />

    <script src="<%= ResolveUrl("~/Scripts/jquery-1.12.4.min.js") %>"></script>

</head>
<body id="splash" class="error">
    <form id="form1" runat="server">

        <div id="content">
            <div id="logo">
                <asp:Literal ID="lLogoSvg" runat="server" />
            </div>

            <div id="content-box">
                <div class="row">
                    <div class="col-md-12">


                        <h1>Rock Relationship Management System License</h1>

                        <h3>Rock Community License</h3>
                        <p>Copyright 2016 Spark Development Network</p>

                        <p>
                            Licensed under the Rock Community License (the "License");
                            you may not use this file except in compliance with the License.
                            You may obtain a copy of the License at
                        </p>

                        <p style="text-align: center;"><a href="http://www.rockrms.com/license">http://www.rockrms.com/license</a></p>

                        <p>
                            Unless required by applicable law or agreed to in writing, software
                            distributed under the License is distributed on an "AS IS" BASIS,
                            WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
                            See the License for the specific language governing permissions and
                            limitations under the License.
                        </p>

                        <h1>Component Licenses</h1>
                        Rock was written using several other open-source projects and packages.  Each 
                        of these is attributed below with their respective licenses.

                        <ul>
                            <li><a href="#aspwebapi">ASP.net Web API</a> (Apache) - <a href="http://aspnetwebstack.codeplex.com/">Project Site</a> </li>
                            
                            <li><a href="http://twitter.github.io/bootstrap/">Bootstrap</a> (Apache)</li>
                            <li><a href="http://entityframework.codeplex.com/">Entity Framework</a> (Apache)</li>
                            <li><a href="http://summernote.org/">Summernote</a> (MIT)</li>
                            <li><a href="https://github.com/facebook-csharp-sdk/facebook-csharp-sdk">Facebook C# SDK</a> (Apache 2)</li>
                            <li><a href="http://fontawesome.github.io/Font-Awesome/">Font Awesome</a> (OFL)</li>
                            <li><a href="http://json.codeplex.com/">JSON.Net</a> (MIT)</li>
                            <li><a href="http://nuget.codeplex.com/">Nuget</a> (Apache)</li>
                            <li><a href="http://nunit.org/index.php?p=license&r=2.6.2">Nunit</a> (Custom)</li>
                            <li><a href="http://quartz-scheduler.org">Quartz</a> (Apache)</li>
                            <li><a href="http://jquery.com/">jQuery</a> (MIT)</li>
                            <li><a href="https://github.com/mikesherov/jquery-idletimer">Idle Timer</a> (Custom)</li>
                            <li><a href="http://keith-wood.name/countdown.html">Countdown</a> (MIT)</li>
                            <li><a href="https://github.com/christianreed/Credit-Card-Type-Detector">jQuery Credit Card Type Detector</a> (Custom)</li>
                            <li><a href="http://threedubmedia.com">Event Drag</a> (Custom)</li>
                            <li><a href="http://xoxco.com/projects/code/tagsinput/">jQuery Tag Input</a> (MIT)</li>
                            <li><a href="http://www.baijs.nl/tinyscrollbar/">Tiny Scrollbar</a> (MIT)</li>
                            <li><a href="http://epplus.codeplex.com/">EPPlus</a> (LGPL)</li>
                            <li><a href="http://dotliquidmarkup.org/">DotLiquid</a> (Apache)</li>
                            <li><a href="https://github.com/tonyheupel/liquid.js">Liquid.js</a> (MIT)</li>
                            <li><a href="http://bootboxjs.com/">Bootbox.js</a> (MIT)</li>
                            <li><a href="http://www.ddaysoftware.com">DDay.iCal</a> (BSD)</li>
                            <li><a href="http://www.codeproject.com/Articles/11305/EXIFectractor-library-extract-EXIF-information">EXIFextractor</a> (Ms-RL)</li>
                            <li><a href="http://dotnetzip.codeplex.com">DotNetZip</a> (Ms-PL)</li>
                            <li><a href="http://nuget.org/packages/WebActivator">WebActivatorEx</a> (Ms-PL)</li>
                            <li><a href="http://nuget.org/packages/WebGrease/">WebGrease</a> (Custom)</li>
                            <li><a href="http://jvashishtha.github.io/bootstrap/javascript.html#limit">Limit Boostrap</a> (Apache)</li>
                            <li><a href="https://github.com/eternicode/bootstrap-datepicker">Boostrap Datepicker</a> (Apache)</li>
                            <li><a href="http://imageresizing.net/">ImageResizer</a> (MIT)</li>
                            <li><a href="https://github.com/ftlabs/fastclick">FastClick</a> (Custom)</li>
                            <li><a href="https://code.google.com/p/google-code-prettify/">Prettify.js</a> (Apache)</li>
                            <li><a href="http://jdewit.github.io/bootstrap-timepicker/index.html">Bootstrap Timepicker</a> (MIT)</li>
                            <li><a href="https://github.com/twilio/twilio-csharp">Twilio</a> (Apache)</li>
                            <li><a href="http://restsharp.org/">RestSharp</a> (Apache)</li>
                            <li><a href="https://github.com/bradyholt/cron-expression-descriptor">Cron Expression Descriptor</a> (MIT)</li>
                            <li><a href="http://ace.c9.io/#nav=about">Ace Code Editor</a> (BSD)</li>
                            <li><a href="http://www.csscript.net/">CS Script</a> (Custom)</li>
                            <li><a href="http://xdt.codeplex.com/">Microsoft.Web.Xdt</a> (Apache)</li>
                            <li><a href="http://twitter.github.io/typeahead.js/">typeahead.js</a> (MIT)</li>
                            <li><a href="http://terrymun.github.io/Fluidbox/">Fluidbox</a> (None)</li>
                            <li><a href="http://imagesloaded.desandro.com/">imagesLoaded</a> (MIT)</li>
                            <li><a href="https://github.com/tobiasahlin/SpinKit">SpinKit</a> (MIT)</li>
                            <li><a href="http://www.webiconset.com/file-type-icons/">Web Icon Set (File Type Icons)</a> (Custom)</li>
                            <li><a href="http://www.paulirish.com/2009/throttled-smartresize-jquery-event-handler/">SmartResize</a> (Creative Commons Public Domain Dedication)</li>
                            <li><a href="http://detectmobilebrowsers.com/">Mobile Detect</a> (Unlicense)</li>
                            <li><a href="http://www.codeproject.com/Articles/4625/Implement-Phonetic-Sounds-like-Name-Searches-wit-4">Double Metaphone</a> (Custom)</li>
                            <li><a href="http://deepliquid.com/content/Jcrop.html">JCrop</a> (MIT)</li>
                            <li><a href="http://snazzymaps.com/">Snazzy Maps</a> (Creative Commons - Share Alike)</li>
                            <li><a href="https://github.com/ten1seven/jRespond">jRespond</a> (MIT)</li>
                            <li><a href="https://github.com/acolangelo/jPanelMenu">jPanelmenu</a> (MIT)</li>
                            <li><a href="https://github.com/martydill/mandrill-inbound-classes">Mandrill Inbound Classes</a> (Custom)</li>
                            <li><a href="https://github.com/tobie/ua-parser">UA-Parser</a> (Apache)</li>
                            <li><a href="https://github.com/cowboy/jquery-dotimeout">doTimeout</a> (MIT)</li>
                            <li><a href="http://bootswatch.com/flatly/">Flatly</a> (MIT)</li>
                            <li><a href="http://www.flotcharts.org/">Flot</a> (Custom)</li>
                            <li><a href="http://iscrolljs.com/">iScroll</a> (MIT)</li>
                            <li><a href="http://humanizr.net/">Humanizer</a> (MIT)</li>
                            <li><a href="http://mediaelementjs.com/">MediaElement.js</a> (MIT)</li>
                            <li><a href="https://github.com/jschr/bootstrap-modal">bootstrap-modal</a> (Apache)</li>
                            <li><a href="https://github.com/javiertoledo/bootstrap-rating-input">Bootstrap Rating Input</a> (MIT)</li>
                            <li><a href="https://github.com/marcj/css-element-queries">ResizeSensor.js</a> (Custom)</li>
                            <li><a href="http://www.appelsiini.net/projects/lazyload">LazyLoad.js</a> (MIT)</li>
                            <li><a href="https://github.com/drvic10k/bootstrap-sortable">Bootstrap Sortable</a> (MIT)</li>
                            <li><a href="http://momentjs.com/">Moment.js</a> (MIT)</li>
                            <li><a href="https://github.com/zeroclipboard">ZeroClipboard</a> (MIT)</li>
                            <li><a href="https://github.com/IonDen/ion.rangeSlider/">RangeSlider</a> (MIT)</li>
                            <li><a href="https://github.com/Knagis/CommonMark.NET">CommonMark</a> (MIT)</li>
                            <li><a href="http://www.dotlesscss.org/">doLess</a> (Apache)</li>
                            <li><a href="https://github.com/mjolnic/bootstrap-colorpicker">ColorPicker</a> (Apache)</li>
                            <li><a href="http://bcrypt.codeplex.com/">BCrypt</a> (BSD)</li>
                            <li><a href="https://github.com/MikaelEliasson/EntityFramework.Utilities/blob/master/EntityFramework.Utilities">EFUtilities</a> (MIT)</li>
                            <li><a href="https://harvesthq.github.io/chosen/">Chosen</a> (MIT)</li>
                            <li><a href="https://github.com/alxlit/bootstrap-chosen">Bootstrap Stylsheet for Chosen</a> (MIT)</li>
                            <li><a href="https://github.com/elastic/elasticsearch-net">Elasticsearch NEST Client</a> (Apache 2)</li>
                            <li><a href="https://github.com/bcuff/elasticsearch-net-aws">Elasticsearch Net for Amazon AWS</a> (Apache 2)</li>
                        </ul>
                    </div>
                </div>
            </div>
        </div>

    </form>
</body>
</html>
