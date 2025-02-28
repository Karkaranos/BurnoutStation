using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PDollarGestureRecognizer
{
	public class CensorshipManager : MonoBehaviour
	{
		[Range(0f,1f)]
		private float minimumScoreForRecognition = .85f;
		[HideInInspector]
		public static CensorshipManager instance { get; private set; }
        public List<LineRenderer> GestureLinesRenderer { get => gestureLinesRenderer; set => gestureLinesRenderer = value; }

        private List<Gesture> recognizedGestures = new List<Gesture>();


		private int strokeId = -1;

		private Vector3 virtualKeyPosition = Vector2.zero;
		private Rect drawArea;

		private RuntimePlatform platform;
		private int vertexCount = 0;

		private List<LineRenderer> gestureLinesRenderer = new List<LineRenderer>();
		private LineRenderer currentGestureLineRenderer;

		private List<char> currentLetters = new List<char>();

		//GUI
		private string message;
		private bool recognized;
		private string newGestureName = "";

		/// <summary>
		/// Ensures there is only one Censorship Manager in the scene at one time
		/// </summary>
		void Awake()
		{
			if (instance != null)
			{
				Destroy(instance.gameObject);
			}
			instance = this;

			//Load pre-made gestures
			TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("GestureSet/10-stylus-MEDIUM/");
			foreach (TextAsset gestureXml in gesturesXml)
				recognizedGestures.Add(GestureIO.ReadGestureFromXML(gestureXml.text));

			//Load user custom gestures
			string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.xml");
			foreach (string filePath in filePaths)
				recognizedGestures.Add(GestureIO.ReadGestureFromFile(filePath));
		}

		/// <summary>
		/// Detects if an object meets existing values
		/// CANNOT TEST
		/// </summary>
		public bool Detect(List<Point> points)
		{
			Debug.Log("Hello. Cade cannot call this function without nullrefs");
			recognized = true;
			Gesture candidate = new Gesture(points.ToArray());
			Result gestureResult = PointCloudRecognizer.Classify(candidate, recognizedGestures.ToArray());
			if(gestureResult.Score >= minimumScoreForRecognition)
            {
				Debug.Log("Recognized drawing as " + gestureResult.GestureClass + " with certainty of " + gestureResult.Score);
				
				//	Checks whether or not the recognized gesture is a letter
				if(gestureResult.GestureClass.Substring(0,1)==gestureResult.GestureClass)
                {
					//	Adds it to the letter recognition 
					currentLetters.Add(gestureResult.GestureClass.ToUpper().ToCharArray()[0]);
                }

				//	Check whether it is an allowed gesture
				Debug.Log("Add logic here later");
				return true;
			}
			else
            {
				Debug.Log("Not recognized");
				return false;
            }

		}
	}
}
