using UnityEngine;
using System.Collections;
using Windows.Kinect;
using System.Collections.Generic;

public class trackedBody {
	
	public int index;
	public bool isTracking;
	public ulong trackingId ;
	public int kinect2ArrayIndex;
	
	public trackedBody(int index, bool isTracking, ulong trackingId) {
		this.index = index;
		this.isTracking = isTracking;
		this.trackingId = trackingId;
		this.kinect2ArrayIndex = -1;
	}
}

public class RUISKinect2DepthView : MonoBehaviour 
{
	public GameObject SourceManager; 
	private Texture2D texture;
	private Kinect2SourceManager _SourceManager;
	
	private Body[] _BodyData = null;
	private ushort[] _DepthData = null;
	private byte[] _BodyIndexData = null;
	private KinectSensor _Sensor = null;
	private static int _DownsampleSize = 4;
	private int imageWidth, imageHeight;
	
	private trackedBody[] trackingIDs = null;
	
	private Dictionary<ulong, int> trackingIDtoIndex = new Dictionary<ulong, int>();
	
	void Start () 
	{
		trackingIDs = new trackedBody[6]; 
		for(int y = 0; y < trackingIDs.Length; y++) {
			trackingIDs[y] = new trackedBody(-1, false, 1);
		}
		_SourceManager = SourceManager.GetComponent<Kinect2SourceManager>();
		_Sensor = _SourceManager.GetSensor();
		if(_Sensor != null) {
			imageWidth = _Sensor.DepthFrameSource.FrameDescription.Width;
			imageHeight = _Sensor.DepthFrameSource.FrameDescription.Height;
			texture = new Texture2D(imageWidth / _DownsampleSize, imageHeight / _DownsampleSize);
		}
		
	}
	
	void Update()
	{
		
		_BodyData = _SourceManager.GetBodyData();
		
		if(_BodyData != null) {
			// Update tracking ID array
			for(int y = 0; y < trackingIDs.Length; y++) {
				trackingIDs[y].isTracking = false; 
				trackingIDs[y].index = -1;
			}
			
			// Check tracking status and assing old indexes
			var arrayIndex = 0;
			foreach(var body in _BodyData) {
				
				if(body.IsTracked) {	
					for(int y = 0; y < trackingIDs.Length; y++) {
						if(trackingIDs[y].trackingId == body.TrackingId) { // Body found in tracking IDs array
							trackingIDs[y].isTracking = true;			   // Reset as tracked
							trackingIDs[y].kinect2ArrayIndex = arrayIndex; // Set current kinect2 array index
						
							if(trackingIDtoIndex.ContainsKey(body.TrackingId)) { // If key added to trackingIDtoIndex array earlier...
								trackingIDs[y].index = trackingIDtoIndex[body.TrackingId]; // Set old index
							}
						}
					}
					
				}
				
				
				arrayIndex++;
			}
			
			// Add new bodies
			arrayIndex = 0;
			foreach(var body in _BodyData) {
				if(body.IsTracked) {
					if(!trackingIDtoIndex.ContainsKey(body.TrackingId)) { // A new body
						for(int y = 0; y < trackingIDs.Length; y++) {
							if(!trackingIDs[y].isTracking) {			// Find an array slot that does not have a tracked body
								trackingIDs[y].index = y;				// Set index to trackingIDs array index
								trackingIDs[y].trackingId = body.TrackingId;	
								trackingIDtoIndex[body.TrackingId] = y;		// Add tracking id to trackingIDtoIndex array
								trackingIDs[y].kinect2ArrayIndex = arrayIndex;
								trackingIDs[y].isTracking = true;
								break;
							}
						}	
					}
				}	
				arrayIndex++;	
			}
		}
		
		_DepthData = _SourceManager.GetDepthData();
		_BodyData = _SourceManager.GetBodyData();
		_BodyIndexData = _SourceManager.GetBodyIndexData();
		
		if(_DepthData != null && _BodyIndexData != null &&  _Sensor != null) {
			for (int y = 0; y < imageHeight; y += _DownsampleSize)
			{
				for (int x = 0; x < imageWidth; x += _DownsampleSize) {
					
					double depth = GetAvg(_DepthData, x, y,imageWidth, imageHeight);
					depth = 1 - (depth / 4500);
					
					if(_BodyIndexData[y * imageWidth + x] != 0xff ) {
						int kinect2ArrayIndex = -2;
						if(_BodyIndexData[y * imageWidth + x] == 0x0) kinect2ArrayIndex = 0;
						if(_BodyIndexData[y * imageWidth + x] == 0x1) kinect2ArrayIndex = 1;
						if(_BodyIndexData[y * imageWidth + x] == 0x2) kinect2ArrayIndex = 2;
						if(_BodyIndexData[y * imageWidth + x] == 0x3) kinect2ArrayIndex = 3;
						if(_BodyIndexData[y * imageWidth + x] == 0x4) kinect2ArrayIndex = 4;
						if(_BodyIndexData[y * imageWidth + x] == 0x5) kinect2ArrayIndex = 5;
						
						bool found = false;
						for(int a = 0; a < trackingIDs.Length; a++) {
							if(trackingIDs[a].kinect2ArrayIndex == kinect2ArrayIndex && trackingIDs[a].isTracking) {
								if(trackingIDs[a].index == 0) texture.SetPixel(x / _DownsampleSize, y / _DownsampleSize, new Color(0.0f, (float)depth, 0.0f, 1));	// Green
								else texture.SetPixel(x / _DownsampleSize, y / _DownsampleSize, new Color(0.0f, 0.0f, (float)depth, 1)); // Blue
								found = true;
							}
						}
						if(!found) {
							texture.SetPixel(x / _DownsampleSize, y / _DownsampleSize, Color.yellow); 
						}
					}
					else {
						texture.SetPixel(x / _DownsampleSize, y / _DownsampleSize, new Color((float)depth, 0.0f, 0.0f, 1));	// Red
					}
					
				}
			}
			texture.Apply();
		} 
	}
	
	void OnGUI()
	{	
		GUI.DrawTexture(new Rect(0, Screen.height/2, Screen.width/2, -Screen.height/2), texture, ScaleMode.StretchToFill, false);
	}
	
	private double GetAvg(ushort[] depthData, int x, int y, int width, int height)
	{
		double sum = 0.0;
		
		for (int y1 = y; y1 < y + 4; y1++)
		{
			for (int x1 = x; x1 < x + 4; x1++)
			{
				int fullIndex = (y1 * width) + x1;
				
				if (depthData[fullIndex] == 0)
					sum += 4500;
				else
					sum += depthData[fullIndex];
				
			}
		}
		
		return sum / 16;
	}
	
}
