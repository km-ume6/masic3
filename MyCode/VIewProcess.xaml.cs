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

        private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            viewModel.CheckedIsRunning();
        }
    }
}