using System;
using Newtonsoft.Json;

namespace ToFConsole
{
    public class TestOrderForm
    {

        [JsonProperty(PropertyName = "id")]
        public string Guid { get; set; }
        public string Profile { get; private set; }
        public DateTime Date { get; private set; }
        public string Source { get; private set; }
    }
}