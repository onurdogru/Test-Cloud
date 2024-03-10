using ProPlan2250QC.DatabaseLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProPlan2250QC
{
    /// <summary>
    /// Interaction logic for DatabaseViewer.xaml
    /// </summary>
    public partial class DatabaseViewer : Window, INotifyPropertyChanged
    {
        private List<TestRecord> _testRecords;
        public List<TestRecord> TestRecords
        {
            get
            {
                return _testRecords;
            }
            set
            {
                _testRecords = value;
                OnPropertyChanged(nameof(TestRecords));
            }
        }

        public DatabaseViewer()
        {
            InitializeComponent();

            this.DataContext = this;

            dpFilterLastDate.SelectedDate = DateTime.Today;
            dpFilterLastDate.DisplayDateEnd = DateTime.Today;

            dgTestRecords.Columns[1].Title = "Ürün Seri No";
            dgTestRecords.Columns[3].Title = "Test Sonucu";
            dgTestRecords.Columns[4].Title = "%100 RPM";
            dgTestRecords.Columns[5].Title = "%100 Titreşim";
            dgTestRecords.Columns[6].Title = "%50 RPM";
            dgTestRecords.Columns[7].Title = "%50 Titreşim";
            dgTestRecords.Columns[8].Title = "%100 miliWatt";
            dgTestRecords.Columns[9].Title = "%50 miliWatt";

            GetRecords();

        }

        public List<string> PT_TestResultList { get; } = new List<string>() { "Hepsi", "Geçenler", "Kalanlar" };

        private void GetRecords()
        {
            using (DbModel oe = new DbModel())
            {
                var trs = from rec in oe.TestRecords
                          orderby rec.Id ascending
                          select rec;
                TestRecords = trs.ToList();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            DoSearch();
        }

        private void DoSearch()
        {
            int testId = -1;
            int.TryParse(txbFilterTestId.Text.Trim(), out testId);

            var isTestResultSelectionOff = cmbFilterTestResult.SelectedIndex == 0; // "Hepsi" seçili ise true verir
            var selectedTestResult = cmbFilterTestResult.SelectedIndex == 1; // "Geçenler" seçili ise true verir

            using (DbModel oe = new DbModel())
            {
                var trs = from rec in oe.TestRecords
                          where
                          (isTestResultSelectionOff || (rec.TestResult == selectedTestResult)) &&
                          (testId < 1 || rec.Id == testId)
                          orderby rec.Id ascending
                          select rec;

                var trs2 = trs.ToList();

                var trs3 = from rec in trs2
                           where
                           (dpFilterFirstDate.SelectedDate == null || rec.TestDateTime?.Date >= dpFilterFirstDate.SelectedDate.Value.Date) &&
                           (dpFilterLastDate.SelectedDate == null || /*(rec.TestDateTime == null && dpFilterLastDate.SelectedDate.Value.Date == DateTime.Today) || */ rec.TestDateTime?.Date <= dpFilterLastDate.SelectedDate.Value.Date) &&
                           checkSimilar(rec.SerialNumber, txbFilterSerialNo.Text) &&
                           checkSimilar(rec.Mounting, txbFilterMounting.Text) &&
                           checkSimilar(rec.Model, txbFilterModel.Text) &&
                           checkSimilar(rec.Operator, txbFilterOperator.Text)
                           select rec;

                TestRecords = trs3.ToList();
            }
        }

        private bool checkSimilar(string mainText, string searchContent)
        {
            var culture = CultureInfo.CreateSpecificCulture("tr");
            var searchContent1 = searchContent.Trim();

            if (string.IsNullOrEmpty(searchContent)) return true;
            else if (string.IsNullOrEmpty(mainText)) return false;
            else return culture.CompareInfo.IndexOf(mainText, searchContent1, CompareOptions.IgnoreCase) >= 0;
        }

        private void btnDeleteSelectedItems_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Seçili kayıtlar silinecek!", "", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
            {
                return;
            }

            List<TestRecord> selectedItems = new List<TestRecord>();
            try
            {
                selectedItems = dgTestRecords.SelectedItems.Cast<TestRecord>().ToList();
            }
            catch { }



            using (DbModel oe = new DbModel())
            {
                foreach (var testRecord in selectedItems)
                {
                    var entry = oe.Entry(testRecord);
                    if (entry.State == EntityState.Detached)
                        oe.TestRecords.Attach(testRecord);
                    oe.TestRecords.Remove(testRecord);
                }
                oe.SaveChanges();
            }

            DoSearch();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Event raised when a property has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        public void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.F5)
            {
                DoSearch();
            }
        }

        private void btnPrintAgain_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.PrintResult((dgTestRecords.SelectedItem as TestRecord), MainWindow.Settings);
        }
    }
}
