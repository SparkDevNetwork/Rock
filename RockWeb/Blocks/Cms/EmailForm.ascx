<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EmailForm.ascx.cs" Inherits="RockWeb.Blocks.Cms.EmailForm" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="emailform">
            <asp:Literal ID="lEmailForm" runat="server" />
        </div>

        <asp:Literal ID="lResponse" runat="server" Visible="false" />

        <script>
            function validateForm() {

                var responseText = '';

                $(".emailform :text").each(function () {
                    if ($(this).attr("required") != null) {
                        if ($(this).val() == '') {
                            var controlLabel = $(this).siblings('label').text();
                            if (controlLabel == null || controlLabel == '') {
                                controlLabel = $(this).attr("id");
                            }
                            responseText += 'Please provide a value for ' + controlLabel + '\n';
                        }
                    }
                });

                if (responseText != '') {
                    alert("A few things before we're done:\n\n" + responseText);
                    return false;
                }

                return true;

            }
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
