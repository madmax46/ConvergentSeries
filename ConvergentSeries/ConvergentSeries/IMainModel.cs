using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

namespace ConvergentSeries
{
    public enum GraduationCriterion
    {
        OnIterationsCount,
        OnAccuracy
    }
    public interface IModel
    {
         void SetCursorValue(int value);
         event EventHandler<MemberValueEventArgs> Addedmembervalue;
         event EventHandler<RemoveMemberValueEventArgs> Removedmembervalue;
         event EventHandler<ValueAnswerEventArgs> SolvingProcess;
         event EventHandler<ValueAnswerEventArgs> StartSolvingProcess;
         event EventHandler<ValueAnswerEventArgs> EndSolvingProcess;
         event EventHandler<ValueAnswerEventArgs> CancelSolvingProcess;
         event EventHandler<ErrorEventArgs> ErrorSolving;
         
         event EventHandler<HelpFilledEventArgs> HelpFilled; 


        //в записку запихнуть интерфейсы, события 
        void AddMemberValueFromButton(int dictionarykey);
        void RemoveMemberValueFromButton(int dictionarykey);
        void MemberPreviewTextboxInput(string membervalueall);
        void SetAccuracy(double accuracy);
        void SetIterationCount(int count);
        void SetStartN(int n);
        void SetGraduationCriterionType(GraduationCriterion value);
        void CancelSolving();
        void StartSolvingSeries();
        void FillHelpView();
        void SetErrorInFillFields();
        void TakeOfErrorInFillFields();
        void ClearAllResultsDescription();
        void WriteToFile(string path);

    }
    class MainModel: IModel
    {

        private Dictionary<int,string> ButtonsDictionary;


        public event EventHandler<MemberValueEventArgs> Addedmembervalue = delegate { };
        public event EventHandler<RemoveMemberValueEventArgs> Removedmembervalue = delegate { };

        public event EventHandler<ValueAnswerEventArgs> SolvingProcess = delegate { };
        public event EventHandler<ValueAnswerEventArgs> StartSolvingProcess = delegate { };
        public event EventHandler<ValueAnswerEventArgs> EndSolvingProcess = delegate { };
        public event EventHandler<ValueAnswerEventArgs> CancelSolvingProcess = delegate { };

        public event EventHandler<ErrorEventArgs> ErrorSolving = delegate { };


        public event EventHandler<HelpFilledEventArgs> HelpFilled = delegate { };


        private string seriesMemberValue;

        private int cursorPosition;
        private string PreperedSeriesForInsertingN;
        private int StartN = 1;

        private bool isStartSolving = false;
        private const int MaxCountofIdentIaliter = 30;

        private GraduationCriterion GradationCriterion = GraduationCriterion.OnIterationsCount;
        private double solvingAccuracy = 0.001;
        private int iterationMaxCount = 20;

        private bool isErrorOnFillFields = false;

        private StringBuilder MainResultsString = new StringBuilder();
        private List<OneIterDescrtiption> AllTracingResults = new List<OneIterDescrtiption>();

        private  CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private  CancellationToken token;


        /// <summary>
        /// Initialize buttons of expression builder /
        /// Инициализирует кнопки отвечающие за кнопки построителя решений
        /// </summary>
        public void InitializeButtonsDictionary()
        {
            ButtonsDictionary = new Dictionary<int, string>();
            ButtonsDictionary.Add(0, "sin(");
            ButtonsDictionary.Add(1, "cos(");
            ButtonsDictionary.Add(2, "tan(");
            ButtonsDictionary.Add(3, "ctg(");
            ButtonsDictionary.Add(4, "abs(");
            ButtonsDictionary.Add(5, "asin(");
            ButtonsDictionary.Add(6, "acos(");
            ButtonsDictionary.Add(7, "atan(");
            ButtonsDictionary.Add(8, "arctg(");
            ButtonsDictionary.Add(9, "exp(");
            ButtonsDictionary.Add(10, "sqrt(");
            ButtonsDictionary.Add(11, "pi");
            ButtonsDictionary.Add(12, "^");
            ButtonsDictionary.Add(13, "ln(");
            ButtonsDictionary.Add(14, "log(");
            ButtonsDictionary.Add(15, "(");
            ButtonsDictionary.Add(16, ")");
            ButtonsDictionary.Add(17, "7");
            ButtonsDictionary.Add(18, "8");
            ButtonsDictionary.Add(19, "9");
            ButtonsDictionary.Add(20, "+");
            ButtonsDictionary.Add(21, "*");
            ButtonsDictionary.Add(22, "4");
            ButtonsDictionary.Add(23, "5");
            ButtonsDictionary.Add(24, "6");
            ButtonsDictionary.Add(25, "-");
            ButtonsDictionary.Add(26, "/");
            ButtonsDictionary.Add(27, "1");
            ButtonsDictionary.Add(28, "2");
            ButtonsDictionary.Add(29, "3");
            ButtonsDictionary.Add(30, "0");
            ButtonsDictionary.Add(31, "n");
            ButtonsDictionary.Add(32, "C");
            ButtonsDictionary.Add(33, "⇐");
        }

        public MainModel()
        {
            Parser.InitializeParser();
            InitializeButtonsDictionary();
            cursorPosition = 0;
            seriesMemberValue = "1/(n*(n+1))";          
        }


        /// <summary>
        /// Set cursor value that moment and wich the sighs will insert /
        /// Устанавливает положение курсора в данный момент и в это положение дальше будут добавляться выражения
        /// </summary>
        /// <param name="value"></param>
        public void SetCursorValue(int value)
        {
            cursorPosition = value;
        }

        /// <summary>
        /// If expression button click this method add button value (return on key dictionary) to string of series member  /
        /// При нажатии на кнопку построителя решений метод добавляет значение этой кнопки получиенное из Dictionary в строку с значением члена ряда
        /// </summary>
        /// <param name="dictionarykey"></param>
        public void AddMemberValueFromButton(int dictionarykey)
        {
            
            if (!isStartSolving && dictionarykey != 32 && dictionarykey != 33)
            {
                string s = ButtonsDictionary[dictionarykey];
                seriesMemberValue = seriesMemberValue.Insert(cursorPosition, s);

                //burn
                Addedmembervalue(this, new MemberValueEventArgs(cursorPosition, s,seriesMemberValue));
            }
        }

        /// <summary>
        /// Delete value from string of series member /
        /// Удаляет значение из строки члена ряда
        /// </summary>
        /// <param name="dictionarykey"></param>
        public void RemoveMemberValueFromButton(int dictionarykey)
        {
            if (!isStartSolving)
            {
                if (dictionarykey == 32)
                {
                    int length = seriesMemberValue.Length;
                    seriesMemberValue = seriesMemberValue.Remove(0, length);
                    Removedmembervalue(this, new RemoveMemberValueEventArgs(0, length,seriesMemberValue));
                }
                else
                {
                    if (dictionarykey == 33)
                    {
                        if (cursorPosition > 0)
                        {
                            seriesMemberValue = seriesMemberValue.Remove(cursorPosition - 1, 1);
                            Removedmembervalue(this, new RemoveMemberValueEventArgs(cursorPosition - 1, 1, seriesMemberValue));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// If user change text with value of series member and not using buttons of expression builder,  value of series member in MainModel change too /
        /// Если пользователь изменяет значение члена ряда вручную, не нажимая на кнопки построителя решений, то член ряда хранящийся в MainModel тоже изменяется
        /// </summary>
        /// <param name="membervalueall"></param>
        public void MemberPreviewTextboxInput(string membervalueall)
        {
            if(!isStartSolving)
            seriesMemberValue = membervalueall;
        }

        /// <summary>
        /// This method start solving process /
        /// Метод начинает процесс решения
        /// </summary>
        public void StartSolvingSeries()
        {
            if (!isStartSolving && !isErrorOnFillFields)
            {
                PreperedSeriesForInsertingN = null;
                ClearAllResultsDescription();
                MainResultsString.Append("Value of Series = " + seriesMemberValue + Environment.NewLine);
                PreperedSeriesForInsertingN = FirstPrepareStringSeries(seriesMemberValue);
                cancellationTokenSource = new CancellationTokenSource();
                token = cancellationTokenSource.Token;
                if (PreperedSeriesForInsertingN != null)
                {
                    bool isError = isErrorInMemberCalcul(PreperedSeriesForInsertingN, StartN);
                    if (!isError)
                    {
                        isStartSolving = true;
                        if (GradationCriterion == GraduationCriterion.OnIterationsCount)
                        {
                            string description = "The Solving starts. Ending criterion is iterations count which equal " + iterationMaxCount + Environment.NewLine;
                            MainResultsString.Append(description);
                            StartSolvingProcess(this, new ValueAnswerEventArgs(0, 0, 0, MainResultsString, null, null, false));
  
                            Task.Factory.StartNew((Action)(() => { MainCalculatingOnIterCount(PreperedSeriesForInsertingN, StartN, token);
                                isStartSolving = false;
                                GC.Collect();
                            }));
                            
                        }
                        else
                        {
                            string changes = "The Solving starts. Ending criterion is accuracy of calculating which equal  " + solvingAccuracy + Environment.NewLine;
                            MainResultsString.Append(changes);
                            StartSolvingProcess(this, new ValueAnswerEventArgs(0, 0, 0, MainResultsString, null, null, false));
                            Task.Factory.StartNew((Action)(() => { MainCalculatingOnAccuracy(PreperedSeriesForInsertingN, StartN, token);
                                isStartSolving = false;
                                GC.Collect();
                            }));
                        }
                    }
                }
            }
        }


        /// <summary>
        /// First prepere string value of series member, check value in sign ?, because this method returns string where all entrence of n repleced on ? / 
        /// Метод делает подготовку выражения, проверяя на знак ?, а также заменяет все вхождения n на знак вопроса для лучшей работы.
        /// </summary>
        /// <param name="seriesstringvalue"></param>
        /// <returns></returns>
        private string FirstPrepareStringSeries(string seriesstringvalue)
        {
            Regex regex = new Regex(@"\?");
            MatchCollection matches = regex.Matches(seriesstringvalue);
            if (matches.Count != 0)
            {
                string description = "Wrong expression. Please check expression or brackets" + Environment.NewLine;
                MainResultsString.Append(description);
                ErrorSolving(this, new ErrorEventArgs("Wrong expression. Please check expression or brackets"));
                return null;
            }
            else
            {
                regex = new Regex(@"(\W|^)n(\W|$)");
                MatchEvaluator PrepereNEvaluator = new MatchEvaluator(evaluateN);
                string intermediatePrepere = regex.Replace(seriesstringvalue, PrepereNEvaluator);
                return intermediatePrepere;
            }
        }
        /// <summary>
        /// This method replace sign ? on digint value /
        /// Метод заменяет все вхождения знака ? на число, переданное методу
        /// </summary>
        /// <param name="seriesstringvalue"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        private string SecondPrepareStringSeries(string seriesstringvalue, int n)
        {
            Regex regex = new Regex(@"\?");
            string preperedvalue = regex.Replace(PreperedSeriesForInsertingN, n.ToString());
            return preperedvalue;
        }

        /// <summary>
        /// set value aclculating accuracy  /
        /// устанавливает значение точности вычисления
        /// </summary>
        /// <param name="accuracy"></param>
        public void SetAccuracy(double accuracy)
        {
          if(!isStartSolving)
                if(accuracy<1&& accuracy > 0)
            solvingAccuracy = accuracy;
        }

        /// <summary>
        /// set value calculating iteration count  /
        /// устанавливает значение количества итераций  для вычисления
        /// </summary>
        /// <param name="count"></param>
        public void SetIterationCount(int count)
        {
            if (!isStartSolving)
                if(count>0)
                iterationMaxCount = count;
        }

        /// <summary>
        /// Set type of graduation criterion  (on iterations count or calculating accuracy) /
        /// Устанавливает тип критерия окончания (количество итераций или точность вычисления)
        /// </summary>
        /// <param name="issolvingonaccuracy"></param>
        public void SetGraduationCriterionType(GraduationCriterion value)
        {
            if (!isStartSolving)
                GradationCriterion = value;
        }

        /// <summary>
        /// Method prepares RexEx Match where replace n on ? /
        /// Метод который в RexEx Match заменяет n на ?
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        private string evaluateN(Match match)
        {
            Regex regex = new Regex("n");
            string value = regex.Replace(match.Value, "?");
            return value;
        }

        /// <summary>
        /// Method sets start N value / 
        /// Метод устанавливает значение начального n
        /// </summary>
        /// <param name="n"></param>
        public void SetStartN(int n)
        {
            if (!isStartSolving)
                StartN = n;
        }

        /// <summary>
        /// Method cancell solving procces /
        /// Метод останавливает процесс вычисления
        /// </summary>
        public void CancelSolving()
        {
            if (isStartSolving)
                cancellationTokenSource.Cancel();
        }


        /// <summary>
        /// Method calculate series and gradation criterion is iterations count / 
        /// Метод вычисления ряда, где критерий окончания количество итераций
        /// </summary>
        /// <param name="seriesstringvalue"></param>
        /// <param name="startindexN"></param>
        /// <param name="token"></param>
        private void MainCalculatingOnIterCount(string seriesstringvalue, int startindexN, CancellationToken token)
        {
            double term = 0, sum = 0, progress = 0;
            int itercountNow = 0;
            double progressaddval = 0;
            double previousTerm = 0;
            OneIterDescrtiption changeOnIter = new OneIterDescrtiption();
            progressaddval = (double)100 / iterationMaxCount;
            string changeOnIterString = "";
            do
            {
                try
                {
                    previousTerm = term;
                    string preperedSValue = SecondPrepareStringSeries(seriesstringvalue, startindexN);
                    term = Parser.process(preperedSValue);
                    sum += term;
                    startindexN++;
                    itercountNow++;
                    if (token.IsCancellationRequested)
                    {
                        changeOnIterString = createChangeString("User breake calculating. Last calculated series member:", itercountNow, (startindexN - 1), sum, term, "");
                        MainResultsString.Append(changeOnIterString);
                        changeOnIter = new OneIterDescrtiption(itercountNow, (startindexN - 1), sum, term);
                        AllTracingResults.Add(changeOnIter);
                        CancelSolvingProcess.Invoke(this, new ValueAnswerEventArgs(sum, itercountNow + 1, progress, MainResultsString, changeOnIter, AllTracingResults, true));
                        return;
                    }
                    //   changes = createChangeString("", itercountNow, (startindexN - 1), sum, term, "");

                    // AllTextDescription.Append(changes);
                    progress += progressaddval;
                    changeOnIter = new OneIterDescrtiption(itercountNow, (startindexN - 1), sum, term);
                    AllTracingResults.Add(changeOnIter);
                    SolvingProcess(this, new ValueAnswerEventArgs(sum, itercountNow + 1, progress, MainResultsString, changeOnIter, AllTracingResults, false));
                }
                catch (DivideByZeroException)
                {
                    changeOnIterString = createChangeString("Divide By Zero. The solving end. Last calculated series member:" + Environment.NewLine, itercountNow, (startindexN - 1), sum, term, "");
                    MainResultsString.Append(changeOnIterString);
                    ErrorSolving(this, new ErrorEventArgs("DivideByZero."));
                    CancelSolvingProcess.Invoke(this, new ValueAnswerEventArgs(sum, itercountNow + 1, progress, MainResultsString, changeOnIter, AllTracingResults, true));
                    return;
                }
                catch (Exception)
                {
                    changeOnIterString = createChangeString("Solving error. We don't know what's wrong. Last calculated series member:" + Environment.NewLine, itercountNow, (startindexN - 1), sum, term, "");
                    MainResultsString.Append(changeOnIterString);
                    ErrorSolving(this, new ErrorEventArgs("Solving error. We don't know what's wrong."));
                    CancelSolvingProcess.Invoke(this, new ValueAnswerEventArgs(sum, itercountNow + 1, progress, MainResultsString, changeOnIter, AllTracingResults, true));
                    return;
                }

            } while (itercountNow < iterationMaxCount);
            changeOnIterString = createChangeString("End Solving" + Environment.NewLine + "Answer:", itercountNow, (startindexN - 1), sum, term, "");
            MainResultsString.Append(changeOnIterString);
            EndSolvingProcess.Invoke(this, new ValueAnswerEventArgs(sum, itercountNow + 1, 100, MainResultsString, null, AllTracingResults, false));
            GC.Collect();
        }

        /// <summary>
        /// Method calculate series and gradation criterion is calculating accuracy / 
        /// Метод вычисления ряда, где критерий окончания точность вычисления
        /// </summary>
        /// <param name="seriesstringvalue"></param>
        /// <param name="startindexN"></param>
        /// <param name="token"></param>
        private void MainCalculatingOnAccuracy(string seriesstringvalue, int startindexN, CancellationToken token)
        {
            OneIterDescrtiption changeOnIter = new OneIterDescrtiption();
            int digitCountAcc = 0;
            double tempAccuracy = 0.1;
            double term = 0, sum = 0, progress = 0;
            int itercountNow = 0;
            double progressaddval = 0;
            //int countofLargeTerms = 0;
            double previousTerm = 0;
            string changeOnIterString = "";
            digitCountAcc = CountDigitsAfterDot(solvingAccuracy);
            progressaddval = ((double)100 / digitCountAcc);
            do
            {
                try
                {
                    previousTerm = term;
                    string preperedSValue = SecondPrepareStringSeries(seriesstringvalue, startindexN);
                    term = Parser.process(preperedSValue);
                    sum += term;
                    startindexN++;
                    itercountNow++;
                    if (token.IsCancellationRequested)
                    {
                        changeOnIterString = createChangeString("User break calculating. Last calculated series member:", itercountNow, (startindexN - 1), sum, term, "");
                        MainResultsString.Append(changeOnIterString);
                        changeOnIter = new OneIterDescrtiption(itercountNow, (startindexN - 1), sum, term);
                        AllTracingResults.Add(changeOnIter);
                        CancelSolvingProcess.Invoke(this, new ValueAnswerEventArgs(sum, itercountNow + 1, progress, MainResultsString, changeOnIter, AllTracingResults, true));
                        return;
                    }
                    calculateProgressAndtAccuracy(digitCountAcc, term, ref tempAccuracy, ref progress, progressaddval);
                    changeOnIter = new OneIterDescrtiption(itercountNow, (startindexN - 1), sum, term);
                    AllTracingResults.Add(changeOnIter);
                    SolvingProcess.Invoke(this, new ValueAnswerEventArgs(sum, itercountNow + 1, progress, MainResultsString, changeOnIter, AllTracingResults, false));
                }
                catch (DivideByZeroException )
                {
                    changeOnIterString = createChangeString("Divide By Zero. The solving end. Last calculated series member:" + Environment.NewLine, itercountNow, (startindexN - 1), sum, term, "");
                    MainResultsString.Append(changeOnIterString);
                    ErrorSolving(this, new ErrorEventArgs("DivideByZero."));
                    CancelSolvingProcess.Invoke(this, new ValueAnswerEventArgs(sum, itercountNow + 1, progress, MainResultsString, changeOnIter, AllTracingResults, true));
                    return;
                }
                catch (Exception )
                {
                    changeOnIterString = createChangeString("Solving error. We don't know what's wrong. Last calculated series member:" + Environment.NewLine, itercountNow, (startindexN - 1), sum, term, "");
                    MainResultsString.Append(changeOnIterString);
                    ErrorSolving(this, new ErrorEventArgs("Solving error. We don't know what's wrong. "));
                    CancelSolvingProcess.Invoke(this, new ValueAnswerEventArgs(sum, itercountNow + 1, progress, MainResultsString, changeOnIter, AllTracingResults, true));
                    return;
                }

                if(previousTerm!=0)
                if (Math.Abs(term/previousTerm)>=1)
                {
                    //countofLargeTerms++;
                    //if (countofLargeTerms >= MaxCountofIdentIaliter)
                    //{
                        changeOnIterString = createChangeString("The series does not tend to zero. Last calculated series member:", itercountNow, (startindexN - 1), sum, term, "");
                        MainResultsString.Append(changeOnIterString);
                        ErrorSolving(this, new ErrorEventArgs("The series does not tend to zero."));
                        CancelSolvingProcess.Invoke(this, new ValueAnswerEventArgs(sum, itercountNow + 1, progress, MainResultsString, changeOnIter, AllTracingResults, true));

                        return;
                    //}
                }
                else
                { /*countofLargeTerms = 0;*/ }

            } while (Math.Abs(term) >= solvingAccuracy);
            progress = 100;
            SolvingProcess.Invoke(this, new ValueAnswerEventArgs(sum, itercountNow + 1, progress, MainResultsString, null, AllTracingResults, false));
            changeOnIterString = createChangeString("End Calculatng" + Environment.NewLine + "Answer:", itercountNow, (startindexN - 1), sum, term, "");
            MainResultsString.Append(changeOnIterString);
            EndSolvingProcess.Invoke(this, new ValueAnswerEventArgs(sum, itercountNow + 1, progress, MainResultsString, null, AllTracingResults, false));
        }
        /// <summary>
        /// Method does first calculating and check on error (divide by zero, breakets etc.) / 
        /// Метод делает первоначальное вычисление подготовленного члена ряда, тем самым проверяет выражение на ошибки (деление на ноль, неправильное количество скобок и т.д.)
        /// </summary>
        /// <param name="seriesstringvalue"></param>
        /// <param name="startindexN"></param>
        /// <returns></returns>
        private bool isErrorInMemberCalcul(string seriesstringvalue, int startindexN)
        {
            try
            {
                string preperedSValue = SecondPrepareStringSeries(seriesstringvalue, startindexN);
                double term = Parser.process(preperedSValue);
            }
            catch (DivideByZeroException )
            {
                ErrorSolving(this, new ErrorEventArgs("Divide By Zero."));
                MainResultsString.Append("Error. Divide By Zero." + Environment.NewLine);
                CancelSolvingProcess.Invoke(this, new ValueAnswerEventArgs(MainResultsString, true));
                return true;
            }
            catch (Exception)
            {
                ErrorSolving(this, new ErrorEventArgs("Wrong expression. Please check expression or brackets."));
                MainResultsString.Append("Error. Wrong expression. Please check expression or brackets." + Environment.NewLine);
                CancelSolvingProcess.Invoke(this, new ValueAnswerEventArgs(MainResultsString,true));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Method calculates how many digits after dot or comma in real number / 
        /// Метод высчитывает сколько цифер после запятой в вещественном числе, нужен для полосы прогресса в вычислении по точности
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private int CountDigitsAfterDot(double number)
        {
            int decimals = 0;
            while ((int)number % 10 == 0)
            {
                number *= 10;
                decimals++;
            }
            return decimals;
        }

        /// <summary>
        /// Method calculates Progress and Accuracy for Solving on Accuracy /
        /// Метод высчитывает прогресс 
        /// </summary>
        /// <param name="digitCountAcc">Count of signs after dot or comma in real</param>
        /// <param name="term">Member value of Series</param>
        /// <param name="tempAccuracy">Accuracy for compare and for calculate progress</param>
        /// <param name="progress"></param>
        /// <param name="progressaddval">Value wich will add to progress</param>
        private void calculateProgressAndtAccuracy(int digitCountAcc,double term,ref double tempAccuracy,ref double progress, double progressaddval)
        {
            if (Math.Abs(term) <= tempAccuracy)
            {
                int j = digitCountAcc;
                double i = Math.Pow(10, digitCountAcc);
                while (Math.Abs(term) > tempAccuracy / i)
                {
                    i /= 10;
                    j--;
                }
                if (tempAccuracy / i < tempAccuracy)
                {
                    progress += progressaddval * (j);
                    tempAccuracy = tempAccuracy / (i);
                }
            }
        }
      
        /// <summary>
        /// Method builds tracing of member to string
        /// Метод для простого построения троссировки члена ряда
        /// </summary>
        /// <param name="startOfString">Добавление сообщения в начало строки</param>
        /// <param name="itercountNow">Итерация на которой происходит вывод</param>
        /// <param name="curindexN">Число N на итерации</param>
        /// <param name="sum">Сумма ряда на данной итерации</param>
        /// <param name="term">Значение члена ряда</param>
        /// <param name="endOfString">Добавление сообщения в конец строки</param>
        /// <returns></returns>
        private string createChangeString(string startOfString,int itercountNow, int curindexN, double sum,double term, string endOfString)
        {
            string change = "";
            if (startOfString != string.Empty)
                change += startOfString + Environment.NewLine;
             change += "Member = " + itercountNow + Environment.NewLine + "   n = " + (curindexN) + Environment.NewLine + "   Sum = "
             + sum  + Environment.NewLine + "   Value of member = " + term + Environment.NewLine;
            if (endOfString != string.Empty)
                change += endOfString + Environment.NewLine;
            change +=  Environment.NewLine ;
            return change;
        }


        /// <summary>
        /// Method fills user help /
        /// Метод заполняет пользовательскую справку по доступным функциям
        /// </summary>
        public  void FillHelpView()
        {
            List<OneLineOfHelpList> list = new List<OneLineOfHelpList>();
            XmlSerializer xmls = new XmlSerializer(list.GetType());
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(Properties.Resources.en_Eng));
            list = (List<OneLineOfHelpList>)xmls.Deserialize(ms);
            ms.Close();
            HelpFilled(this, new HelpFilledEventArgs(list));
        }

        public void ClearAllResultsDescription()
        {
            MainResultsString.Clear();
            AllTracingResults.Clear();
        }

        public void WriteToFile(string path)
        {
            StreamWriter sr = new StreamWriter(path);
            sr.WriteLine(seriesMemberValue);
            sr.WriteLine(MainResultsString);
            sr.WriteLine("Tracing members");
            foreach (OneIterDescrtiption line in AllTracingResults)
            {
                sr.WriteLine(line.ToString());
            }
            sr.Flush();
            sr.Close();
            sr.Dispose();
        }

        /// <summary>
        /// Method sets that input fields have error (needs for not start solving) /
        /// Метод устанавливает, что в полях ввода формы есть ошибки (нужно чтобы не начиать вычисление)
        /// </summary>
        public void SetErrorInFillFields()
        {
            isErrorOnFillFields = true;
        }
    
        /// <summary>
        /// Method sets that input fields have Not error (needs for start solving) /
        /// Метод устанавливает, что в полях ввода формы нет ошибок (нужно чтобы начиать вычисление)
        /// </summary>
        public void TakeOfErrorInFillFields()
        {
            isErrorOnFillFields = false;
        }
    }



    /// <summary>
    /// Method saves info about calculated member (tracing member), it's may be part of all tracing list  /
    /// Метод хранит информацию об троссировке члена ряда, может быть частью списка троссировки всех членов ряда
    /// </summary>
    public class OneIterDescrtiption
    {
        public double Member { get; set; }
        public double N { get; set; }
        public double MemberValue { get; set; }
        public double Sum { get; set; }
       
        public OneIterDescrtiption(double member, double n, double sum, double memberValue)
        {
            Member = member;
            N = n;
            Sum = sum;
            MemberValue = memberValue;
        }
        public OneIterDescrtiption()
        { }

        public override string ToString()
        {
            string change = "Member = " +  Member  + Environment.NewLine
                + "   N = " + N + Environment.NewLine 
                + "   Sum = "  + Sum + Environment.NewLine 
                + "   Value of member = " + MemberValue;
            return change;
        }
    }


    public class MemberValueEventArgs : EventArgs
    {
        public int Curosrvalue { get;}
        public int NewCurosrvalue { get; }
        public string Insertingvalue { get; }

        public string AllMemberValue { get; }
        public MemberValueEventArgs(int curosrvalue, string insertingvalue, string allMemberValue)
        {
            Curosrvalue = curosrvalue;
            Insertingvalue = insertingvalue;
            NewCurosrvalue = curosrvalue + Insertingvalue.Length;
            AllMemberValue = allMemberValue;
        }
    }
    public class RemoveMemberValueEventArgs: EventArgs
    {
        public int StartIndex { get; }
        public int Count { get; }

        public string AllMemberValue { get; }

        public RemoveMemberValueEventArgs(int startIndex, int count, string allMemberValue)
        {
            StartIndex = startIndex;
            Count = count;
            AllMemberValue = allMemberValue;
        }
    }

    public class ValueAnswerEventArgs : EventArgs
    {
        public double ProvisionlSum { get; }
        public int IterationCount { get;  }
        public double Progress { get;  }
        public StringBuilder TextDescriprtion { get; }
        public OneIterDescrtiption Changes { get; }

        public List<OneIterDescrtiption> AllGridDescription { get; }
        public bool IsCanceled { get;}

        public ValueAnswerEventArgs(double provisionlsum, int iterationCount, double progress, StringBuilder allDescriprtion, OneIterDescrtiption changes, List<OneIterDescrtiption> allGridDescription,  bool isCanceled)
        {
            ProvisionlSum = provisionlsum;
            IterationCount = iterationCount;
            Progress = progress;
            TextDescriprtion = allDescriprtion;
            Changes = changes;
            AllGridDescription = allGridDescription;
            IsCanceled = isCanceled;
        }
        public ValueAnswerEventArgs(StringBuilder allDescriprtion, bool isCanceled)
        {
            TextDescriprtion = allDescriprtion;
            IsCanceled = isCanceled;
        }
    }

    public class ErrorEventArgs:EventArgs
    {   
        public  string Message { get; }
        public ErrorEventArgs(string message)
        {
            Message = message;
        }
    }

    public class OneLineOfHelpList
    {
        public string Function { get; set; }
        public string Description { get; set; }
        public OneLineOfHelpList(string function, string description)
        {
            Function = function;
            Description = description;
        }
        public OneLineOfHelpList()
        { }
    }

    public class HelpFilledEventArgs : EventArgs
    {
        public List<OneLineOfHelpList> List { get; }
        public HelpFilledEventArgs(List<OneLineOfHelpList> list)
        {
            List = list;
        }
    }

}
