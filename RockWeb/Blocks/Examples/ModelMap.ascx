<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ModelMap.ascx.cs" Inherits="RockWeb.Blocks.Examples.ModelMap" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <asp:Literal ID="lClasses" runat="server" ViewStateMode="Disabled" ></asp:Literal>

        <h2>Key</h2>

        <table class="table">
            <tr>
                <td class="text-center"><strong class="text-danger">*</strong></td>
                <td>A required field.</td>
            </tr>
            <tr>
                <td class="text-center"><span class="fa-stack small"><i class="fa fa-database fa-stack-1x"></i><i class="fa fa-ban fa-stack-2x text-danger"></i></span></td>
                <td>Not mapped to the database.  These fields are computed and are only available in the object.</td>
            </tr>
            <tr>
                <td class="text-center"><small><span class="tip tip-lava"></span></small></td>
                <td>These fields are available where Lava is supported.</td>
            </tr>
        </table>

        <script type="text/javascript">
            Sys.Application.add_load(function () {

                // Hide and unhide collapsible areas
                $('.js-example-toggle').on('click', function () {
                    $(this).closest('.panel').find('.panel-body').slideToggle();
                    $(this).find('i.js-toggle').toggleClass('fa-circle-o fa-circle');
                });

                // Hide and unhide inherited properties and methods
                $('.js-model-inherited').on('click', function () {
                    $(this).find('i.js-model-check').toggleClass('fa-check-square-o fa-square-o');
                    $(this).parent().find('li.js-model').toggleClass('non-hidden hidden ');
                });

            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>

