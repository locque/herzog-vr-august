using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public class BookComponent : MonoBehaviour
    {
        private BookIndex book = null;

        public BookIndex Book
        {
            get { return book; }
        }

        protected virtual void Start()
        {
            if ((book = GetComponentInParent<BookIndex>()) == null)
            {
                Debug.LogError("Could not find Book Index in parents.");
            }
        }
    }
}