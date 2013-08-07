<%@ Page ValidateRequest="false" Language="C#" MasterPageFile="~/Themes/RockChMS/Layouts/Site.Master"
    AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    <!-- Page Header -->
    <header class="navbar navbar-static-top pageheader">
        <div class="navbar-inner">
            <div class="container-fluid">
                <div class="row-fluid">
                    <div class="span2 clearfix">

                        <asp:HyperLink ID="HyperLink1" runat="server" CssClass="brand" NavigateUrl="~" ToolTip="Rock ChMS">
                            <asp:Image ID="Image1" runat="server" AlternateText="Rock ChMS" ImageUrl="~/Assets/Images/rock-logo.svg" CssClass="pageheader-logo" />
                        </asp:HyperLink>

                    </div>

                    <div class="span10 clearfix">

                        <div class="pageheader-collapse pull-right">
                            <a class="btn btn-navbar" data-target=".nav-collapse" data-toggle="collapse">
                                <span class="icon-bar"></span>
                                <span class="icon-bar"></span>
                                <span class="icon-bar"></span>
                            </a>

                            <div class="nav-collapse collapse">

                                <Rock:Zone ID="Heading" Name="Header" runat="server" />

                            </div>
                        </div>
                        <!-- collapse container -->

                        <Rock:SearchField ID="searchField" runat="server" />
                    </div>
                    <!-- end column -->
                </div>
                <!-- end row -->

            </div>
            <!-- end container -->
        </div>
        <!-- end navbar-inner -->
    </header>

    <nav class="navbar navbar-static-top pagenav">
        <div class="navbar-inner">
            <div class="container-fluid">

                <Rock:Zone ID="Menu" runat="server" />

            </div>
        </div>
    </nav>

    <div class="navbar navbar-static-top pagetitle">
        <div class="navbar-inner">
            <div class="container-fluid">
                <div class="row-fluid">
                    <div class="span6">
                        <Rock:PageIcon ID="PageIcon" runat="server" /> <h1><Rock:PageTitle ID="PageTitle" runat="server" /></h1>
                        <Rock:Zone ID="PageTitleBar" runat="server" />
                    </div>
                    <div class="span6">
                        <Rock:Zone ID="Zone1" runat="server" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="container-fluid body-content">
        <div class="row-fluid">
            <div class="span12">
                <Rock:PageBreadCrumbs ID="PageBreadCrumbs" runat="server" />
                

                <%-- Content Area --%>
                    
                <!-- display any ajax error messages here (use with ajax-client-error-handler.js) -->
                <div class="alert alert-error ajax-error" style="display:none">
                    <strong>Ooops!</strong>
                    <span class="ajax-error-message" / ></span>
                </div>

                <div id="group-viewer" class="row-fluid">
                    
                    <div id="left-column" class="span3">

                        <div id="left-column-content">
                            <Rock:Zone ID="LeftContent" runat="server" />
                        </div>
                    </div>
                    <div id="right-column" class="span9">
                        <div class="panel">
                            <Rock:Zone ID="RightContent" runat="server" />
                        </div>
                    </div>
                </div>

                <%-- End Content Area --%>
            </div>
        </div>
    </div>

    <footer class="page-footer">
        <div class="container-fluid">
            <div class="row-fluid">
                <div class="span12">
                    <Rock:Zone ID="Footer" runat="server" />
                </div>
            </div>
        </div>
    </footer>

</asp:Content>



