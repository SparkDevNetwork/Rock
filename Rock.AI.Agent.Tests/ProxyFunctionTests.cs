using System;
using System.Collections.Generic;

using Microsoft.SemanticKernel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Enums.Core.AI.Agent;
using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Net;
using Rock.Tests.Shared;

namespace Rock.AI.Agent.Tests;

[TestClass]
public class ProxyFunctionTests
{
    #region AddParametersToMergeFields Tests

    [TestMethod]
    public void AddParametersToMergeFields_UnknownParameter_IsIgnored()
    {
        var mergeFields = new Dictionary<string, object>();
        var parameters = new List<ParameterSchema>();
        var args = new KernelArguments
        {
            ["UnknownProperty"] = "Some Value"
        };

        ProxyFunction.AddParametersToMergeFields( mergeFields, parameters, args );

        Assert.AreEqual( 0, mergeFields.Count );
    }

    [TestMethod]
    public void AddParametersToMergeFields_MissingParameter_IsAddedAsNull()
    {
        var mergeFields = new Dictionary<string, object>();
        var parameters = new List<ParameterSchema>
        {
            new ParameterSchema
            {
                Name = "MissingParameter",
                DataType = ParameterSchemaDataType.String,
            }
        };
        var args = new KernelArguments();

        ProxyFunction.AddParametersToMergeFields( mergeFields, parameters, args );

        Assert.IsNull( mergeFields["MissingParameter"] );
    }

    [TestMethod]
    public void AddParametersToMergeFields_CollectionParameter_IsAddedAsCollection()
    {
        var mergeFields = new Dictionary<string, object>();
        var parameters = new List<ParameterSchema>
        {
            new ParameterSchema
            {
                Name = "PersonIds",
                DataType = ParameterSchemaDataType.Number,
                IsCollection = true,
            }
        };
        var args = new KernelArguments
        {
            ["PersonIds"] = new List<int> { 1, 2, 3 }
        };

        ProxyFunction.AddParametersToMergeFields( mergeFields, parameters, args );

        var result = mergeFields["PersonIds"];

        Assert.IsInstanceOfType( result, typeof( List<object> ) );
        CollectionAssert.AreEqual( new List<object> { 1, 2, 3 }, ( List<object> ) result );
    }

    [TestMethod]
    public void AddParametersToMergeFields_NonCollectionParameter_IsAddedAsSingleValue()
    {
        var mergeFields = new Dictionary<string, object>();
        var parameters = new List<ParameterSchema>
        {
            new ParameterSchema
            {
                Name = "PersonId",
                DataType = ParameterSchemaDataType.Number,
                IsCollection = false,
            }
        };
        var args = new KernelArguments
        {
            ["PersonId"] = 42
        };

        ProxyFunction.AddParametersToMergeFields( mergeFields, parameters, args );

        var result = mergeFields["PersonId"];

        Assert.AreEqual( 42, result );
    }

    #endregion

    #region ConvertValueToCollection Tests

    [TestMethod]
    public void ConvertValueToCollection_StringsAsStringCollection_ReturnsStringCollection()
    {
        var expectedValue = new List<string> { "Hello", "World" };

        var result = ProxyFunction.ConvertValueToCollection( expectedValue, ParameterSchemaDataType.String );

        CollectionAssert.AreEqual( expectedValue, result );
    }

    [TestMethod]
    public void ConvertValueToCollection_IntsAsStringCollection_ReturnsStringCollection()
    {
        var expectedValue = new List<string> { "10", "20" };

        var result = ProxyFunction.ConvertValueToCollection( new[] { 10, 20 }, ParameterSchemaDataType.String );

        CollectionAssert.AreEqual( expectedValue, result );
    }

    [TestMethod]
    public void ConvertValueToCollection_SingleStringAsStringCollection_ReturnsStringCollection()
    {
        var expectedValue = new List<string> { "Hello" };

        var result = ProxyFunction.ConvertValueToCollection( "Hello", ParameterSchemaDataType.String );

        CollectionAssert.AreEqual( expectedValue, result );
    }

    [TestMethod]
    public void ConvertValueToCollection_SingleIntAsStringCollection_ReturnsStringCollection()
    {
        var expectedValue = new List<string> { "10" };

        var result = ProxyFunction.ConvertValueToCollection( 10, ParameterSchemaDataType.String );

        CollectionAssert.AreEqual( expectedValue, result );
    }

    [TestMethod]
    public void ConvertValueToCollection_JsonIntArrayAsStringCollection_ReturnsStringCollection()
    {
        var expectedValue = new List<string> { "10", "20" };

        var result = ProxyFunction.ConvertValueToCollection( "[10, 20]", ParameterSchemaDataType.String );

        CollectionAssert.AreEqual( expectedValue, result );
    }

    [TestMethod]
    public void ConvertValueToCollection_JsonNullAsStringCollection_ReturnsEmptyStringCollection()
    {
        var result = ProxyFunction.ConvertValueToCollection( "null", ParameterSchemaDataType.String );

        Assert.That.Empty( result );
    }

    #endregion

    #region ConvertValueToType Tests

    [TestMethod]
    public void ConvertValueToType_StringAsString_ReturnsString()
    {
        var expectedValue = "Hello World!";

        var result = ProxyFunction.ConvertValueToType( expectedValue, ParameterSchemaDataType.String );

        Assert.AreEqual( expectedValue, result );
    }

    [TestMethod]
    public void ConvertValueToType_IntegerAsString_ReturnsString()
    {
        var expectedValue = "42";

        var result = ProxyFunction.ConvertValueToType( 42, ParameterSchemaDataType.String );

        Assert.AreEqual( expectedValue, result );
    }

    [TestMethod]
    public void ConvertValueToType_BooleanAsString_ReturnsString()
    {
        var expectedValue = "True";

        var result = ProxyFunction.ConvertValueToType( true, ParameterSchemaDataType.String );

        Assert.AreEqual( expectedValue, result );
    }

    [TestMethod]
    public void ConvertValueToType_NullAsString_ReturnsNull()
    {
        var result = ProxyFunction.ConvertValueToType( null, ParameterSchemaDataType.String );

        Assert.IsNull( result );
    }

    [TestMethod]
    public void ConvertValueToType_IntAsNumber_ReturnsInt()
    {
        var expectedValue = 42;

        var result = ProxyFunction.ConvertValueToType( 42, ParameterSchemaDataType.Number );

        Assert.AreEqual( expectedValue, result );
    }

    [TestMethod]
    public void ConvertValueToType_DoubleAsNumber_ReturnsInt()
    {
        var expectedValue = 42.0;

        var result = ProxyFunction.ConvertValueToType( 42.0, ParameterSchemaDataType.Number );

        Assert.AreEqual( expectedValue, result );
    }

    [TestMethod]
    public void ConvertValueToType_StringAsNumber_ReturnsDouble()
    {
        var expectedValue = 42.0;

        var result = ProxyFunction.ConvertValueToType( "42", ParameterSchemaDataType.Number );

        Assert.AreEqual( expectedValue, result );
    }

    [TestMethod]
    public void ConvertValueToType_NullAsNumber_ReturnsNull()
    {
        var result = ProxyFunction.ConvertValueToType( null, ParameterSchemaDataType.Number );

        Assert.IsNull( result );
    }

    [TestMethod]
    public void ConvertValueToType_BooleanAsBoolean_ReturnsBoolean()
    {
        var expectedValue = true;

        var result = ProxyFunction.ConvertValueToType( true, ParameterSchemaDataType.Boolean );

        Assert.AreEqual( expectedValue, result );
    }

    [TestMethod]
    public void ConvertValueToType_IntAsBoolean_ReturnsBoolean()
    {
        var expectedValue = true;

        var result = ProxyFunction.ConvertValueToType( 1, ParameterSchemaDataType.Boolean );

        Assert.AreEqual( expectedValue, result );
    }

    [TestMethod]
    public void ConvertValueToType_StringAsBoolean_ReturnsBoolean()
    {
        var expectedValue = true;

        var result = ProxyFunction.ConvertValueToType( "True", ParameterSchemaDataType.Boolean );

        Assert.AreEqual( expectedValue, result );
    }

    [TestMethod]
    public void ConvertValueToType_NullAsBoolean_ReturnsNull()
    {
        var result = ProxyFunction.ConvertValueToType( null, ParameterSchemaDataType.Boolean );

        Assert.IsNull( result );
    }

    [TestMethod]
    public void ConvertValueToType_StringAsUnknown_ReturnsNull()
    {
        var result = ProxyFunction.ConvertValueToType( "Hello World!", ( ParameterSchemaDataType ) 999 );

        Assert.IsNull( result );
    }

    #endregion

    #region ExecuteLava Tests

    [TestMethod]
    public void ExecuteLava_ValidFunction_ReturnsExpectedResult()
    {
        var requestContext = new AgentRequestContext();
        var rockRequestContextMock = new Mock<RockRequestContext>();

        rockRequestContextMock
            .Setup( m => m.GetCommonMergeFields( It.IsAny<Model.Person>(), It.IsAny<Lava.CommonMergeFieldsOptions>() ) )
            .Returns( [] );

        var function = new AgentFunction
        {
            Prompt = "Hello, {{ Name }}!",
            Parameters =
            [
                new ParameterSchema
                {
                    Name = "Name",
                    DataType = ParameterSchemaDataType.String,
                    IsRequired = true,
                    UsageHint = "The name of the person to greet."
                }
            ]
        };

        var args = new KernelArguments
        {
            ["Name"] = "Alisha"
        };

        try
        {
            LavaService.SetCurrentEngine( new FluidEngine() );

            var proxyFunction = new ProxyFunction( requestContext, rockRequestContextMock.Object );
            var result = proxyFunction.ExecuteLava( function, args );

            Assert.AreEqual( "Hello, Alisha!", result );
        }
        finally
        {
            LavaService.SetCurrentEngine( ( Type ) null );
        }
    }
    #endregion
}
