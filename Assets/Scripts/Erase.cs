using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace Leap.Unity.DetectionExamples
{
    public class Erase : MonoBehaviour
    {
        [Tooltip("Specifies which hand you wish to use to earse.")]
        [SerializeField]
        private RigidHand _eraseHands;

       // [Tooltip("Each pinch detector can erase one line at a time.")]
       // [SerializeField]
       // private PinchDetector[] _pinchDetectors;

       //// private EraseState[] _eraseStates;

       // void Awake()
       // {
       //     if (_pinchDetectors.Length == 0)
       //     {
       //         Debug.LogWarning("No pinch detectors were specified for eraser!");
       //     }
       // }

        // Use this for initialization
        void Start()
        {
           /* _eraseStates = new EraseState[_pinchDetectors.Length];
            for (int i = 0; i < _pinchDetectors.Length; i++)
            {
                _eraseStates[i] = new EraseState(this);
            }*/
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown("backspace"))  EraseReset();
            if (Input.GetKeyDown("delete"))     EraseUndo();
        }

        public void EraseUndo()
        {

            int count = this.gameObject.transform.childCount;
            Debug.Log("Child count = " + count);
            //Hand hand = GetLeapHand();
            //if (_eraseHands.Handedness == hand.Handedness) Debug.Log("..........");
            if (count > 0) Destroy(this.gameObject.transform.GetChild(count-1).gameObject);
        }

        public void EraseReset()
        {
            int count = this.gameObject.transform.childCount;
            Debug.Log("Child count = " + count);
            if (count > 0)
            {
                for(int i = 0; i < count; i++) Destroy(this.gameObject.transform.GetChild(i).gameObject);
            }
        }
        //public void OnCollisionEnter(Collision collision)
        //{
        //    Debug.Log("Collision Event " + collision.transform.name);
        //    IHandModel hand = collision.gameObject.GetComponent<IHandModel>();
        //    if(hand != null)
        //    {
        //        Destroy(this.gameObject);
        //    }
        //}
    }

}
