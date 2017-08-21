using System;
using ApiAiSDK;

namespace ApiAiFormsSDK.Interface
{
    public interface IAIServiceFactory
    {
        AIService Get(AIConfiguration config);
    }
}
