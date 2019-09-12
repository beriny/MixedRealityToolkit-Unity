﻿
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI.BoundingBoxTypes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.UI
{
    [Serializable]
    public class BoundingBoxScaleHandles : BoundingBoxHandlesBase
    {

        [SerializeField]
        [Tooltip("Prefab used to display handles for 2D slate. If not set, default box shape will be used")]
        GameObject handleSlatePrefab = null;

        /// <summary>
        /// Prefab used to display handles for 2D slate. If not set, default box shape will be used
        /// </summary>
        public GameObject HandleSlatePrefab
        {
            get { return handleSlatePrefab; }
            set
            {
                if (handleSlatePrefab != value)
                {
                    handleSlatePrefab = value;
                    configurationChanged.Invoke();
                }
            }
        }

        [SerializeField]
        [Tooltip("Check to show scale handles")]
        private bool showScaleHandles = true;

        /// <summary>
        /// Public property to Set the visibility of the corner cube Scaling handles.
        /// This property can be set independent of the Rotate handles.
        /// </summary>
        public bool ShowScaleHandles
        {
            get
            {
                return showScaleHandles;
            }
            set
            {
                if (showScaleHandles != value)
                {
                    showScaleHandles = value;
                    visibilityChanged.Invoke();
                }
            }
        }

        internal void UpdateVisibilityInInspector(HideFlags flags)
        {
            if (handles != null)
            {
                foreach (var cube in handles)
                {
                    cube.hideFlags = flags;
                }
            }
        }

        public override bool IsVisible(Transform handle)
        {
            return ShowScaleHandles;
        }

        public override bool IsHandleTypeActive()
        {
            return ShowScaleHandles;
        }

        //internal void DestroyHandles()
       // {
        ////    boundsCorners = new Vector3[8];
        //    base.Init();
       // }
        


        internal void UpdateHandles(ref Vector3[] boundsCorners)
        {
            for (int i = 0; i < handles.Count; ++i)
            {
                handles[i].position = boundsCorners[i];
            }
        }



        public override HandleType GetHandleType()
        {
            return HandleType.Scale;
        }

        internal void CreateHandles(Transform parent, ref Vector3[] boundsCorners, bool drawManipulationTether, bool isFlattened)
        {
            // ensure materials are set
            SetMaterials();

            // create corners
            for (int i = 0; i < boundsCorners.Length; ++i)
            {
                GameObject corner = new GameObject
                {
                    name = "corner_" + i.ToString()
                };
                corner.transform.parent = parent;
                corner.transform.localPosition = boundsCorners[i];

                GameObject visualsScale = new GameObject();
                visualsScale.name = "visualsScale";
                visualsScale.transform.parent = corner.transform;
                visualsScale.transform.localPosition = Vector3.zero;

                // Compute mirroring scale
                {
                    Vector3 p = boundsCorners[i];
                    visualsScale.transform.localScale = new Vector3(Mathf.Sign(p[0]), Mathf.Sign(p[1]), Mathf.Sign(p[2]));
                }

                // figure out which prefab to instantiate
                GameObject cornerVisual = null;
                GameObject prefabType = isFlattened ? HandleSlatePrefab : HandlePrefab;
                if (prefabType == null)
                {
                    // instantiate default prefab, a cube. Remove the box collider from it
                    cornerVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cornerVisual.transform.parent = visualsScale.transform;
                    cornerVisual.transform.localPosition = Vector3.zero;
                    GameObject.Destroy(cornerVisual.GetComponent<BoxCollider>());
                }
                else
                {
                    cornerVisual = GameObject.Instantiate(prefabType, visualsScale.transform);
                }

                if (isFlattened)
                {
                    // Rotate 2D slate handle asset for proper orientation
                    cornerVisual.transform.Rotate(0, 0, -90);
                }

                cornerVisual.name = "visuals";

                // this is the size of the corner visuals
                var cornerbounds = BoundingBoxHandleUtils.GetMaxBounds(cornerVisual);
                float maxDim = Mathf.Max(Mathf.Max(cornerbounds.size.x, cornerbounds.size.y), cornerbounds.size.z);
                cornerbounds.size = maxDim * Vector3.one;

                // we need to multiply by this amount to get to desired scale handle size
                var invScale = HandleSize / cornerbounds.size.x;
                cornerVisual.transform.localScale = new Vector3(invScale, invScale, invScale);

                BoundingBoxHandleUtils.ApplyMaterialToAllRenderers(cornerVisual, HandleMaterial);

                BoundingBoxHandleUtils.AddComponentsToAffordance(corner, new Bounds(cornerbounds.center * invScale, cornerbounds.size * invScale), 
                    RotationHandlePrefabCollider.Box, CursorContextInfo.CursorAction.Scale, ColliderPadding, parent, drawManipulationTether);
                handles.Add(corner.transform);       
            }
        }
    }
}
