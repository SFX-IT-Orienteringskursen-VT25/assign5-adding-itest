using System.ComponentModel.DataAnnotations;

namespace AdditionApi.Models
{
    public class StorageItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }
}
