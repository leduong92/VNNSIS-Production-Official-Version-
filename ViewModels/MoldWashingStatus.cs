using System.Collections.Generic;

namespace VNNSIS.Models {
    public class MoldWashingStatus
    {
        public List<MoldWashingModel> lstMoldWashing;

        public MoldWashingStatus()
        {
            lstMoldWashing = new List<MoldWashingModel>();
        }
    }
}