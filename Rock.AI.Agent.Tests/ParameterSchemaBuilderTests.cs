using System.Text.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Enums.AI.Agent;

namespace Rock.AI.Agent.Tests
{
    [TestClass]
    public class ParameterSchemaBuilderTests
    {
        [TestMethod]
        public void Metadata_IncludesName()
        {
            var parameter = new ParameterSchema
            {
                Name = "TestParameter",
            };

            var builder = new ParamaterSchemaBuilder();
            var metadata = builder.BuildKernelParameterMetadata( parameter );

            Assert.AreEqual( "TestParameter", metadata.Name );
        }

        [TestMethod]
        public void Metadata_IncludesInstructions()
        {
            var parameter = new ParameterSchema
            {
                Name = "TestParameter",
                Instructions = "This is a test parameter.",
            };

            var builder = new ParamaterSchemaBuilder();
            var metadata = builder.BuildKernelParameterMetadata( parameter );

            Assert.AreEqual( "This is a test parameter.", metadata.Description );
        }

        [TestMethod]
        public void Metadata_IncludeDefaultValue()
        {
            var parameter = new ParameterSchema
            {
                Name = "TestParameter",
                DefaultValue = "Test default value.",
            };

            var builder = new ParamaterSchemaBuilder();
            var metadata = builder.BuildKernelParameterMetadata( parameter );

            Assert.AreEqual( "Test default value.", metadata.DefaultValue );
        }

        [TestMethod]
        public void Metadata_IncludesIsRequired()
        {
            var requiredParameter = new ParameterSchema
            {
                IsRequired = true,
                Name = "TestParameter",
            };

            var nonRequiredParameter = new ParameterSchema
            {
                IsRequired = false,
                Name = "TestParameter",
            };

            var builder = new ParamaterSchemaBuilder();
            var requiredMetadata = builder.BuildKernelParameterMetadata( requiredParameter );
            var nonRequiredMetadata = builder.BuildKernelParameterMetadata( nonRequiredParameter );

            Assert.IsTrue( requiredMetadata.IsRequired );
            Assert.IsFalse( nonRequiredMetadata.IsRequired );
        }

        [TestMethod]
        public void Schema_WithStringDataType_ReturnsStringType()
        {
            var parameter = new ParameterSchema
            {
                DataType = ParameterSchemaDataType.String,
                Name = "TestParameter",
            };

            var builder = new ParamaterSchemaBuilder();
            var schema = builder.BuildKernelParameterMetadata( parameter ).Schema;

            Assert.AreEqual( "string", schema.RootElement.GetProperty( "type" ).GetString() );
        }

        [TestMethod]
        public void Schema_WithNumberDataType_ReturnsNumberType()
        {
            var parameter = new ParameterSchema
            {
                DataType = ParameterSchemaDataType.Number,
                Name = "TestParameter",
            };

            var builder = new ParamaterSchemaBuilder();
            var schema = builder.BuildKernelParameterMetadata( parameter ).Schema;

            Assert.AreEqual( "number", schema.RootElement.GetProperty( "type" ).GetString() );
        }

        [TestMethod]
        public void Schema_WithBooleanDataType_ReturnsBooleanType()
        {
            var parameter = new ParameterSchema
            {
                DataType = ParameterSchemaDataType.Boolean,
                Name = "TestParameter",
            };

            var builder = new ParamaterSchemaBuilder();
            var schema = builder.BuildKernelParameterMetadata( parameter ).Schema;

            Assert.AreEqual( "boolean", schema.RootElement.GetProperty( "type" ).GetString() );
        }

        [TestMethod]
        public void Schema_WithDateDataType_ReturnsStringType()
        {
            var parameter = new ParameterSchema
            {
                DataType = ParameterSchemaDataType.Date,
                Name = "TestParameter",
            };

            var builder = new ParamaterSchemaBuilder();
            var schema = builder.BuildKernelParameterMetadata( parameter ).Schema;

            Assert.AreEqual( "string", schema.RootElement.GetProperty( "type" ).GetString() );
        }

        [TestMethod]
        public void Schema_WithDateTimeDataType_ReturnsStringType()
        {
            var parameter = new ParameterSchema
            {
                DataType = ParameterSchemaDataType.DateTime,
                Name = "TestParameter",
            };

            var builder = new ParamaterSchemaBuilder();
            var schema = builder.BuildKernelParameterMetadata( parameter ).Schema;

            Assert.AreEqual( "string", schema.RootElement.GetProperty( "type" ).GetString() );
        }

        [TestMethod]
        public void Schema_WithDateDataType_ReturnsCorrectFormat()
        {
            var parameter = new ParameterSchema
            {
                DataType = ParameterSchemaDataType.Date,
                Name = "TestParameter",
            };

            var builder = new ParamaterSchemaBuilder();
            var schema = builder.BuildKernelParameterMetadata( parameter ).Schema;

            Assert.AreEqual( "date", schema.RootElement.GetProperty( "format" ).GetString() );
        }

        [TestMethod]
        public void Schema_WithDateTimeDataType_ReturnsCorrectFormat()
        {
            var parameter = new ParameterSchema
            {
                DataType = ParameterSchemaDataType.DateTime,
                Name = "TestParameter",
            };

            var builder = new ParamaterSchemaBuilder();
            var schema = builder.BuildKernelParameterMetadata( parameter ).Schema;

            Assert.AreEqual( "date-time", schema.RootElement.GetProperty( "format" ).GetString() );
        }

        [TestMethod]
        public void Schema_WithStringDataType_ExcludesFormat()
        {
            var parameter = new ParameterSchema
            {
                DataType = ParameterSchemaDataType.String,
                Name = "TestParameter",
            };

            var builder = new ParamaterSchemaBuilder();
            var schema = builder.BuildKernelParameterMetadata( parameter ).Schema;

            Assert.IsFalse( schema.RootElement.TryGetProperty( "format", out _ ) );
        }

        [TestMethod]
        public void Schema_WithStringAllowedValues_IncludesEnumList()
        {
            var parameter = new ParameterSchema
            {
                AllowedValues = ["Value1", "Value2", "Value3"],
                DataType = ParameterSchemaDataType.String,
                Name = "TestParameter",
            };

            var builder = new ParamaterSchemaBuilder();
            var schema = builder.BuildKernelParameterMetadata( parameter ).Schema;

            Assert.IsTrue( schema.RootElement.TryGetProperty( "enum", out var enumProperty ) );
            Assert.IsTrue( enumProperty.ValueKind == JsonValueKind.Array );
            Assert.AreEqual( 3, enumProperty.GetArrayLength() );
            Assert.AreEqual( "Value1", enumProperty[0].GetString() );
            Assert.AreEqual( "Value2", enumProperty[1].GetString() );
            Assert.AreEqual( "Value3", enumProperty[2].GetString() );
        }

        [TestMethod]
        public void Schema_WithStringCollectionAllowedValues_IncludesEnumList()
        {
            var parameter = new ParameterSchema
            {
                AllowedValues = ["Value1", "Value2", "Value3"],
                DataType = ParameterSchemaDataType.String,
                IsCollection = true,
                Name = "TestParameter",
            };

            var builder = new ParamaterSchemaBuilder();
            var schema = builder.BuildKernelParameterMetadata( parameter ).Schema;

            Assert.IsTrue( schema.RootElement.TryGetProperty( "items", out var itemsProperty ) );
            Assert.IsTrue( itemsProperty.TryGetProperty( "enum", out var enumProperty ) );
            Assert.IsTrue( enumProperty.ValueKind == JsonValueKind.Array );
            Assert.AreEqual( 3, enumProperty.GetArrayLength() );
            Assert.AreEqual( "Value1", enumProperty[0].GetString() );
            Assert.AreEqual( "Value2", enumProperty[1].GetString() );
            Assert.AreEqual( "Value3", enumProperty[2].GetString() );
        }

        [TestMethod]
        public void Schema_WithStringCollectionAllowedValues_IncludesCollectionInstructions()
        {
            var parameter = new ParameterSchema
            {
                DataType = ParameterSchemaDataType.String,
                Instructions = "This is a collection parameter.",
                IsCollection = true,
                Name = "TestParameter",
            };

            var builder = new ParamaterSchemaBuilder();
            var schema = builder.BuildKernelParameterMetadata( parameter ).Schema;

            Assert.AreEqual( "This is a collection parameter." + ParamaterSchemaBuilder.CollectionInstructions, schema.RootElement.GetProperty( "description" ).GetString() );
        }
    }
}
