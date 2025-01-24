using System.ComponentModel.DataAnnotations;

namespace MiniPCoded.Models.ViewModels
{
    public class TransferViewModel
    {

        public string Id { get; set; }
        public string TargetId { get; set; }

        [Required]
        [Range(0.01, 1000000000, ErrorMessage = "Amount is High.")]
        public float Amount { get; set; }

    }
}
