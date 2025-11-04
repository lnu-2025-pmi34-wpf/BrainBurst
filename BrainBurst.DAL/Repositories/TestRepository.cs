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
    public class TestRepository : ITestRepository
    {
        private readonly ApplicationDbContext _context;

        public TestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Test> CreateFromFlashcardsAsync(int creatorId, IEnumerable<int> flashcardIds, CancellationToken ct)
        {
            // Створення нової сутності Test.
            // Важливо: фактичний набір питань (flashcardIds) зберігається та використовується 
            // на рівні бізнес-логіки (TestService), а репозиторій лише створює запис про тест.
            var test = new Test
            {
                CreatorId = creatorId,
            };

            _context.Tests.Add(test);
            await _context.SaveChangesAsync(ct);
            return test;
        }

        public async Task<Test?> GetAsync(int id, CancellationToken ct)
        {
            // Отримання сутності Test за її ID.
            return await _context.Tests
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(t => t.TestId == id, ct);
        }
    }
}