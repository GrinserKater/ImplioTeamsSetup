using ImplioTeamsSetup.Entities.Interfaces;
using System;
using Newtonsoft.Json;

namespace ImplioTeamsSetup.Entities
{
    public class ListEntity : IEntity
    {
        [JsonProperty("name")]
        public string   Id               { get; set; }
        public string   Domain           { get; set; }
        public DateTime CreatedTime      { get; set; }
        public string   CreatedBy        { get; set; }
        public DateTime LastModifiedTime { get; set; }
        public string   LastModifiedBy   { get; set; }
    }
}
