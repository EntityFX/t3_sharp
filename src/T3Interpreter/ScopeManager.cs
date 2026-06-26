using System;
using System.Collections.Generic;
using T3Compiler.Parser;

namespace T3Interpreter
{
    /// <summary>Manages nested lexical scopes for the T-lang interpreter.</summary>
    public class ScopeManager
    {
        readonly Stack<Dictionary<string, T3Value>> _scopes = new();
        readonly Dictionary<string, int> _enumValues;
        readonly Dictionary<string, StructDef> _structDefs;

        public ScopeManager(Dictionary<string, int> enumValues, Dictionary<string, StructDef> structDefs)
        {
            _enumValues = enumValues;
            _structDefs = structDefs;
        }

        public void PushScope() => _scopes.Push(new Dictionary<string, T3Value>());
        public void PopScope() { if (_scopes.Count > 0) _scopes.Pop(); }

        public T3Value GetVar(string name)
        {
            if (_enumValues.TryGetValue(name, out int ev)) return T3Value.FromInt(ev);
            foreach (var scope in _scopes)
            {
                if (scope.TryGetValue(name, out var v)) return v;
            }
            throw new Exception($"Undefined variable: {name}");
        }

        public void SetVar(string name, T3Value val)
        {
            if (_scopes.Count > 0) _scopes.Peek()[name] = val;
        }

        public void SetArrayElement(string name, int idx, T3Value val)
        {
            GetVar(name).SetElement(idx, val);
        }

        public bool HasVariable(string name)
        {
            if (_enumValues.ContainsKey(name)) return true;
            foreach (var scope in _scopes) { if (scope.ContainsKey(name)) return true; }
            return false;
        }
    }
}