using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Duc
{
    public abstract class SingletonManager<T> : EnhancedSingletonManager<T> where T: MonoBehaviour
    {
        public static T Get()
        {
            return Instance;
        }
    }
}
