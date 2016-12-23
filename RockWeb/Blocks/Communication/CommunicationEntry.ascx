<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunicationEntry.ascx.cs" Inherits="RockWeb.Blocks.Communication.CommunicationEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-comment"></i> New Communication</h1>

                <div class="panel-labels">
                    <div class="label label-default"><a href="#">Use Legacy Editor</a></div>
                </div>
            </div>
            <div class="panel-body">

                <asp:Panel ID="pnlRecipientSelection" runat="server">
                    <h4>Recipient Selection</h4>

                    <Rock:Toggle ID="tglRecipientSelection" runat="server" CssClass="btn-group-justified margin-b-lg" OnText="Select From List" OffText="Select Specific Individuals" Checked="true" OnCssClass="btn-primary" OffCssClass="btn-primary" />
                    
                    <asp:Panel ID="pnlRecipientSelectionList" runat="server">

                        <Rock:RockDropDownList ID="ddlList" runat="server" Label="List" CssClass="input-width-xxl" Required="true">
                            <asp:ListItem Text="All Members and Attendees" />
                        </Rock:RockDropDownList>

                        <label>Segments</label>
                        <p>Optionally, further refine your recipients by filtering by segment.</p>
                        <asp:CheckBoxList ID="cblSegments" runat="server" RepeatDirection="Horizontal" CssClass="margin-b-lg">
                            <asp:ListItem Text="Male" />
                            <asp:ListItem Text="Female" />
                            <asp:ListItem Text="Under 35" />
                            <asp:ListItem Text="35 and older" />
                            <asp:ListItem Text="Members" />
                        </asp:CheckBoxList>

                        <Rock:RockRadioButtonList ID="rblSegmentFilterType" runat="server" Label="Recipients Must Meet" RepeatDirection="Horizontal">
                            <asp:ListItem Text="All segment filters" Value="all" />
                            <asp:ListItem Text="Any segment filter" Value="any" />
                        </Rock:RockRadioButtonList>
                    </asp:Panel>                   

                    <div class="actions">
                        <asp:LinkButton ID="lbRecipientSelectionNext" runat="server" AccessKey="n" Text="Next" DataLoadingText="Next" CssClass="btn btn-primary pull-right" CausesValidation="true" OnClick="lbRecipientSelectionNext_Click" />
                    </div>

                </asp:Panel>

                <asp:Panel ID="pnlEmailEditor" runat="server" Visible="false">
                    
                </asp:Panel>
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>