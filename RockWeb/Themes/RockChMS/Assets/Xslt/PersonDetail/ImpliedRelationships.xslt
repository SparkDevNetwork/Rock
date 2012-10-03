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
            <i class="icon-chevron-right"></i>
          </a>
        </header>

        <xsl:if test="members/member">
          <ul class="group-members">
            <xsl:for-each select="members/member">
              <li>
                <a class="highlight">
                  <xsl:attribute name="href">
                    <xsl:value-of select="@id"/>
                  </xsl:attribute>
                  <xsl:choose>
                    <xsl:when test="@photo-id!=''">
                      <i class="icon-user photo"></i>
                    </xsl:when>
                    <xsl:otherwise>
                      <i class="icon-blank photo"></i>
                    </xsl:otherwise>
                  </xsl:choose>
                  <xsl:value-of select="@first-name"/>
                  <xsl:text> </xsl:text>
                  <xsl:value-of select="@last-name"/>
                </a>
              </li>
            </xsl:for-each>
          </ul>
        </xsl:if>

      </section>
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
