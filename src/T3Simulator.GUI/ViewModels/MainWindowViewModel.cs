using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using T3Simulator.Common;
using T3Simulator.InOrder;
using T3Simulator.GUI.Utils;
using T3Simulator.GUI.Services;
using T3Assembler;
using TritTypes;

namespace T3Simulator.GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly T3InOrderProcessor<Word18> _processor;
    private readonly IFileDialogService _fileDialogService;
    private CancellationTokenSource? _cts;
    
    private string _currentFormat = T3Formatter.FORMAT_TRINARY;
    public string CurrentFormat
    {
        get => _currentFormat;
        set { if (SetProperty(ref _currentFormat, value)) UpdateState(); }
    }

    private string _logText = string.Empty;
    public string LogText
    {
        get => _logText;
        set => SetProperty(ref _logText, value);
    }

    private string _disassembledCode = "HALT";
    public string DisassembledCode
    {
        get => _disassembledCode;
        set => SetProperty(ref _disassembledCode, value);
    }

    private string _pcFormatted = "0";
    public string PcFormatted
    {
        get => _pcFormatted;
        set => SetProperty(ref _pcFormatted, value);
    }

    private string _spFormatted = "0";
    public string SpFormatted
    {
        get => _spFormatted;
        set => SetProperty(ref _spFormatted, value);
    }

    private string _prFormatted = "0";
    public string PrFormatted
    {
        get => _prFormatted;
        set => SetProperty(ref _prFormatted, value);
    }

    private string _condFormatted = "0";
    public string CondFormatted
    {
        get => _condFormatted;
        set => SetProperty(ref _condFormatted, value);
    }

    private bool _isRunning;
    public bool IsRunning
    {
        get => _isRunning;
        set => SetProperty(ref _isRunning, value);
    }

    // Explicit properties for logical registers
    public RegisterInfo RegPC { get; } = new() { Name = "PC" };
    public RegisterInfo RegRW { get; } = new() { Name = "RW" };
    public RegisterInfo RegRX { get; } = new() { Name = "RX" };
    public RegisterInfo RegRY { get; } = new() { Name = "RY" };
    public RegisterInfo RegRZ { get; } = new() { Name = "RZ" };
    public RegisterInfo RegR0 { get; } = new() { Name = "R0" };
    public RegisterInfo RegR1 { get; } = new() { Name = "R1" };
    public RegisterInfo RegR2 { get; } = new() { Name = "R2" };
    public RegisterInfo RegR3 { get; } = new() { Name = "R3" };
    public RegisterInfo RegR4 { get; } = new() { Name = "R4" };

    public ObservableCollection<RegisterInfo> LogicalRegisters { get; } = new();
    public ObservableCollection<RegisterInfo> PhysicalRegisters { get; } = new();
    public ObservableCollection<MemoryInfo> Memory { get; } = new();

    private long _memViewStart = 0;
    public long MemViewStart
    {
        get => _memViewStart;
        set { if (SetProperty(ref _memViewStart, value)) UpdateMemoryView(); }
    }

    public IAsyncRelayCommand LoadFileCommand { get; }
    public IRelayCommand StepCommand { get; }
    public IRelayCommand ResetCommand { get; }
    public IAsyncRelayCommand RunCommand { get; }
    public IRelayCommand StopCommand { get; }
    public IAsyncRelayCommand SaveLogCommand { get; }

    public MainWindowViewModel(IFileDialogService fileDialogService)
    {
        _fileDialogService = fileDialogService;
        
        try
        {
            _processor = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            AppendLog("Processor instance created.");
            
            InitializeRegisterCollections();
            
            LoadFileCommand = new AsyncRelayCommand(LoadFileAsync);
            StepCommand = new RelayCommand(Step);
            ResetCommand = new RelayCommand(Reset);
            RunCommand = new AsyncRelayCommand(RunAsync);
            StopCommand = new RelayCommand(Stop);
            SaveLogCommand = new AsyncRelayCommand(SaveLogAsync);
            
            UpdateState();
            AppendLog("Simulator initialization successful.");
        }
        catch (Exception ex)
        {
            AppendLog($"CRITICAL ERROR during initialization: {ex.Message}");
            if (LogicalRegisters.Count == 0) InitializeRegisterCollections();
        }
    }

    private void InitializeRegisterCollections()
    {
        LogicalRegisters.Clear();
        LogicalRegisters.Add(RegRW);
        LogicalRegisters.Add(RegRX);
        LogicalRegisters.Add(RegRY);
        LogicalRegisters.Add(RegRZ);
        LogicalRegisters.Add(RegR0);
        LogicalRegisters.Add(RegR1);
        LogicalRegisters.Add(RegR2);
        LogicalRegisters.Add(RegR3);
        LogicalRegisters.Add(RegR4);

        PhysicalRegisters.Clear();
        for (int i = 0; i < 27; i++)
        {
            PhysicalRegisters.Add(new RegisterInfo { Index = i, Name = $"R{i}", Value = "0" });
        }

        RegPC.IsActive = true;
        RegPC.Value = "0";
    }

    public async System.Threading.Tasks.Task LoadFileAsync()
    {
        string? filePath = await _fileDialogService.OpenFileAsync("Ternary Files (*.bin;*.txt;*.asm)|*.bin;*.txt;*.asm|All Files (*.*)|*.*");
        if (string.IsNullOrEmpty(filePath)) return;

        try
        {
            List<Word18> programWords = new List<Word18>();

            if (filePath.EndsWith(".asm", StringComparison.OrdinalIgnoreCase))
            {
                var assembler = new T3InOrderAssembler(T3Config.T3_18);
                string content = await File.ReadAllTextAsync(filePath);
                List<Int128> binary = assembler.Assemble(content);
                foreach (var val in binary) programWords.Add(Word18.FromLong((long)val));
                var dis = T3Disassembler.Disassemble(programWords);
            }
            else if (filePath.EndsWith(".bin", StringComparison.OrdinalIgnoreCase))
            {
                byte[] bytes = await File.ReadAllBytesAsync(filePath);
                List<int> trits = TritEncoding.FromBinary(bytes, 1000000 * 27);
                for (int i = 0; i + 26 < trits.Count; i += 27)
                {
                    int[] wordTrits = trits.GetRange(i, 27).ToArray();
                    programWords.Add(Word18.FromLong(BalancedTernary.ParseFromTritArray(wordTrits)));
                }
            }
            else
            {
                string content = await File.ReadAllTextAsync(filePath);
                List<int> trits = content.StartsWith("0n") ? TritEncoding.FromNinary(content) : 
                               content.StartsWith("0y") ? TritEncoding.FromTryx(content) : 
                               TritEncoding.FromSimpleText(content);

                for (int i = 0; i + 26 < trits.Count; i += 27)
                {
                    int[] wordTrits = trits.GetRange(i, 27).ToArray();
                    programWords.Add(Word18.FromLong(BalancedTernary.ParseFromTritArray(wordTrits)));
                }
            }

            _processor.Reset();
            _processor.LoadProgram(programWords);
            UpdateState();
            UpdateDisassembly();
            AppendLog($"Loaded program from {filePath}. Size: {programWords.Count} words.");
        }
        catch (Exception ex)
        {
            AppendLog($"Error loading file: {ex.Message}");
        }
    }

    private void Step()
    {
        if (_processor.Step())
        {
            UpdateState();
        }
        else
        {
            AppendLog("Processor halted.");
        }
    }

    private async System.Threading.Tasks.Task RunAsync()
    {
        IsRunning = true;
        _cts = new CancellationTokenSource();
        try
        {
            while (IsRunning && !_processor.IsHalted && !_cts.Token.IsCancellationRequested)
            {
                if (!_processor.Step()) break;
                UpdateDisassembly();
                UpdateState();
                await System.Threading.Tasks.Task.Delay(10);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            IsRunning = false;
            UpdateState();
        }
    }

    private void Stop()
    {
        IsRunning = false;
        _cts?.Cancel();
    }

    private void Reset()
    {
        _processor.Reset();
        UpdateState();
        AppendLog("Processor reset successful.");
    }

    private void UpdateState()
    {
        try 
        {
            var state = _processor.GetState();

           
            PcFormatted = T3Formatter.FormatValue(state.PC, CurrentFormat);
            RegPC.Value = PcFormatted;

            SpFormatted = T3Formatter.FormatValue(state.SP, CurrentFormat);
            
            // Using ToInt128() safely via cast for PR
            PrFormatted = T3Formatter.FormatValue((long)state.PR.ToInt128(), CurrentFormat);
            CondFormatted = state.CD.ToString();

            // Update explicit logical registers
            var logicals = new[] { RegRW, RegRX, RegRY, RegRZ, RegR0, RegR1, RegR2, RegR3, RegR4 };
            for (int i = 0; i < logicals.Length; i++)
            {
                int physicalIndex = RegisterWindow.GetPhysicalIndex(i, state.WD);
                // FIX: Use ToInt128() instead of direct (long) cast
                long val = (long)state.Registers[physicalIndex].ToInt128();
                logicals[i].Value = T3Formatter.FormatValue(val, CurrentFormat);
                logicals[i].IsActive = true; 
            }


            UpdateMemoryView();

            Thread.Sleep(100);
        }
        catch (Exception ex)
        {
            AppendLog($"UI State Update Error: {ex.Message}");
        }
    }

    private void UpdateDisassembly()
    {
        try
        {
            long pc = _processor.PC;
            long start = (pc / 16) * 16;
            List<Word18> words = new List<Word18>();
            for (long i = start; i < start + 32; i++)
            {
                words.Add(_processor.ReadWord(i));
            }

            var lines = T3Disassembler.Disassemble(words);
            if (lines == null || lines.Count == 0)
            {
                DisassembledCode = "[No instructions to display]";
                return;
            }

            StringBuilder sb = new StringBuilder();
            foreach (var line in lines)
            {
                string prefix = line.Contains($"0x{pc:X8}:") ? "> " : "  ";
                sb.AppendLine($"{prefix}{line}");
            }
            DisassembledCode = sb.ToString();
        }
        catch (Exception ex)
        {
            DisassembledCode = $"Error in disassembly: {ex.Message}";
        }
    }

    private void UpdateMemoryView()
    {
        try
        {
            Memory.Clear();
            for (long i = MemViewStart; i < MemViewStart + 32; i++)
            {
                long val = _processor.GetMemoryValue(i);
                Memory.Add(new MemoryInfo 
                { 
                    Address = i, 
                    Value = T3Formatter.FormatValue(val, CurrentFormat),
                    DecimalValue = val
                });
            }
        }
        catch (Exception ex)
        {
            AppendLog($"Error updating memory view: {ex.Message}");
        }
    }

    private async System.Threading.Tasks.Task SaveLogAsync()
    {
        string? filePath = await _fileDialogService.SaveFileAsync("Log Files (*.log)|*.log|Text Files (*.txt)|*.txt", "execution_log.log");
        if (string.IsNullOrEmpty(filePath)) return;
        
        try
        {
            await File.WriteAllTextAsync(filePath, LogText);
            AppendLog($"Log saved to {filePath}");
        }
        catch (Exception ex)
        {
            AppendLog($"Error saving log: {ex.Message}");
        }
    }

    private void AppendLog(string message)
    {
        LogText += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
    }
}

public class RegisterInfo : INotifyPropertyChanged
{
    public int Index { get; set; }
    public string Name { get; set; } = string.Empty;

    private string _value = "0";
    public string Value
    {
        get => _value;
        set { _value = value; OnPropertyChanged(); }
    }

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set { _isActive = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class MemoryInfo : INotifyPropertyChanged
{
    public long Address { get; set; }
    
    private string _value = "0";
    public string Value
    {
        get => _value;
        set { _value = value; OnPropertyChanged(); }
    }

    public long DecimalValue { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}