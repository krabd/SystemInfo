namespace ApplicationWatcher.Service.SystemInfo.Models
{
    public class UserInfo
    {
        public string Description { get; set; }

        public bool IsDisabled { get; set; }

        public string Domain { get; set; }

        public string FullName { get; set; }

        public string GroupName { get; set; }

        public bool IsLocalAccount { get; set; }

        public bool IsLockout { get; set; }

        public string Name { get; set; }

        public string Sid { get; set; }

        public SidType SidType { get; set; }

        public string Status { get; set; }
    }

    public enum SidType
    {
        User = 1,
        Group = 2,
        Domain = 3,
        Alias = 4,
        WellKnownGroup = 5,
        DeletedAccount = 6,
        Invalid = 7,
        Unknown = 8,
        Computer = 9
    }
}
