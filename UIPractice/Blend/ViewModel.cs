using System;
using System.Windows.Input;
using System.Windows.Media;

namespace UIPractice
{
    /// <summary>
    /// Logic for application ViewModel
    /// </summary>
    public partial class ViewModel
    {
        public Color TopColor { get; set; }
        public Color BottomColor { get; set; }
        public DelegateCommand ButtonClicked { get; }

        public ViewModel()
        {
            TopColor = Color.FromRgb(17, 102, 157);
            BottomColor = Color.FromRgb(18, 57, 87);
            ButtonClicked = new DelegateCommand((p) =>
            {
                Console.WriteLine("Button clicked");
            });
        }
    }
}
