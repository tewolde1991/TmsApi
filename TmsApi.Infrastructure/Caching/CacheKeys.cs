namespace TmsApi.Infrastructure.Caching;

public static class CacheKeys
{
    private const string SchemaVersion = "V2";
    public static string Course(string code) => $"{SchemaVersion}:course:{code.ToLower()}";
    public static string Course(int id) => $"{SchemaVersion}:course:{id}";
    public static string CoursesAll => $"{SchemaVersion}:courses:all";
    public const string CoursesTag = "courses";
}