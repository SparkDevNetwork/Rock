<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:output method="html" indent="yes"/>

  <xsl:template match="/">

    <xsl:for-each select="/*/attributes">
      <section class="attribute-group">

        <header>
          <xsl:value-of select="@category-name"/>
          <a class="edit" href="#">
            <i class="icon-edit"></i>
          </a>
        </header>

        <xsl:if test="attribute">
          <ul class="attributes">
            <xsl:for-each select="attribute">
              <li>
                <xsl:value-of select="."/>
                <xsl:text> </xsl:text>
                <small>
                  <xsl:value-of select="@name"/>
                </small>
              </li>
            </xsl:for-each>
          </ul>
        </xsl:if>

      </section>
    </xsl:for-each>

  </xsl:template>
</xsl:stylesheet>
