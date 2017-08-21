using System;
using System.Collections;

namespace InRule.Samples.InvoiceObjects
{
	/// <summary>
	/// Summary description for LineItemCollection.
	/// </summary>
	[Serializable]
	public class LineItemCollection : CollectionBase
	{
		public LineItemCollection() {
		}
		
		public int Add(LineItem item) 
		{
			return InnerList.Add(item);
		}

		public LineItem this[int Index] 
		{
			get {return InnerList[Index] as LineItem;}
			set {InnerList[Index] = value;}
		}
	}		
}
