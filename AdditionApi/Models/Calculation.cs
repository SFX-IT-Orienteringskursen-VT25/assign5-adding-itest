using System.ComponentModel.DataAnnotations;

namespace AdditionApi.Models
{
    public class Calculation
    {
        [Key] 
        public int Id { get; set; }

        public int Operand1 { get; set; }
        public int Operand2 { get; set; }
        public int Result { get; set; }
        public string Operation { get; set; } = "add";
    }
}