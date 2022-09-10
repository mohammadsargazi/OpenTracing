namespace Common.Configurations
{
    public class JaegerConfig
    {
        public bool IsEnabled { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public double SamplingRate { get; set; }
    }
}
