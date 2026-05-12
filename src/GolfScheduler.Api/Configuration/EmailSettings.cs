namespace GolfScheduler.Api.Configuration;

public class EmailSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "donotreply@10thhole.com";
}
