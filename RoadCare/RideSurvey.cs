
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

namespace RoadCare
{
	[Activity (Label = "RideSurvey")]			
	public class RideSurvey : Activity
	{
		Button btnSubmit;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Create your application here
			SetContentView (Resource.Layout.Survey);
			btnSubmit=FindViewById<Button>(Resource.Id.submitBtn);
			btnSubmit.Click += SubmitBtnClicked;
		}

		void SubmitBtnClicked (object sender, EventArgs e)
		{
			Toast.MakeText (this,"Thanks for your feedback!!!" , ToastLength.Long).Show ();
			var intent = new Intent(this, typeof(PathholesActivity));
			StartActivity(intent);
		}
	}
}

