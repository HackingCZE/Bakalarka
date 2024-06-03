using DH.Save.SerializableTypes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DH.Save.Demo
{
    public class ObjectGenerator : MonoBehaviour
    {
        [SerializeField] GameObject prefab;
        [SerializeField] List<GeneratedObj> objects = new List<GeneratedObj>();

        
        [ContextMenu("load")]
        public void LoadObjects()
        {
            DestroyObjects();
            ClearObjects();
            foreach (var item in SaveSystem.Load<List<GeneratedObj>>("generatedObjects0"))
            {
                var obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                item.transform.ApplyToTransform(obj.transform);
                obj.GetComponent<ObjectToGenerate>().Setup(new GeneratedObj(item.color, item.transform, item.heightRadius));
                objects.Add(obj.GetComponent<ObjectToGenerate>().GeneratedObj);

            }
        }

        [ContextMenu("destroy")]
        public void DestroyObjects()
        {
            foreach (var item in FindObjectsOfType<ObjectToGenerate>())
            {
                Destroy(item.gameObject);
            }
        }

        public void ClearObjects()
        {
            objects.Clear();
        }

        bool loaded = false;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                SceneManager.LoadScene(1);
            }
            if (SaveSystem.IsSlotLoaded && !loaded)
            {
                loaded = true;
                objects = new List<GeneratedObj>();
                if(SaveSystem.TryLoad<List<GeneratedObj>>(out List<GeneratedObj> savedObjects, "generatedObjects0"))
                {
                    if(savedObjects != null)objects = savedObjects;
                }
                
            }
            else if (Input.GetMouseButtonDown(0) && SaveSystem.IsSlotLoaded && loaded)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    Debug.Log(hit.point);
                    var obj = Instantiate(prefab, hit.point, Quaternion.identity);

                    obj.GetComponent<ObjectToGenerate>().Setup(new GeneratedObj((new Color(Random.Range(50, 255) / 255.0f, Random.Range(50, 255) / 255.0f, Random.Range(50, 255) / 255.0f, 255 / 255.0f)), new SerializableTransform(obj.transform), Random.Range(1, 3)));
                    try
                    {
                        objects.Add(obj.GetComponent<ObjectToGenerate>().GeneratedObj);
                        SaveSystem.UpdateOrSave<List<GeneratedObj>>("generatedObjects0", objects);
                    }
                    catch { }
                }
            }
        }


    }
}
