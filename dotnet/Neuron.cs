using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Drawing;
using Newtonsoft.Json;

/// <summary>
/// Copyright (c) 2019 Buzz-Barry Struck
/// 
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all
/// copies or substantial portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
/// SOFTWARE.
/// </summary>

namespace BuzzNet
{
    /// <summary>
    /// Summary description for Neuron.
    /// </summary>
    public class Neuron : IShutdown
    {
        #region consts

        private const int NUM_OF_SYNAPSES = 0;
        private const double TOLERANCE = 0.00001;
        private const int RADIUS = 5;

        #endregion consts

        #region members

        private ArrayList synapses = null;
        private ArrayList axons = null;

        private Thread SynapsesProcessorThread = null;
        private bool terminate = false;

        private Point pos;
        private int radius = RADIUS;

        private bool outSignal = false;
        private double threashhold = 1.0;

        private Pen penActive = new Pen(Color.Black);
        private Pen penInactive = new Pen(Color.DarkGray);
        private Brush brushSignal = new SolidBrush(Color.Green) as Brush;
        private Brush brushNoSignal = new SolidBrush(Color.Red) as Brush;
        private Brush brushInactive = new SolidBrush(Color.LightGray) as Brush;

        private Random rand = null;
        private int layer = -1;
        private int index = -1;
        private AutoResetEvent dataEvent = null;
        private bool disable = false;
        private bool isActive = false;
        private double score = 0.0;

        #endregion members

        #region properties

        public double Score
        {
            get { return score; }
            set
            {
                score = value;
                foreach (Synapse s in synapses)
                {
                    s.Score = score;
                    if (s.Sender != null) s.Sender.Score = score;
                }
            }
        }
        
        public int Layer
        {
            get { return layer; }
            set { layer = value; }
        }

        public int Index
        {
            get { return index;}
            set {index = value;}
        }

        public bool Disable
        {
            get { return disable; }
            set { disable = value; }
        }

        public double Threashhold
        {
            get { return threashhold; }
            set { threashhold = value; }
        }

        public ArrayList Synapses
        {
            get { return synapses; }
            set { synapses = value; }
        }

        [JsonIgnore]
        public ArrayList Axons
        {
            get { return axons; }
            set { axons = value; }
        }

        [JsonIgnore]
        public int SynapsesCount
        {
            get { return synapses.Count; }
        }

        [JsonIgnore]
        public int AxonsCount
        {
            get { return axons.Count; }
        }
        
        [JsonIgnore]
        public Point Position
        {
            get { return pos; }
            set { pos = value; }
        }

        [JsonIgnore]
        public int Radius
        {
            get { return radius; }
            set { radius = value; }
        }
        
        [JsonIgnore]
        public bool OutSignal
        {
            get
            {
                while (!disable && !dataEvent.WaitOne(200, true)) ;
                if (disable)
                {
                    return false;
                }

                return outSignal;
            }
        }

        [JsonIgnore]
        public ESynapsisType AxonType
        {
            set
            {
                foreach (Synapse s in axons)
                {
                    s.Type = value;
                }
            }
        }
        
        [JsonIgnore]
        public ESynapsisType SynapsesType
        {
            set
            {
                foreach (Synapse s in synapses)
                {
                    s.Type = value;
                }
            }
        }

        #endregion properties

        #region constructor

        public Neuron(int layer, int index, Point position, int seed)
        {
            rand = new Random(seed);
            this.layer = layer;
            this.index = index;
            pos = position;

            axons = new ArrayList();
            synapses = new ArrayList();

            terminate = false;
            dataEvent = new AutoResetEvent(false);
        }

        #endregion constructor

        #region destructor

        ~Neuron()
        {
            terminate = true;
            ShutdownSynapses();
        }

        #endregion destructor

        #region public methods

        public void UseOptimums()
        {
            foreach (Synapse s in synapses)
            {
                s.UseOptimum();
                if (s.Sender != null)
                {
                    s.Sender.UseOptimums();
                }
            }
        }

        public void Start()
        {
            if (isActive) return;
            isActive = true;

            SynapsesProcessorThread = new Thread(new ThreadStart(this.SynapsesProcessor));
            SynapsesProcessorThread.Start();
        }

        public void Reset()
        {
            try
            {
                foreach (Synapse synapse in synapses)
                {
                    synapse.Reset();
                }
                foreach (Synapse axon in axons)
                {
                    axon.Reset();
                }
            }
            catch (Exception) { }
        }

        public void Wipeout()
        {
            synapses.Clear();
            axons.Clear();
            ShutdownSynapses();
        }


        public void Draw(Graphics g)
        {
            Pen pen = (SynapsesCount == 0 ? penInactive : penActive);
            Brush brush = (SynapsesCount == 0 ? brushInactive : (outSignal ? brushSignal : brushNoSignal));

            g.DrawEllipse(pen, pos.X - (radius), pos.Y - (radius), radius * 2, radius * 2);
            g.FillEllipse(brush, pos.X - (radius), pos.Y - (radius), radius * 2, radius * 2);

            if (layer == 0) return; 

            try
            {
                Synapse[] tmp = null;
                tmp = new Synapse[synapses.Count];
                synapses.CopyTo(tmp);

                foreach (Synapse synapse in tmp)
                {
                    synapse.Draw(g);
                }
            }
            catch (Exception) { }
        }

        public Synapse CreateSynapseConnection(ESynapsisType type, double weight)
        {
            Synapse synapse = null;
            synapse = new Synapse(null, this, rand.Next(), type, weight);
            synapse.LayerReceiver = layer;
            synapse.IndexReceiver = index;
            synapses.Add(synapse);

            return synapse;
        }

        public void RemoveSynapseConnection(Synapse synapse)
        {
            try
            {
                synapses.Remove(synapse);
                synapse.Receiver = null;
                if (synapse.Sender != null)
                {
                    synapse.Sender.RemoveAxonConnection(synapse);
                }

                synapse.Shutdown();
            }
            catch (System.Exception ex)
            {
                Debug.Assert(false, ex.ToString());
            }
            synapse = null;
        }

        public void AddAxonConnection(Synapse synapse)
        {
            synapse.Sender = this;
            axons.Add(synapse);
        }

        public void RemoveAxonConnection(Synapse synapse)
        {
            if (synapse == null)
                return;

            try
            {
                axons.Remove(synapse);
                synapse.Sender = null;
                if (synapse.Receiver != null)
                {
                    synapse.Receiver.RemoveSynapseConnection(synapse);
                }
            }
            catch (System.Exception ex)
            {
                Debug.Assert(false, ex.ToString());
            }
            synapse.Shutdown();

            System.GC.Collect();
        }

        public bool IsConnected(Neuron neuron)
        {
            foreach (Synapse synapse in synapses)
            {
                if (synapse.Sender.Equals(neuron))
                {
                    return true;
                }
            }

            return false;
        }

        public void Rethink()
        {
            threashhold = rand.NextDouble();

            foreach (Synapse axon in axons)
            {
                axon.Rethink();
            }
        }


        public void Shrink()
        {
            if (axons.Count > 0)
            {
                int pos = rand.Next(0, axons.Count);
                RemoveAxonConnection((Synapse)axons[pos]);
            }
        }


        public bool Cleanup()
        {
            ArrayList trash = new ArrayList();
            bool res = false;

            foreach (Synapse axon in axons)
            {
                if ((synapses.Count == 0) || (axon.Receiver == null))
                {
                    trash.Add(axon);
                }
            }

            foreach (Synapse axon in trash)
            {
                RemoveAxonConnection(axon);
                res = true;
            }

            return res;
        }

        public bool Peek(List<int> inputMatrix)
        {
            bool res = false;
            double totWeight = 0.0;

            foreach (Synapse s in synapses)
            {
                if (s.Disable || s.Weight < TOLERANCE) continue;

                bool signal = s.Sender != null ? s.Sender.Peek(inputMatrix) : inputMatrix[s.IndexSender] == 1 ? true : false;

                if (signal)
                {
                    switch (s.Type)
                    {
                        case ESynapsisType.Activator:
                            totWeight += s.Weight;
                            break;
                        case ESynapsisType.Inhibitor:
                            totWeight -= s.Weight;
                            break;
                        default:
                            throw new System.NotImplementedException("type unknown");
                    }
                }
            }

            res = (totWeight - threashhold > TOLERANCE);

            return res;
        }

        #endregion public methods

        #region protected methods

        protected void SynapsesProcessor()
        {
            while (!terminate)
            {
                double totWeight = 0.0;

                foreach (Synapse synapse in synapses)
                {
                    if (terminate)
                        return;

                    // see if synapse has any bearing
                    if (synapse.Disable) continue;

                    bool signal = synapse.Signal;
                    if (signal)
                    {
                        switch (synapse.Type)
                        {
                            case ESynapsisType.Activator:
                                totWeight += synapse.Weight;
                                break;
                            case ESynapsisType.Inhibitor:
                                totWeight -= synapse.Weight;
                                break;
                            default:
                                throw new System.NotImplementedException("type unknown");
                        }
                    }
                }
                
                if (terminate)
                    return;

                outSignal = (totWeight - threashhold > TOLERANCE);

                foreach (Synapse axon in axons)
                {
                    // see if axon has any bearing
                    if (axon.Disable) continue;

                    axon.Signal = outSignal;
                }

                dataEvent.Set();
            }
        }

        protected void ShutdownSynapses()
        {
            foreach (Synapse syn in synapses)
            {
                syn.Shutdown();
            }
            foreach (Synapse syn in axons)
            {
                syn.Shutdown();
            }
        }

        protected void DisableSynapses(bool disable)
        {
            foreach (Synapse syn in synapses)
            {
                syn.Disable = disable;
            }
            foreach (Synapse syn in axons)
            {
                syn.Disable = disable;
            }
        }

        #endregion protected methods

        #region private methods
        #endregion private methods

        #region IDisposable Members

        public void Shutdown()
        {
            DisableSynapses(true);
            terminate = true;
            ShutdownSynapses();
        }

        #endregion
    }
}
