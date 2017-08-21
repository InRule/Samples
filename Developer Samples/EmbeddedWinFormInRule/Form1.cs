using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using InRule.Authoring.Controls;
using InRule.Authoring.Editors;
using InRule.Authoring.Extensions;
using InRule.Repository;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private RuleApplicationDef _ruleAppDef;
        private ControlFactory _controlFactory;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // initialize WPF Application object, this will initialize resources that are 
            // required by the InRule WPF controls
            System.Windows.Application app = new System.Windows.Application();

            // set the shutdown mode to explicit so it will stay alive for the life of the WinForm
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // create a delegate to close the Application object when the winform closes
            Closed += delegate { app.Shutdown(); };
        }

        private void LoadDefs()
        {
            defListBox.Items.Clear();

            var defs = from d in _ruleAppDef.AsEnumerable()
                       orderby d.Name
                       select d;

            defListBox.DisplayMember = "Name";
            defs.ForEach(d => defListBox.Items.Add(d));
        }

        private void defListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var validatingEditor = elementHost1.Child as IValidatingEditor;
            if (validatingEditor != null)
            {
                validatingEditor.SaveValues();
            }

            elementHost1.Child = null;

            var def = ((ListBox) sender).SelectedItem as RuleRepositoryDefBase;
            if (def != null)
            {
                try
                {
                    elementHost1.Child = _controlFactory.GetControl(def);
                }
                catch (Exception exception)
                {
                    var errorForm = new ErrorForm(exception, def);
                    errorForm.ShowDialog();
                }
            }
        }
        
        private void OpenClick(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.CheckPathExists = false;
            dialog.CheckFileExists = false;
            dialog.Filter = "Rule Apps|*.ruleapp";
            dialog.ShowDialog();

            if (!String.IsNullOrEmpty(dialog.FileName))
            {
                Cursor.Current = Cursors.WaitCursor;

                Reset();

                _ruleAppDef = RuleApplicationDef.Load(dialog.FileName);
                _controlFactory = new ControlFactory(_ruleAppDef);

                LoadDefs();

                Cursor.Current = Cursors.Default;
            }
        }

        private void ExitClick(object sender, EventArgs e)
        {
            Close();
        }

        private void Reset()
        {
            if (_ruleAppDef != null)
            {
                _ruleAppDef.Dispose();    
            }

            if (_controlFactory != null)
            {
                _controlFactory.Dispose();
            }

            elementHost1.Child = null;
        }

    }
}
