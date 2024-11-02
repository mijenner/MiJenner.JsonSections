using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MiJenner.Tests
{
    public class ApplicationOptions
    {
        public string MyString { get; set; }
        public int MyNumber { get; set; }
        public bool MyBool { get; set; }
        public double MyDouble { get; set; }
        public Guid MyGuid { get; set; }
    }

    public class JsonSectionsTests : IDisposable
    {
        private readonly string _jsonFilePath;

        public JsonSectionsTests()
        {
            _jsonFilePath = "testAppSettings.json"; 
            if (File.Exists(_jsonFilePath))
            {
                File.Delete(_jsonFilePath);
            }
        }

        public void Dispose()
        {
            if (File.Exists(_jsonFilePath))
            {
                File.Delete(_jsonFilePath);
            }
        }

        [Fact]
        public void TestWriteAndReadString()
        {
            var jsonSections = new JsonSections(_jsonFilePath);
            var testValue = "TestString";
            jsonSections.AddSection("TestSection", testValue);

            var config = new ConfigurationBuilder()
                .AddJsonFile(_jsonFilePath)
                .Build();

            var value = config["TestSection"];
            Assert.Equal(testValue, value); 
        }

        [Fact]
        public void TestWriteAndReadMultipleDataTypes()
        {
            var jsonSections = new JsonSections(_jsonFilePath);
            var applicationOptions = new ApplicationOptions
            {
                MyString = "TestString",
                MyNumber = 123,
                MyBool = true,
                MyDouble = 456.78,
                MyGuid = Guid.NewGuid() // Use a new GUID for each test
            };
            jsonSections.AddSection("ApplicationOptions", applicationOptions);

            var config = new ConfigurationBuilder()
                .AddJsonFile(_jsonFilePath)
                .Build();

            var appOptions = config.GetSection("ApplicationOptions").Get<ApplicationOptions>();
            Assert.Equal(applicationOptions.MyString, appOptions.MyString);
            Assert.Equal(applicationOptions.MyNumber, appOptions.MyNumber);
            Assert.Equal(applicationOptions.MyBool, appOptions.MyBool);
            Assert.Equal(applicationOptions.MyDouble, appOptions.MyDouble);
            Assert.Equal(applicationOptions.MyGuid, appOptions.MyGuid); // Ensure GUIDs match
        }

        [Fact]
        public void TestInvalidGuidHandling()
        {
            // Arrange: Create a JSON file with an invalid GUID value
            var invalidGuidJson = """
    {
        "ApplicationOptions": {
            "MyString": "TestString",
            "MyNumber": 123,
            "MyBool": true,
            "MyDouble": 456.78,
            "MyGuid": "InvalidGuidValue" // This is an invalid GUID
        }
    }
    """;

            File.WriteAllText(_jsonFilePath, invalidGuidJson);

            // Act & Assert: Attempt to read the application options and check for an exception
            var config = new ConfigurationBuilder()
                .AddJsonFile(_jsonFilePath)
                .Build();

            // Assert.Throws<Exception>(() => config.GetSection("ApplicationOptions").Get<ApplicationOptions>());
            var exception = Assert.Throws<InvalidOperationException>(() => config.GetSection("ApplicationOptions").Get<ApplicationOptions>());
            Assert.Contains("MyGuid", exception.Message);
        }

        [Fact]
        public void TestInvalidIntHandling()
        {
            // Arrange: Create a JSON file with an invalid GUID value
            var invalidGuidJson = """
    {
        "ApplicationOptions": {
            "MyString": "TestString",
            "MyNumber": 123.45,
            "MyBool": true,
            "MyDouble": 456.78,
            "MyGuid": "42df5101-b069-4dde-a9bf-8130fdb68f25" 
        }
    }
    """;

            File.WriteAllText(_jsonFilePath, invalidGuidJson);

            // Act & Assert: Attempt to read the application options and check for an exception
            var config = new ConfigurationBuilder()
                .AddJsonFile(_jsonFilePath)
                .Build();

            // Assert.Throws<Exception>(() => config.GetSection("ApplicationOptions").Get<ApplicationOptions>());
            var exception = Assert.Throws<InvalidOperationException>(() => config.GetSection("ApplicationOptions").Get<ApplicationOptions>());
            Assert.Contains("MyNumber", exception.Message);

        }


        [Fact]
        public void TestAddingDuplicateSections()
        {
            var jsonSections = new JsonSections(_jsonFilePath);
            var testValue = "UniqueValue";

            // Add the section twice
            jsonSections.AddSection("DuplicateSection", testValue);
            jsonSections.AddSection("DuplicateSection", testValue); // This should not create a duplicate

            var config = new ConfigurationBuilder()
                .AddJsonFile(_jsonFilePath)
                .Build();

            var sectionCount = config.AsEnumerable().Count(kvp => kvp.Key.StartsWith("DuplicateSection"));
            Assert.Equal(1, sectionCount); // Assert that there's only one section
        }

        [Fact]
        public void TestAddingDifferentDataTypes()
        {
            var jsonSections = new JsonSections(_jsonFilePath);

            // Define multiple types
            jsonSections.AddSection("IntValue", 10);
            jsonSections.AddSection("DoubleValue", 10.5);
            jsonSections.AddSection("BoolValue", true);
            jsonSections.AddSection("StringValue", "Hello");
            jsonSections.AddSection("GuidValue", Guid.NewGuid());

            var config = new ConfigurationBuilder()
                .AddJsonFile(_jsonFilePath)
                .Build();

            Assert.Equal("10", config["IntValue"]); // Access integer
            Assert.Equal("10.5", config["DoubleValue"]); // Access double
            Assert.Equal("True", config["BoolValue"]); // Access boolean
            Assert.Equal("Hello", config["StringValue"]); // Access string

            // Access Guid (make sure to keep it consistent in your setup)
            var guidString = config["GuidValue"];
            Assert.NotNull(guidString); // Check if it reads a value
        }
    }
}
