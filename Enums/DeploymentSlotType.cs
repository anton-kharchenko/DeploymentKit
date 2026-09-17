using System.ComponentModel;

namespace DeploymentKit.Enums
{
    /// <summary>
    /// Represents the deployment slot names in a green-blue deployment strategy.
    /// </summary>
    public enum DeploymentSlotType
    {
        /// <summary>
        /// The green deployment slot.
        /// </summary>
        [Description("green")]
        Green,

        /// <summary>
        /// The blue deployment slot.
        /// </summary>
        [Description("blue")]
        Blue
    }
}

