using System;

namespace EcsLib.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class EcsInjectAttribute : Attribute
    { }
}