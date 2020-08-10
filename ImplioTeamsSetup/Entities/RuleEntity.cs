using ImplioTeamsSetup.Entities.Interfaces;
using System;

namespace ImplioTeamsSetup.Entities
{
    public class RuleEntity : IEntity
    {
        public bool     Enabled { get; set; }
        public string   Name    { get; set; }
        public DateTime CreatedTime { get; set; }
        public string   CreatedBy { get; set; }

        public string   Domain { get; set; }
        
        public string   Id { get; set; }
    }
}
