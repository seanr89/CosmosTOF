using System;
using Newtonsoft.Json;

namespace ToFConsole
{
    public class HealthTestOrderForm : TestOrderForm
    {
        public HealthTestOrderForm(string profile, DateTime date, string source) : base(profile, date, source)
        {
        }

        public string PID { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Sex { get; set; }
    }
}