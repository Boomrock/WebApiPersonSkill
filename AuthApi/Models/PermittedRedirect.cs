using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AuthApi.Models
{
    public class PermittedRedirect
    {
        [Key]
        [Column(Order = 1)]
        public Client Client { get; set; } = null!;
        [Key]
        [Column(Order = 2)]
        public string RedirectUrl { get; set; } = null!;
    }
}
