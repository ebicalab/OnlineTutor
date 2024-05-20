FROM gableroux/unity3d:2020.3.23f1

WORKDIR /app

COPY . /app

RUN /opt/Unity/Editor/Unity -batchmode -quit -buildTarget StandaloneLinux64 -projectPath /app -executeMethod BuildScript.BuildLinux -logFile /dev/stdout

CMD ["./Build/Linux/YourUnityApp.x86_64"]
