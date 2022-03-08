using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConvergentSeries
{
    public interface IControllerHelps
    {
        void FillHelps();
    }
        class ControllerHelps: IControllerHelps
    {

        private readonly IModel _model;

        public ControllerHelps(IModel projectModel)
        {
            if (projectModel == null)
                throw new ArgumentNullException(
                    "projectModel is Null");
            _model = projectModel;
        }

        public void FillHelps()
        {
            _model.FillHelpView();
           
        }
    }
}
