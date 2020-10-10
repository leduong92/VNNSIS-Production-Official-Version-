using System;
using System.ComponentModel.DataAnnotations;

namespace VNNSIS.Models
{
    public class MenuMasterModel
    {
		[Key]
		public int MenuIdentity { get; set; }
		public string MenuID { get; set; }
		public string MenuName { get; set; }
		public string ParentMenuID { get; set; }
		public string UserRole { get; set; }
		public string MenuFileName { get; set; }
		public string MenuURL { get; set; }
		public string USE_YN { get; set; }
		public DateTime CreatedDate { get; set; }
		public string MenuClass {get; set;}
		public string MenuText {get; set;}
		public string MenuIMG {get; set;}
		public string MenuName_VN {get; set;}
		public Int16 Order_id	{get; set;}
	}
}
