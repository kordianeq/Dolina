using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class ShowIfAttribute : PropertyAttribute
{
    public string ConditionalSourceField;

    public ShowIfAttribute(string conditionalSourceField)
    {
        ConditionalSourceField = conditionalSourceField;
    }
}