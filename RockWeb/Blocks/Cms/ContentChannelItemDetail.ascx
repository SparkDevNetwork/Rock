<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentChannelItemDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentChannelItemDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" >

            <asp:HiddenField ID="hfId" runat="server" />
            <asp:HiddenField ID="hfChannelId" runat="server" />
            <asp:HiddenField ID="hfApprovalStatusPersonAliasId" runat="server" />
            <asp:HiddenField ID="hfApprovalStatus" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lIcon" runat="server" />
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlContentChannel" runat="server" LabelType="Type" />
                    <Rock:HighlightLabel ID="hlStatus" runat="server"  />
                </div>
            </div>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <asp:Panel ID="pnlEditDetails" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                        <Rock:DataTextBox ID="tbTitle" runat="server" SourceTypeName="Rock.Model.ContentChannelItem, Rock" PropertyName="Title" />
                        </div>
                        <div class="col-md-6">
                            <div class="form-group" id="divStatus" runat="server">
                                <label class="control-label">Status</label>
                                <div class="form-control-static">
                                    <asp:HiddenField ID="hfStatus" runat="server" />
                                    <asp:Panel ID="pnlStatus" runat="server" CssClass="toggle-container">
                                        <div class="btn-group btn-toggle">
                                            <a class="btn btn-xs <%=PendingCss%>" data-status="1" data-active-css="btn-default">Pending</a>
                                            <a class="btn btn-xs <%=ApprovedCss%>" data-status="2" data-active-css="btn-success">Approved</a>
                                            <a class="btn btn-xs <%=DeniedCss%>" data-status="3" data-active-css="btn-danger">Denied</a>
                                        </div>
                                    </asp:Panel>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:HtmlEditor ID="htmlContent" runat="server" Label="Content" ResizeMaxWidth="720" Height="500" />
                            <Rock:CodeEditor ID="ceContent" runat="server" Label="Content" EditorHeight="500" EditorMode="Html" EditorTheme="Rock"  />
                        </div>
                    </div>


                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DateTimePicker ID="dtpStart" runat="server" Label="Start" Required="true" />
                            <Rock:NumberBox ID="nbPriority" runat="server" Label="Priority" />
                        </div>
                        <div class="col-md-6">
                            <Rock:DateTimePicker ID="dtpExpire" runat="server" Label="Expire" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                        <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                    </div>

                </asp:Panel>

            </div>

        </asp:Panel>
        
    </ContentTemplate>
</asp:UpdatePanel>
