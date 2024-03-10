using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;
using ProPlan.QcComponents.ViewModels;
using ProPlan.QcComponents.Data;
using ProPlan.RESTLibrary;
using ProPlan.QcComponents;
using System.Drawing.Printing;
using System.Threading;
using System.Diagnostics;
using System.Printing;
using ProPlan2250QC.DatabaseLibrary;
using Modbus.Data;
using Modbus.Device;
using Modbus.Utility;
using System.IO.Ports;

namespace ProPlan2250QC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// 2250 ile REST protokolü üzerinden iletişimi sağlayan sınıf
        /// </summary>
        private RESTWrapper restWrapper;

        /// <summary>
        /// Ölçüm ayarlarının kaydedildiği sınıf
        /// </summary>
        public static Settings Settings;

        /// <summary>
        /// 2250'den anlık veri alırken kullanılan timer
        /// </summary>
        DispatcherTimer timer;
        DispatcherTimer timerModbus;

        /// <summary>
        /// Ölçülen anlık verilerin toplantığı, tolerans limitlerini, ok/nok bilgilerini
        /// grafikler için alt viewmodel'ları içeren sınıf
        /// </summary>
        private FftViewModel viewModel;

        public static PrintDialog TagPrintDialog = new PrintDialog();
        private bool isPropertyValueChangedHandled;
        private TestResultAdapter testResultAdapter;

        public SerialPort rs232 = new SerialPort();
        public bool modbuskontrolbit = true;
        public ushort[] readreg = new ushort[40];
        public double olcum50, olcum100, rpm50, rpm100, watt50, watt100;
        public bool test;
        public int anlikrpm, anlikwatt;

        public MainWindow()
        {
            InitializeComponent();

            viewModel = new FftViewModel(0, "Titreşim Spektrumu", AbstractGraphViewModel.GraphUnitX.RPM, AbstractGraphViewModel.GraphUnitY.AccelerationMMS2, "AccSpec1", new Profile());
            FftGraph.ViewModel = viewModel; // FftGraph: Arayüzdeki canlı FFT grafiği için usercontrol. Tanımlandığı yer WPF xaml kodu.
            Settings = new Settings(viewModel);
            Settings.DisplayTypeChanged += Settings_DisplayTypeChanged;
            Settings.LoadSettings();
            Settings.LoadPresets();

            // WPF binding için:
            this.DataContext = this;
            testResultAdapter = new TestResultAdapter(viewModel, qcMetadata);
            this.brdStatus.DataContext = testResultAdapter;
            FftGraph.DataContext = testResultAdapter;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(600);
            timer.Tick += Timer_Tick;

            restWrapper = new RESTWrapper();
            //restWrapper = new DemoRestWrapper();  // Design time'da gerçek cihaz bağlamadan çalışmak için

            restWrapper.DeviceIP = Settings.DeviceIp;
            restWrapper.MeasurementPaused += RestWrapper_MeasurementPaused;
            restWrapper.MeasurementStopped += RestWrapper_MeasurementPaused;
            restWrapper.MeasurementRunning += RestWrapper_MeasurementRunning;
            restWrapper.ConnectionEstablising += RestWrapper_ConnectionEstablising;
            restWrapper.WrongSerialNumber += RestWrapper_WrongSerialNumber;
            restWrapper.ConnectionError += RestWrapper_ConnectionError;
            restWrapper.Connected += RestWrapper_Connected;
            this.Closed += new EventHandler((ob, ev) => restWrapper.Dispose());

            timerModbus = new DispatcherTimer();
            timerModbus.Interval = TimeSpan.FromMilliseconds(500);
            timerModbus.Tick += timerModbus_Tick;
            
        }

        public void rs232_baglan()
        {
            try
            {
                rs232.PortName = ayarlar.Default.comPort;
                rs232.BaudRate = 9600;
                rs232.DataBits = 7;
                rs232.StopBits = System.IO.Ports.StopBits.One;
                rs232.Parity = System.IO.Ports.Parity.Even;
                rs232.Open();
                if (rs232.IsOpen)
                {
                    modbuskontrolbit = true;
                }
            }
            catch (Exception)
            {
                modbuskontrolbit = false;
            }
        }

        private void RestWrapper_ConnectionError(object sender, EventArgs e)
        {
            timer.Stop();
            btnConnect.IsEnabled = true;
            btnStartStop2250.IsEnabled = false;
            btnStartAveraging.IsEnabled = false;
            MessageBox.Show("Bağlantı hatası.");
        }

        private void RestWrapper_WrongSerialNumber(object sender, EventArgs e)
        {
            timer.Stop();
            btnConnect.IsEnabled = true;
            btnStartStop2250.IsEnabled = false;
            btnStartAveraging.IsEnabled = false;
            MessageBox.Show("Cihaz seri numarası geçersiz.");
        }

        private void Settings_DisplayTypeChanged(object sender, EventArgs e)
        {
            if (Settings.DisplayType == 0)
            {
                brdStatus.Visibility = Visibility.Visible;
                FftGraph.Visibility = Visibility.Collapsed; 
            }
            else
            {
                brdStatus.Visibility = Visibility.Collapsed;
                FftGraph.Visibility = Visibility.Visible;
            }
        }

        private void RestWrapper_Connected(object sender, EventArgs e)
        {
            btnConnect.IsEnabled = true;
            btnStartStop2250.IsEnabled = true;
            btnStartAveraging.IsEnabled = true;
        }

        private void RestWrapper_ConnectionEstablising(object sender, EventArgs e)
        {
            btnStartStop2250.IsEnabled = false;
            btnStartAveraging.IsEnabled = false;
            btnConnect.IsEnabled = false;
        }

        private void RestWrapper_MeasurementRunning(object sender, EventArgs e)
        {
            btnStartStop2250.IsEnabled = true;
            btnStartAveraging.IsEnabled = true;
            btnStartStop2250.Content = "2250'yi Durdur";
        }

        private void RestWrapper_MeasurementPaused(object sender, EventArgs e)
        {
            btnStartStop2250.IsEnabled = true;
            btnStartAveraging.IsEnabled = true;
            btnStartStop2250.Content = "2250'yi Başlat";
        }

        //Güç ve RPM Değerleri Anlık Yazılıyor
        private void updateDisplays()
        {
            anlikrpm = readreg[0];
            anlikwatt = readreg[2] * readreg[4] / 100;
            label.Content = "Sensör Devir:" + anlikrpm.ToString();
            label14.Content = "Watt:" + anlikwatt.ToString();
        }

        //Güç ve RPM Verileri Alınıyor
        private void timerModbus_Tick(object sender, EventArgs e)
        {
            if (modbuskontrolbit)
            {
                try
                {
                        IModbusSerialMaster master = ModbusSerialMaster.CreateAscii(rs232);
                        master.Transport.ReadTimeout = 300;

                        readreg = master.ReadHoldingRegisters(1, 4256, 10); //Güç ve RPM değerleri
                        updateDisplays();

                }
                catch (Exception Hata)
                {
                    MessageBox.Show("Modbus bağlantı hatası!");
                    modbuskontrolbit = false;
                }
            }
        }

        //Titreşim Verisi Alınıyor
        private async void Timer_Tick(object sender, EventArgs e)
        {
            double[] spectrum;
            try
            {
                await restWrapper.UpdateMeasOutputsAndMeasState();
                viewModel.Lines = restWrapper.Lines;
                viewModel.Resolution = restWrapper.Resolution;

                // High pass filtresinden geçir ve
                // m/s2 degerlerini mm/s2'ye cevir:
                spectrum = Calculator.FilterHighPass(Calculator.Multiply(restWrapper.Raw_FFT_InEngineeringUnits, 1000), Settings.HighPassFrequency, viewModel.Resolution);
            }
            catch
            {
                //viewModel.Lines = 200;
                //viewModel.Resolution = 0.5;
                spectrum = Calculator.FilterHighPass(Calculator.Multiply(Calculator.CreateRandomArray(201, 40, 10), 1000), Settings.HighPassFrequency, viewModel.Resolution);
                //throw;
            }

            if (averageMode)
            {
                if (averageModeInProgress)
                {
                    listToBeAveraged.Add(spectrum);
                }
                viewModel.Profile.AddSpectrum(Calculator.AverageSpectrums(listToBeAveraged));

                testResultAdapter.UpdateProperties();
                return;
            }

            viewModel.Profile.AddSpectrum(spectrum);
            testResultAdapter.UpdateProperties();
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

        //PLC Fan Kontrolü
        private void fanKontrol(bool durum, bool pwm)
        {
            IModbusSerialMaster master = ModbusSerialMaster.CreateAscii(rs232);
            master.Transport.ReadTimeout = 300;

            try
            {
                master.WriteSingleCoil(1, 2049, durum);
                master.WriteSingleCoil(1, 2048, pwm);
            }
            catch (Exception)
            {
                MessageBox.Show("Modbus gönderme hatası!");
            }
        }

        private void btnStartStop2250_Click(object sender, RoutedEventArgs e)
        {
            averageMode = false;
            testResultAdapter.IsIndeterminate = true;
            prgMeasurement.Value = 0;

            btnStartStop2250.IsEnabled = false;
            startPause();
            //fanKontrol(true, false);
        }

        private async void startPause()
        {
            try
            {
                await Task.Factory.StartNew(restWrapper.StartPause);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        //PLC ve Başla Titreşim Ölçer Bağlan
        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                rs232_baglan();
                modbuskontrolbit = true;
                timerModbus.Start();
                
                averageMode = false;
                testResultAdapter.IsIndeterminate = true;
                prgMeasurement.Value = 0;

                restWrapper.DeviceIP = Settings.DeviceIp;

                if (await restWrapper.Connect())
                {
                    await restWrapper.UpdateSetupParameters();  // FFT Lines ve Span bilgilerini cihazdan çekmek için
                    timer.Start(); 
                }
            }
            catch { }
        }

        //Ayarları Aç
        private void btnOpenSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsBackup = Settings.ShallowCopy();

            var settingsWindow = new SettingsWindow(Settings);
            this.Closing += new CancelEventHandler((o, ev) => settingsWindow.Close());
            if (settingsWindow.ShowDialog() == false)
            {
                Settings = settingsBackup;
                Settings.UpdateViewModel();
                Settings_DisplayTypeChanged(this, null);
            }
        }

        private void pgMetadata_PropertyValueChanged(object sender, Xceed.Wpf.Toolkit.PropertyGrid.PropertyValueChangedEventArgs e)
        {
            if (!isPropertyValueChangedHandled)
            {
                isPropertyValueChangedHandled = true;
                pgMetadata.Update();
                isPropertyValueChangedHandled = false;
            }
        }
        //Teste Başla
        private void btnStartAveraging_Click(object sender, RoutedEventArgs e)
        {
            btnStartAveraging.Visibility = System.Windows.Visibility.Hidden;
            label11.Content = "%100 PWM Test Başladı...";
            label10.Content = "----";
            label1.Content = "----";
            label2.Content = "----";
            label3.Content = "----";
            label4.Content = "----";
            label_Sonuc.Content = "";
            label_Sonuc.Background = Brushes.Transparent;
            fanKontrol(true, false);
            testet(sender, e);            
        }

        private async void testet(object sender, RoutedEventArgs e)
        {
            btnStartStop2250_Click(sender, e);

            qcMetadata.UpdateDateTime();

            // Gecikme süresi
            prgMeasurement.IsIndeterminate = true;
            await Task.Run(() => Thread.Sleep(Settings.PreDelayDuration));
            prgMeasurement.IsIndeterminate = false;

            listToBeAveraged.Clear();
            averageMode = true;
            testResultAdapter.IsIndeterminate = false;
            averageModeInProgress = true;

            prgMeasurement.Value = 0;
            var duration = Settings.MeasurementDuration * 1000;
            if (restWrapper.State != RESTWrapper.StateType.Running)
            {
                await Task.Factory.StartNew(startPause);
            }

            Task.Run(() =>
            {
                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < duration)
                {
                    Application.Current.Dispatcher.Invoke(() => prgMeasurement.Value = sw.ElapsedMilliseconds * 100 / duration);
                    Thread.Sleep(50);
                }
            });

            await Task.Run(() => Thread.Sleep(duration));
            prgMeasurement.Value = 100;

            averageModeInProgress = false;
            startPause();

            /*
            long testId = -1;

            if (Settings.IsAutoSaveOn)
            {
                testId = saveToDatabase();
            }
            else
            {
                if (MessageBox.Show("Sonuç veritabanına kaydedilsin mi?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    testId = saveToDatabase();
                }
            }

            testResultAdapter.TemporaryTestId = testId;

            bool isPassed = viewModel.ToleranceState == FftViewModel.ToleranceStates.Inside;

            if (isPassed)
            {
                if (MessageBox.Show("Numune geçti. Etiket yazdırılacak.", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    printResult();
                } 
            }

            */

            olcum100 = Math.Round(viewModel.PeakCursorY, 5);
            label1.Content = olcum100.ToString();
            rpm100 = (double)anlikrpm;
            label2.Content = rpm100.ToString();
            watt100 = (double)anlikwatt;
            label12.Content = watt100.ToString();

            fanKontrol(true, true);
            label11.Content = "%50 PWM Test Başladı...";
            testet2(sender, e);
        }

        private async void testet2(object sender, RoutedEventArgs e)
        {
            btnStartStop2250_Click(sender, e);

            qcMetadata.UpdateDateTime();

            // Gecikme süresi
            prgMeasurement.IsIndeterminate = true;
            await Task.Run(() => Thread.Sleep(Settings.PreDelayDuration));
            prgMeasurement.IsIndeterminate = false;

            listToBeAveraged.Clear();
            averageMode = true;
            testResultAdapter.IsIndeterminate = false;
            averageModeInProgress = true;

            prgMeasurement.Value = 0;
            var duration = Settings.MeasurementDuration * 1000;
            if (restWrapper.State != RESTWrapper.StateType.Running)
            {
                await Task.Factory.StartNew(startPause);
            }

            Task.Run(() =>
            {
                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < duration)
                {
                    Application.Current.Dispatcher.Invoke(() => prgMeasurement.Value = sw.ElapsedMilliseconds * 100 / duration);
                    Thread.Sleep(50);
                }
            });

            await Task.Run(() => Thread.Sleep(duration));
            prgMeasurement.Value = 100;

            averageModeInProgress = false;
            startPause();

            olcum50 = Math.Round(viewModel.PeakCursorY, 5);
            label3.Content = olcum50.ToString();
            rpm50 = (double)anlikrpm;
            label4.Content = rpm50.ToString();
            watt50 = (double)anlikwatt;
            label13.Content = watt50.ToString();

            fanKontrol(false, false);

            //////
            long testId = -1;

            if (Settings.IsAutoSaveOn)
            {
                testId = saveToDatabase();
            }
            else
            {
                if (MessageBox.Show("Sonuç veritabanına kaydedilsin mi?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    testId = saveToDatabase();
                }
            }

            testResultAdapter.TemporaryTestId = testId;

            //bool isPassed = viewModel.ToleranceState == FftViewModel.ToleranceStates.Inside;

            bool isPassed = false;

            if (olcum100 < Convert.ToInt16(ayarlar.Default.level100max))
            {
                if (rpm100 > Convert.ToInt16(ayarlar.Default.rpm100min) && rpm100 < Convert.ToInt16(ayarlar.Default.rpm100max))
                {
                    if (olcum50 < Convert.ToInt16(ayarlar.Default.level50max))
                    {
                        if (rpm50 > Convert.ToInt16(ayarlar.Default.rpm50min) && rpm50 < Convert.ToInt16(ayarlar.Default.rpm50max))
                        {
                            if (watt100 < Convert.ToInt16(ayarlar.Default.watt100max) && watt100 > Convert.ToInt16(ayarlar.Default.watt100min))
                            {
                                if (watt50 < Convert.ToInt16(ayarlar.Default.watt50max) && watt50 > Convert.ToInt16(ayarlar.Default.watt50min))
                                {
                                    isPassed = true;
                                }
                            }                                
                        }
                    }
                }
            }

                if (isPassed)
                {
                //if (MessageBox.Show("Numune geçti. Etiket yazdırılacak.", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                //{
                    printResult();
                    label10.Content = "Test OK";
                label_Sonuc.Content = "TEST OK";
                label_Sonuc.Background = Brushes.Green;
                //}
                }
                else
                {
                    label10.Content = "Test FAIL";
                label_Sonuc.Content = "TEST FAIL";
                label_Sonuc.Background = Brushes.Red;
            }
            label11.Content = "Test Tamamlandı";
            btnStartAveraging.Visibility = System.Windows.Visibility.Visible;
        }

        private List<double[]> listToBeAveraged = new List<double[]>();
        private bool averageModeInProgress;
        private bool averageMode;
        private long saveToDatabase()
        {
            using (DbModel oe = new DbModel())
            {
                var tr = new TestRecord();

                testResultAdapter.Metadata.Operator = "9001047115";
                testResultAdapter.Metadata.LoadingConditions = watt100.ToString();
                testResultAdapter.Metadata.Mounting = watt50.ToString();

                tr.Accessories = testResultAdapter.Metadata.AttachmentsAndAccessories;
                tr.AlpplasMetadata1 =  testResultAdapter.Metadata.AlpplasMetadata1;
                tr.AlpplasMetadata2 =  testResultAdapter.Metadata.AlpplasMetadata2;
                tr.AlpplasMetadata3 = testResultAdapter.Metadata.AlpplasMetadata3;
                tr.Loading = testResultAdapter.Metadata.LoadingConditions;
                tr.Model = rpm50.ToString(); //testResultAdapter.Metadata.Model;
                tr.Mounting = testResultAdapter.Metadata.Mounting;
                tr.Operator =  testResultAdapter.Metadata.Operator;
                tr.PowerSupply = testResultAdapter.Metadata.PowerSource;
                tr.SerialNumber = olcum50.ToString(); //testResultAdapter.Metadata.SerialNumber;
                tr.TestDateTime = testResultAdapter.Metadata.DateTimeUnformatted;
                tr.TestResult = testResultAdapter.IsInTolerance;

                tr.Rpm = rpm100; //viewModel.PeakCursorX;
                tr.Level = olcum100;

                oe.TestRecords.Add(tr);
                oe.SaveChangesAsync();

                return tr.Id;
            }
        }

        // Ölçümün hemen arkasından kullanılan metod
        private void printResult()
        {
            TagPrintDialog.PrintQueue = new PrintQueue(new PrintServer(), Settings.SelectedPrinter);

            //MainWindow.TagPrintDialog.PrintQueue.Name = settings.SelectedPrinter;

            // yeni sayfa üret, sayfa içindeki FixedPage'i al
            var printPage = new PrintPage(testResultAdapter);
            var fixedPage = printPage.fixedPage;

            TagPrintDialog.PrintVisual(fixedPage, "Etiket");
        }

        // Veritabanından tekrar yazdırıldığında kullanılan metod
        public static void PrintResult(TestRecord testRecord, Settings settings)
        {
            TagPrintDialog.PrintQueue = new PrintQueue(new PrintServer(), settings.SelectedPrinter);

            // yeni sayfa üret, sayfa içindeki FixedPage'i al
            var printPage = new PrintPage(testRecord);
            var fixedPage = printPage.fixedPage;

            // yazdır

            TagPrintDialog.PrintVisual(fixedPage, "Etiket");
        }

        //Veritabanını Aç
        private void btnOpenDatabase_Click(object sender, RoutedEventArgs e)
        {
            var asd = new pass();
            asd.ShowDialog();
             
        }
    }
}
