using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConvergentSeries
{
    interface IMainView
    {

        void SetFocusAndCursor(int cursorvalue);
        bool IsErrorInFields();

        bool IsErrorFillAccuracyField(object sender);
        bool IsErrorFillIterCountField(object sender);
        bool isErrorFillNField(object sender);
    }
}
