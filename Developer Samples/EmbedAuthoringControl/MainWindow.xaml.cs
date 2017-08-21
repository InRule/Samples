using System.Linq;
using System.Windows;
using InRule.Authoring.Controls;
using InRule.Authoring.Editors;
using InRule.Authoring.Extensions;
using InRule.Repository;

namespace EmbedAuthoringControl
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            // Load rule application from file system
            var ruleAppDef = RuleApplicationDef.Load("InvoiceLineItem.ruleappx");

            // Create control factory
            var controlFactory = new ControlFactory();
            
            // Open the rule applicationo
            controlFactory.OpenRuleApplication(ruleAppDef);
            
            // Find the def for the control we want to show
            var ruleDef = controlFactory.RuleApplicationDef.AsEnumerable().Where(def => def.Name == "LanguageRule1").First();

            // Get the control and populate the content control
            contentControl.Content = controlFactory.GetControl(ruleDef);
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            // Save the changes to the def
            ((IValidatingEditor)contentControl.Content).SaveValues();
        }
    }
}
