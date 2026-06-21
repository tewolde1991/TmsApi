// using TmsApi.Models;

// namespace TmsApi.Services;

// public interface IGradeCalculator
// {
//     decimal CalculateGpa(IEnumerable<Grade> grades);
// }

// public class GradeCalculator : IGradeCalculator
// {
//     public decimal CalculateGpa(IEnumerable<Grade> grades)
//     {
//         if (!grades.Any()) return 0m;
//         return grades.Average(g => g.Value);
//     }
// }