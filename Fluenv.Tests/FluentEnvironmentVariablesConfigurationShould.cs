namespace Fluenv.Tests;

[TestClass]
public class FluentEnvironmentVariablesConfigurationShould
{
    IConfiguration Configuration =>
        new ConfigurationBuilder()
            .AddFluentEnvironmentVariables("FLUENV_TEST_")
            .Build();
    
    [TestMethod]
    public void ReadSectionVariableInMicrosoftFormat()
    {
        Environment.SetEnvironmentVariable("FLUENV_TEST_MicrosoftFormat__Variable", "ms");

        Console.WriteLine(this.Configuration["MicrosoftFormat:Variable"]);
        
        var section = this.Configuration.GetSection("MicrosoftFormat");
        
        section["Variable"].Should().Be("ms");
    }

    [TestMethod]
    public void ReadVariablesInUnderscoreSeparatedSection()
    {
        Environment.SetEnvironmentVariable("FLUENV_TEST_SECTION_A_VARIABLE_ONE", "ao");
        Environment.SetEnvironmentVariable("FLUENV_TEST_SECTION_A_VARIABLE_TWO", "at");

        var sectionA = this.Configuration.GetSection("SectionA");
        
        sectionA["VariableOne"].Should().Be("ao");
        sectionA["VariableTwo"].Should().Be("at");
    }

    [TestMethod]
    public void ReadVariablesInDoubleUnderscoreSeparatedSection()
    {
        Environment.SetEnvironmentVariable("FLUENV_TEST_SECTION_B__VARIABLE_ONE", "bo");
        Environment.SetEnvironmentVariable("FLUENV_TEST_SECTION_B__VARIABLE_TWO", "bt");
        
        var sectionB = this.Configuration.GetSection("SectionB");
        
        sectionB["VariableOne"].Should().Be("bo");
        sectionB["VariableTwo"].Should().Be("bt");
    }
    
    [TestMethod]
    public void ProvideBindableConfigurationSection()
    {
        Environment.SetEnvironmentVariable("FLUENV_TEST_SECTION_C_VARIABLE_ONE", "bo");
        Environment.SetEnvironmentVariable("FLUENV_TEST_SECTION_C_VARIABLE_TWO", "bt");
        
        var sectionB = this.Configuration.GetSection("SectionC").Get<ExampleSection>() ?? throw new ("Unable to bind");
        
        sectionB.VariableOne.Should().Be("bo");
        sectionB.VariableTwo.Should().Be("bt");
    }

    public class ExampleSection
    {
        public string VariableOne { get; set; } = null!;
        public string VariableTwo { get; set; } = null!;
    }
}