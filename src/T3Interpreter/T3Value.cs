using System;
using System.Collections.Generic;

namespace T3Interpreter
{
    public class T3Value
    {
    public enum ValueKind { Int, Float, Bool, Trit, Void, Array, Struct, Null }
        public ValueKind Kind;
        long _intVal;
        double _floatVal;
        int _boolVal;
        T3Value[]? _arrayVal;
        Dictionary<string,T3Value>? _structVal;

        T3Value() => Kind = ValueKind.Void;
        public static T3Value FromInt(long v) => new() { Kind = ValueKind.Int, _intVal = v };
        public static T3Value FromFloat(double v) => new() { Kind = ValueKind.Float, _floatVal = v };
        public static T3Value FromBool(int v) => new() { Kind = ValueKind.Bool, _boolVal = v };
        public static T3Value FromArray(int sz) { var a = new T3Value { Kind = ValueKind.Array, _arrayVal = new T3Value[sz] }; for (int i = 0; i < sz; i++) a._arrayVal[i] = FromInt(0); return a; }
        public static T3Value FromStruct() => new() { Kind = ValueKind.Struct, _structVal = new() };
        public static readonly T3Value Void = new() { Kind = ValueKind.Void };
        public static readonly T3Value Null = new() { Kind = ValueKind.Null };

        public long AsInt() => Kind switch { ValueKind.Int => _intVal, ValueKind.Bool => _boolVal, ValueKind.Null => 0, _ => 0 };
        public int AsBool() => Kind switch { ValueKind.Bool => _boolVal, ValueKind.Int => _intVal > 0 ? 1 : (_intVal < 0 ? -1 : 0), ValueKind.Null => 0, _ => 0 };
        public bool IsNull => Kind == ValueKind.Null;

        public static T3Value operator +(T3Value a, T3Value b) => FromInt(a.AsInt() + b.AsInt());
        public static T3Value operator -(T3Value a, T3Value b) => FromInt(a.AsInt() - b.AsInt());
        public static T3Value operator *(T3Value a, T3Value b) => FromInt(a.AsInt() * b.AsInt());
        public static T3Value operator /(T3Value a, T3Value b) => FromInt(b.AsInt() == 0 ? throw new DivideByZeroException() : a.AsInt() / b.AsInt());
        public static T3Value operator %(T3Value a, T3Value b) => FromInt(b.AsInt() == 0 ? throw new DivideByZeroException() : a.AsInt() % b.AsInt());

        public T3Value GetElement(int idx) => _arrayVal != null && idx >= 0 && idx < _arrayVal.Length ? _arrayVal[idx] : FromInt(0);
        public void SetElement(int idx, T3Value v) { if (_arrayVal != null && idx >= 0 && idx < _arrayVal.Length) _arrayVal[idx] = v; }
        public T3Value? GetField(string name) => _structVal != null && _structVal.TryGetValue(name, out var v) ? v : null;
        public void SetField(string name, T3Value v) { _structVal ??= new(); _structVal[name] = v; }

        public override string ToString() => Kind switch
        {
            ValueKind.Int => _intVal.ToString(),
            ValueKind.Bool => _boolVal > 0 ? "true" : (_boolVal < 0 ? "false" : "maybe"),
            _ => "void"
        };
    }
}