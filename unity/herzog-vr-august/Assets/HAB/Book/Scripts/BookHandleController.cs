using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public class BookHandleController : MonoBehaviour
    {
        private BookHandle[] bookHandles = null;

        public void GetHandles()
        {
            bookHandles = GetComponentsInChildren<BookHandle>();
            // Debug.Log(this + " " + bookHandles.Length);
        }

        public BookHandle[] GetBookHandles()
        {
            return bookHandles;
        }
    }
}