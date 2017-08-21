using System;

namespace InRule.Samples.InvoiceObjects
{
	/// <summary>
	/// Summary description for FunctLib.
	/// </summary>
	public class FunctLib
	{
		private FunctLib() {}
		
		public static double AddTwoNumbers(double v1, double v2) {
			return v1 + v2;
		}
		
		public static void AddTwoNumbersByRef(double v1, double v2, out double result) {
			result = AddTwoNumbers(v1, v2);
		}
	}
}
