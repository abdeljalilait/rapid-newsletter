namespace NewsletterPlatform.Application.Common;

public class CampaignDispatchWorkerOptions
{
    public const string SectionName = "CampaignDispatchWorker";

    public bool Enabled { get; set; } = true;

    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(10);

    public int BatchSize { get; set; } = 50;

    public int MaxRetries { get; set; } = 3;
}
