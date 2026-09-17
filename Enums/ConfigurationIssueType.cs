using System.ComponentModel;

namespace DeploymentKit.Enums
{
    public enum ConfigurationIssueType
    {
        [Description("Unknown")]
        Unknown,
        
        [Description("InvalidCiphertext")]
        InvalidCiphertext,
        
        [Description("MissingPassphrase")]
        MissingPassphrase,
        
        [Description("InvalidPassphrase")]
        InvalidPassphrase,
        
        [Description("MissingConfig")]
        MissingConfig,
        
        [Description("CorruptedState")]
        CorruptedState
    }
}
