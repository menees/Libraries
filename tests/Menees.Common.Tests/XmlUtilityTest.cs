using System;
using System.Xml.Linq;
using Menees;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Xml.Schema;
using System.Collections.Generic;

namespace Menees.Common.Tests
{
	[TestClass()]
	public class XmlUtilityTest
	{
		private const string xmlText = "<element num='1' flag='true' text='Testing' fileMode='OpenOrCreate' empty=''><inner test='1' /></element>";
		private const string schemaText = "<xs:schema attributeFormDefault='unqualified' elementFormDefault='qualified' xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
			"<xs:element name='element'><xs:complexType><xs:sequence><xs:element name='inner'><xs:complexType>" +
			"<xs:attribute name='test' type='xs:unsignedByte' use='required' /></xs:complexType></xs:element></xs:sequence>" +
			"<xs:attribute name='num' type='xs:unsignedByte' use='required' /><xs:attribute name='flag' type='xs:boolean' use='required' />" +
			"<xs:attribute name='text' type='xs:string' use='required' /><xs:attribute name='fileMode' type='xs:string' use='required' />" +
			"<xs:attribute name='empty' type='xs:string' use='required' /></xs:complexType></xs:element></xs:schema>";

		private static readonly XElement xmlElement = XElement.Parse(xmlText);
		private static readonly XElement schemaElement = XElement.Parse(schemaText);

		private static XElement GetXElement() => xmlElement;

		[TestMethod()]
		public void GetAttributeValueInt32Test()
		{
			XElement element = GetXElement();
			int actual = element.GetAttributeValue("num", 0);
			actual.ShouldBe(1);
			actual = element.GetAttributeValue("missing", 2);
			actual.ShouldBe(2);
		}

		[TestMethod()]
		public void GetAttributeValueBooleanTest()
		{
			XElement element = GetXElement();
			bool actual = element.GetAttributeValue("flag", false);
			Assert.IsTrue(actual);
			actual = element.GetAttributeValue("missing", true);
			Assert.IsTrue(actual);
		}

		[TestMethod()]
		public void GetAttributeValueEnumTest()
		{
			XElement element = GetXElement();
			FileMode actual = element.GetAttributeValue("fileMode", FileMode.CreateNew);
			actual.ShouldBe(FileMode.OpenOrCreate);
			actual = element.GetAttributeValue("missing", FileMode.CreateNew);
			actual.ShouldBe(FileMode.CreateNew);
		}

		[TestMethod()]
		public void GetAttributeValueStringRequiredTest()
		{
			XElement element = GetXElement();
			string actual = element.GetAttributeValue("text");
			actual.ShouldBe("Testing");
		}

		[TestMethod()]
		public void GetAttributeValueStringTest()
		{
			XElement element = GetXElement();
			string actual = element.GetAttributeValue("text", "");
			actual.ShouldBe("Testing");
			actual = element.GetAttributeValue("missing", "Default");
			actual.ShouldBe("Default");
		}

		[TestMethod()]
		public void GetAttributeValueStringNTest()
		{
			XElement element = GetXElement();
			string? actual = element.GetAttributeValueN("empty", "Default", true);
			actual.ShouldBe("Default");
			actual = element.GetAttributeValueN("empty", "Default", false);
			actual.ShouldBe("");
		}

		[TestMethod]
		public void CreateSchemaTest()
		{
			XmlSchemaSet schema = XmlUtility.CreateSchemaSet([schemaElement]);
			schema.ShouldNotBeNull();

			// Switch use='required' to use='unknown', which is not a supported XSD attribute value.
			string badSchemaText = schemaText.Replace("<xs:attribute name='empty' type='xs:string' use='required' />", "<xs:attribute name='empty' type='xs:string' use='unknown' />");
			XElement badSchemaElement = XElement.Parse(badSchemaText, LoadOptions.SetLineInfo);

			List<ValidationEventArgs> errors = [];
			schema = XmlUtility.CreateSchemaSet([badSchemaElement], errors);
			schema.ShouldNotBeNull();
			Assert.HasCount(1, errors);
		}

		[TestMethod]
		public void ValidateTest()
		{
			XmlSchemaSet schema = XmlUtility.CreateSchemaSet([schemaElement]);
			var errors = xmlElement.Validate(schema);
			Assert.IsEmpty(errors);

			schema = CreateValidationFailureSchema();
			errors = xmlElement.Validate(schema);
			Assert.HasCount(1, errors);
		}

		private static XmlSchemaSet CreateValidationFailureSchema()
		{
			// Remove support for an attribute that's used by the sample XML, which should cause a validation error.
			string failingSchemaText = schemaText.Replace("<xs:attribute name='empty' type='xs:string' use='required' />", "");
			XElement failingSchemaElement = XElement.Parse(failingSchemaText, LoadOptions.SetLineInfo);
			XmlSchemaSet schema = XmlUtility.CreateSchemaSet([failingSchemaElement]);
			return schema;
		}

		[TestMethod]
		public void RequireValidationTest()
		{
			XmlSchemaSet schema = XmlUtility.CreateSchemaSet([schemaElement]);
			xmlElement.RequireValidation(schema);

			try
			{
				schema = CreateValidationFailureSchema();
				xmlElement.RequireValidation(schema);
				Assert.Fail("We should never get past RequireValidation");
			}
			catch (XmlSchemaValidationException ex)
			{
#pragma warning disable MSTEST0058 // Do not use asserts in catch blocks. This tests the exception we raised.
				Assert.IsFalse(string.IsNullOrEmpty(ex.Message));
#pragma warning restore MSTEST0058 // Do not use asserts in catch blocks
			}
		}
	}
}
