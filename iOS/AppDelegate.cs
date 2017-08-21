using System;
using System.Collections.Generic;
using System.Linq;
using ApiAiFormsSDK.iOS;
using AVFoundation;
using Foundation;
using UIKit;

namespace ApiAiFormsSDKExample.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();

            AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.PlayAndRecord);

            LoadApplication(new App(new AIServiceFactory()));
            return base.FinishedLaunching(app, options);
        }
    }
}
