using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ConvergentSeries
{

    public interface HelpsView
    {

        void Show();
        void ViewHelpsList(List<OneLineOfHelpList> list);
    }

    /// <summary>
    /// Логика взаимодействия для Helps.xaml
    /// </summary>
    public partial class Helps : Window, HelpsView
    {
        private  IModel _model = null;
        private  IControllerHelps _controllerhelps = null;

        private IController ownercontroller;
        public Helps()
        {
            InitializeComponent();
           
        }

        public  Helps(object sender, IModel model)
        {
            _model = model;
            _controllerhelps = new ControllerHelps(_model);
            InitializeComponent();
            _model.HelpFilled += _model_HelpFilled;
            _controllerhelps.FillHelps();
            ownercontroller = (IController)sender;
            this.Closing += Helps_Closing;
        }

        private void Helps_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _model.HelpFilled -= _model_HelpFilled;
            _model = null;
            _controllerhelps = null;  
            ownercontroller.DisposeHelpsView();
            GC.Collect();
        }

        private void _model_HelpFilled(object sender, HelpFilledEventArgs e)
        {
            ViewHelpsList(e.List);
        }

        public void ViewHelpsList(List<OneLineOfHelpList> list)
        {
            dataGridHelpsView.ItemsSource = list;
        }
    }
}
