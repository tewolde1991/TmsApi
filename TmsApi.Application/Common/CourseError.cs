

namespace TmsApi.Application.Common;
public sealed  record CourseError(string Code, string Message){
    public static CourseError DuplicateCode
    (string code) => new ("duplicate_code", $"Course code'{code}'is already in use");
}