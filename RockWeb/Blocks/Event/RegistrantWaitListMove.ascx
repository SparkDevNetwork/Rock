<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrantWaitListMove.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrantWaitListMove" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlSend" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar-check-o "></i> Wait List Confirmation</h1>
            </div>
            <div class="panel-body">
                <asp:Panel ID="pnlPreview" runat="server">
                    
                    <Rock:NotificationBox ID="nbUpdate" runat="server" NotificationBoxType="Success" />

                    <asp:CheckBox ID="cbShowEmail" runat="server" Text="Send email to individuals" OnCheckedChanged="cbShowEmail_CheckedChanged" AutoPostBack="true" />
                    
                    <asp:Panel ID="pnlEmail" runat="server" Visible="false" CssClass="margin-t-md">
                        <div class="well">

                            <strong>Recipients</strong> <br />
                            <div class="row margin-b-md">
                                
                                <asp:Repeater ID="rptRecipients" runat="server" OnItemDataBound="rptRecipients_ItemDataBound">
                                    <ItemTemplate>
                                        <div class="col-md-4">
                                            <asp:CheckBox ID="cbEmailRecipient" runat="server" Checked="true" CssClass="pull-left" />
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbFromName" runat="server" Label="From Name" Required="true" />
                                    <Rock:RockTextBox ID="tbFromEmail" runat="server" Label="From Email" Required="true" />
                                    <Rock:RockTextBox ID="tbFromSubject" runat="server" Label="Subject" Required="true" />
                                </div>
                                <div class="col-md-12">
                                    <label>Message</label>
                                    <Rock:Toggle ID="tglEmailBodyView" runat="server" CssClass="pull-right" ActiveButtonCssClass="btn-info" ButtonSizeCssClass="btn-xs" OnText="Preview" OffText="Source" Checked="true" OnCheckedChanged="tglEmailBodyView_CheckedChanged" />
                                    <iframe id="ifEmailPreview" runat="server" style="width: 100%; height: 400px; background-color: #fff;" />
                                    <Rock:CodeEditor ID="ceEmailMessage" runat="server" EditorHeight="400" Visible="false" />
                                </div>
                            </div>
                    
                            <asp:LinkButton ID="btnSendEmail" runat="server" Text="Send" CssClass="btn btn-primary margin-t-md" OnClick="btnSendEmail_Click" />
                        </div>
                    </asp:Panel>
                </asp:Panel>
            </div>
        
        </asp:Panel>

        <asp:Panel ID="pnlComplete" runat="server" Visible="false">
            <Rock:NotificationBox ID="nbResult" NotificationBoxType="Success" runat="server" />
        </asp:Panel>

        <script type="text/javascript">
              Sys.WebForms.PageRequestManager.getInstance().add_endRequest(pageLoaded);

              function pageLoaded(sender, args) {
                 window.scrollTo(0,0);
              }
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
