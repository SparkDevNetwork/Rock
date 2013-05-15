<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExceptionDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.ExceptionDetail" %>
<asp:UpdatePanel ID="upExceptionDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlExceptionSummary" runat="server" Visible="false">
            <fieldset>
                <legend>
                    <h4>Exception Summary</h4>
                </legend>

                <div class="row-fluid">
                    <div class="form-horizontal">
                        <div class="span6">
                            <div>
                                <div class="span2">
                                    Site:
                                </div>
                                <div class="span10">
                                    <asp:Label ID="lblSiteName" runat="server" />
                                </div>
                            </div>
                            <div>
                                <div class="span2">
                                    Page:
                                </div>
                                <div class="span5">
                                    <asp:Label ID="lblPage" runat="server" />
                                </div>
                                <div class="span5">
                                    <asp:HyperLink ID="hlViewPage" runat="server" CssClass="btn" Target="_blank"><i class="icon-arrow-right" ></i></asp:HyperLink>
                                </div>

                            </div>
                        </div>
                    </div>
                    <div class="span6">
                        <div>
                            <div class="span2">
                                User:
                            </div>
                            <div class="span10">
                                <asp:Label ID="lblUserName" runat="server" />
                            </div>
                        </div>
                    </div>
                    <asp:Panel ID="pnlQueryString" runat="server" Visible="true">
                        <div class="span12">Query String:</div>
                        <div class="span12">
                            <asp:Literal ID="litQueryString" runat="server" />
                        </div>
                    </asp:Panel>

                    <div id="controls">
                        <div class="span2">
                            <Rock:LabeledCheckBox ID="cbShowCookies" Help="Show user cookies from time of exception." runat="server" AutoPostBack="true" OnCheckedChanged="cbShowCookies_CheckedChanged" />
                        </div>
                        <div class="span2">
                            <Rock:LabeledCheckBox ID="cbShowServerVariables" Help="Show user server variables from time of exception." runat="server" AutoPostBack="true" OnCheckedChanged="cbShowServerVariables_CheckedChanged" />
                        </div>
                    </div>
                </div>
            </fieldset>
        </asp:Panel>
        <asp:Panel ID="pnlCookies" runat="server" Visible="false">
            <fieldset>
                <legend>Cookies</legend>
                <div class="row-fluid">
                    <div class="span12">
                        <asp:Literal ID="litCookies" runat="server" />
                    </div>
                </div>
            </fieldset>
        </asp:Panel>
        <asp:Panel ID="pnlServerVariables" runat="server" Visible="false">
            <fieldset>
                <legend>Server Variables</legend>
                <div class="row-fluid">
                    <div class="span12">
                        <asp:Literal ID="litServerVariables" runat="server" />
                    </div>
                </div>
            </fieldset>
        </asp:Panel>
        <asp:Panel ID="pnlExceptionDetail" runat="server" Visible="false">
            <fieldset>
                <legend>Exception Details</legend>
                <div>
                    asdfk;
                </div>
            </fieldset>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
