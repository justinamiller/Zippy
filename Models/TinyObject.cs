using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class TinyObject
    {
       public string Name { get; }
       public int Id { get; }
        public State StateType { get; }
        public enum State
        {
            Unknown=0,
            Running=1
        }

        public TinyObject(string name, int id, State state)
        {
            this.Name = name;
            this.Id = id;
            this.StateType = state;
        }
    }
}
