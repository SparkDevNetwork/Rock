<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageRouteDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.PageRouteDetail" %>

<asp:UpdatePanel ID="upPageRoutes" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">

            <asp:HiddenField ID="hfPageRouteId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-exchange"></i> <asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <fieldset>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:PagePicker ID="ppPage" runat="server" Label="Page" Required="true" PromptForPageRoute="false"/>
                        </div>
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbRoute" runat="server" SourceTypeName="Rock.Model.PageRoute, Rock" PropertyName="Route" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>     
                         
                </fieldset>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>


