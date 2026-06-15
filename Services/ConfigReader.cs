namespace TmsApi.Services;

public interface IConfigReader
{
    string GetConnectionString();
}

public class ConfigReader : IConfigReader
{
    private readonly string _connectionString;
    public ConfigReader(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("TMS") ?? "DefaultConnection";
    }
    public string GetConnectionString() => _connectionString;
}