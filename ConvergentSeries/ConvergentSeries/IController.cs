using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ConvergentSeries
{

    public interface IController
    {
        void SetCursorValue(int value);
        void AddMemberValueFromButton(int dictoinarykey);
        void RemoveMemberValueFromButton(int dictionarykey);
        void MemberPreviewTextboxInput(string membervalueall);
        void StartSolvingSeries();
        void SetAccuracy(double accuracy);
        void SetIterationCount(int count);
        void SetGraduationCriterionType(GraduationCriterion value);
        void SetStartN(int n);
        void CancelSolving();
        void ShowHelpsView(Window owner);
        void DisposeHelpsView();
        void ClearAllResultsDescription();
        void SetErrorInFillFields();
        void TakeOfErrorInFillFields();
        void WriteToFile(string path);

    }

    class Controller : IController
    {

        private readonly IModel _model;

        HelpsView view = null;

        public Controller(IModel projectModel)
        {
            if (projectModel == null)
                throw new ArgumentNullException(
                    "projectModel is Null");
            _model = projectModel;
        }



        public void SetCursorValue(int value)
        {
            _model.SetCursorValue(value);
        }

        public void AddMemberValueFromButton(int dictoinarykey)
        {
            _model.AddMemberValueFromButton(dictoinarykey);
        }

        public void MemberPreviewTextboxInput(string membervalueall)
        {
            _model.MemberPreviewTextboxInput(membervalueall);
        }
        public void RemoveMemberValueFromButton(int dictionarykey)
        {
            _model.RemoveMemberValueFromButton(dictionarykey);
        }

        public void StartSolvingSeries()
        {
            _model.StartSolvingSeries();
        }

        public void SetAccuracy(double accuracy)
        {
            _model.SetAccuracy(accuracy);
        }

        public void SetIterationCount(int count)
        {
            _model.SetIterationCount(count);
        }

        public void SetGraduationCriterionType(GraduationCriterion value)
        {
            _model.SetGraduationCriterionType(value);
        }

        public void SetStartN(int n)
        {
            _model.SetStartN(n);
        }
        public void CancelSolving()
        {
            _model.CancelSolving();
        }

        public void ShowHelpsView(Window owner)
        {
            if(view ==null)
                view  = new Helps(this, _model);
            view.Show();
            
        }
        public void DisposeHelpsView()
        {
            view = null;
            GC.Collect();
        }
        public void ClearAllResultsDescription()
        { _model.ClearAllResultsDescription(); }

        public  void WriteToFile(string path)
        {
            _model.WriteToFile( path);
        }

        public void SetErrorInFillFields()
        {
            _model.SetErrorInFillFields();
        }

        public void TakeOfErrorInFillFields()
        {
            _model.TakeOfErrorInFillFields();
        }
    }
}
