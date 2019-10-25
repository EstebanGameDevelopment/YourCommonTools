using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace YourCommonTools
{
    interface IInterpolateData
    {
        GameObject GameActor { get; }
        object Goal { get; set; }
        float TotalTime { get; set; }
        int TypeData { get; }

        void Destroy();
        bool Inperpolate();
        void ResetData(Transform _origin, object _goal, float _totalTime, float _timeDone);

    }
}
