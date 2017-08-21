using System;
namespace AiApiSdk.Model
{
    public class SpeechOutput
    {

        public SpeechOutput(string transcription, float confidence)
        {
            Transcription = transcription;
            Confidence = confidence;
        }
        public string Transcription { get; private set; }
        public float Confidence { get; private set; }

    }
}
