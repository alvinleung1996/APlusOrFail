using UnityEngine;
using System.Collections;

namespace APlusOrFail.Objects
{
    public interface IObjectPlayerSource
    {
        Player player { get; set; }
    }
}
