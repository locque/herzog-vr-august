using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Valve.VR.InteractionSystem
{
    public class PageHandleController : BookComponent
    {
        private PageHandle[] pageHandles = null;

        public PageHandle[] PageHandles
        {
            get { return pageHandles; }
        }

        void Awake()
        {
            base.Start();
        }

        public void GetHandles() {
            pageHandles = GetComponentsInChildren<PageHandle>();
        }

        public void SetHandlesActive(bool active, params PageHandle.Type[] handleTypes)
        {
            foreach (var pageHandle in pageHandles)
            {
                if (Array.Exists(handleTypes, handleType => handleType == pageHandle.handleType))
                {
                    pageHandle.SetActive(active);
                }
            }
        }

        public void SetHandlesActive(bool active)
        {
            foreach (var pageHandle in pageHandles)
            {
                pageHandle.SetActive(active);
            }
        }

        void FixedUpdate()
        {
            foreach (var pageHandle in pageHandles)
            {
                switch (pageHandle.handleType)
                {
                    case PageHandle.Type.Back:
                        pageHandle.transform.position = Book.backPageHandleTransform.position;
                        break;
                    case PageHandle.Type.Cover:
                        pageHandle.transform.position = Book.coverPageHandleTransform.position;
                        break;
                }
            }
        }

    }
}