<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    <Rock:Zone Name="Feature" runat="server" />

    <div class="position-relative soft xs-soft-half hard-bottom xs-hard-bottom clearfix">

        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error no-index" style="display:none">
            <p><strong>Error</strong></p>
            <span class="ajax-error-message"></span>
        </div>

        <Rock:Lava ID="PageColor" runat="server">
            {% assign pageColor = CurrentPage | Attribute:'PageColor' %}
            <div class="position-absolute full-screen" style="background-color: {{ pageColor }};"></div>
        </Rock:Lava>

        <div id="content" class="container-fluid">

            <div class="row">
                <div class="col-xs-12 col-sm-12 col-md-3 col-lg-4">
                    <Rock:Zone Name="Section A" runat="server" />
                </div><div class="col-xs-12 col-sm-12 col-md-9 col-lg-8">
                    <Rock:Zone Name="Main" runat="server" />
                </div>
            </div>

            <Rock:Zone Name="Section B" runat="server" />

            <div class="page-constrained mx-auto">
                <Rock:Zone Name="Section C" runat="server" />
            </div>

            <Rock:Zone Name="Section D" runat="server" />

        </div>

    </div>

</asp:Content>