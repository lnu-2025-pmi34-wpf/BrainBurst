using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BrainBurst.DAL.Entities
{
    public class Test
    {
        public int TestId { get; set; }

        public int CreatorId { get; set; }
        public User Creator { get; set; } = null!;

        public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
    }
}