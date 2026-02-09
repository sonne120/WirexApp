using System.Collections.Generic;

namespace WirexApp.Infrastructure.CDC
{
    /// <summary>
    /// Configuration for Change Data Capture (CDC)
    /// </summary>
    public class CDCConfiguration
    {
        public bool Enabled { get; set; } = true;

        public int BatchSize { get; set; } = 100;

        public int PublishIntervalMs { get; set; } = 1000;

        public List<string> EnabledEntities { get; set; } = new List<string>
        {
            "Payment",
            "UserAccount",
            "BonusAccount",
            "User"
        };

        public Dictionary<string, string> TopicMappings { get; set; } = new Dictionary<string, string>
        {
            { "Payment", "cdc.payment" },
            { "UserAccount", "cdc.useraccount" },
            { "BonusAccount", "cdc.bonusaccount" },
            { "User", "cdc.user" }
        };
    }
}
