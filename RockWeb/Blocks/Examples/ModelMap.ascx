<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ModelMap.ascx.cs" Inherits="RockWeb.Blocks.Examples.ModelMap" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <asp:Literal ID="lClasses" runat="server" ViewStateMode="Disabled" ></asp:Literal>

        <script type="text/javascript">
            Sys.Application.add_load(function () {

                // Hide and unhide collapsible areas
                $('.js-example-toggle').on('click', function () {
                    $(this).closest('.panel').find('.panel-body').slideToggle();
                    $(this).find('i.js-toggle').toggleClass('fa-circle-o fa-circle');
                });

            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>

