namespace ApplicationWatcher.Service.SystemInfo.Models
{
    public class ProcessInfo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public long HandleCount { get; set; }

        public long ThreadCount { get; set; }

        public double Memory { get; set; }

        public long CpuTime { get; set; }

        public double Cpu { get; set; }

        public override string ToString()
        {
            return $"Name = {Name}, Memory = {Memory}, Cpu = {Cpu}";
        }
    }
}
