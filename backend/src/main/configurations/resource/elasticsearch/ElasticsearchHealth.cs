namespace backend.main.configurations.resource.elasticsearch
{
    public sealed class ElasticsearchHealth
    {
        public bool IsAvailable { get; internal set; }
        public Exception? Failure { get; internal set; }
    }
}
