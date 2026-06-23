using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TritTypes;

namespace T3Calculator.GUI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly T3ArithmeticEngine _arithmeticEngine = new();
        private readonly T3SciiService _sciiService = new();
        private readonly T3ConversionService _conversionService = new();

        private string _currentTab = "Calculator";
        public string CurrentTab
        {
            get => _currentTab;
            set { _currentTab = value; OnPropertyChanged(); }
        }

        // --- Calculator Tab ---
        private string _calcInput = "";
        public string CalcInput
        {
            get => _calcInput;
            set 
            { 
                _calcInput = value; 
                OnPropertyChanged();
                UpdateCalcResult();
            }
        }

        private string _calcResult = "";
        public string CalcResult
        {
            get => _calcResult;
            set { _calcResult = value; OnPropertyChanged(); }
        }

        private string _calcDetails = "";
        public string CalcDetails
        {
            get => _calcDetails;
            set { _calcDetails = value; OnPropertyChanged(); }
        }

        private int _logicA = 0;
        public int LogicA
        {
            get => _logicA;
            set { _logicA = value; OnPropertyChanged(); UpdateLogicResult(); }
        }

        private int _logicB = 0;
        public int LogicB
        {
            get => _logicB;
            set { _logicB = value; OnPropertyChanged(); UpdateLogicResult(); }
        }

        public List<string> LogicOps { get; } = new() { "AND", "OR", "XOR", "IMPLIES", "XNOR" };
        private string _selectedLogicOp = "AND";
        public string SelectedLogicOp
        {
            get => _selectedLogicOp;
            set { _selectedLogicOp = value; OnPropertyChanged(); UpdateLogicResult(); }
        }

        private string _logicResult = "";
        public string LogicResult
        {
            get => _logicResult;
            set { _logicResult = value; OnPropertyChanged(); }
        }

        // --- Truth Table Tab ---
        public ObservableCollection<TruthTableRow> TruthTableAnd { get; } = new();
        public ObservableCollection<TruthTableRow> TruthTableOr { get; } = new();
        public ObservableCollection<TruthTableRow> TruthTableXor { get; } = new();
        public ObservableCollection<TruthTableRow> TruthTableImplies { get; } = new();
        public ObservableCollection<TruthTableRow> TruthTableXnor { get; } = new();

        // --- T-SCII Tab ---
        public ObservableCollection<T3SciiService.TSciiEntry> SciiTable { get; } = new();

        // --- Converter Tab ---
        private string _convInput = "";
        public string ConvInput
        {
            get => _convInput;
            set { _convInput = value; OnPropertyChanged(); UpdateConversion(); }
        }

        private string _convBin = "", _convHex = "", _convTer = "", _convNon = "", _convT27 = "";
        public string ConvBin { get => _convBin; set { _convBin = value; OnPropertyChanged(); } }
        public string ConvHex { get => _convHex; set { _convHex = value; OnPropertyChanged(); } }
        public string ConvTer { get => _convTer; set { _convTer = value; OnPropertyChanged(); } }
        public string ConvNon { get => _convNon; set { _convNon = value; OnPropertyChanged(); } }
        public string ConvT27 { get => _convT27; set { _convT27 = value; OnPropertyChanged(); } }

        public MainViewModel()
        {
            InitSciiTable();
            InitTruthTables();
        }

        private void InitTruthTables()
        {
            FillTable(TruthTableAnd, T3Logic.And);
            FillTable(TruthTableOr, T3Logic.Or);
            FillTable(TruthTableXor, T3Logic.Xor);
            FillTable(TruthTableImplies, T3Logic.Implies);
            FillTable(TruthTableXnor, T3Logic.Xnor);
        }

        private void FillTable(ObservableCollection<TruthTableRow> table, Func<int, int, int> op)
        {
            table.Clear();
            var data = T3Logic.GenerateTruthTable(op);
            foreach (var row in data)
            {
                table.Add(new TruthTableRow { A = row.A, B = row.B, Result = row.Result });
            }
        }

        public void SetLogicA(int value) { LogicA = value; }
        public void SetLogicB(int value) { LogicB = value; }
        public void SetLogicOp(string op) { SelectedLogicOp = op; }

        private void UpdateCalcResult()
        {
            if (string.IsNullOrWhiteSpace(CalcInput))
            {
                CalcResult = "";
                CalcDetails = "";
                return;
            }
            CalcResult = _arithmeticEngine.Evaluate(CalcInput);
            UpdateCalcDetails();
        }

        private void UpdateCalcDetails()
        {
            if (string.IsNullOrWhiteSpace(CalcInput))
            {
                CalcDetails = "";
                return;
            }

            try
            {
                // Very basic parsing to extract operands for columnar view
                // In a real scenario, we'd use the engine's tokenizer
                string input = CalcInput.Trim();
                int opIdx = input.IndexOfAny(new char[] { '+', '-', '*' });
                
                if (opIdx != -1)
                {
                    string op = input[opIdx].ToString();
                    string a = input.Substring(0, opIdx).Trim();
                    string b = input.Substring(opIdx + 1).Trim();
                    
                    // We assume for the columnar view we are working with balanced ternary strings
                    // If they are decimal, we convert them first
                    if (!IsTernary(a)) a = BalancedTernary.ToTernaryString(long.Parse(a));
                    if (!IsTernary(b)) b = BalancedTernary.ToTernaryString(long.Parse(b));
                    
                    CalcDetails = _arithmeticEngine.GetColumnarSolution(op, a, b);
                }
                else
                {
                    CalcDetails = $"Evaluated: {CalcInput}";
                }
            }
            catch
            {
                CalcDetails = "Could not generate columnar solution";
            }
        }

        private bool IsTernary(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (char c in s) if (c != '+' && c != '-' && c != '0') return false;
            return true;
        }

        private void UpdateLogicResult()
        {
            int res = SelectedLogicOp switch
            {
                "AND" => T3Logic.And(LogicA, LogicB),
                "OR" => T3Logic.Or(LogicA, LogicB),
                "XOR" => T3Logic.Xor(LogicA, LogicB),
                "IMPLIES" => T3Logic.Implies(LogicA, LogicB),
                "XNOR" => T3Logic.Xnor(LogicA, LogicB),
                _ => 0
            };
            LogicResult = $"Result: {res}";
        }


        private void InitSciiTable()
        {
            var data = _sciiService.GenerateSciiTable();
            foreach (var entry in data) SciiTable.Add(entry);
        }

        private void UpdateConversion()
        {
            var res = _conversionService.Convert(ConvInput);
            if (res != null)
            {
                ConvBin = res.Binary ?? "???";
                ConvHex = res.Hex ?? "???";
                ConvTer = res.Ternary ?? "???";
                ConvNon = res.Nonary ?? "???";
                ConvT27 = res.TwentySevenAry ?? "???";
            }
            else
            {
                ConvBin = ConvHex = ConvTer = ConvNon = ConvT27 = "???";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class TruthTableRow
    {
        public int A { get; set; }
        public int B { get; set; }
        public int Result { get; set; }
    }
}