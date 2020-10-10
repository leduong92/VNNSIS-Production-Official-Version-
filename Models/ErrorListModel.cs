using System.ComponentModel.DataAnnotations;

namespace VNNSIS.Models
{
    public class ErrorListModel
    {
        [Key]
        public string error_id { get; set; }
        public string error_name { get; set; }
        public string error_name_jp { get; set; }
        public string error_name_en { get; set; }
    }
}