# 빌드 방법

1. 먼저 패키지를 설치합니다. `dotnet add package YoutubeExplode`

2. 빌드합니다. `dotnet build`

3. 단일 파일(Single File) 출력 게시 커맨드는 다음과 같습니다. `dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true`
  - 용량을 더 줄이고 싶으면 `dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true`

# 실행시 선행 조건

YoutubeUrlExtractor.exe 실행할 때 yt-dlp.exe 파일(https://github.com/yt-dlp/yt-dlp/releases)이 있어야 정상 작동합니다.
