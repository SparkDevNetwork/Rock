<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WebConnectionRequestListLava.ascx.cs" Inherits="RockWeb.Blocks.Connection.WebConnectionRequestListLava" %>

<asp:UpdatePanel ID="upConnectionSelectLava" runat="server">
    <ContentTemplate>
        <!-- Content -->
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <div class="panel-body">
                <div class="row">
                    <div class="col-xs-12">
                        <h2>
                            <asp:Literal ID="lTitle" runat="server"></asp:Literal>
                            <br />
                            <small>
                                <span class="text-muted">
                                    <asp:Literal ID="lSubTitle" runat="server"></asp:Literal>
                                </span>
                            </small>
                        </h2>
                    </div>
                </div>
                <div class="row options-filter-row">
                    <div class="col-xs-12">
                        <asp:LinkButton ID="lbOptions" runat="server" CssClass="text-muted text-semibold pull-right pr-1 mb-2" OnClick="lbOptions_Click"><i class="fa fa-sliders"></i>&nbsp;&nbsp;Options</asp:LinkButton>
                    </div>
                </div>
                <div class="row">
                    <div class="col-xs-12">
                        <asp:Literal ID="lContent" runat="server"></asp:Literal>
                    </div>
                </div>
                <div class="clearfix mt-3 paging">
                    <asp:LinkButton ID="lbLoadPrevious" runat="server" CssClass="btn btn-primary prev-page pull-left" OnClick="lbLoadPrevious_Click">Previous</asp:LinkButton>
                    <asp:LinkButton ID="lbLoadMore" runat="server" CssClass="btn btn-primary next-page pull-right" OnClick="lbLoadMode_Click">More</asp:LinkButton>
                </div>
            </div>
        </asp:Panel>
        <!-- Modal -->
        <Rock:ModalDialog ID="mdOptions" runat="server" Title="Options" SaveButtonText="Save" OnSaveClick="mdOptions_SaveClick">
            <Content>
                <Rock:NotificationBox id="nbWarning" runat="server" NotificationBoxType="Warning" Visible="false">You are not logged in so some options are not available.</Rock:NotificationBox>
                <div class="row">
                    <div class="col-xs-12">
                        <div class="form-group">
                            <Rock:Switch
                            ID="swOnlyShowMyConnections"
                            runat="server"
                            Checked="true"
                            FormGroupCssClass="custom-switch-centered hide-label-sm"
                            Text="Only Show My Connections" />
                        </div>

                        <Rock:RockCheckBoxList ID="cblStates" runat="server" RepeatDirection="Horizontal" Label="Request States" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
