using BrainBurst.DAL.Abstractions;
using BrainBurst.DAL.Data;
using BrainBurst.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrainBurst.DAL.Repositories
{
    public class TestResultRepository : ITestResultRepository
    {
        private readonly ApplicationDbContext _context;

        public TestResultRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TestResult> AddAsync(TestResult tr, IEnumerable<QuestionResult> qr, CancellationToken ct)
        {
            // 1. Додаємо TestResult
            _context.TestResults.Add(tr);
            
            // 2. Додаємо QuestionResults та прив'язуємо їх до TestResult
            // (EF Core автоматично встановить TestResultId при збереженні)
            foreach (var q in qr)
            {
                tr.QuestionResults.Add(q); 
            }

            await _context.SaveChangesAsync(ct);
            return tr;
        }

        public async Task<IReadOnlyList<TestResult>> GetByUserAsync(int userId, CancellationToken ct)
        {
            // Отримуємо всі результати тестів для конкретного користувача,
            // включаючи пов'язані результати питань, для відображення архіву.
            return await _context.TestResults
                                 .Where(tr => tr.UserId == userId)
                                 // Завантажуємо QuestionResults, оскільки це важливо для TestService
                                 .Include(tr => tr.QuestionResults) 
                                 .OrderByDescending(tr => tr.TestDate)
                                 .AsNoTracking()
                                 .ToListAsync(ct);
        }
    }
}