using System.Diagnostics;

namespace masic3.MyCode
{
    public partial class ViewProcess : ContentPage
    {
        ViewModelProcess viewModel;

        public ViewProcess()
        {
            InitializeComponent();

            viewModel = (ViewModelProcess)this.BindingContext;
        }

        private void CheckBox_CheckedChanged_IsRunning(object sender, CheckedChangedEventArgs e)
        {
            viewModel.CheckedIsRunning();
        }

        private void CheckBox_CheckedChanged_IsAdvance(object sender, CheckedChangedEventArgs e)
        {
            viewModel.CheckedIsAdvance();
        }
    }
}