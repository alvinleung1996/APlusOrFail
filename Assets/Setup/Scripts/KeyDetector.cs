using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace APlusOrFail
{
    public static class KeyDetector
    {
        private static readonly ReadOnlyCollection<KeyCode> keys;


        static KeyDetector()
        {
            keys = new ReadOnlyCollection<KeyCode>(((IEnumerable<KeyCode>)Enum.GetValues(typeof(KeyCode)))
                    .Where(code => code != KeyCode.Mouse0 && code != KeyCode.Mouse1 && code != KeyCode.Mouse2 &&
                                   code != KeyCode.Mouse3 && code != KeyCode.Mouse4 && code != KeyCode.Mouse5 && code != KeyCode.Mouse6).ToList());
        }


        public static KeyCode GetKeyDowned()
        {
            return keys
                .SkipWhile(code => !Input.GetKeyDown(code))
                .DefaultIfEmpty(KeyCode.None)
                .First();
        }
    }
}
