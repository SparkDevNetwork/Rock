<%@ Control Language="C#" AutoEventWireup="true" CodeFile="UniversalSearch.ascx.cs" Inherits="RockWeb.Blocks.Cms.UniversalSearch" %>

<style>
    .model-cannavigate {
        cursor: pointer;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server" UpdateMode="Conditional">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block" DefaultButton="btnSearch">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-search"></i> Universal Search</h1>
            </div>
            <div class="panel-body">

                <div class="input-group searchbox">
                    <div class="input-group-addon"><i class="fa fa-search"></i></div>
                    <asp:TextBox id="tbSearch" runat="server" CssClass="form-control" Placeholder="Search Rock" />

                    <span id="spanButtonGroup" runat="server" class="input-group-btn">
                        <asp:LinkButton ID="btnSearch" CssClass="btn btn-primary" runat="server" OnClick="btnSearch_Click">Go</asp:LinkButton>
                    </span>
                </div>

                <div class="row row-eq-height-md margin-t-md">
                    <div class="col-md-3 filter-options">
                        <Rock:RockCheckBoxList ID="cblModelFilter" runat="server" RepeatDirection="Vertical" Label="Types" />

                        <Rock:RockCheckBoxList ID="cblGroupTypes" runat="server" Label="Group Types" />

                        <Rock:RockCheckBoxList ID="cblContentChannelTypes" runat="server" Label="Content Channels" />
                    </div>
                    <div class="col-md-9">
                        
                        <div class="margin-t-md">
                        
                        </div>
                                
                        <div class="margin-t-md">
                            <asp:Literal ID="lResults" runat="server" />
                        </div>
                    </div>
                </div>
            </div>
                
        </asp:Panel>

        <asp:Panel ID="pnlEditModal" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdEdit" runat="server" OnSaveClick="lbSave_Click" Title="Universal Search Configuration" OnCancelScript="clearDialog();">
                <Content>

                    <asp:UpdatePanel ID="upnlEdit" runat="server">
                        <ContentTemplate>
                            <Rock:RockCheckBox ID="cbShowModelFilter" runat="server" Label="Show Model Filter" />
                            

                        </ContentTemplate>
                    </asp:UpdatePanel>
                </Content>
            </Rock:ModalDialog>

        </asp:Panel>

        <script>
            Sys.Application.add_load( function () {
                $(".model-cannavigate").click(function () {
                    window.document.location = $(this).data("href");
                });
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
