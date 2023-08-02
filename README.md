# VirtualHuman

VirtualHuman is a plugin that quickly uses your large language model and speech generation model in the Unity engine, which can be used to realize functions such as AI girlfriend and AI assistant.

It is important to note that this plugin does not provide any models.

## Demo

Bilibili: https://www.bilibili.com/video/BV1fX4y1E7ra/

Example using LLM&&VITS in Unity to create AI girlfriend.

<img src="Image/Sample.png">

Example of synchronizing lips with uLipSync (https://github.com/hecomi/uLipSync).

<img src="Image/Sample2.png">

## Dependency (Unity)
1. TextMeshPro
2. Newtonsoft.Json

## Environment configuration (non-Unity)
1. KoboldAI-KoboldCPP https://github.com/LostRuins/koboldcpp
(No need for this if you don't want to run the language model locally such as using ChatGPT or only using VITS)
2. VITS Simple API https://github.com/Artrajz/vits-simple-api

## How to use
1. Open the sample scene Sample.unity
2. Configure the address and port of VITSController
3. Select the type of LLM to use (if not used, the voice will be generated directly by VITS after the translation process)
4. Configure the corresponding LLM Controller
5. Select the translation process
6. Configure the language type of translation, <b>Language Code</b> please refer to (https://cloud.google.com/translate/docs/languages) to fill in

## LLM type description

### ChatGPT 3.5
- Please ensure the corresponding network environment when using ChatGPT
1. Fill in OpenAI's <b>APIKey</b> in GPTController
2. Fill in ``alwaysInclude`` with content that is always sent, such as "answer in Japanese"
3. Fill in character settings, background settings, etc. in ``m_Prompt``

### KoboldAI-KoboldCPP
- KoboldAI-KoboldCPP is not a model but a text generation software for loading models
- You can use any supported LLM model in KoboldCPP
1. Fill in the address and port in KoboldController
2. Fill in character settings in ``charaPreset``
3. Click ``Generate Memory`` to serialize the above settings to ``generatedMemory``
4. ``Smart Reading`` makes VITS skip Kobold-generated action and character expression descriptions (usually start and end with ``*``)

## Other instructions

### Google Translation
- Classify the language types of LLM, user input and output, VITS
- If it is judged that the current language is different from the specified language, Google Translate will be used, so please ensure the corresponding network environment

## Integration function to be added
- Text-Generation-WebUI
- ChatGPT 4.0