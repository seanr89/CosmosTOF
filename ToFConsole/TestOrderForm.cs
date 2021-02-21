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
        //New records being included
        public DateTime SwabCollectionDate { get; private set; }
        public DateTime ArrivalDate { get; private set; }
        public string Source { get; private set; }

        //Planned new content for testing
        public Dictionary<string, CustomContent> MetaData { get; set; } = new Dictionary<string, CustomContent>();
        public string Type => this.GetType().Name;

        private TestOrderForm()
        {
        }

        public TestOrderForm(string profile, DateTime date, string source) : base()
        {
            Id = Guid.NewGuid();
            Profile = profile;
            Date = date;
            Source = source;
        }

        /// <summary>
        /// overridding the ToString to support easier string conversion for base content
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"[{Id}] : {Profile}";
        }
    }
}