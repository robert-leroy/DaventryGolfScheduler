namespace GolfScheduler.Api.Configuration;

public class EmailSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "noreply@golfscheduler.com";
}
