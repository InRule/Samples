using System;
using System.Windows;
using InRule.Runtime;
using InRule.Runtime.Testing;

class Program
{
	[STAThread]
	static void Main()
	{
		// The EntityTester is WPF and thus requires an Application object. Since this is a very simple
		// application we must create it ourselves. This line of code is not necessary when used from a 
		// traditional WPF application.
		var app = new Application();

		// Load rule application from file system
		var session = new RuleSession(new FileSystemRuleApplicationReference("InvoiceLineItem.ruleappx"));
	    session.Settings.LogOptions = EngineLogOptions.RuleValues;

		// Create Invoice entity
		var invoiceEntity = session.CreateEntity("Invoice");

		// Pass the entity to the ShowDialog method. Note that a non-modal Show is also available
		// and an independent Rule Set may also be passed to the EntityTester.
		EntityTester.ShowDialog(invoiceEntity);
	}
}