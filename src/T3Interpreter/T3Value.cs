using System;
using System.Collections.Generic;
using System.Numerics;

namespace T3Interpreter
{
    public class T3Value
    {
        // 36-trit mask: 3^36 = 150094635296999121
        private static readonly Int128 TlongModulus = Int128.Parse("150094635296999121");
        private static readonly Int128 TlongMax = (TlongModulus - 1) / 2; // ~7.5e16
        private static readonly Int128 TlongMin = -TlongMax;

        public enum ValueKind { Int, Long, Float, Bool, Trit, Void, Array, Struct, Union, Null }
        public ValueKind Kind;
        long _intVal;
        Int128 _longVal;
        double _floatVal;
        int _boolVal;
        T3Value[]? _arrayVal;
        Dictionary<string,T3Value>? _structVal;
        T3Value? _unionVal;      // single shared value for union
        string? _lastField;      // name of the last-written field (union)

        T3Value() => Kind = ValueKind.Void;
        public static T3Value FromInt(long v) => new() { Kind = ValueKind.Int, _intVal = v };
        public static T3Value FromLong(Int128 v) => new() { Kind = ValueKind.Long, _longVal = TlongWrap(v) };
        public static T3Value FromFloat(double v) => new() { Kind = ValueKind.Float, _floatVal = v };
        public static T3Value FromBool(int v) => new() { Kind = ValueKind.Bool, _boolVal = v };
        public static T3Value FromArray(int sz) { var a = new T3Value { Kind = ValueKind.Array, _arrayVal = new T3Value[sz] }; for (int i = 0; i < sz; i++) a._arrayVal[i] = FromInt(0); return a; }
        public static T3Value FromStruct() => new() { Kind = ValueKind.Struct, _structVal = new() };
        public static T3Value FromUnion() => new() { Kind = ValueKind.Union, _unionVal = FromInt(0) };
        public static readonly T3Value Void = new() { Kind = ValueKind.Void };
        public static readonly T3Value Null = new() { Kind = ValueKind.Null };

        /// <summary>Wrap a value to 36-trit range [-TlongMax, TlongMax].</summary>
        static Int128 TlongWrap(Int128 v)
        {
            if (v > TlongMax)
            {
                Int128 wrapped = v % TlongModulus;
                if (wrapped > TlongMax) wrapped -= TlongModulus;
                return wrapped;
            }
            if (v < TlongMin)
            {
                Int128 wrapped = v % TlongModulus;
                if (wrapped < TlongMin) wrapped += TlongModulus;
                return wrapped;
            }
            return v;
        }

        public long AsInt() => Kind switch
        {
            ValueKind.Int => _intVal,
            ValueKind.Long => (long)_longVal,
            ValueKind.Float => (long)_floatVal,
            ValueKind.Bool => _boolVal,
            ValueKind.Null => 0,
            _ => 0
        };
        public Int128 AsLong() => Kind switch
        {
            ValueKind.Long => _longVal,
            ValueKind.Int => _intVal,
            ValueKind.Float => (Int128)_floatVal,
            ValueKind.Bool => _boolVal,
            ValueKind.Null => 0,
            _ => 0
        };
        public double AsFloat() => Kind switch
        {
            ValueKind.Float => _floatVal,
            ValueKind.Int => _intVal,
            ValueKind.Long => (double)_longVal,
            _ => 0.0
        };
        public int AsBool() => Kind switch
        {
            ValueKind.Bool => _boolVal,
            ValueKind.Int => _intVal > 0 ? 1 : (_intVal < 0 ? -1 : 0),
            ValueKind.Long => _longVal > 0 ? 1 : (_longVal < 0 ? -1 : 0),
            ValueKind.Float => _floatVal > 0 ? 1 : (_floatVal < 0 ? -1 : 0),
            ValueKind.Null => 0,
            _ => 0
        };
        public bool IsNull => Kind == ValueKind.Null;

        /// <summary>True if this value should use 36-trit long arithmetic.</summary>
        bool IsLong => Kind == ValueKind.Long;

        public static T3Value operator +(T3Value a, T3Value b)
        {
            if (a.IsLong || b.IsLong)
                return FromLong(a.AsLong() + b.AsLong());
            if (a.Kind == ValueKind.Float || b.Kind == ValueKind.Float)
                return FromFloat(a.AsFloat() + b.AsFloat());
            return FromInt(a.AsInt() + b.AsInt());
        }
        public static T3Value operator -(T3Value a, T3Value b)
        {
            if (a.IsLong || b.IsLong)
                return FromLong(a.AsLong() - b.AsLong());
            if (a.Kind == ValueKind.Float || b.Kind == ValueKind.Float)
                return FromFloat(a.AsFloat() - b.AsFloat());
            return FromInt(a.AsInt() - b.AsInt());
        }
        public static T3Value operator *(T3Value a, T3Value b)
        {
            if (a.IsLong || b.IsLong)
                return FromLong(a.AsLong() * b.AsLong());
            if (a.Kind == ValueKind.Float || b.Kind == ValueKind.Float)
                return FromFloat(a.AsFloat() * b.AsFloat());
            return FromInt(a.AsInt() * b.AsInt());
        }
        public static T3Value operator /(T3Value a, T3Value b)
        {
            if (a.IsLong || b.IsLong)
            {
                Int128 lb = b.AsLong();
                if (lb == 0) throw new DivideByZeroException();
                return FromLong(a.AsLong() / lb);
            }
            if (a.Kind == ValueKind.Float || b.Kind == ValueKind.Float)
            {
                double db = b.AsFloat();
                if (db == 0) throw new DivideByZeroException();
                return FromFloat(a.AsFloat() / db);
            }
            long ib = b.AsInt();
            if (ib == 0) throw new DivideByZeroException();
            return FromInt(a.AsInt() / ib);
        }
        public static T3Value operator %(T3Value a, T3Value b)
        {
            if (a.IsLong || b.IsLong)
            {
                Int128 lb = b.AsLong();
                if (lb == 0) throw new DivideByZeroException();
                return FromLong(a.AsLong() % lb);
            }
            return FromInt(b.AsInt() == 0 ? throw new DivideByZeroException() : a.AsInt() % b.AsInt());
        }

        public T3Value GetElement(int idx) => _arrayVal != null && idx >= 0 && idx < _arrayVal.Length ? _arrayVal[idx] : FromInt(0);
        public void SetElement(int idx, T3Value v) { if (_arrayVal != null && idx >= 0 && idx < _arrayVal.Length) _arrayVal[idx] = v; }
        public T3Value? GetField(string name)
        {
            if (Kind == ValueKind.Union)
                return _unionVal;
            return _structVal != null && _structVal.TryGetValue(name, out var v) ? v : null;
        }
        public void SetField(string name, T3Value v)
        {
            if (Kind == ValueKind.Union)
            {
                _unionVal = v;
                _lastField = name;
                return;
            }
            _structVal ??= new();
            _structVal[name] = v;
        }

        public int ArrayLength => _arrayVal?.Length ?? 0;

        public override string ToString() => Kind switch
        {
            ValueKind.Int => _intVal.ToString(),
            ValueKind.Long => _longVal.ToString(),
            ValueKind.Float => _floatVal.ToString(),
            ValueKind.Bool => _boolVal > 0 ? "true" : (_boolVal < 0 ? "false" : "maybe"),
            ValueKind.Null => "null",
            _ => "void"
        };
    }
}