<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContactInfo.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.ContactInfo" %>

<section class="contact-info group">
    <header>Contact Information <a href="#" class="edit"><i class="icon-edit"></i></a></header>
                
    <ul id="ulPhoneNumbers" runat="server" class="phone-numbers"></ul>

    <ul class="emails">
        <li><asp:HyperLink ID="hlEmail" runat="server" /></li>
    </ul>

    <script>
        $('ul.phone-numbers li a').live({
            mouseenter:
                function () {
                    var spanItem = $('span.phone-unlisted', this);
                    if (spanItem.length > 0) {
                        $(spanItem).text($(spanItem).attr('data-value'));
                    }

                    var phoneIcon = $('i', this);
                    if (phoneIcon.length > 0) {
                        $(phoneIcon).fadeToggle();
                    }
                },
            mouseleave:
                function () {
                    var spanItem = $('span.phone-unlisted', this);
                    if (spanItem.length > 0) {
                        $(spanItem).text('Unlisted');
                    }

                    var phoneIcon = $('i', this);
                    if (phoneIcon.length > 0) {
                        $(phoneIcon).hide();
                    }
                }
        });
    </script>

</section>
