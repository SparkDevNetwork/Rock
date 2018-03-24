<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CCVGroupListPersonalizedLava.ascx.cs" Inherits="Plugins.church_ccv.Groups.CCVGroupListPersonalizedLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfGroupViewCSS" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfAddGroupMemberCSS" runat="server" ClientIDMode="Static" Value="hidden" />
        
        <asp:Panel ID="pnlGroupView" runat="server" ClientIDMode="Static" CssClass="">
            <asp:Literal ID="lContent" runat="server"></asp:Literal>

            <asp:Literal ID="lDebug" runat="server"></asp:Literal>
        </asp:Panel>
        
        <asp:Panel ID="pnlAddGroupMember" runat="server" ClientIDMode="Static" CssClass="hidden">

            <asp:HiddenField ID="hfGroupId" runat="server" ClientIDMode="Static" />
            <asp:HiddenField ID="hfDefaultGroupRoleId" runat="server" ClientIDMode="Static" />

            <asp:Literal ID="lAddGroupMemberPreHTML" runat="server"></asp:Literal>

            <Rock:NotificationBox ID="nbGroupMemberErrorMessage" runat="server" NotificationBoxType="Danger" ClientIDMode="Static" />
                
            <Rock:PersonPicker runat="server" ID="ppGroupMemberPerson" Label="Person" Required="true"/>

            <Rock:RockRadioButtonList ID="rblStatus" runat="server" Label="Member Status" RepeatDirection="Horizontal" />
        
            <div class="actions">
                <asp:Button ID="btnSaveGroupMember" runat="server" AccessKey="s" CssClass="btn btn-primary" Text="Save" OnClick="btnSaveGroupMember_Click" />
                <asp:LinkButton id="btnCancelGroupMember" runat="server" AccessKey="c" CssClass="btn btn-link" OnClientClick="btnCancelAddGroupMember(); return false;" CausesValidation="false">Cancel</asp:LinkButton>
            </div>

            <asp:Literal ID="lAddGroupMemberPostHTML" runat="server"></asp:Literal>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>

<script type="text/javascript">
    // handle button click to cancel adding group member
    function btnCancelAddGroupMember()
    {
        // show/hide panels and set their hidden field css values
        $("#hfAddGroupMemberCSS").val("hidden");
        toggleAddPanel("hide", "pnlAddGroupMember");
        $("#hfGroupViewCSS").val("");
        toggleAddPanel("show", "pnlGroupView");

        // clear any error messages
        //var errorMessage = $("#nbGroupMemberErrorMessage");
        //errorMessage.removeClass();
        $("div").remove("#nbGroupMemberErrorMessage");
    }

    // sets a panel css to show / hide the panel
    function toggleAddPanel(state, panel) {
        var panel = $(`#${panel}`);

        switch (state)
        {
            case "hide":
                panel.addClass("hidden");
                break;
            default:
                panel.removeClass("hidden");
        }
    }

    // handle button click to add group member
    function btnAddGroupMember(groupId, defaultGroupRoleId)
    {
        // show/hide panels and set their hidden field css values
        toggleAddPanel("hide", "pnlGroupView");
        $("#hfGroupViewCSS").val("hidden");
        toggleAddPanel('show', "pnlAddGroupMember");
        $("#hfAddGroupMemberCSS").val("");

        // set hidden field values used by group service
        $("#hfGroupId").val(groupId);
        $("#hfDefaultGroupRoleId").val(defaultGroupRoleId);
    }
</script>
