<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LifeGroupSearch.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.LifeGroupFinder.LifeGroupSearch" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <div class="panel-body">
                <h2>
                    <center><Rock:RockLiteral ID="lTitle" runat="server" Text="Finding a Life Group" /></center>
                </h2>
                </br>
                    <h4>
                        <center>Only two things are needed to get started</center>
                    </h4>

                <div class="row">
                    <div class="col-md-4">
                        Please select primary campus
                    </div>
                    <div class="col-md-4">
                        <Rock:RockDropDownList ID="ddlCampus" runat="server" />
                    </div>
<%--                    <div class="col-md-4">
                        View a
                        <asp:LinkButton ID="lbMap" runat="server" Text="map" OnClick="lbMap_Click" CausesValidation="false"/>
                        from all groups on this campus.                   
                    </div>--%>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <div class="pull-right">
                            <asp:Panel ID="pnlLogin" runat="server">
                                <asp:LinkButton ID='lbLogin' runat='server' Text='Sign in' OnClick='lbLogin_Click' CausesValidation="false"/>
                                to autocomplete forms. 
                            </asp:Panel>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-10">
                            <Rock:AddressControl ID="acAddress" runat="server" Required="true" RequiredErrorMessage="Your Address is Required" />
                        </div>
                        <div class="col-md-2">
                            <asp:LinkButton ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" />
                        </div>
                    </div>

                    <div class="col-md-12">
                        <div class="pull-right">
                            <asp:LinkButton ID="lbSecurity" runat="server" Text="Why" OnClick="lbSecurity_Click" CausesValidation="false"/>
                            your information is safe.
                        </div>
                    </div>
                </div>
                <section class="panel panel-widget groupsearch-filter">
                    <header class="panel-heading clearfix clickable">
                        <Rock:HiddenFieldWithClass ID="_hfExpanded" runat="server" CssClass="filter-expanded" Value="False" />
                        <div class="pull-left panel-actions">
                            <a class='btn btn-xs btn-link'>
                                <i class='groupsearch-filter-state fa fa-chevron-right'></i>
                            </a>
                        </div>
                        <div class="filter-toggle pull-left">
                            <h3 class="panel-title">
                                <span>This is a test</span>
                            </h3>
                        </div>
                    </header>
                    <div class="panel-body" style="display: none;">
                        <div class="row">
                            <div class="col-md-6">
                                If you would have children and want to find groups with children their age please select all that apply                           
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBoxList ID="cblChildren" runat="server" RepeatColumns="2" RepeatDirection="Horizontal" />
                            </div>
                        </div>
                        <hr />
                        <div class="row">
                            <div class="col-md-6">
                                Day the group meets?
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBoxList ID="cblDays" runat="server" RepeatDirection="Horizontal" />
                            </div>
                        </div>
                        <hr />
                        <div class="row">
                            <div class="col-md-6">
                                Would pets take away from your experience?
                            </div>
                            <div class="col-md-3">
                                <Rock:RockCheckBox ID="cbPets" runat="server" Text="Yes" Help="This will remove groups that have pets from your search" />
                            </div>
                            <div class="col-md-3">
                                <asp:LinkButton ID="lbSearch" CssClass="btn btn-primary" runat="server" Text="Search" OnClick="btnSearch_Click" />
                            </div>
                        </div>
                    </div>
                </section>
                <asp:PlaceHolder ID="phSearchFilter" runat="server" />
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
