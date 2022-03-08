using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Win32;

namespace ConvergentSeries
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMainView
    {

        private readonly IModel _model = null;
        private readonly IController _controller = null;


        public MainWindow()
        {
            InitializeComponent();

            _model = new MainModel();
            _controller = new Controller(_model);

            _model.Addedmembervalue += _model_Addedmembervalue;
            _model.Removedmembervalue += _model_Removedmembervalue;
            _model.SolvingProcess += _model_SolvingProcess;
            _model.StartSolvingProcess += _model_StartSolvingProcess;
            _model.EndSolvingProcess += _model_EndSolvingProcess;
            _model.CancelSolvingProcess += _model_CancelSolvingProcess;
            _model.ErrorSolving += _model_ErrorSolving;
            TrasingResultsDataGrid.ColumnWidth = new DataGridLength(2, DataGridLengthUnitType.Star);

        }

        private void _model_StartSolvingProcess(object sender, ValueAnswerEventArgs e)
        {
            if (Dispatcher.CheckAccess())
            {
                textBoxFullDescription.Clear();
                textBoxFullDescription.Text = Convert.ToString(e.TextDescriprtion);
                GC.Collect();

            }
            else Dispatcher.Invoke((Action)(() =>
            {
                textBoxFullDescription.Clear();
                textBoxFullDescription.Text = Convert.ToString(e.TextDescriprtion);
                GC.Collect();

            }));
        }

        private void _model_ErrorSolving(object sender, ErrorEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                buttonSolve.Content = "Solve";
                gradationcriteriapanel.IsEnabled = true;
                textBoxSeriesMember.IsEnabled = true;
                textBoxNvalue.IsEnabled = true;
                buttonSolve.Click -= buttonStop_click;
                buttonSolve.Click += buttonSolve_click;
                ToolTip t = (ToolTip)textBoxSeriesMember.ToolTip;
                if (t == null)
                    t = new System.Windows.Controls.ToolTip();
                t.StaysOpen = true;
                t.PlacementTarget = textBoxSeriesMember;
                t.Placement = System.Windows.Controls.Primitives.PlacementMode.RelativePoint;
                t.VerticalOffset = -30;
                t.HorizontalOffset = 55;
                textBoxSeriesMember.ToolTip = t;
                textBoxSeriesMember.BorderBrush = Brushes.Red; 
                t.Content = e.Message;
                t.BorderBrush = Brushes.Red;
                t.IsOpen = true;
            }));
        }

        private void _model_CancelSolvingProcess(object sender, ValueAnswerEventArgs e)
        {
            if (Dispatcher.CheckAccess())
            {
                textBoxFullDescription.Clear();          
                textBoxFullDescription.Text = e.TextDescriprtion.ToString();
                TrasingResultsDataGrid.ItemsSource = e.AllGridDescription;

                GC.Collect();
                progBar.Value = e.Progress;
            }
            else Dispatcher.Invoke((Action)(() =>
            {
                textBoxFullDescription.Clear();
                textBoxFullDescription.Text = e.TextDescriprtion.ToString();
                TrasingResultsDataGrid.ItemsSource = e.AllGridDescription;

                GC.Collect();
                progBar.Value = e.Progress;
            }));
        }

        private void _model_EndSolvingProcess(object sender, ValueAnswerEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
                       {
                           textBoxFullDescription.Clear();
                           using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                           {
                               using (System.IO.StreamWriter sr = new System.IO.StreamWriter(ms))
                               {

                                   //sr.Write(e.TextDescriprtion);
                                   //sr.Flush();
                                   //ms.Seek(0, System.IO.SeekOrigin.Begin);
                                   textBoxFullDescription.DataContext = e.TextDescriprtion;
                                   //textBoxFullDescription.Text = Convert.ToString(e.TextDescriprtion);
                                   GC.Collect();
                               }
                           }

                           TrasingResultsDataGrid.ItemsSource = e.AllGridDescription;
                           progBar.Value = e.Progress;
                           buttonSolve.Content = "Solve";
                           gradationcriteriapanel.IsEnabled = true;
                           textBoxSeriesMember.IsEnabled = true;
                           textBoxNvalue.IsEnabled = true;
                           buttonSolve.Click -= buttonStop_click;
                           buttonSolve.Click += buttonSolve_click;
                           GC.Collect();
                       }));
        }

        private void _model_SolvingProcess(object sender, ValueAnswerEventArgs e)
        {

            if (Dispatcher.CheckAccess())
            {
                progBar.Value = e.Progress;
            }
            else Dispatcher.Invoke((Action)(() =>
            {
                progBar.Value = e.Progress;
            }));

        }

        private void _model_Removedmembervalue(object sender, RemoveMemberValueEventArgs e)
        {
            textBoxSeriesMember.Text = e.AllMemberValue;
            SetFocusAndCursor(e.StartIndex);
        }

        private void _model_Addedmembervalue(object sender, MemberValueEventArgs e)
        {
            textBoxSeriesMember.Text= e.AllMemberValue;
            SetFocusAndCursor(e.NewCurosrvalue);
        }

        public void SetFocusAndCursor(int cursorvalue)
        {
            textBoxSeriesMember.Focus();
            textBoxSeriesMember.SelectionStart = cursorvalue;
        }


     

        // View will set the associated controller, this is how view is linked to the controller.


        private void buttonvalueadd_Click(object sender, RoutedEventArgs e)
        {
            _controller.AddMemberValueFromButton(Convert.ToInt32(((Button)sender).Tag));
        }

        private void textBoxSeriesMember_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (_controller != null)
                _controller.SetCursorValue(textBoxSeriesMember.SelectionStart);
        }

        private void textBoxSeriesMember_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_controller != null)
                _controller.MemberPreviewTextboxInput(((TextBox)sender).Text);
            ToolTip t = (ToolTip)((TextBox)sender).ToolTip;
            if (t == null)
                t = new System.Windows.Controls.ToolTip();
            t.BorderBrush = Brushes.Gray;
            t.IsOpen = false;
            t.Content = "";
            textBoxSeriesMember.BorderBrush = Brushes.Gray;
        }

        private void buttonvalueclear_Click(object sender, RoutedEventArgs e)
        {
            _controller.RemoveMemberValueFromButton(Convert.ToInt32(((Button)sender).Tag));
        }

        private void buttonSolve_click(object sender, RoutedEventArgs e)
        {
            if (!IsErrorInFields())
            {

                ToolTip t = (ToolTip)((TextBox)textBoxSeriesMember).ToolTip;
                if (t == null)
                    t = new System.Windows.Controls.ToolTip();
                t.BorderBrush = Brushes.Gray;
                t.IsOpen = false;
                t.Content = "";
                textBoxSeriesMember.BorderBrush = Brushes.Gray;


                _controller.TakeOfErrorInFillFields();
                progBar.Value = 0;
                ((Button)sender).Content = "Stop";
                gradationcriteriapanel.IsEnabled = false;
                textBoxSeriesMember.IsEnabled = false;
                textBoxNvalue.IsEnabled = false;
                ((Button)sender).Click -= buttonSolve_click;
                ((Button)sender).Click += buttonStop_click;
                textBoxFullDescription.Text = string.Empty;
                TrasingResultsDataGrid.ItemsSource = null; 
                _controller.StartSolvingSeries();
                FilterMembertextBoxFrom.Text = string.Empty;
                FilterMembertextBoxTo.Text = string.Empty;
            }
            else
            {
                _controller.SetErrorInFillFields();
            }
        }
        private void buttonStop_click(object sender, RoutedEventArgs e)
        {
            progBar.Value = 0;
            ((Button)sender).Content = "Solve";
            gradationcriteriapanel.IsEnabled = true;
            textBoxSeriesMember.IsEnabled = true;
            textBoxNvalue.IsEnabled = true;
            ((Button)sender).Click -= buttonStop_click;
            ((Button)sender).Click += buttonSolve_click;
            _controller.CancelSolving();
        }

        private void radiobitercount_Checked(object sender, RoutedEventArgs e)
        {
            if (_controller != null)
            {
                _controller.SetGraduationCriterionType(GraduationCriterion.OnIterationsCount);
                textBoxIterationCount.IsEnabled = true;
                textBoxAccuracy.IsEnabled = false;
            }
        }

        private void radiobaccuracy_Checked(object sender, RoutedEventArgs e)
        {
            if (_controller != null)
                _controller.SetGraduationCriterionType(GraduationCriterion.OnAccuracy);
            textBoxIterationCount.IsEnabled = false;
            textBoxAccuracy.IsEnabled = true;
        }

        private void textBoxIterationCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            ToolTip t = (ToolTip)((TextBox)sender).ToolTip;
            if (t == null)
            {
                t = new System.Windows.Controls.ToolTip();
            }
            t.PlacementTarget = ((TextBox)sender);
            t.Placement = System.Windows.Controls.Primitives.PlacementMode.RelativePoint;
            t.VerticalOffset = -27;
            ((TextBox)sender).ToolTip = t;
            t.StaysOpen = true;
            if(!IsErrorFillIterCountField(sender))
            {
                t.Content = "Right number";
                t.BorderBrush = Brushes.Gray;
                t.IsOpen = false;
                ((TextBox)sender).BorderBrush = Brushes.Gray;
            }
            else
            {
                t.Content = "Wrong number";
                t.BorderBrush = Brushes.Red;
                t.IsOpen = true;
                ((TextBox)sender).BorderBrush = Brushes.Red;
            }
        }

        private void textBoxAccuracy_TextChanged(object sender, TextChangedEventArgs e)
        {
            ToolTip t = (ToolTip)((TextBox)sender).ToolTip;
            if (t == null)
            {
                t = new System.Windows.Controls.ToolTip();
            }
            t.PlacementTarget = ((TextBox)sender);
            t.Placement = System.Windows.Controls.Primitives.PlacementMode.RelativePoint;
            t.VerticalOffset = 27;
            ((TextBox)sender).ToolTip = t;
            t.StaysOpen = true;
          if(!IsErrorFillAccuracyField(sender))
             { 
                t.Content = "Right number";
                t.BorderBrush = Brushes.Gray;
                t.IsOpen = false;
                ((TextBox)sender).BorderBrush = Brushes.Gray;
            }
            else
            {
                t.Content = "Wrong number";
                t.BorderBrush = Brushes.Red;
                t.IsOpen = true;
                ((TextBox)sender).BorderBrush = Brushes.Red;
            }
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ToolTip t = (ToolTip)((TextBox)sender).ToolTip;
            if (t == null)
            {
                t = new System.Windows.Controls.ToolTip();
            }
            t.PlacementTarget = ((TextBox)sender);
            t.Placement = System.Windows.Controls.Primitives.PlacementMode.RelativePoint;
            t.HorizontalOffset = 27;
            t.StaysOpen = true;
            ((TextBox)sender).ToolTip = t;
            if(!isErrorFillNField(sender))
            { 
                t.Content = "Right number";
                t.BorderBrush = Brushes.Gray;
                ((TextBox)sender).BorderBrush = Brushes.Gray;
                t.IsOpen = false;
            }
             else
            {
                ((TextBox)sender).BorderBrush = Brushes.Red;
                t.Content = "Wrong number";
                t.BorderBrush = Brushes.Red;
                t.IsOpen = true;
            }     
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void menuItexSaveToTxt_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog filedialog = new SaveFileDialog();
            filedialog.DefaultExt = "txt";
            filedialog.Filter = "Text files (*.txt)| *.txt| All(*.*) | *.* ";
            filedialog.AddExtension = true;
            filedialog.FileOk += Filedialog_FileOk;
            if(textBoxFullDescription.Text.Length>0)
            filedialog.ShowDialog();
            else
            { MessageBox.Show("Answer description is empty"); }
        }

        private void Filedialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string path = ((SaveFileDialog)sender).FileName;
            _controller.WriteToFile(path);
        }

        private void MenuHelp_click(object sender, RoutedEventArgs e)
        {
            _controller.ShowHelpsView(this);
        }

        private void MenuAbout_click(object sender, RoutedEventArgs e)
        {       
            string AssemblyDescription = "Program for finding the sum of a convergent series. \nAuthor: Glushakov Max";
            MessageBox.Show(AssemblyDescription,Assembly.GetExecutingAssembly().GetName().Name);
        }

        private void menuItemClearDescription_Click(object sender, RoutedEventArgs e)
        {
            _controller.ClearAllResultsDescription();
            
            progBar.Value = 0;
            TrasingResultsDataGrid.ItemsSource = null;
            textBoxFullDescription.Clear();
            FilterMembertextBoxFrom.Text = string.Empty;
            FilterMembertextBoxTo.Text = string.Empty;
            GC.Collect();
        }

        public bool IsErrorInFields()
        {
         bool isError = IsErrorFillAccuracyField(textBoxAccuracy);
            if (isError)
                return true;
            isError = IsErrorFillIterCountField(textBoxIterationCount);
            if (isError)
                return true;
            isError = isErrorFillNField(textBoxNvalue);
            if (isError)
                return true;
            return false;
        }

        public bool IsErrorFillAccuracyField(object sender)
        {
            try
            {
                double accuracy = Convert.ToDouble(((TextBox)sender).Text);
                if (_controller != null && accuracy > 0 && accuracy < 1)
                {
                    _controller.SetAccuracy(accuracy);
                }
                return false;
            }
            catch(Exception)
            { return true; }
        }

        public bool IsErrorFillIterCountField(object sender)
        {
            try
            {
                int count = Convert.ToInt32(((TextBox)sender).Text);
                if (_controller != null && count > 0)
                {
                    _controller.SetIterationCount(count);
                }
                return false;
            }
            catch (Exception)
            { return true; }
        }

        public bool isErrorFillNField(object sender)
        {
            try
            {
                int Nvalue = Convert.ToInt32(((TextBox)sender).Text);
                if (_controller != null)
                {
                    _controller.SetStartN(Nvalue);
                }
                return false;
            }
            catch (Exception)
            { return true; }
        }

        private void FilterVlueChanged(object sender, TextChangedEventArgs e)
        {
            double filterFrom = 0;
            double filterTo = 0;
            try
            {
                if (FilterMembertextBoxFrom.Text.Length > 0 && FilterMembertextBoxTo.Text.Length > 0)
                {
                    filterFrom = Convert.ToDouble(FilterMembertextBoxFrom.Text);
                    filterTo = Convert.ToDouble(FilterMembertextBoxTo.Text);
                    var yourCostumFilter = new Predicate<object>(item => ((OneIterDescrtiption)item).Member <= filterTo && ((OneIterDescrtiption)item).Member >= filterFrom);
                    TrasingResultsDataGrid.Items.Filter = yourCostumFilter;
                }
                else
                    if(TrasingResultsDataGrid.Items.Filter!=null)
                    TrasingResultsDataGrid.Items.Filter = null;
            }
            catch
            {
                if (TrasingResultsDataGrid.Items.Filter != null)
                    TrasingResultsDataGrid.Items.Filter = null;
            }
        }
    }
}
