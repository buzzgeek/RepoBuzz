using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

/// <summary>
/// buzzgeek aka dad was here :P - barrystruck@googlemail.com
/// 
/// - caches do not seem to help that much and take a very long time to generate for the larger problems.
/// - Although saving the vertical slide sets was a really good idea, to save some time for the e_shiny_selfies problem.
/// - the pet problem seems to have a score of 2 for almost 'all' tile pairs?
/// - For slides with vertical photos, it payed of to check if the tiles hat no intersecting tags.
/// - How the hell, did some teams get 1200000 in under 4 hrs? That is prety incredible and I am in awe. 
/// - If some one is actually checking this code and reading this, Thank you so much for this opportunity :)
/// - So far, the SortEx algorithm always delivered the best results, for me.
/// - For my final attempt, I will try implementing a tree(s) solving approach, if I can still make it in time
/// ...Nah I did not do this in the end:)
///  
/// - And most importantly, Thank You Louis and Dustin, for being part of this incredible experience.
/// 
/// - Oh yeah, what did I learn? I am prety 'competive' and I did not know that. I had the feeling that I was not
/// a really good team player. I feel more comfortable doing things on my own...I need to improve on this, I know
/// that...and I was thinking higher of myself before the competition, but it feels good to be slightly better than average :)
/// ...and I do not seem need alot of sleep...It's like having a kid again, except the kid is me ;)
/// ...and I got to use Linq for the first time eva, it is actually kinda cool and powerful :)
/// 
/// - ...and Sorry for the mess ;)
/// </summary>

namespace OnlineChallenge
{
    class Program
    {
        public class Photo
        {
            public int id;
            public bool horizontal;
            public int numTags;
            public List<string> tags = new List<string>();
            public int weight = -1;

            public Photo(int id, bool horizontal, int numTags)
            {
                this.id = id;
                this.horizontal = horizontal;
                this.numTags = numTags;
            }

            public int Weight(SlideShow slideShow)
            {
                if (weight < 0)
                {
                    weight = tags.Count;
                    foreach (String tag in tags)
                    {
                        foreach (Photo p in slideShow.tagsToPhotos[tag].Values)
                        {
                            weight += p.tags.Count;
                        }
                    }
                }

                return weight;
            }

        }

        public class SlideShow
        {
            public Dictionary<int, Slide> slides = new Dictionary<int, Slide>();
            public List<Slide> sortedSlides = new List<Slide>();
            public Dictionary<string, Dictionary<int, Slide>> tagsToSlides = new Dictionary<string, Dictionary<int, Slide>>();
            public Dictionary<string, Dictionary<int, Photo>> tagsToPhotos = new Dictionary<string, Dictionary<int, Photo>>();

            public int index = 0;

            public void GenerateTagsToPhotos(List<Photo> ps)
            {
                foreach (Photo p in ps)
                {
                    foreach (string tag in p.tags)
                    {
                        if (!tagsToPhotos.ContainsKey(tag))
                            tagsToPhotos[tag] = new Dictionary<int, Photo>();
                        tagsToPhotos[tag][p.id] = p;
                    }
                }

                foreach (Photo p in ps)
                {
                    int weight = p.Weight(this);
                    Console.Write(string.Format("Calculating weights - pid: {0} - weight: {1}\t\r", p.id, weight));
                }
            }

            public List<Slide> GetRelevantSlides(Slide slide)
            {
                List<Slide> res = new List<Slide>();

                foreach(string tag in slide.tags.Keys)
                {
                    if(tagsToSlides.ContainsKey(tag))
                    {
                        foreach(Slide s in tagsToSlides[tag].Values)
                        {
                            if (slide.id != s.id)
                            {
                                res.Add(s);
                            }
                        }
                    }
                }

                return res;
            }

            public Slide AddPhoto(Photo photo)
            {
                try
                {
                    if (!slides.ContainsKey(index))
                        slides[index] = new Slide(index, photo.horizontal);
                    Slide slide = slides[index];
                    slides[index++].AddPhoto(photo);

                    foreach (string tag in slide.tags.Keys)
                    {
                        if (!tagsToSlides.ContainsKey(tag))
                            tagsToSlides[tag] = new Dictionary<int, Slide>();
                        tagsToSlides[tag][slide.id] = slide;
                    }
                    return slide;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                return null;
            }

            public Slide AddPhotos(Photo p1, Photo p2)
            {
                try
                {
                    if (!slides.ContainsKey(index))
                        slides[index] = new Slide(index, p1.horizontal);
                    Slide slide = slides[index];
                    slides[index].AddPhoto(p1);
                    slides[index++].AddPhoto(p2);

                    foreach (string tag in p1.tags)
                    {
                        if (!tagsToSlides.ContainsKey(tag))
                            tagsToSlides[tag] = new Dictionary<int, Slide>();
                        tagsToSlides[tag][slide.id] = slide;
                    }
                    foreach (string tag in p2.tags)
                    {
                        if (!tagsToSlides.ContainsKey(tag))
                            tagsToSlides[tag] = new Dictionary<int, Slide>();
                        tagsToSlides[tag][slide.id] = slide;
                    }
                    return slide;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                return null;
            }

            private void ProcessSlide(ref List<Slide> slides, Slide slide)
            {
                try
                {
                    sortedSlides.Add(slide);
                    slides.Remove(slide);

                    Dictionary<string, int> toDelete = new Dictionary<string, int>();

                    foreach (string k in slide.tags.Keys)
                    {
                        if(tagsToSlides.ContainsKey(k))
                        { 
                            foreach (Slide s in tagsToSlides[k].Values)
                            {
                                toDelete[k] = s.id;
                            }
                        }
                    }
                    foreach(string tag in toDelete.Keys)
                    {
                        tagsToSlides[tag].Remove(toDelete[tag]);
                        if (tagsToSlides[tag].Count == 0)
                            tagsToSlides.Remove(tag);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }

            }

            public void SortByScores()
            {
                try
                {
                    int cnt = 0;
                    var tileSet = scores.GroupBy(k => k.Key.Split(' ')[0]).Select(group => new { key = group.Key, count = group.Count() }).ToList();
                    tileSet.AddRange(scores.GroupBy(k => k.Key.Split(' ')[1]).Select(group => new { key = group.Key, count = group.Count() }).ToList());
                    tileSet = tileSet.OrderBy(x => x.count).ToList();
                    var orderedScores = scores.Select(tuple => new { key = tuple.Key.Split(' '), score = tuple.Value }).ToList();

                    var item = tileSet.First();
                    tileSet.Remove(item);
                    Slide slide = null;

                    while (tileSet.Count > 0)
                    {
                        var fittings = orderedScores.Where(s => s.key.Contains(item.key))
                            .Select(s => new { root = int.Parse(item.key), key = s.key, score = s.score })
                            .ToList();
                        
                        if(fittings.Count == 0)
                        {
                            item = tileSet.First();
                            tileSet.Remove(item);
                            slide = slides[int.Parse(item.key)];
                            slideShow.sortedSlides.Add(slide);
                            tileSet.RemoveAll(x => x.key == slide.id.ToString());
                            orderedScores.RemoveAll(x => x.key.Contains(slide.id.ToString()));
                            cnt++;
                            Console.Write(string.Format("Processed slides: {0} of {1}\t\r", cnt, slides.Count));
                            continue;
                        }

                        var fitting = fittings.First();

                        int i0 = int.Parse(fitting.key[0]);
                        int i1 = int.Parse(fitting.key[1]);
                        slide = slides[i0 != fitting.root ? i0 : i1];
                        slideShow.sortedSlides.Add(slide);
                        tileSet.RemoveAll(x => x.key == slide.id.ToString());
                        orderedScores.RemoveAll(x => x.key.Contains(slide.id.ToString()));
                        cnt++;
                        slide = slides[fitting.root];
                        slideShow.sortedSlides.Add(slide);
                        tileSet.RemoveAll(x => x.key == slide.id.ToString());
                        orderedScores.RemoveAll(x => x.key.Contains(slide.id.ToString()));
                        cnt++;

                        if (tileSet.Count > 0)
                        {
                            item = tileSet.First();
                            tileSet.Remove(item);
                            slide = slides[int.Parse(item.key)];
                            slideShow.sortedSlides.Add(slide);
                            tileSet.RemoveAll(x => x.key == slide.id.ToString());
                            orderedScores.RemoveAll(x => x.key.Contains(slide.id.ToString()));
                            cnt++;
                        }
                        Console.Write(string.Format("Processed slides: {0} of {1}\t\r", cnt, slides.Count));
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }

            public void SortEx()
            {
                try
                {
                    var tmp = this.slides.Values.ToList();

                    int cnt = 0;
                    int max = 0;
                    int score = 0;
                    int totScore = 0;

                    Slide slide = tmp.OrderByDescending(x => x.tagCount).First();
                    ProcessSlide(ref tmp, slide);
                    cnt++;

                    while (tmp.Count > 0)
                    {
                        max = 0;
                        var targets = tmp.OrderByDescending(x => x.Score(slide)).ThenBy(x => x.Weight(slideShow)).ToList();

                        if (slide.Score(targets.First()) == 0)
                        {
                            Slide t = slide;
                            slide = targets.First();
                            totScore += t.Score(t);
                            ProcessSlide(ref tmp, slide);
                            cnt++;
                        }
                        else
                        { 
                            foreach (Slide s in targets)
                            {
                                score = slide.Score(s);
                                if (score == 0 || score < max)
                                {
                                    break;
                                }

                                slide = s;
                                max = score;
                                totScore += score;
                                ProcessSlide(ref tmp, s);
                                cnt++;
                            }
                        }
                        Console.Write(string.Format("Processed {0} slides - max score: {1} - totScore: {2}\t\r", cnt, max, totScore));
                    }
                    Console.Write(string.Format("Processed {0} slides - max score: {1} - totScore: {2}\t\r", cnt, max, totScore));
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }

            public void Sort()
            {
                try
                {
                    var tmp = this.slides.Values.ToList();
                    int cnt = 0;
                    int max = 0;
                    int totScore = 0;

                    Slide slide = tmp.OrderByDescending(x => x.tagCount).Take(1).First();
                    ProcessSlide(ref tmp, slide);
                    cnt++;

                    while (tmp.Count > 0)
                    {
                        max = 0;
                        var targetKeys = slide.tags.Keys.Intersect(tagsToSlides.Keys).ToList();

                        if (targetKeys.Count > 0)
                        {
                            List<Slide> targets = new List<Slide>();
                            foreach (string k in targetKeys)
                            {
                                foreach (Slide s in tagsToSlides[k].Values)
                                {
                                    targets.Add(s);
                                }
                            }

                            targets = tmp.OrderByDescending(x => x.Score(slide)).ThenByDescending(x => x.tags.Count).ToList();
                            foreach (Slide s in targets)
                            {
                                int score = slide.Score(s);

                                if (score == 0 || score < max)
                                {
                                    break;
                                }

                                max = score;
                                totScore += score;
                                slide = s;
                                ProcessSlide(ref tmp, s);
                                cnt++;
                            }
                        }

                        if(tmp.Count > 0)
                        {
                            Slide t = slide;
                            slide = tmp.OrderByDescending(x => x.tagCount).Take(1).First();
                            totScore += t.Score(slide);
                            ProcessSlide(ref tmp, slide);
                            cnt++;
                        }

                        Console.Write(string.Format("Processed {0} slides - max score: {1} - totScore: {2}\t\r", cnt, max, totScore));
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }

            public void CreateSlide(int id, int pid)
            {
                try
                {
                    if (!slides.ContainsKey(id))
                        slides[id] = new Slide(id, true);
                    Slide slide = slides[id];
                    slides[id].AddPhoto(photos[pid]);

                    foreach (string tag in slide.tags.Keys)
                    {
                        if (!tagsToSlides.ContainsKey(tag))
                            tagsToSlides[tag] = new Dictionary<int, Slide>();
                        tagsToSlides[tag][slide.id] = slide;
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }

            public void CreateSlide(int id, int pid1, int pid2)
            {
                try
                {
                    if (!slides.ContainsKey(id))
                        slides[id] = new Slide(id, false);
                    Slide slide = slides[id];
                    slides[id].AddPhoto(photos[pid1]);
                    slides[id].AddPhoto(photos[pid2]);

                    foreach (string tag in slide.tags.Keys)
                    {
                        if (!tagsToSlides.ContainsKey(tag))
                            tagsToSlides[tag] = new Dictionary<int, Slide>();
                        tagsToSlides[tag][slide.id] = slide;
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }
        }

        public class Slide
        {
            public int id;
            public bool hasHorizontal;
            public Dictionary<int, Photo> photos = new Dictionary<int, Photo>();
            public Dictionary<string, string> tags = new Dictionary<string, string>();
            public int lastSlideId = -1;
            public int lastScore = -1;
            public int tagCount = 0;
            public int weight = -1;

            public Slide(int id, bool horizontal)
            {
                this.id = id;
                this.hasHorizontal = horizontal;
            }

            public void AddPhoto(Photo photo)
            {
                try
                {
                    tagCount += photo.numTags;
                    photos[photo.id] = photo;
                    foreach (string tag in photo.tags)
                    {
                        if (!tags.ContainsKey(tag))
                        {
                            tags[tag] = tag;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }

            public int Weight(SlideShow slideShow)
            {
                if (weight < 0)
                {
                    weight = tags.Count;
                    foreach (String tag in tags.Values)
                    {
                        foreach (Slide s in slideShow.tagsToSlides[tag].Values)
                        {
                            weight += s.tags.Count;
                        }
                    }
                }

                return weight;
            }

            public int Score(Slide slide)
            {
                string key = string.Format("{0} {1}", Math.Min(this.id, slide.id), Math.Max(this.id, slide.id));

                lock (scoreLock)
                {
                    if (scores.ContainsKey(key))
                    {
                        return scores[key];
                    }
                    else
                    {
                        int score = Slide.Score(this, slide);
                        return score;
                    }
                }
            }

            public static int Score(Slide s1, Slide s2)
            {
                List<string> common = s1.tags.Values.Intersect<string>(s2.tags.Values).ToList<string>();
                int commonScore = common.Count;

                int score = commonScore;
                if (score > 0)
                {
                    score = Math.Min(s1.tags.Count - commonScore, commonScore);
                    score = Math.Min(score, s2.tags.Count - commonScore);
                }

                return score;
            }

        }

        private const int MIN_CACHED_SCORE = 1;
        private const int MAX_SCORE_FILES = 10;
        private const int MAX_CALC_SCORES = 1000000;
        private const int MAX_TASKS = 64; // number of available cpu cores
        private const int MAX_TAGS = 5000;
        private const int STOP_AFTER_SLIDE = 1000000;
        private static List<string> prevTags = new List<string>();

        private static SlideShow slideShow = new SlideShow();

        private static int numPhotos = 0;
        private static Dictionary<int, Photo> photos = new Dictionary<int, Photo>();

        private static object scoreLock = new object();
        private static Dictionary<string, int> scores = new Dictionary<string, int>();
        private static Dictionary<int, Dictionary<int, List<KeyValuePair<string, int>>>> hiscores = new Dictionary<int, Dictionary<int, List<KeyValuePair<string, int>>>>();

        private static SemaphoreSlim sem = new SemaphoreSlim(MAX_TASKS);
        private static List<Task> tasks = new List<Task>();

        private static int sortIndex = 0;
        private static int scoreSetIndex = 0;
        private static bool generateChacheOnly = false;
        private static int maxScore = 0;
        private static long maxScoreCnt = 0;

        static void Main(string[] args)
        {
            try
            {
                string challenge = "all";

                if (args.Length > 0)
                    int.TryParse(args[0], out sortIndex);
                if (args.Length > 1)
                    challenge = args[1];
                generateChacheOnly = args.Length > 2;

                switch (challenge)
                {
                    case "a":
                        Evaluate("a_example");
                        break;
                    case "b":
                        Evaluate("b_lovely_landscapes");
                        break;
                    case "c":
                        Evaluate("c_memorable_moments");
                        break;
                    case "d":
                        Evaluate("d_pet_pictures");
                        break;
                    case "e":
                        Evaluate("e_shiny_selfies");
                        break;
                    default:
                        Evaluate("a_example");
                        Evaluate("c_memorable_moments");
                        Evaluate("b_lovely_landscapes");
                        Evaluate("e_shiny_selfies");
                        Evaluate("d_pet_pictures");
                        break;
                }

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        private static void Evaluate(string resource)
        {
            Console.WriteLine();
            Console.WriteLine(string.Format("Evaluating {0}", resource));
            Task.WaitAll(tasks.ToArray());
            tasks.Clear();

            scoreSetIndex = 0;

            lock (scoreLock)
            {
                scores = new Dictionary<string, int>();
                hiscores = new Dictionary<int, Dictionary<int, List<KeyValuePair<string, int>>>>();
            }

            Load(resource);

            bool haveScores = File.Exists(string.Format("{0}.hiscores.0", resource));
            bool haveSlides = File.Exists(string.Format("{0}.slides", resource));

            if(!haveSlides)
            { 
                CreateHorizontalSlides(resource, haveScores);
                CreateVerticalSlides(resource, haveScores);
                PersistSlides(string.Format("{0}.slides", resource));
            }
            else
            {
                LoadSlides(string.Format("{0}.slides", resource));
            }
            Console.WriteLine();

            //if (!haveScores)
            //{
            //    CalculateAllHiScores(resource);
            //    Task.WaitAll(tasks.ToArray());
            //    tasks.Clear();
            //    lock (scoreLock)
            //    {
            //        PersistCalculatesHiScores(string.Format("{0}.hiscores.{1}", resource, scoreSetIndex++));
            //    }
            //}

            if (generateChacheOnly) return; // generate cache only

            Console.WriteLine();

            switch (sortIndex)
            {
                case 0:
                    for (int i = 0; i < MAX_SCORE_FILES; i++)
                    {
                        if (LoadCalculatesScores(string.Format("{0}.hiscores.{1}", resource, i)))
                        {
                            slideShow.SortByScores();
                        }
                    }
                    break;
                case 1:
                    //if (LoadCalculatesScores(string.Format("{0}.hiscores.{1}", resource, 0)))
                    //{
                        slideShow.SortEx();
                    //}
                    break;
                default:
                    if (LoadCalculatesScores(string.Format("{0}.hiscores.{1}", resource, 0)))
                    {
                        slideShow.Sort();
                    }
                    break;
            }

            Console.WriteLine();
            int score = Score(slideShow.sortedSlides);
            

            SaveResult(slideShow.sortedSlides, string.Format("{0}_{1}_{2}.out", resource, score, sortIndex));
            Console.WriteLine(string.Format("{0} score: {1}", resource, score));
        }

        private static void LoadSlides(string url)
        {
            try
            {
                string[] lines = File.ReadAllLines(url);

                for (int i = 0; i < lines.Length; i++)
                {
                    string[] spec = lines[i].Split(' ');

                    int id = int.Parse(spec[0]);
                    bool isHorizontal = spec[1].Equals("H");
                    if (isHorizontal)
                    {
                        int pid = int.Parse(spec[2]);
                        slideShow.CreateSlide(id, pid);
                    }
                    else
                    {
                        int pid1 = int.Parse(spec[2]);
                        int pid2 = int.Parse(spec[3]);
                        slideShow.CreateSlide(id, pid1, pid2);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }

        }

        private static void CalculateAllHiScores(string resource)
        {
            try
            {
                foreach(Slide slide in slideShow.slides.Values)
                {
                    if (scoreSetIndex > MAX_SCORE_FILES)
                        break;

                    Slide s1 = slide;
                    sem.Wait();
                    var t = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            CalculateHiScore(resource, s1);
                        }
                        finally
                        {
                            sem.Release();
                        }
                    });
                    tasks.Add(t);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private static void CreateHorizontalSlides(string resource, bool haveScores)
        {
            var horizontals = photos.Values.Where(p => p.horizontal).OrderByDescending(p => p.tags.Count).Select(p => p).ToList();

            while (horizontals.Count >= 1)
            {
                var p = horizontals.First();
                horizontals.Remove(p);
                Slide slide = slideShow.AddPhoto(p);
                Console.Write(string.Format("Preparing horizontal slides - remaining {0}\t\t\t\r", horizontals.Count));
            }
        }

        private static void CalculateScore(string resource, Slide s1)
        {
            try
            {
                lock (scoreLock)
                {
                    List<Slide> slides = slideShow.GetRelevantSlides(s1);

                    foreach (var s2 in slides)
                    {
                        if(s2.id != s1.id)
                        {
                            if (scores == null)
                                return;

                            string key = string.Format("{0} {1}", Math.Min(s2.id, s1.id), Math.Max(s2.id, s1.id));
                            int score = Slide.Score(s1, s2);

                            if (score < MIN_CACHED_SCORE) continue;

                            scores[key] = score;

                            Console.Write("slides {0} - calc. scores {1} \t\r", slideShow.index, scores.Count);

                            if (scores.Count >= MAX_CALC_SCORES)
                            {
                                PersistCalculatesHiScores(string.Format("{0}.hiscores.{1}", resource, scoreSetIndex++));
                                scores.Clear();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private static void CalculateHiScore(string resource, Slide s1)
        {
            try
            {
                    List<Slide> slides = slideShow.GetRelevantSlides(s1);
                    foreach (Slide s2 in slides)
                    {
                        if (s2.id != s1.id)
                        {
                            string key = string.Format("{0} {1}", Math.Min(s2.id, s1.id), Math.Max(s2.id, s1.id));

                            if (scores.ContainsKey(key))
                            {
                                continue;
                            }

                            lock (scoreLock)
                            {
                            int score = Slide.Score(s1, s2);
                            if (score < MIN_CACHED_SCORE)
                            {
                                continue;
                            }

                            if (score > maxScore)
                            {
                                maxScore = score;
                                maxScoreCnt = 1;
                            }
                            else if(score == maxScore)
                            {
                                maxScoreCnt++;
                            }


                            Console.Write("slides {0} - calc. scores {1} - max {2} - cnt {3} - sid {4} \t\r", slideShow.index, hiscores.Count, maxScore, maxScoreCnt, s1.id);

                            var dict = new Dictionary<int, List<KeyValuePair<string, int>>>();
                            dict[score] = new List<KeyValuePair<string, int>>();
                            var keyValuePair = new KeyValuePair<string, int>(key, score);

                            bool hasS1 = hiscores.ContainsKey(s1.id);
                            bool hasS2 = hiscores.ContainsKey(s2.id);
                            int scoreS1 = hasS1 ? hiscores[s1.id].Keys.First() : 0;
                            int scoreS2 = hasS2 ? hiscores[s2.id].Keys.First() : 0;

                            if (scoreS1 >= score &&
                                scoreS2 >= score)
                            {
                                continue;
                            }

                            scores[key] = score;

                            if (!hasS1 || scoreS1 < score)
                            {
                                dict[score].Add(keyValuePair);
                                hiscores[s1.id] = dict;
                            }
                            //else if (hasS1 && scoreS1 == score)
                            //{
                            //    hiscores[s1.id][score].Add(keyValuePair);
                            //}

                            if (!hasS2 || scoreS2 < score)
                            {
                                dict[score].Add(keyValuePair);
                                hiscores[s2.id] = dict;
                            }
                            //else if (hasS2 && scoreS2 == score)
                            //{
                            //    hiscores[s2.id][score].Add(keyValuePair);
                            //}

                            if (hiscores.Count >= MAX_CALC_SCORES)
                            {
                                PersistCalculatesHiScores(string.Format("{0}.hiscores.{1}", resource, scoreSetIndex++));
                                scores.Clear();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private static void CreateVerticalSlides(string resource, bool haveScores)
        {
            var verticals = photos.Values.Where(p => !p.horizontal).OrderByDescending(p => p.tags.Count).Select(p => p).ToList();

            while (verticals.Count >= 2)
            {
                var p1 = verticals.First();
                verticals.Remove(p1);

                var candidates = verticals.Select(p => new { photo = p, count = p.tags.Intersect(p1.tags).Count() })
                    .OrderBy(x => x.count)
                    .ThenBy(x => x.photo.Weight(slideShow))
                    .Take(1).ToList();

                //var candidates = verticals.Select(p => new { photo = p, count = p.tags.Intersect(p1.tags).Count() })
                //.OrderBy(x => x.count)
                //.Take(1).ToList();

                var p2 = candidates.First().photo; 
                verticals.Remove(p2);
                Slide slide = slideShow.AddPhotos(p1, p2);
                Console.Write(string.Format("Preparing vertical slides - remaining {0}\t\t\t\r", verticals.Count));
            }
        }

        private static bool LoadCalculatesScores(string url)
        {
            try
            {
                scores = new Dictionary<string, int>();
                hiscores = new Dictionary<int, Dictionary<int, List<KeyValuePair<string, int>>>>();

                if (!File.Exists(url))
                {
                    return false;   
                }
                var lines = File.ReadAllLines(url);
                foreach (var l in lines)
                {
                    string[] t = l.Split(' ');
                    scores[string.Format("{0} {1}", t[0], t[1])] = int.Parse(t[2]);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
            return false;
        }

        private static void PersistSlides(string url)
        {
            try
            {
                if (File.Exists(url)) // file already exists, will not be overwritten
                {
                    return;
                }

                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(url))
                {
                    foreach (var slide in slideShow.slides.Values)
                    {
                        if (slide.hasHorizontal)
                        {
                            file.WriteLine(string.Format("{0} {1} {2}", slide.id, 'H', slide.photos.Keys.First()));
                        }
                        else
                        {
                            file.WriteLine(string.Format("{0} {1} {2} {3}", slide.id, 'V', slide.photos.Keys.First(), slide.photos.Keys.Last()));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }


        private static void PersistCalculatesHiScores(string url)
        {
            try
            {
                if (File.Exists(url)) // file already exists, will not be overwritten
                {
                    return;
                }

                Dictionary<string, string> allocated = new Dictionary<string, string>();
                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(url))
                {
                    foreach (var hiscore in hiscores.Values)
                    {
                        foreach(var slidePair in hiscore.Values.First())
                        { 
                            if (!allocated.ContainsKey(slidePair.Key))
                            {
                                file.WriteLine(string.Format("{0} {1}", slidePair.Key, slidePair.Value));
                                allocated[slidePair.Key] = slidePair.Key;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private static void PersistCalculatesScores(string url)
        {
            try
            {
                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(url))
                {
                    foreach (var k in scores.Keys)
                    {
                        file.WriteLine(string.Format("{0} {1}", k, scores[k]));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        // this method is simply used to test if load and save work as specified
            private static void CreateExampleOutput()
        {
            try
            {
                slideShow.slides.Add(0,new Slide(0, true));
                slideShow.slides.Add(1, new Slide(1, true));
                slideShow.slides.Add(2, new Slide(2, false));

                slideShow.slides[0].photos[0] = photos[0];
                slideShow.slides[1].photos[3] = photos[3];
                slideShow.slides[2].photos[1] = photos[1];
                slideShow.slides[2].photos[2] = photos[2];
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private static void Load(string resource)
        {
            try
            {
                slideShow = new SlideShow();
                photos = new Dictionary<int, Photo>();
                var example = Properties.Resources.ResourceManager.GetObject(resource) as String;
                if (example == null)
                {
                    throw new NotSupportedException(string.Format("resource {0} is not included in assembly", resource));
                }

                string[] lines = example.Split('\n');

                // parse header containing number of entities
                string[] desc = lines[0].Split(' ');
                numPhotos = int.Parse(desc[0]);

                for(int i = 1; i < lines.Length; i++)
                {
                    if (lines[i].Length == 0)
                        continue;
                    string[] spec = lines[i].Split(' ');

                    bool isHorizontal = spec[0].Equals("H");
                    int numTags = int.Parse(spec[1]);

                    photos[i-1] = new Photo(i-1, isHorizontal, numTags);
                    for (int t = 0; t < numTags; t++)
                    {
                        photos[i-1].tags.Add(spec[2 + t]);
                    }
                }

                slideShow.GenerateTagsToPhotos(photos.Values.ToList());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
                
        private static int Score(List<Slide> s)
        {
            int score = 0;

            for(int i=0; i < s.Count - 1 ; i++)
            {
                List<string> ctags = s[i].tags.Keys.ToList<string>();
                List<string> ntags = s[i + 1].tags.Keys.ToList<string>();

                List<string> common = ctags.Intersect<string>(ntags).ToList<string>();
                int subScore = Math.Min(ctags.Count - common.Count, common.Count);
                subScore = Math.Min(subScore, ntags.Count - common.Count);
                score += subScore;
            }

            return score;
        }

        private static void SaveResult(List<Slide>slides, string urlResult)
        {
            try
            {
                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(urlResult))
                {
                    file.WriteLine(string.Format("{0}", slides.Count));
                    foreach (var slide in slides)
                    {
                        var photos = from photo in slide.photos.Values orderby photo.id select photo.id;
                        string record = string.Join(' ', photos.ToArray<int>());
                        file.WriteLine(record);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }

        }

    }
}
