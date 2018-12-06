using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SECDMachine
{
    class Program
    {
        static void Main(string[] args)
        {
            SECD secd = new SECD();
            secd.State = new List<string> { "" };
            secd.Dump = new Dump();
            secd.Dump.Body = "D0";
            secd.Environment = new List<string> {""};
            //secd.Control = new List<string> { @"((Lx. x) a)" };
            //secd.Control = new List<string> { @"((Lx. Ly. (x y))((Lx. Ly. (x y))(Lx. Ly. y)))" };
            secd.Control = new List<string> { @"((Lx. Lz. (z y))((Lx. Ly. (Lx. Lz. (z y)))(Lx. Ly. z)))" };

            while (secd.Control.Count() > 0 || secd.Dump.Body != "D0")
            {
                Console.WriteLine("State:");
                foreach(var item in secd.State)
                {
                    if (item != "")
                    {
                        Console.WriteLine(item);
                    }
                }
                Console.WriteLine("Environment:");
                foreach (var item in secd.Environment)
                {
                    if (item != "")
                    {
                        Console.WriteLine(item);
                    }
                }
                Console.WriteLine("Control:");
                foreach (var item in secd.Control)
                {
                    if (item != "")
                    {
                        Console.WriteLine(item);
                    }
                }
                Console.WriteLine("Dump:");
                Console.WriteLine(secd.Dump.Body);
                Console.WriteLine("\n");

                if (secd.Control.Count() == 0)
                {
                    RecoverFromDump(secd);
                    continue;
                }
                
                if (secd.Control.First() == "ap")
                {
                    DoAp(secd);
                    continue;
                }

                if (JudgeAbs(secd.Control.First()))
                {
                    if (secd.Environment.Count() == 0)
                    {
                        DoAbs(secd.State, "", secd.Control);
                    }
                    else
                    {
                        DoAbs(secd.State, secd.Environment.First(), secd.Control);
                    }
                    continue;
                }

                if (JudgeApp(secd.Control.First()))
                {
                    DoApp(secd.Control);
                    continue;
                }
                Evaluation(secd);
            }
            Console.WriteLine("State:");
            foreach (var item in secd.State)
            {
                if (item != "")
                {
                    Console.WriteLine(item);
                }
            }
            Console.WriteLine("Environment:");
            foreach (var item in secd.Environment)
            {
                if (item != "")
                {
                    Console.WriteLine(item);
                }
            }
            Console.WriteLine("Control:");
            foreach (var item in secd.Control)
            {
                if (item != "")
                {
                    Console.WriteLine(item);
                }
            }
            Console.WriteLine("Dump:");
            Console.WriteLine(secd.Dump.Body);
            Console.WriteLine("\n");
            Console.WriteLine("The Result is:");
            Console.WriteLine(secd.State.First());
            Console.ReadKey();
        }

        static internal void Evaluation(SECD secd)
        {
            var targetOfEvalation = secd.Control.First();
            var searchStr = "<" + targetOfEvalation + "=";

            var indexOfTarget = secd.Environment.First().IndexOf(searchStr);

            var evaluationResult = "";

            if (indexOfTarget == -1)
            {
                secd.State.Insert(0, targetOfEvalation);
                if (secd.State.Last() == "")
                {
                    secd.State.RemoveAt(secd.State.Count() - 1);
                }
            }
            else
            {
                evaluationResult = secd.Environment.First().Substring(searchStr.Length, secd.Environment.First().Length - searchStr.Length -1);
                secd.State.Insert(0, evaluationResult);
            }
            secd.Control.RemoveAt(0);

        }

        static internal void RecoverFromDump(SECD secd)
        {
            var nestedCount = secd.Dump.NestCount;
            var dumpDivider =  "/s" + (nestedCount - 1) + "/";
            var stateDivider = "/ss" + (nestedCount - 1) + "/";
            var envDivider = "/se" + (nestedCount - 1) + "/";
            var controlDivider = "/sc" + (nestedCount - 1) + "/";
            var stateListFromDump = new List<string> { };

            secd.State =new List<string> { secd.State.First() };
                foreach (var str in secd.Dump.Body.Split(new String[] { dumpDivider }, StringSplitOptions.None)[0].Split(new String[] { stateDivider }, StringSplitOptions.None))
                {
                    if (str != "")
                    {
                        secd.State.Add(str);
                    }
            }
            secd.Environment.Clear();
            foreach (var str in secd.Dump.Body.Split(new String[] { dumpDivider }, StringSplitOptions.None)[1].Split(new String[] { envDivider }, StringSplitOptions.None))
            {
                if (str != "")
                {
                    secd.Environment.Add(str);
                }
            }
            foreach (var str in secd.Dump.Body.Split(new String[] { dumpDivider }, StringSplitOptions.None)[2].Split(new String[] { controlDivider }, StringSplitOptions.None))
            {
                if (str != "")
                {
                    secd.Control.Add(str);
                }
            }
            secd.Dump.Body = secd.Dump.Body.Split(new String[] { dumpDivider }, StringSplitOptions.None)[3];
            secd.Dump.NestCount--;
        }

        static internal bool JudgeApp(string control)
        {
            string str = "";
            if (control.StartsWith(@"(") && control.EndsWith(@")"))
            {
                str = control.Substring(1, control.Length - 2);
            }

            var indexOfDivision = -1;

            for (int i = 0; i < str.Length; i++)
            {
                var substr = str.Substring(0, i);

                MatchCollection m1 = System.Text.RegularExpressions.Regex.Matches(substr, @"\(");
                MatchCollection m2 = System.Text.RegularExpressions.Regex.Matches(substr, @"\)");

                if (m1.Count == m2.Count && m1.Count > 0)
                {
                    indexOfDivision = i;
                    break;
                }
            }
            if (indexOfDivision == str.Length-1)
            {
                return false;
            }
            return true;
        }

        static internal bool JudgeAbs(string control)
        {
            if (control.StartsWith(@"(L"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static internal void DoAbs(List<string> state, string env, List<string> control)
        {
            string firstChd = control.First().Substring(1, control.First().Length - 2);
            control.RemoveAt(0);

            var indexOfFirstPeriod = firstChd.IndexOf('.');

            string newState = "";

            var earlierStr = firstChd.Substring(0, indexOfFirstPeriod).Trim('L', ' ');
            var latterStr = firstChd.Substring(indexOfFirstPeriod + 1).Trim(' ');


            if (latterStr.StartsWith(@"(") && latterStr.EndsWith(@")"))
            {
                latterStr = latterStr.Substring(1, latterStr.Length - 2);
            }
            newState = "<" + earlierStr + ",(" + latterStr + "),[" + env + "]>";
            state.Insert(0, newState);
        }
        
        static internal void DoApp(List<string> control)
        {
            string firstChd = control.First().Substring(1, control.First().Length - 2);
            control.RemoveAt(0);
            
            var indexOfDivision = -1;

            for (int i = 0; i < firstChd.Length; i++)
            {
                var substr = firstChd.Substring(0, i);

                MatchCollection m1 = Regex.Matches(substr, @"\(");
                MatchCollection m2 = Regex.Matches(substr, @"\)");

                if (m1.Count == m2.Count && m1.Count>0)
                {
                    indexOfDivision = i;
                    break;
                }
            }
            
            if (indexOfDivision > 0)
            {
                control.Insert(0, firstChd.Substring(0, indexOfDivision).Trim());
                control.Insert(0, firstChd.Substring(indexOfDivision).Trim());
            }

            control.Insert(2, "ap");
        }

        static internal void DoAp(SECD secd)
        {
            var stateToDump = "";
            var envToDump = "";
            var controlToDump = "";
            var dumpToDump = secd.Dump.Body;

            var nestCount = secd.Dump.NestCount;
            var dumpDivider = "/s" + nestCount + "/";
            var stateDivider = "/ss" + nestCount + "/";
            var envDivider = "/se" + nestCount + "/";
            var controlDivider = "/sc" + nestCount + "/";

            if (secd.State.Last() == "")
            {
                secd.State.RemoveAt(secd.State.Count() - 1);
            }

            if (secd.Environment.Count != 0)
            {
                if (secd.Environment.Last() == "")
                {
                    secd.Environment.RemoveAt(secd.Environment.Count() - 1);
                }
            }

            if (secd.State.Count() > 2)
            {
                foreach (var str in secd.State.Skip(2).Take(secd.State.Count() - 2).ToList())
                {
                    stateToDump = stateToDump + str + stateDivider;
                }
            }
            else
            {
                stateToDump = null;
            }

            if (secd.Environment.Count() > 0)
            {
                foreach (var str in secd.Environment)
                {
                    envToDump = envToDump + str + envDivider;
                }
            }
            else
            {
                envToDump = null;
            }

            if (secd.Control.Count() > 1)
            {
                foreach (var str in secd.Control.Skip(1).Take(secd.Control.Count() - 1).ToList())
                {
                    controlToDump = controlToDump + str + controlDivider;
                }
            }
            else
            {
                controlToDump = null;
            }

            secd.Dump.NestCount++;
            secd.Dump.Body = stateToDump + dumpDivider + envToDump + dumpDivider + controlToDump + dumpDivider + dumpToDump;

            var bvX = secd.State.First().Split(',').First().Trim('<');

            secd.Environment.Insert(0, "<" + bvX + "=" + secd.State[1] + ">");

            secd.Control.Clear();

            var newControlStr = secd.State.First().Split(',')[1].Trim();

            if (newControlStr.IndexOf(' ') == -1)
            {
                newControlStr = newControlStr.Substring(1, newControlStr.Length - 2);
            }
            secd.Control.Add(newControlStr);
            secd.State = new List<string> { "" };
        }
    }
}
