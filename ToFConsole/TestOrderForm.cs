using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ToFConsole
{
    public class TestOrderForm
    {

        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }
        public string Profile { get; private set; }
        public DateTime Date { get; private set; }
        public string Source { get; private set; }

        //Planned new content for testing
        public Dictionary<string, CustomContent> MetaData { get; set; } = new Dictionary<string, CustomContent>();

        private TestOrderForm()
        {

        }

        public TestOrderForm(string profile, DateTime date, string source)
        {
            Profile = profile;
            Date = date;
            Source = source;
        }
    }
}