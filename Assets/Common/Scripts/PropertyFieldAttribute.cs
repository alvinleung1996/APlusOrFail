using System;

namespace APlusOrFail
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EditorPropertyFieldAttribute : Attribute
    {
        public bool forceGet { get; set; }
        public bool forceSet { get; set; }
    }
}
