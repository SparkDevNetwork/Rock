using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.FieldTypes
{
    public class FieldQualifier
    {
        public string Key { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public Field Field { get; private set; }

        public FieldQualifier( string key, string name, string description, Field field )
        {
            Key = key;
            Name = name;
            Description = description;
            Field = field;
        }
    }
}