using System;
using System.Collections.Generic;
using System.Linq;
using T3Compiler.Parser;
using TritTypes;

namespace T3Interpreter
{
    public class T3Interpreter
    {
        readonly AstProgram _program;
        readonly Stack<Dictionary<string, T3Value>> _scopes = new();
        readonly Dictionary<string, FunctionDef> _functions = new(StringComparer.OrdinalIgnoreCase);
        T3Value _returnValue = T3Value.Void;
        bool _didReturn, _didBreak, _didContinue;

        public T3Interpreter(AstProgram program)
        {
            _program = program;
            foreach (var f in program.Functions) _functions[f.Name] = f;
            foreach (var s in program.Structs) _structDefs[s.Name] = s;
            foreach (var ed in program.Enums) { int cur = 0; foreach (var m in ed.Members) { _enumValues[m.Name] = m.Value ?? cur; cur = _enumValues[m.Name] + 1; } }
        }

        public long Run()
        {
            if (!_functions.TryGetValue("main", out var main)) throw new Exception("main() not found");
            _scopes.Push(new());
            // Initialize globals
            foreach (var g in _program.Globals)
            {
                T3Value init = T3Value.FromInt(0);
                if (g.Type.Dims.Count > 0)
                {
                    int sz = g.Type.Dims.Aggregate(1, (a, b) => a * b);
                    init = T3Value.FromArray(sz);
                }
                else if (g.Type.StructName != null)
                {
                    init = T3Value.FromStruct();
                }
                if (g.Initializer != null) init = Eval(g.Initializer);
                _scopes.Peek()[g.Name] = init;
            }
            EvalStmt(main.Body);
            return _returnValue.AsInt();
        }

        T3Value Eval(AstNode n)
        {
            if (n is IntegerLiteral il) return T3Value.FromInt(ParseInt(il.Value));
            if (n is BooleanLiteral bl) return T3Value.FromBool(bl.Value);
            if (n is Identifier id) return GetVar(id.Name);
            if (n is BinaryOp bo) return EvalBin(bo);
            if (n is UnaryOp uo) return EvalUn(uo);
            if (n is Assignment ass) return EvalAssign(ass);
            if (n is FunctionCall fc) return EvalCall(fc);
            if (n is TernaryExpr te) return EvalTernary(te);
            if (n is ArrayAccess aa) return EvalArrayAccess(aa);
            if (n is MemberAccess ma) return EvalMemberAccess(ma);
            if (n is StringLiteral) return T3Value.FromInt(0);
            if (n is FloatLiteral) return T3Value.FromInt(0);
            throw new NotSupportedException(n.GetType().Name);
        }

        T3Value EvalBin(BinaryOp bo)
        {
            var l = Eval(bo.Left); var r = Eval(bo.Right);
            return bo.Operator switch
            {
                "+" => l + r, "-" => l - r, "*" => l * r, "/" => l / r, "%" => l % r,
                "==" => T3Value.FromBool(l.AsInt() == r.AsInt() ? 1 : -1),
                "!=" => T3Value.FromBool(l.AsInt() != r.AsInt() ? 1 : -1),
                "<" => T3Value.FromBool(l.AsInt() < r.AsInt() ? 1 : -1),
                ">" => T3Value.FromBool(l.AsInt() > r.AsInt() ? 1 : -1),
                "<=" => T3Value.FromBool(l.AsInt() <= r.AsInt() ? 1 : -1),
                ">=" => T3Value.FromBool(l.AsInt() >= r.AsInt() ? 1 : -1),
                "&&" => T3Value.FromBool(l.AsBool() > 0 && r.AsBool() > 0 ? 1 : -1),
                "||" => T3Value.FromBool(l.AsBool() > 0 || r.AsBool() > 0 ? 1 : -1),
                "&" => T3Value.FromInt(l.AsInt() & r.AsInt()),
                "|" => T3Value.FromInt(l.AsInt() | r.AsInt()),
                "^" => T3Value.FromInt(l.AsInt() ^ r.AsInt()),
                "<<" => T3Value.FromInt(l.AsInt() << (int)r.AsInt()),
                ">>" => T3Value.FromInt(l.AsInt() >> (int)r.AsInt()),
                _ => throw new NotImplementedException(bo.Operator)
            };
        }

        T3Value EvalUn(UnaryOp uo)
        {
            var v = Eval(uo.Operand);
            return uo.Operator switch { "-" => T3Value.FromInt(-v.AsInt()), "!" => T3Value.FromBool(v.AsBool() > 0 ? -1 : 1), "*" => v, "&" => v, _ => v };
        }

        T3Value EvalAssign(Assignment ass)
        {
            var val = Eval(ass.Value);
            if (ass.Operator == "=") { SetVarId(ass.Target, val); return val; }
            var cur = Eval(ass.Target);
            T3Value newVal = ass.Operator switch { "+=" => cur + val, "-=" => cur - val, "*=" => cur * val, "/=" => cur / val, "%=" => cur % val, _ => val };
            SetVarId(ass.Target, newVal);
            return newVal;
        }

        void SetVarId(AstNode target, T3Value val)
        {
            if (target is Identifier id) SetVar(id.Name, val);
            else if (target is ArrayAccess aa) { SetArrayElement(aa.ArrayName, (int)Eval(aa.Indices[0]).AsInt(), val); }
        }

        T3Value EvalCall(FunctionCall fc)
        {
            if (fc.FunctionName == "putchar") { Console.Write((char)Eval(fc.Arguments[0]).AsInt()); return T3Value.FromInt(0); }
            if (fc.FunctionName == "printint") { Console.Write(Eval(fc.Arguments[0]).AsInt()); return T3Value.FromInt(0); }
            if (fc.FunctionName == "strlen") return T3Value.FromInt(0);

            if (!_functions.TryGetValue(fc.FunctionName, out var f)) throw new Exception($"Undefined function: {fc.FunctionName}");
            var frame = new Dictionary<string, T3Value>();
            for (int i = 0; i < f.Parameters.Count && i < fc.Arguments.Count; i++)
                frame[f.Parameters[i].Name] = Eval(fc.Arguments[i]);
            _scopes.Push(frame);
            var savedRet = _returnValue; var savedDid = _didReturn;
            _returnValue = T3Value.Void; _didReturn = false;
            EvalStmt(f.Body);
            var ret = _returnValue;
            _scopes.Pop();
            _returnValue = savedRet; _didReturn = savedDid;
            return ret;
        }

        T3Value EvalTernary(TernaryExpr te)
        {
            var c = Eval(te.Condition);
            if (c.AsBool() > 0) return Eval(te.TrueExpr);
            if (c.AsBool() == 0) return Eval(te.MaybeExpr);
            return Eval(te.FalseExpr);
        }

        T3Value EvalArrayAccess(ArrayAccess aa) => GetVar(aa.ArrayName).GetElement((int)Eval(aa.Indices[0]).AsInt());
        T3Value EvalMemberAccess(MemberAccess ma)
        {
            if (ma.Object is Identifier id) return GetVar(id.Name).GetField(ma.MemberName) ?? T3Value.FromInt(0);
            return T3Value.FromInt(0);
        }

        void EvalStmt(Statement s)
        {
            if (_didReturn) return;
            switch (s)
            {
                case ExpressionStmt es: if (es.Expression != null) Eval(es.Expression); break;
                case CompoundStmt cs: foreach (var st in cs.Body) { EvalStmt(st); if (_didReturn) return; } break;
                case VarDeclaration vd:
                    T3Value defaultVal;
                    if (vd.Type.Dims.Count > 0)
                    {
                        int sz = vd.Type.Dims.Aggregate(1, (a, b) => a * b);
                        defaultVal = T3Value.FromArray(sz);
                    }
                    else if (vd.Type.StructName != null)
                    {
                        defaultVal = T3Value.FromStruct();
                    }
                    else
                    {
                        defaultVal = T3Value.FromInt(0);
                    }
                    SetVar(vd.Name, vd.Initializer != null ? Eval(vd.Initializer) : defaultVal);
                    break;
                case ReturnStmt rs: _returnValue = rs.Value != null ? Eval(rs.Value) : T3Value.FromInt(0); _didReturn = true; break;
                case IfStmt ifs: EvalIf(ifs); break;
                case WhileStmt ws: EvalWhile(ws); break;
                case DoWhileStmt dws: EvalDoWhile(dws); break;
                case ForStmt fs: EvalFor(fs); break;
                case SwitchStmt ss: EvalSwitch(ss); break;
                case BreakStmt: throw new BreakException();
                case ContinueStmt: throw new ContinueException();
            }
        }

        void EvalIf(IfStmt s) { var c = Eval(s.Condition); if (c.AsBool() > 0) EvalStmt(s.ThenBody); else if (c.AsBool() == 0 && s.MaybeBody != null) EvalStmt(s.MaybeBody); else if (s.ElseBody != null) EvalStmt(s.ElseBody); }
        void EvalWhile(WhileStmt s) { while (Eval(s.Condition).AsBool() > 0) { try { EvalStmt(s.Body); } catch (BreakException) { break; } catch (ContinueException) { } } }
        void EvalDoWhile(DoWhileStmt s) { do { try { EvalStmt(s.Body); } catch (BreakException) { break; } catch (ContinueException) { } } while (Eval(s.Condition).AsBool() > 0); }
        void EvalFor(ForStmt s) { if (s.Init != null) EvalStmt(s.Init); while (s.Condition == null || Eval(s.Condition).AsBool() > 0) { try { EvalStmt(s.Body); } catch (BreakException) { break; } catch (ContinueException) { } if (s.Step != null) Eval(s.Step); } }
        void EvalSwitch(SwitchStmt s) { var expr = Eval(s.Expression); bool matched = false; foreach (var cs in s.Cases) { if (!matched && (cs.Value == null || Eval(cs.Value).AsInt() == expr.AsInt())) matched = true; if (matched) foreach (var st in cs.Body) { EvalStmt(st); if (_didReturn) return; } } }

        T3Value GetVar(string name)
        {
            if (_enumValues.TryGetValue(name, out int ev)) return T3Value.FromInt(ev);
            // Lexical scoping: check top scope first, then walk down
            var arr = _scopes.ToArray();
            for (int i = arr.Length - 1; i >= 0; i--)
            {
                if (arr[i].TryGetValue(name, out var v)) return v;
            }
            throw new Exception($"Undefined variable: {name}");
        }
        void SetVar(string name, T3Value val) { if (_scopes.Count > 0) _scopes.Peek()[name] = val; }
        void SetArrayElement(string name, int idx, T3Value val) { GetVar(name).SetElement(idx, val); }

        static long ParseInt(string v) => LiteralParser.ParseInt(v);

        readonly Dictionary<string, int> _enumValues = new();
        readonly Dictionary<string, StructDef> _structDefs = new();
    }

    class BreakException : Exception { }
    class ContinueException : Exception { }
}