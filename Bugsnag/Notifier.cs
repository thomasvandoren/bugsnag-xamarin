using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bugsnag.Data;
using Bugsnag.IO;
using Newtonsoft.Json;

namespace Bugsnag
{
    internal class Notifier
    {
        private static readonly Uri BaseUrl = new Uri ("https://notify.bugsnag.com/");
        private static readonly NotifierInfo notifierInfo = new NotifierInfo () {
            Name = "Toggl Xamarin/.NET Bugsnag Notifier",
            Version = "1.0",
            Url = "https://github.com/toggl/bugsnag-xamarin",
        };

        private readonly BugsnagClient client;
        private readonly string cacheDir;
        private string notifPrepend = null;
        private string notifAppend = null;

        public Notifier (BugsnagClient client, string cacheDir)
        {
            if (client == null)
                throw new ArgumentNullException ("client");
            if (cacheDir == null)
                throw new ArgumentNullException ("cacheDir");

            this.client = client;
            this.cacheDir = cacheDir;
        }

        public bool StoreOnly { get; set; }

        protected HttpClient MakeHttpClient ()
        {
            // Cannot share HttpClient instance between threads as it might (and will) cause InvalidOperationExceptions
            // occasionally.
            var httpClient = new HttpClient ();
            httpClient.DefaultRequestHeaders.Accept.Add (
                new MediaTypeWithQualityHeaderValue ("application/json"));

            return httpClient;
        }

        public void TrackUser (UserMetrics metrics)
        {
            // Do the serialization and sending on the thread pool
            ThreadPool.QueueUserWorkItem (async delegate {
                try {
                    using (var httpClient = MakeHttpClient ()) {
                        var data = JsonConvert.SerializeObject (metrics);

                        var req = new HttpRequestMessage () {
                            RequestUri = new Uri (BaseUrl, "metrics"),
                            Method = HttpMethod.Post,
                            Content = new StringContent (data, Encoding.UTF8, "application/json"),
                        };

                        await httpClient.SendAsync (req);
                    }
                } catch (Exception exc) {
                    Log (String.Format ("Failed to track user: {0}", exc));
                }
            });
        }

        public void Notify (Event e)
        {
            // Determine file where to persist the error:
            string path = null;
            if (cacheDir != null) {
                var file = String.Format ("{0}.json", DateTime.UtcNow.ToBinary ());
                path = Path.Combine (cacheDir, file);
            }

            Stream eventStream = null;
            Stream notifStream = null;
            try {
                // Serialize the event:
                eventStream = TryStoreEvent (e, path);
                if (StoreOnly) {
                    StoreOnly = false;
                    return;
                }

                // Combine into a valid payload:
                notifStream = MakeNotification (new Stream[] { eventStream });

                SendNotification (notifStream).ContinueWith ((t) => {
                    try {
                        if (t.Result) {
                            // On successful response delete the stored file:
                            try {
                                File.Delete (path);
                            } catch (Exception ex) {
                                Log (String.Format ("Failed to clean up stored event: {0}", ex));
                            }
                        }
                    } finally {
                        if (notifStream != null) {
                            // Also disposes of the eventStream
                            notifStream.Dispose ();
                        }
                    }
                });
            } catch (Exception ex) {
                // Something went wrong...
                Log (String.Format ("Failed to send notification: {0}", ex));

                if (notifStream != null) {
                    // Also disposes of the eventStream
                    notifStream.Dispose ();
                } else if (eventStream != null) {
                    eventStream.Dispose ();
                }
            }
        }

        private Stream MakeNotification (Stream[] jsonEventStreams)
        {
            if (notifPrepend == null || notifAppend == null) {
                var json = JsonConvert.SerializeObject (new Notification () {
                    ApiKey = client.ApiKey,
                    Notifier = notifierInfo,
                    Events = new List<Event> (0),
                });

                // Find empty events array:
                var idx = json.IndexOf ("[]");
                notifPrepend = json.Substring (0, idx + 1);
                notifAppend = json.Substring (idx + 1);
            }

            var stream = new CombiningStream ();
            stream.Add (notifPrepend);
            if (jsonEventStreams.Length > 1) {
                var eventsStream = new CombiningStream (", ");
                foreach (var eventStream in jsonEventStreams) {
                    eventsStream.Add (eventStream);
                }
                stream.Add (eventsStream);
            } else if (jsonEventStreams.Length == 1) {
                stream.Add (jsonEventStreams [0]);
            }
            stream.Add (notifAppend);

            return stream;
        }

        private Stream TryStoreEvent (Event e, string path)
        {
            var json = new MemoryStream (Encoding.UTF8.GetBytes (
                           JsonConvert.SerializeObject (e)));

            // Don't even try storing to disk when invalid path
            if (path == null)
                return json;

            FileStream output = null;
            try {
                output = new FileStream (path, FileMode.CreateNew);
                json.CopyTo (output);
                output.Flush ();

                output.Seek (0, SeekOrigin.Begin);
                json.Dispose ();
                return output;
            } catch (IOException ex) {
                Log (String.Format ("Failed to store error to disk: {0}", ex));

                // Failed to store to disk (full?), return json memory stream instead
                if (output != null) {
                    output.Dispose ();
                }
                json.Seek (0, SeekOrigin.Begin);
                return json;
            }
        }

        public async void Flush ()
        {
            if (cacheDir == null)
                return;

            var files = Directory.GetFiles (cacheDir);
            if (files.Length == 0)
                return;

            var streams = new List<Stream> (files.Length);
            foreach (var path in files) {
                try {
                    streams.Add (new FileStream (path, FileMode.Open));
                } catch (Exception ex) {
                    Log (String.Format ("Failed to open cached file {0}: {1}", Path.GetFileName (path), ex));
                }
            }

            Stream notifStream = null;
            try {
                // Make a single request to send all stored events
                notifStream = MakeNotification (streams.ToArray ());

                var success = await SendNotification (notifStream).ConfigureAwait (false);

                // Remove cached files on success
                if (success) {
                    foreach (var path in files) {
                        try {
                            File.Delete (path);
                        } catch (Exception ex) {
                            Log (String.Format ("Failed to clean up stored event {0}: {1}",
                                Path.GetFileName (path), ex));
                        }
                    }
                }
            } catch (Exception ex) {
                // Something went wrong...
                Log (String.Format ("Failed to send notification: {0}", ex));

                if (notifStream != null) {
                    // Notification stream closes all other streams:
                    notifStream.Dispose ();
                    notifStream = null;
                } else {
                    foreach (var stream in streams) {
                        stream.Dispose ();
                    }
                }
                streams.Clear ();
            } finally {
                if (notifStream != null) {
                    notifStream.Dispose ();
                    notifStream = null;
                }
            }
        }

        private async Task<bool> SendNotification (Stream stream)
        {
            var httpClient = MakeHttpClient ();
            var req = new HttpRequestMessage () {
                Method = HttpMethod.Post,
                RequestUri = BaseUrl,
                Content = new StreamContent (stream),
            };
            req.Content.Headers.ContentType = new MediaTypeHeaderValue ("application/json");

            try {
                var resp = await httpClient.SendAsync (req).ConfigureAwait (false);

                if (resp.StatusCode == HttpStatusCode.Unauthorized) {
                    Log ("Failed to send notification due to invalid API key.");
                } else if (resp.StatusCode == HttpStatusCode.BadRequest) {
                    Log ("Failed to send notification due to invalid payload.");
                } else {
                    return true;
                }
            } catch (Exception ex) {
                // Keep the stored file, it will be retried on next app start
                Log (String.Format ("Failed to send notification: {0}", ex));
            } finally {
                httpClient.Dispose ();
            }

            return false;
        }

        private static void Log (string msg)
        {
            #if __ANDROID__
            Android.Util.Log.Error (BugsnagClient.Tag, msg);
            #else
            Console.WriteLine(msg);
            #endif
        }
    }
}
