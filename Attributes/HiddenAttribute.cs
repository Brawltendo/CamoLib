using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamoLib.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class HiddenAttribute : Attribute
	{
		public bool IsHidden { get; set; }

		public HiddenAttribute(bool isHidden)
		{
			this.IsHidden = isHidden;
		}
	}
}
