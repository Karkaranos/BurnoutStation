﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.InputSystem;
using PDollarGestureRecognizer;

public class Demo : MonoBehaviour
{

	public Transform gestureOnScreenPrefab;

	private List<Gesture> trainingSet = new List<Gesture>();

	private List<Point> points = new List<Point>();
	private int strokeId = -1;

	private Vector3 virtualKeyPosition = Vector2.zero;
	private Rect drawArea;

	private RuntimePlatform platform;
	private int vertexCount = 0;

	private List<LineRenderer> gestureLinesRenderer = new List<LineRenderer>();
	private LineRenderer currentGestureLineRenderer;

	//GUI
	private string message;
	private bool recognized;
	private string newGestureName = "";
	private bool leftClickDown;
	private bool leftInstance;

	private PlayerInput pi;
	private InputAction leftClick;



	void Start()
	{

		pi = GetComponent<PlayerInput>();
		leftClick = pi.currentActionMap.FindAction("Click");

		leftClick.started += LeftClick_started;
		leftClick.canceled += LeftClick_canceled;
		platform = Application.platform;
		drawArea = new Rect(0, 0, Screen.width - Screen.width / 3, Screen.height);

		//Load pre-made gestures
		TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("GestureSet/10-stylus-MEDIUM/");
		foreach (TextAsset gestureXml in gesturesXml)
			trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));

		//Load user custom gestures
		string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.xml");
		foreach (string filePath in filePaths)
			trainingSet.Add(GestureIO.ReadGestureFromFile(filePath));
	}

	private void LeftClick_canceled(InputAction.CallbackContext obj)
	{
		leftClickDown = false;
	}

	private void LeftClick_started(InputAction.CallbackContext obj)
	{
		leftClickDown = true;
		leftInstance = true;
		StartCoroutine(DisableKey());
	}

	private IEnumerator DisableKey()
	{
		yield return new WaitForEndOfFrame();
		leftInstance = false;
	}

	void Update()
	{
		Vector2 mousePos = pi.currentActionMap.FindAction("Position").ReadValue<Vector2>();
		if (platform == RuntimePlatform.Android || platform == RuntimePlatform.IPhonePlayer)
		{
			if (Input.touchCount > 0)
			{
				virtualKeyPosition = new Vector3(mousePos.x, mousePos.y);
			}
		}
		else
		{
			if (leftClickDown)
			{
				virtualKeyPosition = new Vector3(mousePos.x, mousePos.y);
			}
		}

		if (drawArea.Contains(virtualKeyPosition))
		{

			if (leftInstance)
			{

				if (recognized)
				{

					recognized = false;
					strokeId = -1;

					points.Clear();

					foreach (LineRenderer lineRenderer in gestureLinesRenderer)
					{

						lineRenderer.SetVertexCount(0);
						Destroy(lineRenderer.gameObject);
					}

					gestureLinesRenderer.Clear();
				}

				++strokeId;

				Transform tmpGesture = Instantiate(gestureOnScreenPrefab, transform.position, transform.rotation) as Transform;
				currentGestureLineRenderer = tmpGesture.GetComponent<LineRenderer>();

				gestureLinesRenderer.Add(currentGestureLineRenderer);

				vertexCount = 0;
			}

			if (leftClickDown)
			{
				points.Add(new Point(virtualKeyPosition.x, -virtualKeyPosition.y, strokeId));

				currentGestureLineRenderer.SetVertexCount(++vertexCount);
				currentGestureLineRenderer.SetPosition(vertexCount - 1, Camera.main.ScreenToWorldPoint(new Vector3(virtualKeyPosition.x, virtualKeyPosition.y, 10)));
			}
		}
	}

	void OnGUI()
	{

		GUI.Box(drawArea, "Draw Area");

		GUI.Label(new Rect(10, Screen.height - 40, 500, 50), message);

		if (GUI.Button(new Rect(Screen.width - 100, 10, 100, 30), "Recognize"))
		{

			recognized = true;

			Gesture candidate = new Gesture(points.ToArray());
			Result gestureResult = PointCloudRecognizer.Classify(candidate, trainingSet.ToArray());

			message = gestureResult.GestureClass + " " + gestureResult.Score;
		}

		GUI.Label(new Rect(Screen.width - 200, 150, 70, 30), "Add as: ");
		newGestureName = GUI.TextField(new Rect(Screen.width - 150, 150, 100, 30), newGestureName);

		if (GUI.Button(new Rect(Screen.width - 50, 150, 50, 30), "Add") && points.Count > 0 && newGestureName != "")
		{

			string fileName = String.Format("{0}/{1}-{2}.xml", Application.persistentDataPath, newGestureName, DateTime.Now.ToFileTime());

#if !UNITY_WEBPLAYER
			GestureIO.WriteGesture(points.ToArray(), newGestureName, fileName);
#endif

			trainingSet.Add(new Gesture(points.ToArray(), newGestureName));

			newGestureName = "";
		}
	}
}
