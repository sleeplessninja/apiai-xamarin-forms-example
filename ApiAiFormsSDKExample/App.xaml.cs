using ApiAiFormsSDK.Interface;
using ApiAiFormsSDKExample.ViewModels;
using ApiAiFormsSDKExample.Views;
using ApiAiSDK;
using Xamarin.Forms;

namespace ApiAiFormsSDKExample
{
    public partial class App : Application
    {
        public static MainViewModel MainViewModel { get; set; }

        public App(IAIServiceFactory aiServiceFactory)
        {
            InitializeComponent();
            var config = new AIConfiguration("327bf2eb54904e508362f6fb528ce00a",SupportedLanguage.English);
            var aiService = aiServiceFactory.Get(config);

            MainViewModel = new MainViewModel(aiService);

            MainPage = new MainDemoPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
