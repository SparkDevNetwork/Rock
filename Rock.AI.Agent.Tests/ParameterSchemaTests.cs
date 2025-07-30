using System.Text.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Enums.Core.AI.Agent;

namespace Rock.AI.Agent.Tests
{
    [TestClass]
    public class ParameterSchemaTests
    {
        [TestMethod]
        public void Metadata_IncludesName()
        {
            var parameter = new ParameterSchema
            {
                Name = "TestParameter",
            };

            var metadata = parameter.GetKernelParameterMetadata();

            Assert.AreEqual( "TestParameter", metadata.Name );
        }

        [TestMethod]
        public void Metadata_IncludesUsageHint()
        {
            var parameter = new ParameterSchema
            {
                Name = "TestParameter",
                UsageHint = "This is a test parameter.",
            };

            var metadata = parameter.GetKernelParameterMetadata();

            Assert.AreEqual( "This is a test parameter.", metadata.Description );
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

            var requiredMetadata = requiredParameter.GetKernelParameterMetadata();
            var nonRequiredMetadata = nonRequiredParameter.GetKernelParameterMetadata();

            Assert.IsTrue( requiredMetadata.IsRequired );
            Assert.IsFalse( nonRequiredMetadata.IsRequired );
        }

        [TestMethod]
        public void Metadata_WithMultipleCalls_ReturnsSameObject()
        {
            var parameter = new ParameterSchema
            {
                DataType = ParameterSchemaDataType.String,
                Name = "TestParameter",
                UsageHint = "This is a test parameter."
            };

            var metadata1 = parameter.GetKernelParameterMetadata();
            var metadata2 = parameter.GetKernelParameterMetadata();

            Assert.AreSame( metadata1, metadata2, "GetKernelParameterMetadata should return the same object on multiple calls." );
        }

        [TestMethod]
        public void Schema_WithStringDataType_ReturnsStringType()
        {
            var parameter = new ParameterSchema
            {
                DataType = ParameterSchemaDataType.String,
                Name = "TestParameter",
            };

            var schema = parameter.GetKernelParameterMetadata().Schema;

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

            var schema = parameter.GetKernelParameterMetadata().Schema;

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

            var schema = parameter.GetKernelParameterMetadata().Schema;

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

            var schema = parameter.GetKernelParameterMetadata().Schema;

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

            var schema = parameter.GetKernelParameterMetadata().Schema;

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

            var schema = parameter.GetKernelParameterMetadata().Schema;

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

            var schema = parameter.GetKernelParameterMetadata().Schema;

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

            var schema = parameter.GetKernelParameterMetadata().Schema;

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

            var schema = parameter.GetKernelParameterMetadata().Schema;

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

            var schema = parameter.GetKernelParameterMetadata().Schema;

            Assert.IsTrue( schema.RootElement.TryGetProperty( "items", out var itemsProperty ) );
            Assert.IsTrue( itemsProperty.TryGetProperty( "enum", out var enumProperty ) );
            Assert.IsTrue( enumProperty.ValueKind == JsonValueKind.Array );
            Assert.AreEqual( 3, enumProperty.GetArrayLength() );
            Assert.AreEqual( "Value1", enumProperty[0].GetString() );
            Assert.AreEqual( "Value2", enumProperty[1].GetString() );
            Assert.AreEqual( "Value3", enumProperty[2].GetString() );
        }

        [TestMethod]
        public void Schema_WithStringCollectionAllowedValues_IncludesCollectionUsageHint()
        {
            var parameter = new ParameterSchema
            {
                DataType = ParameterSchemaDataType.String,
                UsageHint = "This is a collection parameter.",
                IsCollection = true,
                Name = "TestParameter",
            };

            var schema = parameter.GetKernelParameterMetadata().Schema;

            Assert.AreEqual( "This is a collection parameter." + ParameterSchema.CollectionUsageHint, schema.RootElement.GetProperty( "description" ).GetString() );
        }
    }
}
