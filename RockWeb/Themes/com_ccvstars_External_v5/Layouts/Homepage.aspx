<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctHeader" ContentPlaceHolderID="header" runat="server">
    <!-- Page Header -->
    <header class="masthead big-logo">
        <div class="container">

            <div class="row">
                <div class="col-sm-6">
                    <Rock:Zone Name="Logo" runat="server" />
                </div>
                <div class="col-sm-6">
                    <Rock:Zone Name="Header" runat="server" />
                    <Rock:Zone Name="Login" runat="server" />
                    <Rock:Zone Name="Navigation" runat="server" />
                </div>
            </div>

        </div>
    </header>
</asp:Content>

<asp:Content ID="ctFeature" ContentPlaceHolderID="feature" runat="server">

    <div class="container">
        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error" style="display:none">
            <p><strong>Error</strong></p>
            <span class="ajax-error-message"></span>
        </div>
    </div>

    <section class="main-feature">
        <div class="container">
            <h2 class="margin-v-lg">Choose Your Sport</h2>

            <div class="row">
                <div class="col-md-6">
                    <div class="module module-with-footer">
                        <div class="module-body">
                            <h3 class="module-title margin-b-lg">Peoria Campus</h3>
                            <Rock:Zone Name="PeoriaSports" runat="server" />
                        </div>
                        <div class="module-mid js-fieldstatus clearfix">
                            <strong class="pull-left margin-r-sm">Field Status:</strong>
                            <div class="pull-left">
                                <Rock:Zone Name="PeoriaStatus" runat="server" />
                            </div>
                        </div>
                        <div class="module-footer">
                            <Rock:Zone Name="PeoriaContact" runat="server" />
                        </div>
                    </div>
                    <div class="module">
                        <div class="module-body">
                            <h3 class="module-title margin-b-lg">Avondale Campus</h3>
                            <Rock:Zone Name="AvondaleSports" runat="server" />
                        </div>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="module module-with-footer">
                        <div class="module-body">
                            <h3 class="module-title margin-b-lg">Surprise Campus</h3>
                            <Rock:Zone Name="SurpriseSports" runat="server" />
                        </div>
                        <div class="module-mid js-fieldstatus clearfix">
                            <strong class="pull-left margin-r-sm">Field Status:</strong>
                            <div class="pull-left">
                                <Rock:Zone Name="SurpriseStatus" runat="server" />
                            </div>
                        </div>
                        <div class="module-footer">
                            <Rock:Zone Name="SurpriseContact" runat="server" />
                        </div>
                    </div>

                    <div class="module module-with-footer">
                        <div class="module-body">
                            <h3 class="module-title margin-b-lg">East Valley Campus</h3>
                            <Rock:Zone Name="EastValleySports" runat="server" />
                        </div>
                        <div class="module-mid js-fieldstatus clearfix">
                            <strong class="pull-left margin-r-sm">Field Status:</strong>
                            <div class="pull-left">
                                <Rock:Zone Name="EastValleyStatus" runat="server" />
                            </div>
                        </div>
                        <div class="module-footer">
                            <Rock:Zone Name="EastValleyContact" runat="server" />
                        </div>
                    </div>
                </div>
            </div>
            <Rock:Zone Name="Feature" runat="server" />
        </div>
    </section>

</asp:Content>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    <section class="sub-feature">
        <div class="container">
            <Rock:Zone Name="Sub Feature" runat="server" />
        </div>
    </section>

	<main class="container">

        <!-- Start Content Area -->

        <div class="row">
            <div class="col-md-12">
                <Rock:Zone Name="Section A" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <Rock:Zone Name="Section B" runat="server" />
            </div>
            <div class="col-md-4">
                <Rock:Zone Name="Section C" runat="server" />
            </div>
            <div class="col-md-4">
                <Rock:Zone Name="Section D" runat="server" />
            </div>
        </div>

        <!-- End Content Area -->

	</main>

</asp:Content>

<asp:Content ID="ctFooter" ContentPlaceHolderID="footer" runat="server">
    <footer class="mainfooter mainfooter-dark">
        <div class="container">

            <div class="pull-right margin-t-md">
                <a href="http://ccv.church">
                    <svg version="1.2" baseProfile="tiny" id="Layer_1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink"
                         x="0px" y="0px" width="53.1px" height="20px" viewBox="740.4 461.8 53.1 20" xml:space="preserve">
                        <g>
                            <path d="M744.1,463c-0.6-0.3-1.5,0.3-2.1,1.2c-0.5,0.7-1,2.4-0.2,2.9c1.1,0.5,2.1-0.9,2.6-1.5C744.9,464.7,745.1,463.4,744.1,463z"
                                />
                            <path d="M757.8,462.9c-0.6-0.3-1.5,0.3-2.1,1.2c-0.5,0.7-1,2.4-0.2,2.9c1.1,0.5,2.1-0.9,2.6-1.5
                                C758.6,464.7,758.8,463.4,757.8,462.9z"/>
                            <path d="M779.1,462.6c-0.6-0.3-1.5,0.3-2.1,1.2c-0.5,0.7-1,2.4-0.2,2.9c1.1,0.5,2.1-0.9,2.6-1.5
                                C779.7,464.3,779.9,462.9,779.1,462.6z"/>
                            <path d="M750.8,465.1c-1.1-1.3-9.9,2.7-10.4,6.7c-0.6,5,7.8,11.2,8.7,9.2c0.4-0.7-6.8-4.3-4.9-9C746.2,467,751.8,466.2,750.8,465.1
                                z"/>
                            <path d="M764.4,465.1c-1.1-1.3-9.9,2.7-10.4,6.7c-0.6,5,7.3,11.3,8.7,9.2c0.5-0.7-6.8-4.3-4.9-9
                                C759.8,467.1,765.4,466.2,764.4,465.1z"/>
                            <path d="M768.9,464.3c-0.7,0-0.9,0.2-1,0.5c-0.1,0.6,3.7,5,4.5,8c0.7,3.2,0.5,7.9,0.6,8.3c0.2,0.4,0.6,0.2,1.3,0.3
                                c0.6,0.1,1.2,0.5,1.6,0.2c0.2-0.2,2.7-8.3,7.3-12.2c4.9-4.1,10.1-6.4,10.2-6.8c0.1-0.3-1-0.7-1.9-0.9c-1-0.1-6.1,2.3-9.7,5.7
                                c-3.7,3.5-5.7,8-6.4,7.9c-0.2,0-0.5-1.9-2-5.3C772,466.2,769.7,464.2,768.9,464.3z"/>
                        </g>
                    </svg>
                </a>
            </div>

            <Rock:Zone Name="Footer" runat="server" />

        </div>
    </footer>
</asp:Content>

<asp:Content ID="ctScripts" ContentPlaceHolderID="scripts" runat="server">
   <script>
       $(document).ready(function(){
           $('.js-fieldstatus').each(function(){
                var $this = $(this)
                if ($this.text().toLowerCase().indexOf('some') !== -1) {
                    $this.addClass('alert-warning')
                    return
                }
                if ($this.text().toLowerCase().indexOf('open') !== -1) {
                    $this.addClass('alert-success')
                    return
                }
                if ($this.text().toLowerCase().indexOf('close') !== -1) {
                    $this.addClass('alert-danger')
                    return
                }
           })
       })
   </script>
</asp:Content>

