using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace YoutubeUrlExtractor
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("    YouTube 전체 영상(Shorts 포함) 누락 없는 추출기   ");
            Console.WriteLine("=================================================");
            Console.Write("www.youtube.com/ 뒤의 계정명을 입력하세요 (예: @mihosensei): ");

            string inputAccount = Console.ReadLine()?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(inputAccount))
            {
                Console.WriteLine("[오류] 입력값이 비어 있습니다.");
                return;
            }

            // 1. 계정명 정제 및 파일명 설정
            string cleanHandle = SanitizeAccountInput(inputAccount);
            if (!cleanHandle.StartsWith("@"))
            {
                cleanHandle = "@" + cleanHandle;
            }

            string safeHandleName = cleanHandle.TrimStart('@');
            string outputFileName = $"urls_@{safeHandleName}.txt";

            Console.WriteLine($"\n[정보] 대상 계정: {cleanHandle}");
            Console.WriteLine($"[정보] 저장 파일명: {outputFileName}");
            Console.WriteLine("[작업 중] 채널의 모든 영상을 스캔하고 있습니다. (시간이 다소 소요될 수 있습니다)...\n");

            HashSet<string> videoUrls = new HashSet<string>();

            // 2. yt-dlp 프로세스 실행하여 전체 영상/Shorts/라이브 목록 추출
            // --flat-playlist: 영상 본체를 받지 않고 목록만 고속 탐색
            string channelBaseUrl = $"https://www.youtube.com/{cleanHandle}";
            
            ExtractUrlsWithYtDlp(channelBaseUrl, videoUrls);

            // 3. 추출 결과를 txt 파일로 기록
            try
            {
                using var writer = new StreamWriter(outputFileName, false, System.Text.Encoding.UTF8);
                int count = 0;
                foreach (var url in videoUrls)
                {
                    writer.WriteLine(url);
                    count++;
                    Console.WriteLine($"[{count}] {url}");
                }

                Console.WriteLine("\n=================================================");
                if (count > 0)
                {
                    Console.WriteLine($"[성공] 총 {count}개의 누락 없는 동영상 URL을 '{outputFileName}'에 저장했습니다.");
                }
                else
                {
                    Console.WriteLine("[알림] 수집된 동영상이 없습니다. yt-dlp.exe 파일 위치를 확인해 주세요.");
                }
                Console.WriteLine("=================================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[오류] 파일 저장 중 에러 발생: {ex.Message}");
            }

            Console.WriteLine("\n아무 키나 누르면 프로그램을 종료합니다.");
            Console.ReadKey();
        }

        private static void ExtractUrlsWithYtDlp(string channelUrl, HashSet<string> videoUrls)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = $"--flat-playlist --print id \"{channelUrl}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8
            };

            try
            {
                using Process process = Process.Start(psi);
                if (process == null) return;

                while (!process.StandardOutput.EndOfStream)
                {
                    string id = process.StandardOutput.ReadLine()?.Trim();
                    if (!string.IsNullOrEmpty(id) && id.Length == 11)
                    {
                        // YouTube Download HD 인식 표준 포맷으로 추가
                        videoUrls.Add($"https://www.youtube.com/watch?v={id}");
                    }
                }

                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[오류] yt-dlp 실행 실패. 실행 파일(.exe) 위치에 yt-dlp.exe가 있는지 확인하세요.\n세부사항: {ex.Message}");
            }
        }

        private static string SanitizeAccountInput(string input)
        {
            string trimmed = input.TrimStart('/');
            string pattern = @"/(featured|videos|shorts|streams|community|playlists)/?$";
            string sanitized = Regex.Replace(trimmed, pattern, "", RegexOptions.IgnoreCase);

            return sanitized;
        }
    }
}