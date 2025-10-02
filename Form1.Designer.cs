/* CSV display application.
 * This program requests a .csv file and then reads its contents and displays it as a grid table.
 * Cells can be double clicked to display their value in a new window. */

using System;
using System.IO;

namespace csharp;

partial class Form1 : Form
{
  private Form fileSelectDialog = new Form();
  private DataGridView csvDataGridView = new DataGridView();
  private string file;
  private List<string[]> csv = new List<string[]>();
  
  private void InitializeComponent()
  {
    SelectCSV();
    ReadCSV();
    SetupLayout();
    SetupDataGridView();
    
    // Populate the DataGridView
    this.csv.ForEach(row => csvDataGridView.Rows.Add(row));
  }

  /* Open the file select in the file explorer
   * We ignore the case where the user closes the file select window. They can open the file select dialog again if they wish. */
  private void selectFileButton_Click(object sender, EventArgs e)
  {
    OpenFileDialog openFileDialog1 = new OpenFileDialog();
        
    openFileDialog1.InitialDirectory = "c:\\";
    openFileDialog1.Filter = "CSV files (*.csv)|*.csv";
    openFileDialog1.FilterIndex = 0;
    openFileDialog1.RestoreDirectory = true;

    // Checks if file explorer was closed. Also prevents the fileSelectDialog form from closing
    if (openFileDialog1.ShowDialog() != DialogResult.OK)
      return;

    this.file = openFileDialog1.FileName;

    fileSelectDialog.Close();
  }

  /* Event handler for when the file dialog box is closing */
  private void FileSelectDialog_FormClosing(object sender, FormClosingEventArgs e)
  {
    // Closing before a file is selected is interpreted as wanting to close the program
    if (String.IsNullOrEmpty(this.file))
      Environment.Exit(0);
  }

  /* Open a dialog box that allows the user to select a .csv file */
  private void SelectCSV()
  {
    Label label1 = new System.Windows.Forms.Label();
    Panel buttonPanel = new Panel();
    Button selectFileButton = new Button ();

    label1.AutoSize = true;
    label1.Name = "label1";
    label1.TabIndex = 2;
    label1.Location = new System.Drawing.Point(50, 50);
    label1.Size = new System.Drawing.Size(76, 13);
    label1.Text = "Please select a .CSV file:";

    selectFileButton.Text = "select";
    selectFileButton.Location = new Point (10, 10);
    selectFileButton.Click += new EventHandler(selectFileButton_Click);

    buttonPanel.Height = 50;
    buttonPanel.Dock = DockStyle.Bottom;

    buttonPanel.Controls.Add(selectFileButton);

    fileSelectDialog.Text = "CSV Display";

    fileSelectDialog.Controls.Add(label1);
    fileSelectDialog.Controls.Add(buttonPanel);


    fileSelectDialog.FormClosing += new FormClosingEventHandler(FileSelectDialog_FormClosing);

    fileSelectDialog.ShowDialog();
  }

  /* Write errors to log file. Fallback to terminal */
  private void Error(string s, Exception e)
  {
    try
    {
      string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
      string path = Path.Combine(desktop, "CSVDisplay-Error.txt");

      using (StreamWriter sw = File.AppendText(path))
      {
        sw.WriteLine("----------------------------------------------");
        sw.WriteLine("ERROR: " + s);
        sw.WriteLine(e.Message);
        sw.WriteLine("----------------------------------------------");
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("ERROR: Could not write to error log file");
      Console.WriteLine(ex.Message);
    }
  }
  
  /* Read the contents of the input CSV file */
  private void ReadCSV()
  {
    try
    {
      using StreamReader reader = new(this.file);
    
      string text = reader.ReadToEnd();
    
      // Parse the csv data
      this.csv = text.Split('\n')
		     .Where(t => t.Length > 0) // Skip empty rows in the .csv
                     .Select(t => t.Split(','))
                     .ToList();
    }
    catch (ArgumentException ae)
    {
      Error("File stream does not support reading", ae);
      Environment.Exit(0);
    }
    catch (OutOfMemoryException ome)
    {
      Error("There is insufficient memory to allocate a buffer for the returned string", ome);
      Environment.Exit(0);
    }
    catch (IOException e)
    {
      Error($"Failed to read the file {this.file}:", e);
      Environment.Exit(0);
    }
  }

  /* When a user double clicks on a cell in the form, we want to display
   * the contents of that cell in a new window. */
  private void cellDouble_Click(object sender, DataGridViewCellEventArgs e)
  {
    Form form2 = new Form();
    Label label1 = new System.Windows.Forms.Label();
    
    label1.AutoSize = true;
    label1.Name = "label1";
    label1.TabIndex = 2;
    label1.Location = new System.Drawing.Point(50, 50);
    label1.Size = new System.Drawing.Size(76, 13);

    // If a cell has no value, say if there were two commas with no characters in between them, then fallback
    label1.Text = csvDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString() ?? "(Empty)";
    
    form2.Controls.Add(label1);
    form2.Text = $"Cell[{e.ColumnIndex},{e.RowIndex}]";
    
    form2.Show();
  }

  /* Add settings for the layout */
  private void SetupLayout()
  {
    this.Size = new Size(600, 500);
  }

  /* Initialize the DataGridView */
  private void SetupDataGridView()
  {
    this.Controls.Add(csvDataGridView);
    
    // determine longest row
    int length = this.csv.Max(line => line.Length);
    
    csvDataGridView.ColumnCount = length;
    csvDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.Navy;
    csvDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
    csvDataGridView.ColumnHeadersDefaultCellStyle.Font = new Font(csvDataGridView.Font, FontStyle.Bold);
    
    csvDataGridView.Name = "csvDataGridView";
    csvDataGridView.Location = new Point(8, 8);
    csvDataGridView.Size = new Size(500, 250);
    csvDataGridView.GridColor = Color.Black;
    csvDataGridView.RowHeadersVisible = false;
    csvDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
    csvDataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
    csvDataGridView.CellBorderStyle = DataGridViewCellBorderStyle.Single;
    
    // Label the columns
    for (var i = 0; i < length; i++)
      csvDataGridView.Columns[i].Name = $"Column {i + 1}";
    
    csvDataGridView.SelectionMode = DataGridViewSelectionMode.CellSelect;
    csvDataGridView.MultiSelect = false;
    csvDataGridView.Dock = DockStyle.Fill;
    
    csvDataGridView.CellDoubleClick += new DataGridViewCellEventHandler(cellDouble_Click);
  }
}
