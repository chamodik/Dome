using System;

// Editor-discoverable marker for methods that may
// be selected through PXR_UnifiedInputs.
//
// This is not a network authorization mechanism.
// It controls which methods appear in the Inspector.

[AttributeUsage(
    AttributeTargets.Method,
    AllowMultiple = false,
    Inherited = false)]
public sealed class PXR_InputCallableAttribute : Attribute
{
}