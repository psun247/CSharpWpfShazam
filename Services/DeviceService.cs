using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using CSharpWpfShazam.Services.ShazamMagic;
using CSharpWpfShazam.Models;

namespace CSharpWpfShazam.Services
{
    public class DeviceService
    {
        private static readonly MMDeviceEnumerator _DeviceEnumerator = new();
        private static readonly string _DeviceIdTag = Guid.NewGuid().ToString();
        private static readonly string _JsonToken = Guid.NewGuid().ToString(); // Used to be dynamic, but the same one works

        private HttpClient _httpClient;

        public DeviceService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<DeviceInfo> GetDeviceList()
        {
            return _DeviceEnumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active).Select(device =>
                    new DeviceInfo
                    {
                        DeviceID = device.ID,
                        DeviceName = (device.DataFlow switch { DataFlow.Capture => "IN – ", DataFlow.Render => "OUT – ", _ => "UNK – " }) + device.FriendlyName,
                        FriendlyName = device.FriendlyName
                    }).ToList();
        }

        public async Task<Tuple<VideoInfo?, string>> Listen(DeviceSetting selectedDeviceSetting, CancellationTokenSource cancelTokenSource)
        {
            Tuple<VideoInfo?, string> result;

            try
            {
                VideoInfo? videoInfo = await Task.Run(() => IdentifyAsync(selectedDeviceSetting.DeviceID, cancelTokenSource.Token));
                result = Tuple.Create(videoInfo, string.Empty);
            }
            catch (OperationCanceledException)
            {
                // ex.CancellationToken.IsCancellationRequested (always false) and cancelTokenSource.IsCancellationRequested (aways true)
                // so let the caller figure out "Canceled" vs "Timed out"
                result = Tuple.Create(null as VideoInfo, string.Empty);
            }
            catch (Exception ex)
            {
                result = Tuple.Create(null as VideoInfo, $"[Listen] {ex.Message}");
            }

            return result;
        }

        public async Task<VideoInfo?> IdentifyAsync(string deviceId, CancellationToken cancelToken)
        {
            MMDevice device = _DeviceEnumerator.GetDevice(deviceId);
            if (device == null || device.State != DeviceState.Active)
            {
                throw new ArgumentException("Selected device not available");
            }

            WasapiCapture capture = device.DataFlow switch
            {
                DataFlow.Capture => new WasapiCapture(device), // Microphone
                DataFlow.Render => new WasapiLoopbackCapture(device), // Speaker
                _ => throw new NotImplementedException(),
            };

            var bufferedWaveProvider = new BufferedWaveProvider(capture.WaveFormat)
            {
                ReadFully = false,
                DiscardOnBufferOverflow = true
            };
            using var resampler = new MediaFoundationResampler(bufferedWaveProvider, new WaveFormat(16000, 16, 1));
            ISampleProvider sampleProvider = resampler.ToSampleProvider();

            capture.DataAvailable += (_, e) => { bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded); };
            capture.StartRecording();

            var analyser = new Analyser();
            var landmarker = new Landmarker(analyser);
            int retryMs = 3000;

            // Loop to process result data
            while (true)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    capture.StopRecording();
                    throw new OperationCanceledException();
                }

                if (bufferedWaveProvider.BufferedDuration.TotalSeconds < 1)
                {
                    Thread.Sleep(100);
                    continue;
                }

                analyser.ReadChunk(sampleProvider);
                if (analyser.StripeCount > 2 * Landmarker.RADIUS_TIME)
                {
                    landmarker.Find(analyser.StripeCount - Landmarker.RADIUS_TIME - 1);
                }
                if (analyser.ProcessedMs < retryMs)
                {
                    continue;
                }

                var request = new ShazamRequest
                {
                    Signature = new ShazamSignature
                    {
                        Uri = "data:audio/vnd.shazam.sig;base64," + Convert.ToBase64String(Signature.Create(Analyser.SAMPLE_RATE, analyser.ProcessedSamples, landmarker)),
                        SampleMs = analyser.ProcessedMs
                    }
                };

                using var res = await _httpClient.PostAsync($"https://amp.shazam.com/discovery/v5/en/US/android/-/tag/{_DeviceIdTag}/{_JsonToken}",
                                        new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"), cancelToken);
                string jsonResponse = await res.Content.ReadAsStringAsync(cancelToken);
                ShazamResponse? response = JsonSerializer.Deserialize<ShazamResponse>(jsonResponse);
                if (response?.RetryMs > 0)
                {
                    retryMs = (int)response.RetryMs;
                    continue;
                }

                capture.StopRecording();
                if (response == null || response.Track == null)
                {
                    return null;
                }

                return new VideoInfo
                {
                    Song = response.Track.Title,
                    Artist = response.Track.Subtitle,
                    Link = response.Track.Share?.Link,
                    CoverUrl = response.Track.Images?.CoverHQ ?? response.Track.Images?.Cover ?? response.Track.Share?.Image,
                    Released = response.Track.SectionList?
                                                .FirstOrDefault(x => x.SectionType == "SONG")?
                                                .MetadataList?.FirstOrDefault(x => x.Title == "Released")?
                                                .Text
                };
            }
        }
    }
}
