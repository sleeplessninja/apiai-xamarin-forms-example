# apiai-xamarin-forms-example

Xamarin Forms Sample Application that uses the [Api.Ai](https://github.com/api-ai) natural language processing service.  

This is based on two github projects: 
https://github.com/api-ai/api-ai-net and 
https://github.com/api-ai/apiai-xamarin-client

The big difference besides integrating the code from the above repositories into a forms project, is I modified the iOS part to use Speech to text and send the resulting text to the Api.AI services for a result.  This is due to the fact that the 'Automatic Speech Recognition' (VSR) or VoiceRequest() functionality in the SDK for iOS has been deprecated.

Here is the reference blog post: https://api.ai/support/change-log#april_26_2017


# Open Source Project Credits

JSON parsing implemented using [Json.NET](https://github.com/JamesNK/Newtonsoft.Json).
