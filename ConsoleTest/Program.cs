using Microsoft.Extensions.Configuration;

namespace MiJenner
{
    public class LoggingSettings
    {
        public LogLevel LogLevel { get; set; }
    }

    public class LogLevel
    {
        public string Default { get; set; }
        public string Hmm { get; set; }
    }

    public class ApplicationOptions
    {
        public string MyString { get; set; }
        public int MyNumber {  get; set; }
        public bool MyBool { get; set; }
        public double MyDouble { get; set; }
        public Guid MyGuid { get; set; }
    }

    public class Program
    {
        static void Main()
        {
            // Initiate JsonSections: 
            var jsonFilePath = "appsettings.json";
            var jsonSections = new JsonSections(jsonFilePath);

            // Add Logging, hardcoded string in AddSection(): 
            var loggingSettings = new LoggingSettings
            {
                LogLevel = new LogLevel
                {
                    Default = "Information",
                    Hmm = "Warning"
                }
            };
            jsonSections.AddSection("Logging", loggingSettings);

            // Add AllowedHosts, hardcoded string in AddSection(): 
            var allowedHostsValue = "*";
            jsonSections.AddSection("AllowedHosts", allowedHostsValue);

            // Add ApplicationOptions, but this object will also be used for readback: 
            var applicationOptions = new ApplicationOptions
            {
                MyString = "Current value is 1",
                MyNumber = 314,
                MyBool = true,
                MyDouble = 3.1425,
                MyGuid = Guid.NewGuid()
            };  
            jsonSections.AddSection("ApplicationOptions", applicationOptions);

            // Reading back all settings
            var config = new ConfigurationBuilder()
                .AddJsonFile(jsonFilePath)
                .Build();

            // Accessing a nested setting, here Default under LogLevel under Logging: 
            var logLevelDefault = config["Logging:LogLevel:Default"];
            Console.WriteLine($"LogLevel.Default: {logLevelDefault}");

            // Accessing a nested setting, LogLevel: 
            var logLevelHmm = config["Logging:LogLevel:Hmm"];
            Console.WriteLine($"LogLevel.Hmm: {logLevelHmm}");

            // Accessing a simple setting, not nested, here AllowedHosts. 
            var allowedHosts = config["AllowedHosts"];
            Console.WriteLine($"AllowedHosts: {allowedHosts}");

            // Read back into an object for easier handling of a group of data: 
            var appOptions = config.GetSection("ApplicationOptions").Get<ApplicationOptions>();
            Console.WriteLine($"ApplicationOptions.ExampleValue: {appOptions.MyString}");
        }
    }
}
