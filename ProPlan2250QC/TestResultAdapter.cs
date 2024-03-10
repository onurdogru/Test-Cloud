using ProPlan.QcComponents.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ProPlan2250QC
{
    public class TestResultAdapter : AbstractViewModel
    {
        public TestResultAdapter(FftViewModel fftViewModel, Metadata metadata)
        {
            this.ViewModel = fftViewModel;
            this.Metadata = metadata;
        }

        public void UpdateProperties()
        {
            OnPropertyChanged(nameof(PeakFreq));
            OnPropertyChanged(nameof(PeakRPM));
            OnPropertyChanged(nameof(PeakValue));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(BorderBackground));
        }

        public bool IsIndeterminate = true;

        public string PeakFreq
        {
            get
            {
                var val = ViewModel.PeakCursorX > int.MinValue ? ViewModel.PeakCursorX.ToString("0") : "---";
                return $"Frekans: {val}";
            }
        }

        public string PeakRPM
        {
            get
            {
                var val = ViewModel.PeakCursorX > int.MinValue ? ViewModel.PeakCursorX.ToString("0") : "---";
                return $"Devir: {val}";
            }
        }

        public string PeakValue
        {
            get
            {
                var val = ViewModel.PeakCursorY > int.MinValue ? ViewModel.PeakCursorY.ToString("0.00") : "---";
                return $"Seviye: {val}";
            }
        }

        public bool IsInTolerance
        {
            get { return ViewModel.ToleranceState == FftViewModel.ToleranceStates.Inside; }
        }

        public string Status    
        {
            get
            {
                if (IsInTolerance)
                {
                    return "GEÇTİ";
                }
                else
                {
                    return "KALDI";
                }
            }
        }

        public string StatusForPrint
        {
            get
            {
                if (IsInTolerance)
                {
                    return "Test OK";
                }
                else
                {
                    return "Test NOK";
                }
            }
        }

        public Brush BorderBackground
        {
            get
            {
                if (IsIndeterminate)
                {
                    return Brushes.LightGray;
                }
                else if (IsInTolerance)
                {
                    return Brushes.Green;
                }
                else
                {
                    return Brushes.Red;
                }
            }
        }

        public FftViewModel ViewModel { get; private set; }
        public Metadata Metadata { get; private set; }

        public long TemporaryTestId { get; set; }

    }
}
