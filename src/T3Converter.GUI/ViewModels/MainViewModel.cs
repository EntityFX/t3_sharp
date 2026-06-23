using System;
using CommunityToolkit.Mvvm.ComponentModel;
using TritTypes;

namespace T3Converter.GUI.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly T3ConversionService _conversionService;

        [ObservableProperty]
        private string _inputText = string.Empty;

        [ObservableProperty]
        private string _binaryResult = string.Empty;

        [ObservableProperty]
        private string _hexResult = string.Empty;

        [ObservableProperty]
        private string _ternaryResult = string.Empty;

        [ObservableProperty]
        private string _nonaryResult = string.Empty;

        [ObservableProperty]
        private string _twentySevenAryResult = string.Empty;

        [ObservableProperty]
        private string _octalResult = string.Empty;

        [ObservableProperty]
        private string _decimalResult = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public MainViewModel()
        {
            _conversionService = new T3ConversionService();
        }

        partial void OnInputTextChanged(string value)
        {
            UpdateResults(value);
        }

        private void UpdateResults(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                ClearResults();
                return;
            }

            try
            {
                var result = _conversionService.Convert(input);
                if (result != null)
                {
                    BinaryResult = result.Binary;
                    HexResult = result.Hex;
                    TernaryResult = result.Ternary;
                    NonaryResult = result.Nonary;
                    TwentySevenAryResult = result.TwentySevenAry;
                    OctalResult = result.Octal;
                    DecimalResult = result.DecimalValue.ToString();
                    ErrorMessage = string.Empty;
                }
                else
                {
                    ClearResults();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                // We don't clear results here so the user can see what was last valid
                // or we can clear them. Let's clear them for clarity.
                ClearResults(keepError: true);
            }
        }

        private void ClearResults(bool keepError = false)
        {
            BinaryResult = string.Empty;
            HexResult = string.Empty;
            TernaryResult = string.Empty;
            NonaryResult = string.Empty;
            TwentySevenAryResult = string.Empty;
            OctalResult = string.Empty;
            DecimalResult = string.Empty;
            if (!keepError) ErrorMessage = string.Empty;
        }
    }
}