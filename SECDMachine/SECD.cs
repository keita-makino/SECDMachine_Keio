using System.Collections.Generic;

namespace SECDMachine
{
    internal class SECD
    {
        public List<string> State { get; set; }
        public List<string> Environment { get; set; }
        public List<string> Control { get; set; }
        public Dump Dump { get; set; }
    }

    public class Dump
    {
        public string Body { get; set; }
        public int NestCount { get; set; }
    }
}