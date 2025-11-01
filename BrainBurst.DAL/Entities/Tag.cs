using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BrainBurst.DAL.Entities
{
    public class Tag
    {
        public int TagId { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = null!;

        public int? CreatorId { get; set; }
        public User? Creator { get; set; }
    }
}