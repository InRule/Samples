using System;

namespace InRule.Samples.InvoiceObjects
{
	/// <summary>
	/// Summary description for LineItemObject.
	/// </summary>
	[Serializable]
	public class LineItem
	{
		//Public Fields
		public LineItem() {}
		
		public int ProductID;
		
		public int Quantity;
		
		//Public Fields (that will be utilized as calcs in ruleapp)
		public string ProductName;
		
		public decimal UnitPrice;
		
		public decimal LineItemTotal;

		//Public Method(s)
		
		public double CalcSalesTax() 
		{
			return (double) LineItemTotal * 0.08;
		}

		public void AddRefVals(ref double d1, ref double d2, out double answer)
		{
			d1 = d1 + 1;
			d2 = d2 + 1;
			answer = d1 + d2;
		}
	}
}
