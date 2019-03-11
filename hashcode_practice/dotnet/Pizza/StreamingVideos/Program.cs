using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;


// this one took me 3 hours on my own. Time limit was 4 hr. The tricky part was to understand that videos can be cached on an endpoint
// eventhough they have not been requested for that one ore any other for that matter - buzzgeek

namespace StreamingVideos
{
    class Program
    {
        public class Video
        {
            public int id;
            public int sizeInMB;

            public Video(int id, int sizeInMB)
            {
                this.id = id;
                this.sizeInMB = sizeInMB;
            }
        }

        public class Endpoint
        {
            public Dictionary<int, Cache> caches = new Dictionary<int, Cache>();
            public int latency;
            public int id;

            public Endpoint(int id, int latency)
            {
                this.id = id;
                this.latency = latency;
            }
        }

        public class Cache
        {
            public int id;
            public int latency;
            public Dictionary<int, Video> videos = new Dictionary<int, Video>();
            private int cacheSize;

            public Cache(int id, int latency, int cacheSize)
            {
                this.id = id;
                this.latency = latency;
                this.cacheSize = cacheSize;
            }

            public int FreeSpace { get { return cacheSize - videos.Values.Sum(s => s.sizeInMB); } }
        }

        public class Request
        {
            public int videoId;
            public int endpointId;
            public int numRequests;

            public Request(int videoId, int endpointId, int numRequests)
            {
                this.videoId = videoId;
                this.endpointId = endpointId;
                this.numRequests = numRequests;
            }
        }

        public static Dictionary<int, Request> requests = new Dictionary<int, Request>();
        public static Dictionary<int, Video> videos = new Dictionary<int, Video>();
        public static Dictionary<int, Endpoint> endpoints = new Dictionary<int, Endpoint>();
        public static Dictionary<int, Cache> caches = new Dictionary<int, Cache>();


        static void Main(string[] args)
        {
            string resource = "example";
            string output = ".\\example.out";

            Load(resource);
            ProcessRequestsByPopularity();
            //ProcessRequests();
            SaveResult(output);
            double finalScore = Score();
            Console.WriteLine(string.Format("final score: {0}", finalScore));
        }

        public static void SaveResult(string urlResult)
        {
            try
            {
                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(urlResult))
                {
                    var used = from c in caches.Values where c.videos.Values.Count > 0 select c;
                    file.WriteLine(string.Format("{0}", used.Count<Cache>()));

                    foreach (Cache cache in used)
                    {
                        var vids = from id in cache.videos.Keys orderby id select id;

                        file.Write(string.Format("{0} ", cache.id));
                        string record = string.Join(' ', vids.ToArray<int>());
                        file.WriteLine(record);
                    }
                    file.WriteLine(string.Format("score: {0}", Score()));
                }
                
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }

        }

        public static void ProcessRequestsByPopularity()
        {
            var popularRequests = (from req in requests.Values orderby req.numRequests select req).Reverse<Request>();

            for(int i = 0; i < caches.Values.Count; i++)
            { 
                foreach (Request pop in popularRequests)
                {
                    if (pop.numRequests == 0)
                            continue;
                    foreach(Endpoint endpoint in endpoints.Values)
                    {
                        var sortedCaches = from c in endpoint.caches.Values orderby c.latency select c;
                        foreach (Cache cache in sortedCaches)
                        {
                            if (videos[pop.videoId].sizeInMB <= cache.FreeSpace)
                            {
                                cache.videos[pop.videoId] = videos[pop.videoId];
                                break;
                            }
                        }
                    }
                }
            }

            // now see if we still have some available cache to fill
            var availCache = (from c in caches.Values where c.FreeSpace > 0 select c).Reverse<Cache>();
            foreach (Cache c in availCache)
            {
                var smallVids = (from v in videos.Values where v.sizeInMB <= c.FreeSpace orderby v.sizeInMB select v).Reverse<Video>();
                foreach(Video vid in smallVids)
                {
                    if(vid.sizeInMB <= c.FreeSpace)
                    { 
                        c.videos[vid.id] = vid;
                    }
                    if (c.FreeSpace == 0)
                        break;
                }

            }

        }

        public static void ProcessRequests()
        {
            try
            {
                var query = requests.Values.GroupBy(e => e.endpointId);
                foreach (IGrouping<int, Request> grp in query)
                {
                    var sorted = (from req in grp orderby req.numRequests select req).Reverse<Request>();

                    foreach (Request req in sorted)
                    {
                        if (endpoints[req.endpointId].caches.Count == 0 ||
                            req.numRequests == 0)
                        {
                            // no cache servers available
                            // but maybe the vid should be cached anyway on a different endpoint since it is so popular
                            continue;
                        }

                        var sortedCaches = from c in endpoints[req.endpointId].caches.Values orderby c.latency select c;
                        foreach (Cache cache in sortedCaches)
                        {
                            if (videos[req.videoId].sizeInMB <= cache.FreeSpace)
                            {
                                cache.videos[req.videoId] = videos[req.videoId];
                                break;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }


        public static void Load(string resource)
        {
            try
            {
                var example = Properties.Resources.ResourceManager.GetObject(resource) as String;
                if(example == null)
                {
                    throw new NotSupportedException(string.Format("resource {0} is not included in assembly", resource));
                }

                int index = 0;
                string[] lines = example.Split('\n');

                // parse header containing number of entities
                string[] desc = lines[index++].Split(' ');

                int V = int.Parse(desc[0]); // number of videos
                int E = int.Parse(desc[1]); // number of endpoints
                int R = int.Parse(desc[2]); // number of request descriptions
                int C = int.Parse(desc[3]); // number of cache servers
                int X = int.Parse(desc[4]); // the capacity of each server in megabytes

                // parse videos
                string[] videoSizes = lines[index++].Split(' ');
                for (int i = 0; i < V; i++)
                {
                    videos[i] = new Video(i, int.Parse(videoSizes[i]));
                    requests[i] = new Request(i, 0, 0); // create a default request entry for each video with 0 requests
                }

                // parse endpoints
                for (int idxE = 0; idxE < E; idxE++)
                {
                    string[] epDesc = lines[index++].Split(' ');
                    int latency = int.Parse(epDesc[0]);
                    int numCaches = int.Parse(epDesc[1]);
                    endpoints[idxE] = new Endpoint(idxE, latency);
                    // parse endpoint's caches
                    for(int idxC=0; idxC < numCaches; idxC++)
                    {
                        string[] cDesc = lines[index++].Split(' ');
                        int cacheId = int.Parse(cDesc[0]);
                        int cacheLatency = int.Parse(cDesc[1]);
                        endpoints[idxE].caches[cacheId] = new Cache(cacheId, cacheLatency, X);
                        caches[cacheId] = endpoints[idxE].caches[cacheId];
                    }
                }
                // parse requests
                for (int idxRequest = 0; idxRequest < R; idxRequest++)
                {
                    string [] reqDesc =  lines[index++].Split(' ');
                    int videoId = int.Parse(reqDesc[0]);
                    int endpointId = int.Parse(reqDesc[1]);
                    int numRequests = int.Parse(reqDesc[2]);
                    requests[videoId] = new Request(videoId, endpointId, numRequests);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        public static double Score()
        {
            double res = 0d;
            double numerator = 0d;

            double denominator = requests.Values.Sum(s => s.numRequests);

            foreach(Endpoint e in endpoints.Values)
            {
                foreach (Cache c in e.caches.Values)
                {
                    foreach(Video v in c.videos.Values)
                    {
                        if (requests[v.id].endpointId == e.id)
                        {
                            numerator += (e.latency - c.latency) * requests[v.id].numRequests;
                        }
                    }
                }
            }

            res = numerator * 1000 / denominator;

            return res;
        }

    }
}
