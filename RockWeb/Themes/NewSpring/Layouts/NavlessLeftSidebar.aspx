<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

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

        <Rock:Zone Name="Feature" runat="server" />

        <div id="content" class="clearfix">
            <div class="row">
                <div class="col-xs-12 col-sm-5 col-md-4 col-lg-3">
                    <Rock:Zone Name="Section A" runat="server" />
                </div><div class="col-xs-12 col-sm-7 col-md-8 col-lg-9">
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