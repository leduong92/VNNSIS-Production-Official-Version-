using System.ComponentModel.DataAnnotations;

namespace VNNSIS.Models {
    public class SisProErrorRecord {

        [Key]
        // public int Id { get; set; }
        public string JobOrderNo { get; set; }
        public int OperationNumber { get; set; }
        //[MaxLength (20)]
        public string FinishedGoodsCode { get; set; }
        //[MaxLength (10)]
        public string LotNo { get; set; }
        public int CavityQty { get; set; }
        //[MaxLength (9)]
        public string LineNo { get; set; }
        //[MaxLength (8)]
        public string RubberName { get; set; }
        public int PlanCycle { get; set; }
        public int PlanQty { get; set; }
        public int UnitCost { get; set; }
        public int UnitPrice { get; set; }
        public string JobStartDate { get; set; }
        public string JobEndDate { get; set; }
        public string OperationStartDate { get; set; }
        public string OperationEndDate { get; set; }
        //[MaxLength (10)]
        public string MachineNo { get; set; }
        public int OkQty { get; set; }
        //[MaxLength (6)]
        public string ProgressOperationCode { get; set; }
        public int ProgressOperationSeq { get; set; }
        //[MaxLength (22)]
        public string ProgressOperationName { get; set; }
        public string ErrorID { get; set; }
        //[MaxLength (255)]
        public string ErrorName { get; set; }
        //[MaxLength (255)]
        public string ErrorNameJP { get; set; }
        public int ErrorQty { get; set; }
        //[MaxLength (255)]
        public string Notes { get; set; }
        public string EntryDate { get; set; }
        public string EntryTime { get; set; }
        public string EntryUser { get; set; }
        public string UpdateDate { get; set; }
        public string UpdateTime { get; set; }
        public string UpdateUser { get; set; }
        public string Status { get; set; }
        //[MaxLength (255)]
        public string ErrorNameEn { get; set; }
        public string CuringDate {get; set;}
        public string Department {get; set;}
        public string Area {get; set;}
        public string ProgramID {get; set;}
        public string PressNo {get; set;}
    }
}