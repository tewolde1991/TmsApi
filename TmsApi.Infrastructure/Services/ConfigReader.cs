namespace TmsApi.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

public interface IConfigReader
{
    string GetConnectionString();
}

public class ConfigReader(IConfiguration config) : IConfigReader
{
    private readonly string _connectionString = config.GetConnectionString("TMS") ?? "DefaultConnection";
    public string GetConnectionString() => _connectionString;
}