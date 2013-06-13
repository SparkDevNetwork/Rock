<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AddFamily.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.AddFamily" %>

<asp:UpdatePanel ID="upEditPerson" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlFamilyData" runat="server">
            <div class="row-fluid">

                <div class="span4 form-horizontal">

                    <fieldset>
                        <Rock:LabeledTextBox ID="tbFamilyName" runat="server" LabelText="Family Name" CssClass="input-meduim" />
                    </fieldset>

                </div>

                <div class="span8 form-horizontal">

                    <fieldset>
                        <Rock:CampusPicker ID="cpCampus" runat="server" />
                        <Rock:LabeledTextBox ID="tbStreet1" runat="server" LabelText="Address Line 1" />
                        <Rock:LabeledTextBox ID="tbStreet2" runat="server" LabelText="Address Line 2" />
                        <div class="control-group">
                            <div class="control-label">City</div>
                            <div class="controls">
                                <asp:TextBox ID="tbCity" runat="server" CssClass="input-small" />
                                &nbsp;&nbsp;&nbsp;&nbsp;State&nbsp;&nbsp;
                                <Rock:StateDropDownList ID="ddlState" runat="server" />
                                &nbsp;&nbsp;&nbsp;&nbsp;Zip&nbsp;&nbsp;
                                <asp:TextBox ID="tbZip" runat="server" CssClass="input-mini" />
                            </div>
                        </div>
                    </fieldset>
                </div>

            </div>

            <Rock:NewFamilyMembers id="nfmMembers" runat="server" OnAddFamilyMemberClick="nfmMembers_AddFamilyMemberClick" />

        </asp:Panel>

        <asp:HiddenField ID="hfAttributeCategory" runat="server" />
        <asp:Panel ID="pnlAttributes" runat="server" Visible="false">
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
