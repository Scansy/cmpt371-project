using Unity.VisualScripting;
using UnityEngine;

namespace MockTest
{
    public class TestPrint : MonoBehaviour
    {
        void Start()
        {
            for (int i = 0; i < 5; i++)
            {
                Debug.Log("sigma boi");
            }
        }
    }
}