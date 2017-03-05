using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Hardware;
using Java.Lang;
using System.Net;
using Geolocator.Plugin;

namespace RoadCare
{
	[Activity (Label = "Pathholes detection" , Icon = "@drawable/appicon")]			
	public class PathholesActivity : Activity
	{
		private static readonly object _syncLock = new object();
		private SensorManager _sensorManager;
		private Sensor _sensor;
		private ShakeDetector _shakeDetector;
		TextView alertlbl;
		Button surveyBtn;
		Button settingBtn;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Create your application here
			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Pathholes);
			alertlbl = FindViewById<TextView>(Resource.Id.textView1);
			surveyBtn = FindViewById<Button> (Resource.Id.buttonRideSurvey);
			settingBtn = FindViewById<Button> (Resource.Id.btnSettings);
			var locator = CrossGeolocator.Current;
			//locator.DesiredAccuracy = 50;
			surveyBtn.Click += OpenSurveypage;
			settingBtn.Click += OpenSettingpage;
			_sensorManager = (SensorManager)GetSystemService(Context.SensorService);
			_sensor = _sensorManager.GetDefaultSensor(SensorType.Accelerometer);

			_shakeDetector = new ShakeDetector();

			_shakeDetector.Shaked += async (sender, shakeCount,xval,yval,zval) =>
			{
				var position = await locator.GetPositionAsync (timeoutMilliseconds: 5000);
				lock (_syncLock)
				{

					Toast.MakeText (this,"deviation recorded!" , ToastLength.Long).Show ();
					alertlbl.Text = shakeCount.ToString()+" x="+xval.ToString()+" y="+yval.ToString()+" z="+zval.ToString()+" GPS Lat:="+position.Latitude.ToString()+" GPS Log:="+position.Longitude.ToString();
					alertlbl.RefreshDrawableState();
					try{
						if(yval>10.0||yval<-10.0)
						{
							using (WebClient client = new WebClient())
							{
								string json = "{ \"query\": \"mutation { createIncident(latitude: "+position.Latitude.ToString()+", longitude: "+position.Longitude.ToString()+",type:\\\"deviation\\\", userid: 1){ id }}\" }";

								//string json = "{ \"query\": \"mutation { createIncident(latitude: 52.5057, longitude: 13.3935, userid: 1){ id }}\" }";
								client.Headers[HttpRequestHeader.ContentType] = "application/json";
								client.UploadString("https://api.graph.cool/simple/v1/cizvc5t3ufd7w0132e0v2ljvj/", json);
							}

						}
						else
						{
					using (WebClient client = new WebClient())
					{
								string json = "{ \"query\": \"mutation { createIncident(latitude: "+position.Latitude.ToString()+", longitude: "+position.Longitude.ToString()+",type:\\\"bump\\\", userid: 1){ id }}\" }";
								//string json = "{ \"query\": \"mutation { createIncident(latitude: 52.5057, longitude: 13.3935, userid: 1){ id }}\" }";
							//string json="{ \"query\": \"mutation { createIncident(latitude: 52.5057, longitude: 13.3935, userid: 1, type:\\\"bump\\\",level: 1){ id }}\"}";
						client.Headers[HttpRequestHeader.ContentType] = "application/json";
						client.UploadString("https://api.graph.cool/simple/v1/cizvc5t3ufd7w0132e0v2ljvj/", json);
					}
					}
					}
					catch (System.Exception exception) {
						Toast.MakeText (this,exception.Message , ToastLength.Long).Show ();
					}

				}
			};
		}

		//open pathholes window
		void OpenSurveypage (object sender, EventArgs e)
		{
			var intent = new Intent(this, typeof(RideSurvey));
			StartActivity(intent);
		}

		//open pathholes window
		void OpenSettingpage (object sender, EventArgs e)
		{
			var intent = new Intent(this, typeof(SettingsPage));
			StartActivity(intent);
		}
		protected override void OnResume()
		{
			base.OnResume();

			_sensorManager.RegisterListener(_shakeDetector, _sensor, SensorDelay.Ui);
		}

//		protected override void OnPause()
//		{
//			base.OnPause();
//
//			_sensorManager.UnregisterListener(_shakeDetector);
//		}

	}

	public class ShakeDetector : Java.Lang.Object, ISensorEventListener
	{
		private const float SHAKE_THRESHOLD_GRAVITY = 1.7F ;
		private const int SHAKE_SLOP_TIME_MS = 500;
		private const int SHAKE_COUNT_RESET_TIME_MS = 5000;

		private long mShakeTimestamp;
		private int mShakeCount;

		public delegate void OnshakeHandler(object sender, int shakeCount,Single x,Single y,Single z);
		public event OnshakeHandler Shaked;

		public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
		{

			// Do nothing in our case.
		}

		public void OnSensorChanged(SensorEvent e)
		{
			var x = e.Values[0];
			var y = e.Values[1];
			var z = e.Values[2];

			var gX = x / SensorManager.GravityEarth;
			var gY = y / SensorManager.GravityEarth;
			var gZ = z / SensorManager.GravityEarth;


			// gForce will be close to 1 when there is no movement.
			var gForce = Java.Lang.Math.Sqrt(gX * gX + gY * gY + gZ * gZ);

			if (!(gForce > SHAKE_THRESHOLD_GRAVITY))
				return;

			var now = JavaSystem.CurrentTimeMillis();


			// Ignore shake events too close to each other (500ms)
			if (mShakeTimestamp + SHAKE_SLOP_TIME_MS > now)
			{
				return;
			}


			// Reset the shake count after 3 seconds of no shakes
			if (mShakeTimestamp + SHAKE_COUNT_RESET_TIME_MS < now)
			{
				mShakeCount = 0;
			}

			mShakeTimestamp = now;
			mShakeCount++;

			if (this.Shaked != null)
			{
				this.Shaked(this, mShakeCount,x,y,z);
			}
		}
	}
}

