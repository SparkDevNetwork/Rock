<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
    <xsl:output method="html" indent="yes"/>

    <xsl:template match="/">
        <xsl:for-each select="/*/group">
            <div>
                <xsl:attribute name="class">
                    container-fluid <xsl:value-of select="@class-name"/>-group
                </xsl:attribute>

                <div class="actions" style="display: none;">
                    <a class="edit btn btn-mini" href="#">
                        <i class="icon-pencil"></i>
                        Edit Family
                    </a>
                </div>
                <div class="row-fluid">
                    
                    <div class="span8 members clearfix">
                        <header>
                            <xsl:value-of select="@name"/>
                        </header>
                        <xsl:if test="members/member">
                            <ul class="clearfix">
                                <xsl:for-each select="members/member">
                                    <li>
                                        <a>
                                            <xsl:attribute name="href">
                                                <xsl:value-of select="@id"/>
                                            </xsl:attribute>
                                            <xsl:if test="@photo-id!='0'">
                                                <img>
                                                    <xsl:attribute name="src">
                                                        <xsl:value-of select="@photo-url"/>
                                                    </xsl:attribute>
                                                </img>
                                            </xsl:if>
                                            <div class="member">
                                                <h4>
                                                    <xsl:value-of select="@first-name"/>
                                                </h4>
                                                <small class="age">
                                                    <xsl:value-of select="@age"/>
                                                </small>
                                                <small>
                                                    <xsl:value-of select="@role"/>
                                                </small>
                                            </div>
                                        </a>
                                    </li>
                                </xsl:for-each>
                            </ul>
                        </xsl:if>
                    </div>

                    <div class="span4 addresses clearfix">
                        <xsl:if test="locations/location/address">
                            <ul>
                                <xsl:for-each select="locations/location">
                                    <xsl:if test="address">

                                        <li class="address clearfix">
                                            <h4>
                                                <xsl:value-of select="@type"/> Address
                                            </h4>
                                            <a class="map" title="Map This Address">
                                                <i class="icon-map-marker"></i>
                                            </a>
                                            <div class="address">
                                                <xsl:if test="address/@street1">
                                                    <xsl:value-of select="address/@street1"/>
                                                    <br/>
                                                </xsl:if>
                                                <xsl:if test="address/@street2">
                                                    <xsl:value-of select="address/@street2"/>
                                                    <br/>
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
                    </div>
                    
                </div>

            </div>
        </xsl:for-each>
    </xsl:template>
</xsl:stylesheet>
