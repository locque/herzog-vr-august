﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: A linear mapping value that is used by other components
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class LinearMapping : MonoBehaviour
    {
        private float current;
        private float last;
        public float value
        {
            set
            {
                last = current;
                current = value;
            }
            get { return current; }
        }

        public float lastValue
        {
            get { return last; }
        }

    }
}
