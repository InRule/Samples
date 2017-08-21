using System;
using System.Windows.Forms;
using InRule.Repository;

namespace WindowsFormsApplication1
{
    public partial class ErrorForm : Form
    {
        public ErrorForm(Exception ex, RuleRepositoryDefBase def)
        {
            InitializeComponent();

            errorTextBox.Text = String.Format("ERROR for def: {0} ({1}){2}{2}{3}", def.Name, def.GetType().ToString(), Environment.NewLine, ex.ToString());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
