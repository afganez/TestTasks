using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcApplication5.Controllers
{
    public class ComputeController : Controller
    {
        //
        // GET: /Compute/

        public ActionResult Index()
        {
            ViewBag.Answer = "Выражение не задано. Пример ввода выражения ?x=20+10*(-1)/30";
            if (Request.Url != null)
            {
                var url = HttpUtility.UrlEncode(Request.Url.Query);
                var urlDecode = HttpUtility.UrlDecode(url);
                if (urlDecode != null)
                {
                    var replacedExpression = urlDecode.Replace("%20", "");
                    var x = replacedExpression.Substring(replacedExpression.LastIndexOf("=", System.StringComparison.Ordinal) + 1);
                    if (!String.IsNullOrEmpty(x))
                    {
                        ViewBag.Answer = Calc(x);
                    }
                    
                }
            }
            return View("Compute");
        }
        public double Calc(string s)
        {
            s = '(' + s + ')';
            var operandStack = new Stack<double>();
            var functionsStack = new Stack<char>();
            int pos = 0;
            object symbol;
            object prevSymbol = 'Ё';

            do
            {
                symbol = GetSymbol(s, ref pos);

                // Если у нас имеется знак перед числом (т.к. отрицательное), то добавляем нулевой элемент в стек
                if (symbol is char && prevSymbol is char && (char)prevSymbol == '(' && ((char)symbol == '+' || (char)symbol == '-'))
                    operandStack.Push(0); 

                // Если symbol - операнд
                if (symbol is double)
                {
                    operandStack.Push((double)symbol);
                }
                // Если symbol - операция
                else if (symbol is char)
                {
                    if ((char)symbol == ')')
                    {
                        // Скобка - исключение из правил. Вытаскиваем все операции до первой открывающейся скобки
                        while (functionsStack.Count > 0 && functionsStack.Peek() != '(')
                            popFunction(operandStack, functionsStack);
                        functionsStack.Pop(); // Удаляем саму скобку "("
                    }
                    else
                    {
                        while (CanPop((char)symbol, functionsStack)) // Если можно вытащить из стека, то вытаскиваем
                            popFunction(operandStack, functionsStack);

                        functionsStack.Push((char)symbol); // Заносим новую операцию в стек
                    }
                }
                prevSymbol = symbol;
            }
            while (symbol != null);

            if (operandStack.Count > 1 || functionsStack.Count > 0)
                throw new Exception("Ошибка при разборе выражения");

            return operandStack.Pop();
        }

        private object GetSymbol(string s, ref int pos)
        {
            readWhiteSpace(s, ref pos);

            if (pos == s.Length) // Если конец строки, то возвращаем null
                return null;
            if (char.IsDigit(s[pos]))
                return Convert.ToDouble(readDouble(s, ref pos));
            else
                return readFunction(s, ref pos);
        }

        private void popFunction(Stack<double> operandStack, Stack<char> functionsStack)
        {
            double b = operandStack.Pop();
            double a = operandStack.Pop();
            switch (functionsStack.Pop())
            {
                case '+': operandStack.Push(a + b);
                    break;
                case '-': operandStack.Push(a - b);
                    break;
                case '*': operandStack.Push(a * b);
                    break;
                case '/': operandStack.Push(a / b);
                    break;
            }
        }

        private bool CanPop(char operation, Stack<char> functionsStack)
        {
            if (functionsStack.Count == 0)
                return false;
            int p1 = getPriority(operation);
            int p2 = getPriority(functionsStack.Peek());

            return p1 >= 0 && p2 >= 0 && p1 >= p2;
        }

        private int getPriority(char operation)
        {
            switch (operation)
            {
                case '(':
                    return -1; // не выталкивает сам и не дает вытолкнуть себя другим
                case '*':
                case '/':
                    return 1;
                case '+':
                case '-':
                    return 2;
                default:
                    throw new Exception("Обнаружена недопустимая операция");
            }
        }

        private char readFunction(string s, ref int pos)
        {
            // Так как все операции состоят из одного символа прибавляем только на 1
            return s[pos++];
        }

        private string readDouble(string s, ref int pos)
        {
            string res = "";
            while (pos < s.Length && (char.IsDigit(s[pos]) || s[pos] == '.'))
                res += s[pos++];

            return res;
        }

        // Считываем все проблемы и прочие символы.
        private void readWhiteSpace(string s, ref int pos)
        {
            while (pos < s.Length && char.IsWhiteSpace(s[pos]))
                pos++;
        }
    }
}
