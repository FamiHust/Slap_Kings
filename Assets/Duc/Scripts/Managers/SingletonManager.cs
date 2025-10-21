using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Duc
{
    /// <summary>
    /// Legacy singleton manager - use EnhancedSingletonManager for new code
    /// </summary>
    public abstract class SingletonManager<T> : EnhancedSingletonManager<T> where T: MonoBehaviour
    {
        // Legacy Get() method for backward compatibility
        public static T Get()
        {
            return Instance;
        }
    }
}
