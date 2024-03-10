using ProPlan2250QC.DatabaseLibrary;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProPlan2250QC
{
    /// <summary>
    /// Interaction logic for PrintPage.xaml
    /// </summary>
    public partial class PrintPage : Page
    {
        private TestResultAdapter testResult;

        public PrintPage(TestResultAdapter status)
        {
            this.testResult = status;

            this.DataContext = testResult;

            InitializeComponent();
        }


        private TestRecord testRecord;

        public PrintPage(TestRecord testRecord)
        {
            this.testRecord = testRecord;

            this.Metadata = new Metadata();

            Metadata.AlpplasMetadata1 = testRecord.AlpplasMetadata1;
            Metadata.AlpplasMetadata2 = testRecord.AlpplasMetadata2;
            Metadata.AlpplasMetadata3 = testRecord.AlpplasMetadata3;
            Metadata.AttachmentsAndAccessories = testRecord.Accessories;
            Metadata.DateTimeUnformatted = testRecord.TestDateTime ?? new DateTime(2000, 01, 01, 00, 00, 00);
            Metadata.LoadingConditions = testRecord.Loading;
            Metadata.Model = testRecord.Model;
            Metadata.Mounting = testRecord.Mounting;
            Metadata.Operator = testRecord.Operator;
            Metadata.PowerSource = testRecord.PowerSupply;
            Metadata.SerialNumber = testRecord.SerialNumber;

            TemporaryTestId = testRecord.Id;

            this.DataContext = this;

            InitializeComponent();
        }

        public Metadata Metadata { get; private set; }

        public long TemporaryTestId { get; private set; }

        public string StatusForPrint
        {
            get
            {
                if (testRecord.TestResult)
                {
                    return "Test OK";
                }
                else
                {
                    return "Test NOK";
                }
            }
        }
    }
}
