<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:output method="html" indent="yes"/>

  <xsl:template match="/">
    <xsl:for-each select="/*/group">
      <section>
        <xsl:attribute name="class">
          <xsl:value-of select="@class-name"/> group
        </xsl:attribute>
        <header>
          <xsl:value-of select="@name"/>
          <a class="edit" href="#">
            <i class="icon-edit"></i>
          </a>
        </header>

        <xsl:if test="members/member">
          <ul class="group-members">
            <xsl:for-each select="members/member">
              <li>
                <a>
                  <xsl:attribute name="href">
                    <xsl:value-of select="@id"/>
                  </xsl:attribute>
                  <xsl:if test="@photo-id!='0'">
                    <img>
                      <xsl:attribute name="src">../image.ashx?id=<xsl:value-of select="@photo-id"/>&#38;maxwidth=38&#38;maxheight=38</xsl:attribute>
                    </img>
                  </xsl:if>                    
                  <h4>
                    <xsl:value-of select="@first-name"/>
                  </h4>
                  <small>
                    <xsl:value-of select="@role"/>
                  </small>
                </a>
              </li>
            </xsl:for-each>
          </ul>
        </xsl:if>

        <xsl:if test="locations/location/address">
          <ul class="addresses">
            <xsl:for-each select="locations/location">
              <xsl:if test="address">

                <li class="group">
                  <h4>
                    <xsl:value-of select="@type"/>
                  </h4>
                  <a class="map">
                    <i class="icon-map-marker"></i>
                  </a>
                  <div class="address">
                    <xsl:if test="address/@street1">
                      <span>
                        <xsl:value-of select="address/@street1"/>
                      </span>
                    </xsl:if>
                    <xsl:if test="address/@street2">
                      <span>
                        <xsl:value-of select="address/@street2"/>
                      </span>
                    </xsl:if>
                    <span>
                      <xsl:value-of select="address/@city"/>,
                      <xsl:value-of select="address/@state"/>
                      <xsl:value-of select="address/@zip"/>
                    </span>
                  </div>
                  <div class="actions" style="display: none;">
                    <a title="GPS" href="../Blocks/Crm/PersonDetail/#">
                      <i class="icon-globe"></i>
                    </a>
                    <a title="Address Standardized" href="../Blocks/Crm/PersonDetail/#">
                      <i class="icon-magic"></i>
                    </a>
                  </div>
                </li>
              </xsl:if>

            </xsl:for-each>
          </ul>
        </xsl:if>

      </section>
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
