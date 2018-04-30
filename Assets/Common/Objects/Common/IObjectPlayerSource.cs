using UnityEngine;
using System.Collections;

namespace APlusOrFail.Objects
{
    public interface IObjectPlayerSource
    {
        IReadOnlyPlayerSetting player { get; set; }
    }
}
