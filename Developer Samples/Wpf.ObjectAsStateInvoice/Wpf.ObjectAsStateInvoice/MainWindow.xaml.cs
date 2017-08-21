using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

using InRule.Runtime;
using InRule.Runtime.Testing;
using InRule.Repository;

using InRule.Samples.InvoiceObjects;
using InRule.Repository.Infos;

namespace Wpf.ObjectAsStateInvoice
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            base.DataContext = Invoice;
        }

        public Invoice Invoice;
        public Entity InvoiceEntity;
        private RuleSession _ruleSession;

        private RuleApplicationDef GetInvoiceRuleAppDef()
        {
            // lookup the rule appfile name as a relative path
            string exPath = AppDomain.CurrentDomain.BaseDirectory;
            string ruleAppFileName = "ObjectAsStateInvoice.ruleappx";
            FileInfo outputFile = new FileInfo(System.IO.Path.Combine(exPath, ruleAppFileName));
            if (outputFile.Exists == false)
            {
                throw new ApplicationException(string.Format("Rule app '{0}' was not found", outputFile.FullName));
            }

            RuleApplicationDef ruleApp = RuleApplicationDef.Load(outputFile.FullName);

            return ruleApp;
        }
        
        private RuleSession GetRuleSession()
        {
            RuleApplicationDef ruleAppDef = GetInvoiceRuleAppDef();

            try
            {
                _ruleSession = new RuleSession(ruleAppDef);
            }
            catch (CompileException ex)
            {
                ShowErrorMessageBox(ex);
            }
            return _ruleSession;
        }
        private Entity GetRuleEntity()
        {
            // Create the state for our entity, create and reference to the entity and apply rules.
            InvoiceEntity = GetRuleSession().CreateEntity("Invoice", Invoice);
            return InvoiceEntity;
        }
        private void ApplyRules()
        {
            Entity entity = GetRuleEntity();
            _ruleSession.ApplyRules();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            try
            {
            	//Create an instance of the entity object
				Invoice = new Invoice();
                base.DataContext = Invoice;

				Invoice.CustomerId = 1;
				Invoice.InvoiceId = 1001;
				Invoice.Value1 = 10;
				Invoice.Value2 = 20;
				Invoice.Value3 = 0;
				Invoice.Value4 = 0;

				//Add a single line item				
				LineItem lineItem = new LineItem();
				lineItem.ProductID = 1;
				lineItem.Quantity = 2;
				Invoice.LineItems.Add(lineItem);

				//Apply rules and update the form from the results				
				ApplyRules();

			}
			catch(Exception ex)
			{
				ShowErrorMessageBox(ex);
			}

        }

        private void InputTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyRules();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EntityTester.ShowDialog(InvoiceEntity);
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox(ex);
            }
        }

        #region ErrorMessageBoxes
        private void ShowErrorMessageBox(Exception ex)
        {
            string errMsg = ex.Message;

            if (ex is CompileException)
            {
                var errors = ((CompileException)ex).Errors;

                foreach (var err in errors)
                {
                    errMsg += Environment.NewLine + " Compile Error: " + err;    
                }
            }

            if (this._ruleSession != null)
            {   
                if (_ruleSession.LastRuleExecutionLog.HasErrors)
                {
                    foreach (var err in _ruleSession.LastRuleExecutionLog.ErrorMessages)
                    {
                        errMsg += Environment.NewLine + " Error: " + err.Description;
                    }
                }
            }

            MessageBox.Show(this, errMsg, "An error has occured", MessageBoxButton.OK, MessageBoxImage.Error );
        }
        #endregion

    }
}
