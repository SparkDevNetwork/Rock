<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TemplateDetail.ascx.cs" Inherits="RockWeb.Blocks.Communication.TemplateDetail" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
 
        <asp:HiddenField ID="hfCommunicationTemplateId" runat="server" />
        <asp:HiddenField ID="hfChannelId" runat="server" />

        <div class="banner">
            <h1><asp:Literal ID="lTitle" runat="server"></asp:Literal></h1>
        </div>

        <asp:ValidationSummary ID="ValidationSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

        <div class="row">
            <div class="col-md-6">
                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.CommunicationTemplate, Rock" PropertyName="Name" />
            </div>
        </div> 
                    
        <div class="row">
            <div class="col-md-12">
                <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.CommunicationTemplate, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
            </div>
        </div>

        <ul class="nav nav-pills nav-pagelist">
            <asp:Repeater ID="rptChannels" runat="server">
                <ItemTemplate>
                    <li class='<%# (int)Eval("Key") == ChannelEntityTypeId ? "active" : "" %>'>
                        <asp:LinkButton ID="lbChannel" runat="server" Text='<%# Eval("Value") %>' CommandArgument='<%# Eval("Key") %>' OnClick="lbChannel_Click" CausesValidation="false">
                        </asp:LinkButton>
                    </li>
                </ItemTemplate>
            </asp:Repeater>
        </ul>
        
        <asp:PlaceHolder ID="phContent" runat="server" />

        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
            <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false" />
        </div>

    </ContentTemplate>
</asp:UpdatePanel>


