using System.ComponentModel.DataAnnotations;

namespace VNNSIS.ViewModels {
    public class InformationDataViewModel {

        public string job_order_no { get; set; }
        public string lot_no { get; set; }
        public string finished_goods_code { get; set; }
        [Key]
        public string operation_code { get; set; }
        public string operation_name { get; set; }
        public int operation_sequence { get; set; }
        public string line_no { get; set; }
        public string material_mark1 { get; set; }
        public int rotation { get; set; }
        public double curing_qty { get; set; }
        public int cavity_qty { get; set; }
        public string order_date { get; set; }
        public string curing_date { get; set; }
    }
}