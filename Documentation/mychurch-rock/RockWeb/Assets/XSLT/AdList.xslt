<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="html" indent="yes"/>

  <xsl:template match="/">
    <div class="ad-list">
      <xsl:for-each select="Ads/Ad">

        <div class="ad-content">
          <div class="ad-url">
            <a>
              <xsl:attribute name="href">
                <xsl:value-of select="@Url"/>
              </xsl:attribute>
              <xsl:value-of select="@Title" disable-output-escaping="yes"/>
            </a>
          </div>
          <div class="ad-attributes">
            <xsl:for-each select="./Attributes/Attribute" >
              <div>
                <xsl:value-of select="@Value" disable-output-escaping="yes"/>
              </div>
            </xsl:for-each>
          </div>
        </div>
      </xsl:for-each>
    </div>
  </xsl:template>
</xsl:stylesheet>
