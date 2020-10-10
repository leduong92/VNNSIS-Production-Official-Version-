using System.ComponentModel.DataAnnotations;

namespace VNNSIS.Models {
    public class MoldWashingModel {

        public MoldWashingModel() {}
        public string wc                { get; set; }
        public string material_mark1    { get; set; }
        public string type_name         { get; set; }
        [Key]
        public string mold              { get; set; }
        public string mold_job          { get; set; }
        public string status            { get; set; }
        public string entry_date        { get; set; }
        public string entry_time        { get; set; }
        public string receive_wash_date { get; set; }
        public string receive_date        { get; set; }
        public string receive_time      { get; set; }
        public string start_time        { get; set; }
        public string estimate_time     { get; set; }
        public string end_time          { get; set; }
        public string delivery_time     { get; set; }
        public string judgment          { get; set; }
    }
}