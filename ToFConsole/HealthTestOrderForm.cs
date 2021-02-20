using System;
using Newtonsoft.Json;

namespace ToFConsole
{
    public class HealthTestOrderForm : TestOrderForm
    {
        public string PID { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}