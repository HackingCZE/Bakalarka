using DH.Save.SerializableTypes;
using System;
using UnityEngine;

namespace DH.Save.Demo
{
    [System.Serializable]
    public class ObjectToGenerate : MonoBehaviour
    {
        public GeneratedObj GeneratedObj;

        public void Setup(GeneratedObj GeneratedObj)
        {
            this.GeneratedObj = GeneratedObj;
            GeneratedObj.transform.ApplyToTransform(transform);
            transform.localScale = new Vector3(transform.localScale.x * GeneratedObj.heightRadius, transform.localScale.x * GeneratedObj.heightRadius, transform.localScale.x * GeneratedObj.heightRadius);
            GetComponent<MeshRenderer>().material.color = GeneratedObj.color;
        }
    }
    [Serializable]
    public class GeneratedObj
    {
        public Color color;
        public SerializableTransform transform;
        public float heightRadius = 5f;

        public GeneratedObj(Color color, SerializableTransform transform, float heaightRadius)
        {
            this.color = color;
            this.transform = transform;
            this.heightRadius = heaightRadius;
        }
    }
}
