<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContactInfo.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.ContactInfo" %>

<section class="contact-info group">
    <header>Contact Information <a href="#" class="edit"><i class="icon-edit"></i></a></header>
                
    <ul class="phone-numbers">
        <li><a href="#" class="highlight"><i class="icon-phone"></i><span class="phone-unlisted" data-value="623.780.0135">Unlisted</span> <small>Home</small></a></li>
        <li><a href="#" class="highlight"><i class="icon-phone"></i>623.298.2911 <small>Internal</small></a></li>
        <li><a href="#" class="highlight"><i class="icon-phone"></i>623.866.2792 <small>Cell</small></a></li>
        <li><a href="#" class="highlight"><i class="icon-phone"></i>623.376.2444 <small>Work</small></a></li>
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

    <ul class="emails">
        <li>jonathan.edmiston@gmail.com</li>
    </ul>

    <ul class="addresses">
        <li class="group">
            <h4>Home Address</h4>
            <a href="" class="map"><i class="icon-map-marker"></i></a>
            <div class="address">
                <span>9039 W Molly Ln</span>
                <span>Peoria, AZ 85383</span>
            </div>
            <div class="actions">
                <a href="" title="GPS: 33.7281 -112.2546"><i class="icon-globe"></i></a>
                <a href="" title="Address Standardized"><i class="icon-magic"></i></a>
            </div>
        </li>
    </ul>
    <script>
        $('ul.addresses li').live({
            mouseenter:
                function () {
                    var actionsDiv = $('div.actions', this);
                    if (actionsDiv.length > 0) {
                        $(actionsDiv).fadeToggle();
                    }
                },
            mouseleave:
                function () {
                    var actionsDiv = $('div.actions', this);
                    if (actionsDiv.length > 0) {
                        $(actionsDiv).fadeToggle();
                    }
                }
        });
    </script>
</section>
