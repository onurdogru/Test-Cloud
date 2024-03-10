using ProPlan.QcComponents.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ProPlan2250QC
{
    public class Settings : AbstractViewModel
    {
        public Settings(FftViewModel viewModel)
        {
            this.ViewModel = viewModel;
        }

        public Settings ShallowCopy()
        {
            Settings copy = (Settings)this.MemberwiseClone();
            copy.ViewModel = this.ViewModel.ShallowCopy();
            if (this.SelectedPrinter != null)
            {
                copy.SelectedPrinter = string.Copy(this.SelectedPrinter);

            }            return copy;
        }

        public string SelectedPrinter { get; set; }

        public string DeviceIp { get; set; }

        public int MeasurementDuration { get; set; }

        public int PreDelayDuration { get; set; }

        public double HighPassFrequency{ get; set; }

        public static List<string> OnOff { get; } = new List<string> { "Kapalı", "Açık" };

        public int AutoSaveSelection { get; set; }

        public bool IsAutoSaveOn
        {
            get
            {
                return AutoSaveSelection == 1 ? true : false;
            }
            set
            {
                if (value)
                {
                    AutoSaveSelection = 1;
                }
                else
                {
                    AutoSaveSelection = 0;
                }
            }
        }

        public static List<string> DisplayTypes { get; } = new List<string> { "Bildirim ekranı", "Spektrum" };

        private int _displayType;

        public int DisplayType
        {
            get { return _displayType; }
            set
            {
                if (_displayType != value)
                {
                    _displayType = value;
                    DisplayTypeChanged?.Invoke(this, null);
                }
            }
        }


        public FftViewModel ViewModel;

        private double _sidebandWidth;
        public double SidebandWidth
        {
            get { return _sidebandWidth; }
            set
            {
                if (_sidebandWidth != value)
                {
                    _sidebandWidth = value;
                    UpdateViewModel();
                }
            }
        }

        private double _detectLow;
        public double DetectLow
        {
            get { return _detectLow; }
            set
            {
                if (_detectLow != value)
                {
                    _detectLow = value;
                    UpdateViewModel();
                }
            }
        }

        private double _detectHigh;
        public double DetectHigh
        {
            get { return _detectHigh; }
            set
            {
                if (_detectHigh != value)
                {
                    _detectHigh = value;
                    UpdateViewModel();
                }
            }
        }

        private double _toleranceMinX;
        public double ToleranceMinX
        {
            get { return _toleranceMinX; }
            set
            {
                if (_toleranceMinX != value)
                {
                    _toleranceMinX = value;
                    UpdateViewModel();
                }
            }
        }

        private double _toleranceMaxX;
        public double ToleranceMaxX
        {
            get { return _toleranceMaxX; }
            set
            {
                if (_toleranceMaxX != value)
                {
                    _toleranceMaxX = value;
                    UpdateViewModel();
                }
            }
        }

        private double _toleranceMinY;
        public double ToleranceMinY
        {
            get { return _toleranceMinY; }
            set
            {
                if (_toleranceMinY != value)
                {
                    _toleranceMinY = value;
                    UpdateViewModel();
                }
            }
        }

        private double _toleranceMaxY;
        public double ToleranceMaxY
        {
            get { return _toleranceMaxY; }
            set
            {
                if (_toleranceMaxY != value)
                {
                    _toleranceMaxY = value;
                    UpdateViewModel();
                }
            }
        }

        public List<string> Presets { get; set; }

        private string _selectedPreset;
        public string SelectedPreset
        {
            get { return _selectedPreset; }
            set
            {
                _selectedPreset = value;
                LoadSelectedPreset(_selectedPreset);
            }
        }


        public void LoadPresets()
        {
            //var hwnd = new WindowInteropHelper(this).Handle;
            //SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);

            try
            {
                // Loading from the Project xml file
                var xmlDocument = XDocument.Load("Presets.xml");

                // Query the data and write out
                var queryId = from c in xmlDocument.Root.Descendants("preset")
                              select c.Attribute("id").Value;

                Presets = queryId.ToList();

                queryId = from c in xmlDocument.Root.Descendants("preset")
                          where c.Attribute("isSelected").Value == "true"
                          select c.Attribute("id").Value;

                //SelectPreset(queryId.First());
                SelectedPreset = queryId.FirstOrDefault();

                OnPropertyChanged(nameof(SelectedPreset));
                OnPropertyChanged(nameof(Presets));
            }
            catch (Exception ex)
            {
                
            }
        }

        private void LoadSelectedPreset(string selectedPreset)
        {
            try
            {
                // Loading from the Project xml file
                var xmlDocument = XDocument.Load("Presets.xml");

                // Query the data and write out
                var queryId = from c in xmlDocument.Root.Descendants("preset")
                              where c.Attribute("id").Value == selectedPreset
                              select c;

                SidebandWidth = double.Parse(queryId.First().Attribute(nameof(SidebandWidth)).Value);
                DetectLow = double.Parse(queryId.First().Attribute(nameof(DetectLow)).Value);
                DetectHigh = double.Parse(queryId.First().Attribute(nameof(DetectHigh)).Value);
                ToleranceMinX = double.Parse(queryId.First().Attribute(nameof(ToleranceMinX)).Value);
                ToleranceMaxX = double.Parse(queryId.First().Attribute(nameof(ToleranceMaxX)).Value);
                ToleranceMinY = double.Parse(queryId.First().Attribute(nameof(ToleranceMinY)).Value);
                ToleranceMaxY = double.Parse(queryId.First().Attribute(nameof(ToleranceMaxY)).Value);
                UpdateProperties();
            }
            catch (Exception ex)
            {

            }
        }

        public void LoadSettings()
        {
            //var hwnd = new WindowInteropHelper(this).Handle;
            //SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);

            try
            {
                // Loading from the Project xml file
                var xmlDocument = XDocument.Load("Settings.xml");

                // Query the data and write out
                var queryId = from c in xmlDocument.Root.Descendants("settings")
                              select c;

                DeviceIp = queryId.First().Attribute(nameof(DeviceIp)).Value;
                MeasurementDuration = int.Parse(queryId.First().Attribute(nameof(MeasurementDuration)).Value);
                PreDelayDuration = int.Parse(queryId.First().Attribute(nameof(PreDelayDuration)).Value);
                HighPassFrequency = double.Parse(queryId.First().Attribute(nameof(HighPassFrequency)).Value);
                IsAutoSaveOn = bool.Parse(queryId.First().Attribute(nameof(IsAutoSaveOn)).Value);
                SelectedPrinter = queryId.First().Attribute(nameof(SelectedPrinter)).Value;
                DisplayType = int.Parse(queryId.First().Attribute(nameof(DisplayType)).Value);
            }
            catch (Exception ex)
            {

            }
        }

        public void UpdateProperties()
        {
            OnPropertyChanged(nameof(SidebandWidth));
            OnPropertyChanged(nameof(ToleranceMaxX));
            OnPropertyChanged(nameof(ToleranceMaxY));
            OnPropertyChanged(nameof(ToleranceMinX));
            OnPropertyChanged(nameof(ToleranceMinY));
            OnPropertyChanged(nameof(DetectLow));
            OnPropertyChanged(nameof(DetectHigh));
        }

        public void UpdateViewModel()
        {
            ViewModel.PeakCursorWidth = SidebandWidth;
            ViewModel.ToleranceCheckAnnotationMinX = ToleranceMinX;
            ViewModel.ToleranceCheckAnnotationMaxX = ToleranceMaxX;
            ViewModel.ToleranceCheckAnnotationMinY = ToleranceMinY;
            ViewModel.ToleranceCheckAnnotationMaxY = ToleranceMaxY;
            ViewModel.Profile.DetectFreqLow = DetectLow;
            ViewModel.Profile.DetectFreqHigh = DetectHigh;
        }

        public event EventHandler DisplayTypeChanged;
    }
}
