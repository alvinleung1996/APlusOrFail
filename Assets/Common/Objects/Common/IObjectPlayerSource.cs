using UnityEngine;
using System.Collections;

namespace APlusOrFail.Objects
{
    public interface IObjectPlayerSource
    {
        IReadOnlySharedPlayerSetting player { get; set; }
    }
}
