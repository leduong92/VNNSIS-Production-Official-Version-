using System;
using System.ComponentModel.DataAnnotations;

namespace VNNSIS.ViewModels {
    public class OperationNumberViewModel {

        [Key]
        public Int16 OperationNumber { get; set; }
    }
}