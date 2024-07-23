using System;
using System.Collections.Generic;
using System.Windows;
using System.Configuration;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace InRule.Runtime.Metrics.TestHarness
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string[] Columns;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void RunRulesClick(object sender, RoutedEventArgs e)
        {
			// delete previous csv files
			foreach (string filePath in Directory.GetFiles(ConfigurationManager.AppSettings["OutputDirectory"], "*.csv"))
			{
				try
				{
					File.Delete(filePath);
				}
				catch
				{
				}
			}

			RunRulesButton.IsEnabled = false;

			try
			{
				// run the rules
				await RuleExecution.RunRules(
					ConfigurationManager.AppSettings["RuleAppPath"],
					ConfigurationManager.AppSettings["DataPath"],
					ConfigurationManager.AppSettings["TopEntityName"]);

				MetricDataGrid.Items.Clear();

				// read the data from the CSV and show it on the screen

				await LoadGridFromCsv();
			}
			finally
			{
				RunRulesButton.IsEnabled = true;
			}
		}

        private async Task LoadGridFromCsv()
        {
            string path = $"{ConfigurationManager.AppSettings["OutputDirectory"]}";

            // only looking at the first file for this sample
            string firstFile = Directory.GetFiles(path).First();
			bool firstLine = true;
			string fileContents;
			using (FileStream stream = new FileStream(firstFile, FileMode.Open, FileAccess.Read, FileShare.Read))
			using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
			{
				fileContents = await reader.ReadToEndAsync();
			}

			foreach (string line in fileContents.Split(new [] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
			{
				if (firstLine)
				{
					if (MetricDataGrid.Columns.Count == 0)
					{
						AddColumns(line);
					}
					firstLine = false;
				}
				else
				{
					AddRow(line);
				}
			}
        }

        private void AddRow(string line)
        {
            string[] list = line.Split(',');
            var row = new ExpandoObject() as IDictionary<string, Object>;

			for (int i = 0; i < Columns.Length; i++)
			{
				row.Add(Columns[i], list[i]);
			}

			MetricDataGrid.Items.Add(row);
        }

        private void AddColumns(string line)
        {
            Columns = line.Split(',');
            double width = MetricDataGrid.ActualWidth / Columns.Length - 10;
            
            foreach (string header in Columns)
            {
                DataGridTextColumn textColumn = new DataGridTextColumn();
                textColumn.Header = header;
                textColumn.Binding = new Binding(header);
                textColumn.Width = width;
                MetricDataGrid.Columns.Add(textColumn);
            }
        }
    }
}
