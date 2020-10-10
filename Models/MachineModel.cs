using System.ComponentModel.DataAnnotations;

namespace VNNSIS.Models
{
    public class MachineModel
    {
        public string line_no { get; set; }
        [Key]
        public string press_no { get; set; }
        public string plc_m { get; set; }
        public string plc_m1 { get; set; }
        public string plc_m2 { get; set; }
        public string ip { get; set; }
        public int status { get; set; }
        public int mold_type { get; set; }
        public int trim_type { get; set; }
        public string machine_status { get; set; }
    }
    public class DataMachine{
        public string LineNo { get; set; }
        public string PressNo { get; set; }
        public string value { get; set; }
        public string users { get; set; }
        public string errType { get; set; }
    }
}