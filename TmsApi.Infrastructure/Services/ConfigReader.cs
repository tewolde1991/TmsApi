using Microsoft.Extensions.Configuration;

namespace TmsApi.Infrastructure.Services;

public interface IConfigReader
{
    string GetConnectionString();
}

public class ConfigReader(IConfiguration config) : IConfigReader
{
    private readonly string _connectionString = config.GetConnectionString("TMS") ?? "DefaultConnection";
    public string GetConnectionString() => _connectionString;
}