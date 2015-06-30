<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationEntry.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
<ContentTemplate>

    <asp:ValidationSummary ID="vsSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

    <asp:Panel ID="pnlHowMany" runat="server" Visible="false">
        <h1>How many people will you be registering?</h1>
        <asp:HiddenField ID="hfMinRegistrants" runat="server" />
        <asp:HiddenField ID="hfMaxRegistrants" runat="server" />
        <asp:HiddenField ID="hfHowMany" runat="server" />
        <asp:Label ID="lblHowMany" runat="server" Text="1" />
        <a class="btn btn-default js-how-many-add"><i class="fa fa-plus fa-lg"></i></a>
        <a class="btn btn-default js-how-many-subtract"><i class="fa fa-minus fa-lg"></i></a>
        <div class="actions">
            <asp:LinkButton ID="lbHowManyNext" runat="server" AccessKey="n" Text="Next" CssClass="btn btn-primary pull-right" CausesValidation="true" OnClick="lbHowManyNext_Click" />
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlRegistrant" runat="server" Visible="false">
        <h1><asp:Literal ID="lRegistrantTitle" runat="server" /></h1>
        <asp:PlaceHolder ID="phRegistrantForm" runat="server" />
        <asp:PlaceHolder ID="phFees" runat="server" />
        <div class="actions">
            <asp:LinkButton ID="lbRegistrantPrev" runat="server" AccessKey="p" Text="Previous" CssClass="btn btn-default" CausesValidation="false" OnClick="lbRegistrantPrev_Click"  />
            <asp:LinkButton ID="lbRegistrantNext" runat="server" AccessKey="n" Text="Next" CssClass="btn btn-primary pull-right" CausesValidation="true" OnClick="lbRegistrantNext_Click" />
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlSummaryAndPayment" runat="server" Visible="false" >
        <h1>Summary</h1>
        <div class="actions">
            <asp:LinkButton ID="lbSummaryPrev" runat="server" AccessKey="p" Text="Previous" CssClass="btn btn-default" CausesValidation="false" OnClick="lbSummaryPrev_Click" />
            <asp:LinkButton ID="lbSummaryNext" runat="server" AccessKey="n" Text="Next" CssClass="btn btn-primary pull-right" CausesValidation="true" OnClick="lbSummaryNext_Click" />
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlSuccess" runat="server" Visible="false" >
        <h1>Success</h1>
    </asp:Panel>

</ContentTemplate>
</asp:UpdatePanel>
