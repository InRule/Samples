using System;
using System.ComponentModel;

namespace InRule.Samples.InvoiceObjects
{
	/// <summary>
	/// Summary description for Invoice.
	/// </summary>
	[Serializable]
	public class Invoice : INotifyPropertyChanged
	{
		private string _customerName;
		private int _customerId;
		private int _invoiceId;
		private decimal _total;

		private int _value1;
		private int _value2;
		private int _value3;
		private int _value4;

		//Public Instance Methods
		
		/// <summary>
		/// Simple int return, byval int params example instance method
		/// </summary>
        public int ComputeTestValue(int v1, int v2)
		{
			return _value1 + v1+v2;
		}
		
		/// <summary>
		/// Simple int return, byval int params example instance method
		/// </summary>
		public void ComputeTestValueByRef(int v1, int v2, out int v3)
		{
			v3 = _value1 + v1+v2;
		}


		/// <summary>
		/// Simple DateTime in / out test
		/// </summary>
		public DateTime AddYearsToDate(DateTime arg, int yrs)
		{
			return arg.AddYears(yrs);
		}
		
		//Public Properties
		
		public string CustomerName 
        {
            get{return this._customerName;} 
            set
            {
                this._customerName=value;
                this.OnPropertyChanged("CustomerName");
            }
        }
		public int CustomerId 
        {
            get{return this._customerId;} 
            set
            {
                this._customerId=value;
                this.OnPropertyChanged("CustomerId");
            }
        }
		public int InvoiceId 
        {
            get{return this._invoiceId;} 
            set
            {
                this._invoiceId=value;
                this.OnPropertyChanged("InvoiceId");
            }
        }
		public decimal Total 
        {
            get{return this._total;} 
            set
            {
                this._total=value;
                this.OnPropertyChanged("Total");
            }
        }

		public int Value1 {get{logGet("Value1",_value1);return this._value1;} set{logSet("Value1",value);this._value1=value;}}
		public int Value2 {get{logGet("Value2",_value2);return this._value2;} set{logSet("Value2",value);this._value2=value;}}
		public int Value3 
        {
            get{logGet("Value3",_value3);return this._value3;} 
            set
            {
                logSet("Value3",value);
                this._value3=value;
                this.OnPropertyChanged("Value3");
            }
        }
		public int Value4 
        {
            get{logGet("Value4",_value4);return this._value4;} 
            set
            {
                logSet("Value4",value);
                this._value4=value;
                this.OnPropertyChanged("Value4");
            }
        }
		
		public LineItemCollection LineItems = new LineItemCollection();

		private void logGet(string name, object value)
		{
			Console.WriteLine("Get(\"{0}\")=\"{1}\"", name,value);
		}
		private void logSet(string name, object value)
		{
			Console.WriteLine("Set(\"{0}\")=\"{1}\"", name,value);
		}


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(
                    this, new PropertyChangedEventArgs(propName));
        }



        #endregion
    }
}
